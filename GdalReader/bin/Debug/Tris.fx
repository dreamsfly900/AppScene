float4x4 worldViewProjection;
float Time=1.0f;
struct VS_OUTPUT
{  float4 Pos   : POSITION;
   float4 Color : COLOR;
};
VS_OUTPUT VS(float4 Pos:POSITION,float4 Color:COLOR)
{   	VS_OUTPUT Out = (VS_OUTPUT)0;
	float4 pos1=Pos;        
	pos1.y+=cos(Pos+(Time*2.0f));
    	Out.Pos = mul(pos1, worldViewProjection);
    	Out.Color = Color;
    	return Out;
}
float4 PS( VS_OUTPUT vsout ) : COLOR
{    return vsout.Color;	
}
technique RenderScene
{    pass P0
    {   	CullMode=None;							//È¡Ïû±³ÃæÌÞ³ý
    		vertexShader = compile vs_1_1 VS();
    		pixelShader=compile ps_1_1 PS();
    }
}
