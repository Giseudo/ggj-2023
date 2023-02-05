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
  public class MudSolidAngle : MudSolid
  {
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; set { m_radius = value; MarkDirty(); } }

    [SerializeField] [Range(0.0f, 180.0f)] private float m_angle = 45.0f;
    public float Angle { get => m_angle; set { m_angle = value; MarkDirty(); } }

    [SerializeField] private float m_round = 0.0f;
    public float Round { get => m_round; set { m_round = value; MarkDirty(); } }

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 r = (m_radius + m_round) * VectorUtil.Abs(transform.localScale);
        Aabb bounds = new Aabb(-r, r);
        bounds.Min += posRs;
        bounds.Max += posRs;
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
      brush.Type = (int) SdfBrush.TypeEnum.SolidAngle;
      brush.Radius = m_radius;
      brush.Data0.x = Mathf.Sin(m_angle * MathUtil.Deg2Rad);
      brush.Data0.y = Mathf.Cos(m_angle * MathUtil.Deg2Rad);
      brush.Data0.z = m_round;

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
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.SolidAngle)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      return Sdf.SolidAngle(pRel, new float2(aBrush[iBrush].Data0.x, aBrush[iBrush].Data0.y), aBrush[iBrush].Radius, aBrush[iBrush].Data0.z);
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleSphere(PointRs(transform.position), Radius + Round, Vector3.one, Quaternion.identity);
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireSolidAngle(PointRs(transform.position), Radius + Round, m_angle * MathUtil.Deg2Rad, RotationRs(transform.rotation));
    }
  }
}

