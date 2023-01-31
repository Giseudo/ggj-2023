/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_RAY_TRACED_VOXELS_COMMON
#define MUDBUN_RAY_TRACED_VOXELS_COMMON

#ifdef MUDBUN_VALID

#include "../../Customization/CustomRayTracedVoxels.cginc"
#include "../AabbTreeFuncs.cginc"
#include "../BrushFuncs.cginc"
#include "../GenPointDefs.cginc"
#include "../Math/Codec.cginc"
#include "../Math/Geometry.cginc"
#include "../Math/Vector.cginc"
#include "../Math/Quaternion.cginc"
#include "../MeshingModeDefs.cginc"
#include "../Noise/ClassicNoise3D.cginc"
#include "../Noise/SimplexNoise3D.cginc"
#include "../Noise/RandomNoise.cginc"
#include "../NormalFuncs.cginc"
#include "../RenderModeDefs.cginc"
#include "../SDF/Util.cginc"
#include "../VoxelDefs.cginc"
#include "../VoxelHashDefs.cginc"
#include "../VoxelModeDefs.cginc"

#endif

float rayTracedVoxelSizeMultiplier;
float rayTracedVoxelSmoothCubeNormal;
float rayTracedVoxelRadius;

int3 ray_step(float3 ro, float3 s, float3 m, float3 k, float3 nodeCenter)
{
  ro -= nodeCenter;
  float3 n = m * ro;
  float3 tMax = -n + k;
  float3 tMaxNeg = -tMax;
  return s * step(tMaxNeg.yzx, tMaxNeg.xyz) * step(tMaxNeg.zxy, tMaxNeg.xyz);
}

static const float3 aUnitBoxVertLs[8] = 
{
  float3(-0.5f, -0.5f, -0.5f), 
  float3( 0.5f, -0.5f, -0.5f), 
  float3( 0.5f, -0.5f,  0.5f), 
  float3(-0.5f, -0.5f,  0.5f), 
  float3(-0.5f,  0.5f, -0.5f), 
  float3( 0.5f,  0.5f, -0.5f), 
  float3( 0.5f,  0.5f,  0.5f), 
  float3(-0.5f,  0.5f,  0.5f), 
};

static const int aiUnitBoxTriVert[36] = 
{
  0, 1, 2, 0, 2, 3,
  3, 2, 6, 3, 6, 7,
  7, 6, 5, 7, 5, 4,
  4, 5, 1, 4, 1, 0,
  1, 5, 6, 1, 6, 2,
  0, 3, 7, 0, 7, 4,
};

static const int aiInvertedUnitBoxTriVert[36] = 
{
  0, 2, 1, 0, 3, 2, 
  3, 6, 2, 3, 7, 6, 
  7, 5, 6, 7, 4, 5, 
  4, 1, 5, 4, 0, 1, 
  1, 6, 5, 1, 2, 6, 
  0, 7, 3, 0, 4, 7, 
};

void mudbun_ray_traced_voxels_vert
(
  uint id, 
  out float3 vertPosLs, 
  out float3 vertPosWs
)
{
#ifdef MUDBUN_VALID

  uint iChunk = id % 36;

  Aabb rootBounds = aabbTree[aabbRoot].aabb;

  #if !defined(SHADERPASS_SHADOWCASTER)
    vertPosLs = aUnitBoxVertLs[aiInvertedUnitBoxTriVert[iChunk]];
  #else
    vertPosLs = aUnitBoxVertLs[aiUnitBoxTriVert[iChunk]];
  #endif

  vertPosLs *= voxelNodeSizes[0];
  vertPosLs += nodePool[id / 36].center;
  vertPosLs = clamp(vertPosLs, rootBounds.boundsMin, rootBounds.boundsMax);

  vertPosWs = mul(localToWorld, float4(vertPosLs, 1.0f)).xyz;

#else

  vertPosLs = 0.0f;
  vertPosWs = 0.0f;

#endif
}

float3 msign(float3 v)
{
  return v >= 0.0f ? float3(1.0f, 1.0f, 1.0f) : float3(-1.0f, -1.0f, -1.0f);
}

void mudbun_ray_traced_voxels_frag
(
  uint id,
  float3 vertPosLs,
  float3 rayOriginLs, 
  float3 rayDirLs, 
  float3 viewDirLs, 
  out float3 posLs, 
  out float3 normLs, 
  out float depth, 
  out float4 color, 
  out float3 emission, 
  out float metallic, 
  out float smoothness, 
  out float4 textureWeight
)
{
#ifdef MUDBUN_VALID

  posLs = 0.0f;
  normLs = 0.0f;
  depth = 0.0f;
  color = 0.0f;
  emission = 0.0f;
  metallic = 0.0f;
  smoothness = 0.0f;
  textureWeight = 0.0f;

  uint iChunk = id / 36;

  float4 aNodeExtent = 0.5f * voxelNodeSizes;
  float4 aVoxelNodeSizeInv = rcp(voxelNodeSizes) * 0.9999999f; // make sure cell node coords are not out-of-bounds
  uint4 aBranchingFactor = get_voxel_tree_branching_factors();
  uint4 aHalfBranchingFactor = aBranchingFactor / 2;
  uint4 aMaxSteps = 3 * aBranchingFactor;
  float voxelExtent = aNodeExtent[3];
  float voxelHalfDiag = 1.733f * voxelExtent;

  float3 ro = rayOriginLs;
  float3 rd = rayDirLs;

  #if defined(SHADERPASS_SHADOWCASTER)
    #if defined(MUDBUN_URP)
      rd = normalize(mul((float3x3) worldToLocalIt, _MainLightPosition.xyz));
      ro = vertPosLs + 0.01f * voxelNodeSizes[3] * rd;
      rayOriginLs = ro - 1.733f * voxelNodeSizes[0] * rd;
    #endif
    // TODO: HDRP
  #endif

  float3 rayS = msign(rd);
  float3 rayM = (abs(rd) > 1e-6f) ? rcp(rd) : kFltMax;
  float3 absRayM = abs(rayM);
  float3 rayK0 = absRayM * aNodeExtent[0];
  float3 rayK1 = absRayM * aNodeExtent[1];
  float3 rayK2 = absRayM * aNodeExtent[2];
  float3 rayK3 = absRayM * aNodeExtent[3];

  float3 rayNudge0 = 0.01f * voxelNodeSizes[3] * rd;
  float3 rayNudge1 = 0.01f * voxelNodeSizes[3] * rd;
  float3 rayNudge2 = 0.01f * voxelNodeSizes[3] * rd;
  float3 rayNudge3 = 0.01f * voxelNodeSizes[3] * rd;

  float3 center0 = nodePool[iChunk].center;
  float3 chunkOrigin = center0 - aNodeExtent[0];

  // quantize ro at node 0 boundaries
  ro += ray_box_intersect_fast_raw(ro, rayM, rayK0, center0).x * rd;

  // depth 1
  uint key0 = nodePool[iChunk].key;
  float3 rw1 = ro; // ray walker
  int3 coord1 = (rw1 - chunkOrigin + rayNudge1) * aVoxelNodeSizeInv[1] % aBranchingFactor[0];
  float3 center1 = (coord1 + 0.5f - aHalfBranchingFactor[0]) * voxelNodeSizes[1] + center0;
  [loop] for(int iter1 = 0, maxIter1 = aMaxSteps[0]; iter1 < maxIter1; ++iter1)
  {
    uint key1 = concat_node_key(key0, coord1);
    int iNode1 = look_up_node(key1);

    // hit node at depth 1?
    if (iNode1 >= 0)
    {
      // depth 2
      float3 rw2 = rw1;
      int3 coord2 = ((rw2 - chunkOrigin + rayNudge2) * aVoxelNodeSizeInv[2]) % aBranchingFactor[1];
      float3 center2 = (coord2 + 0.5f - aHalfBranchingFactor[1]) * voxelNodeSizes[2] + center1;
      [loop] for (int iter2 = 0, maxIter2 = aMaxSteps[1]; iter2 < maxIter2; ++iter2)
      {
        uint key2 = concat_node_key(key1, coord2);
        int iNode2 = look_up_node(key2);

        // hit node at depth 2?
        if (iNode2 >= 0)
        {
          // depth 3
          float3 rw3 = rw2;
          int3 coord3 = ((rw3 - chunkOrigin + rayNudge3) * aVoxelNodeSizeInv[3]) % aBranchingFactor[2];
          float3 center3 = (coord3 + 0.5f - aHalfBranchingFactor[2]) * voxelNodeSizes[3] + center2;
          [loop] for (int iter3 = 0, maxIter3 = aMaxSteps[2]; iter3 < maxIter3; ++iter3)
          {
            uint key3 = concat_node_key(key2, coord3);
            int iNode3 = look_up_node(key3);

            // hit node at depth 3?
            // voxel hit test
            if (iNode3 >= 0)
            {
              // in front of ray origin?
              float3 voxelCenter = nodePool[iNode3].center;
              if (dot(voxelCenter - rayOriginLs, rd) > 0.0f)
              {
                bool hitVoxel = false;

                float sizeMult = saturate(rayTracedVoxelSizeMultiplier * aGenPoint[iNode3].material.size);
                switch (rayTracedVoxelPaddingMode)
                {
                  case kVoxelPaddingModeByDistance:
                  case kVoxelPaddingModeFull:
                    if (rayTracedVoxelSizeFadeDistance > kEpsilon)
                    {
                      sizeMult *= saturate(-aGenPoint[iNode3].sdfValue / rayTracedVoxelSizeFadeDistance);
                    }
                    break;
                }

                switch (rayTracedVoxelMode)
                {
                case kVoxelModeFlatCubes:
                case kVoxelModeFacetedCubes:
                  {
                    float t = ray_box_intersect_fast(ro, rayM, rayK3 * sizeMult, voxelCenter).x;
                    if (t >= -1e-3f)
                    {
                      posLs = ro + t * rd;
                      normLs = 
                        (rayTracedVoxelMode == kVoxelModeFlatCubes)
                          ? unpack_normal(aGenPoint[iNode3].posNorm.w)
                          : box_gradient(posLs, voxelCenter, voxelExtent * sizeMult * (1.0f - rayTracedVoxelSmoothCubeNormal));
                      hitVoxel = true;
                    }
                  }
                  break;

                case kVoxelModeSmoothSpheres:
                case kVoxelModeFlatSpheres:
                  {
                    float2 tSphere = ray_sphere_intersect(ro - voxelCenter, rd, voxelExtent * rayTracedVoxelRadius * sizeMult);
                    float2 tBox = ray_box_intersect_fast(ro, rayM, rayK3 * sizeMult, voxelCenter);
                    if (tBox.x >= -1e-3f 
                        && tSphere.x <= tBox.y 
                        && tBox.x <= tSphere.y)
                    {
                      float t = max(tSphere.x, tBox.x);
                      posLs = ro + t * rd;
                      normLs = 
                        (rayTracedVoxelMode == kVoxelModeFlatSpheres) 
                          ? unpack_normal(aGenPoint[iNode3].posNorm.w) 
                          : (tSphere.x >= tBox.x) 
                            ? sphere_gradient(posLs - voxelCenter) 
                            : box_gradient(posLs, voxelCenter, voxelExtent * sizeMult);
                      hitVoxel = true;
                    }
                  }
                  break;

                case kVoxelModeCustom:
                  hitVoxel = ray_traced_voxels_hit_func(ro, rd, voxelCenter, voxelExtent, posLs, normLs);
                  break;
                }

                if (hitVoxel)
                {
                #if !defined(SHADERPASS_SHADOWCASTER)
                  #if defined(MUDBUN_URP)
                  {
                    color = unpack_rgba(aGenPoint[iNode3].material.color);
                    emission = unpack_rgba(aGenPoint[iNode3].material.emissionTightness).rgb;

                    float2 metallicSmoothness = unpack_saturated(aGenPoint[iNode3].material.metallicSmoothness);
                    metallic = metallicSmoothness.x;
                    smoothness = metallicSmoothness.y;

                    textureWeight = unpack_rgba(aGenPoint[iNode3].material.textureWeight);

                    float4 posWs = mul(localToWorld, float4(posLs, 1.0f));
                    float4 posCs = mul(UNITY_MATRIX_VP, posWs);
                    depth = clamp(posCs.z / posCs.w, 1e-6f, 1.0f);
                  }
                  #endif
                  // TODO: HDRP
                #else // shadow pass
                  #if defined (MUDBUN_URP)
                  {
                    color = unpack_rgba(aGenPoint[iNode3].material.color);

                    float4 posWs = mul(localToWorld, float4(posLs, 1.0f));
                    float3 normWs = mul((float3x3) localToWorldIt, normLs);
                    float invNdotL = 1.0f - saturate(dot(_MainLightPosition.xyz, normWs));
                    posWs.xyz += _MainLightPosition.xyz * _ShadowBias.x;
                    posWs.xyz += invNdotL * _ShadowBias.y * normWs;
                    float4 posCs = mul(UNITY_MATRIX_VP, posWs);
                    depth = clamp(posCs.z / posCs.w, 1e-6f, 1.0f);
                  }
                  #endif
                  // TODO: HDRP
                #endif

                  return;
                }
              } // end: in front of ray origin?
            } // end: voxel hit test

            int3 coordStep3 = ray_step(ro, rayS, rayM, rayK3, center3);
            coord3 += coordStep3;
            if (any(coord3 < 0 || coord3 >= int(aBranchingFactor[2])))
              break;

            float tNextRw3 = ray_box_intersect_fast(ro, rayM, rayK3, center3).y;
            rw3 = ro + tNextRw3 * rd;
            center3 += coordStep3 * voxelNodeSizes[3];
          } // end: depth 3
        }

        int3 coordStep2 = ray_step(ro, rayS, rayM, rayK2, center2);
        coord2 += coordStep2;
        if (any(coord2 < 0 || coord2 >= int(aBranchingFactor[1])))
          break;

        float tNextRw2 = ray_box_intersect_fast(ro, rayM, rayK2, center2).y;
        rw2 = ro + tNextRw2 * rd;
        center2 += coordStep2 * voxelNodeSizes[2];
      } // end: depth 2
    }

    int3 coordStep1 = ray_step(ro, rayS, rayM, rayK1, center1);
    coord1 += coordStep1;
    if (any(coord1 < 0 || coord1 >= int(aBranchingFactor[0])))
      break;

    float tNextRw1 = ray_box_intersect_fast(ro, rayM, rayK1, center1).y;
    rw1 = ro + tNextRw1 * rd;
    center1 += coordStep1 * voxelNodeSizes[1];
  } // end: depth 1

#else

  posLs = 0.0f;
  normLs = 0.0f;
  depth = 0.0f;
  color = 0.0f;
  emission = 0.0f;
  metallic = 0.0f;
  smoothness = 0.0f;
  textureWeight = 0.0f;

#endif
}

#endif

