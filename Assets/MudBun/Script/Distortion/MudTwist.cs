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
  public class MudTwist : MudDistortion
  {
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; set { m_radius = value; MarkDirty(); } }

    [SerializeField] private float m_angle = 90.0f;
    public float Angle { get => m_angle; set { m_angle = value; MarkDirty(); } }

    [SerializeField] [Range(1.0f, 5.0f)] private float m_strength = 1.0f;
    public float Strength { get => m_strength; set { m_strength = value; MarkDirty(); } }

    public override float MaxDistortion => 
      2.0f * Mathf.Sin(0.5f * Mathf.Min(MathUtil.Pi, Mathf.Abs(m_angle * MathUtil.Deg2Rad))) * m_radius;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 size = VectorUtil.Abs(transform.localScale);
        Vector3 r = new Vector3(m_radius, 0.5f * size.y, m_radius);
        Aabb bounds = new Aabb(-r, r);
        bounds.Rotate(RotationRs(transform.rotation));
        bounds.Min += posRs;
        bounds.Max += posRs;
        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_radius);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Twist;
      brush.Radius = m_radius;
      brush.Data0.x = m_angle * MathUtil.Deg2Rad;
      brush.Data0.y = m_strength;
      aBrush[iStart] = brush;

      return 1;
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Twist)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      if (Sdf.Cylinder(pRelCopy, h.y, aBrush[iBrush].Radius, 0.0f) > 0.0f)
        return res;

      float angle = aBrush[iBrush].Data0.x;
      float strength = aBrush[iBrush].Data0.y;
      float r = math.length(pRelCopy.xz);
      float t = r / aBrush[iBrush].Radius;
      float a = angle * (1.0f - math.pow(math.abs(t), strength));
      float s = math.sin(a);
      float c = math.cos(a);
      pRelCopy.xz = math.mul(new float2x2(c, -s, s, c), pRelCopy.xz);
      p = new float3(aBrush[iBrush].Rotation * pRelCopy + aBrush[iBrush].Position);

      return res;
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleCylinder(PointRs(transform.position), m_radius, transform.localScale.y, RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireCylinder(PointRs(transform.position), m_radius, 0.0f, transform.localScale.y, RotationRs(transform.rotation));
    }
  }
}

