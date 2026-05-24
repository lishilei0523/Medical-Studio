#version 330 core
in vec3 WorldPosition;

out vec4 FragColor;

uniform sampler3D u_OriginalTexture;
uniform sampler3D u_PreviewTexture;
uniform usampler3D u_MarkTexture;

uniform vec3 u_VolumeScale;

//预览模式：0=Preview, 1=Original
uniform int u_PreviewMode;

//常量
const float MAX_16BIT_SIGNED = 32767.0;


void main()
{
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
    
    //采样标记纹理
    uint markValue = texture(u_MarkTexture, texCoord).r;
    
    float snormValue;
    if (u_PreviewMode == 0)
    {
        snormValue = texture(u_PreviewTexture, texCoord).r;
    }
    else
    {
        snormValue = texture(u_OriginalTexture, texCoord).r;
    }
    
    //归一化snorm到[0, 1]范围（用于存储）
    //snorm范围 -1~1 -> 0~1
    float normalized = (snormValue + 1.0) / 2.0;
    normalized = clamp(normalized, 0.0, 1.0);
    
    //输出：R=归一化HU，G=0，B=0，A=标记值/255
    FragColor = vec4(normalized, 0.0, 0.0, float(markValue) / 255.0);
}
