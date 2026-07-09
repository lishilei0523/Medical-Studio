#version 330 core
in vec3 WorldPosition;
in vec2 UV;

out vec4 FragColor;

uniform sampler3D u_PreviewTexture;
uniform usampler3D u_MarkTexture;
uniform sampler1D u_TransferFunction;
uniform sampler1D u_MarkStrategy;

//FrenetFrame纹理
uniform sampler1D u_PositionTexture;
uniform sampler1D u_TangentTexture;
uniform sampler1D u_NormalTexture;
uniform sampler1D u_BinormalTexture;

//曲线参数
uniform float u_RadialWidth;
uniform float u_RotationAngle;
uniform float u_ProjectionThickness;
uniform int u_MaxStepsCount;
uniform int u_ProjectionMode;
uniform int u_ProjectionDirection;      //CPR投影方向：0=Tangent, 1=Normal

//渲染参数
uniform vec3 u_VolumeScale;
uniform float u_WindowWidth;
uniform float u_WindowCenter;
uniform float u_Brightness;             //亮度
uniform float u_Contrast;               //对比度
uniform float u_HUMin;
uniform float u_HUMax;

//渲染模式：0=Gray, 1=PseudoColor
uniform int u_RenderMode;

//标记策略：每个标记值的行为（0=Visible, 1=Collapsed, 2=Tinted）
uniform int u_MarkModes[256];

//常量
const float MAX_16BIT_SIGNED = 32767.0;
const float EPSILON = 0.0001;
const int PROJECTION_AIP = 0;
const int PROJECTION_MIP = 1;
const int PROJECTION_MINIP = 2;
const int PROJECTION_DIRECTION_TANGENT = 0;
const int PROJECTION_DIRECTION_NORMAL = 1;


//从弧长采样FrenetFrame，t: 归一化弧长 0~1
void sampleFrenetFrame(float t, out vec3 position, out vec3 tangent, out vec3 normal, out vec3 binormal)
{
    vec4 posSample = texture(u_PositionTexture, t);
    vec4 tanSample = texture(u_TangentTexture, t);
    vec4 norSample = texture(u_NormalTexture, t);
    vec4 binSample = texture(u_BinormalTexture, t);
    
    position = posSample.xyz;
    tangent = tanSample.xyz;
    normal = norSample.xyz;
    binormal = binSample.xyz;
}

//绕轴旋转向量
vec3 rotateAroundAxis(vec3 direction, vec3 axis, float angle)
{
    float cosA = cos(angle);
    float sinA = sin(angle);
    vec3 rotatedDirection = direction * cosA + cross(axis, direction) * sinA + axis * dot(axis, direction) * (1.0 - cosA);

    return rotatedDirection;
}

//灰度模式：窗宽窗位裁剪 + 线性映射
float applyWindowLevel(float value, float windowCenter, float windowWidth)
{
    if (windowWidth < EPSILON)
    {
        return 0.0;
    }
    
    float windowMin = windowCenter - windowWidth * 0.5;
    float windowMax = windowCenter + windowWidth * 0.5;

    //窗内线性映射
    float result = (value - windowMin) / windowWidth;
    result = clamp(result, 0.0, 1.0);

    return result;
}

//获取体素的医学值（HU值）
float getMedicalValue(vec3 texCoord)
{
    //边界检查
    if (texCoord.x < 0.0 || texCoord.x > 1.0 ||
        texCoord.y < 0.0 || texCoord.y > 1.0 ||
        texCoord.z < 0.0 || texCoord.z > 1.0)
    {
        return -1000.0;  //空气的CT值
    }
    
    float snormValue = texture(u_PreviewTexture, texCoord).r;
    float medicalValue = snormValue * MAX_16BIT_SIGNED;

    return medicalValue;
}

void main()
{
    //UV.x -> 归一化弧长
    float normalizedArcLength = UV.x;
    
    //UV.y -> 径向偏移
    float radialOffset = (UV.y - 0.5) * u_RadialWidth;
    
    //采样FrenetFrame
    vec3 position, tangent, normal, binormal;
    sampleFrenetFrame(normalizedArcLength, position, tangent, normal, binormal);
    
    //绕Tangent旋转Normal
    vec3 rotatedNormal = rotateAroundAxis(normal, tangent, u_RotationAngle);
    
    //射线起点和方向
    vec3 rayOrigin = position + rotatedNormal * radialOffset;
    vec3 rayDirection = (u_ProjectionDirection == PROJECTION_DIRECTION_TANGENT) ? tangent : rotatedNormal;
    
    //沿射线方向步进采样
    float halfThickness = u_ProjectionThickness * 0.5;
    float stepSize = u_ProjectionThickness / float(u_MaxStepsCount);
    
    float projectedHU;    
    if (u_ProjectionMode == PROJECTION_MIP)
    {
        projectedHU = -1000.0;
        for (int index = 0; index <= u_MaxStepsCount; index++)
        {
            float offset = -halfThickness + stepSize * float(index);
            vec3 samplePosition = rayOrigin + rayDirection * offset;
            vec3 localTexCoord = (samplePosition / u_VolumeScale) + 0.5;
            float hu = getMedicalValue(localTexCoord);
            projectedHU = max(projectedHU, hu);
        }
    }
    else if (u_ProjectionMode == PROJECTION_MINIP)
    {
        projectedHU = 3071.0;
        for (int index = 0; index <= u_MaxStepsCount; index++)
        {
            float offset = -halfThickness + stepSize * float(index);
            vec3 samplePosition = rayOrigin + rayDirection * offset;
            vec3 localTexCoord = (samplePosition / u_VolumeScale) + 0.5;
            float hu = getMedicalValue(localTexCoord);
            projectedHU = min(projectedHU, hu);
        }
    }
    else //PROJECTION_AIP
    {
        float sumHU = 0.0;
        int validCount = 0;
        for (int index = 0; index <= u_MaxStepsCount; index++)
        {
            float offset = -halfThickness + stepSize * float(index);
            vec3 samplePosition = rayOrigin + rayDirection * offset;
            vec3 localTexCoord = (samplePosition / u_VolumeScale) + 0.5;
            float hu = getMedicalValue(localTexCoord);
            if (hu > -1000.0)
            {
                sumHU += hu;
                validCount++;
            }
        }
        projectedHU = validCount > 0 ? sumHU / float(validCount) : -1000.0;
    }
    
    //取射线起点位置的纹理坐标用于标记采样
    vec3 texCoord = (rayOrigin / u_VolumeScale) + 0.5;
    
    //边界检查
    if (texCoord.x < 0.0 || texCoord.x > 1.0 ||
        texCoord.y < 0.0 || texCoord.y > 1.0 ||
        texCoord.z < 0.0 || texCoord.z > 1.0)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 0.0);
        return;
    }
    
    //采样标记纹理
    uint markValue = texture(u_MarkTexture, texCoord).r;

    //根据标记模式决定是否渲染
    int markMode = u_MarkModes[markValue];
    if (markMode == 1)
    {
        discard;
    }
    
    //基础颜色
    vec3 color;
    float alpha = 1.0;
    
    //Gray - 灰度模式
    if (u_RenderMode == 0)
    {
        //应用窗宽窗位（裁剪 + 线性映射）
        float grayValue = applyWindowLevel(projectedHU, u_WindowCenter, u_WindowWidth);

        //应用亮度和对比度
        grayValue = (grayValue - 0.5) * u_Contrast + 0.5;
        grayValue *= u_Brightness;
        grayValue = clamp(grayValue, 0.0, 1.0);

        color = vec3(grayValue);
    }
    //PseudoColor - 伪彩模式
    else
    {
        //将HU值映射到传递函数的归一化位置
        float normalizedPosition = (projectedHU - u_HUMin) / (u_HUMax - u_HUMin);
        normalizedPosition = clamp(normalizedPosition, 0.0, 1.0);

        //采样传递函数获取伪彩色
        vec4 pseudoColor = texture(u_TransferFunction, normalizedPosition);

        //应用亮度和对比度
        pseudoColor.rgb = (pseudoColor.rgb - 0.5) * u_Contrast + 0.5;
        pseudoColor.rgb *= u_Brightness;
        pseudoColor.rgb = clamp(pseudoColor.rgb, 0.0, 1.0);

        color = pseudoColor.rgb;
        alpha = pseudoColor.a;
    }
    
    //染色模式下，标记区域混合染色颜色
    if (markMode == 2 && markValue != 0u)
    {
        //从标记颜色纹理采样（纹理坐标 = 标记值 / 255）
        float markTexCoord = (float(markValue) + 0.5) / 256.0;
        vec4 markColor = texture(u_MarkStrategy, markTexCoord);

        //颜色叠加：用标记颜色的Alpha作为混合系数
        color = mix(color, markColor.rgb, markColor.a);
    }
    
    FragColor = vec4(color, alpha);
}
