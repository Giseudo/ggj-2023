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

#if UNITY_EDITOR
using UnityEditor;
#endif

using Unity.Collections;
using UnityEngine;

namespace MudBun
{
  /// <summary>
  /// The base class for all brushes. Users should extend one of the MudSolid, MudDistortion, or MudModifier classes to create custom brushes.
  /// </summary>
  [ExecuteInEditMode]
  public abstract class MudBrushBase : MonoBehaviour
  {
    internal MudRendererBase m_renderer;
    internal int m_iSdfBrush;
    internal bool m_dirty = true;

    #if UNITY_EDITOR
    [HideInInspector] public bool Hidden = false;
    #else
    public bool Hidden => false;
    #endif

    public MudRendererBase Renderer => m_renderer;

    public void MarkDirty() { m_dirty = true; }

    /// <summary>
    /// The actual bounds in renderer space used for spatial optimization. Normally this would be the same as BoundsRs, but for special situations like solid brushes with symmetry turned on, this would be a modified version of BoundsRs.
    /// </summary>
    public virtual Aabb BoundsRs => RawBoundsRs;
    /// <summary>
    /// The raw AABB bounds in renderer space. This should be always encompass the brush's maximum range of effect. Otherwise, parts of the brush's effect could be missing due to the renderer's spatial optimization.
    /// </summary>
    public virtual Aabb RawBoundsRs => Aabb.Empty;
    public virtual float BoundsRsPadding => 0.0f;
    public virtual bool IsSuccessorModifier => false;
    public virtual bool ShouldUseAccumulatedBounds => false;

    internal bool m_preChildrenFlag = false;
    public virtual bool IsBrushGroup => false;

    internal virtual bool UsesMaterial => false;
    internal virtual int MaterialHash => 0;

    internal int m_iProxy = AabbTree<MudBrushBase>.Null;
    public virtual void UpdateProxies(AabbTree<MudBrushBase> tree, Aabb opBounds)
    {
      if (m_iProxy == AabbTree<MudBrushBase>.Null)
        m_iProxy = tree.CreateProxy(opBounds, this);

      tree.UpdateProxy(m_iProxy, opBounds, m_iSdfBrush);
    }

    public virtual void DestroyProxies(AabbTree<MudBrushBase> tree)
    {
      tree.DestroyProxy(m_iProxy);
      m_iProxy = AabbTree<MudBrushBase>.Null;
    }

    public Aabb BoundaryShapeBounds(SdfBrush.BoundaryShapeEnum boundaryShape, float radius)
    {
      Vector3 size = transform.localScale;
      Aabb bounds = Aabb.Empty;
      switch (boundaryShape)
      {
        case SdfBrush.BoundaryShapeEnum.Box:
          {
            Vector3 r = 0.5f * size;
            bounds = new Aabb(-r, r);
            bounds.Rotate(RotationRs(transform.rotation));
            break;
          }

        case SdfBrush.BoundaryShapeEnum.Sphere:
          {
            Vector3 r = radius * size;
            bounds = new Aabb(-r, r);
            bounds.Rotate(RotationRs(transform.rotation));
            break;
          }

        case SdfBrush.BoundaryShapeEnum.Cylinder:
          {
            Vector3 r = new Vector3(radius + Mathf.Max(0.0f, size.x - 1.0f), 0.5f * size.y, radius + Mathf.Max(0.0f, size.z - 1.0f));
            bounds = new Aabb(-r, r);
            bounds.Rotate(RotationRs(transform.rotation));
            break;
          }

        case SdfBrush.BoundaryShapeEnum.Torus:
          {
            Vector3 r = new Vector3(0.5f * size.x, 0.0f, 0.5f * size.z);
            bounds = new Aabb(-r, r);
            bounds.Rotate(RotationRs(transform.rotation));
            Vector3 round = radius * Vector3.one;
            bounds.Min -= round;
            bounds.Max += round;
            break;
          }

        case SdfBrush.BoundaryShapeEnum.SolidAngle:
          {
            Vector3 r = radius * VectorUtil.Abs(transform.localScale);
            bounds = new Aabb(-r, r);
            break;
          }
      }

      return bounds;
    }

    protected virtual void ScanRenderer() { }

    public virtual void OnEnable()
    {
      m_renderer = null;
      m_iProxy = AabbTree<MudBrushBase>.Null;
      m_iSdfBrush = -1;
      MarkDirty();

      ScanRenderer();
    }

    public virtual void OnDisable()
    {
      if (m_renderer != null)
        m_renderer.OnBrushDisabled(this);

#if UNITY_EDITOR
      SelectionManager.NotifyBrushDisabled(this);
#endif
    }

    private void OnValidate()
    {
      SanitizeParameters();
      MarkDirty();
    }

    public virtual void SanitizeParameters() { }

    // Ws: world space
    // Rs: renderer space
    // Bs: brush space

    public Vector3 PointRs(Vector3 posWs)
    {
      return 
        m_renderer != null 
          ? m_renderer.transform.InverseTransformPoint(posWs) 
          : posWs;
    }

    public Vector3 VectorRs(Vector3 vecWs)
    {
      return 
        m_renderer != null 
          ? m_renderer.transform.InverseTransformVector(vecWs) 
          : vecWs;
    }

    public Quaternion RotationRs(Quaternion rotWs)
    {
      return
        m_renderer != null
          ? Quaternion.Inverse(m_renderer.transform.rotation) * rotWs
          : rotWs;
    }

    public Vector3 PointBs(Vector3 posWs) => transform.InverseTransformPoint(posWs);
    public Vector3 VectorBs(Vector3 vecWs) => transform.InverseTransformVector(vecWs);
    public Quaternion RotationBs(Quaternion rotWs) => Quaternion.Inverse(transform.rotation) * rotWs;

    public virtual bool CountAsBone => false;

    public virtual int FillComputeData(NativeArray<SdfBrush> aBrush, int iStart, List<Transform> aBone) { return 0; }
    public virtual void FillBrushData(ref SdfBrush brush, int iBrush)
    {
      brush.Proxy = m_iProxy;

      brush.Position = PointRs(transform.position);
      brush.Rotation = RotationRs(transform.rotation);
      brush.Size = transform.localScale;

      brush.Flags.AssignBit((int) SdfBrush.FlagBit.Hidden, Hidden);
    }

    public virtual int FillComputeDataPostChildren(NativeArray<SdfBrush> aBrush, int iStart) { return 0; }
    public virtual void FillBrushDataPostChildren(ref SdfBrush brush, int iBrush) { }

    public virtual void FillBrushMaterialData(ref SdfBrushMaterial mat) { }

    public virtual void ValidateMaterial() { }

    private struct Locator
    {
      public Vector3 Position;
      public Quaternion Rotation;
    }

    public ICollection<Transform> GetFlipXTransforms()
    {
      var aTransform = new List<Transform>();
      CollectChildrenRecursive(transform, aTransform);
      return aTransform;
    }

    public void FlipX()
    {
      var aTransform = new List<Transform>();
      CollectChildrenRecursive(transform, aTransform);
      var aMirroredLocator = new List<Locator>(aTransform.Count);

      for (int i = 0; i < aTransform.Count; ++i)
      {
        var t = aTransform[i];
        Locator loc = new Locator() { Position = t.position, Rotation = t.rotation };
        loc.Position.x = -loc.Position.x;
        loc.Rotation.y = -loc.Rotation.y;
        loc.Rotation.z = -loc.Rotation.z;
        aMirroredLocator.Add(loc);
      }

      for (int i = 0; i < aTransform.Count; ++i)
      {
        var t = aTransform[i];
        var loc = aMirroredLocator[i];
        t.position = loc.Position;
        t.rotation = loc.Rotation;
      }
    }

    private void CollectChildrenRecursive(Transform t, List<Transform> aTransform)
    {
      aTransform.Add(t);
      for (int i = 0; i < t.childCount; ++i)
        CollectChildrenRecursive(t.GetChild(i), aTransform);
    }

    public float GetFloatHash() => Mathf.Abs(Codec.Hash(GetHashCode()) % 0xFFFF) / ((float) 0xFFFF);

    internal virtual bool IsSelected()
    {
      #if UNITY_EDITOR
      bool selected = Selection.Contains(gameObject);
      #else
      bool selected = false;
      #endif
      return selected;
    }

    protected virtual void OnDrawGizmos()
    {
      if (Renderer == null)
        return;

      if (Renderer.ClickSelection != MudRendererBase.ClickSelectionEnum.Gizmos)
        return;

      Gizmos.matrix = Renderer.transform.localToWorldMatrix;
      DrawSelectionGizmosRs();
      Gizmos.matrix = Matrix4x4.identity;
    }

    public virtual void DrawGizmosRs() { }
    public virtual void DrawSelectionGizmosRs() { }
    public virtual void DrawOutlineGizmosRs() { }
  }
}


