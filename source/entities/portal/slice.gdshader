shader_type spatial;

uniform vec4 albedo : source_color = vec4(1);
uniform sampler2D texture_albedo : source_color,filter_linear,repeat_disable;
uniform float point_size : hint_range(0,128) = 1;
uniform float roughness : hint_range(0,1) = 1;
uniform sampler2D texture_metallic : hint_default_white,filter_linear,repeat_disable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear,repeat_disable;
uniform float specular = 0.5;
uniform float metallic = 0;

uniform vec3 slice_center;
uniform vec3 slice_normal;

void fragment() {
    // VERTEX is passed as view_space by default
    vec3 world_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
    float slice_side = dot(slice_normal, world_pos - slice_center);
    if (slice_side > 0.0f) discard;
    
    vec2 base_uv = UV;
    vec4 albedo_tex = texture(texture_albedo,base_uv);
    ALBEDO = albedo.rgb * albedo_tex.rgb;
    
    float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
    METALLIC = metallic_tex * metallic;
    
    vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
    float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
    ROUGHNESS = roughness_tex * roughness;
    
    SPECULAR = specular;
}
