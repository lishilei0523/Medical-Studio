#version 330 core
in vec3 vWorldPos;
in vec3 vTexCoord;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform sampler1D u_TransferFunction;

uniform int u_PlaneType;
uniform float u_RescaleSlope;
uniform float u_RescaleIntercept;
uniform float u_WindowWidth;
uniform float u_WindowCenter;

//线性窗宽窗位转换
float applyWindowLevel(float voxelValue, float windowCenter, float windowWidth)
{
    float windowMin = windowCenter - windowWidth * 0.5;
    float windowMax = windowCenter + windowWidth * 0.5;    

    //裁剪到窗口范围内
    voxelValue = clamp(voxelValue, windowMin, windowMax);
    
    //窗内：线性映射到[0,1]
    return (voxelValue - windowMin) / windowWidth;
}

void main()
{
    //根据平面类型采样3D纹理
    vec3 sampleCoord = vTexCoord;
    
    //确保在有效范围内
    if (any(lessThan(sampleCoord, vec3(0.0))) || any(greaterThan(sampleCoord, vec3(1.0)))) 
    {
        discard;
    }
    
    //采样体积纹理
    float originalValue = texture(u_VolumeTexture, sampleCoord).r;
    float voxelValue = originalValue * 32767.0 * u_RescaleSlope + u_RescaleIntercept; 
    float density = applyWindowLevel(voxelValue, u_WindowCenter, u_WindowWidth);

    //采样传输函数
    vec4 color = texture(u_TransferFunction, density);
    
    //添加网格线（可选）
    //vec2 gridCoord = fract(vTexCoord.xy * 10.0);
    //if (gridCoord.x < 0.02 || gridCoord.y < 0.02)
    //{
    //    color.rgb = mix(color.rgb, vec3(1.0, 0.0, 0.0), 0.3);
    //}
    
    FragColor = color;
}
