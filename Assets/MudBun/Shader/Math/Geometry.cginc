/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_GEOMETRY
#define MUDBUN_GEOMETRY

#include "MathConst.cginc"
#include "Vector.cginc"

float ray_plane_intersect(float3 ro, float3 rd, float3 n)
{
  float rdn = dot(rd, n);
  return (abs(rdn) > kEpsilon) ? (dot(-ro, n) / rdn) : -1.0f;
}

float3 plane_gradient(float3 p, float3 n)
{
  return sign(dot(p, n)) * n;
}

// https://link.springer.com/content/pdf/10.1007%2F978-1-4842-4427-2_7.pdf
float2 ray_sphere_intersect(float3 ro, float3 rd, float r)
{
  float b = dot(ro, rd); b += b;
  float3 v = ro - dot(ro, rd) * rd;
  float d = 4.0f * dot(rd, rd) * (r * r - dot(v, v));
  if (d <= 0.0f)
    return float2(-1.0f, -1.0f);

  float2 t = 0.5f * (-b + float2(1.0f, -1.0f) * sqrt(d));
  return (t.x <= t.y) ? t : t.yx;
}

float3 sphere_gradient(float3 ro)
{
  return normalize(ro);
}

// https://www.iquilezles.org/www/articles/boxfunctions/boxfunctions.htm
float2 ray_box_intersect_fast_raw(float3 ro, float3 m, float3 k, float3 boxCenter)
{
  ro -= boxCenter;
  float3 n = m * ro;
  bool3 mValid = (m < kFltMax);
  float3 tMin = mValid ? (-n - k) : -kFltMax;
  float3 tMax = mValid ? (-n + k) : kFltMax;
  float tNear = max_comp(tMin);
  float tFar = min_comp(tMax);
  return float2(tNear, tFar);
}

float2 ray_box_intersect_fast(float3 ro, float3 m, float3 k, float3 boxCenter)
{
  ro -= boxCenter;
  float3 n = m * ro;
  bool3 mValid = (m < kFltMax);
  float3 tMin = mValid ? (-n - k) : -kFltMax;
  float3 tMax = mValid ? (-n + k) : kFltMax;
  float tNear = max_comp(tMin);
  float tFar = min_comp(tMax);
  return (tNear <= tFar && tFar >= 0.0f) ? float2(tNear, tFar) : float2(-1.0f, -1.0f);
}

float2 ray_box_intersect(float3 ro, float3 rd, float3 boxCenter, float3 boxExtents)
{
  float3 m = 1.0f / rd;
  float3 k = abs(m) * boxExtents;
  return ray_box_intersect_fast(ro, m, k, boxCenter);
}

float3 box_gradient(float3 p, float3 boxCenter, float3 boxExtents)
{
  p -= boxCenter;
  float3 d = abs(p) - boxExtents;
  float3 s = p >= 0.0f ? float3(1.0f, 1.0f, 1.0f) : float3(-1.0f, -1.0f, -1.0f);
  float g = max_comp(d);
  return s * ((g > 0.0) ? normalize(max(d, 0.0f)) : step(d.yzx, d.xyz) * step(d.zxy, d.xyz));
}

#endif
