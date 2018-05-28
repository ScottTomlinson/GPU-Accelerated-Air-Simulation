Shader "Custom/AirVisualizer" {
	Properties
	{
		_ColorLow("Color Slow Speed", Color) = (0, 0, 0.5, 0.1)
		_ColorHigh("Color High Speed", Color) = (1, 0, 0, 0.8)
		_Texture("Base (RGB)", 2D) = "blue" {}
		_HighPressureValue("High Pressure Value", Range(0, 50)) = 50
	}

		SubShader
	{
		Pass
	{
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma target 5.0
#pragma vertex vert
#pragma geometry geom
#pragma fragment frag

#include "UnityCG.cginc"

int height;
int width;
int depth;

	// Pixel shader input
	struct PS_INPUT
	{
		float4 position : SV_POSITION;
		float4 color : COLOR;
		
	};

	// node's data, shared with the compute shader
	StructuredBuffer<float> airVisBuffer;

	// Properties variables
	uniform float4 _ColorLow;
	uniform float4 _ColorHigh;
	uniform float _HighPressureValue;
	uniform sampler2D _Texture;	

	// Vertex shader
	PS_INPUT vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
	{
		PS_INPUT o = (PS_INPUT)0;
		float3 _size = { depth, width, height };

		// Color
		float value = length(airVisBuffer[instance_id]);
		float lerpValue = clamp(value / _HighPressureValue, 0.0f, 1.0f);
		o.color.r = lerpValue;
		
		
		o.color.a = lerp(_ColorLow.a, _ColorHigh.a, lerpValue);

		// Position
		//1D index to 3D array http://stackoverflow.com/questions/11316490/convert-a-1d-array-index-to-a-3d-array-index
		o.position = float4(instance_id % _size.x, instance_id / (_size.z * _size.x), (instance_id / width) % depth, 1);

		return o;
	}

	//geometry shader
	[maxvertexcount(6)]
	void geom(point PS_INPUT input[1], inout TriangleStream<PS_INPUT> outputStream) {
		PS_INPUT output;
		float4 position = input[0].position;
		for (int x = 0; x < 2; x++) {
			for (int y = 0; y < 2; y++) {
				float3 _position = { (float)x - 0.5, 0, (float)y - 0.5 };
				output.position = position + float4((float)x, 0, (float)y, 0);
				output.position = mul(UNITY_MATRIX_VP, output.position);
				output.color = input[0].color;
				
				outputStream.Append(output);
			}
		}
		outputStream.RestartStrip();
	}

	// Pixel shader
	float4 frag(PS_INPUT i, uint instance_id : SV_InstanceID) : COLOR
	{
		
		
		
		float4 col = tex2D(_Texture, float2(i.color.r,i.color.r)).rgba;
		col.a = i.color.a;
		return col;
	}

		ENDCG
	}

	}

		Fallback Off
}
