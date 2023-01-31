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
  public class MudPinch : MudDistortion
  {
    [SerializeField] private float m_radius = 0.5f;
    public float Radius { get => m_radius; set { m_radius = value; MarkDirty(); } }

    [SerializeField] private float m_depth = 1.0f;
    public float Depth { get => m_depth; set { m_depth = value; MarkDirty(); } }

    [SerializeField] [Range(0.0f, 1.0f)] private float m_amount = 1.0f;
    public float Amount { get => m_amount; set { m_amount = value; MarkDirty(); } }

    [SerializeField] [Range(1.0f, 10.0f)] private float m_strength = 2.0f;
    public float Strength { get => m_strength; set { m_strength = value; MarkDirty(); } }

    public override float MaxDistortion => Depth;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 r = new Vector3(m_radius, Depth, m_radius);
        Aabb bounds = new Aabb(-r, new Vector3(r.x, 0.0f, r.z));
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
      Validate.Saturate(ref m_amount);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.Pinch;
      brush.Radius = m_radius;
      brush.Data0.x = m_depth;
      brush.Data0.y = m_amount;
      brush.Data0.z = m_strength;
      aBrush[iStart] = brush;

      return 1;
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.Pinch)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 pRelCopy = pRel;
      float depth = aBrush[iBrush].Data0.x;
      float r = math.length(pRelCopy.xz);
      if (Sdf.Cylinder(pRelCopy + new float3(0.0f, 0.5f * depth, 0.0f), 0.5f * depth, aBrush[iBrush].Radius) > 0.0f)
        return res;

      float amount = aBrush[iBrush].Data0.y;
      float strength = aBrush[iBrush].Data0.z;
      float g = -pRelCopy.y / depth;
      float t = r / math.max(MathUtil.Epsilon, aBrush[iBrush].Radius);
      float pinchRatio = math.pow(math.abs(1.0f - t), strength);
      g = math.pow(math.abs(g), 0.5f);
      pRelCopy.y = -g * depth; // remap
      float fade = (depth + pRelCopy.y) / depth;
      p += new float3((amount * pinchRatio * fade) * (aBrush[iBrush].Rotation * new Vector3(0.0f, pRelCopy.y, 0.0f)));

      return res;
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      GizmosUtil.DrawInvisibleCone(PointRs(transform.position) + VectorRs(-m_depth * transform.up), m_radius, m_depth, RotationRs(transform.rotation));
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      GizmosUtil.DrawWireCone(PointRs(transform.position) + VectorRs(-m_depth * transform.up), m_radius, m_depth, RotationRs(transform.rotation));
    }
  }
}

