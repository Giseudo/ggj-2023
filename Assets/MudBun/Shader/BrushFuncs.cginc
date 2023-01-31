/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_BRUSH_FUNCS
#define MUDBUN_BRUSH_FUNCS

#include "BrushDefs.cginc"

#include "AabbTreeFuncs.cginc"
#include "BrushMaskFuncs.cginc"
#include "Math/CatmullRom.cginc"
#include "Math/Codec.cginc"
#include "Math/MathConst.cginc"
#include "Math/Quaternion.cginc"
#include "Math/Vector.cginc"
#include "Noise/RandomNoise.cginc"
#include "SDF/SDF.cginc"
#include "VoxelDefs.cginc"

SdfBrushMaterial init_brush_material()
{
  SdfBrushMaterial mat;
  mat.color = float4(0.0f, 0.0f, 0.0f, 1.0f);
  mat.emissionHash = float4(0.0f, 0.0f, 0.0f, 0.0f);
  mat.metallicSmoothnessSizeTightness = float4(0.0f, 0.0f, 1.0f, 0.0f);
  mat.textureWeight = float4(0.0f, 0.0f, 0.0f, 0.0f);
  mat.iBrush = -1;
  mat.padding0 = mat.padding1 = mat.padding2 = 0;
  return mat;
}

SdfBrushMaterial lerp(SdfBrushMaterial a, SdfBrushMaterial b, float t)
{
  SdfBrushMaterial o = a;

  o.color = lerp(a.color, b.color, t);
  o.emissionHash.rgb = lerp(a.emissionHash.rgb, b.emissionHash.rgb, t);
  o.emissionHash.a = (t < 0.5f) ? a.emissionHash.a : b.emissionHash.a;
  o.iBrush = (t < 0.5f) ? a.iBrush : b.iBrush;
  o.metallicSmoothnessSizeTightness.xyz = lerp(a.metallicSmoothnessSizeTightness.xyz, b.metallicSmoothnessSizeTightness.xyz, t);
  o.textureWeight = lerp(a.textureWeight, b.textureWeight, t);

  return o;
}

SdfBrushMaterialCompressed lerp(SdfBrushMaterialCompressed a, SdfBrushMaterialCompressed b, float t)
{
  return pack_material(lerp(unpack_material(a), unpack_material(b), t));
}

float sdf_boundary(float3 pRel, SdfBrush b, int shape, out float fadeDist)
{
  float3 h = abs(0.5f * b.size);

  fadeDist = 0.0f;

  float res = kInfinity;
  switch (shape)
  {
    case kSdfNoiseBoundaryBox:
    {
      res = sdf_box(pRel, h);
      fadeDist = max_comp(h);
      break;
    }

    case kSdfNoiseBoundarySphere:
    {
      res = sdf_ellipsoid(pRel, b.radius * b.size);
      fadeDist = b.radius * max_comp(b.size);
      break;
    }

    case kSdfNoiseBoundaryCylinder:
    {
      float2 elongation = max(0.0f, b.size.xz - 1.0f);
      pRel.xz -= clamp(pRel.xz, -elongation, elongation);
      res = sdf_cylinder(pRel, h.y, b.radius);
      fadeDist = max(b.radius, h.y);
      break;
    }

    case kSdfNoiseBoundaryTorus:
    {
      float3 hTorus = float3(h.x + 0.5f * b.radius, h.y, h.z + 0.5f * b.radius);
      res = sdf_torus(pRel, hTorus.x - hTorus.z, hTorus.z - b.radius, b.radius);
      fadeDist = max(max(h.x, h.z), b.radius);
      break;
    }

    case kSdfNoiseBoundarySolidAngle:
    {
      res = sdf_solid_angle(pRel, float2(b.data3.x, b.data3.y), b.radius);
      res = sdf_int(res, sdf_box(pRel, b.radius * b.size));
      fadeDist = b.radius;
      break;
    }
  }

  return res;
}

#include "../Customization/CustomBrush.cginc"

float sdf_brush(float res, inout float3 p, SdfBrush b)
{
  float preMirrorX = p.x;

  bool doMirrorX = ((b.flags & kSdfBrushFlagsMirrorX) != 0);
  if (doMirrorX)
    p.x = abs(p.x);

  bool flipX = ((b.flags & kSdfBrushFlagsFlipX) != 0);
  if (flipX)
    p.x = -p.x;

  // extent
  float3 h = abs(0.5f * b.size);

  // relative to transform
  float3 pRel = quat_rot(quat_inv(b.rotation), p - b.position);

  switch (b.type)
  {
    case kSdfBox:
    {
      float pivotShift = b.data0.x;
      pRel.y += pivotShift * h.y;
      res = sdf_box(pRel, h, b.radius);
      break;
    }

    case kSdfSphere:
    {
      float pivotShift = b.data0.x;
      pRel.y += pivotShift * h.y;
      res = sdf_ellipsoid(pRel, b.radius * b.size);
      break;
    }

#ifndef MUDBUN_FAST_ITERATION
    case kSdfCylinder:
    {
      float pivotShift = b.data0.z;
      pRel.y += pivotShift * h.y;

      float2 elongation = max(0.0f, b.size.xz - 1.0f);
      pRel.xz -= clamp(pRel.xz, -elongation, elongation);

      res = sdf_capped_cone(pRel, h.y, b.radius, max(0.0f, b.radius + b.data0.y), b.data0.x);
      break;
    }

    case kSdfTorus:
    {
      float elongation = b.data0.x;
      pRel.y -= clamp(pRel.y, -elongation, elongation);
      float3 hTorus = float3(h.x + 0.5f * b.radius, h.y, h.z + 0.5f * b.radius);
      float r = abs(0.25f * b.size.y);
      res = sdf_torus(pRel, hTorus.x - hTorus.z, hTorus.z - r, r);
      break;
    }

    case kSdfSolidAngle:
    {
      res = sdf_solid_angle(pRel, float2(b.data0.x, b.data0.y), b.radius, b.data0.z);
      break;
    }

  #ifndef MUDBUN_DISABLE_SDF_NOISE_VOLUME
    case kSdfNoiseVolume:
    case kSdfNoiseModifier:
    {
      float thresholdFadeDist = kEpsilon;
      int boundaryShape = int(b.data2.z);
      float noiseRes = sdf_boundary(pRel, b, boundaryShape, thresholdFadeDist);
      float thresholdFadeT = sqrt(saturate(length(pRel) / thresholdFadeDist));

      // because noise functions are not real SDFs
      float distScale = 1.0f;

      float3 aSample[2];
      float3 aPeriod[2];
      float aWeight[2];
      int numSamples = 0;

      bool lockPosition = ((b.flags & kSdfBrushFlagsLockNoisePosition) != 0);
      float3 size = b.data0.xyz;
      float3 offset = b.data1.xyz;
      if ((b.flags & kSdfBrushFlagsSphericalNoiseCoordinates) == 0)
      {
        // uniform
        aSample[0] = lockPosition ? pRel : p;
        aPeriod[0] = kCartesianNoisePeriod;
        aWeight[0] = 1.0f;
        numSamples = 1;
      }
      else
      {
        // radial
        aSample[0] = cartesian_to_spherical(pRel.xzy + kEpsilon) * float3(1.0f, kSphericalNoisePeriod / kTwoPi, 1.0f);
        aSample[1] = cartesian_to_spherical(-pRel.xyz + kEpsilon) * float3(1.0f, kSphericalNoisePeriod / kTwoPi, 1.0f);
        aPeriod[0] = float3(kCartesianNoisePeriod, kSphericalNoisePeriod, kCartesianNoisePeriod);
        aPeriod[1] = float3(kCartesianNoisePeriod, kSphericalNoisePeriod, kCartesianNoisePeriod);
        aWeight[0] = min(aSample[0].z, kPi - aSample[0].z) / kHalfPi;
        aWeight[1] = 1.0f - aWeight[0];
        numSamples = 2;

        if (!lockPosition)
        {
          aSample[0] += b.position;
          aSample[1] += b.position;
        }

        distScale = 0.25f; //clamp(s.x, 1.0f, 1.0f);
      }
      float threshold = b.data0.w;
      float thresholdFade = b.data2.y;
      threshold += (1.0f - threshold) * thresholdFade * thresholdFadeT;
      int numOctaves = int(b.data1.w);
      float octaveOffsetFactor = b.data2.x;
      float boundaryBlend = b.data2.w;
      boundaryBlend = max(0.1f * min(min(h.x, h.y), h.z), boundaryBlend);
      int noiseType = int(b.data3.z);
      float sRes = 0.0f;
      [loop] for (int i = 0; i < numSamples; ++i)
      {
        float s = sdf_noise(noiseType, aSample[i], -h, h, offset, size, threshold, numOctaves, octaveOffsetFactor, aPeriod[i]);
        sRes += aWeight[i] * s * distScale;
      }
      noiseRes = sdf_int_smooth(noiseRes, sRes, boundaryBlend);

      switch (b.type)
      {
        case kSdfNoiseVolume:
          res = noiseRes;
          break;
        case kSdfNoiseModifier:
          res -= noiseRes * b.blend;
          break;
      }
      break;
    }
  #endif // MUDBUN_DISABLE_SDF_NOISE_VOLUME

  #ifndef MUDBUN_DISABLE_SDF_SIMPLE_CURVE
    case kSdfCurveSimple:
    {
      float3 pA = b.data0.xyz;
      float3 pB = b.data1.xyz;
      float3 pC = b.data2.xyz;

      float3 pRelRaw = pRel;
      float elongation = b.data3.x;
      pRel.z -= clamp(pRel.z, -elongation, elongation);

      float controlPointR = b.data3.y;
      float smoothStepBlend = b.data3.z;
      float r = 0.0f;

      const bool colinear = b.data3.w > 0.0f;
      float2 curRes = 
        b.data3.w > 0.0f // colinear?
          ? sdf_segment(pRel, pA, pB) 
          : sdf_bezier(pRel, pA, pC, pB);

      if (controlPointR < 0.0f)
      {
        float t = curRes.y;
        r = b.data0.w + (b.data1.w - b.data0.w) * lerp(t, smoothstep(0.0f, 1.0f, t), smoothStepBlend);
      }
      else
      {
        if (curRes.y < 0.5f)
        {
          float t = 2.0f * curRes.y;
          r = b.data0.w + (controlPointR - b.data0.w) * lerp(t, smoothstep(0.0f, 1.0f, t), smoothStepBlend);
        }
        else
        {
          float t = 2.0f * (curRes.y - 0.5f);
          r = controlPointR + (b.data1.w - controlPointR) * lerp(t, smoothstep(0.0f, 1.0f, t), smoothStepBlend);
        }
      }
      res = curRes.x - r;

      bool useNoise = (b.data2.w > 0.0f);
      if (useNoise)
      {
        float curveLen = 0.0f;
        int precision = 16;
        float dt = 1.0f / precision;
        float t = dt;
        float3 prevPos = pA;
        [loop] for (int i = 1; i < precision; ++i, t += dt)
        {
          float3 currPos = bezier_quad(pA, pC, pB, t);
          curveLen += length(currPos - prevPos);
          prevPos = currPos;
        }
        if (curRes.y < 0.0001f)
          curRes.y = min(0.0f, -dot(normalize(pA - pC), pRel - pA) / curveLen);
        else if (curRes.y > 0.9999f)
          curRes.y = max(1.0f, 1.0f + dot(normalize(pB - pC), pRel - pB) / curveLen);

        float3 up = normalize(kUnitY + 1e-3f * rand(pA));
        float3 front = normalize(slerp(pA - pC, pC - pB, curRes.y));
        float3 left = normalize(cross(up, front));
        up = cross(front, left);
        float3 closest = bezier_quad(pA, pC, pB, curRes.y);
        float3 pDelta = pRelRaw - closest;
        float3 s = float3(curRes.y * curveLen, dot(pDelta, up), dot(pDelta, left));

        // advance to additional noise data
        b = aBrush[b.index + 1];

        float thresholdFade = b.data3.x;
        float thresholdCoreBias = b.data3.y;

        // twist
        float twistA = b.data2.y;
        float twistB = b.data2.z;
        float twistT = lerp(twistA, twistB, curRes.y);
        float twistCos = cos(twistT);
        float twistSin = sin(twistT);
        s.yz = mul(float2x2(twistCos, twistSin, -twistSin, twistCos), s.yz);

        float3 offset = b.data1.xyz;
        float3 size = b.data0.xyz;
        float threshold = b.data0.w;
        float rDelta = length(pDelta);
        float coreBiasT = 1.0f - saturate(rDelta / max(kEpsilon, r));
        threshold = saturate(threshold + sign(thresholdCoreBias) * abs(thresholdCoreBias) * coreBiasT);
        threshold += (1.0f - threshold) * thresholdFade * saturate(curRes.y);
        int numOctaves = int(b.data1.w);
        float octaveOffsetFactor = b.data2.x;

        float twistSdfMult = 1.0f / (1.0f + saturate(abs(twistA - twistB))); // hack: evlauate more surrounding voxels when twisted to avoid holes
        float n = twistSdfMult * sdf_noise(kSdfNoiseTypeCachedPerlin, s, -kInfinity, kInfinity, offset, size, threshold, numOctaves, octaveOffsetFactor, kCartesianNoisePeriod);
        res = sdf_int_smooth(res, n, 0.5f * r);
      }

      break;
    }
  #endif // MUDBUN_DISABLE_SDF_SIMPLE_CURVE

  #ifndef MUDBUN_DISABLE_SDF_FULL_CURVE
    case kSdfCurveFull:
    {
      int numPoints = int(b.data0.x);
      if (numPoints > 1)
      {
        res = kInfinity;

        int precision = int(b.data0.y);
        float dt = 1.0f / precision;

        bool useNoise = false;//(b.data0.z > 0.0f);

        int iA = b.index + (useNoise ? 2 : 1);
        float globalLen = 0.0f;
        int iClosest = -1;
        float tClosest = 0.0f;
        float segResClosest = 0.0f;
        float rClosest = 0.0f;
        float3 pClosest = 0.0f;
        float closestLen = 0.0f;
        [loop] for (int i = 1, n = numPoints - 2; i < n; ++i, ++iA)
        {
          float3 pA = aBrush[iA + 0].data0.xyz;
          float3 pB = aBrush[iA + 1].data0.xyz;
          float3 pC = aBrush[iA + 2].data0.xyz;
          float3 pD = aBrush[iA + 3].data0.xyz;
          float3 prevPos = pB;
          float r = aBrush[iA + 1].data0.w;
          float dr = (aBrush[iA + 2].data0.w - r) * dt;
          float localLen = 0.0f;
          for (float t = dt; t < 1.0001f; t += dt)
          {
            float3 currPos = catmull_rom(pA, pB, pC, pD, min(1.0f, t));
            float segLen = length(currPos - prevPos);
            float d = sdf_round_cone(p, prevPos, currPos, r, r + dr);
            if (d < res)
            {
              float2 segRes = sdf_segment(p, prevPos, currPos);
              res = d;
              iClosest = i;
              tClosest = t;
              rClosest = r + dr * segRes.y;
              pClosest = lerp(prevPos, currPos, segRes.y);
              closestLen = globalLen + localLen + segLen * segRes.y;
              segResClosest = segRes.y;
            }
            prevPos = currPos;
            r += dr;
            localLen += segLen;
          }
          globalLen += localLen;
        }
        
        /*
        if (iClosest > 0 
            && globalLen > kEpsilon 
            && useNoise)
        {
          int iA = b.index + (useNoise ? 2 : 1) + (iClosest - 1); // reset
          float3 p0 = (iClosest > 1) ? aBrush[iA - 1].data0.xyz : aBrush[iA + 0].data0.xyz;
          float3 pA = aBrush[iA + 0].data0.xyz;
          float3 pB = aBrush[iA + 1].data0.xyz;
          float3 pC = aBrush[iA + 2].data0.xyz;
          float3 pD = aBrush[iA + 3].data0.xyz;
          float3 pE = (iClosest < numPoints - 3) ? aBrush[iA + 4].data0.xyz : aBrush[iA + 3].data0.xyz;
          float3 segPosB = catmull_rom(pA, pB, pC, pD, tClosest - dt);
          float3 segPosC = catmull_rom(pA, pB, pC, pD, tClosest);
          float3 front = normalize(segPosC - segPosB);
          float3 dir0B = pB - p0;
          float3 dirAC = pC - pA;
          float3 dirBD = pD - pB;
          float3 dirCE = pE - pC;
          //float3 front = normalize(catmull_rom(dir0B, dirAC, dirBD, dirCE, tClosest + (-1.0f + segResClosest) * dt));
          float3 up = normalize(kUnitY + 1e-3f * rand(aBrush[iA].data0.xyz));
          float3 left = normalize(cross(up, front));
          up = cross(front, left);
          float3 s = float3(closestLen, dot(p - pClosest, up), dot(p - pClosest, left));
          float3 offset = aBrush[b.index + 1].data1.xyz;
          float3 size = aBrush[b.index + 1].data0.xyz;
          float threshold = aBrush[b.index + 1].data0.w;
          int numOctaves = int(aBrush[b.index + 1].data1.w);
          float octaveOffsetFactor = aBrush[b.index + 1].data2.x;
          // TODO: noise type (if we ever re-instate noise along full curves...)
          float n = sdf_noise(kSdfNoiseTypeCachedPerlin, s, -kInfinity, kInfinity, offset, size, threshold, numOctaves, octaveOffsetFactor, kCartesianNoisePeriod);
          res = sdf_int_smooth(res, n, 0.5f * rClosest);
        }
        */
      }
      break;
    }
  #endif // MUDBUN_DISABLE_SDF_FULL_CURVE

    case kSdfParticleSystem:
    {
      res = kInfinity;
      int numParticles = int(b.data2.x);
      for (int i = 0; i < numParticles; ++i)
      {
        float3 pos = aBrush[b.index + i].data0.xyz;
        float r = aBrush[b.index + i].data0.w;
        float selfBlend = aBrush[b.index + i].data1.x;
        res = sdf_uni_smooth(res, sdf_sphere(p - pos, r), selfBlend);
      }
      break;
    }

  #ifndef MUDBUN_DISABLE_SDF_DISTORTION_BRUSHES
    case kSdfFishEye:
    {
      float r = length(pRel);
      if (r > b.radius)
        break;

      float t = r / b.radius;
      float strength = b.data0.x;
      float fade = 1.0f - pow(abs(t), strength);
      p -= (b.radius * fade) * quat_rot(b.rotation, normalize_safe(pRel, kUnitY));
      break;
    }

    case kSdfPinch:
    {
      float depth = b.data0.x;
      float r = length(pRel.xz);
      if (sdf_cylinder(pRel + float3(0.0f, 0.5f * depth, 0.0f), 0.5f * depth, b.radius) > 0.0f)
        break;
      
      float amount = b.data0.y;
      float strength = b.data0.z;
      float g = -pRel.y / depth;
      float t = r / max(kEpsilon, b.radius);
      float pinchRatio = pow(abs(1.0f - t), strength);
      g = pow(abs(g), 0.5f);
      pRel.y = -g * depth; // remap
      float fade = (depth + pRel.y) / depth;
      p += (amount * pinchRatio * fade) * quat_rot(b.rotation, float3(0.0f, pRel.y, 0.0f));
      break;
    }

    case kSdfTwist:
    {
      if (sdf_cylinder(pRel, h.y, b.radius, 0.0f) > 0.0f)
        break;
    
      float angle = b.data0.x;
      float strength = b.data0.y;
      float r = length(pRel.xz);
      float t = r / b.radius;
      float a = angle * (1.0f - pow(abs(t), strength));
      float s = sin(a);
      float c = cos(a);
      pRel.xz = mul(float2x2(c, -s, s, c), pRel.xz);
      p = quat_rot(b.rotation, pRel) + b.position;
      break;
    }

    case kSdfQuantize:
    {
      float cellSize = b.data0.x;
      float fade = b.data0.z;
      float d = sdf_box(pRel, h, fade * cellSize);
      if (d > 0.0f)
        break;
      
      float strength = b.data0.y;
      float3 r = p / cellSize;
      float3 f = floor(r);
      float3 t = r - f;
      float3 q = (f + smoothstep(0.0f, 1.0f, max(1.0f, strength) * (t - 0.5f) + 0.5f)) * cellSize;
      p = lerp(p, q, saturate(strength) * saturate(-d / max(kEpsilon, fade * cellSize)));
      break;
    }
  #endif // MUDBUN_DISABLE_SDF_DISTORTION_BRUSHES

  #ifndef MUDBUN_DISABLE_SDF_MODIFIER_BRUSHES
    case kSdfOnion:
    {
      float d = sdf_box(pRel, h, b.blend);
      if (d > 0.0f)
        break;
      
      float thickness = b.data0.x;
      res = abs(res) - thickness;
      break;
    }
  #endif // MUDBUN_DISABLE_SDF_MODIFIER_BRUSHES
#endif // MUDBUN_FAST_ITERATION

    default:
    {
      res = sdf_custom_brush(res, p, pRel, b);
      break;
    }
  }

  if (flipX || doMirrorX)
    p.x = preMirrorX;

  return res;
}

float sdf_distortion_modifier_bounds_query(float3 p, SdfBrush b)
{
  float res = kInfinity;

  float3 pRel = quat_rot(quat_inv(b.rotation), p - b.position);
  float3 h = 0.5f * b.size;

  switch (b.type)
  {
#ifndef MUDBUN_FAST_ITERATION
    case kSdfPinch:
    {
      float depth = b.data0.x;
      res = sdf_cylinder(pRel + float3(0.0f, 0.5f * depth, 0.0f), 0.5f * depth, b.radius);
      break;
    }

    case kSdfTwist:
    {
      float angle = b.data0.x;
      res = sdf_cylinder(pRel, h.y, b.radius);
      break;
    }

    case kSdfQuantize:
    {
      float cellSize = b.data0.x;
      float fade = b.data0.z;
      res = sdf_box(pRel, h, fade * cellSize);
      break;
    }

    case kSdfFishEye:
    {
      res = sdf_sphere(pRel, b.radius);
      break;
    }

    case kSdfOnion:
    {
      float thickness = b.data0.x;
      res = sdf_box(pRel, h + thickness, b.blend);
      break;
    }
#endif // MUDBUN_FAST_ITERATION

    default:
    {
      res = sdf_custom_distortion_modifier_bounds_query(p, pRel, b);
      break;
    }
  }

  return res;
}

float dist_blend_weight(float distA, float distB, float strength)
{
  float m = 1.0f / max(kEpsilon, distA);
  float n = 1.0f / max(kEpsilon, distB);
  m = pow(m, strength);
  n = pow(n, strength);
  return saturate(n / (m + n));
}

float sdf_brush_apply(float res, float groupRes, SdfBrushMaterial groupMat, inout float3 p, SdfBrush b, inout SdfBrushMaterial oMat, bool outputMat = true, float halfNodeDiag = -1.0f)
{
  float d = sdf_brush(res, p, b);

  if (b.type == kSdfEndGroup)
    d = groupRes;

  bool isGroupBrush = false;
  switch (b.type)
  {
  case kSdfBeginGroup:
  case kSdfEndGroup:
    isGroupBrush = true;
    break;
  }

  float tMat = 0.0f;
  float blend = b.blend;
  switch (b.op)
  {
    case kSdfUnion:
      // compute tMat before res is updated!
      if (!isGroupBrush) // don't perform surface this on group brushes, or it might crash the GPU!
        d -= surfaceShift;
      tMat = dist_blend_weight(res, d, 1.5f);
      if (enable2dMode && d < 0.25 * blend)
        tMat = max(tMat, min(1.0f, 1.0f - d / max(kEpsilon, 0.25 * blend)));
      res = sdf_uni_smooth(res, d, blend);
      break;

    case kSdfSubtract:
      if (!isGroupBrush) // don't perform surface this on group brushes, or it might crash the GPU!
        d += surfaceShift;
      res = sdf_sub_smooth(res, d, blend);
      if (enable2dMode)
        tMat = 1.0f - saturate(d / max(kEpsilon, blend));
      else
        tMat = 1.0f - saturate(2.0f * (d - 1.5f * voxelSize) / max(kEpsilon, blend));
      break;

    case kSdfIntersect:
      if (!isGroupBrush) // don't perform surface this on group brushes, or it might crash the GPU!
        d -= surfaceShift;
      res = sdf_int_smooth(res, d, blend);
      if (enable2dMode)
        tMat = saturate((-d + voxelSize) / max(kEpsilon, blend));
      else
        tMat = 1.0f - saturate(-2.0f * (d + 1.0f * voxelSize) / max(kEpsilon, blend));
      break;

    case kSdfDye:
      if (!isGroupBrush) // don't perform surface this on group brushes, or it might crash the GPU!
        d -= surfaceShift;
      if (enable2dMode)
        tMat = 1.0f - saturate((d - voxelSize) / max(kEpsilon, 0.25f * blend));
      else
        tMat = 1.0f - saturate(max(0.0f, d) / max(kEpsilon, blend));
      break;

    case kSdfCullInside:
      if (halfNodeDiag < 0.0f)
        break;
      if (!isGroupBrush 
          || b.type == kSdfEndGroup)
      {
        if (d < -halfNodeDiag)
          res = kCull;
      }
      break;

    case kSdfCullOutside:
      if (halfNodeDiag < 0.0f)
        break;
      if (!isGroupBrush 
          || b.type == kSdfEndGroup)
      {
        if (d > halfNodeDiag)
          res = kCull;
      }
      break;

    case kSdfDistort:
      res = sdf_uni(res, d);
      break;

    case kSdfModify:
      res = d;
      break;
  }

  if (b.materialIndex >= 0)
  {
    float blendTightness = aBrushMaterial[b.materialIndex].metallicSmoothnessSizeTightness.w;
    if (blendTightness > 0.0f)
    {
      // remap to between [-1.0, 1.0]
      // take 1.0 - x
      // curve with tightness
      // take 1.0 - x
      // remap back to [0.0, 1.0]
      tMat -= 0.5f;
      tMat = 0.5f + 0.5f * sign(tMat) * (1.0f - pow(abs(1.0f - abs(2.0f * tMat)), pow(1.0f + blendTightness, 5.0f)));
    }

    SdfBrushMaterial iMat = aBrushMaterial[b.materialIndex];
    if (b.type == kSdfEndGroup)
      iMat = groupMat;

    if ((b.flags & kSdfBrushFlagsContributeMaterial) == 0)
    {
      iMat = oMat;
    }

    iMat.emissionHash.a = b.hash;
    iMat.iBrush = b.index;

    // selection highlight
    if (b.hash < 0.0f)
    {
      iMat.color.rgb = saturate(iMat.color.rgb + 0.1f);
      iMat.emissionHash.rgb = saturate(iMat.emissionHash.rgb + 0.1f);
    }

    oMat = lerp(oMat, iMat, tMat);
  }

  return res;
}

float sdf_all_brushes(float3 p, int iBrushMask, out SdfBrushMaterial mat, bool outputMat = true)
{
  mat = init_brush_material();
  float res = kInfinity;
  for (int iBrush = 0; iBrush < numBrushes; ++iBrush)
    res = sdf_brush_apply(res, res, mat, p, aBrush[iBrush], mat);

  res -= surfaceShift;

  return res;
}

float sdf_masked_brushes(float3 p, int iBrushMask, out SdfBrushMaterial mat, bool outputMat = true, float halfNodeDiag = -1.0f)
{
  int iStack = -1;
  float3 pStack[kMaxBrushGroupDepth];
  float resStack[kMaxBrushGroupDepth];
  SdfBrushMaterial matStack[kMaxBrushGroupDepth];

  float res = kInfinity;
  mat = init_brush_material();
  float groupRes = kInfinity;
  SdfBrushMaterial groupMat = init_brush_material();
  FOR_EACH_BRUSH(iBrushMask, 
    switch (aBrush[iBrush].type)
    {
      case kSdfBeginGroup:
        {
          iStack = min(kMaxBrushGroupDepth - 1, iStack + 1);
          pStack[iStack] = p;
          resStack[iStack] = res;
          matStack[iStack] = mat;
          res = kInfinity;
          mat = init_brush_material();

          bool doMirrorX = ((aBrush[iBrush].flags & kSdfBrushFlagsMirrorX) != 0);
          if (doMirrorX)
            p.x = abs(p.x);

          bool flipX = ((aBrush[iBrush].flags & kSdfBrushFlagsFlipX) != 0);
          if (flipX)
            p.x = -p.x;
        }
        break;
      case kSdfEndGroup:
        {
          groupRes = res;
          groupMat = mat;
          p = pStack[iStack];
          res = resStack[iStack];
          mat = matStack[iStack];
        }
        break;
    }
    res = sdf_brush_apply(res, groupRes, groupMat, p, aBrush[iBrush], mat, outputMat, halfNodeDiag);
    if (res == kCull)
      break; // early-out if we have decided to cull the node
    switch (aBrush[iBrush].type)
    {
      case kSdfEndGroup:
        iStack = max(-1, iStack - 1);
        break;
    }
  );

  return res;
}

#endif


