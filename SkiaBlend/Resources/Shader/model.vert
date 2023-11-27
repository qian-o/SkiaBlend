#version 320 es

in vec3 in_Pos;
in vec2 in_UV;

out vec2 vs_UV;

uniform mat4 u_MVP;

void main() {
    gl_Position = u_MVP * vec4(in_Pos, 1.0);
    vs_UV = in_UV;
}
