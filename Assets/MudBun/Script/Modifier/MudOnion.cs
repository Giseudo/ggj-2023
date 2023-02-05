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
  public class MudOnion : MudModifier
  {
    [SerializeField] private float m_thickness = 0.1f;
    public float Thickness { get => m_thickness; set { m_thickness = value; MarkDirty(); } }

    public override float MaxModification => m_thickness;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 r = 0.5f * VectorUtil.Abs(transform.localScale) + m_thickness * Vector3.one;
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

      Validate.AtLeast(1e-2f, ref m_thickness);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Onion;
      brush.Data0.x = m_thickness;
      aBrush[iStart] = brush;

      return 1;
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Onion)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float d = Sdf.Box(pRel, h, aBrush[iBrush].Blend);
      if (d > 0.0f)
        return res;

      float thickness = aBrush[iBrush].Data0.x;
      res = math.abs(res) - thickness;
      return res;
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleBox(PointRs(transform.position), transform.localScale, RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireBox(PointRs(transform.position), transform.localScale, RotationRs(transform.rotation));
    }
  }
}

