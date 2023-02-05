/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Linq;
using System.Reflection;

using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Unity.Collections.LowLevel.Unsafe;

#if MUDBUN_BURST
using Unity.Burst;
using Unity.Mathematics;
#endif

namespace MudBun
{
  using AabbTree = NativeArray<AabbTree<MudBrushBase>.NodePod>;
  using BrushArray = NativeArray<SdfBrush>;
  using MaterialArray = NativeArray<SdfBrushMaterial>;
  using IntStack = NativeArray<int>;
  using FloatStack = NativeArray<float>;
  using VecStack = NativeArray<Vector3>;
  using MatStack = NativeArray<SdfBrushMaterial>;

  /// <summary>
  /// Low-level interface for CPU-based SDF evaluation. Used by renderers internally. Tinker at your own risk.
  /// </summary>
#if MUDBUN_BURST
  [BurstCompile]
#endif
  public static unsafe class Sdf
  {
#if MUDBUN_BURST
    private static readonly int MaxAabbTreeStackSize = 16;
    private static readonly int MaxBrushGroupDepth = 8;
#endif

    /// <summary>
    /// SDF value and/or normal (normalized gradient) evaluation result.
    /// </summary>
    public struct Result
    {
      public static Result New(float value, in SdfBrushMaterial material, Vector3 normal)
      {
        return 
          new Result()
          {
            m_value = value, 
            m_material = material, 
            m_normal = normal, 
          };
      }

      private float m_value;
      private SdfBrushMaterial m_material;
      private Vector3 m_normal;
      
      /// <summary>
      /// SDF value.
      /// </summary>
      public float Value => m_value;
      /// <summary>
      /// Material (if material computation is specified).
      /// </summary>
      public SdfBrushMaterial Material => m_material;
      /// <summary>
      /// SDF normal (normalized gradient).
      /// </summary>
      public Vector3 Normal => m_normal;
    }

    /// <summary>
    /// Signature for static methods meant to be tagged with <c>RegisterSdfBrushEvalFuncAttribute</c> for CPU-based SDF brush computation.
    /// </summary>
    /// <param name="res">SDF value.</param>
    /// <param name="p">Sample position.</param>
    /// <param name="pRel">Relative sample position to brush.</param>
    /// <param name="aBrush">Array of brush compute data.</param>
    /// <param name="iBrush">Index of the current brush's first compute data element in the array.</param>
    /// <returns></returns>
    public delegate float SdfBrushEvalFunc(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush);

#if MUDBUN_BURST
    public struct SdfBrushEvalFuncMapEntry
    {
      public int BrushType;
      public FunctionPointer<SdfBrushEvalFunc> Func;
    }

    private static readonly int DenseSdfEvalMapSize = 500;
    private static NativeArray<FunctionPointer<SdfBrushEvalFunc>> s_sdfEvalFuncMapDense;
    private static NativeArray<SdfBrushEvalFuncMapEntry> s_sdfEvalFuncMapSparse;
#else
    public struct float3 { } // for documentation
    private static bool s_warnedBurstMissing = false;
    private static void WarnBurstMissing()
    {
      if (s_warnedBurstMissing)
        return;

      Debug.LogWarning("Burst is now needed for MudBun's new raycast-based selection & CPU-based computations (SDF/normal/raycast/snap).\n"
                      + "Please import the Burst package (version 1.2.3 or 1.7.0 and newer) in Unity's package manager and then click Tools > MudBun > Refresh Compatibility.");
      s_warnedBurstMissing = true;
    }
#endif

#if MUDBUN_BURST
    [BurstCompile]
    public static unsafe float DummyEvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      return float.MaxValue;
    }
#endif

    public static void InitEvalMap()
    {
#if MUDBUN_BURST
      if (!s_sdfEvalFuncMapDense.IsCreated)
      {
        s_sdfEvalFuncMapDense = new NativeArray<FunctionPointer<SdfBrushEvalFunc>>(DenseSdfEvalMapSize, Allocator.Persistent);
        var dummy = (SdfBrushEvalFunc) DummyEvaluateSdf;
        for (int i = 0; i < s_sdfEvalFuncMapDense.Length; ++i)
          RegisterSdfBrushEvalFunc(i, dummy);
      }
      if (!s_sdfEvalFuncMapSparse.IsCreated)
      {
        s_sdfEvalFuncMapSparse = new NativeArray<SdfBrushEvalFuncMapEntry>(1, Allocator.Persistent);
      }

      var assembly = Assembly.GetExecutingAssembly();
      var types = assembly.GetTypes();
      var brushTypes = types.Where(x => x.IsSubclassOf(typeof(MudBrushBase)));
      foreach (var brushClass in brushTypes)
      {
        var methods = brushClass.GetMethods();
        var sdfEvalFuncs = methods.Where(x => x.IsStatic && x.GetCustomAttribute<RegisterSdfBrushEvalFuncAttribute>() != null && !x.Name.Contains("$Burst"));
        foreach (var evalFunc in sdfEvalFuncs)
        {
          var attr = evalFunc.GetCustomAttribute<RegisterSdfBrushEvalFuncAttribute>();
          var d = (SdfBrushEvalFunc) evalFunc.CreateDelegate(typeof(SdfBrushEvalFunc));
          RegisterSdfBrushEvalFunc(attr.BrushType, d);
        }
      }
#endif
    }

    public static void DisposeEvalMap()
    {
#if MUDBUN_BURST
      if (s_sdfEvalFuncMapDense.IsCreated)
        s_sdfEvalFuncMapDense.Dispose();
      if (s_sdfEvalFuncMapSparse.IsCreated)
        s_sdfEvalFuncMapSparse.Dispose();
#endif
    }

#if MUDBUN_BURST
    public static void RegisterSdfBrushEvalFunc(int brushType, SdfBrushEvalFunc func)
    {
      //var pFunc = new FunctionPointer<SdfBrushEvalFunc>(Marshal.GetFunctionPointerForDelegate(func));
      var pFunc = BurstCompiler.CompileFunctionPointer(func);

      if (brushType < DenseSdfEvalMapSize)
      {
        s_sdfEvalFuncMapDense[brushType] = pFunc;
      }
      else
      {
        var entry =
            new SdfBrushEvalFuncMapEntry()
            {
              BrushType = brushType, 
              Func = pFunc, 
            };

        int iExistingEntry = -1;

        if (s_sdfEvalFuncMapSparse.IsCreated)
        {
          for (int i = 0; i < s_sdfEvalFuncMapSparse.Length; ++i)
          {
            if (s_sdfEvalFuncMapSparse[i].BrushType != brushType)
              continue;

            iExistingEntry = i;
            break;
          }
        }

        if (iExistingEntry < 0)
        {
          if (s_sdfEvalFuncMapSparse.IsCreated)
          {
            int len = s_sdfEvalFuncMapSparse.Length;
            var oldMap = s_sdfEvalFuncMapSparse;
            s_sdfEvalFuncMapSparse = new NativeArray<SdfBrushEvalFuncMapEntry>(len + 1, Allocator.Persistent);
            for (int i = 0; i < oldMap.Length; ++i)
              s_sdfEvalFuncMapSparse[i] = oldMap[i];
            oldMap.Dispose();
          }
          else
          {
            s_sdfEvalFuncMapSparse = new NativeArray<SdfBrushEvalFuncMapEntry>(1, Allocator.Persistent);
          }

          s_sdfEvalFuncMapSparse[s_sdfEvalFuncMapSparse.Length - 1] = entry;
        }
        else
        {
          s_sdfEvalFuncMapSparse[iExistingEntry] = entry;
        }
      }
    }

    // operators
    //-------------------------------------------------------------------------

    [BurstCompile]
    public static float DistBlendWeight(float distA, float distB, float strength)
    {
      float m = 1.0f / Mathf.Max(MathUtil.Epsilon, distA);
      float n = 1.0f / Mathf.Max(MathUtil.Epsilon, distB);
      m = Mathf.Pow(m, strength);
      n = Mathf.Pow(n, strength);
      return MathUtil.Saturate(n / (m + n));
    }

    [BurstCompile]
    public static float UniSmooth(float a, float b, float k)
    {
      float h = math.max(k - math.abs(a - b), 0.0f) / math.max(k, MathUtil.Epsilon);
      return math.min(a, b) - h * h * h * k * (1.0f / 6.0f);
    }

    [BurstCompile]
    public static float SubSmooth(float a, float b, float k)
    {
      float h = math.max(k - math.abs(a + b), 0.0f) / math.max(k, MathUtil.Epsilon);
      return math.max(a, -b) + h * h * h * k * (1.0f / 6.0f);
    }

    [BurstCompile]
    public static float IntSmooth(float a, float b, float k)
    {
      float h = math.max(k - math.abs(a - b), 0.0f) / math.max(k, MathUtil.Epsilon);
      return math.max(a, b) + h * h * h * k * (1.0f / 6.0f);
    }

    //-------------------------------------------------------------------------
    // end: operators


    // primitives
    //-------------------------------------------------------------------------

    [BurstCompile]
    public static float Sphere(in float3 p, float r)
    {
      return math.length(p) - r;
    }

    [BurstCompile]
    public static float Ellipsoid(in float3 p, in float3 h)
    {
      float k0 = math.max(MathUtil.Epsilon, math.length(p / h));
      float k1 = math.max(MathUtil.Epsilon, math.length(p / (h * h)));
      return k0 * (k0 - 1.0f) / k1;
    }

    [BurstCompile]
    public static float Box(in float3 p, in float3 h, float r = 0.0f)
    {
      float3 absH = math.abs(h);
      float3 d = math.abs(p) - absH;
      return math.length(math.max(d, 0.0f)) + math.min(math.cmax(d), 0.0f) - r;
    }

    [BurstCompile]
    public static float Capsule(in float3 p, in float3 a, in float3 b, float r)
    {
      float3 ab = b - a;
      float3 ap = p - a;
      float3 pAdj = p - a + math.saturate(math.dot(ap, ab) / math.dot(ab, ab)) * ab;
      return math.length(pAdj) - r;
    }

    [BurstCompile]
    public static float CappedCone(in float3 p, float h, float r1, float r2, float r = 0.0f)
    {
      float2 q = new float2(math.length(p.xz), p.y);
      float2 k1 = new float2(r2, h);
      float2 k2 = new float2(r2 - r1, 2.0f * h);
      float2 ca = new float2(q.x - math.min(q.x, (q.y < 0.0f) ? r1 : r2), math.abs(q.y) - h);
      float2 cb = q - k1 + k2 * math.clamp(math.dot(k1 - q, k2) / math.dot(k2, k2), 0.0f, 1.0f);
      float s = (cb.x < 0.0f && ca.y < 0.0f) ? -1.0f : 1.0f;
      return s * math.sqrt(math.min(math.dot(ca, ca), math.dot(cb, cb))) - r;
    }

    [BurstCompile]
    public static float RoundCone(in float3 p, in float3 a, in float3 b, float r1, float r2)
    {
      // sampling independent computations (only depend on shape)
      float3 ba = b - a;
      float l2 = math.dot(ba, ba);
      float rr = r1 - r2;
      float a2 = l2 - rr * rr;
      float il2 = 1.0f / l2;

      // sampling dependent computations
      float3 pa = p - a;
      float y = math.dot(pa, ba);
      float z = y - l2;
      float3 g = pa * l2 - ba * y;
      float x2 = math.dot(g, g);
      float y2 = y * y * l2;
      float z2 = z * z * l2;

      // single square root!
      float k = math.sign(rr) * rr * rr * x2;
      if (math.sign(z) * a2 * z2 > k) 
        return math.sqrt(x2 + z2) * il2 - r2;

      if (math.sign(y) * a2 * y2 < k) 
        return math.sqrt(x2 + y2) * il2 - r1;

      return (math.sqrt(x2*a2*il2) + y * rr)*il2 - r1;
    }

    [BurstCompile]
    public static float Cylinder(in float3 p, float h, float r, float rr = 0.0f)
    {
      float2 d = math.abs(new float2(math.length(p.xz), p.y)) - new float2(r, h);
      return math.min(math.max(d.x, d.y), 0.0f) + math.length(math.max(d, 0.0f)) - rr;
    }

    [BurstCompile]
    public static float Torus(in float3 p, float h, float r1, float r2)
    {
      float3 q = new float3(math.max(math.abs(p.x) - h, 0.0f), p.y, p.z);
      return math.length(new float2(math.length(q.xz) - r1, q.y)) - r2;
    }

    [BurstCompile]
    public static float SolidAngle(in float3 p, in float2 c, float r, float rr = 0.0f)
    {
      // c is the sin/cos of the angle
      float2 q = new float2(math.length(p.xz), p.y);
      float l = math.length(q) - r;
      float m = math.length(q - c * math.clamp(math.dot(q, c), 0.0f, r));
      return math.max(l, m * math.sign(c.y * q.x - c.x * q.y)) - rr;
    }

    [BurstCompile]
    public static void Segment(in float3 p, in float3 a, in float3 b, out float2 ret)
    {
      float3 pa = p - a, ba = b - a;
      float h = math.saturate(math.dot(pa, ba) / math.dot(ba, ba));
      ret = new float2(math.length(pa - ba * h), h);
    }

    [BurstCompile]
    public static void Bezier(in float3 pos, in float3 A, in float3 B, in float3 C, out float2 ret)
    {
      float3 a = B - A;
      float3 b = A - 2.0f * B + C;
      float3 c = a * 2.0f;
      float3 d = A - pos;

      float kk = 1.0f / math.dot(b, b);
      float kx = kk * math.dot(a, b);
      float ky = kk * (2.0f * math.dot(a, a) + math.dot(d, b)) / 3.0f;
      float kz = kk * math.dot(d,a);

      ret = -1.0f;

      float p = ky - kx * kx;
      float p3 = p * p * p;
      float q = kx * (2.0f * kx * kx - 3.0f * ky) + kz;
      float h = q * q + 4.0f * p3;

      if(h >= 0.0f) 
      { 
          h = math.sqrt(h);
          float2 x = (new float2(h, -h) - q) / 2.0f;
          float2 uv = math.sign(x) * math.pow(math.abs(x), 0.33333333f);
          float t = math.clamp(uv.x + uv.y - kx, 0.0f, 1.0f);

          // 1 root
          float3 g = d + (c + b * t) * t;
          ret = new float2(math.dot(g, g), t);
      }
      else
      {
          float z = math.sqrt(-p);
          float v = math.acos(q / (p * z * 2.0f)) / 3.0f;
          float m = math.cos(v);
          float n = math.sin(v) * 1.732050808f;
          float3 t = math.clamp(new float3(m + m,-n - m, n - m) * z - kx, 0.0f, 1.0f);
          
          // 3 roots, but only need two
          float3 g = d + (c + b * t.x) * t.x;
          float dis = math.dot(g, g);
          ret = new float2(dis, t.x);

          g = d + (c + b * t.y) * t.y;
          dis = math.dot(g, g);
          if(dis < ret.x)
            ret = new float2(dis, t.y);
      }
      
      ret.x = math.sqrt(ret.x);
    }

    [BurstCompile]
    public static float Noise(int type, in float3 p, in float3 boundsMin, in float3 boundsMax, in float3 offset, in float3 size, float threshold, int numOctaves, float octaveOffsetFactor, in float3 period)
    {
      float n = 0.0f;
      float f = 1.0f;
      switch ((SdfBrush.NoiseTypeEnum) type)
      {
        case SdfBrush.NoiseTypeEnum.Perlin:
          n = 0.8f * (math.saturate(PNoise(p / size, offset, numOctaves, octaveOffsetFactor, period)) - 0.5f) + 0.5f;
          f = 0.9f;
          break;

        case SdfBrush.NoiseTypeEnum.BakedPerlin:
          n = 0.9f * CachedNoise(p / size, offset, numOctaves, octaveOffsetFactor);
          f = 0.8f;
          break;

        case SdfBrush.NoiseTypeEnum.Triangle:
          n = TriangleNoise(p / size, offset, numOctaves, octaveOffsetFactor);
          f = 0.4f;
          break;
      }

      float d = threshold - n;

      // noise is not an actual SDF
      // we need to scale the result to make it behave like one
      // making the result slightly smaller than it should be would prevent false positive voxel node culling
      d *= f * math.min(math.min(size.x, size.y), size.z);

      return d;
    }

    //-------------------------------------------------------------------------
    // end: primitives


    // noises
    //-------------------------------------------------------------------------

    [BurstCompile]
    public static float Rand(float s)
    {
      float m;
      Mod(s, 6.2831853f, out m);
      return math.frac(math.sin(m) * 43758.5453123f);
    }

    [BurstCompile]
    public static float Rand(in float2 s)
    {
      float d = math.dot(s + 0.1234567f, new float2(1111112.9819837f, 78.237173f));
      float m;
      Mod(d, 6.2831853f, out m);
      return math.frac(math.sin(m) * 43758.5453123f);
    }

    [BurstCompile]
    public static float Rand(in float3 s)
    {
      float d = math.dot(s + 0.1234567f, new float3(11112.9819837f, 378.237173f, 3971977.9173179f));
      float m;
      Mod(d, 6.2831853f, out m);
      return math.frac(math.sin(m) * 43758.5453123f);
    }

    [BurstCompile]
    public static void Mod(float x, float y, out float ret)
    {
      ret = x - y * math.floor(x / y);
    }

    [BurstCompile]
    public static void Mod(in float2 x, in float2 y, out float2 ret)
    {
      ret = x - y * math.floor(x / y);
    }

    [BurstCompile]
    public static void Mod(in float3 x, in float3 y, out float3 ret)
    {
      ret = x - y * math.floor(x / y);
    }

    [BurstCompile]
    public static void Mod(in float4 x, in float4 y, out float4 ret)
    {
      ret = x - y * math.floor(x / y);
    }

    [BurstCompile]
    public static void Mod289(in float2 x, out float2 ret)
    {
      ret = x - math.floor(x / 289.0f) * 289.0f;
    }

    [BurstCompile]
    public static void Mod289(in float3 x, out float3 ret)
    {
      ret = x - math.floor(x / 289.0f) * 289.0f;
    }

    [BurstCompile]
    public static void Mod289(in float4 x, out float4 ret)
    {
      ret = x - math.floor(x / 289.0f) * 289.0f;
    }

    [BurstCompile]
    public static void Permute(in float3 x, out float3 ret)
    {
      Mod289((x * 34.0f + 1.0f) * x, out ret);
    }

    [BurstCompile]
    public static void Permute(in float4 x, out float4 ret)
    {
      Mod289((x * 34.0f + 1.0f) * x, out ret);
    }

    [BurstCompile]
    public static void TaylorInvSqrt(in float3 r, out float3 ret)
    {
      ret = 1.79284291400159f - 0.85373472095314f * r;
    }

    [BurstCompile]
    public static void TaylorInvSqrt(in float4 r, out float4 ret)
    {
      ret = 1.79284291400159f - 0.85373472095314f * r;
    }

    [BurstCompile]
    public static void Fade(in float2 t, out float2 ret)
    {
      ret = t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
    }

    [BurstCompile]
    public static void Fade(in float3 t, out float3 ret)
    {
      ret = t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
    }

    [BurstCompile]
    public static int Index(in int3 id, in int3 dimension)
    {
      return ((id.z * dimension.z + id.y) * dimension.y) + id.x;
    }

    [BurstCompile]
    public static void UnitTriWave(in float3 x, out float3 ret)
    {
      ret = math.abs(x - math.floor(x) - 0.5f);
    }

    [BurstCompile]
    public static float PNoise(in float3 P, in float3 rep)
    {
      float3 Pi0, Pi1;
      Mod(math.floor(P), math.max(MathUtil.Epsilon, rep), out Pi0);
      Mod(Pi0 + (float3)1.0f, math.max(MathUtil.Epsilon, rep), out Pi1); // Integer part + 1, mod period
      float3 Pf0 = math.frac(P); // math.fractional part for interpolation
      float3 Pf1 = Pf0 - (float3)1.0f; // math.fractional part - 1.0f
      float4 ix = new float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
      float4 iy = new float4(Pi0.y, Pi0.y, Pi1.y, Pi1.y);
      float4 iz0 = (float4)Pi0.z;
      float4 iz1 = (float4)Pi1.z;

      float4 perRet;
      Permute(ix, out perRet);
      float4 ixy, ixy0, ixy1;
      Permute(perRet + iy, out ixy);
      Permute(ixy + iz0, out ixy0);
      Permute(ixy + iz1, out ixy1);

      float4 gx0 = ixy0 / 7.0f;
      float4 gy0 = math.frac(math.floor(gx0) / 7.0f) - 0.5f;
      gx0 = math.frac(gx0);
      float4 gz0 = (float4)0.5f - math.abs(gx0) - math.abs(gy0);
      float4 sz0 = math.step(gz0, (float4)0.0f);
      gx0 -= sz0 * (math.step((float4)0.0f, gx0) - 0.5f);
      gy0 -= sz0 * (math.step((float4)0.0f, gy0) - 0.5f);

      float4 gx1 = ixy1 / 7.0f;
      float4 gy1 = math.frac(math.floor(gx1) / 7.0f) - 0.5f;
      gx1 = math.frac(gx1);
      float4 gz1 = (float4)0.5f - math.abs(gx1) - math.abs(gy1);
      float4 sz1 = math.step(gz1, (float4)0.0f);
      gx1 -= sz1 * (math.step((float4)0.0f, gx1) - 0.5f);
      gy1 -= sz1 * (math.step((float4)0.0f, gy1) - 0.5f);

      float3 g000 = new float3(gx0.x,gy0.x,gz0.x);
      float3 g100 = new float3(gx0.y,gy0.y,gz0.y);
      float3 g010 = new float3(gx0.z,gy0.z,gz0.z);
      float3 g110 = new float3(gx0.w,gy0.w,gz0.w);
      float3 g001 = new float3(gx1.x,gy1.x,gz1.x);
      float3 g101 = new float3(gx1.y,gy1.y,gz1.y);
      float3 g011 = new float3(gx1.z,gy1.z,gz1.z);
      float3 g111 = new float3(gx1.w,gy1.w,gz1.w);

      float4 norm0;
      TaylorInvSqrt(new float4(math.dot(g000, g000), math.dot(g010, g010), math.dot(g100, g100), math.dot(g110, g110)), out norm0);
      g000 *= norm0.x;
      g010 *= norm0.y;
      g100 *= norm0.z;
      g110 *= norm0.w;
      float4 norm1;
      TaylorInvSqrt(new float4(math.dot(g001, g001), math.dot(g011, g011), math.dot(g101, g101), math.dot(g111, g111)), out norm1);
      g001 *= norm1.x;
      g011 *= norm1.y;
      g101 *= norm1.z;
      g111 *= norm1.w;

      float n000 = math.dot(g000, Pf0);
      float n100 = math.dot(g100, new float3(Pf1.x, Pf0.y, Pf0.z));
      float n010 = math.dot(g010, new float3(Pf0.x, Pf1.y, Pf0.z));
      float n110 = math.dot(g110, new float3(Pf1.x, Pf1.y, Pf0.z));
      float n001 = math.dot(g001, new float3(Pf0.x, Pf0.y, Pf1.z));
      float n101 = math.dot(g101, new float3(Pf1.x, Pf0.y, Pf1.z));
      float n011 = math.dot(g011, new float3(Pf0.x, Pf1.y, Pf1.z));
      float n111 = math.dot(g111, Pf1);

      float3 fade_xyz;
      Fade(Pf0, out fade_xyz);
      float4 n_z = math.lerp(new float4(n000, n100, n010, n110), new float4(n001, n101, n011, n111), fade_xyz.z);
      float2 n_yz = math.lerp(n_z.xy, n_z.zw, fade_xyz.y);
      float n_xyz = math.lerp(n_yz.x, n_yz.y, fade_xyz.x);
      return 2.2f * n_xyz;
    }

    [BurstCompile]
    public static float PNoise(in float3 s, in float3 offset, int numOctaves, float octaveOffsetFactor, in float3 period)
    {
      float3 sCopy = s;
      float3 offsetCopy = offset;
      float3 periodCopy = period;
      float o = 0.0f;
      float w = 0.5f;
      float wTotal = 0.0f;
      int i = 0;
      do
      {
        o += w * PNoise(sCopy - offsetCopy, periodCopy);
        wTotal += w;
        offsetCopy *= 2.0f * octaveOffsetFactor;
        periodCopy *= 2.0f * octaveOffsetFactor;
        sCopy *= 2.0f;
        w *= 0.5f;
      } while (++i < numOctaves);
      o *= 0.5f / wTotal;
      o += 0.5f;
      return o;
    }

    [BurstCompile]
    // not actually cached, but to match GPU implementation
    public static float CachedNoise(in float3 s)
    {
      float3 dimensions = new float3(256, 128, 256);
      float density = 32.0f;
      float3 sampleInterval = 1.0f / density;
      float3 period = dimensions * sampleInterval;

      return math.saturate(0.8f * PNoise(s, period) + 0.5f) - 0.5f;

      /*
      float3 sQ = s / sampleInterval;
      float3 sL = sampleInterval * math.floor(sQ);
      float3 sH = sL + sampleInterval;
      float3 sT = math.frac(sQ);

      float3 s0 = new float3(sL.x, sL.y, sL.z);
      float3 s1 = new float3(sH.x, sL.y, sL.z);
      float3 s2 = new float3(sL.x, sH.y, sL.z);
      float3 s3 = new float3(sH.x, sH.y, sL.z);
      float3 s4 = new float3(sL.x, sL.y, sH.z);
      float3 s5 = new float3(sH.x, sL.y, sH.z);
      float3 s6 = new float3(sL.x, sH.y, sH.z);
      float3 s7 = new float3(sH.x, sH.y, sH.z);

      float n0 = math.saturate(0.8f * PNoise(s0, period) + 0.5f) - 0.5f;
      float n1 = math.saturate(0.8f * PNoise(s1, period) + 0.5f) - 0.5f;
      float n2 = math.saturate(0.8f * PNoise(s2, period) + 0.5f) - 0.5f;
      float n3 = math.saturate(0.8f * PNoise(s3, period) + 0.5f) - 0.5f;
      float n4 = math.saturate(0.8f * PNoise(s4, period) + 0.5f) - 0.5f;
      float n5 = math.saturate(0.8f * PNoise(s5, period) + 0.5f) - 0.5f;
      float n6 = math.saturate(0.8f * PNoise(s6, period) + 0.5f) - 0.5f;
      float n7 = math.saturate(0.8f * PNoise(s7, period) + 0.5f) - 0.5f;
      
      return
        math.lerp
        (
          math.lerp
          (
            math.lerp(n0, n1, sT.x), 
            math.lerp(n2, n3, sT.x), 
            sT.y
          ), 
          math.lerp
          (
            math.lerp(n4, n5, sT.x), 
            math.lerp(n6, n7, sT.x), 
            sT.y
          ), 
          sT.z
        );
      */
    }

    [BurstCompile]
    // not actually cached, but to match GPU implementation
    public static float CachedNoise(in float3 s, in float3 offset, int numOctaves, float octaveOffsetFactor)
    {
      float3 sCopy = s;
      float3 offsetCopy = offset;
      float o = 0.0f;
      float w = 0.5f;
      float wTotal = 0.0f;
      int i = 0;
      do
      {
        o += w * CachedNoise(sCopy - offsetCopy);
        wTotal += w;
        offsetCopy *= 2.0f * octaveOffsetFactor;
        sCopy *= 2.0f;
        w *= 0.5f;
      } while (++i < numOctaves);
      o *= 0.5f / wTotal;
      o += 0.5f;
      return o;
    }

    [BurstCompile]
    public static float TriangleNoise(in float3 p)
    {
      float3 utw0, utw1, utw2;
      UnitTriWave(p * 0.23f, out utw0);
      UnitTriWave(p * 0.41f + utw0.yzx, out utw1);
      UnitTriWave(p + utw1.zxy, out utw2);
      return math.dot(utw2, (float3) 1.0f) - 0.5f;
    }

    [BurstCompile]
    public static float TriangleNoise(in float3 s, in float3 offset, int numOctaves, in float octaveOffsetFactor)
    {
      float3 sCopy = s;
      float3 offsetCopy = offset;
      float o = 0.0f;
      float w = 0.5f;
      float wTotal = 0.0f;
      int i = 0;
      do
      {
        o += w * TriangleNoise(sCopy - offsetCopy);
        wTotal += w;
        offsetCopy *= 2.0f * octaveOffsetFactor;
        sCopy *= 2.0f;
        w *= 0.5f;
      } while (++i < numOctaves);
      o *= 0.5f / wTotal;
      o += 0.5f;
      return o;
    }

    //-------------------------------------------------------------------------
    // end: noises


    private static readonly int MaxBrushMaskInts = 32;
    private static readonly int BitsPerInt = 32;

    [BurstCompile]
    private struct BrushMask
    {
      public NativeArray<uint> m_ints;

      public struct BrushMaskIterator
      {
        private BrushMask m_mask;
        private uint m_curInt;
        private int m_numInts;
        private int m_iInt;
        private int m_iBrushBase;

        public void Init(BrushMask mask)
        {
          m_mask = mask;
          m_iInt = 0;
          m_iBrushBase = 0;
        }

        public int First()
        {
          m_iInt = 0;
          m_iBrushBase = 0;
          m_curInt = m_mask.m_ints[m_iInt];
          return Next();
        }

        public int Next()
        {
          while (m_iInt < m_mask.m_ints.Length)
          {
            if (m_curInt == 0)
            {
              ++m_iInt;
              m_iBrushBase += BitsPerInt;
              if (m_iInt < m_mask.m_ints.Length)
              {
                m_curInt = m_mask.m_ints[m_iInt];
              }
              continue;
            }

            int iFirstSetBit = (int) Mathf.Log(m_curInt & (~m_curInt + 1u), 2);
            m_curInt &= ~(1u << iFirstSetBit);
            return m_iBrushBase + iFirstSetBit;
          }

          return -1;
        }
      }

      public void Init()
      {
        m_ints = new NativeArray<uint>(MaxBrushMaskInts, Allocator.Temp, NativeArrayOptions.ClearMemory);
      }

      public void Dispose()
      {
        if (m_ints.IsCreated)
          m_ints.Dispose();
      }

      public void SetBit(int bit)
      {
        m_ints[bit / BitsPerInt] |= (1u << (bit % BitsPerInt));
      }

      public void ClearBit(int bit)
      {
        m_ints[bit / BitsPerInt] &= (~(1u << (bit % BitsPerInt)));
      }

      public bool IsBitSet(int bit)
      {
        return (m_ints[bit / BitsPerInt] & (1u << (bit % BitsPerInt))) != 0;
      }

      public BrushMaskIterator GetIterator()
      {
        BrushMaskIterator iter = new BrushMaskIterator();
        iter.Init(this);
        return iter;
      }
    }
#endif

    public struct Ray
    {
      /// <summary>
      /// The starting point of the ray.
      /// </summary>
      public Vector3 From;
      /// <summary>
      /// The direction of the ray.
      /// </summary>
      public Vector3 Direction;
      /// <summary>
      /// The maximum travel distance the ray
      /// </summary>
      public float MaxDistance;
    }

    /// <summary>
    /// Raycast result.
    /// </summary>
    public struct Contact
    {
      /// <summary>
      /// Whether the ray has hit the SDF zero isosurface.
      /// </summary>
      public bool Hit;
      /// <summary>
      /// Whether the ray has reached its maximum number of steps.
      /// </summary>
      public bool MaxStepsReached;
      /// <summary>
      /// Contact position (if hit).
      /// </summary>
      public Vector3 Position;
      /// <summary>
      /// Contact normal (if hit).
      /// </summary>
      public Vector3 Normal;
      /// <summary>
      /// Ratio of the ray's travel distance (until hit or miss) compared to its maximum distance.
      /// <p/>
      /// For a raycast, this is the same as <c>GlobalT</c>.
      /// <br/>
      /// For a raycast chain, this is the ratio local to the last evaluated ray segment.
      /// </summary>
      public float LocalT;
      /// <summary>
      /// Ratio of the ray's travel distance (until hit or miss) compared to its maximum distance.
      /// For a raycast, this is the same as <c>LocalT</c>.
      /// <br/>
      /// For a raycast chain, this is overall ratio global to the entire chain.
      /// </summary>
      public float GlobalT;
      /// <summary>
      /// Material at contact point (if material computation is specified).
      /// </summary>
      public SdfBrushMaterial Material;

      public static Contact New => 
        new Contact()
        {
          Hit = false, 
          MaxStepsReached = false, 
          Position = Vector3.zero, 
          Normal = Vector3.zero, 
          LocalT = -1.0f, 
          GlobalT = -1.0f, 
          Material = SdfBrushMaterial.New, 
        };
    }

    private static NativeArray<Vector3> s_sampleDummy;
    private static NativeArray<Ray> s_castDummy;
    private static NativeArray<Vector3> s_castChainDummy;
    private static NativeArray<Vector3> s_normalDummy;
    private static NativeArray<Contact> s_contactDummy;
    private static NativeArray<SdfBrushMaterial> s_materialDummy;
    private static NativeArray<Result> s_resultDummy;

    internal static void InitAsyncJobData()
    {
      s_sampleDummy = new NativeArray<Vector3>(1, Allocator.Persistent); 
      s_castDummy = new NativeArray<Ray>(1, Allocator.Persistent);
      s_castChainDummy = new NativeArray<Vector3>(1, Allocator.Persistent);
      s_normalDummy = new NativeArray<Vector3>(1, Allocator.Persistent);
      s_contactDummy = new NativeArray<Contact>(1, Allocator.Persistent);
      s_materialDummy = new NativeArray<SdfBrushMaterial>(1, Allocator.Persistent);
      s_resultDummy = new NativeArray<Result>(1, Allocator.Persistent); 
    }

    internal static void DisposeAsyncJobData()
    {
      s_sampleDummy.Dispose();
      s_castDummy.Dispose();
      s_castChainDummy.Dispose();
      s_normalDummy.Dispose();
      s_contactDummy.Dispose();
      s_materialDummy.Dispose();
      s_resultDummy.Dispose();
    }

    /// <summary>
    /// Job handles are associated with SDF evaluation jobs running in parallel. A job handle exposes the interface to query and wait on the completion of its associated job.
    /// </summary>
    public struct EvalJobHandle
    {
      private class Shared
      {
        public bool m_valid = false;
        public bool m_scheduled = false;
        public bool m_completed = false;
        public EvalJob m_job;
        public JobHandle m_hJob;
        public MudRendererBase m_renderer;
      }

      private Shared m_shared;

      /// <summary>
      /// Whether this handle has been associated with a job.
      /// </summary>
      public bool Valid => (m_shared != null) && m_shared.m_valid;

      /// <summary>
      /// Whether a call to the handle's <c>Complete</c> method has been finished, thus whether its associated job is guaranteed to have finished. At this point it's safe to process and dipose of the job's output.
      /// </summary>
      public bool Completed => (m_shared != null) && m_shared.m_completed;

      /// <summary>
      /// Invalidate this job handle, disassociating it with any job.
      /// </summary>
      public void Invalidate() { m_shared = null; }

#if MUDBUN_BURST
      public static EvalJobHandle New(EvalJob job, MudRendererBase renderer)
      {
        return
          new EvalJobHandle()
          {
            m_shared = 
              new Shared()
              {
                m_valid = true, 
                m_scheduled = false, 
                m_completed = false, 
                m_job = job, 
                m_renderer = renderer, 
              }
          };
      }
#endif

      public static EvalJobHandle Empty => new EvalJobHandle();

      internal void Schedule(bool byRenderer)
      {
#if MUDBUN_BURST
        if (m_shared == null)
          return;

        if (m_shared.m_scheduled)
          return;

        if (!byRenderer)
          m_shared.m_renderer.UpdateComputeData();

        switch (m_shared.m_job.Type)
        {
          case EvalJob.TypeEnum.Sdf:
          case EvalJob.TypeEnum.Normal:
          case EvalJob.TypeEnum.SdfAndNormal:
          case EvalJob.TypeEnum.SnapToSurface:
            m_shared.m_hJob = m_shared.m_job.Schedule(m_shared.m_job.Samples.Length, GetJobBatchSize(m_shared.m_job.Samples.Length));
            break;

          case EvalJob.TypeEnum.Raycast:
            m_shared.m_hJob = m_shared.m_job.Schedule(m_shared.m_job.Casts.Length, GetJobBatchSize(m_shared.m_job.Casts.Length));
            break;

          case EvalJob.TypeEnum.RaycastChain:
            m_shared.m_hJob = m_shared.m_job.Schedule(1, 1);
            break;
        }

        if (!byRenderer)
          JobHandle.ScheduleBatchedJobs();

        m_shared.m_scheduled = true;
#endif
      }

      /// <summary>
      /// Wait on the job handle's associated job until it completes. When this method returns, it's safe to process and dispose of the job's output.
      /// </summary>
      public void Complete()
      {
#if MUDBUN_BURST
        if (m_shared == null)
          return;

        if (!m_shared.m_valid)
          return;

        if (!m_shared.m_scheduled)
          Schedule(false);

        if (m_shared.m_completed)
          return;

        m_shared.m_hJob.Complete();
        m_shared.m_job.Dispose();
        m_shared.m_completed = true;
#endif
      }
    }

#if MUDBUN_BURST
    [BurstCompile]
#endif
    public struct EvalJob : IJobParallelFor
    {
#if MUDBUN_BURST
      public enum TypeEnum
      {
        Invalid = -1, 
        Sdf, 
        Normal, 
        SdfAndNormal, 
        Raycast, 
        RaycastChain, 
        SnapToSurface, 
      }

      // input
      public TypeEnum Type;
      public Matrix4x4 WorldToLocal;
      public Matrix4x4 LocalToWorld;
      public Matrix4x4 LocalToWorldIt;
      [ReadOnly] public NativeArray<FunctionPointer<SdfBrushEvalFunc>> SdfEvalFuncMapDense;
      [ReadOnly] public NativeArray<SdfBrushEvalFuncMapEntry> SdfEvalFuncMapSprase;
      [ReadOnly] public NativeArray<Vector3> Samples;
      [ReadOnly] public BrushArray Brushes;
      [ReadOnly] public MaterialArray MaterialsIn;
      [ReadOnly] public AabbTree Tree;
      public int NumBrushes;
      public int RootIndex;
      public float MaxSurfaceDistance;
      public bool ComputeMaterials;
      public float SurfaceShift;
      [ReadOnly] public NativeArray<Ray> Casts;
      [ReadOnly] public NativeArray<Vector3> CastChain;
      public int MaxSteps;
      public float CastMargin;
      public bool ForceZeroBlendUnion;

      // output
      [WriteOnly] public NativeArray<Result> SdfResults;
      [WriteOnly] public NativeArray<Contact> Contacts;

      private bool LookUpBrushFunc(int brushType, out FunctionPointer<SdfBrushEvalFunc> pFunc)
      {
        pFunc = new FunctionPointer<SdfBrushEvalFunc>();

        if (brushType >= 0 
            && brushType < DenseSdfEvalMapSize)
        {
          if (!SdfEvalFuncMapDense.IsCreated)
          {
            //Debug.LogError($"Brush evaluation function for brush type {brushType} not registered.");
            return false;
          }

          pFunc = SdfEvalFuncMapDense[brushType];

          // TODO: return false if pFunc is not registered

          return true;
        }
        else
        {
          if (!SdfEvalFuncMapSprase.IsCreated)
          {
            //Debug.LogError($"Brush evaluation function for brush type {brushType} not registered.");
            return false;
          }

          for (int i = 0; i < SdfEvalFuncMapSprase.Length; ++i)
          {
            if (SdfEvalFuncMapSprase[i].BrushType != brushType)
              continue;

            pFunc = SdfEvalFuncMapSprase[i].Func;
            return true;
          }
        }

        //Debug.LogError($"Brush evaluation function for brush type {brushType} not registered.");
        return false;
      }

      private float ApplyBrush(float res, float groupRes, in SdfBrushMaterial groupMat, ref float3 p, BrushArray aBrush, int iBrush, in SdfBrush b, MaterialArray aMaterial, ref SdfBrushMaterial oMat, bool outputMat, float surfaceShift)
      {
        //Profiler.BeginSample("ApplyBrush");

        float d = EvalBrush(res, ref p, aBrush, iBrush, b);

        if (b.Type == (int) MudBrushGroup.TypeEnum.EndGroup)
          d = groupRes;

        bool isGroupBrush = false;
        switch ((MudBrushGroup.TypeEnum) b.Type)
        {
          case MudBrushGroup.TypeEnum.BeginGroup:
          case MudBrushGroup.TypeEnum.EndGroup:
            isGroupBrush = true;
            break;
        }

        float tMat = 0.0f;
        float blend = ForceZeroBlendUnion ? 0.0f : b.Blend;
        var op = ForceZeroBlendUnion ? SdfBrush.OperatorEnum.Union : (SdfBrush.OperatorEnum) b.Operator;
        switch (op)
        {
          case SdfBrush.OperatorEnum.Union:
            if (!isGroupBrush)
              d -= surfaceShift;
            tMat = DistBlendWeight(res, d, 1.5f);
            res = UniSmooth(res, d, blend);
            break;

          case SdfBrush.OperatorEnum.Subtract:
            if (!isGroupBrush)
              d += surfaceShift;
            res = SubSmooth(res, d, blend);
            tMat = 1.0f - MathUtil.Saturate(2.0f * d / Mathf.Max(MathUtil.Epsilon, blend));
            break;

          case SdfBrush.OperatorEnum.Intersect:
            if (!isGroupBrush)
              d -= surfaceShift;
            res = IntSmooth(res, d, blend);
            tMat = 1.0f - MathUtil.Saturate(-2.0f * d / Mathf.Max(MathUtil.Epsilon, blend));
            break;

          case SdfBrush.OperatorEnum.Dye:
            if (!isGroupBrush)
              d -= surfaceShift;
            tMat = 1.0f - MathUtil.Saturate(Mathf.Max(0.0f, d) / Mathf.Max(MathUtil.Epsilon, blend));
            break;

          default:
            if (b.Operator == (int) MudDistortion.OperatorEnum.Distort)
              res = Mathf.Min(res, d);
            else if (b.Operator == (int) MudModifier.OperatorEnum.Modify)
              res = d;
            break;
        }

        if (b.MaterialIndex >= 0)
        {
          float blendTightness = aMaterial[b.MaterialIndex].MetallicSmoothnessSizeTightness.w;
          if (blendTightness > 0.0f)
          {
            tMat -= 0.5f;
            tMat = 0.5f + 0.5f * math.sign(tMat) * (1.0f - math.pow(math.abs(1.0f - math.abs(2.0f * tMat)), math.pow(1.0f + blendTightness, 5.0f)));
          }

          SdfBrushMaterial iMat = aMaterial[b.MaterialIndex];
          iMat.EmissionHash.a = b.Hash;
          iMat.BrushIndex = b.Index;
          if (b.Type == (int) MudBrushGroup.TypeEnum.EndGroup)
            iMat = groupMat;

          if (outputMat 
              && b.Flags.IsBitSet((int)SdfBrush.FlagBit.ContributeMaterial))
          {
            SdfBrushMaterial oMatNew;
            SdfBrushMaterial.Lerp(oMat, iMat, tMat, out oMatNew);
            oMat = oMatNew;
          }
          else if (tMat > 0.5f)
          {
            // still record brush hash even if not computing or contributing materials (for click selection)
            oMat.EmissionHash.a = iMat.EmissionHash.a;
          }
        }

        //Profiler.EndSample();

        return res;
      }

      private float EvalBrush(float res, ref float3 p, BrushArray aBrush, int iBrush, in SdfBrush b)
      {
        float d = float.MaxValue;

        if (!LookUpBrushFunc(aBrush[iBrush].Type, out var pFunc))
          return d;

        //Profiler.BeginSample("EvalBrush");

        float preMirrorX = p.x;
        bool doMirrorX = b.Flags.IsBitSet((int) SdfBrush.FlagBit.MirrorX);
        if (doMirrorX)
          p.x = Mathf.Abs(p.x);

        bool flipX = b.Flags.IsBitSet((int) SdfBrush.FlagBit.FlipX);
        if (flipX)
          p.x = -p.x;

        Vector3 h = VectorUtil.Abs(0.5f * b.Size);
        Vector3 pRel = Quaternion.Inverse(b.Rotation) * (p - (float3) b.Position);

        //Profiler.BeginSample("Invoke");

        res = pFunc.Invoke(res, ref p, pRel, (SdfBrush*) aBrush.GetUnsafeReadOnlyPtr(), iBrush);

        //Profiler.EndSample();

        if (flipX || doMirrorX)
          p.x = preMirrorX;

        //Profiler.EndSample();

        return res;
      }

      private float EvalSdf(Vector3 p, BrushMask mask, ref SdfBrushMaterial materialOut, bool computeMaterials, float castRadius = 0.0f)
      {
        //Profiler.BeginSample("EvalSdf");

        //Profiler.BeginSample("PrepareApplyBrush");

        int iStack = -1;
        VecStack pStack = new VecStack(MaxBrushGroupDepth, Allocator.Temp);
        FloatStack resStack = new FloatStack(MaxBrushGroupDepth, Allocator.Temp);
        MatStack matStack = new MatStack(MaxBrushGroupDepth, Allocator.Temp);

        float res = float.MaxValue;
        SdfBrushMaterial mat = SdfBrushMaterial.New;
        float3 pFloat3 = p;
        float groupRes = float.MaxValue;
        SdfBrushMaterial groupMat = SdfBrushMaterial.New;

        //Profiler.EndSample();

        var iter = mask.GetIterator();
        for (int iBrush = iter.First(); iBrush >= 0; iBrush = iter.Next())
        {
          //Profiler.BeginSample("Per Brush");

          switch ((MudBrushGroup.TypeEnum) Brushes[iBrush].Type)
          {
            case MudBrushGroup.TypeEnum.BeginGroup:
            {
              iStack = Mathf.Min(MaxBrushGroupDepth - 1, iStack + 1);
              pStack[iStack] = pFloat3;
              resStack[iStack] = res;
              matStack[iStack] = mat;
              res = float.MaxValue;
              mat = SdfBrushMaterial.New;

              bool doMirrorX = Brushes[iBrush].Flags.IsBitSet((int) SdfBrush.FlagBit.MirrorX);
              if (doMirrorX)
                pFloat3.x = Mathf.Abs(pFloat3.x);

              bool flipX = Brushes[iBrush].Flags.IsBitSet((int) SdfBrush.FlagBit.FlipX);
              if (flipX)
                pFloat3.x = -pFloat3.x;

              break;
            }

            case MudBrushGroup.TypeEnum.EndGroup:
            {
              groupRes = res;
              groupMat = mat;
              pFloat3 = pStack[iStack];
              res = resStack[iStack];
              mat = matStack[iStack];

              break;
            }
          }

          res = ApplyBrush(res, groupRes, groupMat, ref pFloat3, Brushes, iBrush, Brushes[iBrush], MaterialsIn, ref mat, ComputeMaterials, SurfaceShift + castRadius);

          switch ((MudBrushGroup.TypeEnum) Brushes[iBrush].Type)
          {
            case MudBrushGroup.TypeEnum.EndGroup:
              iStack = Mathf.Max(-1, iStack - 1);
              break;
          }

          //Profiler.EndSample();
        }

        pStack.Dispose();
        resStack.Dispose();
        matStack.Dispose();

        if (computeMaterials)
          materialOut = mat;

        //Profiler.EndSample();

        return 
          MaxSurfaceDistance > 0.0f 
            ? Mathf.Min(res, MaxSurfaceDistance) 
            : res;
      }

      // AABB query
      private void BuildBrushMask(AabbTree tree, int iRoot, Aabb query, out BrushMask ret)
      {
        //Profiler.BeginSample("BuildBrushMask (AABB query)");

        float margin = 0.0f;
        for (int iBrush = 0; iBrush < NumBrushes; ++iBrush)
        {
          margin = Mathf.Max(margin, Brushes[iBrush].Blend);
        }
        query.Expand(margin);

        ret = new BrushMask();
        ret.Init();

        int stackTop = 0;
        IntStack stack = new IntStack(MaxAabbTreeStackSize, Allocator.Temp);
        stack[stackTop] = iRoot;

        while (stackTop >= 0)
        {
          int index = stack[stackTop--];
          if (index < 0)
            continue;

          if (!Aabb.Intersects(tree[index].Bounds, query))
            continue;

          if (tree[index].ChildA < 0)
          {
            ret.SetBit(tree[index].UserDataIndex);
          }
          else
          {
            stackTop = Mathf.Min(stackTop + 1, MaxAabbTreeStackSize - 1);
            stack[stackTop] = tree[index].ChildA;
            stackTop = Mathf.Min(stackTop + 1, MaxAabbTreeStackSize - 1);
            stack[stackTop] = tree[index].ChildB;
          }
        }

        stack.Dispose();

        //Profiler.EndSample();
      }

      // ray query
      private void BuildBrushMask(AabbTree tree, int iRoot, in Ray query, float margin, out BrushMask ret, out float tMin)
      {
        //Profiler.BeginSample("BuildBrushMask (ray query)");

        for (int iBrush = 0; iBrush < NumBrushes; ++iBrush)
        {
          margin = Mathf.Max(margin, Brushes[iBrush].Blend);
        }

        ret = new BrushMask();
        ret.Init();

        tMin = 1.0f;

        int stackTop = 0;
        IntStack stack = new IntStack(MaxAabbTreeStackSize, Allocator.Temp);
        stack[stackTop] = iRoot;

        while (stackTop >= 0)
        {
          int index = stack[stackTop--];
          if (index < 0)
            continue;

          Aabb expandedBounds = tree[index].Bounds;
          expandedBounds.Expand(margin);
          float t = 
            expandedBounds.Contains(query.From) // starting from inside AABB is okay
              ? 0.0f 
              : expandedBounds.RayCast(query.From, query.Direction * query.MaxDistance);
          if (t < 0.0f || t > 1.0f)
            continue;

          if (tree[index].ChildA < 0)
          {
            ret.SetBit(tree[index].UserDataIndex);
            tMin = Mathf.Min(t, tMin);
          }
          else
          {
            stackTop = Mathf.Min(stackTop + 1, MaxAabbTreeStackSize - 1);
            stack[stackTop] = tree[index].ChildA;
            stackTop = Mathf.Min(stackTop + 1, MaxAabbTreeStackSize - 1);
            stack[stackTop] = tree[index].ChildB;
          }
        }

        stack.Dispose();

        //Profiler.EndSample();
      }

      // point query
      private void BuildBrushMask(AabbTree tree, int iRoot, in Vector3 query, out BrushMask ret)
      {
        Aabb bounds = new Aabb(query - MathUtil.Epsilon * Vector3.one, query + MathUtil.Epsilon * Vector3.one);
        BuildBrushMask(tree, iRoot, bounds, out ret);
      }

      private float EvalSdf(Vector3 p, ref SdfBrushMaterial materialOut, bool computeMaterials, float castRadius = 0.0f)
      {
        Aabb maskQuery = new Aabb(p - Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one, p + Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one);
        BrushMask mask;
        BuildBrushMask(Tree, RootIndex, maskQuery, out mask);
        float result = EvalSdf(p, mask, ref materialOut, computeMaterials, castRadius);
        mask.Dispose();
        return result;
      }

      private Vector3 EvalNormal(Vector3 p, BrushMask mask)
      {
        //Profiler.BeginSample("EvalNormal");

        Vector3 n = Vector3.zero;
        var mat = SdfBrushMaterial.New;
        n += new Vector3( 1.0f, -1.0f, -1.0f) * EvalSdf(p + new Vector3( 1e-4f, -1e-4f, -1e-4f), mask, ref mat, false);
        n += new Vector3(-1.0f, -1.0f,  1.0f) * EvalSdf(p + new Vector3(-1e-4f, -1e-4f,  1e-4f), mask, ref mat, false);
        n += new Vector3(-1.0f,  1.0f, -1.0f) * EvalSdf(p + new Vector3(-1e-4f,  1e-4f, -1e-4f), mask, ref mat, false);
        n += new Vector3( 1.0f,  1.0f,  1.0f) * EvalSdf(p + new Vector3(1e-4f * 1.0001f, 1e-4f * 1.0002f, 1e-4f * 1.0003f), mask, ref mat, false);

        //Profiler.EndSample();

        return VectorUtil.NormalizeSafe(n, Vector3.zero, 1e-10f);
      }

      private Vector3 EvalNormal(Vector3 p)
      {
        Aabb maskQuery = new Aabb(p - Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one, p + Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one);
        BrushMask mask;
        BuildBrushMask(Tree, RootIndex, maskQuery, out mask);
        Vector3 n = EvalNormal(p, mask);
        mask.Dispose();
        return n;
      }

      private static readonly float RayStepRatio = 0.5f;

      private Contact EvalRaycast(Ray ray, ref SdfBrushMaterial materialOut)
      {
        //Profiler.BeginSample("EvalCast");

        /*
        // make sure the query cast starts from outside the root AABB
        float maskQueryOffset = Tree[RootIndex].Bounds.Size.magnitude;
        maskQuery.From -= maskQuery.Direction * maskQueryOffset;
        maskQuery.MaxDistance += maskQueryOffset;
        */

        float tMin;
        BrushMask mask;
        BuildBrushMask(Tree, RootIndex, ray, CastMargin, out mask, out tMin);

        Vector3 p = ray.From + tMin * ray.MaxDistance * ray.Direction;

        var contact = Contact.New;
        float dist = 0.0f;
        for (int iStep = 0; iStep < MaxSteps; ++iStep)
        {
          if (iStep == MaxSteps - 1)
            contact.MaxStepsReached = true;

          float d = EvalSdf(p, mask, ref materialOut, false);

          // within margin?
          if (Mathf.Abs(d) < CastMargin)
          {
            dist = (p - ray.From).magnitude;

            // actually within max distance?
            if (dist <= ray.MaxDistance)
            {
              contact.Hit = true;
              contact.Position = p;
              contact.Normal = EvalNormal(p, mask);
              contact.LocalT = contact.GlobalT = dist / ray.MaxDistance;
              if (ComputeMaterials)
                EvalSdf(p, mask, ref materialOut, true);
  
              break;
            }
          }

          float stepDist = RayStepRatio * d;
          p += stepDist * ray.Direction;
          dist += stepDist;

          // exceed max distance?
          if (dist > ray.MaxDistance)
          {
            p = ray.From + ray.MaxDistance * ray.Direction;

            // still return sensible data that might be useful
            contact.Hit = false;
            contact.Position = p;
            contact.Normal = EvalNormal(p, mask);
            contact.LocalT = contact.GlobalT = 1.0f;
            if (ComputeMaterials)
              EvalSdf(p, mask, ref materialOut, true);

            break;
          }
        }

        mask.Dispose();

        //Profiler.EndSample();
        
        return contact;
      }

      private Contact EvalRaycastChain(NativeArray<Vector3> castChain, ref SdfBrushMaterial materialOut)
      {
        Aabb maskQuery = Aabb.Empty;
        for (int i = 0; i < castChain.Length; ++i)
          maskQuery.Include(castChain[i]);

        BrushMask mask;
        BuildBrushMask(Tree, RootIndex, maskQuery, out mask);

        var contact = Contact.New;

        float totalRayMaxDist = 0.0f;
        for (int i = 0; i < castChain.Length - 1; ++i)
          totalRayMaxDist = (castChain[i + 1] - castChain[i]).magnitude;

        int iStep = -1;
        int iCurrRay = -1;
        Vector3 p = float.MaxValue * Vector3.one;
        Vector3 currRayFrom = float.MaxValue * Vector3.one;
        Vector3 currRayDir = float.MaxValue * Vector3.one;
        float currRayMaxDist = -1.0f;
        float currRayDist = 0.0f;
        float totalRayDist = 0.0f;
        while (++iStep < MaxSteps)
        {
          if (iStep == MaxSteps - 1)
            contact.MaxStepsReached = true;

          if (currRayDist > currRayMaxDist)
          {
            // advance to next ray segment
            ++iCurrRay;
            if (iCurrRay >= castChain.Length - 1)
              break;

            // initialize new ray
            p = castChain[iCurrRay];
            currRayFrom = p;
            currRayMaxDist = (castChain[iCurrRay + 1] - castChain[iCurrRay]).magnitude;
            currRayDist = 0.0f;
          }

          float d = EvalSdf(p, mask, ref materialOut, false);

          // within margin?
          if (Mathf.Abs(d) < CastMargin)
          {
            currRayDist = (p - currRayFrom).magnitude;

            // actually within max distance?
            if (currRayDist <= currRayMaxDist)
            {
              contact.Hit = true;
              contact.Position = p;
              contact.Normal = EvalNormal(p, mask);
              contact.LocalT = currRayDist / currRayMaxDist;
              contact.GlobalT = totalRayDist / totalRayMaxDist;
              if (ComputeMaterials)
                EvalSdf(p, mask, ref materialOut, true);

              break;
            }
          }

          float stepDist = RayStepRatio * d;
          p += stepDist * currRayDir;
          currRayDist += stepDist;
        }

        // went past last ray?
        if (iCurrRay == castChain.Length - 1)
        {
          p = castChain[iCurrRay];

          // still return sensible data that might be useful
          contact.Hit = false;
          contact.Position = p;
          contact.Normal = EvalNormal(p, mask);
          contact.LocalT = contact.GlobalT = 1.0f;
          if (ComputeMaterials)
            EvalSdf(p, mask, ref materialOut, true);
        }

        mask.Dispose();

        return contact;
      }

      private Contact EvalSnapToSurface(Vector3 p, ref SdfBrushMaterial materialOut)
      {
        //Profiler.BeginSample("EvalSnapToSurface");

        Aabb normalMaskQuery = new Aabb(p - Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one, p + Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one);
        BrushMask normalMask;
        BuildBrushMask(Tree, RootIndex, normalMaskQuery, out normalMask);
        Vector3 n = EvalNormal(p, normalMask);
        float d = EvalSdf(p, ref materialOut, false);
        normalMask.Dispose();

        var cast = 
          new Ray()
          {
            From = p, 
            Direction = -n, 
            MaxDistance = MaxSurfaceDistance, 
          };
        var contact = EvalRaycast(cast, ref materialOut);

        //Profiler.EndSample();

        return contact;
      }
#endif

      public void Execute(int index)
      {
#if MUDBUN_BURST
        switch (Type)
        {
          case TypeEnum.Sdf:
          {
            if (RootIndex < 0)
            {
              SdfResults[index] = Result.New(MaxSurfaceDistance, SdfBrushMaterial.New, Vector3.zero);
              return;
            }

            Vector3 p = WorldToLocal.MultiplyPoint(Samples[index]);

            var mat = SdfBrushMaterial.New;
            float res = EvalSdf(p, ref mat, ComputeMaterials);

            SdfResults[index] = Result.New(res, mat, Vector3.zero);
            break;
          }

          case TypeEnum.Normal:
          {
            if (RootIndex < 0)
            {
              SdfResults[index] = Result.New(float.MaxValue, SdfBrushMaterial.New, Vector3.zero);
              return;
            }

            Vector3 p = WorldToLocal.MultiplyPoint(Samples[index]);
            Vector3 n = EvalNormal(p);
            n = LocalToWorldIt.MultiplyVector(n);

            SdfResults[index] = Result.New(float.MaxValue, SdfBrushMaterial.New, n);
            break;
          }

          case TypeEnum.SdfAndNormal:
          {
            if (RootIndex < 0)
            {
              SdfResults[index] = Result.New(MaxSurfaceDistance, SdfBrushMaterial.New, Vector3.zero);
              return;
            }

            Vector3 p = WorldToLocal.MultiplyPoint(Samples[index]);

            Aabb maskQuery = new Aabb(p - Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one, p + Mathf.Max(MathUtil.Epsilon, MaxSurfaceDistance) * Vector3.one);
            BrushMask mask;
            BuildBrushMask(Tree, RootIndex, maskQuery, out mask);

            var mat = SdfBrushMaterial.New;
            float res = EvalSdf(p, mask, ref mat, ComputeMaterials);

            Vector3 n = EvalNormal(p, mask);
            n = LocalToWorldIt.MultiplyVector(n);

            SdfResults[index] = Result.New(res, mat, n);

            mask.Dispose();
            break;
          }

          case TypeEnum.Raycast:
          {
            if (RootIndex < 0)
            {
              Contacts[index] = Contact.New;
              return;
            }

            var cast = Casts[index];
            cast.From = WorldToLocal.MultiplyPoint(cast.From);
            cast.Direction = WorldToLocal.MultiplyVector(cast.Direction).normalized;

            var mat = SdfBrushMaterial.New;
            var contact = EvalRaycast(cast, ref mat);
            if (ComputeMaterials)
              contact.Material = mat;
            contact.Position = LocalToWorld.MultiplyPoint(contact.Position);
            contact.Normal = LocalToWorldIt.MultiplyVector(contact.Normal).normalized;
            Contacts[index] = contact;
            break;
          }

          case TypeEnum.RaycastChain:
          {
            if (RootIndex < 0)
            {
              Contacts[0] = Contact.New;
              return;
            }

            for (int i = 0; i < Casts.Length; ++i)
            {
              CastChain[i] = WorldToLocal.MultiplyPoint(CastChain[i]);
            }

            var mat = SdfBrushMaterial.New;
            var contact = EvalRaycastChain(CastChain, ref mat);
            if (ComputeMaterials)
              contact.Material = mat;
            contact.Position = LocalToWorld.MultiplyPoint(contact.Position);
            contact.Normal = LocalToWorldIt.MultiplyVector(contact.Normal).normalized;
            Contacts[0] = contact;
            break;
          }

          case TypeEnum.SnapToSurface:
          {
            if (RootIndex < 0)
            {
              Contacts[index] = Contact.New;
              return;
            }

            Vector3 p = WorldToLocal.MultiplyPoint(Samples[index]);

            var mat = SdfBrushMaterial.New;
            var contact = EvalSnapToSurface(p, ref mat);
            if (ComputeMaterials)
              contact.Material = mat;
            contact.Position = LocalToWorld.MultiplyPoint(contact.Position);
            contact.Normal = LocalToWorldIt.MultiplyVector(contact.Normal).normalized;
            Contacts[index] = contact;
            break;
          }
        }
#endif
      }

#if MUDBUN_BURST
      public void Dispose()
      {
        // nothing to dispose at this moment
      }
#endif
    }

    private static int GetJobBatchSize(int numJobs) => Mathf.Max(1, numJobs / Mathf.Max(1, SystemInfo.processorCount - 1));

    /// <summary>
    /// Evaluate SDF values.
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="samples"></param>
    /// <param name="results"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="maxDistance"></param>
    /// <param name="computeMaterials"></param>
    /// <param name="surfaceShift"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateSdf
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Vector3> samples, 
      NativeArray<Result> results, 
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      float maxDistance, 
      bool computeMaterials, 
      float surfaceShift
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.Sdf,
          WorldToLocal = renderer.transform.worldToLocalMatrix, 
          LocalToWorld = renderer.transform.localToWorldMatrix, 
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = samples,
          Casts = s_castDummy, 
          CastChain = s_castChainDummy, 
          CastMargin = 0.0f,
          ForceZeroBlendUnion = false, 
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot,
          MaxSurfaceDistance = maxDistance, 
          ComputeMaterials = computeMaterials, 
          SurfaceShift = surfaceShift, 
          SdfResults = results, 
          Contacts = s_contactDummy, 
        };

      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        for (int i = 0; i < samples.Length; ++i)
          job.Execute(i);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }

    /// <summary>
    /// Evaluate SDF normals (normalized gradients).
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="samples"></param>
    /// <param name="results"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="maxDistance"></param>
    /// <param name="surfaceShift"></param>
    /// <param name="h"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateNormal
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Vector3> samples, 
      NativeArray<Result> results, 
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      float maxDistance, 
      float surfaceShift, 
      float h
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.Normal,
          WorldToLocal = renderer.transform.worldToLocalMatrix,
          LocalToWorld = renderer.transform.localToWorldMatrix,
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = samples, 
          Casts = s_castDummy, 
          CastChain = s_castChainDummy, 
          CastMargin = 0.0f,
          ForceZeroBlendUnion = false, 
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot,
          MaxSurfaceDistance = maxDistance, 
          ComputeMaterials = false, 
          SurfaceShift = surfaceShift, 
          SdfResults = results, 
          Contacts = s_contactDummy, 
        };


      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        for (int i = 0; i < samples.Length; ++i)
          job.Execute(i);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }

    /// <summary>
    /// Evaluate SDF values and normals (normalized gradients) simultaneously. More efficient than evaluating separately.
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="samples"></param>
    /// <param name="sdfResults"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="maxDistance"></param>
    /// <param name="computeMaterials"></param>
    /// <param name="surfaceShift"></param>
    /// <param name="h"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateSdfAndNormal
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Vector3> samples, 
      NativeArray<Result> sdfResults,
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      float maxDistance, 
      bool computeMaterials, 
      float surfaceShift,
      float h
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.SdfAndNormal,
          WorldToLocal = renderer.transform.worldToLocalMatrix,
          LocalToWorld = renderer.transform.localToWorldMatrix,
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = samples, 
          Casts = s_castDummy, 
          CastChain = s_castChainDummy, 
          CastMargin = 0.0f, 
          ForceZeroBlendUnion = false,
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot,
          MaxSurfaceDistance = maxDistance, 
          ComputeMaterials = computeMaterials, 
          SurfaceShift = surfaceShift, 
          SdfResults = sdfResults, 
          Contacts = s_contactDummy, 
        };


      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        for (int i = 0; i < samples.Length; ++i)
          job.Execute(i);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }

    /// <summary>
    /// Evaluate raycasts against SDF zero isosurface.
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="casts"></param>
    /// <param name="results"></param>
    /// <param name="castMargin"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="computeMaterials"></param>
    /// <param name="maxSteps"></param>
    /// <param name="surfaceShift"></param>
    /// <param name="forceZeroBlendUnion"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateRaycast
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Ray> casts, 
      NativeArray<Contact> results, 
      float castMargin, 
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      bool computeMaterials, 
      int maxSteps, 
      float surfaceShift, 
      bool forceZeroBlendUnion
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.Raycast, 
          WorldToLocal = renderer.transform.worldToLocalMatrix, 
          LocalToWorld = renderer.transform.localToWorldMatrix, 
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = s_sampleDummy, 
          Casts = casts, 
          CastChain = s_castChainDummy, 
          CastMargin = castMargin,
          ForceZeroBlendUnion = forceZeroBlendUnion, 
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot,
          MaxSurfaceDistance = -1.0f, 
          MaxSteps = maxSteps, 
          ComputeMaterials = computeMaterials, 
          SurfaceShift = surfaceShift,
          SdfResults = s_resultDummy, 
          Contacts = results,
        };

      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        for (int i = 0; i < casts.Length; ++i)
          job.Execute(i);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }

    /// <summary>
    /// Evaluate raycast chains.
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="castChain"></param>
    /// <param name="contact"></param>
    /// <param name="castMargin"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="computeMaterials"></param>
    /// <param name="maxSteps"></param>
    /// <param name="surfaceShift"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateRaycastChain
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Vector3> castChain, 
      NativeArray<Contact> contact, 
      float castMargin, 
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      bool computeMaterials, 
      int maxSteps, 
      float surfaceShift
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.RaycastChain, 
          WorldToLocal = renderer.transform.worldToLocalMatrix, 
          LocalToWorld = renderer.transform.localToWorldMatrix, 
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = s_sampleDummy, 
          Casts = s_castDummy, 
          CastChain = castChain, 
          CastMargin = castMargin, 
          ForceZeroBlendUnion = false, 
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot,
          MaxSurfaceDistance = -1.0f, 
          MaxSteps = maxSteps, 
          ComputeMaterials = computeMaterials, 
          SurfaceShift = surfaceShift,
          SdfResults = s_resultDummy, 
          Contacts = contact, 
        };

      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        job.Execute(0);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }

    /// <summary>
    /// Snap points to closest SDF zero isosurface.
    /// </summary>
    /// <param name="async"></param>
    /// <param name="renderer"></param>
    /// <param name="samples"></param>
    /// <param name="results"></param>
    /// <param name="castMargin"></param>
    /// <param name="aBrush"></param>
    /// <param name="numBrushes"></param>
    /// <param name="aMaterial"></param>
    /// <param name="tree"></param>
    /// <param name="iRoot"></param>
    /// <param name="computeMaterials"></param>
    /// <param name="maxSurfaceDistance"></param>
    /// <param name="maxSteps"></param>
    /// <param name="surfaceShift"></param>
    /// <returns></returns>
    public static EvalJobHandle EvaluateSnapToSurface
    (
      bool async, 
      MudRendererBase renderer, 
      NativeArray<Vector3> samples, 
      NativeArray<Contact> results, 
      float castMargin, 
      BrushArray aBrush, 
      int numBrushes, 
      MaterialArray aMaterial, 
      AabbTree tree, 
      int iRoot, 
      bool computeMaterials, 
      float maxSurfaceDistance, 
      int maxSteps, 
      float surfaceShift
    )
    {
#if !MUDBUN_BURST
      WarnBurstMissing();
      return EvalJobHandle.Empty;
#else
      var job = 
        new EvalJob()
        {
          Type = EvalJob.TypeEnum.SnapToSurface, 
          WorldToLocal = renderer.transform.worldToLocalMatrix, 
          LocalToWorld = renderer.transform.localToWorldMatrix, 
          LocalToWorldIt = renderer.transform.localToWorldMatrix.inverse.transpose, 
          SdfEvalFuncMapDense = s_sdfEvalFuncMapDense, 
          SdfEvalFuncMapSprase = s_sdfEvalFuncMapSparse, 
          Samples = samples, 
          Casts = s_castDummy, 
          CastChain = s_castChainDummy, 
          CastMargin = castMargin,
          ForceZeroBlendUnion = false, 
          Brushes = aBrush, 
          NumBrushes = numBrushes, 
          MaterialsIn = aMaterial, 
          Tree = tree, 
          RootIndex = iRoot, 
          MaxSurfaceDistance = maxSurfaceDistance, 
          MaxSteps = maxSteps, 
          ComputeMaterials = computeMaterials, 
          SurfaceShift = surfaceShift,
          SdfResults = s_resultDummy, 
          Contacts = results, 
        };

      if (async)
      {
        return EvalJobHandle.New(job, renderer);
      }
      else
      {
        for (int i = 0; i < samples.Length; ++i)
          job.Execute(i);
        job.Dispose();
        return EvalJobHandle.Empty;
      }
#endif
    }
  }
}

