shader_type spatial;

group_uniforms face;
uniform sampler2D albedo: source_color,filter_nearest,repeat_enable;
uniform float spin_rate = 1;
uniform vec2 uv_mid = vec2(0.5);

group_uniforms fresnel;
uniform float fresnel_amount = 1;
uniform float fresnel_intensity = 1;
uniform vec3 fresnel_color: source_color = vec3(1);

vec3 fresnel_glow(float amount, float intensity, vec3 color, vec3 normal, vec3 view)
{
    return pow((1.0 - dot(normalize(normal), normalize(view))), amount) * color * intensity;
}

vec2 rotateUV(vec2 uv, float rotation, vec2 mid)
{
    float cosAngle = cos(rotation);
    float sinAngle = sin(rotation);
    vec2 p = uv - mid;
    return vec2(
        cosAngle * p.x + sinAngle * p.y + mid.x,
        cosAngle * p.y - sinAngle * p.x + mid.y
    );
}

void fragment() {
    vec2 uv = rotateUV(UV, TIME * spin_rate, uv_mid);
    vec4 color = texture(albedo, uv);
    vec3 fresnel = fresnel_glow(fresnel_amount, fresnel_intensity, fresnel_color, NORMAL, VIEW);
    ALBEDO = color.xyz + fresnel;
    ALPHA = color.w;
    EMISSION = color.xyz;
}
