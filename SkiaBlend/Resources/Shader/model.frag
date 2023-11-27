#version 320 es

precision highp float;

in vec2 vs_UV;

out vec4 out_Color;

uniform sampler2D u_Tex;

void main() {
	out_Color = vec4(texture(u_Tex, vs_UV).rgb, 1.0);
}
