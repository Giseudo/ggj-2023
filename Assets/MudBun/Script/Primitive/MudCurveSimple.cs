/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Unity.Collections;
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
  public class MudCurveSimple : MudSolid
  {
    [Header("Shape")]
  
    [SerializeField] private float m_elongation = 0.0f;
    public float Elongation { get => m_elongation; set { m_elongation = value; MarkDirty(); } }

    public Transform PointA;
    public Transform ControlPoint;
    public Transform PointB;

    [SerializeField] private float m_radiusA = 0.2f;
    public float RadiusA { get => m_radiusA; set { m_radiusA = value; MarkDirty(); } }

    [SerializeField] private float m_radiusControlPoint = -1.0f;
    public float ControlPointRadius { get => m_radiusControlPoint; set { m_radiusControlPoint = value; MarkDirty(); } }

    [SerializeField] private float m_radiusB = 0.2f;
    public float RadiusB { get => m_radiusB; set { m_radiusB = value; MarkDirty(); } }

    [SerializeField] [Range(0.0f, 1.0f)] private float m_smoothStepBlend = 0.0f;
    public float SmoothStepBlend { get =>m_smoothStepBlend; set { m_smoothStepBlend = value; MarkDirty(); } }

    [Header("Noise")]

    [SerializeField] private bool m_enableNoise = false;
    [SerializeField] private float m_noiseOffset = 0.0f;
    [SerializeField] private Vector2 m_noiseBaseOctaveSize = 0.5f * Vector2.one;
    [SerializeField] [Range(0.0f, 1.0f)] private float m_noiseThreshold = 0.45f;
    [SerializeField] [Range(0.0f, 1.0f)] private float m_noiseThresholdFade = 0.0f;
    [SerializeField] [Range(-1.0f, 1.0f)] private float m_noiseThresholdCoreBias = 0.0f;
    [SerializeField] [Range(1, 3)] private int m_noiseNumOctaves = 2;
    [SerializeField] private float m_noiseOctaveOffsetFactor = 0.5f;
    [SerializeField] private float m_noiseTwist = 0.0f;
    [SerializeField] private float m_noiseTwistOffset = 0.0f;
    public bool EnableNoise { get => m_enableNoise; set { m_enableNoise = value; MarkDirty(); } }
    public float NoiseOffset { get => m_noiseOffset; set { m_noiseOffset = value; MarkDirty(); } }
    public Vector2 NoiseBaseOctaveSize { get => m_noiseBaseOctaveSize; set { m_noiseBaseOctaveSize = value; MarkDirty(); } }
    public float NoiseThreshold { get => m_noiseThreshold; set { m_noiseThreshold = value; MarkDirty(); } }
    public float NoiseThresholdFade { get => m_noiseThresholdFade; set { m_noiseThresholdFade = value; MarkDirty(); } }
    public float NoiseThresholdCoreBias { get => m_noiseThresholdCoreBias; set { m_noiseThresholdCoreBias = value; MarkDirty(); } }
    public int NoiseNumOctaves { get => m_noiseNumOctaves; set { m_noiseNumOctaves = value; MarkDirty(); } }
    public float NoiseOctaveOffsetFactor { get => m_noiseOctaveOffsetFactor; set { m_noiseOctaveOffsetFactor = value; MarkDirty(); } }
    public float NoiseTwist { get => m_noiseTwist; set { m_noiseTwist = value; MarkDirty(); } }
    public float NoiseTwistOffset { get => m_noiseTwistOffset; set { m_noiseTwistOffset = value; MarkDirty(); } }

    public override Aabb RawBoundsRs
    {
      get
      {
        if (PointA == null || PointB == null || ControlPoint == null)
          return Aabb.Empty;

        Vector3 a = PointRs(PointA.position);
        Vector3 b = PointRs(PointB.position);
        Vector3 c = PointRs(ControlPoint.position);

        Vector3 r = Mathf.Max(m_radiusA, m_radiusB, m_radiusControlPoint) * Vector3.one;
        Aabb bounds = Aabb.Empty;
        bounds.Include(new Aabb(a - r, a + r));
        bounds.Include(new Aabb(b - r, b + r));
        bounds.Include(new Aabb(c - r, c + r));

        if (m_elongation != 0.0f)
        {
          Vector3 ab = b - a;
          Vector3 ac = c - a;
          Vector3 x = VectorUtil.NormalizeSafe(ab, VectorRs(transform.right));
          Vector3 z = VectorUtil.NormalizeSafe(Vector3.Cross(ab, ac), VectorRs(transform.forward));

          Vector3 e = m_elongation * VectorUtil.Abs(VectorRs(z));
          bounds.Min -= e;
          bounds.Max += e;
        }

        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_elongation);

      Validate.NonNegative(ref m_radiusA);
      Validate.NonNegative(ref m_radiusB);

      Validate.NonNegative(ref m_noiseBaseOctaveSize);

    }

    private Transform[] m_aPoint = new Transform [] { null, null, null };
    private void Update()
    {
      if (m_aPoint[0] != PointA || m_aPoint[1] != PointB || m_aPoint[2] != ControlPoint)
        MarkDirty();

      m_aPoint[0] = PointA;
      m_aPoint[1] = PointB;
      m_aPoint[2] = ControlPoint;
      foreach (var p in m_aPoint)
      {
        if (p == null)
          return;

        if (!p.hasChanged)
          continue;

        MarkDirty();
        p.hasChanged = false;
      }
    }

    public Matrix4x4 m_basis;

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      if (PointA == null || PointB == null || ControlPoint == null)
        return 0;

      Vector3 a = PointA.position;
      Vector3 b = PointB.position;
      Vector3 c = ControlPoint.position;
      Vector3 d = 0.5f * (a + b);
      Vector3 ab = b - a;
      Vector3 ac = c - a;
      Vector3 x = VectorUtil.NormalizeSafe(ab, transform.right);
      Vector3 z = VectorUtil.NormalizeSafe(Vector3.Cross(ab, ac), transform.forward);
      Vector3 y = VectorUtil.NormalizeSafe(Vector3.Cross(z, x), transform.up);
      m_basis = Matrix4x4.TRS(d, RotationRs(Quaternion.LookRotation(z, y)), Vector3.one);
      Matrix4x4 basisInv = m_basis.inverse;
      a = basisInv.MultiplyPoint(a);
      b = basisInv.MultiplyPoint(b);
      c = basisInv.MultiplyPoint(c);

      int iBrush = iStart;
      SdfBrush brush = SdfBrush.New();

      bool colinear = Mathf.Abs(Vector3.Dot(VectorUtil.NormalizeSafe(ab, Vector3.forward), VectorUtil.NormalizeSafe(ac, Vector3.forward))) > 0.99999f;

      brush.Type = (int) SdfBrush.TypeEnum.CurveSimple;
      brush.Data0 = new Vector4(a.x, a.y, a.z, m_radiusA);
      brush.Data1 = new Vector4(b.x, b.y, b.z, m_radiusB);
      brush.Data2 = new Vector4(c.x, c.y, c.z, m_enableNoise ? 1.0f : 0.0f);
      brush.Data3 = new Vector4(m_elongation, m_radiusControlPoint, m_smoothStepBlend, colinear ? 1.0f : 0.0f);

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        aBone.Add(PointA);
        aBone.Add(PointB);
        aBone.Add(ControlPoint);
      }

      aBrush[iBrush++] = brush;

      if (m_enableNoise)
      {
        brush.Type = (int) SdfBrush.TypeEnum.Nop;
        brush.Data0 = new Vector4(m_noiseBaseOctaveSize.x, m_noiseBaseOctaveSize.y, m_noiseBaseOctaveSize.y, m_noiseThreshold);
        brush.Data1 = new Vector4(m_noiseOffset, 0.0f, 0.0f, m_noiseNumOctaves);
        brush.Data2 = new Vector4(m_noiseOctaveOffsetFactor, MathUtil.TwoPi * (m_noiseTwistOffset), MathUtil.TwoPi * (m_noiseTwistOffset + m_noiseTwist), 0.0f);
        brush.Data3 = new Vector4(m_noiseThresholdFade, m_noiseThresholdCoreBias, 0.0f, 0.0f);
        aBrush[iBrush++] = brush;
      }

      return iBrush - iStart;
    }

    public override void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      base.FillBrushData(ref brush, iBrush);

      brush.Position = PointRs(m_basis.MultiplyPoint(Vector3.zero));
      brush.Rotation = RotationRs(m_basis.rotation);
    }

    internal override bool IsSelected()
    {
      if (base.IsSelected())
        return true;

      #if UNITY_EDITOR
      if (PointA?.gameObject != null && Selection.Contains(PointA.gameObject))
        return true;

      if (PointB?.gameObject != null && Selection.Contains(PointB.gameObject))
        return true;

      if (ControlPoint?.gameObject != null && Selection.Contains(ControlPoint.gameObject))
        return true;
      #endif

      return false;
    }

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      if (PointA == null || PointB == null || ControlPoint == null)
        return;

      Vector3 a = PointRs(PointA.position);
      Vector3 b = PointRs(PointB.position);
      Vector3 c = PointRs(ControlPoint.position);

      int n = 8;
      float t = 0.0f;
      float dt = 1.0f / (n - 1);
      for (int i = 0; i < n; ++i)
      {
        GizmosUtil.DrawInvisibleSphere(VectorUtil.BezierQuad(a, b, c, t), Mathf.Lerp(m_radiusA, m_radiusB, t), Vector3.one, Quaternion.identity);
        t += dt;
      }
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.CurveSimple)]
    public static unsafe float EvaluateSdf(float resDummy, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float res = float.MaxValue;
      var b = aBrush[iBrush];
      float3 pRelAdj = pRel;

      float3 pA = new float4(b.Data0).xyz;
      float3 pB = new float4(b.Data1).xyz;
      float3 pC = new float4(b.Data2).xyz;

      float3 pRelRaw = pRelAdj;
      float elongation = b.Data3.x;
      pRelAdj.z -= math.clamp(pRelAdj.z, -elongation, elongation);

      float controlPointR = b.Data3.y;
      float smoothStepBlend = b.Data3.z;
      float r = 0.0f;

      bool colinear = b.Data3.w > 0.0f;
      float2 curRes;
      if (b.Data3.w > 0.0f) // colinear?
        Sdf.Segment(pRelAdj, pA, pB, out curRes);
      else
        Sdf.Bezier(pRelAdj, pA, pC, pB, out curRes);

      if (controlPointR < 0.0f)
      {
        float t = curRes.y;
        r = b.Data0.w + (b.Data1.w - b.Data0.w) * math.lerp(t, math.smoothstep(0.0f, 1.0f, t), smoothStepBlend);
      }
      else
      {
        if (curRes.y < 0.5f)
        {
          float t = 2.0f * curRes.y;
          r = b.Data0.w + (controlPointR - b.Data0.w) * math.lerp(t, math.smoothstep(0.0f, 1.0f, t), smoothStepBlend);
        }
        else
        {
          float t = 2.0f * (curRes.y - 0.5f);
          r = controlPointR + (b.Data1.w - controlPointR) * math.lerp(t, math.smoothstep(0.0f, 1.0f, t), smoothStepBlend);
        }
      }
      res = curRes.x - r;

      bool useNoise = (b.Data2.w > 0.0f);
      if (useNoise)
      {
        float curveLen = 0.0f;
        int precision = 16;
        float dt = 1.0f / precision;
        float t = dt;
        float3 prevPos = pA;
        for (int i = 1; i < precision; ++i, t += dt)
        {
          float3 currPos;
          MathUtil.BezierQuad(pA, pC, pB, t, out currPos);
          curveLen += math.length(currPos - prevPos);
          prevPos = currPos;
        }
        if (curRes.y < 0.0001f)
          curRes.y = math.min(0.0f, -math.dot(math.normalize(pA - pC), pRelAdj - pA) / curveLen);
        else if (curRes.y > 0.9999f)
          curRes.y = math.max(1.0f, 1.0f + math.dot(math.normalize(pB - pC), pRelAdj - pB) / curveLen);

        float3 up = math.normalize(new float3(0.0f, 1.0f, 0.0f) + 1e-3f * Sdf.Rand(pA));
        float3 front = math.normalize(math.lerp(pA - pC, pC - pB, curRes.y));
        float3 left = math.normalize(math.cross(up, front));
        up = math.cross(front, left);
        float3 closest;
        MathUtil.BezierQuad(pA, pC, pB, curRes.y, out closest);
        float3 pDelta = pRelRaw - closest;
        float3 s = new float3(curRes.y * curveLen, math.dot(pDelta, up), math.dot(pDelta, left));

        // advance to additional noise data
        b = aBrush[iBrush + 1];

        float thresholdFade = b.Data3.x;
        float thresholdCoreBias = b.Data3.y;

        // twist
        float twistA = b.Data2.y;
        float twistB = b.Data2.z;
        float twistT = math.lerp(twistA, twistB, curRes.y);
        float twistCos = math.cos(twistT);
        float twistSin = math.sin(twistT);
        s.yz = math.mul(new float2x2(twistCos, twistSin, -twistSin, twistCos), s.yz);

        float3 offset = new float4(b.Data1).xyz;
        float3 size = new float4(b.Data0).xyz;
        float threshold = b.Data0.w;
        float rDelta = math.length(pDelta);
        float coreBiasT = 1.0f - math.saturate(rDelta / math.max(MathUtil.Epsilon, r));
        threshold = math.saturate(threshold + math.sign(thresholdCoreBias) * math.abs(thresholdCoreBias) * coreBiasT);
        threshold += (1.0f - threshold) * thresholdFade * math.saturate(curRes.y);
        int numOctaves = (int) b.Data1.w;
        float octaveOffsetFactor = b.Data2.x;

        float twistSdfMult = 1.0f / (1.0f + math.saturate(math.abs(twistA - twistB))); // hack: evlauate more surrounding voxels when twisted to avoid holes
        float n = twistSdfMult * Sdf.Noise((int) SdfBrush.NoiseTypeEnum.BakedPerlin, s, float.MinValue, float.MaxValue, offset, size, threshold, numOctaves, octaveOffsetFactor, 8.0f);
        res = Sdf.IntSmooth(res, n, 0.5f * r);
      }

      return res;
    }
#endif

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      if (PointA != null)
      {
        GizmosUtil.DrawWireSphere(PointRs(PointA.position), m_radiusA, Vector3.one, RotationRs(PointA.rotation));
      }

      if (PointB != null)
      {
        GizmosUtil.DrawWireSphere(PointRs(PointB.position), m_radiusB, Vector3.one, RotationRs(PointB.rotation));
      }

      if (ControlPoint != null)
      {
        float da = (ControlPoint.position - PointA.position).magnitude;
        float db = (ControlPoint.position - PointB.position).magnitude;
        float r = m_radiusControlPoint >= 0.0f ? m_radiusControlPoint : Mathf.Lerp(m_radiusA, m_radiusB, da / (da + db));
        GizmosUtil.DrawWireSphere(PointRs(ControlPoint.position), r, Vector3.one, RotationRs(ControlPoint.rotation));
      }

      if (PointA != null && PointB != null && ControlPoint != null)
      {
        GizmosUtil.DrawBezierQuad(PointRs(PointA.position), PointRs(PointB.position), PointRs(ControlPoint.position));
      }
    }
  }
}

