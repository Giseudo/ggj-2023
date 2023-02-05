/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_RAY_MARCHING
#define MUDBUN_RAY_MARCHING

#include "Render/ShaderCommon.cginc"

#define kLightTypeDirectional (1)
#define kLightTypePoint       (2)

#if MUDBUN_VALID

#include "BrushFuncs.cginc"

int maxRayMarchSteps;
float rayMarchHitDistance;
float rayMarchMaxRayDistance;

int numLightMarchSteps;
float rayMarchStepSize;
float rayMarchVolumeDensity;
float4 rayMarchLightPositionType;
float4 rayMarchLightDirection;
float4 rayMarchAbsorption; // x: volume, y: light
float rayMarchDarknesThreshold;
float rayMarchTransmittanceCurve;

float rayMarchNoiseThreshold;
float rayMarchNoiseEdgeFade;
float4 rayMarchNoiseScrollSpeed;
float4 rayMarchNoiseBaseOctaveSize;
int rayMarchNoiseNumOctaves;
float rayMarchNoiseOctaveOffsetFactor;

struct RayMarchResults
{
  bool hit;
  float3 pos;
  SdfBrushMaterial mat;
};

RayMarchResults init_ray_march_results()
{
    RayMarchResults res;
    res.hit = false;
    res.pos = 0.0f;
    res.mat = init_brush_material();

    return res;
}

#define SDF_RAY_MARCH_MASKED_BRUSHES(res, p, brushMask, mat, outputMat)        \
{                                                                              \
  int iStack = -1;                                                             \
  float3 pStack[kMaxBrushGroupDepth];                                          \
  float resStack[kMaxBrushGroupDepth];                                         \
  SdfBrushMaterial matStack[kMaxBrushGroupDepth];                              \
                                                                               \
  res = kInfinity;                                                             \
  mat = init_brush_material();                                                 \
  float3 groupP = p;                                                           \
  float groupRes = kInfinity;                                                  \
  SdfBrushMaterial groupMat = init_brush_material();                           \
  FOR_EACH_BRUSH_EXTERN_MASK(brushMask, /* TODO: skip brushes whose AABBs do not contain p */ \
    switch (aBrush[iBrush].type)                                               \
    {                                                                          \
      case kSdfBeginGroup:                                                     \
        iStack = min(kMaxBrushGroupDepth - 1, iStack + 1);                     \
        pStack[iStack] = p;                                                    \
        resStack[iStack] = res;                                                \
        matStack[iStack] = mat;                                                \
        res = kInfinity;                                                       \
        mat = init_brush_material();                                           \
        break;                                                                 \
      case kSdfEndGroup:                                                       \
        groupP = p;                                                            \
        groupRes = res;                                                        \
        groupMat = mat;                                                        \
        p = pStack[iStack];                                                    \
        res = resStack[iStack];                                                \
        mat = matStack[iStack];                                                \
        break;                                                                 \
    }                                                                          \
    res = sdf_brush_apply(res, groupRes, groupMat, groupP, aBrush[iBrush], mat, outputMat); \
    switch (aBrush[iBrush].type)                                               \
    {                                                                          \
      case kSdfEndGroup:                                                       \
        iStack = max(-1, iStack - 1);                                          \
        break;                                                                 \
    }                                                                          \
  );                                                                           \
}

// macro that generates less inline code
#define SDF_RAY_MARCH_NORMAL(normal, p, brushMask, h)                           \
  {                                                                             \
    float3 aSign[4] =                                                           \
    {                                                                           \
      float3( 1.0f, -1.0f, -1.0f),                                              \
      float3(-1.0f, -1.0f,  1.0f),                                              \
      float3(-1.0f,  1.0f, -1.0f),                                              \
      float3( 1.0f,  1.0f,  1.0f),                                              \
    };                                                                          \
    float3 aDelta[4] =                                                          \
    {                                                                           \
      float3( (h), -(h), -(h)),                                                 \
      float3(-(h), -(h),  (h)),                                                 \
      float3(-(h),  (h), -(h)),                                                 \
      float3( (h * 1.0001f), (h * 1.0002f), (h * 1.0003f)),                     \
    };                                                                          \
    float3 ss = 0.0f;                                                           \
    SdfBrushMaterial nmat;                                                      \
    [loop] for (int iDelta = 0; iDelta < 4; ++iDelta)                           \
    {                                                                           \
      float rr = 0.0f;                                                          \
      float3 pp = p + aDelta[iDelta];                                           \
      SDF_RAY_MARCH_MASKED_BRUSHES(rr, pp, brushMask, nmat, false);             \
      ss += aSign[iDelta] * rr;                                                 \
    }                                                                           \
    normal = normalize_safe(ss, float3(0.0f, 0.0f, 0.0f));                      \
  }

float3 ray_march_aabb_extents(Aabb aabb)
{
  // TODO: use max blend
  aabb.boundsMin -= 5.0f;
  aabb.boundsMax += 5.0f;

  return 0.5f * (aabb.boundsMax - aabb.boundsMin);
}

bool ray_march_aabb_intersects(Aabb a, Aabb b)
{
  // TODO: use max blend
  a.boundsMin -= 5.0f;
  a.boundsMax += 5.0f;

  return all(a.boundsMin <= b.boundsMax && a.boundsMax >= b.boundsMin);
}

float2 ray_march_aabb_ray_cast(Aabb aabb, float3 from, float3 to)
{
  // TODO: use max blend
  aabb.boundsMin -= 5.0f;
  aabb.boundsMax += 5.0f;

  float tMin = -kFltMax;
  float tMax = +kFltMax;

  float3 d = to - from;
  float3 absD = abs(d);
  bool3 isZero = absD < kEpsilon;

  // parallel?
  if (any(isZero && ((from < aabb.boundsMin) || (aabb.boundsMax < from))))
    return -kFltMax;

  float3 invD = sign(d) / max(kEpsilon, absD);
  float3 t1 = (aabb.boundsMin - from) * invD;
  float3 t2 = (aabb.boundsMax - from) * invD;
  float3 minComps = isZero ? (-kFltMax) : min(t1, t2);
  float3 maxComps = isZero ? (+kFltMax) : max(t1, t2);

  tMin = max(minComps.x, max(minComps.y, minComps.z));
  tMax = min(maxComps.x, min(maxComps.y, maxComps.z));

  if (tMin > tMax)
    return -kFltMax;

  if (tMin > 1.0f)
    return -kFltMax;

  return float2(max(0.0f, tMin), min(1.0f, tMax));
}

// stmt = statements processing "iData" of hit leaf AABB nodes
// will gracefully handle maxed-out stacks
#define RAY_MARCH_AABB_TREE_RAY_CAST(tree, root, rayFrom, rayTo, stmt)         \
{                                                                              \
  float3 rayDir = normalize_safe(rayTo - rayFrom, kUnitZ);                     \
  float3 rayDirOrtho = normalize_safe(find_ortho(rayDir), kUnitX);             \
  float3 rayDirOrthoAbs = abs(rayDirOrtho);                                    \
                                                                               \
  Aabb rayBounds;                                                              \
  rayBounds.boundsMin = min(rayFrom, rayTo);                                   \
  rayBounds.boundsMax = max(rayFrom, rayTo);                                   \
                                                                               \
  int stackTop = 0;                                                            \
  int stack[kAabbTreeNodeStackSize];                                           \
  stack[stackTop] = root;                                                      \
                                                                               \
  int numIters = 0;                                                            \
  while (stackTop >= 0 && numIters < 128 /* safeguard */)                      \
  {                                                                            \
    int index = stack[stackTop--];                                             \
    if (index < 0)                                                             \
      continue;                                                                \
                                                                               \
    if (!ray_march_aabb_intersects(tree[index].aabb, rayBounds))               \
      continue;                                                                \
                                                                               \
    float3 aabbCenter = aabb_center(tree[index].aabb);                         \
    float3 aabbHalfExtents = ray_march_aabb_extents(tree[index].aabb);         \
    float separation =                                                         \
      abs(dot(rayDirOrtho, rayFrom - aabbCenter))                              \
      - dot(rayDirOrthoAbs, aabbHalfExtents);                                  \
    if (separation > 0.0f)                                                     \
      continue;                                                                \
                                                                               \
    float2 t = ray_march_aabb_ray_cast(tree[index].aabb, rayFrom, rayTo);      \
    if (t.x < 0.0f)                                                            \
        continue;                                                              \
                                                                               \
    if (tree[index].iChildA < 0)                                               \
    {                                                                          \
      int iData = tree[index].iData;                                           \
                                                                               \
      stmt                                                                     \
    }                                                                          \
    else                                                                       \
    {                                                                          \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);                \
      stack[stackTop] = tree[index].iChildA;                                   \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);                \
      stack[stackTop] = tree[index].iChildB;                                   \
    }                                                                          \
  }                                                                            \
}

float sample_noise(float3 p, float sdfSample, float detailWeight = 0.0f, float detailScale = 0.5f, float detailOffsetScale = 2.0f)
{
  if (rayMarchNoiseThreshold < kEpsilon)
    return 1.0f;

  // base noise
  float base = 
    cached_noise
    (
      p / rayMarchNoiseBaseOctaveSize.xyz 
      + rayMarchNoiseScrollSpeed.xyz * _Time.y
    );

  float n = base;
  if (detailWeight > kEpsilon)
  {
    float detail = 
      cached_noise
      (
        p / (rayMarchNoiseBaseOctaveSize.xyz * detailScale) 
        + rayMarchNoiseScrollSpeed.xyz * _Time.y * detailOffsetScale
      );

    n = (base + detailWeight * detail) / (1.0f + max(0.0f, detailWeight));
  }
  n += 0.5f; // normalize to [0, 1]

  // apply threshold
  n = saturate(saturate(1.4f * n) - rayMarchNoiseThreshold);
  //n = rayMarchNoiseThreshold - saturate(1.4f * n);
  n = lerp(0.0f, n, saturate(10.0f * rayMarchNoiseThreshold));

  return n;
}

float sample_density(float3 p, float sdfSample, float detailWeight = 0.0f, float detailScale = 0.5f, float detailOffsetScale = 2.0f)
{
  if (rayMarchNoiseThreshold < kEpsilon)
    return -sdfSample;

  if (rayMarchNoiseEdgeFade < kEpsilon)
    return -sdfSample;

  float n = sample_noise(p, sdfSample, detailWeight, detailScale, detailOffsetScale);
  float noiseWeight = 1.0f - saturate(-sdfSample / rayMarchNoiseEdgeFade);
  float w = lerp(sdfSample, n, noiseWeight);
  //sdfSample = sdf_int_smooth(sdfSample, w, 1.0f);

  float density = n * (-sdfSample);
  density = density * pow(density, max(kEpsilon, rayMarchTransmittanceCurve));

  return density;
}

float light_transmittance(float3 rayFrom, float3 rayDirection)
{
  BRUSH_MASK(brushMask);
  BRUSH_MASK_CLEAR_ALL(brushMask);

  float3 rayTo = rayFrom + rayMarchMaxRayDistance * rayDirection;
  float tMin = kFltMax;
  float tMax = -kFltMax;
  RAY_MARCH_AABB_TREE_RAY_CAST(aabbTree, aabbRoot, rayFrom, rayTo, 
    BRUSH_MASK_SET(brushMask, iData);
    tMin = min(tMin, t.x);
    tMax = max(tMax, t.y);
  );
  float rayDist = (tMax - tMin) * rayMarchMaxRayDistance;

  float transmittance = 1.0f;
  float stepSize = rayDist / numLightMarchSteps;
  float3 rayStep = rayDirection * stepSize;
  float3 p = rayFrom;
  [loop] for (int iStep = 0; iStep < numLightMarchSteps; ++iStep)
  {
      p += rayStep;

      float sdfSample = 0.0f;
      SdfBrushMaterial mat;
      SDF_RAY_MARCH_MASKED_BRUSHES(sdfSample, p, brushMask, mat, true);
      if (sdfSample < 0.0f)
      {
        float density = rayMarchVolumeDensity * sample_density(p, sdfSample);
        transmittance *= exp(-density * stepSize * rayMarchAbsorption.y);

        if (transmittance < 0.1f)
        {
          transmittance = 0.0f;
          break;
        }
      }
  }

  return rayMarchDarknesThreshold + (1.0f - rayMarchDarknesThreshold) * transmittance;
}


// https://github.com/TheAllenChou/unity-ray-marching/blob/master/unity-ray-marching/Assets/Shader/Ray%20Marching/Resources/RayMarcherCs.compute
RayMarchResults ray_march_surface
(
  float3 rayFrom, 
  float3 rayDirection, 
  out float3 normal
)
{
  RayMarchResults results;
  results.hit = false;
  results.pos = rayFrom;
  results.mat = init_brush_material();

  normal = -rayDirection;

  BRUSH_MASK(brushMask);
  BRUSH_MASK_CLEAR_ALL(brushMask);

  // gather shapes around ray by casting it against AABB tree
  float3 rayTo = rayFrom + rayMarchMaxRayDistance * rayDirection;
  float tMin = kFltMax;
  float tMax = -kFltMax;
  RAY_MARCH_AABB_TREE_RAY_CAST(aabbTree, aabbRoot, rayFrom, rayTo, 
    BRUSH_MASK_SET(brushMask, iData);
    tMin = min(tMin, t.x);
    tMax = max(tMax, t.y);
  );
  float rayDist = (tMax - tMin) * rayMarchMaxRayDistance;

  // miss any AABB (tMin > 1.0f) ?
  clip(1.0f - tMin);

  // start at ray's earliest intersection with AABB tree
  float3 p = lerp(rayFrom, rayTo, tMin);

  // march ray
  float dist = 0.0f;
  [loop] for (int iStep = 0; iStep < maxRayMarchSteps; ++iStep)
  {
    float sdfSample = 0.0f;
    SDF_RAY_MARCH_MASKED_BRUSHES(sdfSample, p, brushMask, results.mat, false);
    if (sdfSample < rayMarchHitDistance)
    {
      SDF_RAY_MARCH_MASKED_BRUSHES(sdfSample, p, brushMask, results.mat, true);

#ifdef MUDBUN_RAY_MARCHING_COMPUTE_NORMAL
      SDF_RAY_MARCH_NORMAL(normal, p, brushMask, 1e-3f);
#endif

      results.hit = true;
      results.pos = p;
      break;
    }
    
    p += sdfSample * rayDirection;
    dist += sdfSample;

    if (dist > rayMarchMaxRayDistance || iStep == maxRayMarchSteps - 1)
      discard;
  }

  if (!results.hit)
    discard;

  return results;
}

// https://github.com/TheAllenChou/unity-ray-marching/blob/master/unity-ray-marching/Assets/Shader/Ray%20Marching/Resources/RayMarcherCs.compute
// https://shaderbits.com/blog/creating-volumetric-ray-marcher
// https://github.com/SebLague/Clouds/blob/master/Assets/Scripts/Clouds/Shaders/Clouds.shader
RayMarchResults ray_march_volume
(
  float3 rayFrom, 
  float3 rayDirection, 
  float3 backgroundColor, 
  sampler2D ditherTexture, 
  int ditherTextureSize, 
  float2 screenPos
)
{
  RayMarchResults results;
  results.hit = false;
  results.pos = rayFrom;
  results.mat = init_brush_material();





  // TODO: this is temp
  return results;





  BRUSH_MASK(brushMask);
  BRUSH_MASK_CLEAR_ALL(brushMask);

  // gather shapes around ray by casting it against AABB tree
  float3 rayTo = rayFrom + rayMarchMaxRayDistance * rayDirection;
  float tMin = kFltMax;
  float tMax = -kFltMax;
  RAY_MARCH_AABB_TREE_RAY_CAST(aabbTree, aabbRoot, rayFrom, rayTo, 
    BRUSH_MASK_SET(brushMask, iData);
    tMin = min(tMin, t.x);
    tMax = max(tMax, t.y);
  );
  float rayDist = (tMax - tMin) * rayMarchMaxRayDistance;

  // miss any AABB (tMin > 1.0f) ?
  clip(1.0f - tMin);

  // ray march step size is step size along camera direction
  // actual ray step size is different for each ray
  float3 camDir = mul(unity_CameraToWorld, float4(0.0f, 0.0f, 1.0f, 0.0f)).xyz;
  float actualStepSize = rayMarchStepSize / dot(camDir, rayDirection);
  float3 rayStep = rayDirection * actualStepSize;

  // start at ray's earliest intersection with AABB tree
  float3 p = lerp(rayFrom, rayTo, tMin);

  // snap ray start position to view-aligned planes
  //p -= fmod(dot(p - rayFrom, rayDirection), actualStepSize) * rayDirection;

  // jitter start position
  p -= tex2D(ditherTexture, screenPos / ditherTextureSize).r * rayStep;

  // march ray
  float transmittance = 1.0f;
  float3 lightEnergy = 0.0f;
  float3 toLightDir = -rayMarchLightDirection.xyz;
  int numRayMarchSteps = min(maxRayMarchSteps, ceil(rayDist / actualStepSize));
  [loop] for (int iStep = 0; iStep < numRayMarchSteps; ++iStep)
  {
    p += rayStep;
    
    float sdfSample = 0.0f;
    SdfBrushMaterial mat;
    SDF_RAY_MARCH_MASKED_BRUSHES(sdfSample, p, brushMask, mat, false);
    if (sdfSample < 0.0f)
    {
      float density = rayMarchVolumeDensity * sample_density(p, sdfSample, 0.1f, 0.3f, 2.0f);

      switch ((int) rayMarchLightPositionType.w)
      {
        case kLightTypePoint:
          toLightDir = normalize(rayMarchLightPositionType.xyz - p);
          break;
      }
      float lightTransmittance = light_transmittance(p, toLightDir);
      lightEnergy += density * actualStepSize * transmittance * lightTransmittance;

      transmittance *= exp(-density * actualStepSize * rayMarchAbsorption.x);

      if (transmittance < 1e-2f)
      {
        transmittance = 0.0f;
        break;
      }
    }
  }

  float3 cloudColor = lightEnergy; // TODO: * lightColor
  results.mat.color.rgb = backgroundColor * transmittance + cloudColor;
  //results.mat.color.a = saturate(1.0f - transmittance * exp(rayMarchTransmittanceCurve));
  results.mat.color.a = saturate(1.0f - transmittance);

  return results;
}

#else

struct SdfBrushMaterialDummy
{
  float4 color;
  float4 emissionHash;
  float4 metallicSmoothnessSizeTightness;
  float4 textureWeight;
};

struct RayMarchResults
{
  bool hit;
  float3 pos;
  SdfBrushMaterialDummy mat;
};

RayMarchResults ray_march_surface
(
  float3 rayOrigin, 
  float3 rayDirection, 
  out float3 normal
)
{
  RayMarchResults res;
  res.hit = false;
  res.pos = rayOrigin;
  res.mat.color = float4(0.00001f * rayDirection, 1.0f);
  res.mat.emissionHash = 0.0f;
  res.mat.metallicSmoothnessSizeTightness = 0.0f;
  res.mat.textureWeight = 0.0f;
  normal = -rayDirection;

  return res;
}

RayMarchResults ray_march_volume
(
  float3 rayOrigin, 
  float3 rayDirection, 
  float3 screenColor, 
  sampler2D ditherTexture, 
  int ditherTextureSize, 
  float2 screenPos
)
{
  RayMarchResults res;
  res.hit = false;
  res.pos = rayOrigin;
  res.mat.color = float4(0.00001f * rayDirection, 1.0f);

  return res;
}

#endif

#endif

