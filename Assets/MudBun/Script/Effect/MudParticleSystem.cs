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
  [ExecuteInEditMode]
  public class MudParticleSystem : MudSolid
  {
    public ParticleSystem Particles;

    [SerializeField] private float m_radiusMultiplier = 1.0f;
    public float RadiusMultiplier { get => m_radiusMultiplier; set { m_radiusMultiplier = value; MarkDirty(); } }

    [SerializeField] private float m_selfBlend = 0.5f;
    public float SelfBlend { get => m_selfBlend; set { m_selfBlend = value; MarkDirty(); } }

    private static readonly int InitNumParticles = 16;
    private int m_lastReadFrame = -1;

    // read particle data
    private ParticleSystem.Particle [] m_aParticle = new ParticleSystem.Particle[InitNumParticles];
    private int m_numParticles = 0;
    private Vector3 [] m_aPosWs = new Vector3[InitNumParticles];
    private Vector3 [] m_aPosRs = new Vector3[InitNumParticles];
    private float [] m_aRadius = new float[InitNumParticles];
    private float [] m_aSelfBlendMult = new float[InitNumParticles];

    public override Aabb RawBoundsRs
    {
      get
      {
        if (!ReadParticles())
          return Aabb.Empty;

        Aabb bounds = Aabb.Empty;
        for (int i = 0, n = Particles.particleCount; i < n; ++i)
        {
          Vector3 posCs = m_aPosRs[i];
          Vector3 r = (m_aRadius[i] + m_selfBlend) * Vector3.one;
          bounds.Include(new Aabb(posCs - r, posCs + r));
        }

        return bounds;
      }
    }

    private bool ReadParticles()
    {
      if (Particles == null)
        return false;

      if (!Particles.isPlaying)
        return m_numParticles > 0;

      if (m_lastReadFrame >= Time.renderedFrameCount)
        return m_aParticle.Length >= Particles.particleCount;

      if (m_aParticle.Length < Particles.particleCount)
      {
        int newLen = m_aParticle.Length;
        while (newLen < Particles.particleCount)
          newLen *= 2;

        m_aParticle = new ParticleSystem.Particle[newLen];
        m_aPosWs = new Vector3[newLen];
        m_aPosRs = new Vector3[newLen];
        m_aRadius = new float[newLen];
        m_aSelfBlendMult = new float[newLen];
      }

      float selfBlendSizeFactor = Mathf.Clamp(5.0f / Mathf.Max(MathUtil.Epsilon, Particles.main.startSizeMultiplier), 0.1f, 100.0f);
      Particles.GetParticles(m_aParticle, Particles.particleCount);
      for (int i = 0, n = Particles.particleCount; i < n; ++i)
      {
        m_aPosWs[i] = Particles.gameObject.transform.TransformPoint(m_aParticle[i].position);
        m_aRadius[i] = m_aParticle[i].GetCurrentSize(Particles) * m_radiusMultiplier;
        m_aSelfBlendMult[i] = Mathf.Clamp01(m_aParticle[i].GetCurrentSize(Particles) * selfBlendSizeFactor);
      }

      switch (Particles.main.simulationSpace)
      {
        case ParticleSystemSimulationSpace.Local:
          break;
        case ParticleSystemSimulationSpace.World:
          for (int i = 0, n = Particles.particleCount; i < n; ++i)
            m_aPosWs[i] = transform.InverseTransformPoint(m_aPosWs[i]);
          break;
        case ParticleSystemSimulationSpace.Custom:
          if (Particles.main.customSimulationSpace != null)
            for (int i = 0, n = Particles.particleCount; i < n; ++i)
              m_aPosWs[i] = Particles.main.customSimulationSpace.InverseTransformPoint(m_aPosWs[i]);
          break;
      }

      for (int i = 0, n = Particles.particleCount; i < n; ++i)
        m_aPosRs[i] = PointRs(m_aPosWs[i]);

      m_lastReadFrame = Time.renderedFrameCount;
      m_numParticles = Particles.particleCount;
      return true;
    }

    private void LateUpdate()
    {
      if (!ReadParticles())
        return;

      MarkDirty();
    }

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_radiusMultiplier);
      Validate.NonNegative(ref m_selfBlend);
    }

    private int m_iStart = -1;

    public override int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone)
    {
      m_iStart = -1;

      if (!ReadParticles())
        return 0;

      m_iStart = iStart;

      SdfBrush brush = SdfBrush.New();

      if (aBone != null)
      {
        brush.BoneIndex = aBone.Count;
        aBone.Add(gameObject.transform);
      }

      for (int i = 0, n = m_numParticles; i < n; ++i)
      {
        if (i == 0)
        {
          brush.Type = (int) SdfBrush.TypeEnum.ParticleSystem;
          brush.Data2.x = n;
        }
        else
        {
          brush.Type = (int) SdfBrush.TypeEnum.Nop;
        }

        Vector3 posCs = m_aPosRs[i];
        float r = m_aRadius[i];
        brush.Data0 = new Vector4(posCs.x, posCs.y, posCs.z, r);

        // fade out self blend as particles die off to avoid pops
        brush.Data1.x = m_selfBlend * Mathf.Clamp01(10.0f * (m_aSelfBlendMult[i] - 0.1f));

        aBrush[iStart++] = brush;
      }

      return iStart - m_iStart;
    }

    public override void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      if (m_iStart < 0)
        return;

      // only need to fill in the first one
      if (iBrush == m_iStart)
        base.FillBrushData(ref brush, iBrush);

      brush.Position = m_aPosWs[brush.Index - m_iStart];
    }

#if MUDBUN_BURST
    [BurstCompile]
    [RegisterSdfBrushEvalFunc(SdfBrush.TypeEnum.ParticleSystem)]
    public static unsafe float EvaluateSdf(float resDummy, ref float3 p, in float3 pRel, SdfBrush* aBrush, int iBrush)
    {
      float res = float.MaxValue;
      int numParticles = (int) aBrush[iBrush].Data2.x;
      for (int i = 0; i < numParticles; ++i)
      {
        float3 pos = new float4(aBrush[iBrush + i].Data0).xyz;
        float r = aBrush[iBrush + i].Data0.w;
        float selfBlend = aBrush[iBrush + i].Data1.x;
        res = Sdf.UniSmooth(res, Sdf.Sphere(p - pos, r), selfBlend);
      }
      return res;
    }
#endif
  }
}

