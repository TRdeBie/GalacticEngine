//------------------------------------------- Defines -------------------------------------------

#define Pi 3.14159265

//------------------------------------- Top Level Variables -------------------------------------

// Top level variables can and have to be set at runtime

// Matrices for 3D perspective projection 
float4x4 View, Projection, World, InverseTransposeWorld;
float4x4 LightWorldViewProjection;

//The Camera position
float3 EyeDirection;

// The light source location
float3 LightSource;

//Spotlight info
float3 LightDirection;
float InnerAngle;
float OuterAngle;

//Roughness, only used in CookTerrance but declared here anyway
float2 Roughness;

//Base color
float3 BaseColor;

//Ambient color
float3 AmbientColor;
float AmbientIntensity;

//Specular lighting
float3 SpecularColor;
float SpecularIntensity;
float SpecularPower;

//Texture for our spotlight
texture SampleTexture;
sampler TextureSampler = sampler_state
{
	Texture = <SampleTexture>;
};


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
	float4 DirectionalColor : TEXCOORD3;
	float4 TexPosition : TEXCOORD4;
};

//------------------------------------------ Functions ------------------------------------------

//Spotlight Function
float4 SpotLight(float4 Position3D)
{
	//Following http://content.gpwiki.org/index.php/D3DBook:(Lighting)_Direct_Light_Sources
	//For directions on how to implement a spotlight
	//This spotlight shader was created using the directions from the example
	//The major difference lies in the fact that the example has its spotlight code in the vertex shader,
	//whereas this is done in the pixel shader

	float4 worldPosition = mul(Position3D, World);
	float4 lightPosition = mul(LightSource, World);

	//Calculate the distance between pixel and lightsource
	float3 d = worldPosition.xyz - lightPosition;
	float distance = length(d);

	//Calculate the distance attenuation factor, for demonstration it's linear instead of inverse quadratic.
	//Allows for spot-light attenuation to be more apparent
	float lightRange = 200; //300 is max view range
	float linearAttenuation = lerp(1.0f, 0.0f, distance / lightRange); //lerp = x + s(y-x)

	//Calculate the direction to the light
	float3 directionToLight = normalize(lightPosition - worldPosition);

	//The angle between the current sample and the light's direction
	float cosAlpha = max(0.0f, dot(directionToLight, LightDirection));

	//Then calculate the spot attenuation factor
	float falloff = 1.0f; //Determines transition between middle of spotlight and edges
	float spotAttenuation = 0.0f; //Default simplifies the following
	if (cosAlpha > InnerAngle)
		spotAttenuation = 1.0f;
	else if (cosAlpha > OuterAngle)
		spotAttenuation = pow(abs((cosAlpha - OuterAngle) / (InnerAngle - OuterAngle)), falloff);

	//Final attenuation is the product of both types
	float attenuation = linearAttenuation * spotAttenuation;

	return attenuation;
}

// Calculating the Lambertian shading that needs to be passed on to the pixel shader
float4 AmbientPortion(void)
{
	return float4(mul(AmbientColor, AmbientIntensity), 1.0f);
}

float4 LambertPortion(float3 normal)
{
	//c = kd * I * max(0, n dot l);
	//Color value = diffuse coefficient * intensity of source * max(0, normal dot reflected light);
	float4 color;
	float intensity = 0.01;
	color = float4(mul(mul(BaseColor, intensity), max(0, dot(normal, LightSource))), 1.0f);
	return color;
}

float4 SpecularPortion(float3 normal)
{
	//c = ks * I * max(0, n dot h) ^ p
	//Color = specular color * intensity light source * max(0, normal dot h) to the power Phong exponent
	//h = (e + l) / |e + l| with e = eye vector and l = light vector and both being unit vectors
	float3 e = normalize(EyeDirection);
	float3 l = normalize(LightSource);
	float3 h = (e + l) / (length(e) + length(l));
	float4 color = float4(SpecularColor * SpecularIntensity * pow(max(0, dot(normal, h)), SpecularPower), 1.0f);
	float intensity = 0.2f;
	return color * intensity;
}

float4 CookTorrancePortion(float3 N)
{
	//Specular lighting by the Cook Terrance method
	//http://www.gamedev.net/topic/444098-hlsl-cook-torrance-lighting/ Voor hulp en bron

	float3 V = normalize(EyeDirection);
	float3 L = normalize(LightSource);
	float3 H = normalize(V + L);

	float m2 = Roughness.x * Roughness.x;
	float NDotH2 = dot(N, H) * dot(N, H);
	float D1 = 1.0f / (4.0f * m2 * NDotH2 * NDotH2);
	float D2 = exp(-(1.0f - NDotH2) / (m2 * NDotH2));
	float D = D1 * D2;

	float F0 = Roughness.y;
	float F = F0 + (1.0f - F0) * pow(1.0f - dot(N, V), 5.0f);

	float NDotH = dot(N, H);
	float VDotH = max(0, dot(V, H));
	float G1 = (2.0f * NDotH * dot(N, V)) / VDotH;
	float G2 = (2.0f * NDotH * dot(N, L)) / VDotH;
	float G = min(1.0f, max(0, min(G1, G2)));

	float Rs = (D * F * G) / (dot(N, V) * dot(N, L));
	float4 color = float4(SpecularIntensity * dot(N, L) * (SpecularColor * Rs), 1.0f);
	float intensity = 0.15f;

	return color * intensity;
}

float4 SpotlightColor(float2 TextureCoordinate:TEXCOORD0)
{
	float4 color = tex2D(TextureSampler, TextureCoordinate);

	return color;
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
	output.TexPosition = mul(input.Position3D, LightWorldViewProjection);

	return output;
}

float4 SimplePixelShader(VertexShaderOutput input) : COLOR0
{	
	//As per the Spotlight.fx shader, the attenuation is a value between 0 and 1.0f, by which the light portions are multiplied.
	//Anything outside of the spotlight has an attenuation of 0, and anything inside of it has one >0
	float attenuation = SpotLight(input.Position3D);

	float4 baseColor = float4(0 ,0, 0, 1);
	float3x3 rotationAndScale = (float3x3) InverseTransposeWorld;
	float3 newNormal = mul(input.Normal, rotationAndScale);
	newNormal = normalize(newNormal);

	float2 texCoord;
	//To find the proper texture coordinate, divide the x and z by the w, then divide by two and add 0.5 to convert from [-1,1] to [0, 1] region
	texCoord[0] = input.TexPosition.x / input.TexPosition.w / 2.0f + 0.5f;
	texCoord[1] = input.TexPosition.y / input.TexPosition.w / 2.0f + 0.5f;
	float4 textureColor = SpotlightColor(texCoord);

	float4 lambert = LambertPortion(newNormal); //Calculating the Lambertian portion once
	float lambertIntensity = (lambert.x + lambert.y + lambert.z) / 3; //Using the Lambertian portion to determine how brightly lit a pixel is lit
	baseColor = textureColor * attenuation * lambertIntensity; //Multiplied by that brightness to ensure pixels facing away from the light aren't lit by it

	//As usual for a spotlight
	baseColor = baseColor + (attenuation + 0.3f) * AmbientPortion();
	baseColor = baseColor + attenuation * lambert;
	baseColor = baseColor + attenuation * CookTorrancePortion(newNormal);
	baseColor = baseColor + attenuation * SpecularPortion(newNormal);

	return baseColor;
}

technique Simple
{
	pass Pass0
	{
		VertexShader = compile vs_3_0 SimpleVertexShader();
		PixelShader  = compile ps_3_0 SimplePixelShader();
	}
}