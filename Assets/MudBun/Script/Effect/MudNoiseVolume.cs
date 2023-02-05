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
  public class MudNoiseVolume : MudSolid
  {
    public enum CoordinateSystemEnum
    {
      Cartesian, 
      Spherical, 
    }

    [SerializeField] private SdfBrush.NoiseTypeEnum m_noiseType = SdfBrush.NoiseTypeEnum.BakedPerlin;
    [SerializeField] private CoordinateSystemEnum m_coordinateSystem = CoordinateSystemEnum.Cartesian;
    [SerializeField] private SdfBrush.BoundaryShapeEnum m_boundaryShape = SdfBrush.BoundaryShapeEnum.Box;
    [SerializeField] private float m_boundaryBlend = 0.0f;
    [SerializeField] [ConditionalField("m_boundaryShape", SdfBrush.BoundaryShapeEnum.Sphere, SdfBrush.BoundaryShapeEnum.Cylinder, SdfBrush.BoundaryShapeEnum.Torus, SdfBrush.BoundaryShapeEnum.SolidAngle)]
    private float m_boundaryRadius = 0.4f;
    [SerializeField] [ConditionalField("m_boundaryShape", SdfBrush.BoundaryShapeEnum.SolidAngle)]
    private float m_boundaryAngle = 45.0f;
    [SerializeField] private Vector3 m_offset = Vector3.zero;
    [SerializeField] private Vector3 m_baseOctaveSize = Vector3.one;
    [SerializeField] [Range(0.0f, 1.0f)] private float m_threshold = 0.5f;
    [SerializeField] [Range(0.0f, 1.0f)] private float m_thresholdFade = 0.0f;
    [SerializeField] [Range(1, 3)] private int m_numOctaves = 2;
    [SerializeField] private float m_octaveOffsetFactor = 0.5f;
    [SerializeField] private bool m_lockPosition = false;
    public SdfBrush.NoiseTypeEnum NoiseType { get => m_noiseType; set { m_noiseType = value; MarkDirty(); } }
    public CoordinateSystemEnum CoordinateSystem { get => m_coordinateSystem; set { m_coordinateSystem = value; MarkDirty(); } }
    public SdfBrush.BoundaryShapeEnum BoundaryShape { get => m_boundaryShape; set { m_boundaryShape = value; MarkDirty(); } }
    public float BoundaryBlend { get => m_boundaryBlend; set { m_boundaryBlend = value; MarkDirty(); } }
    public float BoundaryRadius { get => m_boundaryRadius; set { m_boundaryRadius = value; MarkDirty(); } }
    public float BoundaryAngle { get => m_boundaryRadius; set { m_boundaryRadius = value; MarkDirty(); } }
    public Vector3 Offset { get => m_offset; set { m_offset = value; MarkDirty(); } }
    public Vector3 BaseOctaveSize { get => m_baseOctaveSize; set { m_baseOctaveSize = value; MarkDirty(); } }
    public float Threshold { get => m_threshold; set { m_threshold = value; MarkDirty(); } }
    public float ThresholdFade { get => m_thresholdFade; set { m_thresholdFade = value; MarkDirty(); } }
    public int NumOctaves { get => m_numOctaves; set { m_numOctaves = value; MarkDirty(); } }
    public float OctaveOffsetFactor { get => m_octaveOffsetFactor; set { m_octaveOffsetFactor = value; MarkDirty(); } }
    public bool LockPosition { get => m_lockPosition; set { m_lockPosition = value; MarkDirty(); } }

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Aabb bounds = BoundaryShapeBounds(m_boundaryShape, m_boundaryRadius);
        Vector3 r = 0.5f * Mathf.Max(m_baseOctaveSize.x, m_baseOctaveSize.y, m_baseOctaveSize.z) * Vector3.one;

        bounds.Min += posRs - r;
        bounds.Max += posRs + r;

        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_boundaryBlend);
      Validate.NonNegative(ref m_boundaryRadius);
      Validate.NonNegative(ref m_baseOctaveSize);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.UniformNoise;
      brush.Radius = m_boundaryRadius;
      brush.Flags.AssignBit((int) SdfBrush.FlagBit.LockNoisePosition, m_lockPosition);
      brush.Flags.AssignBit((int) SdfBrush.FlagBit.SphericalNoiseCoordinates, (m_coordinateSystem == CoordinateSystemEnum.Spherical));
      brush.Data0 = new Vector4(m_baseOctaveSize.x, m_baseOctaveSize.y, m_baseOctaveSize.z, m_threshold);
      brush.Data1 = new Vector4(m_offset.x, m_offset.y, m_offset.z, m_numOctaves);
      brush.Data2 = new Vector4(m_octaveOffsetFactor, m_thresholdFade, (int) m_boundaryShape, m_boundaryBlend);
      brush.Data3.x = Mathf.Sin(m_boundaryAngle * MathUtil.Deg2Rad);
      brush.Data3.y = Mathf.Cos(m_boundaryAngle * MathUtil.Deg2Rad);
      brush.Data3.z = (int) m_noiseType;

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        aBone.Add(gameObject.transform);
      }

      aBrush[iStart] = brush;

      return 1;
    }

#if MUDBUN_BURST
    [BurstCompile]
    private static float SdfBoundary(in float3 pRel, in SdfBrush b, int shape, ref float fadeDist)
    {
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * b.Size);

      fadeDist = 0.0f;

      float res = float.MaxValue;
      switch ((SdfBrush.BoundaryShapeEnum) shape)
      {
        case SdfBrush.BoundaryShapeEnum.Box:
        {
          res = Sdf.Box(pRelCopy, h);
          fadeDist = math.cmax(h);
          break;
        }

        case SdfBrush.BoundaryShapeEnum.Sphere:
        {
          res = Sdf.Ellipsoid(pRelCopy, b.Radius * b.Size);
          fadeDist = b.Radius * math.cmax(b.Size);
          break;
        }

        case SdfBrush.BoundaryShapeEnum.Cylinder:
        {
          float2 elongation = math.max(0.0f, new float3(b.Size).xz - 1.0f);
          pRelCopy.xz -= math.clamp(pRelCopy.xz, -elongation, elongation);
          res = Sdf.Cylinder(pRelCopy, h.y, b.Radius);
          fadeDist = math.max(b.Radius, h.y);
          break;
        }

        case SdfBrush.BoundaryShapeEnum.Torus:
        {
          float3 hTorus = new float3(h.x + 0.5f * b.Radius, h.y, h.z + 0.5f * b.Radius);
          res = Sdf.Torus(pRelCopy, hTorus.x - hTorus.z, hTorus.z - b.Radius, b.Radius);
          fadeDist = math.max(math.max(h.x, h.z), b.Radius);
          break;
        }

        case SdfBrush.BoundaryShapeEnum.SolidAngle:
        {
          res = Sdf.SolidAngle(pRelCopy, new float2(b.Data3.x, b.Data3.y), b.Radius);
          res = math.max(res, Sdf.Box(pRelCopy, b.Radius * b.Size));
          fadeDist = b.Radius;
          break;
        }
      }

      return res;
    }

    [BurstCompile]
    private static void CartesianToSpherical(in float3 p, out float3 ret)
    {
      float r = math.length(p);
      ret = new float3(r, math.atan2(p.z, p.x), math.acos(p.y / r));
    }

    [BurstCompile]
    private static void SphericalToCartesian(in float3 p, out float3 ret)
    {
      float s = math.sin(p.z);
      ret = p.x * new float3(math.cos(p.y) * s, math.cos(p.z), math.sin(p.y) * s);
    }

    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.UniformNoise)]
    public static unsafe float EvaluateSdf(float resDummy, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      const float kSphericalNoisePeriod = 4.0f;
      const float kCartesianNoisePeriod = 8.0f;

      float3 h = 0.5f * aBrush[iBrush].Size;

      float thresholdFadeDist = MathUtil.Epsilon;
      int boundaryShape = (int) aBrush[iBrush].Data2.z;
      float res = SdfBoundary(pRel, aBrush[iBrush], boundaryShape, ref thresholdFadeDist);
      float thresholdFadeT = math.sqrt(math.saturate(math.length(pRel) / thresholdFadeDist));

      // because noise functions are not real SDFs
      float distScale = 1.0f;

      float3 sample0 = 0.0f, sample1 = 0.0f;
      float3 period0 = 0.0f, period1 = 0.0f;
      float weight0 = 0.0f, weight1 = 0.0f;
      int numSamples = 0;

      bool lockPosition = aBrush[iBrush].Flags.IsBitSet((int) SdfBrush.FlagBit.LockNoisePosition);
      float3 size = new float4(aBrush[iBrush].Data0).xyz;
      float3 offset = new float4(aBrush[iBrush].Data1).xyz;
      if (!aBrush[iBrush].Flags.IsBitSet((int) SdfBrush.FlagBit.SphericalNoiseCoordinates))
      {
        // uniform
        sample0 = lockPosition ? pRel : p;
        period0 = kCartesianNoisePeriod;
        weight0 = 1.0f;
        numSamples = 1;
      }
      else
      {
        // radial
        float3 s0;
        float3 s1;
        CartesianToSpherical(pRel.xzy + MathUtil.Epsilon, out s0);
        CartesianToSpherical(-pRel.xyz + MathUtil.Epsilon, out s1);
        sample0 = s0 * new float3(1.0f, kSphericalNoisePeriod / MathUtil.TwoPi, 1.0f);
        sample1 = s1 * new float3(1.0f, kSphericalNoisePeriod / MathUtil.TwoPi, 1.0f);
        period0 = new float3(kCartesianNoisePeriod, kSphericalNoisePeriod, kCartesianNoisePeriod);
        period1 = new float3(kCartesianNoisePeriod, kSphericalNoisePeriod, kCartesianNoisePeriod);
        weight0 = math.min(sample0.z, MathUtil.Pi - sample0.z) / MathUtil.HalfPi;
        weight1 = 1.0f - weight0;
        numSamples = 2;

        if (!lockPosition)
        {
          sample0 += new float3(aBrush[iBrush].Position);
          sample1 += new float3(aBrush[iBrush].Position);
        }

        distScale = 0.25f; //clamp(s.x, 1.0f, 1.0f);
      }
      float threshold = aBrush[iBrush].Data0.w;
      float thresholdFade = aBrush[iBrush].Data2.y;
      threshold += (1.0f - threshold) * thresholdFade * thresholdFadeT;
      int numOctaves = (int) aBrush[iBrush].Data1.w;
      float octaveOffsetFactor = aBrush[iBrush].Data2.x;
      float boundaryBlend = aBrush[iBrush].Data2.w;
      boundaryBlend = math.max(0.1f * math.cmin(h), boundaryBlend);
      int noiseType = (int) aBrush[iBrush].Data3.z;
      float nRes = 0.0f;
      for (int i = 0; i < numSamples; ++i)
      {
        float n = Sdf.Noise(noiseType, (i == 0 ? sample0 : sample1), -h, h, offset, size, threshold, numOctaves, octaveOffsetFactor, (i == 0 ? period0 : period1));
        nRes += (i == 0 ? weight0 : weight1) * n * distScale;
      }
      return Sdf.IntSmooth(res, nRes, boundaryBlend);
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      Vector3 posRs = PointRs(transform.position);
      Quaternion rotRs = RotationRs(transform.rotation);

      switch (m_boundaryShape)
      {
        case SdfBrush.BoundaryShapeEnum.Box:
          GizmosUtil.DrawInvisibleBox(posRs, transform.localScale, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Sphere:
          GizmosUtil.DrawInvisibleSphere(posRs, m_boundaryRadius, transform.localScale, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Cylinder:
          GizmosUtil.DrawInvisibleCylinder(posRs, m_boundaryRadius, transform.localScale.y, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Torus:
          GizmosUtil.DrawInvisibleTorus
          (
            PointRs(transform.position), 
            m_boundaryRadius, 
            transform.localScale.x, 
            transform.localScale.z, 
            RotationRs(transform.rotation)
          );
          break;

        case SdfBrush.BoundaryShapeEnum.SolidAngle:
          GizmosUtil.DrawInvisibleSphere(posRs, m_boundaryRadius, Vector3.one, Quaternion.identity);
          break;
      }
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      Vector3 posRs = PointRs(transform.position);
      Quaternion rotRs = RotationRs(transform.rotation);

      switch (m_boundaryShape)
      {
        case SdfBrush.BoundaryShapeEnum.Box:
          GizmosUtil.DrawWireBox(posRs, transform.localScale, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Sphere:
          GizmosUtil.DrawWireSphere(posRs, m_boundaryRadius, transform.localScale, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Cylinder:
          GizmosUtil.DrawWireCylinder(posRs, m_boundaryRadius, 0.0f, transform.localScale.y, rotRs);
          break;

        case SdfBrush.BoundaryShapeEnum.Torus:
          GizmosUtil.DrawWireTorus
          (
            PointRs(transform.position), 
            m_boundaryRadius, 
            transform.localScale.x, 
            transform.localScale.z, 
            RotationRs(transform.rotation)
          );
          break;

        case SdfBrush.BoundaryShapeEnum.SolidAngle:
          GizmosUtil.DrawWireSolidAngle(posRs, m_boundaryRadius, m_boundaryAngle * MathUtil.Deg2Rad, rotRs);
          break;
      }
    }
  }
}

