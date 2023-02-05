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
  public class MudTorus : MudSolid
  {
    [SerializeField] private float m_elongation = 0.0f;
    public float Elongation { get => m_elongation; set { m_elongation = value; MarkDirty(); } }

    public float Radius
    {
      get => Mathf.Abs(0.25f * transform.localScale.y);
      set
      {
        transform.localScale = new Vector3(transform.localScale.x, 4.0f * value, transform.localScale.z);
        MarkDirty();
      }
    }

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 size = VectorUtil.Abs(transform.localScale);
        Vector3 r = new Vector3(0.5f * size.x, m_elongation, 0.5f * size.z) + Radius * Vector3.one;
        Aabb bounds = new Aabb(-r, r);
        bounds.Rotate(RotationRs(transform.rotation));
        Vector3 round = Radius * Vector3.one;
        bounds.Min += posRs;
        bounds.Max += posRs;
        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_elongation);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Torus;
      brush.Radius = Radius;

      brush.Data0.x = m_elongation;

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
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Torus)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 pRelCopy = pRel;
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float elongation = aBrush[iBrush].Data0.x;
      pRelCopy.y -= math.clamp(pRelCopy.y, -elongation, elongation);
      float3 hTorus = new float3(h.x + 0.5f * aBrush[iBrush].Radius, h.y, h.z + 0.5f * aBrush[iBrush].Radius);
      float r = math.abs(0.25f * aBrush[iBrush].Size.y);
      return Sdf.Torus(pRelCopy, hTorus.x - hTorus.z, hTorus.z - r, r);
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleTorus
      (
        PointRs(transform.position), 
        0.25f * transform.localScale.y, 
        transform.localScale.x, 
        transform.localScale.z, 
        RotationRs(transform.rotation)
      );
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireTorus
      (
        PointRs(transform.position), 
        0.25f * transform.localScale.y, 
        transform.localScale.x, 
        transform.localScale.z, 
        RotationRs(transform.rotation)
      );
    }
  }
}

