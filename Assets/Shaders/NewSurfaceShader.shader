// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "NewSurfaceShader"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_DefaultMaterial_Base_Color("DefaultMaterial_Base_Color", 2D) = "white" {}
		_DefaultMaterial_Metallic("DefaultMaterial_Metallic", 2D) = "white" {}
		_DefaultMaterial_Normal_DirectX("DefaultMaterial_Normal_DirectX", 2D) = "bump" {}
		_DefaultMaterial_Roughness("DefaultMaterial_Roughness", 2D) = "white" {}
		_DefaultMaterial_Mixed_AO("DefaultMaterial_Mixed_AO", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _DefaultMaterial_Normal_DirectX;
		uniform float4 _DefaultMaterial_Normal_DirectX_ST;
		uniform sampler2D _DefaultMaterial_Base_Color;
		uniform float4 _DefaultMaterial_Base_Color_ST;
		uniform sampler2D _DefaultMaterial_Metallic;
		uniform float4 _DefaultMaterial_Metallic_ST;
		uniform sampler2D _DefaultMaterial_Roughness;
		uniform float4 _DefaultMaterial_Roughness_ST;
		uniform sampler2D _DefaultMaterial_Mixed_AO;
		uniform float4 _DefaultMaterial_Mixed_AO_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_DefaultMaterial_Normal_DirectX = i.uv_texcoord * _DefaultMaterial_Normal_DirectX_ST.xy + _DefaultMaterial_Normal_DirectX_ST.zw;
			o.Normal = UnpackNormal( tex2D( _DefaultMaterial_Normal_DirectX, uv_DefaultMaterial_Normal_DirectX ) );
			float2 uv_DefaultMaterial_Base_Color = i.uv_texcoord * _DefaultMaterial_Base_Color_ST.xy + _DefaultMaterial_Base_Color_ST.zw;
			o.Albedo = tex2D( _DefaultMaterial_Base_Color, uv_DefaultMaterial_Base_Color ).xyz;
			float2 uv_DefaultMaterial_Metallic = i.uv_texcoord * _DefaultMaterial_Metallic_ST.xy + _DefaultMaterial_Metallic_ST.zw;
			o.Metallic = tex2D( _DefaultMaterial_Metallic, uv_DefaultMaterial_Metallic ).x;
			float2 uv_DefaultMaterial_Roughness = i.uv_texcoord * _DefaultMaterial_Roughness_ST.xy + _DefaultMaterial_Roughness_ST.zw;
			o.Smoothness = tex2D( _DefaultMaterial_Roughness, uv_DefaultMaterial_Roughness ).x;
			float2 uv_DefaultMaterial_Mixed_AO = i.uv_texcoord * _DefaultMaterial_Mixed_AO_ST.xy + _DefaultMaterial_Mixed_AO_ST.zw;
			o.Occlusion = tex2D( _DefaultMaterial_Mixed_AO, uv_DefaultMaterial_Mixed_AO ).x;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13101
1927;29;1906;1124;192;30;1;True;True
Node;AmplifyShaderEditor.SamplerNode;4;165,672;Float;True;Property;_DefaultMaterial_Roughness;DefaultMaterial_Roughness;3;0;Assets/Materials/DefaultMaterial_Roughness.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;5;168,859;Float;True;Property;_DefaultMaterial_Mixed_AO;DefaultMaterial_Mixed_AO;4;0;Assets/Materials/DefaultMaterial_Mixed_AO.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;3;169,266;Float;True;Property;_DefaultMaterial_Normal_DirectX;DefaultMaterial_Normal_DirectX;2;0;Assets/Materials/DefaultMaterial_Normal_DirectX.tga;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;1;169,82;Float;True;Property;_DefaultMaterial_Base_Color;DefaultMaterial_Base_Color;0;0;Assets/Materials/DefaultMaterial_Base_Color.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SamplerNode;2;165,491;Float;True;Property;_DefaultMaterial_Metallic;DefaultMaterial_Metallic;1;0;Assets/Materials/DefaultMaterial_Metallic.tga;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;726,88;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;NewSurfaceShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;0;0;1;0
WireConnection;0;1;3;0
WireConnection;0;3;2;0
WireConnection;0;4;4;0
WireConnection;0;5;5;0
ASEEND*/
//CHKSM=FF13E0780DC7F28094596055D82C4EE3062A5EB9