/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEngine;
using UnityEngine.Rendering;

namespace MudBun
{
  [ExecuteInEditMode]
  [RequireComponent(typeof(MudMaterial))]
  public class MudSolid : MudBrush
  {
    public enum SymmetryMode
    {
      None, 
      FlipX, 
      MirrorX, 
      FlipMirrorX, 
    }

    public static Aabb SymmetryBounds(SymmetryMode mode, Aabb bounds)
    {
      switch (mode)
      {
        case SymmetryMode.FlipX:
        {
          Vector3 center = bounds.Center;
          Vector3 extent = bounds.Extent;
          center.x = -center.x;
          bounds = new Aabb(center - extent, center + extent);
          break;
        }
          
        case SymmetryMode.MirrorX:
        {
          Vector3 newMin = bounds.Min;
          newMin.x = -Mathf.Max(0.0f, bounds.Max.x);
          bounds = new Aabb(newMin, bounds.Max);
          break;
        }

        case SymmetryMode.FlipMirrorX:
        {
          Vector3 center = bounds.Center;
          Vector3 extent = bounds.Extent;
          center.x = -center.x;
          bounds = new Aabb(center - extent, center + extent);
          Vector3 newMin = bounds.Min;
          newMin.x = -Mathf.Max(0.0f, bounds.Max.x);
          bounds = new Aabb(newMin, bounds.Max);
          break;
        }
      }

      if (bounds.IsEmpty)
        return Aabb.Empty;

      return bounds;
    }

    public override Aabb BoundsRs => SymmetryBounds(m_symmetry, base.BoundsRs);

    [SerializeField] private SdfBrush.OperatorEnum m_operator = SdfBrush.OperatorEnum.Union;
    public SdfBrush.OperatorEnum Operator { get => m_operator; set { m_operator = value; MarkDirty(); } }

    [SerializeField] private float m_blend;
    public float Blend { get => m_blend; set { m_blend = value; MarkDirty(); } }

    // TODO: not ready for auto-rigging yet
    [SerializeField] private SymmetryMode m_symmetry = SymmetryMode.None;
    public SymmetryMode Symmetry { get => m_symmetry; set { m_symmetry = value; MarkDirty(); } }

    [Tooltip("If checked, this brush will be counted as bone during auto rigging.")]
    [SerializeField] [ConditionalField("m_canCountAsBone", true)] public bool m_countAsBone = true;
    [SerializeField] [HideInInspector] protected bool m_canCountAsBone = true;
    public override bool CountAsBone => m_countAsBone || (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal); // Metal is weird
    //[ConditionalField("m_mirrorX", true, Label = "  Create Mirrored Bone")] public bool CreateMirroredBone = true;

    public override float BoundsRsPadding => m_blend;
    public override bool ShouldUseAccumulatedBounds
    {
      get
      {
        switch (m_operator)
        {
          case SdfBrush.OperatorEnum.Intersect:
          case SdfBrush.OperatorEnum.CullOutside:
            return true;
        }
        return false;
      }
    }

    internal override bool UsesMaterial => true;
    internal override int MaterialHash => GetComponent<MudMaterial>().MaterialHash;

    internal MudMaterial m_material;

    public override void SanitizeParameters()
    {
      base.SanitizeParameters();

      Validate.NonNegative(ref m_blend);
    }

    public override void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      base.FillBrushData(ref brush, iBrush);
      
      brush.Operator = (int) m_operator;
      brush.Blend = Blend;

      if (!m_material)
        m_material = GetComponent<MudMaterial>();
#if MUDBUN_DEV
      Assert.True(m_material != null, "Mussing brush material. A solid brush must have a MudMaterial component.");
#endif

      brush.Flags.AssignBit((int) SdfBrush.FlagBit.ContributeMaterial, m_material.ContributeMaterial);

      switch (m_symmetry)
      {
        case SymmetryMode.FlipX:
          brush.Flags.SetBit((int) SdfBrush.FlagBit.FlipX);
          break;

        case SymmetryMode.MirrorX:
          brush.Flags.SetBit((int) SdfBrush.FlagBit.MirrorX);
          break;

        case SymmetryMode.FlipMirrorX:
          brush.Flags.SetBit((int) SdfBrush.FlagBit.FlipX);
          brush.Flags.SetBit((int) SdfBrush.FlagBit.MirrorX);
          break;
      }

      brush.Flags.AssignBit((int) SdfBrush.FlagBit.CountAsBone, CountAsBone);
      //brush.Flags.AssignBit((int) SdfBrush.FlagBit.CreateMirroredBone, CreateMirroredBone);
    }

    public override void FillBrushMaterialData(ref SdfBrushMaterial mat)
    {
      base.FillBrushMaterialData(ref mat);

      if (!m_material)
        m_material = GetComponent<MudMaterial>();
#if MUDBUN_DEV
      Assert.True(m_material != null, "Missing brush material. A solid brush must have a MudMaterial component.");
#endif

      mat.Color = m_material.Color;
      mat.EmissionHash = m_material.Emission;
      mat.MetallicSmoothnessSizeTightness.Set(m_material.Metallic, m_material.Smoothness, m_material.SplatSize, m_material.BlendTightness);
      mat.TextureWeight.Set
      (
        (m_material.TextureIndex == 0 ? 1.0f : 0.0f), 
        (m_material.TextureIndex == 1 ? 1.0f : 0.0f), 
        (m_material.TextureIndex == 2 ? 1.0f : 0.0f), 
        (m_material.TextureIndex == 3 ? 1.0f : 0.0f)
      );
    }

    public override void ValidateMaterial()
    {
      var material = GetComponent<MudMaterial>();
      if (material != null)
        return;

      material = gameObject.AddComponent<MudMaterial>();
    }

    private bool OperatorShouldDrawOutline(SdfBrush.OperatorEnum op)
    {
      if (Renderer != null 
          && Renderer.ClickSelection != MudRendererBase.ClickSelectionEnum.Gizmos)
          return false;

      switch (op)
      {
        case SdfBrush.OperatorEnum.Union:
        {
          if (Renderer == null)
            break;

          if (Renderer.RenderModeCategory != MudRendererBase.RenderModeCategoryEnum.Splats)
            break;

          var material = GetComponent<MudMaterial>();
          if (material == null)
            break;
          
          return 
            Renderer.SplatSize * material.SplatSize < 0.1f 
            || material.Color.a < 0.25f;
        }

        case SdfBrush.OperatorEnum.Subtract:
        case SdfBrush.OperatorEnum.Intersect:
        case SdfBrush.OperatorEnum.CullInside:
        case SdfBrush.OperatorEnum.CullOutside:
        case SdfBrush.OperatorEnum.Dye:
        case SdfBrush.OperatorEnum.NoOp:
          return true;
      }

      return false;
    }

    public override void DrawGizmosRs()
    {
      base.DrawGizmosRs();

      bool selected = IsSelected();

      bool shouldDrawOutlines = 
        selected 
        || (Renderer != null 
            && (Renderer.Enable2dMode
                || Renderer.AlwaysDrawGizmos));

      if (!shouldDrawOutlines)
      {
        shouldDrawOutlines = OperatorShouldDrawOutline(m_operator);
      }

      if (!shouldDrawOutlines)
      {
        var parent = transform.parent;
        while (parent != null)
        {
          var groupComp = parent.GetComponent<MudBrushGroup>();
          if (groupComp != null)
          {
            shouldDrawOutlines = OperatorShouldDrawOutline(groupComp.Operator);
            break;
          }

          if (parent == m_renderer)
            break;
            
          parent = parent.parent;
        }
      }

      if (shouldDrawOutlines)
      {
        Color prevColor = Gizmos.color;
        if (selected)
          Gizmos.color = GizmosUtil.OutlineSelected;

        DrawOutlineGizmosRs();

        Gizmos.color = prevColor;
      }
    }
  }
}

