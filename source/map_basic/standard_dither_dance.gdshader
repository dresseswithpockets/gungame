shader_type spatial;

const float PX_PER_UNIT = 16.0;

uniform sampler2D albedo;

uniform vec2 dither_pos;
uniform sampler2D dither_pos_tex;

float saturatef(float x) {
    return clamp(x, 0, 1);
}

float luma3(vec3 color) {
  return dot(color, vec3(0.299, 0.587, 0.114));
}

float luma4(vec4 color) {
  return dot(color.rgb, vec3(0.299, 0.587, 0.114));
}

float dither4x4(vec2 position, float brightness) {
  int x = int(mod(position.x, 4.0));
  int y = int(mod(position.y, 4.0));
  int index = x + y * 4;
  float limit = 0.0;

  if (x < 8) {
    if (index == 0) limit = 0.0625;
    if (index == 1) limit = 0.5625;
    if (index == 2) limit = 0.1875;
    if (index == 3) limit = 0.6875;
    if (index == 4) limit = 0.8125;
    if (index == 5) limit = 0.3125;
    if (index == 6) limit = 0.9375;
    if (index == 7) limit = 0.4375;
    if (index == 8) limit = 0.25;
    if (index == 9) limit = 0.75;
    if (index == 10) limit = 0.125;
    if (index == 11) limit = 0.625;
    if (index == 12) limit = 1.0;
    if (index == 13) limit = 0.5;
    if (index == 14) limit = 0.875;
    if (index == 15) limit = 0.375;
  }

  return brightness < limit ? 0.0 : 1.0;
}

vec4 dither4x4_4(vec2 position, vec4 color) {
  return vec4(color.rgb * dither4x4(position, luma4(color)), 1.0);
}

vec3 dither4x4_3(vec2 position, vec3 color) {
  return color * dither4x4(position, luma3(color));
}

const int DITHER_MATRIX[64] = {
    0, 32, 8, 40, 2, 34, 10, 42,
    48, 16, 56, 24, 50, 18, 58, 26,
    12, 44, 4, 36, 14, 46, 6, 38,
    60, 28, 52, 20, 62, 30, 54, 22,
    3, 35, 11, 43, 1, 33, 9, 41,
    51, 19, 59, 27, 49, 17, 57, 25,
    15, 47, 7, 39, 13, 45, 5, 37,
    64, 31, 55, 23, 61, 29, 53, 21
};

float find_nearest(ivec2 pos, float c) {
    float limit = 0.0;
    if (pos.x < 8) {
        
    }
    
    if (c < limit) {
        return 0.0;
    }
    
    return 1.0;
}

void fragment() {
//    vec3 color = texture(albedo, UV).xyz;
//    ALBEDO = dither4x4_3(vec2(dither_pos * 2), color);
    
    // ivec2 pos_dither = ivec2(floor(dither_pos.xy * PX_PER_UNIT));
//    float z = -texture(dither_pos_tex, UV).z;
//
//    const float r = 1.0 / 16.0;
//    int q = ((pos_dither.x * 17 % 8) * 8) + ((pos_dither.y * 31) % 8);
//    float dither_offset = mix(r * (float(DITHER_MATRIX[q]) / 255.0), 0.0, saturatef(z / 8.0));
//    ALBEDO = texture(albedo, UV).xyz + vec3(0, 0, dither_offset);

}
