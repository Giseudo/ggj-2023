using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace MudBun
{
  [ExecuteAlways]
  public class SelectionManager
  {
#if UNITY_EDITOR
    private static Ray s_lastRay;
    private static Sdf.Contact s_lastHit;

    internal static void NotifyRendererDisabled(MudRendererBase renderer)
    {
      if (s_lastHoveredRenderer == renderer)
        s_lastHoveredRenderer = null;

      if (s_lastMouseDownRenderer == renderer)
        s_lastMouseDownRenderer = null;

      System.Func<Object, bool> filter =
        x => 
          (x != null) 
          && (x is GameObject) 
          && ((GameObject) x).TryGetComponent(out MudRendererBase r) 
          && r != renderer;

      s_selectedObjectsOnMouseDown = s_selectedObjectsOnMouseDown.Where(filter).ToList();
      s_lastSelectedObjects = s_lastSelectedObjects.Where(filter).ToList();
    }

    internal static void NotifyBrushDisabled(MudBrushBase brush)
    {
      if (s_lastHoveredBrush == brush)
        s_lastHoveredBrush = null;

      if (s_lastMouseDownBrush == brush)
        s_lastMouseDownBrush = null;

      System.Func<Object, bool> filter = 
        x =>
          (x != null) 
          && (x is GameObject) 
          && ((GameObject) x).TryGetComponent(out MudBrushBase b) 
          && b != brush;

      s_selectedObjectsOnMouseDown = s_selectedObjectsOnMouseDown.Where(filter).ToList();
      s_lastSelectedObjects = s_lastSelectedObjects.Where(filter).ToList();
    }

    internal static void Init()
    {
      SceneView.duringSceneGui += OnScene;
      Selection.selectionChanged += OnSelectionChanged;
      EditorApplication.update += Update;
    }

    internal static void Dispose()
    {
      SceneView.duringSceneGui -= OnScene;
      Selection.selectionChanged -= OnSelectionChanged;
      EditorApplication.update -= Update;
    }

    //private static int s_lastMouseDownFrame = -1;
    private static int s_lastMouseMoveFrame = -1;
    private static float s_lastMouseMoveTime = 0;
    private static MudRendererBase s_lastHoveredRenderer;
    private static MudRendererBase s_lastMouseDownRenderer;
    private static MudBrushBase s_lastHoveredBrush;
    private static MudBrushBase s_lastMouseDownBrush;
    private static Vector2 s_mouseMovePos;
    private static Vector2 s_mouseDownPos;
    private static List<Object> s_selectedObjectsOnMouseDown = new List<Object>();
    private static List<Object> s_lastSelectedObjects = new List<Object>();

    private static void OnScene(SceneView sceneView)
    {
      /*
      if (Application.isPlaying)
        return;

      if (!UnityEditorInternal.InternalEditorUtility.isApplicationActive)
        return;
      */

      if (EditorApplication.isPaused)
        return;

      Event e = Event.current;

      if (e.alt)
        return;

      int frame = Time.renderedFrameCount;

      float ppp = EditorGUIUtility.pixelsPerPoint;
      int mouseScreenX = (int) (e.mousePosition.x * ppp);
      int mouseScreenY = (int) (e.mousePosition.y * ppp);
      if (mouseScreenX < 0 
          || mouseScreenX >= sceneView.camera.pixelWidth 
          || mouseScreenY < 0 
          || mouseScreenY >= sceneView.camera.pixelHeight)
        return;

      int controlID = GUIUtility.GetControlID(FocusType.Passive);
      var controlEventType = e.GetTypeForControl(controlID);

      switch (e.type)
      {
        case EventType.Repaint:
          sceneView.Repaint();
          break;

        case EventType.Used:
        /*
        case EventType.MouseUp:
          if (controlID != GUIUtility.hotControl)
            break;

          GUIUtility.hotControl = 0;

          if ((e.mousePosition - s_mouseDownPos).magnitude > 3.0f)
            break;
        */
          if (s_lastMouseDownBrush == null)
            break;

          if (s_lastMouseDownRenderer == null)
            break;

          // stabilize original selection
          var selection = s_lastSelectedObjects;
          foreach (var obj in s_selectedObjectsOnMouseDown)
          {
            if (!selection.Contains(obj))
            {
              selection.Add(obj);
            }
          }

          var brushGo = s_lastMouseDownBrush.gameObject;
          var rendererGo = s_lastMouseDownRenderer.gameObject;

          var selectedGo = brushGo;


          System.Func<Object, bool> filter = 
            x => 
              (x != null) 
              && (x is GameObject) 
              && ((GameObject) x).TryGetComponent(out MudBrushBase b) 
              && (b.Renderer != null) 
              && (b.Renderer == s_lastMouseDownRenderer);

          if (!selection.Contains(rendererGo) 
              && !selection.Any(filter))
          {
            selectedGo = rendererGo;
          }

          if (e.shift || e.control)
          {
            if (selection.Contains(selectedGo))
              selection.Remove(selectedGo);
            else
              selection.Add(selectedGo);
          }
          else
          {
            selection.Clear();
            selection.Add(selectedGo);
          }

          Selection.objects = selection.ToArray();
          s_lastMouseDownRenderer.MarkNeedsCompute();
          EditorApplication.QueuePlayerLoopUpdate();

          s_lastSelectedObjects = selection;

          s_lastMouseDownBrush = null;
          s_lastMouseDownRenderer = null;
          s_selectedObjectsOnMouseDown.Clear();
          break;

        case EventType.MouseDown:
          if (e.button != 0)
            break;

          if (s_lastHoveredBrush == null)
            break;

          if (s_lastHoveredRenderer == null)
            break;

          if (!e.shift && !e.control)
            s_mouseDownPos = e.mousePosition;

          /*
          if (!e.shift 
              && !e.control 
              && Selection.objects.Contains(s_lastHoveredBrush.gameObject))
            break;

          GUIUtility.hotControl = controlID;
          */

          s_lastMouseDownBrush = s_lastHoveredBrush;
          s_lastMouseDownRenderer = s_lastHoveredRenderer;
          s_selectedObjectsOnMouseDown = Selection.objects.ToList();
          break;

        case EventType.MouseMove:
          s_mouseMovePos = e.mousePosition;

          if (s_lastMouseMoveFrame == Time.renderedFrameCount)
            break;

          s_lastMouseMoveFrame = frame;
          s_lastMouseMoveTime = Time.realtimeSinceStartup;

          var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
          var contact = MudRenderer.RaycastClickSelection(ray.origin, ray.direction, 1000.0f);
          s_lastRay = ray;

          // hit normal collision before brush?
          if (contact.Hit)
          {
            float maxDistance = (contact.Position - ray.origin).magnitude;
            if (Physics.Raycast(ray, out var hitInfo, maxDistance) 
              && hitInfo.collider != null)
            {
              bool hitMudBunObject = false;
              var t = hitInfo.collider.transform;
              while (t != null)
              {
                if (t.GetComponent<MudBrushBase>() != null 
                    || t.GetComponent<MudRenderer>() != null)
                {
                  hitMudBunObject = true;
                  break;
                }

                t = t.parent;
              }

              if (!hitMudBunObject)
                contact.Hit = false;
            }
          }

          MudBrushBase hoveredBrush = null;
          if (contact.Hit)
          {
            s_lastHit = contact;
            hoveredBrush = MudRendererBase.LookupBrush(Mathf.Abs(contact.Material.EmissionHash.a));
          }

          if (MudRenderer.HoveredBrush != hoveredBrush)
          {
            if (MudRenderer.HoveredBrush != null 
                && MudRenderer.HoveredBrush.Renderer != null)
            {
              MudRenderer.HoveredBrush.Renderer.MarkNeedsCompute();
            }


            MudRenderer.HoveredBrush = hoveredBrush;
            var renderer = hoveredBrush != null ? hoveredBrush.Renderer : null;
            if (renderer != null)
            {
              s_lastHoveredBrush = hoveredBrush;
              s_lastHoveredRenderer = renderer;
              renderer.MarkNeedsCompute();
            }
            else if (s_lastHoveredRenderer != null)
            {
              s_lastHoveredRenderer.MarkNeedsCompute();
              s_lastHoveredBrush = null;
              s_lastHoveredRenderer = null;
            }
          }

          EditorApplication.QueuePlayerLoopUpdate();
          break;
      }
    }

    private static List<UnityEngine.Object> s_restoreSelection;

    private static void OnSelectionChanged()
    {
      if (Time.realtimeSinceStartup - s_lastMouseMoveTime > 0.5f)
      {
        s_restoreSelection = null;
        return;
      }

      // HACK: someone a single-pixel mouse move during click selection can nuke the selection
      if ((s_mouseDownPos - s_mouseMovePos).magnitude < 3.0f)
      {
        var selection = Selection.objects.ToList();
        foreach (var obj in s_lastSelectedObjects)
        {
          var go  = obj as GameObject;
          if (go == null)
            continue;

          if (go.GetComponent<MudBrushBase>() == null 
              && go.GetComponent<MudRenderer>() == null)
            continue;

          if (!selection.Contains(go))
          {
            if (s_restoreSelection == null)
              s_restoreSelection = new List<Object>();

            s_restoreSelection.Add(go);
          }
        }

        if (s_restoreSelection != null)
          EditorApplication.QueuePlayerLoopUpdate();
      }

      bool markedNeedsCompute = false;
      foreach (var obj in s_lastSelectedObjects)
      {
        var go = obj as GameObject;
        if (go == null)
          continue;

        if (Selection.objects.Contains(go))
          continue;

        var brush = go.GetComponent<MudBrushBase>();
        if (brush == null)
          continue;

        var renderer = brush.Renderer;
        if (renderer == null)
          continue;

        renderer.MarkNeedsCompute();
        markedNeedsCompute = true;
      }

      if (markedNeedsCompute)
        EditorApplication.QueuePlayerLoopUpdate();

      s_lastSelectedObjects = Selection.objects.ToList();
    }

    private static void Update()
    {
      if (s_restoreSelection != null)
      {
        var selection = Selection.objects.ToList();
        foreach (var go in s_restoreSelection)
        {
          if (!selection.Contains(go))
            selection.Add(go);
        }
        Selection.objects = selection.ToArray();
        s_restoreSelection = null;
      }
    }
#endif
  }
}

