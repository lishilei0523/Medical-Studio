#version 330 core
layout(location = 0) in vec3 aPos;

out vec3 WorldPosition;
out vec3 LocalPosition;

uniform mat4 u_ProjectionMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ModelMatrix;

void main()
{
	//位置变换
    vec4 worldPos = u_ModelMatrix * vec4(aPos, 1.0);

    WorldPosition = worldPos.xyz;    
    LocalPosition = aPos;

    //将位置转换到裁剪空间
    gl_Position = u_ProjectionMatrix * u_ViewMatrix * worldPos;
}
