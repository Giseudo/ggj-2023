// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MudBun/Stopmotion Mesh (URP)"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector]_Color("Color", Color) = (1,1,1,1)
		[HideInInspector]_Emission("Emission", Color) = (1,1,1,1)
		[HideInInspector]_Metallic("Metallic", Range( 0 , 1)) = 0
		[HideInInspector]_Smoothness("Smoothness", Range( 0 , 1)) = 1
		[HideInInspector]_IsMeshRenderMaterial("Is Mesh Render Material", Float) = 1
		[ASEBegin]_AlphaCutoutThreshold("Alpha Cutout Threshold", Range( 0 , 1)) = 0
		_Dithering("Dithering", Range( 0 , 1)) = 1
		_NoiseSize("Noise Size", Float) = 0.5
		_OffsetAmount("Offset Amount", Float) = 0.005
		_TimeInterval("Time Interval", Float) = 0.15
		[NoScaleOffset]_DisplacementMap("Displacement Map", 2D) = "bump" {}
		_Displacement("Displacement", Float) = 0
		[Normal]_NormalMap("Normal Map", 2D) = "white" {}
		[Toggle]_RandomDither("Random Dither", Range( 0 , 1)) = 0
		[NoScaleOffset]_RoughnessMap("Roughness Map", 2D) = "black" {}
		[NoScaleOffset]_DitherTexture("Dither Texture", 2D) = "white" {}
		[ASEEnd]_DitherTextureSize("Dither Texture Size", Int) = 256

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Back
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 4.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				uint ase_vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
				float4 ase_texcoord9 : TEXCOORD9;
				float4 ase_texcoord10 : TEXCOORD10;
				float4 ase_texcoord11 : TEXCOORD11;
				float4 ase_texcoord12 : TEXCOORD12;
				float4 ase_texcoord13 : TEXCOORD13;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _DisplacementMap;
			sampler2D _NormalMap;
			sampler2D _RoughnessMap;
			sampler2D _DitherTexture;


			float3 SimplexNoiseGradient6_g359( float3 Position, float Size )
			{
				#ifdef MUDBUN_VALID
				return snoise_grad(Position / max(1e-6, Size)).xyz;
				#else
				return Position;
				#endif
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = v.ase_vertexID;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_35_2 = NormalWs4_g262;
				float3 vertexToFrag94 = temp_output_35_2;
				float3 temp_output_44_0_g357 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g357 = temp_output_44_0_g357;
				float3 temp_output_35_32 = PositionLs4_g262;
				float3 vertexToFrag95 = temp_output_35_32;
				float3 temp_output_36_0_g357 = vertexToFrag95;
				float4 appendResult23_g357 = (float4(temp_output_44_0_g357 , 0.0));
				float4 appendResult24_g357 = (float4(temp_output_44_0_g357 , 1.0));
				float4 break10_g358 = ( ( break14_g357.x + break14_g357.y + break14_g357.z ) > 0.0 ? appendResult23_g357 : appendResult24_g357 );
				float4 color20_g357 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 temp_output_35_0 = PositionWs4_g262;
				float2 temp_cast_4 = (floor( ( _TimeParameters.x / _TimeInterval ) )).xx;
				float dotResult4_g356 = dot( temp_cast_4 , float2( 12.9898,78.233 ) );
				float lerpResult10_g356 = lerp( 0.0 , 10000.0 , frac( ( sin( dotResult4_g356 ) * 43758.55 ) ));
				float3 Position6_g359 = ( temp_output_35_32 + lerpResult10_g356 );
				float Size6_g359 = _NoiseSize;
				float3 localSimplexNoiseGradient6_g359 = SimplexNoiseGradient6_g359( Position6_g359 , Size6_g359 );
				float3 temp_output_122_0 = ( ( _Displacement * ( (( ( ( ( break14_g357.x > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).yz * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.x ) + ( ( break14_g357.y > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).zx * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.y ) + ( ( break14_g357.z > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).xy * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.z ) + ( color20_g357 * break10_g358.w ) ) / ( break10_g358.x + break10_g358.y + break10_g358.z + break10_g358.w ) )).x - 0.5 ) * NormalLs4_g262 ) + ( temp_output_35_0 + ( localSimplexNoiseGradient6_g359 * _OffsetAmount ) ) );
				
				float4 vertexToFrag5_g262 = Color4_g262;
				o.ase_texcoord7 = vertexToFrag5_g262;
				
				float3 vertexToFrag66 = TangentWs4_g262;
				o.ase_texcoord8.xyz = vertexToFrag66;
				o.ase_texcoord9.xyz = vertexToFrag94;
				o.ase_texcoord10.xyz = vertexToFrag95;
				
				float3 vertexToFrag6_g262 = (EmissionHash4_g262).xyz;
				o.ase_texcoord11.xyz = vertexToFrag6_g262;
				
				float vertexToFrag8_g262 = Metallic4_g262;
				o.ase_texcoord8.w = vertexToFrag8_g262;
				
				float vertexToFrag7_g262 = Smoothness4_g262;
				o.ase_texcoord9.w = vertexToFrag7_g262;
				
				float3 finalVertexPositionWs124 = temp_output_122_0;
				float3 vertexToFrag126 = finalVertexPositionWs124;
				o.ase_texcoord12.xyz = vertexToFrag126;
				float3 vertexToFrag27_g382 = temp_output_35_0;
				o.ase_texcoord13.xyz = vertexToFrag27_g382;
				
				o.ase_texcoord10.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord11.w = 0;
				o.ase_texcoord12.w = 0;
				o.ase_texcoord13.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_122_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = temp_output_35_2;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				uint ase_vertexID : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				o.vertex = v.vertex;
				o.ase_vertexID = v.ase_vertexID;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_vertexID = patch[0].ase_vertexID * bary.x + patch[1].ase_vertexID * bary.y + patch[2].ase_vertexID * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float4 vertexToFrag5_g262 = IN.ase_texcoord7;
				float4 temp_output_25_0_g262 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g262 );
				
				float3 vertexToFrag66 = IN.ase_texcoord8.xyz;
				float3 temp_output_10_0_g377 = vertexToFrag66;
				float3 vertexToFrag94 = IN.ase_texcoord9.xyz;
				float3 temp_output_9_0_g377 = vertexToFrag94;
				float3 temp_output_44_0_g380 = ( float3( 1,1,1 ) * abs( vertexToFrag94 ) );
				float3 break14_g380 = temp_output_44_0_g380;
				float3 vertexToFrag95 = IN.ase_texcoord10.xyz;
				float3 temp_output_36_0_g380 = vertexToFrag95;
				float4 appendResult23_g380 = (float4(temp_output_44_0_g380 , 0.0));
				float4 appendResult24_g380 = (float4(temp_output_44_0_g380 , 1.0));
				float4 break10_g381 = ( ( break14_g380.x + break14_g380.y + break14_g380.z ) > 0.0 ? appendResult23_g380 : appendResult24_g380 );
				float4 color20_g380 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 normalizeResult6_g377 = normalize( ( ( cross( temp_output_10_0_g377 , temp_output_9_0_g377 ) * UnpackNormalScale( ( ( ( ( break14_g380.x > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).yz * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.x ) + ( ( break14_g380.y > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).zx * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.y ) + ( ( break14_g380.z > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).xy * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.z ) + ( color20_g380 * break10_g381.w ) ) / ( break10_g381.x + break10_g381.y + break10_g381.z + break10_g381.w ) ), 1.0 ).x ) + ( temp_output_10_0_g377 * UnpackNormalScale( ( ( ( ( break14_g380.x > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).yz * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.x ) + ( ( break14_g380.y > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).zx * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.y ) + ( ( break14_g380.z > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).xy * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.z ) + ( color20_g380 * break10_g381.w ) ) / ( break10_g381.x + break10_g381.y + break10_g381.z + break10_g381.w ) ), 1.0 ).y ) + ( temp_output_9_0_g377 * UnpackNormalScale( ( ( ( ( break14_g380.x > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).yz * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.x ) + ( ( break14_g380.y > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).zx * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.y ) + ( ( break14_g380.z > 0.0 ? tex2D( _NormalMap, ( ( (temp_output_36_0_g380).xy * _NormalMap_ST.xy ) + _NormalMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g381.z ) + ( color20_g380 * break10_g381.w ) ) / ( break10_g381.x + break10_g381.y + break10_g381.z + break10_g381.w ) ), 1.0 ).z ) ) );
				
				float3 vertexToFrag6_g262 = IN.ase_texcoord11.xyz;
				
				float vertexToFrag8_g262 = IN.ase_texcoord8.w;
				
				float3 temp_output_44_0_g378 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g378 = temp_output_44_0_g378;
				float3 temp_output_36_0_g378 = vertexToFrag95;
				float4 appendResult23_g378 = (float4(temp_output_44_0_g378 , 0.0));
				float4 appendResult24_g378 = (float4(temp_output_44_0_g378 , 1.0));
				float4 break10_g379 = ( ( break14_g378.x + break14_g378.y + break14_g378.z ) > 0.0 ? appendResult23_g378 : appendResult24_g378 );
				float4 color20_g378 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float vertexToFrag7_g262 = IN.ase_texcoord9.w;
				
				float localComputeOpaqueTransparency20_g382 = ( 0.0 );
				float3 vertexToFrag126 = IN.ase_texcoord12.xyz;
				float4 unityObjectToClipPos1_g362 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag126));
				float4 computeScreenPos3_g362 = ComputeScreenPos( unityObjectToClipPos1_g362 );
				float2 ScreenPos20_g382 = (( ( computeScreenPos3_g362 / (computeScreenPos3_g362).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g382 = IN.ase_texcoord13.xyz;
				float3 VertPos20_g382 = vertexToFrag27_g382;
				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = IN.ase_texcoord10.w;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g382 = (EmissionHash4_g262).w;
				float AlphaIn20_g382 = (temp_output_25_0_g262).a;
				float AlphaOut20_g382 = 0;
				float AlphaThreshold20_g382 = 0;
				sampler2D DitherNoiseTexture20_g382 = _DitherTexture;
				int DitherNoiseTextureSize20_g382 = _DitherTextureSize;
				int UseRandomDither20_g382 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g382 = _AlphaCutoutThreshold;
				float DitherBlend20_g382 = _Dithering;
				{
				float alpha = AlphaIn20_g382;
				computeOpaqueTransparency(ScreenPos20_g382, VertPos20_g382, Hash20_g382, DitherNoiseTexture20_g382, DitherNoiseTextureSize20_g382, UseRandomDither20_g382 > 0, AlphaCutoutThreshold20_g382, DitherBlend20_g382,  alpha, AlphaThreshold20_g382);
				AlphaOut20_g382 = alpha;
				}
				
				float3 Albedo = temp_output_25_0_g262.rgb;
				float3 Normal = normalizeResult6_g377;
				float3 Emission = ( vertexToFrag6_g262 * (_Emission).rgb );
				float3 Specular = 0.5;
				float Metallic = ( _Metallic * vertexToFrag8_g262 );
				float Smoothness = ( ( 1.0 - (( ( ( ( break14_g378.x > 0.0 ? tex2D( _RoughnessMap, ( ( (temp_output_36_0_g378).yz * _RoughnessMap_ST.xy ) + _RoughnessMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g379.x ) + ( ( break14_g378.y > 0.0 ? tex2D( _RoughnessMap, ( ( (temp_output_36_0_g378).zx * _RoughnessMap_ST.xy ) + _RoughnessMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g379.y ) + ( ( break14_g378.z > 0.0 ? tex2D( _RoughnessMap, ( ( (temp_output_36_0_g378).xy * _RoughnessMap_ST.xy ) + _RoughnessMap_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g379.z ) + ( color20_g378 * break10_g379.w ) ) / ( break10_g379.x + break10_g379.y + break10_g379.z + break10_g379.w ) )).x ) * ( _Smoothness * vertexToFrag7_g262 ) );
				float Occlusion = 1;
				float Alpha = AlphaOut20_g382;
				float AlphaClipThreshold = AlphaThreshold20_g382;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma vertex vert
			#pragma fragment frag
#if ASE_SRP_VERSION >= 110000
			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
#endif
			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _DisplacementMap;
			sampler2D _DitherTexture;


			float3 SimplexNoiseGradient6_g359( float3 Position, float Size )
			{
				#ifdef MUDBUN_VALID
				return snoise_grad(Position / max(1e-6, Size)).xyz;
				#else
				return Position;
				#endif
			}
			

			float3 _LightDirection;
#if ASE_SRP_VERSION >= 110000 
			float3 _LightPosition;
#endif
			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = v.ase_vertexID;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_35_2 = NormalWs4_g262;
				float3 vertexToFrag94 = temp_output_35_2;
				float3 temp_output_44_0_g357 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g357 = temp_output_44_0_g357;
				float3 temp_output_35_32 = PositionLs4_g262;
				float3 vertexToFrag95 = temp_output_35_32;
				float3 temp_output_36_0_g357 = vertexToFrag95;
				float4 appendResult23_g357 = (float4(temp_output_44_0_g357 , 0.0));
				float4 appendResult24_g357 = (float4(temp_output_44_0_g357 , 1.0));
				float4 break10_g358 = ( ( break14_g357.x + break14_g357.y + break14_g357.z ) > 0.0 ? appendResult23_g357 : appendResult24_g357 );
				float4 color20_g357 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 temp_output_35_0 = PositionWs4_g262;
				float2 temp_cast_4 = (floor( ( _TimeParameters.x / _TimeInterval ) )).xx;
				float dotResult4_g356 = dot( temp_cast_4 , float2( 12.9898,78.233 ) );
				float lerpResult10_g356 = lerp( 0.0 , 10000.0 , frac( ( sin( dotResult4_g356 ) * 43758.55 ) ));
				float3 Position6_g359 = ( temp_output_35_32 + lerpResult10_g356 );
				float Size6_g359 = _NoiseSize;
				float3 localSimplexNoiseGradient6_g359 = SimplexNoiseGradient6_g359( Position6_g359 , Size6_g359 );
				float3 temp_output_122_0 = ( ( _Displacement * ( (( ( ( ( break14_g357.x > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).yz * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.x ) + ( ( break14_g357.y > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).zx * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.y ) + ( ( break14_g357.z > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).xy * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.z ) + ( color20_g357 * break10_g358.w ) ) / ( break10_g358.x + break10_g358.y + break10_g358.z + break10_g358.w ) )).x - 0.5 ) * NormalLs4_g262 ) + ( temp_output_35_0 + ( localSimplexNoiseGradient6_g359 * _OffsetAmount ) ) );
				
				float3 finalVertexPositionWs124 = temp_output_122_0;
				float3 vertexToFrag126 = finalVertexPositionWs124;
				o.ase_texcoord2.xyz = vertexToFrag126;
				float3 vertexToFrag27_g382 = temp_output_35_0;
				o.ase_texcoord3.xyz = vertexToFrag27_g382;
				float4 vertexToFrag5_g262 = Color4_g262;
				o.ase_texcoord4 = vertexToFrag5_g262;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_122_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_35_2;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

		#if ASE_SRP_VERSION >= 110000 
			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif
		#else
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#endif
		#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.vertex = v.vertex;
				o.ase_vertexID = v.ase_vertexID;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_vertexID = patch[0].ase_vertexID * bary.x + patch[1].ase_vertexID * bary.y + patch[2].ase_vertexID * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float localComputeOpaqueTransparency20_g382 = ( 0.0 );
				float3 vertexToFrag126 = IN.ase_texcoord2.xyz;
				float4 unityObjectToClipPos1_g362 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag126));
				float4 computeScreenPos3_g362 = ComputeScreenPos( unityObjectToClipPos1_g362 );
				float2 ScreenPos20_g382 = (( ( computeScreenPos3_g362 / (computeScreenPos3_g362).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g382 = IN.ase_texcoord3.xyz;
				float3 VertPos20_g382 = vertexToFrag27_g382;
				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = IN.ase_texcoord2.w;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g382 = (EmissionHash4_g262).w;
				float4 vertexToFrag5_g262 = IN.ase_texcoord4;
				float4 temp_output_25_0_g262 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g262 );
				float AlphaIn20_g382 = (temp_output_25_0_g262).a;
				float AlphaOut20_g382 = 0;
				float AlphaThreshold20_g382 = 0;
				sampler2D DitherNoiseTexture20_g382 = _DitherTexture;
				int DitherNoiseTextureSize20_g382 = _DitherTextureSize;
				int UseRandomDither20_g382 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g382 = _AlphaCutoutThreshold;
				float DitherBlend20_g382 = _Dithering;
				{
				float alpha = AlphaIn20_g382;
				computeOpaqueTransparency(ScreenPos20_g382, VertPos20_g382, Hash20_g382, DitherNoiseTexture20_g382, DitherNoiseTextureSize20_g382, UseRandomDither20_g382 > 0, AlphaCutoutThreshold20_g382, DitherBlend20_g382,  alpha, AlphaThreshold20_g382);
				AlphaOut20_g382 = alpha;
				}
				
				float Alpha = AlphaOut20_g382;
				float AlphaClipThreshold = AlphaThreshold20_g382;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _DisplacementMap;
			sampler2D _DitherTexture;


			float3 SimplexNoiseGradient6_g359( float3 Position, float Size )
			{
				#ifdef MUDBUN_VALID
				return snoise_grad(Position / max(1e-6, Size)).xyz;
				#else
				return Position;
				#endif
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = v.ase_vertexID;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_35_2 = NormalWs4_g262;
				float3 vertexToFrag94 = temp_output_35_2;
				float3 temp_output_44_0_g357 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g357 = temp_output_44_0_g357;
				float3 temp_output_35_32 = PositionLs4_g262;
				float3 vertexToFrag95 = temp_output_35_32;
				float3 temp_output_36_0_g357 = vertexToFrag95;
				float4 appendResult23_g357 = (float4(temp_output_44_0_g357 , 0.0));
				float4 appendResult24_g357 = (float4(temp_output_44_0_g357 , 1.0));
				float4 break10_g358 = ( ( break14_g357.x + break14_g357.y + break14_g357.z ) > 0.0 ? appendResult23_g357 : appendResult24_g357 );
				float4 color20_g357 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 temp_output_35_0 = PositionWs4_g262;
				float2 temp_cast_4 = (floor( ( _TimeParameters.x / _TimeInterval ) )).xx;
				float dotResult4_g356 = dot( temp_cast_4 , float2( 12.9898,78.233 ) );
				float lerpResult10_g356 = lerp( 0.0 , 10000.0 , frac( ( sin( dotResult4_g356 ) * 43758.55 ) ));
				float3 Position6_g359 = ( temp_output_35_32 + lerpResult10_g356 );
				float Size6_g359 = _NoiseSize;
				float3 localSimplexNoiseGradient6_g359 = SimplexNoiseGradient6_g359( Position6_g359 , Size6_g359 );
				float3 temp_output_122_0 = ( ( _Displacement * ( (( ( ( ( break14_g357.x > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).yz * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.x ) + ( ( break14_g357.y > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).zx * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.y ) + ( ( break14_g357.z > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).xy * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.z ) + ( color20_g357 * break10_g358.w ) ) / ( break10_g358.x + break10_g358.y + break10_g358.z + break10_g358.w ) )).x - 0.5 ) * NormalLs4_g262 ) + ( temp_output_35_0 + ( localSimplexNoiseGradient6_g359 * _OffsetAmount ) ) );
				
				float3 finalVertexPositionWs124 = temp_output_122_0;
				float3 vertexToFrag126 = finalVertexPositionWs124;
				o.ase_texcoord2.xyz = vertexToFrag126;
				float3 vertexToFrag27_g382 = temp_output_35_0;
				o.ase_texcoord3.xyz = vertexToFrag27_g382;
				float4 vertexToFrag5_g262 = Color4_g262;
				o.ase_texcoord4 = vertexToFrag5_g262;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_122_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_35_2;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.vertex = v.vertex;
				o.ase_vertexID = v.ase_vertexID;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_vertexID = patch[0].ase_vertexID * bary.x + patch[1].ase_vertexID * bary.y + patch[2].ase_vertexID * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float localComputeOpaqueTransparency20_g382 = ( 0.0 );
				float3 vertexToFrag126 = IN.ase_texcoord2.xyz;
				float4 unityObjectToClipPos1_g362 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag126));
				float4 computeScreenPos3_g362 = ComputeScreenPos( unityObjectToClipPos1_g362 );
				float2 ScreenPos20_g382 = (( ( computeScreenPos3_g362 / (computeScreenPos3_g362).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g382 = IN.ase_texcoord3.xyz;
				float3 VertPos20_g382 = vertexToFrag27_g382;
				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = IN.ase_texcoord2.w;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g382 = (EmissionHash4_g262).w;
				float4 vertexToFrag5_g262 = IN.ase_texcoord4;
				float4 temp_output_25_0_g262 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g262 );
				float AlphaIn20_g382 = (temp_output_25_0_g262).a;
				float AlphaOut20_g382 = 0;
				float AlphaThreshold20_g382 = 0;
				sampler2D DitherNoiseTexture20_g382 = _DitherTexture;
				int DitherNoiseTextureSize20_g382 = _DitherTextureSize;
				int UseRandomDither20_g382 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g382 = _AlphaCutoutThreshold;
				float DitherBlend20_g382 = _Dithering;
				{
				float alpha = AlphaIn20_g382;
				computeOpaqueTransparency(ScreenPos20_g382, VertPos20_g382, Hash20_g382, DitherNoiseTexture20_g382, DitherNoiseTextureSize20_g382, UseRandomDither20_g382 > 0, AlphaCutoutThreshold20_g382, DitherBlend20_g382,  alpha, AlphaThreshold20_g382);
				AlphaOut20_g382 = alpha;
				}
				
				float Alpha = AlphaOut20_g382;
				float AlphaClipThreshold = AlphaThreshold20_g382;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				uint ase_vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _DisplacementMap;
			sampler2D _DitherTexture;


			float3 SimplexNoiseGradient6_g359( float3 Position, float Size )
			{
				#ifdef MUDBUN_VALID
				return snoise_grad(Position / max(1e-6, Size)).xyz;
				#else
				return Position;
				#endif
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = v.ase_vertexID;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_35_2 = NormalWs4_g262;
				float3 vertexToFrag94 = temp_output_35_2;
				float3 temp_output_44_0_g357 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g357 = temp_output_44_0_g357;
				float3 temp_output_35_32 = PositionLs4_g262;
				float3 vertexToFrag95 = temp_output_35_32;
				float3 temp_output_36_0_g357 = vertexToFrag95;
				float4 appendResult23_g357 = (float4(temp_output_44_0_g357 , 0.0));
				float4 appendResult24_g357 = (float4(temp_output_44_0_g357 , 1.0));
				float4 break10_g358 = ( ( break14_g357.x + break14_g357.y + break14_g357.z ) > 0.0 ? appendResult23_g357 : appendResult24_g357 );
				float4 color20_g357 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 temp_output_35_0 = PositionWs4_g262;
				float2 temp_cast_4 = (floor( ( _TimeParameters.x / _TimeInterval ) )).xx;
				float dotResult4_g356 = dot( temp_cast_4 , float2( 12.9898,78.233 ) );
				float lerpResult10_g356 = lerp( 0.0 , 10000.0 , frac( ( sin( dotResult4_g356 ) * 43758.55 ) ));
				float3 Position6_g359 = ( temp_output_35_32 + lerpResult10_g356 );
				float Size6_g359 = _NoiseSize;
				float3 localSimplexNoiseGradient6_g359 = SimplexNoiseGradient6_g359( Position6_g359 , Size6_g359 );
				float3 temp_output_122_0 = ( ( _Displacement * ( (( ( ( ( break14_g357.x > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).yz * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.x ) + ( ( break14_g357.y > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).zx * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.y ) + ( ( break14_g357.z > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).xy * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.z ) + ( color20_g357 * break10_g358.w ) ) / ( break10_g358.x + break10_g358.y + break10_g358.z + break10_g358.w ) )).x - 0.5 ) * NormalLs4_g262 ) + ( temp_output_35_0 + ( localSimplexNoiseGradient6_g359 * _OffsetAmount ) ) );
				
				float4 vertexToFrag5_g262 = Color4_g262;
				o.ase_texcoord2 = vertexToFrag5_g262;
				
				float3 vertexToFrag6_g262 = (EmissionHash4_g262).xyz;
				o.ase_texcoord3.xyz = vertexToFrag6_g262;
				
				float3 finalVertexPositionWs124 = temp_output_122_0;
				float3 vertexToFrag126 = finalVertexPositionWs124;
				o.ase_texcoord4.xyz = vertexToFrag126;
				float3 vertexToFrag27_g382 = temp_output_35_0;
				o.ase_texcoord5.xyz = vertexToFrag27_g382;
				
				o.ase_texcoord3.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_122_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_35_2;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				uint ase_vertexID : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.vertex = v.vertex;
				o.ase_vertexID = v.ase_vertexID;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_vertexID = patch[0].ase_vertexID * bary.x + patch[1].ase_vertexID * bary.y + patch[2].ase_vertexID * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 vertexToFrag5_g262 = IN.ase_texcoord2;
				float4 temp_output_25_0_g262 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g262 );
				
				float3 vertexToFrag6_g262 = IN.ase_texcoord3.xyz;
				
				float localComputeOpaqueTransparency20_g382 = ( 0.0 );
				float3 vertexToFrag126 = IN.ase_texcoord4.xyz;
				float4 unityObjectToClipPos1_g362 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag126));
				float4 computeScreenPos3_g362 = ComputeScreenPos( unityObjectToClipPos1_g362 );
				float2 ScreenPos20_g382 = (( ( computeScreenPos3_g362 / (computeScreenPos3_g362).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g382 = IN.ase_texcoord5.xyz;
				float3 VertPos20_g382 = vertexToFrag27_g382;
				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = IN.ase_texcoord3.w;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g382 = (EmissionHash4_g262).w;
				float AlphaIn20_g382 = (temp_output_25_0_g262).a;
				float AlphaOut20_g382 = 0;
				float AlphaThreshold20_g382 = 0;
				sampler2D DitherNoiseTexture20_g382 = _DitherTexture;
				int DitherNoiseTextureSize20_g382 = _DitherTextureSize;
				int UseRandomDither20_g382 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g382 = _AlphaCutoutThreshold;
				float DitherBlend20_g382 = _Dithering;
				{
				float alpha = AlphaIn20_g382;
				computeOpaqueTransparency(ScreenPos20_g382, VertPos20_g382, Hash20_g382, DitherNoiseTexture20_g382, DitherNoiseTextureSize20_g382, UseRandomDither20_g382 > 0, AlphaCutoutThreshold20_g382, DitherBlend20_g382,  alpha, AlphaThreshold20_g382);
				AlphaOut20_g382 = alpha;
				}
				
				
				float3 Albedo = temp_output_25_0_g262.rgb;
				float3 Emission = ( vertexToFrag6_g262 * (_Emission).rgb );
				float Alpha = AlphaOut20_g382;
				float AlphaClipThreshold = AlphaThreshold20_g382;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _DisplacementMap;
			sampler2D _DitherTexture;


			float3 SimplexNoiseGradient6_g359( float3 Position, float Size )
			{
				#ifdef MUDBUN_VALID
				return snoise_grad(Position / max(1e-6, Size)).xyz;
				#else
				return Position;
				#endif
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = v.ase_vertexID;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_35_2 = NormalWs4_g262;
				float3 vertexToFrag94 = temp_output_35_2;
				float3 temp_output_44_0_g357 = ( abs( vertexToFrag94 ) * abs( vertexToFrag94 ) );
				float3 break14_g357 = temp_output_44_0_g357;
				float3 temp_output_35_32 = PositionLs4_g262;
				float3 vertexToFrag95 = temp_output_35_32;
				float3 temp_output_36_0_g357 = vertexToFrag95;
				float4 appendResult23_g357 = (float4(temp_output_44_0_g357 , 0.0));
				float4 appendResult24_g357 = (float4(temp_output_44_0_g357 , 1.0));
				float4 break10_g358 = ( ( break14_g357.x + break14_g357.y + break14_g357.z ) > 0.0 ? appendResult23_g357 : appendResult24_g357 );
				float4 color20_g357 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 temp_output_35_0 = PositionWs4_g262;
				float2 temp_cast_4 = (floor( ( _TimeParameters.x / _TimeInterval ) )).xx;
				float dotResult4_g356 = dot( temp_cast_4 , float2( 12.9898,78.233 ) );
				float lerpResult10_g356 = lerp( 0.0 , 10000.0 , frac( ( sin( dotResult4_g356 ) * 43758.55 ) ));
				float3 Position6_g359 = ( temp_output_35_32 + lerpResult10_g356 );
				float Size6_g359 = _NoiseSize;
				float3 localSimplexNoiseGradient6_g359 = SimplexNoiseGradient6_g359( Position6_g359 , Size6_g359 );
				float3 temp_output_122_0 = ( ( _Displacement * ( (( ( ( ( break14_g357.x > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).yz * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.x ) + ( ( break14_g357.y > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).zx * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.y ) + ( ( break14_g357.z > 0.0 ? tex2Dlod( _DisplacementMap, float4( ( ( (temp_output_36_0_g357).xy * _DisplacementMap_ST.xy ) + _DisplacementMap_ST.zw ), 0, 0.0) ) : float4( 0,0,0,0 ) ) * break10_g358.z ) + ( color20_g357 * break10_g358.w ) ) / ( break10_g358.x + break10_g358.y + break10_g358.z + break10_g358.w ) )).x - 0.5 ) * NormalLs4_g262 ) + ( temp_output_35_0 + ( localSimplexNoiseGradient6_g359 * _OffsetAmount ) ) );
				
				float4 vertexToFrag5_g262 = Color4_g262;
				o.ase_texcoord2 = vertexToFrag5_g262;
				
				float3 finalVertexPositionWs124 = temp_output_122_0;
				float3 vertexToFrag126 = finalVertexPositionWs124;
				o.ase_texcoord3.xyz = vertexToFrag126;
				float3 vertexToFrag27_g382 = temp_output_35_0;
				o.ase_texcoord4.xyz = vertexToFrag27_g382;
				
				o.ase_texcoord3.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_122_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_35_2;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				uint ase_vertexID : SV_VertexID;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.vertex = v.vertex;
				o.ase_vertexID = v.ase_vertexID;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_vertexID = patch[0].ase_vertexID * bary.x + patch[1].ase_vertexID * bary.y + patch[2].ase_vertexID * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float4 vertexToFrag5_g262 = IN.ase_texcoord2;
				float4 temp_output_25_0_g262 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g262 );
				
				float localComputeOpaqueTransparency20_g382 = ( 0.0 );
				float3 vertexToFrag126 = IN.ase_texcoord3.xyz;
				float4 unityObjectToClipPos1_g362 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag126));
				float4 computeScreenPos3_g362 = ComputeScreenPos( unityObjectToClipPos1_g362 );
				float2 ScreenPos20_g382 = (( ( computeScreenPos3_g362 / (computeScreenPos3_g362).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g382 = IN.ase_texcoord4.xyz;
				float3 VertPos20_g382 = vertexToFrag27_g382;
				float localMudBunMeshPoint4_g262 = ( 0.0 );
				int VertexID4_g262 = IN.ase_texcoord3.w;
				float3 PositionWs4_g262 = float3( 0,0,0 );
				float3 PositionLs4_g262 = float3( 0,0,0 );
				float3 NormalWs4_g262 = float3( 0,0,0 );
				float3 NormalLs4_g262 = float3( 0,0,0 );
				float3 TangentWs4_g262 = float3( 0,0,0 );
				float3 TangentLs4_g262 = float3( 0,0,0 );
				float4 Color4_g262 = float4( 0,0,0,0 );
				float4 EmissionHash4_g262 = float4( 0,0,0,0 );
				float Metallic4_g262 = 0;
				float Smoothness4_g262 = 0;
				float4 TextureWeight4_g262 = float4( 1,0,0,0 );
				float SdfValue4_g262 = 0;
				float3 Outward2dNormalLs4_g262 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g262 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g262, positionWs, PositionLs4_g262, NormalWs4_g262, NormalLs4_g262, TangentWs4_g262, TangentLs4_g262, Color4_g262, EmissionHash4_g262, metallicSmoothness, TextureWeight4_g262, SdfValue4_g262, Outward2dNormalLs4_g262, Outward2dNormalWs4_g262);
				PositionWs4_g262 = positionWs.xyz;
				Metallic4_g262 = metallicSmoothness.x;
				Smoothness4_g262 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g262, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g382 = (EmissionHash4_g262).w;
				float AlphaIn20_g382 = (temp_output_25_0_g262).a;
				float AlphaOut20_g382 = 0;
				float AlphaThreshold20_g382 = 0;
				sampler2D DitherNoiseTexture20_g382 = _DitherTexture;
				int DitherNoiseTextureSize20_g382 = _DitherTextureSize;
				int UseRandomDither20_g382 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g382 = _AlphaCutoutThreshold;
				float DitherBlend20_g382 = _Dithering;
				{
				float alpha = AlphaIn20_g382;
				computeOpaqueTransparency(ScreenPos20_g382, VertPos20_g382, Hash20_g382, DitherNoiseTexture20_g382, DitherNoiseTextureSize20_g382, UseRandomDither20_g382 > 0, AlphaCutoutThreshold20_g382, DitherBlend20_g382,  alpha, AlphaThreshold20_g382);
				AlphaOut20_g382 = alpha;
				}
				
				
				float3 Albedo = temp_output_25_0_g262.rgb;
				float Alpha = AlphaOut20_g382;
				float AlphaClipThreshold = AlphaThreshold20_g382;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				
				return float4(PackNormalOctRectEncode(TransformWorldToViewDir(IN.worldNormal, true)), 0.0, 0.0);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _EMISSION
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 100801

			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _GBUFFER_NORMALS_OCT
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/MeshCommon.cginc"


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _DisplacementMap_ST;
			float4 _NormalMap_ST;
			float4 _RoughnessMap_ST;
			float _Displacement;
			float _TimeInterval;
			float _NoiseSize;
			float _OffsetAmount;
			float _IsMeshRenderMaterial;
			int _DitherTextureSize;
			float _RandomDither;
			float _AlphaCutoutThreshold;
			float _Dithering;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			FragmentOutput frag ( VertexOutput IN 
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				BRDFData brdfData;
				InitializeBRDFData( Albedo, Metallic, Specular, Smoothness, Alpha, brdfData);
				half4 color;
				color.rgb = GlobalIllumination( brdfData, inputData.bakedGI, Occlusion, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;
				
					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;
				
					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				
							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif
				
				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;
				
					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
				
					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;
				
					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				
							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif
				
				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal, 0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif
				
				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif
				
				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb);
			}

			ENDHLSL
		}
		
	}
	
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18935
396;313.6;1104;664.6;1449.832;343.6734;1.3;True;False
Node;AmplifyShaderEditor.RangedFloatNode;23;-1408,800;Inherit;False;Property;_TimeInterval;Time Interval;10;0;Create;True;0;0;0;False;0;False;0.15;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;126;-1152,896;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;125;-928,896;Inherit;False;World To Screen;-1;;362;50b3ac8846f702445a58bf980e772412;0;1;8;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1424,992;Inherit;True;Property;_DitherTexture;Dither Texture;16;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;f240bbb7854046345b218811e5681a54;f240bbb7854046345b218811e5681a54;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;6;-1424,1312;Inherit;False;Property;_RandomDither;Random Dither;14;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1424,1408;Inherit;False;Property;_AlphaCutoutThreshold;Alpha Cutout Threshold;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;8;-1424,1216;Inherit;False;Property;_DitherTextureSize;Dither Texture Size;17;0;Create;True;0;0;0;False;0;False;256;256;False;0;1;INT;0
Node;AmplifyShaderEditor.ComponentMaskNode;119;-96,-1664;Inherit;False;True;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;112;-1024,-1664;Inherit;True;Property;_RoughnessMap;Roughness Map;15;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;5b4f3b34a6be3bd4585c339dff8d1a37;5b4f3b34a6be3bd4585c339dff8d1a37;False;black;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.OneMinusNode;121;128,-1664;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;111;-640,-1280;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;129;0,-512;Inherit;False;URP Normal Helper;-1;;377;ac9d436fdaef92c469abf91a59be3ca9;0;3;8;FLOAT4;0,0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;288,-1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;115;-384,-1664;Inherit;False;Mud Triplanar Sample;-1;;378;d9088f0d6015c424b98757b174010394;0;5;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0;False;26;SAMPLERSTATE;0,0,0;False;11;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;102;-384,-640;Inherit;False;Mud Triplanar Sample;-1;;380;d9088f0d6015c424b98757b174010394;0;5;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0;False;26;SAMPLERSTATE;0,0,0;False;11;FLOAT3;1,1,1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexToFragmentNode;66;-1024,-112;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;9;-400,864;Inherit;False;Mud Alpha Threshold;-1;;382;926535703f4c32948ac1f55275a22bf0;0;9;8;FLOAT2;0,0;False;15;FLOAT3;0,0,0;False;18;FLOAT;0;False;22;FLOAT;0;False;19;SAMPLER2D;0;False;26;INT;256;False;9;INT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;2;FLOAT;24;FLOAT;25
Node;AmplifyShaderEditor.TexturePropertyNode;96;-1280,-640;Inherit;True;Property;_NormalMap;Normal Map;13;1;[Normal];Create;True;0;0;0;False;0;False;8fb1a6acf59188448bca62119afcccde;679204acdc00b564398a68f691979695;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;127;-1405.373,896.5676;Inherit;False;124;finalVertexPositionWs;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;896,-1152;Inherit;False;finalVertexPositionWs;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-1424,1504;Inherit;False;Property;_Dithering;Dithering;7;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;120;384,-1152;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-1408,704;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;35;-1408,0;Inherit;False;Mud Mesh;0;;262;4f444db5091a94140ab2b15b933d37b6;0;0;17;COLOR;9;FLOAT;13;FLOAT3;10;FLOAT;11;FLOAT;12;FLOAT4;33;FLOAT3;0;FLOAT3;32;FLOAT3;2;FLOAT3;31;FLOAT3;53;FLOAT3;52;FLOAT3;48;FLOAT3;46;FLOAT;45;FLOAT2;15;FLOAT;41
Node;AmplifyShaderEditor.SimpleDivideOpNode;24;-1152,736;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;94;-1024,-192;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FloorOpNode;26;-992,736;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;105;-640,-768;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;122;640,-1152;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;95;-1024,-272;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;27;-832,736;Inherit;False;Random Range;-1;;356;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;10000;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;107;-1280,-1152;Inherit;True;Property;_DisplacementMap;Displacement Map;11;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;679204acdc00b564398a68f691979695;8fb1a6acf59188448bca62119afcccde;True;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-256,704;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;113;-384,-1152;Inherit;False;Mud Triplanar Sample;-1;;357;d9088f0d6015c424b98757b174010394;0;5;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0;False;26;SAMPLERSTATE;0,0,0;False;11;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;117;128,-1248;Inherit;False;Property;_Displacement;Displacement;12;0;Create;True;0;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;118;128,-1152;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;14;256,608;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-1408,448;Inherit;False;Property;_NoiseSize;Noise Size;8;0;Create;True;0;0;0;False;0;False;0.5;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-1408,544;Inherit;False;Property;_OffsetAmount;Offset Amount;9;0;Create;True;0;0;0;False;0;False;0.005;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;116;-96,-1152;Inherit;False;True;False;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;21;-64,656;Inherit;False;Mud Noise Gradient;-1;;359;ded4656e0e0531448b1f2a26fd64d584;0;3;2;FLOAT3;0,0,0;False;5;FLOAT;0.1;False;7;FLOAT;0.1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;130;1152,60;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;32;896,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;29;896,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;33;896,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;34;896,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;30;1152,0;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;MudBun/Stopmotion Mesh (URP);94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;4;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;2;0;Transmission;0;0;  Transmission Shadow;0.5,False,-1;0;Translucency;0;0;  Translucency Strength;1,False,-1;0;  Normal Distortion;0.5,False,-1;0;  Scattering;2,False,-1;0;  Direct;0.9,False,-1;0;  Ambient;0.1,False,-1;0;  Shadow;0.5,False,-1;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;0;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;31;896,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;131;1152,60;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;126;0;127;0
WireConnection;125;8;126;0
WireConnection;119;0;115;0
WireConnection;121;0;119;0
WireConnection;111;0;94;0
WireConnection;129;8;102;0
WireConnection;129;9;94;0
WireConnection;129;10;66;0
WireConnection;123;0;121;0
WireConnection;123;1;35;12
WireConnection;115;36;95;0
WireConnection;115;37;94;0
WireConnection;115;3;112;0
WireConnection;115;26;112;1
WireConnection;115;11;111;0
WireConnection;102;36;95;0
WireConnection;102;37;94;0
WireConnection;102;3;96;0
WireConnection;102;26;96;1
WireConnection;66;0;35;53
WireConnection;9;8;125;0
WireConnection;9;15;35;0
WireConnection;9;18;35;41
WireConnection;9;22;35;13
WireConnection;9;19;4;0
WireConnection;9;26;8;0
WireConnection;9;9;6;0
WireConnection;9;6;7;0
WireConnection;9;7;5;0
WireConnection;124;0;122;0
WireConnection;120;0;117;0
WireConnection;120;1;118;0
WireConnection;120;2;35;31
WireConnection;24;0;22;0
WireConnection;24;1;23;0
WireConnection;94;0;35;2
WireConnection;26;0;24;0
WireConnection;105;0;94;0
WireConnection;122;0;120;0
WireConnection;122;1;14;0
WireConnection;95;0;35;32
WireConnection;27;1;26;0
WireConnection;28;0;35;32
WireConnection;28;1;27;0
WireConnection;113;36;95;0
WireConnection;113;37;94;0
WireConnection;113;3;107;0
WireConnection;113;26;107;1
WireConnection;113;11;105;0
WireConnection;118;0;116;0
WireConnection;14;0;35;0
WireConnection;14;1;21;0
WireConnection;116;0;113;0
WireConnection;21;2;28;0
WireConnection;21;5;11;0
WireConnection;21;7;19;0
WireConnection;30;0;35;9
WireConnection;30;1;129;0
WireConnection;30;2;35;10
WireConnection;30;3;35;11
WireConnection;30;4;123;0
WireConnection;30;6;9;24
WireConnection;30;7;9;25
WireConnection;30;8;122;0
WireConnection;30;10;35;2
ASEEND*/
//CHKSM=A599E247E47C1008A738E48E020D87BBD78D5117