// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
#pragma fragment frag

#include "UnityCG.cginc"

		// Particle's data
	struct Particle
	{
		float3 position;
		float3 velocity;
	};

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
		int xLength = 100;
		int yLength = 100;
		int zLength = 100;

		//x then z then y
		int xPos = vertex_id % _size.x;
		int zPos = (vertex_id / _size.x) % _size.z;
		int yPos = vertex_id / (_size.z * _size.y);

		// Color
		float value = length(airBuffer[vertex_id]);
		float lerpValue = clamp(value / _HighSpeedValue, 0.0f, 1.0f);
		o.color = lerp(_ColorLow, _ColorHigh, lerpValue);

		// Position
		float3 pos = { xPos, yPos, zPos };

		o.position = UnityObjectToClipPos(pos);

		return o;
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
