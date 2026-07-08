#version 330 core
layout(location = 0) in vec3 aPos;

out vec3 WorldPosition;
out vec2 UV;

uniform mat4 u_ModelMatrix;
uniform mat4 u_ViewMatrix;
uniform mat4 u_ProjectionMatrix;

void main()
{
    //计算世界空间位置
    vec4 worldPos = u_ModelMatrix * vec4(aPos, 1.0);
    WorldPosition = worldPos.xyz;

    //计算U/V坐标
    UV = aPos.xy + 0.5;  //UnitPlane: -0.5~0.5 -> 0~1

    //计算裁剪空间位置
    gl_Position = u_ProjectionMatrix * u_ViewMatrix * worldPos;
}
