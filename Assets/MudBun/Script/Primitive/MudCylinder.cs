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
  public class MudCylinder : MudSolid
  {
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; set { m_radius = value; MarkDirty(); } }

    [SerializeField] private float m_topRadiusOffset = 0.0f;
    public float TopRadiusOffset { get => m_topRadiusOffset; set { m_topRadiusOffset = value; MarkDirty(); } }

    [SerializeField] private float m_round = 0.0f;
    public float Round { get => m_round; set { m_round = value; MarkDirty(); } }

    [Range(-1.0f, 1.0f)] public float PivotShift = 0.0f;
    public Vector3 PivotShiftOffset => -0.5f * transform.up * PivotShift * transform.localScale.y;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 size = VectorUtil.Abs(transform.localScale);
        float maxRadius = m_radius + Mathf.Max(0.0f, m_topRadiusOffset);
        Vector3 r = new Vector3(maxRadius + Mathf.Max(0.0f, size.x - 1.0f), Mathf.Abs(0.5f * size.y), maxRadius + Mathf.Max(0.0f, size.z - 1.0f));
        Vector3 posRs = PointRs(transform.position) + VectorRs(PivotShiftOffset);
        Aabb bounds = new Aabb(-r, r);
        bounds.Rotate(RotationRs(transform.rotation));
        Vector3 round = m_round * Vector3.one;
        bounds.Min += posRs - round;
        bounds.Max += posRs + round;
        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_radius);
      Validate.NonNegative(ref m_round);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Cylinder;
      brush.Radius = m_radius;
      brush.Data0.x = m_round;
      brush.Data0.y = m_topRadiusOffset;
      brush.Data0.z = PivotShift;

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
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Cylinder)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float pivotShift = aBrush[iBrush].Data0.z;
      pRelCopy.y += pivotShift * h.y;

      float2 elongation = math.max(0.0f, new float3(aBrush[iBrush].Size).xz - 1.0f);
      pRelCopy.xz -= math.clamp(pRelCopy.xz, -elongation, elongation);

      return Sdf.CappedCone(pRelCopy, h.y, aBrush[iBrush].Radius, math.max(0.0f, aBrush[iBrush].Radius + aBrush[iBrush].Data0.y), aBrush[iBrush].Data0.x);
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      Vector3 xCornerOffsetRs = Mathf.Max(0.0f, transform.localScale.x - 1.0f) * VectorRs(transform.right);
      Vector3 zCornerOffsetRs = Mathf.Max(0.0f, transform.localScale.z - 1.0f) * VectorRs(transform.forward);

      GizmosUtil.DrawInvisibleCylinder(PointRs(transform.position) + VectorRs(PivotShiftOffset) + xCornerOffsetRs + zCornerOffsetRs, Radius + Mathf.Max(0.0f, TopRadiusOffset) + Round, transform.localScale.y + 2.0f * Round, RotationRs(transform.rotation));
      GizmosUtil.DrawInvisibleCylinder(PointRs(transform.position) + VectorRs(PivotShiftOffset) + xCornerOffsetRs - zCornerOffsetRs, Radius + Mathf.Max(0.0f, TopRadiusOffset) + Round, transform.localScale.y + 2.0f * Round, RotationRs(transform.rotation));
      GizmosUtil.DrawInvisibleCylinder(PointRs(transform.position) + VectorRs(PivotShiftOffset) - xCornerOffsetRs + zCornerOffsetRs, Radius + Mathf.Max(0.0f, TopRadiusOffset) + Round, transform.localScale.y + 2.0f * Round, RotationRs(transform.rotation));
      GizmosUtil.DrawInvisibleCylinder(PointRs(transform.position) + VectorRs(PivotShiftOffset) - xCornerOffsetRs - zCornerOffsetRs, Radius + Mathf.Max(0.0f, TopRadiusOffset) + Round, transform.localScale.y + 2.0f * Round, RotationRs(transform.rotation));

      Vector3 boxCoreSize = VectorUtil.CompMul(VectorUtil.Max(new Vector3(1.0f, 0.0f, 1.0f), transform.localScale), new Vector3(2.0f, 1.0f, 2.0f)) + new Vector3(-2.0f, 0.0f, -2.0f) + 2.0f * Round * Vector3.one;
      GizmosUtil.DrawInvisibleBox(PointRs(transform.position) + VectorRs(PivotShiftOffset), boxCoreSize + new Vector3(2.0f * Radius, 0.0f, 0.0f), RotationRs(transform.rotation));
      GizmosUtil.DrawInvisibleBox(PointRs(transform.position) + VectorRs(PivotShiftOffset), boxCoreSize + new Vector3(0.0f, 0.0f, 2.0f * Radius), RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireCylinder(PointRs(transform.position) + VectorRs(PivotShiftOffset), Radius + Round, TopRadiusOffset, transform.localScale.y + 2.0f * Round, RotationRs(transform.rotation));
    }
  }
}

