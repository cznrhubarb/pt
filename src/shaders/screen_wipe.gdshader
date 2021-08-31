shader_type canvas_item;
render_mode unshaded;

uniform float cutoff : hint_range(0.0, 1.0);
uniform float flash_size : hint_range(0.0, 1.0);
uniform sampler2D mask : hint_albedo;

void fragment() {
	float value = texture(mask, UV).r;
	float alpha = smoothstep(cutoff, cutoff + flash_size, value * (1.0 - flash_size) + flash_size);
	if (alpha >= 0.98) {
		COLOR = texture(TEXTURE, UV);
	} else if (alpha <= 0.02) {
		COLOR = vec4(0);
	} else {
		COLOR = vec4(1);
	}
}