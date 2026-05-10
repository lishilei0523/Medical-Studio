#version 330 core
in vec2 TexCoord;
in vec3 WorldPosition;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform usampler3D u_MarkTexture;
uniform sampler1D u_TransferFunction;
uniform sampler1D u_MarkStrategy;

uniform vec3 u_VolumeScale;
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;
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
const float EPSILON = 0.0001;
const float MAX_16BIT_SIGNED = 32767.0;


//将R16Snorm值转换为原始像素值
float convertR16SnormToRaw(float snormValue)
{
    return snormValue * MAX_16BIT_SIGNED;
}

//线性窗宽窗位转换
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
    return clamp(result, 0.0, 1.0);
}

void main()
{
    //2D纹理边界检查
    if (TexCoord.x < 0.0 || TexCoord.x > 1.0 || TexCoord.y < 0.0 || TexCoord.y > 1.0)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 0.0);
        return;
    }
    
    //构建3D纹理坐标
    vec3 texCoord = (WorldPosition / u_VolumeScale) + 0.5;
    
    //3D纹理边界检查
    if (texCoord.x < 0.0 || texCoord.x > 1.0 ||
        texCoord.y < 0.0 || texCoord.y > 1.0 ||
        texCoord.z < 0.0 || texCoord.z > 1.0)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 0.0);
        return;
    }
    
    //采样体积纹理 (R16Snorm格式)
    float snormValue = texture(u_VolumeTexture, texCoord).r;

    //采样标记纹理
    uint markValue = texture(u_MarkTexture, texCoord).r;

    //根据标记模式决定是否渲染
    int markMode = u_MarkModes[markValue];
    if (markMode == 1) //Collapsed(1) - 隐藏
    {
        discard;
    }

    //转换为原始值
    float rawValue = convertR16SnormToRaw(snormValue);

    //应用重缩放
    float medicalValue = rawValue * u_RescaleSlope + u_RescaleIntercept;

    //基础颜色
    vec3 color;

    //Gray - 灰度模式
    if (u_RenderMode == 0) 
    {
        //应用窗宽窗位
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
    }

    //染色模式下，标记区域混合染色颜色
    if (markMode == 2 && markValue != 0u)
    {
        //从标记颜色纹理采样（纹理坐标 = 标记值 / 255）
        float markTexCoord = float(markValue) / 255.0;
        vec4 markColor = texture(u_MarkStrategy, markTexCoord);
        
        //颜色叠加：用标记颜色的Alpha作为混合系数
        color = mix(color, markColor.rgb, markColor.a);
    }

    FragColor = vec4(color, 1.0);
}
