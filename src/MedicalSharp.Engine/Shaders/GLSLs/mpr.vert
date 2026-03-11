#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 2) in vec2 aTexCoord;

out vec2 TexCoord;
out vec3 WorldPosition;

uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

void main()
{
    //计算世界空间位置
    vec4 worldPos = u_ModelMatrix * vec4(aPos, 1.0);
    WorldPosition = worldPos.xyz;
    
    //传递纹理坐标
    TexCoord = aTexCoord;
    
    //计算裁剪空间位置
    gl_Position = u_ProjectionMatrix * u_ViewMatrix * worldPos;
}
