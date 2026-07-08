#version 330 core
in vec2 UV;
in vec3 WorldPosition;

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
uniform float u_TotalArcLength;
uniform float u_RadialWidth;
uniform float u_RotationAngle;

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

//伪彩模式：只裁剪，窗外返回-1，跳过
float applyWindowClip(float value, float windowCenter, float windowWidth)
{
    if (windowWidth < EPSILON)
    {   
        return 0.0;
    }
    
    float windowMin = windowCenter - windowWidth * 0.5;
    float windowMax = windowCenter + windowWidth * 0.5;

    //窗外返回-1.0（特殊标记，表示跳过）
    if (value <= windowMin || value >= windowMax)
    {
        return -1.0;
    }
    
    //窗内返回原始值
    return value;
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
    //UV.x -> 弧长
    float normalizedArcLength = UV.x;
    
    //UV.y -> 径向偏移
    float radialOffset = (UV.y - 0.5) * u_RadialWidth;
    
    //采样FrenetFrame
    vec3 position, tangent, normal, binormal;
    sampleFrenetFrame(normalizedArcLength, position, tangent, normal, binormal);
    
    //绕Tangent旋转Normal
    vec3 rotatedNormal = rotateAroundAxis(normal, tangent, u_RotationAngle);
    
    //计算采样位置（世界空间）
    vec3 samplePosition = position + rotatedNormal * radialOffset;
    
    //世界空间 -> 纹理坐标
    vec3 texCoord = (samplePosition / u_VolumeScale) + 0.5;
    
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
    
    //获取原始医学值
    float medicalValue = getMedicalValue(texCoord);
    
    //基础颜色
    vec3 color;
    float alpha = 1.0;
    
    //Gray - 灰度模式
    if (u_RenderMode == 0)
    {
        //应用窗宽窗位（裁剪 + 线性映射）
        float grayValue = applyWindowLevel(medicalValue, u_WindowCenter, u_WindowWidth);

        //应用亮度和对比度
        grayValue = (grayValue - 0.5) * u_Contrast + 0.5;
        grayValue *= u_Brightness;
        grayValue = clamp(grayValue, 0.0, 1.0);

        color = vec3(grayValue);
    }
    //PseudoColor - 伪彩模式
    else
    {
        //应用窗宽窗位（只裁剪，窗外直接跳过）
        //float clippedValue = applyWindowClip(medicalValue, u_WindowCenter, u_WindowWidth);
        
        //窗外值跳过
        //if (clippedValue < 0.0)
        //{
            //discard;
        //}

        //将HU值映射到传递函数的归一化位置
        float normalizedPosition = (medicalValue - u_HUMin) / (u_HUMax - u_HUMin);
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
