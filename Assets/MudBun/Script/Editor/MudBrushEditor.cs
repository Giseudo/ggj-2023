/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using UnityEditor;
using UnityEngine;

namespace MudBun
{
  [CustomEditor(typeof(MudBrush), true)]
  [CanEditMultipleObjects]
  public class MudBrushEditor : MudEditorBase
  {
    public override void OnInspectorGUI()
    {
      if (targets.Length > 1)
      {
        Header("* Multiple Brushes Selected *");
      }
      else
      {
        Header("Utilities");

        var brush = (MudBrush) target;
        var renderer = brush.Renderer;

        if (GUILayout.Button("Select Renderer"))
        {
          if (renderer != null)
          {
            Selection.activeGameObject = renderer.gameObject;
            return;
          }
        }

        {
          var t = brush.transform.parent;
          while (t != null)
          {
            var group = t.GetComponent<MudBrushGroup>();
            if (group != null)
            {
              if (GUILayout.Button("Select Brush Group"))
              {
                Selection.activeGameObject = group.gameObject;
                return;
              }
            }

            t = t.parent;
          }
        }

        if (GUILayout.Button("Flip X"))
        {
          var transformToFlip = brush.GetFlipXTransforms();
          var aFlippedTransform = new Transform[transformToFlip.Count];
          transformToFlip.CopyTo(aFlippedTransform, 0);
          Undo.RecordObjects(aFlippedTransform, "Flip X");

          brush.FlipX();
        }

        if (GUILayout.Button("Duplicate"))
        {
          var duplicateGo = Instantiate(brush.gameObject);
          duplicateGo.transform.SetParent(brush.transform.parent);
          duplicateGo.transform.SetSiblingIndex(brush.transform.GetSiblingIndex() + 1);
          Undo.RegisterCreatedObjectUndo(duplicateGo, duplicateGo.name);
          Selection.activeGameObject = duplicateGo;
        }
      }

      Space();

      Header("Brush Parameters");

      base.OnInspectorGUI();
    }

    public Aabb GetChildBounds(Transform t)
    {
      Aabb bounds = Aabb.Empty;
      GetChildBounds(t, ref bounds);
      return bounds;
    }

    private void GetChildBounds(Transform t, ref Aabb bounds)
    {
      if (t == null)
        return;

      var renderer = t.GetComponent<MudRenderer>();
      if (renderer != null)
        return;

      var brush = t.GetComponent<MudBrush>();
      if (brush != null)
        bounds.Include(brush.BoundsRs);

      for (int i = 0; i < t.childCount; ++i)
        GetChildBounds(t.GetChild(i), ref bounds);
    }

    public bool HasFrameBounds()
    {
      var brush = (MudBrush) target;

      if (brush is MudBrushGroup)
      {
        Aabb bounds = GetChildBounds(brush.transform);
        if (bounds.IsEmpty)
          return false;
      }
        
      return true;
    }

    public Bounds OnGetFrameBounds()
    {
      var brush = (MudBrush) target;
      var renderer = brush.Renderer;
      if (renderer == null)
        return new Bounds(brush.transform.position, Vector3.one);

      var bounds = brush.BoundsRs;

      if (brush is MudBrushGroup)
      {
        bounds = GetChildBounds(brush.transform);
        if (bounds.IsEmpty)
          return new Bounds(brush.transform.position, Vector3.one);
      }

      bounds.Expand(renderer.SurfaceShift);
      bounds.Transform(renderer.transform);

      return new Bounds(bounds.Center, bounds.Size);
    }
  }
}

