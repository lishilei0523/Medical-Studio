#version 330 core
in vec3 WorldPosition;

out vec4 FragColor;

uniform sampler3D u_PreviewTexture;
uniform usampler3D u_MarkTexture;

uniform vec3 u_VolumeScale;

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

    //归一化snorm范围[-1, 1] -> [0, 1]
	float snormValue = texture(u_PreviewTexture, texCoord).r;
    float normalized = (snormValue + 1.0) / 2.0;
    normalized = clamp(normalized, 0.0, 1.0);
    
    //输出：R=归一化HU值，G=1，B=1，A=标记值/255
    FragColor = vec4(normalized, 1, 1, float(markValue) / 255.0);
}
