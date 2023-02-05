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
  public class CustomDistortion : MudDistortion
  {
    // this value matches kCustomDistortion in CustomBrush.cginc
    public static readonly int TypeId = 901;

    [SerializeField] private float m_cellSize = 0.25f;
    public float CellSize { get => m_cellSize; set { m_cellSize = value; MarkDirty(); } }

    [SerializeField] [Range(1.0f, 10.0f)] private float m_strength = 5.0f;
    public float Strength { get => m_strength; set { m_strength = value; MarkDirty(); } }

    [SerializeField] [Range(0.0f, 1.0f)] private float m_fade = 1.0f;
    public float Fade { get => m_fade; set { m_fade = value; MarkDirty(); } }

    public override float MaxDistortion => CellSize;

    public override Aabb RawBoundsRs
    {
      get
      {
        Vector3 posRs = PointRs(transform.position);
        Vector3 r = 0.5f * VectorUtil.Abs(transform.localScale) + (m_fade + m_cellSize) * Vector3.one;
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

      Validate.AtLeast(1e-2f, ref m_cellSize);
      Validate.AtLeast(1.0f, ref m_strength);
      Validate.NonNegative(ref m_fade);
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      SdfBrush brush = SdfBrush.New();
      brush.Type = TypeId;
      brush.Data0.x = m_cellSize;
      brush.Data0.y = m_strength;
      brush.Data0.z = m_fade * m_cellSize;
      aBrush[iStart] = brush;

      return 1;
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(901)]
    public static unsafe float EvaluateSdf(float res, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float3 h = math.abs(0.5f * aBrush[iBrush].Size);
      float cellSize = aBrush[iBrush].Data0.x;
      float fade = aBrush[iBrush].Data0.z;
      float d = Sdf.Box(pRel, h, fade * cellSize);
      if (d > 0.0f)
        return res;

      float strength = aBrush[iBrush].Data0.y;
      float3 r = p / cellSize;
      float3 f = math.floor(r);
      float3 t = r - f;
      float3 q = (f + math.smoothstep(0.0f, 1.0f, math.max(1.0f, strength) * (t - 0.5f) + 0.5f)) * cellSize;
      p = math.lerp(p, q, math.saturate(strength) * math.saturate(-d / math.max(MathUtil.Epsilon, fade * cellSize)));

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

