#version 330 core
in vec3 WorldPosition;
in vec3 LocalPosition;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform usampler3D u_MarkTexture; 
uniform sampler1D u_TransferFunction;
uniform sampler1D u_MarkStrategy;

uniform vec3 u_CameraPosition;
uniform vec3 u_VolumeScale;
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;
uniform float u_WindowWidth;
uniform float u_WindowCenter;
uniform float u_StepSize;               //步长
uniform float u_Brightness;             //亮度
uniform float u_DensityScale;           //密度缩放
uniform int u_MaxStepsCount;            //最大步数
uniform float u_OpacityThreshold;       //透明度阈值

//渲染模式：0=Raycast, 1=AIP, 2=MIP, 3=MinIP, 4=SSD
uniform int u_RenderMode;

//标记策略：每个标记值的行为（0=Visible, 1=Collapsed, 2=Tinted）
uniform int u_MarkModes[256];


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
    
    //各渲染模式需要的累积变量
    vec4 accumulatedColor = vec4(0.0);  //Raycast
    float accumulatedSum = 0.0;         //AIP
    int sampleCount = 0;                //AIP
    float accumulatedMax = -1e20;       //MIP
    float accumulatedMin = 1e20;        //MinIP
    int minSampleCount = 0;             //MinIP
    
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

        //采样标记纹理
        uint markValue = texture(u_MarkTexture, texCoord).r;        
        int markMode = u_MarkModes[markValue];        
        if (markMode == 1) //Collapsed - 隐藏
        {
            currentPos += step;
            continue;
        }

        //应用密度缩放
        density = clamp(density * u_DensityScale, 0.0, 1.0);
        
        //采样传递函数
        vec4 sampleColor = texture(u_TransferFunction, density);

        //如果透明度很低，跳过
        if (sampleColor.a < 0.01)
        {
            currentPos += step;
            continue;
        }

        //Mark值染色处理
        if (markMode == 2 && markValue != 0u) //Tinted - 染色
        {
            //从标记颜色纹理采样（纹理坐标 = 标记值 / 255）
            float markTexCoord = float(markValue) / 255.0;
            vec4 markColor = texture(u_MarkStrategy, markTexCoord);
            
            //颜色叠加：用标记颜色的Alpha作为混合系数
            sampleColor.rgb = mix(sampleColor.rgb, markColor.rgb, markColor.a);
        }

        //亮度调整
        sampleColor.rgb *= u_Brightness;
        
        //根据渲染模式累积
        if (u_RenderMode == 0)  //Raycast
        {
            //前向Alpha合成
            accumulatedColor.rgb += (1.0 - accumulatedColor.a) * sampleColor.a * sampleColor.rgb;
            accumulatedColor.a += (1.0 - accumulatedColor.a) * sampleColor.a;
        
            //提前终止
            if (accumulatedColor.a > u_OpacityThreshold) 
            {
                accumulatedColor.a = 1.0;
                break;
            }
        }
        if (u_RenderMode == 1)  //AIP
        {
            accumulatedSum += sampleColor.a;
            sampleCount++;
        }
        if (u_RenderMode == 2)  //MIP
        {
            float intensity = (sampleColor.r + sampleColor.g + sampleColor.b) / 3.0;
            accumulatedMax = max(accumulatedMax, intensity);
        }
        if (u_RenderMode == 3)  //MinIP
        {
            if (density >= 0.1)  
            {
                //有效组织
                float intensity = (sampleColor.r + sampleColor.g + sampleColor.b) / 3.0;
                if (minSampleCount == 0 || intensity < accumulatedMin)
                {
                    accumulatedMin = intensity;
                }
                minSampleCount++;
            }
        }        
        
        //步进到下一个采样点
        currentPos += step;
    }

    //根据渲染模式输出颜色
    if (u_RenderMode == 0)  //Raycast
    {
        FragColor = accumulatedColor;
    }
    if (u_RenderMode == 1)  //AIP
    {
        float avg = sampleCount > 0 ? accumulatedSum / float(sampleCount) : 0.0;
        FragColor = vec4(vec3(avg), 1.0);
    }
    if (u_RenderMode == 2)  //MIP
    {
        FragColor = vec4(vec3(accumulatedMax), 1.0);
    }
    if (u_RenderMode == 3)  //MinIP
    {
        if (minSampleCount == 0)
        {
            FragColor = vec4(0.0, 0.0, 0.0, 1.0);  //没有有效组织，黑色
        }
        else
        {
            FragColor = vec4(vec3(accumulatedMin), 1.0);
        }
    }
    
    //应用Gamma校正（仅 Raycast 模式）
    if (u_RenderMode == 0)
    {
        FragColor.rgb = pow(accumulatedColor.rgb, vec3(1.0/2.2));
        FragColor.a = accumulatedColor.a;
    }
}
