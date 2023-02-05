// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MudBun/Mud Mesh Single-Textured (URP)"
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
		_AlphaCutoutThreshold("Alpha Cutout Threshold", Range( 0 , 1)) = 0
		_Dithering("Dithering", Range( 0 , 1)) = 1
		[Toggle]_RandomDither("Random Dither", Range( 0 , 1)) = 0
		[Toggle]_UseTex0("Use Texture", Float) = 0
		[Toggle]_UseNorm0("Use Normal Map", Float) = 0
		_MainTex("Texture 0", 2D) = "white" {}
		_MainNorm("Normal Map 0", 2D) = "bump" {}
		_DitherTexture("Dither Texture", 2D) = "white" {}
		_DitherTextureSize("Dither Texture Size", Int) = 256
		[Toggle]_MainTexX("Project Texture 0 X", Float) = 1
		[Toggle]_MainNormX("Project Normal Map 0 X", Float) = 1
		[Toggle]_MainTexY("Project Texture 0 Y", Float) = 1
		[Toggle]_MainNormY("Project Normal Map 0 Y", Float) = 1
		[Toggle]_MainTexZ("Project Texture 0 Z", Float) = 1
		[ASEEnd][Toggle]_MainNormZ("Project Normal Map 0 Z", Float) = 1

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
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _MainNorm;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord7.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord8.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord9 = vertexToFrag5_g83;
				
				float3 vertexToFrag257 = TangentWs4_g83;
				o.ase_texcoord10.xyz = vertexToFrag257;
				
				float3 vertexToFrag6_g83 = (EmissionHash4_g83).xyz;
				o.ase_texcoord11.xyz = vertexToFrag6_g83;
				
				float vertexToFrag8_g83 = Metallic4_g83;
				o.ase_texcoord7.w = vertexToFrag8_g83;
				
				float vertexToFrag7_g83 = Smoothness4_g83;
				o.ase_texcoord8.w = vertexToFrag7_g83;
				
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord12.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord13.xyz = vertexToFrag27_g241;
				
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
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = temp_output_198_2;

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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord7.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord8.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord9;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				
				float3 vertexToFrag257 = IN.ase_texcoord10.xyz;
				float3 temp_output_10_0_g240 = vertexToFrag257;
				float3 temp_output_9_0_g240 = vertexToFrag247;
				float3 appendResult251 = (float3(_MainNormX , _MainNormY , _MainNormZ));
				float3 temp_output_44_0_g238 = ( appendResult251 * abs( vertexToFrag247 ) );
				float3 break14_g238 = temp_output_44_0_g238;
				float3 temp_output_36_0_g238 = vertexToFrag246;
				float4 appendResult23_g238 = (float4(temp_output_44_0_g238 , 0.0));
				float4 appendResult24_g238 = (float4(temp_output_44_0_g238 , 1.0));
				float4 break10_g239 = ( ( break14_g238.x + break14_g238.y + break14_g238.z ) > 0.0 ? appendResult23_g238 : appendResult24_g238 );
				float4 color20_g238 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 normalizeResult6_g240 = normalize( ( ( cross( temp_output_10_0_g240 , temp_output_9_0_g240 ) * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).x ) + ( temp_output_10_0_g240 * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).y ) + ( temp_output_9_0_g240 * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).z ) ) );
				
				float3 vertexToFrag6_g83 = IN.ase_texcoord11.xyz;
				
				float vertexToFrag8_g83 = IN.ase_texcoord7.w;
				
				float vertexToFrag7_g83 = IN.ase_texcoord8.w;
				
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord12.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord13.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord10.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				float3 Albedo = temp_output_175_0.xyz;
				float3 Normal = normalizeResult6_g240;
				float3 Emission = ( vertexToFrag6_g83 * (_Emission).rgb );
				float3 Specular = 0.5;
				float Metallic = ( _Metallic * vertexToFrag8_g83 );
				float Smoothness = ( _Smoothness * vertexToFrag7_g83 );
				float Occlusion = 1;
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;
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
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _DitherTexture;


			
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

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord2.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord3.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord4 = vertexToFrag5_g83;
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord5.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord6.xyz = vertexToFrag27_g241;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_198_2;

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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord2.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord3.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord4;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord5.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord6.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord2.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;
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
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord2.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord3.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord4 = vertexToFrag5_g83;
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord5.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord6.xyz = vertexToFrag27_g241;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_198_2;
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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord2.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord3.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord4;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord5.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord6.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord2.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;
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
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord2.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord3.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord4 = vertexToFrag5_g83;
				
				float3 vertexToFrag6_g83 = (EmissionHash4_g83).xyz;
				o.ase_texcoord5.xyz = vertexToFrag6_g83;
				
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord6.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord7.xyz = vertexToFrag27_g241;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				o.ase_texcoord7.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_198_2;

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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord2.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord3.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord4;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				
				float3 vertexToFrag6_g83 = IN.ase_texcoord5.xyz;
				
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord6.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord7.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord2.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				
				float3 Albedo = temp_output_175_0.xyz;
				float3 Emission = ( vertexToFrag6_g83 * (_Emission).rgb );
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;

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
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord2.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord3.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord4 = vertexToFrag5_g83;
				
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord5.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord6.xyz = vertexToFrag27_g241;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord5.w = 0;
				o.ase_texcoord6.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_198_2;

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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord2.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord3.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord4;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord5.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord6.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord2.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				
				float3 Albedo = temp_output_175_0.xyz;
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;

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
				float3 worldNormal : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord3.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord4.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord5 = vertexToFrag5_g83;
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord6.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord7.xyz = vertexToFrag27_g241;
				
				o.ase_texcoord3.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord6.w = 0;
				o.ase_texcoord7.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = temp_output_198_2;
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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord3.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord4.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord5;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord6.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord7.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord3.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;
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
			float4 _MainTex_ST;
			float4 _MainNorm_ST;
			float _UseTex0;
			float _MainTexX;
			float _MainTexY;
			float _MainTexZ;
			float _IsMeshRenderMaterial;
			float _UseNorm0;
			float _MainNormX;
			float _MainNormY;
			float _MainNormZ;
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
			sampler2D _MainTex;
			sampler2D _MainNorm;
			sampler2D _DitherTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = v.ase_vertexID;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float3 temp_output_198_0 = PositionWs4_g83;
				
				float3 temp_output_198_2 = NormalWs4_g83;
				
				float3 vertexToFrag247 = temp_output_198_2;
				o.ase_texcoord7.xyz = vertexToFrag247;
				float3 vertexToFrag246 = PositionLs4_g83;
				o.ase_texcoord8.xyz = vertexToFrag246;
				float4 vertexToFrag5_g83 = Color4_g83;
				o.ase_texcoord9 = vertexToFrag5_g83;
				
				float3 vertexToFrag257 = TangentWs4_g83;
				o.ase_texcoord10.xyz = vertexToFrag257;
				
				float3 vertexToFrag6_g83 = (EmissionHash4_g83).xyz;
				o.ase_texcoord11.xyz = vertexToFrag6_g83;
				
				float vertexToFrag8_g83 = Metallic4_g83;
				o.ase_texcoord7.w = vertexToFrag8_g83;
				
				float vertexToFrag7_g83 = Smoothness4_g83;
				o.ase_texcoord8.w = vertexToFrag7_g83;
				
				float3 vertexToFrag16_g83 = PositionWs4_g83;
				o.ase_texcoord12.xyz = vertexToFrag16_g83;
				float3 vertexToFrag27_g241 = temp_output_198_0;
				o.ase_texcoord13.xyz = vertexToFrag27_g241;
				
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
				float3 vertexValue = temp_output_198_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = temp_output_198_2;

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

				float3 appendResult238 = (float3(_MainTexX , _MainTexY , _MainTexZ));
				float3 vertexToFrag247 = IN.ase_texcoord7.xyz;
				float3 temp_output_44_0_g242 = ( appendResult238 * abs( vertexToFrag247 ) );
				float3 break14_g242 = temp_output_44_0_g242;
				float3 vertexToFrag246 = IN.ase_texcoord8.xyz;
				float3 temp_output_36_0_g242 = vertexToFrag246;
				float4 appendResult23_g242 = (float4(temp_output_44_0_g242 , 0.0));
				float4 appendResult24_g242 = (float4(temp_output_44_0_g242 , 1.0));
				float4 break10_g243 = ( ( break14_g242.x + break14_g242.y + break14_g242.z ) > 0.0 ? appendResult23_g242 : appendResult24_g242 );
				float4 color20_g242 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float4 color182 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
				float4 vertexToFrag5_g83 = IN.ase_texcoord9;
				float4 temp_output_25_0_g83 = ( _IsMeshRenderMaterial * _Color * vertexToFrag5_g83 );
				float4 temp_output_175_0 = ( ( _UseTex0 > 0.0 ? ( ( ( ( break14_g242.x > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).yz * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.x ) + ( ( break14_g242.y > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).zx * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.y ) + ( ( break14_g242.z > 0.0 ? tex2D( _MainTex, ( ( (temp_output_36_0_g242).xy * _MainTex_ST.xy ) + _MainTex_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g243.z ) + ( color20_g242 * break10_g243.w ) ) / ( break10_g243.x + break10_g243.y + break10_g243.z + break10_g243.w ) ) : color182 ) * temp_output_25_0_g83 );
				
				float3 vertexToFrag257 = IN.ase_texcoord10.xyz;
				float3 temp_output_10_0_g240 = vertexToFrag257;
				float3 temp_output_9_0_g240 = vertexToFrag247;
				float3 appendResult251 = (float3(_MainNormX , _MainNormY , _MainNormZ));
				float3 temp_output_44_0_g238 = ( appendResult251 * abs( vertexToFrag247 ) );
				float3 break14_g238 = temp_output_44_0_g238;
				float3 temp_output_36_0_g238 = vertexToFrag246;
				float4 appendResult23_g238 = (float4(temp_output_44_0_g238 , 0.0));
				float4 appendResult24_g238 = (float4(temp_output_44_0_g238 , 1.0));
				float4 break10_g239 = ( ( break14_g238.x + break14_g238.y + break14_g238.z ) > 0.0 ? appendResult23_g238 : appendResult24_g238 );
				float4 color20_g238 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 normalizeResult6_g240 = normalize( ( ( cross( temp_output_10_0_g240 , temp_output_9_0_g240 ) * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).x ) + ( temp_output_10_0_g240 * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).y ) + ( temp_output_9_0_g240 * UnpackNormalScale( ( _UseNorm0 > 0.0 ? ( ( ( ( break14_g238.x > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).yz * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.x ) + ( ( break14_g238.y > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).zx * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.y ) + ( ( break14_g238.z > 0.0 ? tex2D( _MainNorm, ( ( (temp_output_36_0_g238).xy * _MainNorm_ST.xy ) + _MainNorm_ST.zw ) ) : float4( 0,0,0,0 ) ) * break10_g239.z ) + ( color20_g238 * break10_g239.w ) ) / ( break10_g239.x + break10_g239.y + break10_g239.z + break10_g239.w ) ) : float4(0.5019608,0.5019608,1,1) ), 1.0 ).z ) ) );
				
				float3 vertexToFrag6_g83 = IN.ase_texcoord11.xyz;
				
				float vertexToFrag8_g83 = IN.ase_texcoord7.w;
				
				float vertexToFrag7_g83 = IN.ase_texcoord8.w;
				
				float localComputeOpaqueTransparency20_g241 = ( 0.0 );
				float3 vertexToFrag16_g83 = IN.ase_texcoord12.xyz;
				float4 unityObjectToClipPos1_g84 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag16_g83));
				float4 computeScreenPos3_g84 = ComputeScreenPos( unityObjectToClipPos1_g84 );
				float2 ScreenPos20_g241 = (( ( computeScreenPos3_g84 / (computeScreenPos3_g84).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g241 = IN.ase_texcoord13.xyz;
				float3 VertPos20_g241 = vertexToFrag27_g241;
				float localMudBunMeshPoint4_g83 = ( 0.0 );
				int VertexID4_g83 = IN.ase_texcoord10.w;
				float3 PositionWs4_g83 = float3( 0,0,0 );
				float3 PositionLs4_g83 = float3( 0,0,0 );
				float3 NormalWs4_g83 = float3( 0,0,0 );
				float3 NormalLs4_g83 = float3( 0,0,0 );
				float3 TangentWs4_g83 = float3( 0,0,0 );
				float3 TangentLs4_g83 = float3( 0,0,0 );
				float4 Color4_g83 = float4( 0,0,0,0 );
				float4 EmissionHash4_g83 = float4( 0,0,0,0 );
				float Metallic4_g83 = 0;
				float Smoothness4_g83 = 0;
				float4 TextureWeight4_g83 = float4( 1,0,0,0 );
				float SdfValue4_g83 = 0;
				float3 Outward2dNormalLs4_g83 = float3( 0,0,0 );
				float3 Outward2dNormalWs4_g83 = float3( 0,0,0 );
				{
				float4 positionWs;
				float2 metallicSmoothness;
				mudbun_mesh_vert(VertexID4_g83, positionWs, PositionLs4_g83, NormalWs4_g83, NormalLs4_g83, TangentWs4_g83, TangentLs4_g83, Color4_g83, EmissionHash4_g83, metallicSmoothness, TextureWeight4_g83, SdfValue4_g83, Outward2dNormalLs4_g83, Outward2dNormalWs4_g83);
				PositionWs4_g83 = positionWs.xyz;
				Metallic4_g83 = metallicSmoothness.x;
				Smoothness4_g83 = metallicSmoothness.y;
				#ifdef MUDBUN_BUILT_IN_RP
				#ifndef MUDBUN_VERTEX_SHADER
				v.tangent = float4(TangentWs4_g83, 0.0f);
				#define MUDBUN_VERTEX_SHADER
				#endif
				#endif
				}
				float Hash20_g241 = (EmissionHash4_g83).w;
				float AlphaIn20_g241 = (temp_output_25_0_g83).a;
				float AlphaOut20_g241 = 0;
				float AlphaThreshold20_g241 = 0;
				sampler2D DitherNoiseTexture20_g241 = _DitherTexture;
				int DitherNoiseTextureSize20_g241 = _DitherTextureSize;
				int UseRandomDither20_g241 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g241 = _AlphaCutoutThreshold;
				float DitherBlend20_g241 = _Dithering;
				{
				float alpha = AlphaIn20_g241;
				computeOpaqueTransparency(ScreenPos20_g241, VertPos20_g241, Hash20_g241, DitherNoiseTexture20_g241, DitherNoiseTextureSize20_g241, UseRandomDither20_g241 > 0, AlphaCutoutThreshold20_g241, DitherBlend20_g241,  alpha, AlphaThreshold20_g241);
				AlphaOut20_g241 = alpha;
				}
				
				float3 Albedo = temp_output_175_0.xyz;
				float3 Normal = normalizeResult6_g240;
				float3 Emission = ( vertexToFrag6_g83 * (_Emission).rgb );
				float3 Specular = 0.5;
				float Metallic = ( _Metallic * vertexToFrag8_g83 );
				float Smoothness = ( _Smoothness * vertexToFrag7_g83 );
				float Occlusion = 1;
				float Alpha = ( (temp_output_175_0).w * AlphaOut20_g241 );
				float AlphaClipThreshold = AlphaThreshold20_g241;
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
	
	CustomEditor "MudBun.MudMeshSingleTexturedMaterialEditor"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18935
396;313.6;1104;664.6;-1345.491;531.9808;1;True;False
Node;AmplifyShaderEditor.FunctionNode;198;-128,-192;Inherit;False;Mud Mesh;0;;83;4f444db5091a94140ab2b15b933d37b6;0;0;17;COLOR;9;FLOAT;13;FLOAT3;10;FLOAT;11;FLOAT;12;FLOAT4;33;FLOAT3;0;FLOAT3;32;FLOAT3;2;FLOAT3;31;FLOAT3;53;FLOAT3;52;FLOAT3;48;FLOAT3;46;FLOAT;45;FLOAT2;15;FLOAT;41
Node;AmplifyShaderEditor.RangedFloatNode;184;128,-432;Inherit;False;Property;_MainTexY;Project Texture 0 Y;17;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;185;128,-352;Inherit;False;Property;_MainTexZ;Project Texture 0 Z;19;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;128,-512;Inherit;False;Property;_MainTexX;Project Texture 0 X;15;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;238;450.062,-502.8722;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;146;128,-736;Inherit;True;Property;_MainTex;Texture 0;11;0;Create;False;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.VertexToFragmentNode;246;384,-288;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexToFragmentNode;247;384,-208;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;248;128,-896;Inherit;False;Property;_MainNormZ;Project Normal Map 0 Z;20;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;182;1152,-512;Inherit;False;Constant;_White;White;26;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;250;128,-1056;Inherit;False;Property;_MainNormX;Project Normal Map 0 X;16;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;245;768,-512;Inherit;False;Mud Triplanar Sample;-1;;242;d9088f0d6015c424b98757b174010394;0;5;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0;False;26;SAMPLERSTATE;0,0,0;False;11;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;178;1152,-768;Inherit;False;Property;_UseTex0;Use Texture;9;1;[Toggle];Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;249;128,-976;Inherit;False;Property;_MainNormY;Project Normal Map 0 Y;18;1;[Toggle];Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;252;128,-1280;Inherit;True;Property;_MainNorm;Normal Map 0;12;0;Create;False;0;0;0;False;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Compare;231;1408,-768;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;251;448,-1040;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;175;1664,-384;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-128,768;Inherit;False;Property;_AlphaCutoutThreshold;Alpha Cutout Threshold;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-128,672;Inherit;False;Property;_Dithering;Dithering;7;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;241;-128,480;Inherit;False;Property;_DitherTextureSize;Dither Texture Size;14;0;Create;True;0;0;0;False;0;False;256;256;False;0;1;INT;0
Node;AmplifyShaderEditor.ColorNode;254;1152,-1024;Inherit;False;Constant;_Flat;Flat;26;0;Create;True;0;0;0;False;0;False;0.5019608,0.5019608,1,1;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;239;-128,576;Inherit;False;Property;_RandomDither;Random Dither;8;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;240;-128,256;Inherit;True;Property;_DitherTexture;Dither Texture;13;0;Create;True;0;0;0;False;0;False;f240bbb7854046345b218811e5681a54;f240bbb7854046345b218811e5681a54;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;255;1152,-1280;Inherit;False;Property;_UseNorm0;Use Normal Map;10;1;[Toggle];Create;False;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;253;768,-1056;Inherit;False;Mud Triplanar Sample;-1;;238;d9088f0d6015c424b98757b174010394;0;5;36;FLOAT3;0,0,0;False;37;FLOAT3;0,0,0;False;3;SAMPLER2D;0,0,0;False;26;SAMPLERSTATE;0,0,0;False;11;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexToFragmentNode;257;384,-128;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;256;1408,-1280;Inherit;False;2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FunctionNode;125;512,128;Inherit;False;Mud Alpha Threshold;-1;;241;926535703f4c32948ac1f55275a22bf0;0;9;8;FLOAT2;0,0;False;15;FLOAT3;0,0,0;False;18;FLOAT;0;False;22;FLOAT;0;False;19;SAMPLER2D;0;False;26;INT;256;False;9;INT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;2;FLOAT;24;FLOAT;25
Node;AmplifyShaderEditor.ComponentMaskNode;176;1920,-288;Inherit;False;False;False;False;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;243;2176,-272;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;258;1664,-1280;Inherit;False;URP Normal Helper;-1;;240;ac9d436fdaef92c469abf91a59be3ca9;0;3;8;FLOAT4;0,0,0,0;False;9;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;221;2944,-960;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;259;2688,-228;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;220;2944,-960;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;223;2944,-960;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;218;2432,-288;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;219;2688,-288;Float;False;True;-1;2;MudBun.MudMeshSingleTexturedMaterialEditor;0;2;MudBun/Mud Mesh Single-Textured (URP);94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;4;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;2;0;Transmission;0;0;  Transmission Shadow;0.5,False,-1;0;Translucency;0;0;  Translucency Strength;1,False,-1;0;  Normal Distortion;0.5,False,-1;0;  Scattering;2,False,-1;0;  Direct;0.9,False,-1;0;  Ambient;0.1,False,-1;0;  Shadow;0.5,False,-1;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;0;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;222;2944,-960;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;260;2688,-228;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;238;0;183;0
WireConnection;238;1;184;0
WireConnection;238;2;185;0
WireConnection;246;0;198;32
WireConnection;247;0;198;2
WireConnection;245;36;246;0
WireConnection;245;37;247;0
WireConnection;245;3;146;0
WireConnection;245;26;146;1
WireConnection;245;11;238;0
WireConnection;231;0;178;0
WireConnection;231;2;245;0
WireConnection;231;3;182;0
WireConnection;251;0;250;0
WireConnection;251;1;249;0
WireConnection;251;2;248;0
WireConnection;175;0;231;0
WireConnection;175;1;198;9
WireConnection;253;36;246;0
WireConnection;253;37;247;0
WireConnection;253;3;252;0
WireConnection;253;26;252;1
WireConnection;253;11;251;0
WireConnection;257;0;198;53
WireConnection;256;0;255;0
WireConnection;256;2;253;0
WireConnection;256;3;254;0
WireConnection;125;8;198;15
WireConnection;125;15;198;0
WireConnection;125;18;198;41
WireConnection;125;22;198;13
WireConnection;125;19;240;0
WireConnection;125;26;241;0
WireConnection;125;9;239;0
WireConnection;125;6;64;0
WireConnection;125;7;65;0
WireConnection;176;0;175;0
WireConnection;243;0;176;0
WireConnection;243;1;125;24
WireConnection;258;8;256;0
WireConnection;258;9;247;0
WireConnection;258;10;257;0
WireConnection;219;0;175;0
WireConnection;219;1;258;0
WireConnection;219;2;198;10
WireConnection;219;3;198;11
WireConnection;219;4;198;12
WireConnection;219;6;243;0
WireConnection;219;7;125;25
WireConnection;219;8;198;0
WireConnection;219;10;198;2
ASEEND*/
//CHKSM=0F21662C41F9D8C04773B8A0153648A2B3627B03