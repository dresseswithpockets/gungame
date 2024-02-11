shader_type spatial;

uniform vec4 albedo_color : source_color = vec4(1);
uniform sampler2D albedo : hint_default_black, filter_nearest;
uniform float wave_scale;
uniform float wave_frequency;
uniform float wave_amplitude;

void fragment() {
    vec2 sin_time = (UV.yx * wave_scale + TIME * wave_frequency) * PI;
    vec2 uv = UV + sin(sin_time) * wave_amplitude;
    vec4 color = texture(albedo, uv);
    ALBEDO = color.xyz * albedo_color.xyz;
    SPECULAR = 0.0;
}
