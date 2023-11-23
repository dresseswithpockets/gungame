shader_type spatial;
//render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx,unshaded,shadows_disabled,ambient_light_disabled;
render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx,unshaded,shadows_disabled,ambient_light_disabled;

uniform vec4 albedo = vec4(1, 1, 1, 1);
uniform sampler2D albedo_texture;
uniform float specular : hint_range(0,1) = 0.5;

uniform float metallic : hint_range(0,1) = 0.0;
uniform sampler2D metallic_texture : hint_default_white,filter_linear,repeat_disable;
uniform vec4 metallic_channel = vec4(1, 0, 0, 0);

uniform float roughness : hint_range(0,1) = 1.0;
uniform sampler2D roughness_texture : hint_roughness_r,filter_linear,repeat_disable;
uniform vec4 roughness_channel = vec4(1, 0, 0, 0);

void fragment() {
    vec2 base_uv = SCREEN_UV;
    ALBEDO = albedo.rgb * texture(albedo_texture, base_uv).rgb;
    METALLIC = metallic * dot(texture(metallic_texture, base_uv), metallic_channel);
    ROUGHNESS = roughness * dot(texture(roughness_texture, base_uv), roughness_channel);
    SPECULAR = specular;
}
