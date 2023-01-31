/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

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
  [ExecuteInEditMode]
  public class MudCurveFull : MudSolid
  {
    /*
    [Header("Noise")]

    [SerializeField] private bool m_enableNoise = false;
    [SerializeField] private float m_noiseOffset = 0.0f;
    [SerializeField] private Vector2 m_noiseBaseOctaveSize = 0.5f * Vector2.one;
    [SerializeField] [Range(0.0f, 1.0f)] private float m_noiseThreshold = 0.5f;
    [SerializeField] [Range(1, 3)] private int m_noiseNumOctaves = 2;
    [SerializeField] private float m_noiseOctaveOffsetFactor = 0.5f;
    public bool EnableNoise { get => m_enableNoise; set { m_enableNoise = value; MarkDirty(); } }
    public float NoiseOffset { get => m_noiseOffset; set { m_noiseOffset = value; MarkDirty(); } }
    public Vector2 NoiseBaseOctaveSize { get => m_noiseBaseOctaveSize; set { m_noiseBaseOctaveSize = value; MarkDirty(); } }
    public float NoiseThreshold { get => m_noiseThreshold; set { m_noiseThreshold = value; MarkDirty(); } }
    public int NoiseNumOctaves { get => m_noiseNumOctaves; set { m_noiseNumOctaves = value; MarkDirty(); } }
    public float NoiseOctaveOffsetFactor { get => m_noiseOctaveOffsetFactor; set { m_noiseOctaveOffsetFactor = value; MarkDirty(); } }
    */

    [Serializable]
    public class Point
    {
      public Transform Transform;
      public float Radius;

      public Point(Transform transform = null, float radius = 0.2f)
      {
        Transform = transform;
        Radius = radius;
      }

      public Point(GameObject go, float radius = 0.2f)
      {
        Transform = go?.transform;
        Radius = radius;
      }
    }

    [Header("Shape")]

    [SerializeField] [Range(1, 16)] private int m_precision = 8;
    public int Precision { get => m_precision; set { m_precision = value; MarkDirty(); } }

    public Transform HeadControlPoint;
    public Transform TailControlPoint;
    [SerializeField] private List<Point> m_points = new List<Point>();
    public ICollection<Point> Points
    {
      get => m_points;
      set
      {
        m_points.Clear();
        foreach (var p in value)
          m_points.Add(p);
        
        MarkDirty();
      }
    }

    public MudCurveFull()
    {
      m_points.Add(new Point());
    }

    public override Aabb RawBoundsRs
    {
      get
      {
        Aabb bounds = Aabb.Empty;

        foreach (var p in m_points)
        {
          if (p == null || p.Transform == null)
            continue;

          Vector3 posRs = PointRs(p.Transform.position);
          Vector3 r = 1.5f * p.Radius * Vector3.one;
          bounds.Include(new Aabb(posRs - r, posRs + r));
        }

        return bounds;
      }
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      //Validate.NonNegative(ref m_noiseBaseOctaveSize);

      if (m_points != null)
      {
        foreach(var p in m_points)
        {
          if (p == null || p.Transform == null)
            continue;

          Validate.NonNegative(ref p.Radius);
        }
      }
    }

    private void Update()
    {
      foreach (var p in m_points)
      {
        if (p == null || p.Transform == null)
          continue;

        if (!p.Transform.hasChanged)
          continue;

        MarkDirty();
        p.Transform.hasChanged = false;
      }

      if (HeadControlPoint != null && HeadControlPoint.hasChanged)
      {
        MarkDirty();
        HeadControlPoint.hasChanged = false;
      }

      if (TailControlPoint != null && TailControlPoint.hasChanged)
      {
        MarkDirty();
        TailControlPoint.hasChanged = false;
      }
    }

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      if (m_points == null || m_points.Count == 0)
        return 0;

      if (m_points.Any(p => p == null || p.Transform == null))
        return 0;

      SdfBrush brush = SdfBrush.New();
      brush.Type = (int) SdfBrush.TypeEnum.CurveFull;

      if (m_points.Count == 1)
      {
        return 0;
      }

      int iBrush = iStart;

      brush.Data0.x = m_points.Count + 2;
      brush.Data0.y = Precision;
      brush.Data0.z = 0.0f;//m_enableNoise ? 1.0f : 0.0f;

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        foreach (var p in m_points)
          aBone.Add(p.Transform);
      }

      aBrush[iBrush++] = brush;
      brush.Type = (int) SdfBrush.TypeEnum.Nop;

      /*
      if (m_enableNoise)
      {
        brush.Data0 = new Vector4(m_noiseBaseOctaveSize.x, m_noiseBaseOctaveSize.y, m_noiseBaseOctaveSize.y, m_noiseThreshold);
        brush.Data1 = new Vector4(m_noiseOffset, 0.0f, 0.0f, m_noiseNumOctaves);
        brush.Data2 = new Vector4(m_noiseOctaveOffsetFactor, 0.0f, 0.0f, 0.0f);
        aBrush[iBrush++] = brush;
      }
      */

      int iPreHead = iBrush;
      var head = m_points[0];
      var postHead = m_points[1];
      Vector3 preHeadPosRs = 
        HeadControlPoint == null 
          ? 2.0f * head.Transform.position - postHead.Transform.position 
          : HeadControlPoint.position;
      preHeadPosRs = PointRs(preHeadPosRs);
      brush.Data0 = new Vector4(preHeadPosRs.x, preHeadPosRs.y, preHeadPosRs.z, head.Radius);
      aBrush[iBrush++] = brush;

      for (int i = 0; i < m_points.Count; ++i)
      {
        var p = m_points[i];
        Vector3 pointPosRs = PointRs(p.Transform.position);
        brush.Data0 = new Vector4(pointPosRs.x, pointPosRs.y, pointPosRs.z, p.Radius);
        aBrush[iBrush++] = brush;
      }

      int iPostTail = iBrush;
      var tail = m_points[m_points.Count - 1];
      var preTail = m_points[m_points.Count - 2];
      Vector3 postTailPosRs = 
        TailControlPoint == null 
          ? 2.0f * tail.Transform.position - preTail.Transform.position 
          : TailControlPoint.position;
      postTailPosRs = PointRs(postTailPosRs);
      brush.Data0 = new Vector4(postTailPosRs.x, postTailPosRs.y, postTailPosRs.z, tail.Radius);
      aBrush[iBrush++] = brush;

      if (HeadControlPoint == null)
      {
        Vector3 headPosRs = PointRs(head.Transform.position);
        Vector3 postHeadPosRs = PointRs(postHead.Transform.position);
        Vector3 headControlPosRs = 
          2.0f * headPosRs 
          - VectorUtil.CatmullRom
            (
              preHeadPosRs, 
              headPosRs, 
              postHeadPosRs, 
              aBrush[iPreHead + 3].Data0, 
              0.75f
            );
        var b = aBrush[iPreHead];
        b.Data0 = new Vector4(headControlPosRs.x, headControlPosRs.y, headControlPosRs.z, head.Radius);
        aBrush[iPreHead] = b;
      }

      if (TailControlPoint == null)
      {
        Vector3 tailPosRs = PointRs(tail.Transform.position);
        Vector3 preTailPosRs = PointRs(preTail.Transform.position);
        Vector3 tailControlPosRs = 
          2.0f * tailPosRs 
          - VectorUtil.CatmullRom
            (
              postTailPosRs, 
              tailPosRs, 
              preTailPosRs, 
              aBrush[iPostTail - 3].Data0, 
              0.75f
            );
        var b = aBrush[iPostTail];
        b.Data0 = new Vector4(tailControlPosRs.x, tailControlPosRs.y, tailControlPosRs.z, tail.Radius);
        aBrush[iPostTail] = b;
      }

      return iBrush - iStart;
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.CurveFull)]
    public static unsafe float EvaluateSdf(float resDummy, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float res = float.MaxValue;
      var b = aBrush[iBrush];

      int numPoints = (int) b.Data0.x;
      if (numPoints > 1)
      {
        int precision = (int) b.Data0.y;
        float dt = 1.0f / precision;

        bool useNoise = false;//(b.Data0.z > 0.0f);

        int iA = iBrush + (useNoise ? 2 : 1);
        float globalLen = 0.0f;
        int iClosest = -1;
        float tClosest = 0.0f;
        float segResClosest = 0.0f;
        float rClosest = 0.0f;
        float3 pClosest = 0.0f;
        float closestLen = 0.0f;
        for (int i = 1, n = numPoints - 2; i < n; ++i, ++iA)
        {
          float3 pA = new float4(aBrush[iA + 0].Data0).xyz;
          float3 pB = new float4(aBrush[iA + 1].Data0).xyz;
          float3 pC = new float4(aBrush[iA + 2].Data0).xyz;
          float3 pD = new float4(aBrush[iA + 3].Data0).xyz;
          float3 prevPos = pB;
          float r = aBrush[iA + 1].Data0.w;
          float dr = (aBrush[iA + 2].Data0.w - r) * dt;
          float localLen = 0.0f;
          for (float t = dt; t < 1.0001f; t += dt)
          {
            float3 currPos;
            MathUtil.CatmullRom(pA, pB, pC, pD, math.min(1.0f, t), out currPos);
            float segLen = math.length(currPos - prevPos);
            float d = Sdf.RoundCone(p, prevPos, currPos, r, r + dr);
            if (d < res)
            {
              float2 segRes;
              Sdf.Segment(p, prevPos, currPos, out segRes);
              res = d;
              iClosest = i;
              tClosest = t;
              rClosest = r + dr * segRes.y;
              pClosest = math.lerp(prevPos, currPos, segRes.y);
              closestLen = globalLen + localLen + segLen * segRes.y;
              segResClosest = segRes.y;
            }
            prevPos = currPos;
            r += dr;
            localLen += segLen;
          }
          globalLen += localLen;
        }
      }
      
      return res;
    }
#endif

    public override void DrawSelectionGizmosRs()
    {
      base.DrawSelectionGizmosRs();

      if (m_points == null)
        return;

      if (m_points.Any(p => p == null || p.Transform == null))
        return;

      GizmosUtil.DrawInvisibleCatmullRom
      (
        m_points.Select(p => PointRs(p.Transform.position)).ToArray(), 
        m_points.Select(p => p.Radius).ToArray(), 
        HeadControlPoint != null ? PointRs(HeadControlPoint.position) : VectorUtil.Invalid, 
        TailControlPoint != null ? PointRs(TailControlPoint.position) : VectorUtil.Invalid
      );
    }

    public override void DrawOutlineGizmosRs()
    {
      base.DrawOutlineGizmosRs();

      if (m_points == null)
        return;

      if (m_points.Any(p => p == null || p.Transform == null))
        return;

      GizmosUtil.DrawWireCatmullRom
      (
        m_points.Select(p => PointRs(p.Transform.position)).ToArray(), 
        m_points.Select(p => p.Radius).ToArray(), 
        HeadControlPoint != null ? PointRs(HeadControlPoint.position) : VectorUtil.Invalid, 
        TailControlPoint != null ? PointRs(TailControlPoint.position) : VectorUtil.Invalid
      );
    }
  }
}

