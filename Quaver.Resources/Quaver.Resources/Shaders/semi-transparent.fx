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


float2 p_dimensions;

float2 p_position;
float2 p_rectangle;

float  p_alpha;


float4 MainPS(VertexShaderOutput input) : COLOR
{

	float2 coord = input.TextureCoordinates * p_dimensions;

	if (coord.x <= p_position.x + p_rectangle.x && coord.x >= p_position.x &&
		coord.y <= p_position.y + p_rectangle.y && coord.y >= p_position.y)
	{
		input.Color.a = p_alpha;
	}

	return tex2D(SpriteTextureSampler,input.TextureCoordinates) * input.Color;	
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};