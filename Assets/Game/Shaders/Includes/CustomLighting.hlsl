#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

void MainLight_float(float3 WorldPos, out float3 Direction, out float3 Color, out float ShadowAttenuation)
{
#ifdef SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    ShadowAttenuation = 0;
#else
    #if SHADOWS_SCREEN
        float4 positionCS = TransformWorldToHClip(WorldPos);
        float4 shadowCoord = ComputeScreenPos(WorldPos);
    #else
        float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    #endif

    Light mainLight = GetMainLight(shadowCoord);

    Direction = mainLight.direction;
    Color = mainLight.color;
    ShadowAttenuation = mainLight.shadowAttenuation * mainLight.distanceAttenuation;
#endif
}

#endif