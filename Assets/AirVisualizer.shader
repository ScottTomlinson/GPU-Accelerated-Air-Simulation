// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AirVisualizer" {
	Properties
	{
		_ColorLow("Color Slow Speed", Color) = (0, 0, 0.5, 0.1)
		_ColorHigh("Color High Speed", Color) = (1, 0, 0, 0.8)
		_HighSpeedValue("High speed Value", Range(0, 50)) = 50
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

	// Pixel shader input
	struct PS_INPUT
	{
		float4 position : SV_POSITION;
		float4 color : COLOR;
	};

	// Particle's data, shared with the compute shader
	StructuredBuffer<float> airBuffer;

	// Properties variables
	uniform float4 _ColorLow;
	uniform float4 _ColorHigh;
	uniform float _HighSpeedValue;

	// Vertex shader
	PS_INPUT vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
	{
		PS_INPUT o = (PS_INPUT)0;
		float3 _size = { 100,100,100 };

		// Color
		float value = length(airBuffer[vertex_id]);
		float lerpValue = clamp(value / _HighSpeedValue, 0.0f, 1.0f);
		o.color = lerp(_ColorLow, _ColorHigh, lerpValue);

		// Position
		//1D index to 3D array http://stackoverflow.com/questions/11316490/convert-a-1d-array-index-to-a-3d-array-index
		o.position = float4(vertex_id % _size.x, vertex_id / (_size.z * _size.x), (vertex_id / 100) % 100, 1);;

		return o;
	}

	//geometry shader
	[maxvertexcount(4)]
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
	float4 frag(PS_INPUT i) : COLOR
	{
		return i.color;
	}

		ENDCG
	}
	}

		Fallback Off
}
