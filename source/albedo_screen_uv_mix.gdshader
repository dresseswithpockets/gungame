shader_type spatial;

uniform sampler2D albedo : hint_default_black, filter_nearest;
uniform float albedo_scale = 4;
uniform sampler2D noise : hint_default_white, filter_nearest;
uniform float noise_mix : hint_range(0.0, 1.0, 0.01);

void fragment() {
    vec3 color = texture(albedo, SCREEN_UV * albedo_scale).xyz;
    vec3 noise_color = texture(noise, UV).xyz;
    ALBEDO = mix(color, noise_color, noise_mix);
}
