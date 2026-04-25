#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

uniform sampler2D u_TextTexture;
uniform vec4 u_Color;

void main()
{
    float alpha = texture(u_TextTexture, TexCoord).r;
    FragColor = vec4(u_Color.rgb, u_Color.a * alpha);
}
