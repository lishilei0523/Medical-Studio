#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform int u_ColorMode;        //0: 纯色, 1: 纹理, 2: 混合(纹理灰度+颜色)
uniform vec4 u_Color;
uniform sampler2D u_Texture;

void main()
{
    if (u_ColorMode == 1)
    {
        FragColor = texture(u_Texture, TexCoord);
    }
    else if (u_ColorMode == 2)
    {
        float alpha = texture(u_Texture, TexCoord).r;
        FragColor = vec4(u_Color.rgb, u_Color.a * alpha);
    }
    else 
    {
        FragColor = u_Color;
    }
}
