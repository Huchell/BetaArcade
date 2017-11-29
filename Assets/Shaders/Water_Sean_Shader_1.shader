// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water_Sean_Shader_T"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Translucency)]
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_DepthFqade0("Depth Fqade 0", Range( 0 , 1)) = 1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_Color1("Color 1", Color) = (1,1,1,0)
		_Color3("Color 3", Color) = (1,1,1,0)
		_Color2("Color 2", Color) = (0,0.4212982,0.9117647,0)
		_Float7("Float 7", Range( 0 , 36)) = 0
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_Float3("Float 3", Range( 0 , 1)) = 0.3058824
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TextureSample1("Texture Sample 1", 2D) = "white" {}
		_SpeedofEachNormal("Speed of Each Normal", Vector) = (0.3,-0.2,0,0)
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
		_Float8("Float 8", Float) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.6
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 screenPos;
		};

		struct SurfaceOutputStandardCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Translucency;
		};

		uniform sampler2D _TextureSample1;
		uniform float2 _SpeedofEachNormal;
		uniform sampler2D _TextureSample0;
		uniform float4 _Color2;
		uniform float4 _Color3;
		uniform float _Float3;
		uniform float _Float7;
		uniform float4 _Color1;
		uniform sampler2D _CameraDepthTexture;
		uniform float _DepthFqade0;
		uniform float _Float8;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _MaskClipValue = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 appendResult37 = (float2(( _Time.y * _SpeedofEachNormal.x ) , ( _Time.y * _SpeedofEachNormal.y )));
			float2 temp_output_38_0 = ( v.texcoord.xy + appendResult37 );
			float4 tex2DNode41 = tex2Dlod( _TextureSample1, float4( temp_output_38_0, 0.0 , 0.0 ) );
			float2 appendResult46 = (float2(tex2DNode41.r , tex2DNode41.g));
			float2 appendResult48 = (float2(( appendResult46 * 0.25 )));
			float4 tex2DNode2 = tex2Dlod( _TextureSample0, float4( temp_output_38_0, 0.0 , 0.0 ) );
			float2 appendResult42 = (float2(tex2DNode2.g , tex2DNode2.b));
			float2 appendResult45 = (float2(( appendResult42 * 0.5 )));
			float2 temp_output_50_0 = ( appendResult48 + appendResult45 );
			v.vertex.xyz += float3( temp_output_50_0 ,  0.0 );
		}

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			UNITY_GI(gi, s, data);
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float2 appendResult37 = (float2(( _Time.y * _SpeedofEachNormal.x ) , ( _Time.y * _SpeedofEachNormal.y )));
			float2 temp_output_38_0 = ( i.uv_texcoord + appendResult37 );
			float4 tex2DNode41 = tex2D( _TextureSample1, temp_output_38_0 );
			float2 appendResult46 = (float2(tex2DNode41.r , tex2DNode41.g));
			float2 appendResult48 = (float2(( appendResult46 * 0.25 )));
			float4 tex2DNode2 = tex2D( _TextureSample0, temp_output_38_0 );
			float2 appendResult42 = (float2(tex2DNode2.g , tex2DNode2.b));
			float2 appendResult45 = (float2(( appendResult42 * 0.5 )));
			float2 temp_output_50_0 = ( appendResult48 + appendResult45 );
			o.Normal = float3( temp_output_50_0 ,  0.0 );
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float fresnelFinalVal21 = (0.0 + _Float3*pow( 1.0 - dot( WorldNormalVector( i , appendResult48 ), worldViewDir ) , _Float7));
			float4 lerpResult12 = lerp( _Color2 , _Color3 , fresnelFinalVal21);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float screenDepth6 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(ase_screenPos))));
			float distanceDepth6 = abs( ( screenDepth6 - LinearEyeDepth( ase_screenPos.z/ ase_screenPos.w ) ) / ( _DepthFqade0 ) );
			float4 lerpResult10 = lerp( lerpResult12 , _Color1 , ( 1.0 - saturate( distanceDepth6 ) ));
			o.Albedo = lerpResult10.rgb;
			float3 temp_cast_2 = (_Float8).xxx;
			o.Emission = temp_cast_2;
			float3 temp_cast_3 = (0.5).xxx;
			o.Translucency = temp_cast_3;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD6;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				fixed3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				fixed3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord.xy = IN.texcoords01.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=13101
1927;29;1906;1124;3043.804;886.6536;1.627126;True;True
Node;AmplifyShaderEditor.SimpleTimeNode;31;-3439.732,-224.9968;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.Vector2Node;35;-3429.707,41.26804;Float;False;Property;_SpeedofEachNormal;Speed of Each Normal;4;0;0.3,-0.2;0;3;FLOAT2;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-3081.31,-223.5758;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-3085.283,68.82335;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;37;-2764.782,57.32261;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.TexCoordVertexDataNode;39;-2767.18,-230.5775;Float;False;0;2;0;5;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-2470.581,-14.77724;Float;False;2;2;0;FLOAT2;0.0,0,0,0;False;1;FLOAT2;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.SamplerNode;41;-2154.353,-178.6758;Float;True;Property;_TextureSample1;Texture Sample 1;4;0;Assets/723-normal.jpg;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;46;-1755.27,-187.1271;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;52;-1608.393,-439.6806;Float;False;Constant;_Float2;Float 2;4;0;0.25;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-1431.504,-199.3673;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT;0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.DynamicAppendNode;48;-1146.619,-199.3673;Float;False;FLOAT2;4;0;FLOAT2;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;7;-416.3517,-1024.742;Float;False;Property;_DepthFqade0;Depth Fqade 0;2;0;1;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.SamplerNode;2;-2153.589,52.15275;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Assets/723-normal.jpg;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;53;-696.9079,-1275.432;Float;False;Property;_Float3;Float 3;3;0;0.3058824;0;1;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;54;-720.3567,-1158.092;Float;False;Property;_Float7;Float 7;3;0;0;0;36;0;1;FLOAT
Node;AmplifyShaderEditor.DepthFade;6;-117.6713,-1021.868;Float;False;1;0;FLOAT;1.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;44;-1753.468,295.2971;Float;False;Constant;_Float6;Float 6;5;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.WorldNormalVector;49;-1044.609,-1426.473;Float;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DynamicAppendNode;42;-1773.716,49.52238;Float;False;FLOAT2;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.ColorNode;13;-259.1147,-1909.874;Float;False;Property;_Color2;Color 2;2;0;0,0.4212982,0.9117647,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SaturateNode;8;96.775,-1036.868;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.ColorNode;14;-253.9308,-1681.899;Float;False;Property;_Color3;Color 3;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.FresnelNode;21;-289.2021,-1356.226;Float;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1439.835,55.25773;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT;0.0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.DynamicAppendNode;45;-1147.089,52.26652;Float;False;FLOAT2;4;0;FLOAT2;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;3;FLOAT;0.0;False;1;FLOAT2
Node;AmplifyShaderEditor.ColorNode;11;643.9935,-1675.813;Float;False;Property;_Color1;Color 1;2;0;1,1,1,0;0;5;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.LerpOp;12;544.2305,-1396.248;Float;False;3;0;COLOR;0.0;False;1;COLOR;0.0,0,0,0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.OneMinusNode;9;303.345,-1046.868;Float;False;1;0;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;707.2922,-982.2957;Float;False;2;2;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;50;-857.9922,-147.4561;Float;False;2;2;0;FLOAT2;0.0;False;1;FLOAT2;0.0,0,0,0;False;1;FLOAT2
Node;AmplifyShaderEditor.RangedFloatNode;28;-444.5178,-586.9811;Float;True;Constant;_Float4;Float 4;4;0;5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.FresnelNode;26;-248.3337,-345.2537;Float;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;3;FLOAT;5.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;18;908.7003,-1124.703;Float;False;Constant;_Float0;Float 0;3;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;10;1093.87,-1346.604;Float;True;3;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;29;-446.0071,-819.3322;Float;False;Constant;_Float5;Float 5;4;0;1;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;56;1145.637,-351.5336;Float;False;Property;_Float8;Float 8;7;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.LerpOp;16;1130.289,-960.5768;Float;False;3;0;FLOAT;0,0,0,0;False;1;COLOR;0.0;False;2;FLOAT;0.0;False;1;COLOR
Node;AmplifyShaderEditor.LerpOp;27;86.24509,-539.7309;Float;False;3;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;0.0;False;1;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;20;476.0911,-525.2535;Float;False;Property;_Float1;Float 1;3;0;41;0;41;0;1;FLOAT
Node;AmplifyShaderEditor.SimpleContrastOpNode;19;706.204,-685.8112;Float;False;2;0;FLOAT;0.0;False;1;COLOR;0,0,0,0;False;1;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;57;1174.637,-206.5336;Float;False;Constant;_Float9;Float 9;8;0;0.5;0;0;0;1;FLOAT
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1469.069,-280.2843;Float;False;True;6;Float;ASEMaterialInspector;0;0;Standard;Water_Sean_Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Custom;0.5;True;True;0;True;Transparent;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;1;1;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;32;0;31;0
WireConnection;32;1;35;1
WireConnection;36;0;31;0
WireConnection;36;1;35;2
WireConnection;37;0;32;0
WireConnection;37;1;36;0
WireConnection;38;0;39;0
WireConnection;38;1;37;0
WireConnection;41;1;38;0
WireConnection;46;0;41;1
WireConnection;46;1;41;2
WireConnection;47;0;46;0
WireConnection;47;1;52;0
WireConnection;48;0;47;0
WireConnection;2;1;38;0
WireConnection;6;0;7;0
WireConnection;49;0;48;0
WireConnection;42;0;2;2
WireConnection;42;1;2;3
WireConnection;8;0;6;0
WireConnection;21;0;49;0
WireConnection;21;2;53;0
WireConnection;21;3;54;0
WireConnection;43;0;42;0
WireConnection;43;1;44;0
WireConnection;45;0;43;0
WireConnection;12;0;13;0
WireConnection;12;1;14;0
WireConnection;12;2;21;0
WireConnection;9;0;8;0
WireConnection;15;0;12;0
WireConnection;15;1;27;0
WireConnection;50;0;48;0
WireConnection;50;1;45;0
WireConnection;26;0;50;0
WireConnection;10;0;12;0
WireConnection;10;1;11;0
WireConnection;10;2;9;0
WireConnection;16;0;18;0
WireConnection;16;1;15;0
WireConnection;16;2;19;0
WireConnection;27;0;29;0
WireConnection;27;1;28;0
WireConnection;27;2;26;0
WireConnection;19;0;6;0
WireConnection;19;1;20;0
WireConnection;0;0;10;0
WireConnection;0;1;50;0
WireConnection;0;2;56;0
WireConnection;0;7;57;0
WireConnection;0;11;50;0
ASEEND*/
//CHKSM=D80F2AA8612F1AC01C65E53513DA5EDD428C0978