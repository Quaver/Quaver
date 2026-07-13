#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

// The pixel-space width/height of the rectangle being drawn.
float2 p_size;

// The corner radius, in pixels.
float p_radius;

// Exponent for the corner's distance norm. 2.0 is a true circular arc, which (correctly) still
// reads as a flat vertical/horizontal run near a cap's tip before it visibly curves - the tangent
// there is exactly parallel to the edge, so the boundary's deviation from straight grows only with
// the square of the distance travelled. Dropping the exponent below 2 sharpens the curve so it
// bends away almost immediately, at the cost of the cap looking a bit more like a pointed lens than
// a perfectly soft circular pill. Back to a true circle now that the feather is crisp (fwidth-based)
// instead of the old fixed 3px blur, which was what made the flat run at the tip look exaggerated.
static const float CORNER_NORM_EXPONENT = 2.0;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 halfSize = p_size * 0.5;

	// Pixel position relative to the center of the rectangle.
	float2 p = input.TextureCoordinates * p_size - halfSize;

	float2 q = abs(p) - (halfSize - p_radius);
	float2 qPos = max(q, 0);
	float cornerDist = pow(pow(qPos.x, CORNER_NORM_EXPONENT) + pow(qPos.y, CORNER_NORM_EXPONENT), 1.0 / CORNER_NORM_EXPONENT);
	float dist = cornerDist + min(max(q.x, q.y), 0) - p_radius;

	// Anti-aliased feather sized from the screen-space derivative of dist rather than a fixed virtual-
	// pixel constant. fwidth(dist) is how much dist changes between adjacent real pixels, so this always
	// resolves to ~1 real pixel of blur. crisp at native (1:1) UI scale, but still automatically widens
	// enough to stay anti-aliased if the button is ever drawn smaller than its virtual size (e.g. a
	// downscaled backbuffer), instead of a fixed constant that's either too blurry or too jagged
	// depending on scale.
	//
	// The window is [-aa, 0] rather than centered on 0 so the blur only ever eats into the shape's own
	// interior, never bleeding past its true (p_size/p_radius-defined) footprint - matters for
	// pixel-perfect layout/hover bounds that assume the visible edge never exceeds the logical one.
	float aa = max(fwidth(dist), 0.0001);
	float coverage = 1 - smoothstep(-aa, 0, dist);

	float4 texColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float4 color = texColor * input.Color;
	color.a *= coverage;

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
