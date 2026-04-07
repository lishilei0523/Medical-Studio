#version 330 core
in vec3 WorldPosition;
in vec3 LocalPosition;

out vec4 FragColor;

//体积纹理
uniform sampler3D u_VolumeTexture;
uniform sampler1D u_TransferFunction;

//变换矩阵
uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

//射线参数（由CPU传入）
uniform vec3 u_RayOrigin;
uniform vec3 u_RayDirection;

//相机参数
uniform vec3 u_CameraPosition;
uniform vec3 u_VolumeScale;

//DICOM重缩放参数
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;

//窗宽窗位参数
uniform float u_WindowCenter;
uniform float u_WindowWidth;

//材质参数
uniform float u_Brightness;
uniform float u_DensityScale;

//采样参数
uniform float u_StepSize;
uniform int u_MaxStepsCount;
uniform float u_OpacityThreshold;

// 常量
const float MAX_16BIT_SIGNED = 32767.0;
const float EPSILON = 0.0001;

//线性窗宽窗位转换
float applyWindowLevel(float voxelValue, float windowCenter, float windowWidth)
{
    float windowMin = windowCenter - windowWidth * 0.5;
    float windowMax = windowCenter + windowWidth * 0.5;
    
    //窗外返回-1.0（完全透明）
    if (voxelValue <= windowMin || voxelValue >= windowMax)
    {
        return -1.0;
    }
    
    //窗内：线性映射到[0,1]
    return (voxelValue - windowMin) / windowWidth;
}

//计算与立方体的交点
bool rayBoxIntersect(vec3 rayOrigin, vec3 rayDirection, vec3 boxMin, vec3 boxMax, out float nearDistance, out float farDistance)
{
    vec3 invRayDirection = 1.0 / rayDirection;
    
    vec3 t1 = (boxMin - rayOrigin) * invRayDirection;
    vec3 t2 = (boxMax - rayOrigin) * invRayDirection;
    
    vec3 tMinVec = min(t1, t2);
    vec3 tMaxVec = max(t1, t2);
    
    nearDistance = max(max(tMinVec.x, tMinVec.y), tMinVec.z);
    farDistance = min(min(tMaxVec.x, tMaxVec.y), tMaxVec.z);
    
    return farDistance > max(nearDistance, 0.0);
}

//获取体素的医学值（HU值）
float getMedicalValue(vec3 texCoord)
{
    // 边界检查
    if (texCoord.x < 0.0 || texCoord.x > 1.0 ||
        texCoord.y < 0.0 || texCoord.y > 1.0 ||
        texCoord.z < 0.0 || texCoord.z > 1.0)
    {
        return -1000.0;  // 空气的CT值
    }
    
    float snormValue = texture(u_VolumeTexture, texCoord).r;
    float rawValue = snormValue * MAX_16BIT_SIGNED;
    float medicalValue = rawValue * u_RescaleSlope + u_RescaleIntercept;
    
    return medicalValue;
}

void main()
{
    //使用CPU传入的射线
    vec3 rayOrigin = u_RayOrigin;
    vec3 rayDirection = normalize(u_RayDirection);
    
    //定义体积边界（与渲染Shader完全一致）
    vec3 boxMin = vec3(-0.5, -0.5, -0.5) * u_VolumeScale;
    vec3 boxMax = vec3(0.5, 0.5, 0.5) * u_VolumeScale;
    
    //计算与体积边界的交点
    float nearDistance, farDistance;
    if (!rayBoxIntersect(rayOrigin, rayDirection, boxMin, boxMax, nearDistance, farDistance))
    {
        FragColor = vec4(0.0);
        return;
    }
    
    //确保从近处开始
    nearDistance = max(nearDistance, 0.0);
    
    //光线步进（与渲染Shader完全一致）
    vec3 rayStart = rayOrigin + rayDirection * nearDistance;
    vec3 rayEnd = rayOrigin + rayDirection * farDistance;
    float rayLength = distance(rayStart, rayEnd);
    
    int numSteps = int(rayLength / u_StepSize);
    float stepSize = rayLength / float(numSteps);
    
    vec3 step = rayDirection * stepSize;
    vec3 currentPos = rayStart;
    
    //Alpha累积（与渲染Shader完全一致）
    vec4 accumulatedColor = vec4(0.0);
    
    for (int index = 0; index < numSteps && index < u_MaxStepsCount; index++)
    {
        //转换到纹理坐标（与渲染Shader完全一致）
        vec3 texCoord = (currentPos - boxMin) / (boxMax - boxMin);
        
        //获取原始医学值
        float medicalValue = getMedicalValue(texCoord);
        
        //应用窗宽窗位
        float density = applyWindowLevel(medicalValue, u_WindowCenter, u_WindowWidth);
        
        // 窗外值，跳过
        if (density < 0.0)
        {
            currentPos += step;
            continue;
        }
        
        //应用密度缩放
        density = clamp(density * u_DensityScale, 0.0, 1.0);
        
        //采样传输函数
        vec4 sampleColor = texture(u_TransferFunction, density);
        
        //透明度很低，跳过
        if (sampleColor.a < 0.01)
        {
            currentPos += step;
            continue;
        }
        
        //应用亮度（与渲染一致）
        sampleColor.rgb *= u_Brightness;
        
        //前向Alpha合成（与渲染完全一致）
        accumulatedColor.rgb += (1.0 - accumulatedColor.a) * sampleColor.a * sampleColor.rgb;
        accumulatedColor.a += (1.0 - accumulatedColor.a) * sampleColor.a;
        
        //检查是否达到不透明度阈值
        //达到阈值意味着用户能在屏幕上看到这个体素
        if (accumulatedColor.a > u_OpacityThreshold)
        {
            //找到第一个用户能看到的纹理坐标
            FragColor = vec4(texCoord, 1);
            return;
        }
        
        //步进
        currentPos += step;
    }
    
    //未找到任何用户可见的体素
    FragColor = vec4(0.0);
}
