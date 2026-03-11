#version 330 core
in vec2 TexCoord;
in vec3 WorldPosition;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform int u_PlaneType;               //平面类型: 0-Axial, 1-Coronal, 2-Sagittal
uniform float u_SliceIndex;            //切片索引

//窗宽窗位参数
uniform float u_WindowWidth;
uniform float u_WindowCenter;

//材质参数
uniform float u_Brightness;
uniform float u_Contrast;

//DICOM重缩放参数
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;

//体积参数
uniform vec3 u_VolumeScale;            //体积缩放
uniform vec3 u_VoxelSize;              //体素尺寸
uniform vec3 u_Spacing;                //间距

//常量
const float EPSILON = 0.0001;
const float MAX_16BIT_SIGNED = 32767.0;  //16位有符号整数最大值

//将R16Snorm值转换为原始像素值
float convertR16SnormToRaw(float snormValue)
{
    return snormValue * MAX_16BIT_SIGNED;
}

//应用窗宽窗位
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

//获取归一化的切片索引
float getNormalizedSliceIndex()
{
    float maxIndex;
    
    //根据平面类型选择正确的维度
    switch (u_PlaneType)
    {
        case 0: //Axial - 沿Z轴切片
            maxIndex = u_VoxelSize.z - 1.0;
            break;
        case 1: //Coronal - 沿Y轴切片
            maxIndex = u_VoxelSize.y - 1.0;
            break;
        case 2: //Sagittal - 沿X轴切片
            maxIndex = u_VoxelSize.x - 1.0;
            break;
        default:
            maxIndex = u_VoxelSize.z - 1.0;
            break;
    }

    return u_SliceIndex / maxIndex;
}

//根据平面类型构建3D纹理坐标
vec3 getTextureCoordinate(vec2 texCoord, float sliceNormalized)
{
    switch (u_PlaneType)
    {
        case 0: //Axial - XY平面
            return vec3(texCoord.x, texCoord.y, sliceNormalized);            
        case 1: //Coronal - XZ平面
            return vec3(texCoord.x, sliceNormalized, texCoord.y);            
        case 2: //Sagittal - YZ平面
            return vec3(sliceNormalized, texCoord.x, texCoord.y);            
        default:
            return vec3(texCoord, sliceNormalized);
    }
}

void main()
{
    //2D纹理边界检查
    if (TexCoord.x < 0.0 || TexCoord.x > 1.0 || TexCoord.y < 0.0 || TexCoord.y > 1.0)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 0.0);
        return;
    }
    
    //获取归一化切片索引
    float sliceNormalized = getNormalizedSliceIndex();
    
    //构建3D纹理坐标
    vec3 texCoord = getTextureCoordinate(TexCoord, sliceNormalized);
    
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
    
    //转换为原始值
    float rawValue = convertR16SnormToRaw(snormValue);
    
    //应用重缩放
    float medicalValue = rawValue * u_RescaleSlope + u_RescaleIntercept;
    
    //应用窗宽窗位
    float grayValue = applyWindowLevel(medicalValue, u_WindowCenter, u_WindowWidth);
    
    //应用亮度和对比度
    grayValue = (grayValue - 0.5) * u_Contrast + 0.5;
    grayValue *= u_Brightness;
    grayValue = clamp(grayValue, 0.0, 1.0);

    //基础颜色
    vec4 color = vec4(grayValue, grayValue, grayValue, 1.0);

    FragColor = color;
}
