shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_disabled,diffuse_burley,specular_schlick_ggx,unshaded,shadows_disabled,ambient_light_disabled;
uniform vec4 albedo : source_color = vec4(1);
uniform sampler2D texture_albedo : source_color,filter_linear,repeat_disable;
uniform float point_size : hint_range(0,128) = 1;
uniform float roughness : hint_range(0,1) = 1;
uniform sampler2D texture_metallic : hint_default_white,filter_linear,repeat_disable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r,filter_linear,repeat_disable;
uniform float specular = 0.5;
uniform float metallic = 0;
uniform vec3 uv1_scale = vec3(1);
uniform vec3 uv1_offset = vec3(0);
uniform vec3 uv2_scale = vec3(1);
uniform vec3 uv2_offset = vec3(0);
//uniform vec2 depth_viewport_size = vec2(0);

void vertex() {
//    disabling this until I figure out multi-portal recursion
//    if (VIEWPORT_SIZE != depth_viewport_size) {
//        VERTEX = CAMERA_POSITION_WORLD + vec3(0, 0, -50000);
//    }
    UV=UV*uv1_scale.xy+uv1_offset.xy;
}

void fragment() {
    vec2 base_uv = SCREEN_UV;
    vec4 albedo_tex = texture(texture_albedo,base_uv);
    ALBEDO = albedo.rgb * albedo_tex.rgb;
    
    float metallic_tex = dot(texture(texture_metallic,base_uv),metallic_texture_channel);
    METALLIC = metallic_tex * metallic;
    
    vec4 roughness_texture_channel = vec4(1.0,0.0,0.0,0.0);
    float roughness_tex = dot(texture(texture_roughness,base_uv),roughness_texture_channel);
    ROUGHNESS = roughness_tex * roughness;
    
    SPECULAR = specular;
}
