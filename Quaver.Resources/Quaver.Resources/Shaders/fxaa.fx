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

float2 p_texel;
float  p_strength;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float reducemul = 1.0 / 8.0;
	float reducemin = 1.0 / 128.0;

	float3 basecol =   tex2D(SpriteTextureSampler, input.TextureCoordinates).rgb;
    float3 baseNW =    tex2D(SpriteTextureSampler, input.TextureCoordinates - p_texel).rgb;
	float3 baseNE =    tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(p_texel.x, - p_texel.y)).rgb;
    float3 baseSW =	   tex2D(SpriteTextureSampler, input.TextureCoordinates + float2(- p_texel.x, p_texel.y)).rgb;
	float3 baseSE =    tex2D(SpriteTextureSampler, input.TextureCoordinates + p_texel).rgb;

	float3 gray = float3(0.299, 0.587, 0.114);

    float monocol = dot(basecol, gray);
    float monoNW =  dot(baseNW, gray);
    float monoNE =  dot(baseNE, gray);
    float monoSW =  dot(baseSW, gray);
	float monoSE =  dot(baseSE, gray);

	float monomin = min(monocol, min(min(monoNW, monoNE), min(monoSW, monoSE)));
	float monomax = max(monocol, max(max(monoNW, monoNE), max(monoSW, monoSE)));

	float2 dir = float2(-((monoNW + monoNE) - (monoSW + monoSE)), ((monoNW + monoSW) - (monoNE + monoSE)));
    float dirreduce = max((monoNW + monoNE + monoSW + monoSE) * reducemul * 0.25, reducemin);
    float dirmin = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirreduce);
	dir = min(float2(p_strength, p_strength), max(float2(-p_strength, -p_strength), dir * dirmin)) * p_texel;



	float4 resultA = 0.5 * (tex2D(SpriteTextureSampler, input.TextureCoordinates + dir * -0.166667) +
	 					    tex2D(SpriteTextureSampler, input.TextureCoordinates + dir *  0.166667));

	float4 resultB = resultA * 0.5 + 0.25 * (tex2D(SpriteTextureSampler, input.TextureCoordinates + dir * -0.5) +
											 tex2D(SpriteTextureSampler, input.TextureCoordinates + dir *  0.5));

	float monoB = dot(resultB.rgb, gray);
	float4 color;

	if(monoB < monomin || monoB > monomax) 
	{
        color = resultA * input.Color;
    } 
	else 
	{
        color = resultB * input.Color;
	}					

	return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};