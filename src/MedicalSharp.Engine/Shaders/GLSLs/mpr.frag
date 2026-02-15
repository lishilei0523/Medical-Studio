#version 330 core
in vec3 vWorldPos;
in vec3 vTexCoord;

out vec4 FragColor;

uniform sampler3D u_VolumeTexture;
uniform sampler1D u_TransferFunction;

uniform int u_PlaneType;
uniform float u_WindowWidth = 400.0;
uniform float u_WindowCenter = 40.0;

//计算窗宽窗位
float applyWindowLevel(float value)
{
    float windowMin = u_WindowCenter - u_WindowWidth / 2.0;
    float windowMax = u_WindowCenter + u_WindowWidth / 2.0;
    
    //裁剪到窗口范围内
    value = clamp(value, windowMin, windowMax);
    
    //线性映射到 [0, 1]
    return (value - windowMin) / (windowMax - windowMin);
}

void main()
{
    //根据平面类型采样3D纹理
    vec3 sampleCoord = vTexCoord;
    
    //确保在有效范围内
    if (any(lessThan(sampleCoord, vec3(0.0))) || 
        any(greaterThan(sampleCoord, vec3(1.0)))) 
    {
        discard;
    }
    
    //采样体积数据
    float density = texture(u_VolumeTexture, sampleCoord).r;
    
    //应用窗宽窗位（可选）
    //density = applyWindowLevel(density * 4095.0 - 1024.0);
    
    //采样传输函数
    vec4 color = texture(u_TransferFunction, density);
    
    //添加网格线（可选）
    vec2 gridCoord = fract(vTexCoord.xy * 10.0);
    if (gridCoord.x < 0.02 || gridCoord.y < 0.02)
    {
        color.rgb = mix(color.rgb, vec3(1.0, 0.0, 0.0), 0.3);
    }
    
    FragColor = color;
}
