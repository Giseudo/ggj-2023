/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEngine;

#if MUDBUN_BURST
using Unity.Burst;
using Unity.Mathematics;
#endif

namespace MudBun
{
#if MUDBUN_BURST
  [BurstCompile]
#endif
  public class MathUtil
  {
    public static readonly float Pi        = Mathf.PI;
    public static readonly float TwoPi     = 2.0f * Mathf.PI;
    public static readonly float HalfPi    = Mathf.PI / 2.0f;
    public static readonly float ThirdPi   = Mathf.PI / 3.0f;
    public static readonly float QuarterPi = Mathf.PI / 4.0f;
    public static readonly float FifthPi   = Mathf.PI / 5.0f;
    public static readonly float SixthPi   = Mathf.PI / 6.0f;

    public static readonly float Sqrt2    = Mathf.Sqrt(2.0f);
    public static readonly float Sqrt2Inv = 1.0f / Mathf.Sqrt(2.0f);
    public static readonly float Sqrt3    = Mathf.Sqrt(3.0f);
    public static readonly float Sqrt3Inv = 1.0f / Mathf.Sqrt(3.0f);

    public static readonly float Epsilon = 1.0e-9f;
    public static readonly float EpsilonComp = 1.0f - Epsilon;
    public static readonly float Rad2Deg = 180.0f / Mathf.PI;
    public static readonly float Deg2Rad = Mathf.PI / 180.0f;

    public static readonly int CartesianNoisePeriod = 8;
    public static readonly int SphericalNoisePeriod = 4;
    public static readonly int CachedNoiseDensity = 16;

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float AsinSafe(float x)
    {
      return Mathf.Asin(Mathf.Clamp(x, -1.0f, 1.0f));
    }

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float AcosSafe(float x)
    {
      return Mathf.Acos(Mathf.Clamp(x, -1.0f, 1.0f));
    }

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float InvSafe(float x)
    {
      return 1.0f / Mathf.Max(Epsilon, x);
    }

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float BezierQuad(float a, float b, float controlPoint, float t)
    {
      return Mathf.Lerp(Mathf.Lerp(a, controlPoint, t), Mathf.Lerp(controlPoint, b, t), t);
    }

#if MUDBUN_BURST
    [BurstCompile]
    public static void BezierQuad(in float2 a, in float2 b, in float2 controlPoint, float t, out float2 ret)
    {
      ret = math.lerp(math.lerp(a, controlPoint, t), math.lerp(controlPoint, b, t), t);
    }

    [BurstCompile]
    public static void BezierQuad(in float3 a, in float3 b, in float3 controlPoint, float t, out float3 ret)
    {
      ret = math.lerp(math.lerp(a, controlPoint, t), math.lerp(controlPoint, b, t), t);
    }

    [BurstCompile]
    public static void BezierQuad(in float4 a, in float4 b, in float4 controlPoint, float t, out float4 ret)
    {
      ret = math.lerp(math.lerp(a, controlPoint, t), math.lerp(controlPoint, b, t), t);
    }
#endif

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float CatmullRom(float p0, float p1, float p2, float p3, float t)
    {
      float tt = t * t;
      return
        0.5f 
        * ((2.0f * p1) 
          + (-p0 + p2) * t 
          + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
          + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t 
          );
    }

#if MUDBUN_BURST
    [BurstCompile]
    public static void CatmullRom(in float2 p0, in float2 p1, in float2 p2, in float2 p3, float t, out float2 ret)
    {
      float tt = t * t;
      ret = 
        0.5f 
        * ((2.0f * p1) 
          + (-p0 + p2) * t 
          + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
          + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t 
          );
    }

    [BurstCompile]
    public static void CatmullRom(in float3 p0, in float3 p1, in float3 p2, in float3 p3, float t, out float3 ret)
    {
      float tt = t * t;
      ret = 
        0.5f 
        * ((2.0f * p1) 
          + (-p0 + p2) * t 
          + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
          + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t 
          );
    }

    [BurstCompile]
    public static void CatmullRom(in float4 p0, in float4 p1, in float4 p2, in float4 p3, float t, out float4 ret)
    {
      float tt = t * t;
      ret =
        0.5f
        * ((2.0f * p1)
          + (-p0 + p2) * t
          + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt
          + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
          );
    }
#endif

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public static float Saturate(float x)
    {
      return 
        x < 0.0f 
          ? 0.0f 
          : x > 1.0f 
            ? 1.0f 
            :x;
    }
  }
}

