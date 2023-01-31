/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

// URP shader template by Felipe Lira
// https://gist.github.com/phi-lira/225cd7c5e8545be602dca4eb5ed111ba

Shader "MudBun/Mud Ray-Marched SDF (URP)"
{
  Properties
  {
    [MainColor] _BaseColor("Color", Color) = (0.5,0.5,0.5,1)
    _EmissionColor("Emission", Color) = (0,0,0)
    [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
    _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
    _ReceiveShadows("Receive Shadows", Float) = 1.0

    _MaterialNeedsSdfProperties("", Float) = 1.0
    _MaterialNeedsRayMarchingProperties("", Float) = 1.0
  }

  SubShader
  {
    Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" "IgnoreProjector" = "True"}
    LOD 300

    Pass
    {
      Name "StandardLit"
      Tags{"LightMode" = "UniversalForward"}

      ZWrite On
      ZTest LEqual
      Cull Back

      HLSLPROGRAM
      #pragma prefer_hlslcc gles
      #pragma exclude_renderers d3d11_9x
      #pragma target 4.5

      #pragma shader_feature _ALPHATEST_ON
      #pragma shader_feature _ALPHAPREMULTIPLY_ON
      #pragma shader_feature _EMISSION

      #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
      #pragma shader_feature _GLOSSYREFLECTIONS_OFF
      #pragma shader_feature _SPECULAR_SETUP
      #pragma shader_feature _RECEIVE_SHADOWS_OFF

      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
      #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
      #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
      #pragma multi_compile _ _SHADOWS_SOFT
      #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

      #pragma multi_compile_fog
      #pragma multi_compile_instancing

      #pragma vertex LitPassVertex
      #pragma fragment LitPassFragment

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

      #define MUDBUN_URP_TEMPLATE
      #define MUDBUN_RAY_MARCHING_COMPUTE_NORMAL
      #include "../../RayMarching.cginc"

      #pragma multi_compile _ MUDBUN_PROCEDURAL

      struct Attributes
      {
        float4 positionOs : POSITION;
      };

      struct Varyings
      {
        float4 positionWsAndFogFactor : TEXCOORD2; // xyz: positionWs, w: vertex fog factor
        float4 positionCs             : SV_POSITION;
      };

      Varyings LitPassVertex(Attributes input)
      {
        Varyings output;

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOs.xyz);

        float fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

        output.positionWsAndFogFactor = float4(vertexInput.positionWS, fogFactor);
        output.positionCs = vertexInput.positionCS;
        return output;
      }

      half4 LitPassFragment(Varyings input, out float depth : SV_DepthGreaterEqual) : SV_Target
      {
        float3 positionWs = input.positionWsAndFogFactor.xyz;
        float3 cameraPosWs = GetCameraPositionWS();
        float3 viewDirectionWs = SafeNormalize(cameraPosWs - positionWs);
        float3 cameraDirWs = -viewDirectionWs;

        float3 cameraPosLs = mul(worldToLocal, float4(cameraPosWs, 1.0f)).xyz;
        float3 cameraDirLs = normalize(mul(worldToLocal, float4(cameraDirWs, 0.0f)).xyz);

        SurfaceData surfaceData;
        InitializeStandardLitSurfaceData(float2(0.0f, 0.0f), surfaceData);

#ifdef MUDBUN_PROCEDURAL
        float3 normalLs;
        RayMarchResults res = ray_march_surface(cameraPosLs, cameraDirLs, normalLs);
#else
        float3 normalLs = 0.0f;
        RayMarchResults res = init_ray_march_results();
#endif

        float3 normalWs = normalize(mul(localToWorldIt, float4(normalLs, 0.0f)).xyz);

        positionWs = mul(localToWorld, float4(res.pos, 1.0f)).xyz;
        surfaceData.albedo = res.mat.color.rgb;
        surfaceData.alpha = res.mat.color.a;
        surfaceData.emission = res.mat.emissionHash.rgb;
        surfaceData.metallic = res.mat.metallicSmoothnessSizeTightness.x;
        surfaceData.smoothness = res.mat.metallicSmoothnessSizeTightness.y;

        float4 positionCs = mul(UNITY_MATRIX_VP, float4(positionWs, 1.0f));
        depth = positionCs.z / positionCs.w;

        half3 bakedGI = SampleSH(normalWs);

        BRDFData brdfData;
        InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

#ifdef _MAIN_LIGHT_SHADOWS
        float4 shadowCoord = TransformWorldToShadowCoord(positionWs);
        Light mainLight = GetMainLight(shadowCoord);
#else
        Light mainLight = GetMainLight();
#endif

        half3 color = GlobalIllumination(brdfData, bakedGI, surfaceData.occlusion, normalWs, viewDirectionWs);
        color += LightingPhysicallyBased(brdfData, mainLight, normalWs, viewDirectionWs);

#ifdef _ADDITIONAL_LIGHTS
        int additionalLightsCount = GetAdditionalLightsCount();
        for (int i = 0; i < additionalLightsCount; ++i)
        {
          Light light = GetAdditionalLight(i, positionWs);
          color += LightingPhysicallyBased(brdfData, light, normalWs, viewDirectionWs);
        }
#endif

        color += surfaceData.emission;

        float fogFactor = input.positionWsAndFogFactor.w;
        color = MixFog(color, fogFactor);

        return half4(color, surfaceData.alpha);
      }
      ENDHLSL
    }

    Pass
    {
      Name "ShadowCaster"
      Tags{"LightMode" = "ShadowCaster"}

      ZWrite On
      ZTest LEqual
      Cull Back
      ColorMask 0

      HLSLPROGRAM
      #pragma prefer_hlslcc gles
      #pragma exclude_renderers d3d11_9x
      #pragma target 4.5

      #pragma multi_compile_instancing

      #pragma vertex ShadowPassVertex
      #pragma fragment ShadowPassFragment

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

      #define MUDBUN_URP_TEMPLATE
      #include "../../RayMarching.cginc"

      #pragma multi_compile _ MUDBUN_PROCEDURAL

      struct Attributes
      {
        float4 positionOs : POSITION;
      };

      struct Varyings
      {
        float4 positionWs : TEXCOORD2;
        float4 positionCs : SV_POSITION;
      };

      Varyings ShadowPassVertex(Attributes input)
      {
        Varyings output;

        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOs.xyz);

        output.positionWs = float4(vertexInput.positionWS, 1.0f);
        output.positionCs = vertexInput.positionCS;
        return output;
      }

      half4 ShadowPassFragment(Varyings input, out float depth : SV_DepthGreaterEqual) : SV_Target
      {
        float3 positionWs = input.positionWs.xyz;
        float3 lightDirWs = SafeNormalize(_MainLightPosition.xyz);
        float3 lightPosWs = positionWs - 100.0f * lightDirWs; // TODO: more robust ray origin

        float3 lightPosLs = mul(worldToLocal, float4(lightPosWs, 1.0f)).xyz;
        float3 lightDirLs = normalize(mul(worldToLocal, float4(lightDirWs, 0.0f)).xyz);

#ifdef MUDBUN_PROCEDURAL
        float3 normalLs;
        RayMarchResults res = ray_march_surface(lightPosWs, lightDirLs, normalLs);
#else
        RayMarchResults res = init_ray_march_results();
#endif

        positionWs = mul(localToWorld, float4(res.pos, 1.0f)).xyz;
        float4 positionCs = mul(UNITY_MATRIX_VP, float4(positionWs, 1.0f));
        depth = positionCs.z / positionCs.w;

        return float4(0.0f, 0.0f, 0.0f, 1.0f);
      }
      ENDHLSL
    }
  }
}
