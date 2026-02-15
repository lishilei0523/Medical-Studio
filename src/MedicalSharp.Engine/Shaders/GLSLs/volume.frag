#version 330 core
in vec3 WorldPosition;
in vec3 LocalPosition;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform sampler1D u_TransferFunction;

uniform vec3 u_CameraPosition;
uniform vec3 u_VolumeScale;
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;
uniform float u_WindowCenter;
uniform float u_WindowWidth;
uniform float u_StepSize;             //步长
uniform float u_Brightness;           //亮度
uniform float u_DensityScale;         //密度缩放
uniform int u_MaxStepsCount;          //最大步数
uniform float u_OpacityThreshold;     //透明度阈值

//线性窗宽窗位转换
float applyWindowLevel(float voxelValue, float windowCenter, float windowWidth)
{
    float windowMin = windowCenter - windowWidth * 0.5;
    float windowMax = windowCenter + windowWidth * 0.5;    

    //窗外应该返回-1.0（完全透明）
    if (voxelValue <= windowMin || voxelValue >= windowMax)
    {
        return -1.0;  //特殊标记，表示跳过
    }
    
    //窗内：线性映射到[0,1]
    return (voxelValue - windowMin) / windowWidth;
}

//计算与立方体的交点
bool rayBoxIntersect(vec3 rayOrigin, vec3 rayDirection, vec3 boxMin, vec3 boxMax, out float nearDistance, out float farDistance)
{
    //关键优化：预先计算倒数方向
    vec3 invRayDirection = 1.0 / rayDirection;

    //计算与每个轴对齐平面的交点
    vec3 t1 = (boxMin - rayOrigin) * invRayDirection;
    vec3 t2 = (boxMax - rayOrigin) * invRayDirection;
    
    //对每个轴，找到近点和远点
    vec3 tMinVec = min(t1, t2);
    vec3 tMaxVec = max(t1, t2);
    
    //找到所有轴中最大的tMin（进入点）
    nearDistance = max(max(tMinVec.x, tMinVec.y), tMinVec.z);

    //找到所有轴中最小的tMax（离开点）
    farDistance = min(min(tMaxVec.x, tMaxVec.y), tMaxVec.z);
    
    //如果进入点 > 离开点，射线没有穿过盒子
    //如果离开点 < 0，盒子在射线后面
    return farDistance > max(nearDistance, 0.0);
}

void main()
{
    //计算视线方向
    vec3 rayOrigin = u_CameraPosition;
    vec3 rayDirection = normalize(WorldPosition - rayOrigin);
    
    //定义体积边界（单位立方体 [-0.5, 0.5]）
    vec3 boxMin = vec3(-0.5, -0.5, -0.5) * u_VolumeScale;
    vec3 boxMax = vec3(0.5, 0.5, 0.5) * u_VolumeScale;
    
    //计算与体积边界的交点
    float nearDistance, farDistance;
    if (!rayBoxIntersect(rayOrigin, rayDirection, boxMin, boxMax, nearDistance, farDistance)) 
    {
        discard;
    }
    
    //确保从近处开始
    nearDistance = max(nearDistance, 0.0);
    
    //光线步进
    vec3 rayStart = rayOrigin + rayDirection * nearDistance;
    vec3 rayEnd = rayOrigin + rayDirection * farDistance;
    float rayLength = distance(rayStart, rayEnd);
    
    int numSteps = int(rayLength / u_StepSize);
    float stepSize = rayLength / float(numSteps);
    
    vec3 step = rayDirection * stepSize;
    vec3 currentPos = rayStart;
    
    vec4 accumulatedColor = vec4(0.0);
    
    for (int index = 0; index < numSteps && index < u_MaxStepsCount; index++) 
    {
        //将位置转换到纹理坐标[0, 1]
        vec3 texCoord = (currentPos - boxMin) / (boxMax - boxMin);;
        
        //采样体积纹理
        float originalValue = texture(u_VolumeTexture, texCoord).r;
        float voxelValue = originalValue * 32767.0 * u_RescaleSlope + u_RescaleIntercept; 
        float density = applyWindowLevel(voxelValue, u_WindowCenter, u_WindowWidth);

        //如果密度为负（窗外），跳过这个采样点
        if (density < 0.0)
        {
            currentPos += step;
            continue;
        }

        //应用密度缩放
        density = clamp(density * u_DensityScale, 0.0, 1.0);
        
        //采样传输函数
        vec4 sampleColor = texture(u_TransferFunction, density);

        //如果透明度很低，跳过
        if (sampleColor.a < 0.01)
        {
            currentPos += step;
            continue;
        }

        //亮度调整
        sampleColor.rgb *= u_Brightness;
        
        //前向Alpha合成
        accumulatedColor.rgb += (1.0 - accumulatedColor.a) * sampleColor.a * sampleColor.rgb;
        accumulatedColor.a += (1.0 - accumulatedColor.a) * sampleColor.a;
        
        //提前终止
        if (accumulatedColor.a > u_OpacityThreshold) 
        {
            accumulatedColor.a = 1.0;
            break;
        }
        
        //步进到下一个采样点
        currentPos += step;
    }
    
    //应用Gamma校正
    FragColor.rgb = pow(accumulatedColor.rgb, vec3(1.0/2.2));
    FragColor.a = accumulatedColor.a;
}
