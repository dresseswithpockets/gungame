shader_type particles;

uniform float time_scale = 2.0;
uniform float amplitude = 4.0;
uniform float minimum = -4.0;
uniform vec3 start_extent = vec3(1);

float rand_from_seed(in uint seed) {
  int k;
  int s = int(seed);
  if (s == 0)
    s = 305420679;
  k = s / 127773;
  s = 16807 * (s - k * 127773) - 2836 * k;
  if (s < 0)
    s += 2147483647;
  seed = uint(s);
  return float(seed % uint(65536)) / 65535.0;
}

uint hash(uint x) {
  x = ((x >> uint(16)) ^ x) * uint(73244475);
  x = ((x >> uint(16)) ^ x) * uint(73244475);
  x = (x >> uint(16)) ^ x;
  return x;
}

void start() {
    uint seed1 = hash(NUMBER + uint(1) + RANDOM_SEED);
    uint seed2 = hash(NUMBER + uint(27) + RANDOM_SEED);
    uint seed3 = hash(NUMBER + uint(43) + RANDOM_SEED);
    uint seed4 = hash(NUMBER + uint(111) + RANDOM_SEED);
    
    CUSTOM.x = rand_from_seed(seed1);
    vec3 position = vec3(
        rand_from_seed(seed2) * 2.0 - 1.0,
        rand_from_seed(seed3) * 2.0 - 1.0,
        rand_from_seed(seed4) * 2.0 - 1.0);
        
    TRANSFORM[3].xyz = position * start_extent;
}

void process() {
    float adjust = amplitude + minimum;
    VELOCITY.z = cos(TIME + CUSTOM.x * time_scale * PI) * amplitude + adjust;
    VELOCITY.x = sin(TIME + CUSTOM.x * time_scale * PI) * amplitude + adjust;
}
