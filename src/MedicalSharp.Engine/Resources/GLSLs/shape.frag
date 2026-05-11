#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform int u_HasTexture;       //0: 无纹理（纯色），1: 有纹理
uniform sampler2D u_Texture;
uniform vec4 u_Color;

void main()
{
    if (u_HasTexture == 1)
    {
        float alpha = texture(u_Texture, TexCoord).r;
        FragColor = vec4(u_Color.rgb, u_Color.a * alpha);
    }
    else
    {
        FragColor = u_Color;
    }
}
