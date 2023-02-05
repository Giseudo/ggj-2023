/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_CUSTOM_RAY_TRACED_VOXELS
#define MUDBUN_CUSTOM_RAY_TRACED_VOXELS

#include "../Shader/Math/Geometry.cginc"

// return true if the ray should hit the voxel
bool ray_traced_voxels_hit_func
(
  float3 ro,         // ray origin
  float3 rd,         // ray direction (normalized)
  float3 c,          // center of voxel
  float h,           // half size of voxel
  out float3 hitPos, // position of ray hit
  out float3 hitNorm // normal at hit position
)
{
  hitPos = 0.0f;
  hitNorm = 0.0f;

  // example: box
  float t = ray_box_intersect(ro, rd, c, h).x;
  if (t >= -1e-3f)
  {
    hitPos = ro + t * rd;
    hitNorm = box_gradient(hitPos, c, h);
    return true;
  }

  return false;
}

#endif

