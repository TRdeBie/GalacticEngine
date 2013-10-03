//------------------------------------------- Defines -------------------------------------------

#define Pi 3.14159265
#define MAX_LIGHTS 10

//------------------------------------- Top Level Variables -------------------------------------

// Top level variables can and have to be set at runtime

// Matrices for 3D perspective projection 
float4x4 View, Projection, World, InverseTransposeWorld;

//The Camera position
float3 EyeDirection;

// The light source location
float3 LightSource[MAX_LIGHTS];

//Roughness, only used in CookTerrance but declared here anyway
float2 Roughness;

//Base color
float3 BaseColor[MAX_LIGHTS];

//Ambient color
float3 AmbientColor;
float AmbientIntensity;

//Specular lighting
float3 SpecularColor;
float SpecularIntensity;
float SpecularPower;

//---------------------------------- Input / Output structures ----------------------------------

// Each member of the struct has to be given a "semantic", to indicate what kind of data should go in
// here and how it should be treated. Read more about the POSITION0 and the many other semantics in 
// the MSDN library
struct VertexShaderInput
{
	float4 Position3D : POSITION0;
	float4 Normal : NORMAL0;
};

// The output of the vertex shader. After being passed through the interpolator/rasterizer it is also 
// the input of the pixel shader. 
// Note 1: The values that you pass into this struct in the vertex shader are not the same as what 
// you get as input for the pixel shader. A vertex shader has a single vertex as input, the pixel 
// shader has 3 vertices as input, and lets you determine the color of each pixel in the triangle 
// defined by these three vertices. Therefor, all the values in the struct that you get as input for 
// the pixel shaders have been linearly interpolated between there three vertices!
// Note 2: You cannot use the data with the POSITION0 semantic in the pixel shader.
struct VertexShaderOutput
{
	float4 Position2D : POSITION0;
	float4 Normal : TEXCOORD0;
	float4 Depth : TEXCOORD1;
	float4 Position3D : TEXCOORD2;
	float4 WorldPosition : TEXCOORD3;
};

//------------------------------------------ Functions ------------------------------------------

float4 AmbientPortion(void)
{
	return float4(mul(AmbientColor, AmbientIntensity), 1.0f);
}

// Calculating the Lambertian shading that needs to be passed on to the pixel shader
float4 LambertPortion(float3 normal, uint i, float3 Light)
{
	//c = kd * I * max(0, n dot l);
	//Color value = diffuse coefficient * intensity of source * max(0, normal dot reflected light);
	float4 color;
	float intensity = 0.5;
	color = mul(mul(float4(BaseColor[i], 1.0f), intensity), max(0, dot(normal, Light)));
	return color;
}

float4 SpecularPortion(float3 normal, float3 e, uint i, float3 Light)
{
	//c = ks * I * max(0, n dot h) ^ p
	//Color = specular color * intensity light source * max(0, normal dot h) to the power Phong exponent
	//h = (e + l) / |e + l| with e = eye vector and l = light vector and both being unit vectors
	float3 l = normalize(Light);
	float3 h = (e + l) / (length(e) + length(l));
	float4 color = float4(SpecularColor * SpecularIntensity * pow(max(0, dot(normal, h)), SpecularPower), 1.0f);
	return color;
}

float LinearAttenuation(float4 Position3D, uint i)
{
	//returns linear attenuation to a given lightsource to take distance into consideration
	float4 worldPosition = mul(Position3D, World);
	float4 lightPosition = mul(LightSource[i], World);

	//Calculate the distance between pixel and lightsource
	float3 d = worldPosition.xyz - lightPosition;
	float distance = length(d);

	//Calculate the distance attenuation factor, for demonstration it's linear instead of inverse quadratic.
	//Allows for spot-light attenuation to be more apparent
	float lightRange = 100; //Determine a range for the light
	float linearAttenuation = lerp(1.0f, 0.0f, distance / lightRange); //lerp = x + s(y-x)

	return linearAttenuation;
}

float4 CookTorrancePortion(float3 N, float3 V, float m2, float F, uint i, float3 Light)
{
	//Specular lighting by the Cook Terrance method
	//http://www.gamedev.net/topic/444098-hlsl-cook-torrance-lighting/ Voor hulp en bron

	float3 L = normalize(Light);
	float3 H = normalize(V + L);

	float NDotH2 = dot(N, H) * dot(N, H);
	float D1 = 1.0f / (4.0f * m2 * NDotH2 * NDotH2);
	float D2 = exp(-(1.0f - NDotH2) / (m2 * NDotH2));
	float D = D1 * D2;

	float NDotH = dot(N, H);
	float VDotH = max(0, dot(V, H));
	float G1 = (2.0f * NDotH * dot(N, V)) / VDotH;
	float G2 = (2.0f * NDotH * dot(N, L)) / VDotH;
	float G = min(1.0f, max(0, min(G1, G2)));

	float Rs = (D * F * G) / (dot(N, V) * dot(N, L));
	//Instead of using the default specular color (usually white), the light's own colour is chosen
	//Uncomment the line below and comment the one underneath that to revert this
	//float4 color = float4(SpecularIntensity * dot(N, L) * (SpecularColor * Rs), 1.0f);
	float4 color = float4(SpecularIntensity * dot(N, L) * (BaseColor[i] * Rs), 1.0f);
	float intensity = 0.15f;

	return color * intensity;
}

//---------------------------------------- Technique: Simple ----------------------------------------

VertexShaderOutput SimpleVertexShader(VertexShaderInput input)
{
	// Allocate an empty output struct
	VertexShaderOutput output = (VertexShaderOutput)0;

	// Do the matrix multiplications for perspective projection and the world transform
	float4 worldPosition = mul(input.Position3D, World);
    float4 viewPosition  = mul(worldPosition, View);
	output.Position2D    = mul(viewPosition, Projection);

	// Calculate the normal
	float4 normal = normalize(mul(input.Normal, InverseTransposeWorld));
	output.Normal = normal;

	output.Position3D = input.Position3D;

	output.WorldPosition = worldPosition;

	return output;
}

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0
{	
	float4 color = float4(0,0,0,1);
	color = color + AmbientPortion(); //Calculate the ambient portion once
	float3x3 rotationAndScale = (float3x3) InverseTransposeWorld;
	float3 e = normalize(EyeDirection);
	float attenuation;

	//CookTorrance pre-calculations: to avoid having to redo these calculations for each light
	float3 N = mul(input.Normal, rotationAndScale);
	N = normalize(N);
	float3 V = normalize(EyeDirection);
	float m2 = Roughness.x * Roughness.x;
	float F0 = Roughness.y;
	float F = F0 + (1.0f - F0) * pow(1.0f - dot(N, V), 5.0f);

	[loop]
	for(uint i = 0; i < MAX_LIGHTS; i++)
	{
		float3 RelativeLight = normalize(mul(LightSource[i], World) - input.WorldPosition);
		attenuation = LinearAttenuation(input.Position3D, i);
		color = color + (LambertPortion(N, i, RelativeLight) * attenuation);
		color = color + (CookTorrancePortion(N, V, m2, F, i, RelativeLight) * attenuation); 
		//Implemented Cook Torrance lighting for specular highlights with a small twist
		//Instead of using white for the specular colour, the colour of the particular light was chosen
	}

	return color;
}

technique Simple
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader  = compile ps_3_0 SimplePixelShader();
	}
}