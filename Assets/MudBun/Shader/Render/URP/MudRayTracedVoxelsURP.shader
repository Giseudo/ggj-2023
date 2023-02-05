// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Mud Ray Traced Voxels (URP)"
{
	Properties
	{
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[ASEBegin]_Color("Color", Color) = (0,0,0,0)
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		_AlphaCutoutThreshold("Alpha Cutout Threshold", Range( 0 , 1)) = 0
		_DitherTexture("Dither Texture", 2D) = "white" {}
		_Dithering("Dithering", Range( 0 , 1)) = 1
		_DitherTextureSize("Dither Texture Size", Int) = 256
		[ASEEnd][Toggle]_RandomDither("Random Dither", Range( 0 , 1)) = 0

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
			#define ASE_DEPTH_WRITE_ON
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
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

			#define MUDBUN_URP
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/RayTracedVoxelsCommon.cginc"


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
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
			sampler2D _DitherTexture;


			float3 MudBunRayTracedVoxelsVertex2_g51( int Id, out float3 VertexPosLs )
			{
				float3 vertexPosWs;
				mudbun_ray_traced_voxels_vert(Id, VertexPosLs, vertexPosWs);
				return vertexPosWs;
			}
			
			float4x4 WorldToLocalMatrix69_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 WorldToLocalMatrix138_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 LocalToWorldIT78_g51(  )
			{
				return localToWorldIt;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int Id2_g51 = v.ase_vertexID;
				float3 VertexPosLs2_g51 = float3( 0,0,0 );
				float3 localMudBunRayTracedVoxelsVertex2_g51 = MudBunRayTracedVoxelsVertex2_g51( Id2_g51 , VertexPosLs2_g51 );
				float3 vertexPosWs40_g51 = localMudBunRayTracedVoxelsVertex2_g51;
				float3 temp_output_127_0 = vertexPosWs40_g51;
				
				float3 vertexPosLs75_g51 = VertexPosLs2_g51;
				float3 vertexToFrag112_g51 = vertexPosLs75_g51;
				o.ase_texcoord7.yzw = vertexToFrag112_g51;
				float3 vertexToFrag50_g51 = vertexPosLs75_g51;
				o.ase_texcoord8.xyz = vertexToFrag50_g51;
				
				float3 vertexToFrag121_g51 = vertexPosWs40_g51;
				o.ase_texcoord9.xyz = vertexToFrag121_g51;
				float3 vertexToFrag27_g53 = temp_output_127_0;
				o.ase_texcoord10.xyz = vertexToFrag27_g53;
				
				o.ase_texcoord7.x = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.w = 0;
				o.ase_texcoord9.w = 0;
				o.ase_texcoord10.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_127_0;
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

				float localMudBunRayTracedVoxelsFragment64_g51 = ( 0.0 );
				int Id64_g51 = IN.ase_texcoord7.x;
				float3 vertexToFrag112_g51 = IN.ase_texcoord7.yzw;
				float3 VertPosLs64_g51 = vertexToFrag112_g51;
				float4x4 localWorldToLocalMatrix69_g51 = WorldToLocalMatrix69_g51();
				float3 rayOriginLs72_g51 = (mul( localWorldToLocalMatrix69_g51, float4( unity_CameraToWorld[0][3],unity_CameraToWorld[1][3],unity_CameraToWorld[2][3],unity_CameraToWorld[3][3] ) )).xyz;
				float3 RayOriginLs64_g51 = rayOriginLs72_g51;
				float3 vertexToFrag50_g51 = IN.ase_texcoord8.xyz;
				float3 normalizeResult46_g51 = normalize( (( vertexToFrag50_g51 - rayOriginLs72_g51 )).xyz );
				float3 rayDirLs49_g51 = normalizeResult46_g51;
				float3 RayDirLs64_g51 = rayDirLs49_g51;
				float4x4 localWorldToLocalMatrix138_g51 = WorldToLocalMatrix138_g51();
				float3 viewDirLs137_g51 = (mul( localWorldToLocalMatrix138_g51, float4( unity_CameraToWorld[0][2],unity_CameraToWorld[1][2],unity_CameraToWorld[2][2],unity_CameraToWorld[3][2] ) )).xyz;
				float3 ViewDirLs64_g51 = viewDirLs137_g51;
				float4 Color64_g51 = float4( 0,0,0,0 );
				float3 Emission64_g51 = float3( 0,0,0 );
				float Metallic64_g51 = 0;
				float Smoothness64_g51 = 0;
				float4 TextureWeight64_g51 = float4( 0,0,0,0 );
				float3 FragmentPosLs64_g51 = float3( 0,0,0 );
				float Depth64_g51 = 0;
				float3 FragmentNormLs64_g51 = float3( 0,0,0 );
				int BrushHash64_g51 = 0;
				{
				mudbun_ray_traced_voxels_frag(Id64_g51, VertPosLs64_g51, RayOriginLs64_g51, RayDirLs64_g51, ViewDirLs64_g51, FragmentPosLs64_g51, FragmentNormLs64_g51, Depth64_g51, Color64_g51, Emission64_g51, Metallic64_g51, Smoothness64_g51, TextureWeight64_g51);
				#ifdef SHADERPASS_FORWARD
				ShadowCoords = TransformWorldToShadowCoord(mul(localToWorld, float4(FragmentPosLs64_g51, 1.0f)).xyz);
				#endif
				}
				float4 temp_output_104_0_g51 = ( Color64_g51 * _Color );
				
				float4x4 localLocalToWorldIT78_g51 = LocalToWorldIT78_g51();
				
				float localComputeOpaqueTransparency20_g53 = ( 0.0 );
				float3 vertexToFrag121_g51 = IN.ase_texcoord9.xyz;
				float4 unityObjectToClipPos1_g52 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag121_g51));
				float4 computeScreenPos3_g52 = ComputeScreenPos( unityObjectToClipPos1_g52 );
				float2 ScreenPos20_g53 = (( ( computeScreenPos3_g52 / (computeScreenPos3_g52).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g53 = IN.ase_texcoord10.xyz;
				float3 VertPos20_g53 = vertexToFrag27_g53;
				float Hash20_g53 = (float)BrushHash64_g51;
				float AlphaIn20_g53 = (temp_output_104_0_g51).w;
				float AlphaOut20_g53 = 0;
				float AlphaThreshold20_g53 = 0;
				sampler2D DitherNoiseTexture20_g53 = _DitherTexture;
				int DitherNoiseTextureSize20_g53 = _DitherTextureSize;
				int UseRandomDither20_g53 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g53 = _AlphaCutoutThreshold;
				float DitherBlend20_g53 = _Dithering;
				{
				float alpha = AlphaIn20_g53;
				computeOpaqueTransparency(ScreenPos20_g53, VertPos20_g53, Hash20_g53, DitherNoiseTexture20_g53, DitherNoiseTextureSize20_g53, UseRandomDither20_g53 > 0, AlphaCutoutThreshold20_g53, DitherBlend20_g53,  alpha, AlphaThreshold20_g53);
				AlphaOut20_g53 = alpha;
				}
				
				float3 Albedo = (temp_output_104_0_g51).xyz;
				float3 Normal = mul( localLocalToWorldIT78_g51, float4( FragmentNormLs64_g51 , 0.0 ) ).xyz;
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = ( Metallic64_g51 * _Metallic );
				float Smoothness = ( Smoothness64_g51 * _Smoothness );
				float Occlusion = 1;
				float Alpha = AlphaOut20_g53;
				float AlphaClipThreshold = AlphaThreshold20_g53;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = Depth64_g51;
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
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, WorldNormal ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos ) * RefractionColor;
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

			HLSLPROGRAM
			#define _NORMAL_DROPOFF_WS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define ASE_DEPTH_WRITE_ON
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define MUDBUN_URP
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/RayTracedVoxelsCommon.cginc"


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
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
			sampler2D _DitherTexture;


			float3 MudBunRayTracedVoxelsVertex2_g51( int Id, out float3 VertexPosLs )
			{
				float3 vertexPosWs;
				mudbun_ray_traced_voxels_vert(Id, VertexPosLs, vertexPosWs);
				return vertexPosWs;
			}
			
			float4x4 WorldToLocalMatrix69_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 WorldToLocalMatrix138_g51(  )
			{
				return worldToLocal;
			}
			

			float3 _LightDirection;

			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int Id2_g51 = v.ase_vertexID;
				float3 VertexPosLs2_g51 = float3( 0,0,0 );
				float3 localMudBunRayTracedVoxelsVertex2_g51 = MudBunRayTracedVoxelsVertex2_g51( Id2_g51 , VertexPosLs2_g51 );
				float3 vertexPosWs40_g51 = localMudBunRayTracedVoxelsVertex2_g51;
				float3 temp_output_127_0 = vertexPosWs40_g51;
				
				float3 vertexToFrag121_g51 = vertexPosWs40_g51;
				o.ase_texcoord2.xyz = vertexToFrag121_g51;
				float3 vertexToFrag27_g53 = temp_output_127_0;
				o.ase_texcoord3.xyz = vertexToFrag27_g53;
				float3 vertexPosLs75_g51 = VertexPosLs2_g51;
				float3 vertexToFrag112_g51 = vertexPosLs75_g51;
				o.ase_texcoord4.xyz = vertexToFrag112_g51;
				float3 vertexToFrag50_g51 = vertexPosLs75_g51;
				o.ase_texcoord5.xyz = vertexToFrag50_g51;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_127_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

				float4 clipPos = TransformWorldToHClip( ApplyShadowBias( positionWS, normalWS, _LightDirection ) );

				#if UNITY_REVERSED_Z
					clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
				#else
					clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
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

				float localComputeOpaqueTransparency20_g53 = ( 0.0 );
				float3 vertexToFrag121_g51 = IN.ase_texcoord2.xyz;
				float4 unityObjectToClipPos1_g52 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag121_g51));
				float4 computeScreenPos3_g52 = ComputeScreenPos( unityObjectToClipPos1_g52 );
				float2 ScreenPos20_g53 = (( ( computeScreenPos3_g52 / (computeScreenPos3_g52).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g53 = IN.ase_texcoord3.xyz;
				float3 VertPos20_g53 = vertexToFrag27_g53;
				float localMudBunRayTracedVoxelsFragment64_g51 = ( 0.0 );
				int Id64_g51 = IN.ase_texcoord2.w;
				float3 vertexToFrag112_g51 = IN.ase_texcoord4.xyz;
				float3 VertPosLs64_g51 = vertexToFrag112_g51;
				float4x4 localWorldToLocalMatrix69_g51 = WorldToLocalMatrix69_g51();
				float3 rayOriginLs72_g51 = (mul( localWorldToLocalMatrix69_g51, float4( unity_CameraToWorld[0][3],unity_CameraToWorld[1][3],unity_CameraToWorld[2][3],unity_CameraToWorld[3][3] ) )).xyz;
				float3 RayOriginLs64_g51 = rayOriginLs72_g51;
				float3 vertexToFrag50_g51 = IN.ase_texcoord5.xyz;
				float3 normalizeResult46_g51 = normalize( (( vertexToFrag50_g51 - rayOriginLs72_g51 )).xyz );
				float3 rayDirLs49_g51 = normalizeResult46_g51;
				float3 RayDirLs64_g51 = rayDirLs49_g51;
				float4x4 localWorldToLocalMatrix138_g51 = WorldToLocalMatrix138_g51();
				float3 viewDirLs137_g51 = (mul( localWorldToLocalMatrix138_g51, float4( unity_CameraToWorld[0][2],unity_CameraToWorld[1][2],unity_CameraToWorld[2][2],unity_CameraToWorld[3][2] ) )).xyz;
				float3 ViewDirLs64_g51 = viewDirLs137_g51;
				float4 Color64_g51 = float4( 0,0,0,0 );
				float3 Emission64_g51 = float3( 0,0,0 );
				float Metallic64_g51 = 0;
				float Smoothness64_g51 = 0;
				float4 TextureWeight64_g51 = float4( 0,0,0,0 );
				float3 FragmentPosLs64_g51 = float3( 0,0,0 );
				float Depth64_g51 = 0;
				float3 FragmentNormLs64_g51 = float3( 0,0,0 );
				int BrushHash64_g51 = 0;
				{
				mudbun_ray_traced_voxels_frag(Id64_g51, VertPosLs64_g51, RayOriginLs64_g51, RayDirLs64_g51, ViewDirLs64_g51, FragmentPosLs64_g51, FragmentNormLs64_g51, Depth64_g51, Color64_g51, Emission64_g51, Metallic64_g51, Smoothness64_g51, TextureWeight64_g51);
				#ifdef SHADERPASS_FORWARD
				ShadowCoords = TransformWorldToShadowCoord(mul(localToWorld, float4(FragmentPosLs64_g51, 1.0f)).xyz);
				#endif
				}
				float Hash20_g53 = (float)BrushHash64_g51;
				float4 temp_output_104_0_g51 = ( Color64_g51 * _Color );
				float AlphaIn20_g53 = (temp_output_104_0_g51).w;
				float AlphaOut20_g53 = 0;
				float AlphaThreshold20_g53 = 0;
				sampler2D DitherNoiseTexture20_g53 = _DitherTexture;
				int DitherNoiseTextureSize20_g53 = _DitherTextureSize;
				int UseRandomDither20_g53 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g53 = _AlphaCutoutThreshold;
				float DitherBlend20_g53 = _Dithering;
				{
				float alpha = AlphaIn20_g53;
				computeOpaqueTransparency(ScreenPos20_g53, VertPos20_g53, Hash20_g53, DitherNoiseTexture20_g53, DitherNoiseTextureSize20_g53, UseRandomDither20_g53 > 0, AlphaCutoutThreshold20_g53, DitherBlend20_g53,  alpha, AlphaThreshold20_g53);
				AlphaOut20_g53 = alpha;
				}
				
				float Alpha = AlphaOut20_g53;
				float AlphaClipThreshold = AlphaThreshold20_g53;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = Depth64_g51;
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
			#define ASE_DEPTH_WRITE_ON
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define MUDBUN_URP
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/RayTracedVoxelsCommon.cginc"


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
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
			sampler2D _DitherTexture;


			float3 MudBunRayTracedVoxelsVertex2_g51( int Id, out float3 VertexPosLs )
			{
				float3 vertexPosWs;
				mudbun_ray_traced_voxels_vert(Id, VertexPosLs, vertexPosWs);
				return vertexPosWs;
			}
			
			float4x4 WorldToLocalMatrix69_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 WorldToLocalMatrix138_g51(  )
			{
				return worldToLocal;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int Id2_g51 = v.ase_vertexID;
				float3 VertexPosLs2_g51 = float3( 0,0,0 );
				float3 localMudBunRayTracedVoxelsVertex2_g51 = MudBunRayTracedVoxelsVertex2_g51( Id2_g51 , VertexPosLs2_g51 );
				float3 vertexPosWs40_g51 = localMudBunRayTracedVoxelsVertex2_g51;
				float3 temp_output_127_0 = vertexPosWs40_g51;
				
				float3 vertexToFrag121_g51 = vertexPosWs40_g51;
				o.ase_texcoord2.xyz = vertexToFrag121_g51;
				float3 vertexToFrag27_g53 = temp_output_127_0;
				o.ase_texcoord3.xyz = vertexToFrag27_g53;
				float3 vertexPosLs75_g51 = VertexPosLs2_g51;
				float3 vertexToFrag112_g51 = vertexPosLs75_g51;
				o.ase_texcoord4.xyz = vertexToFrag112_g51;
				float3 vertexToFrag50_g51 = vertexPosLs75_g51;
				o.ase_texcoord5.xyz = vertexToFrag50_g51;
				
				o.ase_texcoord2.w = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_127_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
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

				float localComputeOpaqueTransparency20_g53 = ( 0.0 );
				float3 vertexToFrag121_g51 = IN.ase_texcoord2.xyz;
				float4 unityObjectToClipPos1_g52 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag121_g51));
				float4 computeScreenPos3_g52 = ComputeScreenPos( unityObjectToClipPos1_g52 );
				float2 ScreenPos20_g53 = (( ( computeScreenPos3_g52 / (computeScreenPos3_g52).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g53 = IN.ase_texcoord3.xyz;
				float3 VertPos20_g53 = vertexToFrag27_g53;
				float localMudBunRayTracedVoxelsFragment64_g51 = ( 0.0 );
				int Id64_g51 = IN.ase_texcoord2.w;
				float3 vertexToFrag112_g51 = IN.ase_texcoord4.xyz;
				float3 VertPosLs64_g51 = vertexToFrag112_g51;
				float4x4 localWorldToLocalMatrix69_g51 = WorldToLocalMatrix69_g51();
				float3 rayOriginLs72_g51 = (mul( localWorldToLocalMatrix69_g51, float4( unity_CameraToWorld[0][3],unity_CameraToWorld[1][3],unity_CameraToWorld[2][3],unity_CameraToWorld[3][3] ) )).xyz;
				float3 RayOriginLs64_g51 = rayOriginLs72_g51;
				float3 vertexToFrag50_g51 = IN.ase_texcoord5.xyz;
				float3 normalizeResult46_g51 = normalize( (( vertexToFrag50_g51 - rayOriginLs72_g51 )).xyz );
				float3 rayDirLs49_g51 = normalizeResult46_g51;
				float3 RayDirLs64_g51 = rayDirLs49_g51;
				float4x4 localWorldToLocalMatrix138_g51 = WorldToLocalMatrix138_g51();
				float3 viewDirLs137_g51 = (mul( localWorldToLocalMatrix138_g51, float4( unity_CameraToWorld[0][2],unity_CameraToWorld[1][2],unity_CameraToWorld[2][2],unity_CameraToWorld[3][2] ) )).xyz;
				float3 ViewDirLs64_g51 = viewDirLs137_g51;
				float4 Color64_g51 = float4( 0,0,0,0 );
				float3 Emission64_g51 = float3( 0,0,0 );
				float Metallic64_g51 = 0;
				float Smoothness64_g51 = 0;
				float4 TextureWeight64_g51 = float4( 0,0,0,0 );
				float3 FragmentPosLs64_g51 = float3( 0,0,0 );
				float Depth64_g51 = 0;
				float3 FragmentNormLs64_g51 = float3( 0,0,0 );
				int BrushHash64_g51 = 0;
				{
				mudbun_ray_traced_voxels_frag(Id64_g51, VertPosLs64_g51, RayOriginLs64_g51, RayDirLs64_g51, ViewDirLs64_g51, FragmentPosLs64_g51, FragmentNormLs64_g51, Depth64_g51, Color64_g51, Emission64_g51, Metallic64_g51, Smoothness64_g51, TextureWeight64_g51);
				#ifdef SHADERPASS_FORWARD
				ShadowCoords = TransformWorldToShadowCoord(mul(localToWorld, float4(FragmentPosLs64_g51, 1.0f)).xyz);
				#endif
				}
				float Hash20_g53 = (float)BrushHash64_g51;
				float4 temp_output_104_0_g51 = ( Color64_g51 * _Color );
				float AlphaIn20_g53 = (temp_output_104_0_g51).w;
				float AlphaOut20_g53 = 0;
				float AlphaThreshold20_g53 = 0;
				sampler2D DitherNoiseTexture20_g53 = _DitherTexture;
				int DitherNoiseTextureSize20_g53 = _DitherTextureSize;
				int UseRandomDither20_g53 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g53 = _AlphaCutoutThreshold;
				float DitherBlend20_g53 = _Dithering;
				{
				float alpha = AlphaIn20_g53;
				computeOpaqueTransparency(ScreenPos20_g53, VertPos20_g53, Hash20_g53, DitherNoiseTexture20_g53, DitherNoiseTextureSize20_g53, UseRandomDither20_g53 > 0, AlphaCutoutThreshold20_g53, DitherBlend20_g53,  alpha, AlphaThreshold20_g53);
				AlphaOut20_g53 = alpha;
				}
				
				float Alpha = AlphaOut20_g53;
				float AlphaClipThreshold = AlphaThreshold20_g53;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = Depth64_g51;
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
			#define ASE_DEPTH_WRITE_ON
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define MUDBUN_URP
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/RayTracedVoxelsCommon.cginc"


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
			sampler2D _DitherTexture;


			float3 MudBunRayTracedVoxelsVertex2_g51( int Id, out float3 VertexPosLs )
			{
				float3 vertexPosWs;
				mudbun_ray_traced_voxels_vert(Id, VertexPosLs, vertexPosWs);
				return vertexPosWs;
			}
			
			float4x4 WorldToLocalMatrix69_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 WorldToLocalMatrix138_g51(  )
			{
				return worldToLocal;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				int Id2_g51 = v.ase_vertexID;
				float3 VertexPosLs2_g51 = float3( 0,0,0 );
				float3 localMudBunRayTracedVoxelsVertex2_g51 = MudBunRayTracedVoxelsVertex2_g51( Id2_g51 , VertexPosLs2_g51 );
				float3 vertexPosWs40_g51 = localMudBunRayTracedVoxelsVertex2_g51;
				float3 temp_output_127_0 = vertexPosWs40_g51;
				
				float3 vertexPosLs75_g51 = VertexPosLs2_g51;
				float3 vertexToFrag112_g51 = vertexPosLs75_g51;
				o.ase_texcoord2.yzw = vertexToFrag112_g51;
				float3 vertexToFrag50_g51 = vertexPosLs75_g51;
				o.ase_texcoord3.xyz = vertexToFrag50_g51;
				
				float3 vertexToFrag121_g51 = vertexPosWs40_g51;
				o.ase_texcoord4.xyz = vertexToFrag121_g51;
				float3 vertexToFrag27_g53 = temp_output_127_0;
				o.ase_texcoord5.xyz = vertexToFrag27_g53;
				
				o.ase_texcoord2.x = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_127_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

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

				float localMudBunRayTracedVoxelsFragment64_g51 = ( 0.0 );
				int Id64_g51 = IN.ase_texcoord2.x;
				float3 vertexToFrag112_g51 = IN.ase_texcoord2.yzw;
				float3 VertPosLs64_g51 = vertexToFrag112_g51;
				float4x4 localWorldToLocalMatrix69_g51 = WorldToLocalMatrix69_g51();
				float3 rayOriginLs72_g51 = (mul( localWorldToLocalMatrix69_g51, float4( unity_CameraToWorld[0][3],unity_CameraToWorld[1][3],unity_CameraToWorld[2][3],unity_CameraToWorld[3][3] ) )).xyz;
				float3 RayOriginLs64_g51 = rayOriginLs72_g51;
				float3 vertexToFrag50_g51 = IN.ase_texcoord3.xyz;
				float3 normalizeResult46_g51 = normalize( (( vertexToFrag50_g51 - rayOriginLs72_g51 )).xyz );
				float3 rayDirLs49_g51 = normalizeResult46_g51;
				float3 RayDirLs64_g51 = rayDirLs49_g51;
				float4x4 localWorldToLocalMatrix138_g51 = WorldToLocalMatrix138_g51();
				float3 viewDirLs137_g51 = (mul( localWorldToLocalMatrix138_g51, float4( unity_CameraToWorld[0][2],unity_CameraToWorld[1][2],unity_CameraToWorld[2][2],unity_CameraToWorld[3][2] ) )).xyz;
				float3 ViewDirLs64_g51 = viewDirLs137_g51;
				float4 Color64_g51 = float4( 0,0,0,0 );
				float3 Emission64_g51 = float3( 0,0,0 );
				float Metallic64_g51 = 0;
				float Smoothness64_g51 = 0;
				float4 TextureWeight64_g51 = float4( 0,0,0,0 );
				float3 FragmentPosLs64_g51 = float3( 0,0,0 );
				float Depth64_g51 = 0;
				float3 FragmentNormLs64_g51 = float3( 0,0,0 );
				int BrushHash64_g51 = 0;
				{
				mudbun_ray_traced_voxels_frag(Id64_g51, VertPosLs64_g51, RayOriginLs64_g51, RayDirLs64_g51, ViewDirLs64_g51, FragmentPosLs64_g51, FragmentNormLs64_g51, Depth64_g51, Color64_g51, Emission64_g51, Metallic64_g51, Smoothness64_g51, TextureWeight64_g51);
				#ifdef SHADERPASS_FORWARD
				ShadowCoords = TransformWorldToShadowCoord(mul(localToWorld, float4(FragmentPosLs64_g51, 1.0f)).xyz);
				#endif
				}
				float4 temp_output_104_0_g51 = ( Color64_g51 * _Color );
				
				float localComputeOpaqueTransparency20_g53 = ( 0.0 );
				float3 vertexToFrag121_g51 = IN.ase_texcoord4.xyz;
				float4 unityObjectToClipPos1_g52 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag121_g51));
				float4 computeScreenPos3_g52 = ComputeScreenPos( unityObjectToClipPos1_g52 );
				float2 ScreenPos20_g53 = (( ( computeScreenPos3_g52 / (computeScreenPos3_g52).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g53 = IN.ase_texcoord5.xyz;
				float3 VertPos20_g53 = vertexToFrag27_g53;
				float Hash20_g53 = (float)BrushHash64_g51;
				float AlphaIn20_g53 = (temp_output_104_0_g51).w;
				float AlphaOut20_g53 = 0;
				float AlphaThreshold20_g53 = 0;
				sampler2D DitherNoiseTexture20_g53 = _DitherTexture;
				int DitherNoiseTextureSize20_g53 = _DitherTextureSize;
				int UseRandomDither20_g53 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g53 = _AlphaCutoutThreshold;
				float DitherBlend20_g53 = _Dithering;
				{
				float alpha = AlphaIn20_g53;
				computeOpaqueTransparency(ScreenPos20_g53, VertPos20_g53, Hash20_g53, DitherNoiseTexture20_g53, DitherNoiseTextureSize20_g53, UseRandomDither20_g53 > 0, AlphaCutoutThreshold20_g53, DitherBlend20_g53,  alpha, AlphaThreshold20_g53);
				AlphaOut20_g53 = alpha;
				}
				
				
				float3 Albedo = (temp_output_104_0_g51).xyz;
				float3 Emission = 0;
				float Alpha = AlphaOut20_g53;
				float AlphaClipThreshold = AlphaThreshold20_g53;

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
			#define ASE_DEPTH_WRITE_ON
			#define ASE_ABSOLUTE_VERTEX_POS 1
			#define _ALPHATEST_ON 1
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 70301

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define MUDBUN_URP
			#define SHADER_GRAPH
			#pragma multi_compile _ MUDBUN_PROCEDURAL
			#include "Assets/MudBun/Shader/Render/ShaderCommon.cginc"
			#include "Assets/MudBun/Shader/Render/RayTracedVoxelsCommon.cginc"


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
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
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
			sampler2D _DitherTexture;


			float3 MudBunRayTracedVoxelsVertex2_g51( int Id, out float3 VertexPosLs )
			{
				float3 vertexPosWs;
				mudbun_ray_traced_voxels_vert(Id, VertexPosLs, vertexPosWs);
				return vertexPosWs;
			}
			
			float4x4 WorldToLocalMatrix69_g51(  )
			{
				return worldToLocal;
			}
			
			float4x4 WorldToLocalMatrix138_g51(  )
			{
				return worldToLocal;
			}
			

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				int Id2_g51 = v.ase_vertexID;
				float3 VertexPosLs2_g51 = float3( 0,0,0 );
				float3 localMudBunRayTracedVoxelsVertex2_g51 = MudBunRayTracedVoxelsVertex2_g51( Id2_g51 , VertexPosLs2_g51 );
				float3 vertexPosWs40_g51 = localMudBunRayTracedVoxelsVertex2_g51;
				float3 temp_output_127_0 = vertexPosWs40_g51;
				
				float3 vertexPosLs75_g51 = VertexPosLs2_g51;
				float3 vertexToFrag112_g51 = vertexPosLs75_g51;
				o.ase_texcoord2.yzw = vertexToFrag112_g51;
				float3 vertexToFrag50_g51 = vertexPosLs75_g51;
				o.ase_texcoord3.xyz = vertexToFrag50_g51;
				
				float3 vertexToFrag121_g51 = vertexPosWs40_g51;
				o.ase_texcoord4.xyz = vertexToFrag121_g51;
				float3 vertexToFrag27_g53 = temp_output_127_0;
				o.ase_texcoord5.xyz = vertexToFrag27_g53;
				
				o.ase_texcoord2.x = v.ase_vertexID;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = temp_output_127_0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

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

				float localMudBunRayTracedVoxelsFragment64_g51 = ( 0.0 );
				int Id64_g51 = IN.ase_texcoord2.x;
				float3 vertexToFrag112_g51 = IN.ase_texcoord2.yzw;
				float3 VertPosLs64_g51 = vertexToFrag112_g51;
				float4x4 localWorldToLocalMatrix69_g51 = WorldToLocalMatrix69_g51();
				float3 rayOriginLs72_g51 = (mul( localWorldToLocalMatrix69_g51, float4( unity_CameraToWorld[0][3],unity_CameraToWorld[1][3],unity_CameraToWorld[2][3],unity_CameraToWorld[3][3] ) )).xyz;
				float3 RayOriginLs64_g51 = rayOriginLs72_g51;
				float3 vertexToFrag50_g51 = IN.ase_texcoord3.xyz;
				float3 normalizeResult46_g51 = normalize( (( vertexToFrag50_g51 - rayOriginLs72_g51 )).xyz );
				float3 rayDirLs49_g51 = normalizeResult46_g51;
				float3 RayDirLs64_g51 = rayDirLs49_g51;
				float4x4 localWorldToLocalMatrix138_g51 = WorldToLocalMatrix138_g51();
				float3 viewDirLs137_g51 = (mul( localWorldToLocalMatrix138_g51, float4( unity_CameraToWorld[0][2],unity_CameraToWorld[1][2],unity_CameraToWorld[2][2],unity_CameraToWorld[3][2] ) )).xyz;
				float3 ViewDirLs64_g51 = viewDirLs137_g51;
				float4 Color64_g51 = float4( 0,0,0,0 );
				float3 Emission64_g51 = float3( 0,0,0 );
				float Metallic64_g51 = 0;
				float Smoothness64_g51 = 0;
				float4 TextureWeight64_g51 = float4( 0,0,0,0 );
				float3 FragmentPosLs64_g51 = float3( 0,0,0 );
				float Depth64_g51 = 0;
				float3 FragmentNormLs64_g51 = float3( 0,0,0 );
				int BrushHash64_g51 = 0;
				{
				mudbun_ray_traced_voxels_frag(Id64_g51, VertPosLs64_g51, RayOriginLs64_g51, RayDirLs64_g51, ViewDirLs64_g51, FragmentPosLs64_g51, FragmentNormLs64_g51, Depth64_g51, Color64_g51, Emission64_g51, Metallic64_g51, Smoothness64_g51, TextureWeight64_g51);
				#ifdef SHADERPASS_FORWARD
				ShadowCoords = TransformWorldToShadowCoord(mul(localToWorld, float4(FragmentPosLs64_g51, 1.0f)).xyz);
				#endif
				}
				float4 temp_output_104_0_g51 = ( Color64_g51 * _Color );
				
				float localComputeOpaqueTransparency20_g53 = ( 0.0 );
				float3 vertexToFrag121_g51 = IN.ase_texcoord4.xyz;
				float4 unityObjectToClipPos1_g52 = TransformWorldToHClip(TransformObjectToWorld(vertexToFrag121_g51));
				float4 computeScreenPos3_g52 = ComputeScreenPos( unityObjectToClipPos1_g52 );
				float2 ScreenPos20_g53 = (( ( computeScreenPos3_g52 / (computeScreenPos3_g52).w ) * _ScreenParams )).xy;
				float3 vertexToFrag27_g53 = IN.ase_texcoord5.xyz;
				float3 VertPos20_g53 = vertexToFrag27_g53;
				float Hash20_g53 = (float)BrushHash64_g51;
				float AlphaIn20_g53 = (temp_output_104_0_g51).w;
				float AlphaOut20_g53 = 0;
				float AlphaThreshold20_g53 = 0;
				sampler2D DitherNoiseTexture20_g53 = _DitherTexture;
				int DitherNoiseTextureSize20_g53 = _DitherTextureSize;
				int UseRandomDither20_g53 = (int)_RandomDither;
				float AlphaCutoutThreshold20_g53 = _AlphaCutoutThreshold;
				float DitherBlend20_g53 = _Dithering;
				{
				float alpha = AlphaIn20_g53;
				computeOpaqueTransparency(ScreenPos20_g53, VertPos20_g53, Hash20_g53, DitherNoiseTexture20_g53, DitherNoiseTextureSize20_g53, UseRandomDither20_g53 > 0, AlphaCutoutThreshold20_g53, DitherBlend20_g53,  alpha, AlphaThreshold20_g53);
				AlphaOut20_g53 = alpha;
				}
				
				
				float3 Albedo = (temp_output_104_0_g51).xyz;
				float Alpha = AlphaOut20_g53;
				float AlphaClipThreshold = AlphaThreshold20_g53;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}
		
	}
	/*ase_lod*/
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18909
-1610;238;1302;678;955.4334;262.7346;1.6;True;False
Node;AmplifyShaderEditor.RangedFloatNode;115;-384,1024;Inherit;False;Property;_Dithering;Dithering;7;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-384,832;Inherit;False;Property;_RandomDither;Random Dither;9;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;117;-384,928;Inherit;False;Property;_AlphaCutoutThreshold;Alpha Cutout Threshold;5;0;Create;True;0;0;0;False;0;False;0;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;118;-384,512;Inherit;True;Property;_DitherTexture;Dither Texture;6;0;Create;True;0;0;0;False;0;False;f240bbb7854046345b218811e5681a54;f240bbb7854046345b218811e5681a54;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.IntNode;119;-384,736;Inherit;False;Property;_DitherTextureSize;Dither Texture Size;8;0;Create;True;0;0;0;False;0;False;256;256;False;0;1;INT;0
Node;AmplifyShaderEditor.FunctionNode;127;-384,1;Inherit;False;Mud Ray-Traced Voxels;0;;51;8db3c5db036dbdf47979917bd2067f63;0;0;15;FLOAT3;52;FLOAT;58;FLOAT3;54;FLOAT;60;FLOAT;61;FLOAT4;62;FLOAT3;0;FLOAT3;9;FLOAT3;76;FLOAT3;77;FLOAT3;55;FLOAT3;56;FLOAT;99;FLOAT2;113;INT;114
Node;AmplifyShaderEditor.FunctionNode;120;192,384;Inherit;False;Mud Alpha Threshold;-1;;53;926535703f4c32948ac1f55275a22bf0;0;9;8;FLOAT2;0,0;False;15;FLOAT3;0,0,0;False;18;FLOAT;0;False;22;FLOAT;0;False;19;SAMPLER2D;0;False;26;INT;256;False;9;INT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;2;FLOAT;24;FLOAT;25
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;2;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;3;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;4;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;5;0,0;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1;768,0;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;Mud Ray Traced Voxels (URP);94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;4;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;2;Include;;False;;Native;Define;MUDBUN_URP;False;;Custom;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;Surface;0;  Refraction Model;0;  Blend;0;Two Sided;1;Fragment Normal Space,InvertActionOnDeselection;2;Transmission;0;  Transmission Shadow;0.5,False,-1;Translucency;0;  Translucency Strength;1,False,-1;  Normal Distortion;0.5,False,-1;  Scattering;2,False,-1;  Direct;0.9,False,-1;  Ambient;0.1,False,-1;  Shadow;0.5,False,-1;Cast Shadows;1;  Use Shadow Threshold;0;Receive Shadows;1;GPU Instancing;1;LOD CrossFade;1;Built-in Fog;1;_FinalColorxAlpha;0;Meta Pass;1;Override Baked GI;0;Extra Pre Pass;0;DOTS Instancing;0;Tessellation;0;  Phong;0;  Strength;0.5,False,-1;  Type;0;  Tess;16,False,-1;  Min;10,False,-1;  Max;25,False,-1;  Edge Length;16,False,-1;  Max Displacement;25,False,-1;Write Depth;1;  Early Z;0;Vertex Position,InvertActionOnDeselection;0;0;6;False;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;768,-192;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;2;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;120;8;127;113
WireConnection;120;15;127;0
WireConnection;120;18;127;114
WireConnection;120;22;127;58
WireConnection;120;19;118;0
WireConnection;120;26;119;0
WireConnection;120;9;116;0
WireConnection;120;6;117;0
WireConnection;120;7;115;0
WireConnection;1;0;127;52
WireConnection;1;1;127;55
WireConnection;1;3;127;60
WireConnection;1;4;127;61
WireConnection;1;6;120;24
WireConnection;1;7;120;25
WireConnection;1;17;127;99
WireConnection;1;8;127;0
ASEEND*/
//CHKSM=56FDAC96DEDEABE65A1B6D8EC5B02BEB517BC7C8