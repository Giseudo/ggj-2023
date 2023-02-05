/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.IO;

using UnityEditor;
using UnityEngine;

namespace MudBun
{
  public class CreationMenu
  {
    // options
    //-------------------------------------------------------------------------

    public static bool IsQuickCreation = false;
    public static bool SwapSolidBrushes = false;

    //-------------------------------------------------------------------------
    // end: options


    // utils
    //-------------------------------------------------------------------------

    //[MenuItem("GameObject/MudBun", priority = 151)]
    //public static void MudBunSubMenuHelper() { }

    protected static GameObject CreateGameObject(string name)
    {
      var newGo = new GameObject(name);

      var selectedGo = Selection.activeGameObject;
      if (selectedGo != null)
      {
        bool shouldParent = true;
        
        if (IsQuickCreation)
        {
          shouldParent = 
               selectedGo.GetComponent<MudRenderer>() != null 
            || selectedGo.GetComponent<MudBrushGroup>() != null;
        }

        if (shouldParent)
        {
          // make child
          newGo.transform.parent = selectedGo.transform;
        }
        else
        {
          // make sibling
          newGo.transform.parent = selectedGo.transform.parent;
        }
      }

      Undo.RegisterCreatedObjectUndo(newGo, newGo.name);

      return newGo;
    }

    protected static GameObject OnBrushCreated(GameObject newGo, bool setAsFirstChild = false)
    {
      bool parentedUnderRenderer = false;
      var t = newGo.transform.parent;
      while (t != null)
      {
        if (t.GetComponent<MudRenderer>() != null)
        {
          parentedUnderRenderer = true;
          break;
        }
        t = t.parent;
      }

      if (!parentedUnderRenderer)
      {
        var renderer = new GameObject("Mud Renderer");
        renderer.AddComponent<MudRenderer>();
        if (newGo.transform.parent != null)
        {
          renderer.transform.parent = newGo.transform.parent;
          renderer.transform.localPosition = Vector3.zero;
        }

        newGo.transform.parent = renderer.transform;

        Undo.RegisterCreatedObjectUndo(renderer, renderer.name);
      }

      if (setAsFirstChild)
        newGo.transform.SetSiblingIndex(0);

      var selectedGo = Selection.activeGameObject;

      bool shouldSwap = 
          SwapSolidBrushes 
          && newGo.GetComponent<MudSolid>() != null 
          && !(newGo.GetComponent<MudSolid>() is MudBrushGroup);

      if (shouldSwap)
      {
        newGo = HandleSwap(newGo, selectedGo);
      }
      else
      {
        newGo.transform.localPosition = Vector3.zero;
        newGo.transform.localScale = Vector3.one;
      }

      Selection.activeGameObject = newGo;

      return newGo;
    }

    // https://gist.github.com/allanolivei/9260107
    public static string GetSelectedPathOrFallback()
    {
      string path = "Assets";

      foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
      {
        path = AssetDatabase.GetAssetPath(obj);
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
          path = Path.GetDirectoryName(path);
          break;
        }
      }

      return path;
    }

    private static string [] s_aTransferPropertyName = 
    {
      "Blend", 
      "Operator", 
      "Symmetry", 
      "Round", 
    };

    private static GameObject HandleSwap(GameObject newGo, GameObject selectedGo)
    {
      if (selectedGo == null)
        return newGo;

      var newComp = newGo.GetComponent<MudSolid>();
      if (newComp != null)
      {
        var existingComp = selectedGo.GetComponent<MudSolid>();
        if (existingComp != null)
        {
          // transfer properties
          var existingType = existingComp.GetType();
          var newType = newComp.GetType();
          foreach (string propertyName in s_aTransferPropertyName)
          {
            var existingProp = existingType.GetProperty(propertyName);
            if (existingProp == null)
              continue;

            var newProp = newType.GetProperty(propertyName);
            if (newProp == null)
              continue;

            var existingValue = existingProp.GetValue(existingComp);
            newProp.SetValue(newComp, existingValue);
          }

          ExtraSolidBrushDestroy(selectedGo);
          Undo.DestroyObjectImmediate(existingComp);
        }

        UnityEditorInternal.ComponentUtility.CopyComponent(newComp);
        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(selectedGo);

        selectedGo.name = newGo.name;

        ExtraSolidBrushInit(selectedGo);
      }

      Object.DestroyImmediate(newGo);

      return selectedGo;
    }

    //-------------------------------------------------------------------------
    // end: utils


    // extra init/destroy
    //-------------------------------------------------------------------------

    private static void ExtraSolidBrushInit(MudParticleSystem comp)
    {
      var go = comp.gameObject;

      var particles = go.AddComponent<ParticleSystem>();
      comp.Particles = particles;

      var main = particles.main;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      main.startLifetime = 2.0f;
      main.startSpeed = 2.0f;
      main.startSize = 0.1f;

      var shape = particles.shape;
      shape.enabled = true;
      shape.angle = 15.0f;
      shape.radius = 0.0f;

      var size = particles.sizeOverLifetime;
      size.enabled = true;
      size.size = 
        new ParticleSystem.MinMaxCurve 
        (
          1.0f, 
          new AnimationCurve
          (
            new Keyframe[]
            {
              new Keyframe(0.0f, 0.0f), 
              new Keyframe(0.2f, 1.0f), 
              new Keyframe(0.8f, 1.0f), 
              new Keyframe(1.0f, 0.0f), 
            }
          )
        );

      var renderer = go.GetComponent<ParticleSystemRenderer>();
      renderer.enabled = false;
    }

    private static void ExtraSolidBrushInit(MudCurveSimple comp)
    {
      var go = comp.gameObject;

      var pA = CreateGameObject("Curve Point A");
      pA.transform.parent = go.transform;
      pA.transform.localPosition = new Vector3(-0.5f, 0.0f, 0.0f);

      var pC = CreateGameObject("Curve Control Point");
      pC.transform.parent = go.transform;
      pC.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);

      var pB = CreateGameObject("Curve Point B");
      pB.transform.parent = go.transform;
      pB.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);

      comp.PointA = pA.transform;
      comp.ControlPoint = pC.transform;
      comp.PointB = pB.transform;
    }

    private static void ExtraSolidBrushInit(MudCurveFull comp)
    {
      var go = comp.gameObject;

      var p0 = CreateGameObject("Curve Point (0)");
      p0.transform.parent = go.transform;
      p0.transform.localPosition = new Vector3(-0.5f, 0.0f);

      var p1 = CreateGameObject("Curve Point (1)");
      p1.transform.parent = go.transform;
      p1.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);

      var p2 = CreateGameObject("Curve Point (2)");
      p2.transform.parent = go.transform;
      p2.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);

      comp.Points = new MudCurveFull.Point[]
      {
        new MudCurveFull.Point(p0, 0.2f), 
        new MudCurveFull.Point(p1, 0.2f), 
        new MudCurveFull.Point(p2, 0.2f), 
      };
    }

    private static void ExtraSolidBrushInit(GameObject go)
    {
      var solid = go.GetComponent<MudSolid>();
      if (solid == null)
        return;

      if (solid is MudParticleSystem)
      {
        ExtraSolidBrushInit((MudParticleSystem) solid);
      }
      else if (solid is MudCurveSimple)
      {
        ExtraSolidBrushInit((MudCurveSimple) solid);
      }
      else if (solid is MudCurveFull)
      {
        ExtraSolidBrushInit((MudCurveFull) solid);
      }
    }

    private static void ExtraSolidBrushDestroy(GameObject go)
    {
      var solid = go.GetComponent<MudSolid>();
      if (solid == null)
        return;

      if (solid is MudCurveSimple)
      {
        var curveSimple = (MudCurveSimple) solid;

        if (curveSimple.PointA.parent == curveSimple.transform)
          Object.DestroyImmediate(curveSimple.PointA.gameObject);

        if (curveSimple.PointB.parent == curveSimple.transform)
          Object.DestroyImmediate(curveSimple.PointB.gameObject);

        if (curveSimple.ControlPoint == curveSimple.transform)
          Object.DestroyImmediate(curveSimple.ControlPoint.gameObject);
      }
      else if (solid is MudCurveFull)
      {
        var curveFull = (MudCurveFull) solid;

        foreach (var p in curveFull.Points)
        {
          if (p.Transform.parent == curveFull.transform)
            Object.DestroyImmediate(p.Transform.gameObject);
        }
      }

      var ps = go.GetComponent<ParticleSystem>();
      if (ps != null)
        Object.DestroyImmediate(ps);

      var pr = go.GetComponent<ParticleSystemRenderer>();
      if (pr != null)
        Object.DestroyImmediate(pr);
    }

    //-------------------------------------------------------------------------
    // end: extra init/destroy


    // creation
    //-------------------------------------------------------------------------

    [MenuItem("GameObject/MudBun/Renderer", priority = 5)]
    public static GameObject CreateRenderer()
    {
      var go = CreateGameObject("Mud Renderer");
      go.AddComponent<MudRenderer>();

      Selection.activeGameObject = go;

      return go;
    }

    [MenuItem("GameObject/MudBun/Brush Group", priority = 4)]
    public static GameObject CreateBrushGroup()
    {
      var go = CreateGameObject("Mud Brush Group");
      go.AddComponent<MudBrushGroup>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Box", priority = 3)]
    public static GameObject CreateBox()
    {
      var go = CreateGameObject("Mud Box");
      go.AddComponent<MudBox>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Sphere", priority = 3)]
    public static GameObject CreateSphere()
    {
      var go = CreateGameObject("Mud Sphere");
      go.AddComponent<MudSphere>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Cylinder", priority = 3)]
    public static GameObject CreateCylinder()
    {
      var go = CreateGameObject("Mud Cylinder");
      go.AddComponent<MudCylinder>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Cone", priority = 3)]
    public static GameObject CreateCone()
    {
      var go = CreateGameObject("Mud Cone");
      var comp = go.AddComponent<MudCone>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Torus", priority = 3)]
    public static GameObject CreateTorus()
    {
      var go = CreateGameObject("Mud Torus");
      go.AddComponent<MudTorus>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Solid Angle", priority = 3)]
    public static GameObject CreateSolidAngle()
    {
      var go = CreateGameObject("Mud Solid Angle");
      go.AddComponent<MudSolidAngle>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Curve (Simple: 2 Points + 1 Control)", priority = 3)]
    public static GameObject CreateCurveSimple()
    {
      var go = CreateGameObject("Mud Curve (Simple)");
      var comp = go.AddComponent<MudCurveSimple>();
      comp.EnableNoise = false;

      ExtraSolidBrushInit(comp);

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Primitives/Curve (Full: Any Points)", priority = 3)]
    public static GameObject CreateCurveFull()
    {
      var go = CreateGameObject("Mud Curve (Full)");
      var comp = go.AddComponent<MudCurveFull>();
      //comp.EnableNoise = false;

      ExtraSolidBrushInit(comp);

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Effects/Particle System", priority = 3)]
    public static GameObject CreateParticleSystem()
    {
      var go = CreateGameObject("Mud Particle System");
      var comp = go.AddComponent<MudParticleSystem>();

      go.transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.right);

      ExtraSolidBrushInit(comp);

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Effects/Noise Volume", priority = 3)]
    public static GameObject CreateNoiseVolume()
    {
      var go = CreateGameObject("Mud Noise");
      go.AddComponent<MudNoiseVolume>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Effects/Noise Curve (Simple: 2 Points + 1 Control)", priority = 3)]
    public static GameObject CreateNoiseCurveSimple()
    {
      var go = CreateGameObject("Mud Curve (Simple)");
      var comp = go.AddComponent<MudCurveSimple>();
      comp.EnableNoise = true;

      ExtraSolidBrushInit(comp);

      return OnBrushCreated(go);
    }

    /*
    [MenuItem("GameObject/MudBun/Effects/Noise Curve (Full: Any Points)", priority = 3)]
    public static GameObject CreateNoiseCurveFull()
    {
      var go = CreateGameObject("Mud Curve (Full)");
      var comp = go.AddComponent<MudCurveFull>();
      //comp.EnableNoise = true;

      var p0 = CreateGameObject("Curve Point (0)");
      p0.transform.parent = go.transform;
      p0.transform.localPosition = new Vector3(-0.5f, 0.0f);

      var p1 = CreateGameObject("Curve Point (1)");
      p1.transform.parent = go.transform;
      p1.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);

      var p2 = CreateGameObject("Curve Point (2)");
      p2.transform.parent = go.transform;
      p2.transform.localPosition = new Vector3(0.5f, 0.0f, 0.0f);

      comp.Points = new MudCurveFull.Point[]
      {
        new MudCurveFull.Point(p0, 0.2f), 
        new MudCurveFull.Point(p1, 0.2f), 
        new MudCurveFull.Point(p2, 0.2f), 
      };

      return OnBrushCreated(go);
    }
    */

    [MenuItem("GameObject/MudBun/Distortion/Fish Eye", priority = 3)]
    public static GameObject CreateFishEye()
    {
      var go = CreateGameObject("Mud Fish Eye");
      go.AddComponent<MudFishEye>();

      return OnBrushCreated(go, true);
    }

    [MenuItem("GameObject/MudBun/Distortion/Twist", priority = 3)]
    public static GameObject CreateTwist()
    {
      var go = CreateGameObject("Mud Twist");
      go.AddComponent<MudTwist>();

      go.transform.localScale = new Vector3(1.0f, 1.2f, 1.0f);

      return OnBrushCreated(go, true);
    }

    [MenuItem("GameObject/MudBun/Distortion/Pinch", priority = 3)]
    public static GameObject CreatePinch()
    {
      var go = CreateGameObject("Mud Pinch");
      go.AddComponent<MudPinch>();

      return OnBrushCreated(go, true);
    }

    [MenuItem("GameObject/MudBun/Distortion/Quantize", priority = 3)]
    public static GameObject CreateQuantize()
    {
      var go = CreateGameObject("Mud Quantize");
      go.AddComponent<MudQuantize>();

      return OnBrushCreated(go, true);
    }

    [MenuItem("GameObject/MudBun/Modifiers/Noise Modifier", priority = 3)]
    public static GameObject CreateNoiseModifier()
    {
      var go = CreateGameObject("Mud Noise Modifier");
      go.AddComponent<MudNoiseModifier>();

      return OnBrushCreated(go);
    }

    [MenuItem("GameObject/MudBun/Modifiers/Onion", priority = 3)]
    public static GameObject CreateOnion()
    {
      var go = CreateGameObject("Mud Onion");
      go.AddComponent<MudOnion>();

      return OnBrushCreated(go);
    }

    //-------------------------------------------------------------------------
    // end: creation


    // materials
    //-------------------------------------------------------------------------

    private static void CreateRenderMaterial(string assetName, Material mat)
    {
      string path = $"{GetSelectedPathOrFallback()}/{assetName}.mat";

      Material matCopy = new Material(mat);
      if (matCopy == null)
      {
        Debug.LogWarning("MudBun: Failed to create render material at \"{path\"");
        return;
      }

      ProjectWindowUtil.CreateAsset(matCopy, path);
    }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Mesh (Single-Textured)", priority = 200)]
    public static void CreateMeshSingleTexturedRendererMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Mesh Renderer Material (Single-Textured)", 
        ResourcesUtilEditor.DefaultMeshSingleTexturedMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Mesh (Multi-Textured)", priority = 200)]
    public static void CreateMeshMultiTexturedRendererMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Mesh Renderer Material (Multi-Textured)", 
        ResourcesUtilEditor.DefaultMeshMultiTexturedMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Splat (Single-Textured)", priority = 250)]
    public static void CreateSplatSingleTexturedRendererMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Splat Renderer Material (Single-Textured)", 
        ResourcesUtilEditor.DefaultSplatSingleTexturedMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Splat (Multi-Textured)", priority = 250)]
    public static void CreateSplatMultiTexturedRendererMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Splat Renderer Material (Multi-Textured)", 
        ResourcesUtilEditor.DefaultSplatMultiTexturedMaterial
      );
     }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Decal", priority = 300)]
    public static void CreateDecalRendererMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Decal Renderer Material", 
        ResourcesUtilEditor.DefaultDecalMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Basic Render Material/Locked Mesh", priority = 350)]
    public static void CreateStandardMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Locked Mesh Material", 
        ResourcesUtil.DefaultLockedMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Clay Mesh", priority = 350)]
    public static void CreateClayMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Clay Mesh Render Material",
        ResourcesUtilEditor.ClayMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Claymation Mesh", priority = 350)]
    public static void CreateClaymationMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Claymation Mesh Render Material",
        ResourcesUtilEditor.ClaymationMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Stopmotion Mesh", priority = 350)]
    public static void CreateStopmotionMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Stopmotion Mesh Render Material", 
        ResourcesUtilEditor.StopmotionMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/2D Mode Mesh/Alpha-Blended Transparent Mesh", priority = 351)]
    public static void CreateAlphaBlendedTransparentMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Alpha-Blended Transparent Mesh Render Material",
        ResourcesUtilEditor.AlphaBlendedTransparentMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/2D Mode Mesh/Outline Mesh", priority = 351)]
    public static void CreateOutlineMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Alpha-Blended Transparent Mesh Render Material",
        ResourcesUtilEditor.OutlineMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/2D Mode Mesh/SDF Ripple Mesh", priority = 351)]
    public static void CreateSdfRippleMeshMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun SDF Ripple Mesh Render Material",
        ResourcesUtilEditor.SdfRippleMeshMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Brush Strokes Splat", priority = 400)]
    public static void CreateStrokesSplatMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Brush Strokes Splat Render Material", 
        ResourcesUtilEditor.BrushStrokesSplatMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Floater Splat", priority = 400)]
    public static void CreateFloaterMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Floater Splat Render Material", 
        ResourcesUtilEditor.FloaterSplatMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Floof Splat", priority = 400)]
    public static void CreateFloofMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Floof Splat Render Material", 
        ResourcesUtilEditor.FloofSplatMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Leaf Splat", priority = 400)]
    public static void CreateLeafMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Leaf Splat Render Material", 
        ResourcesUtilEditor.LeafSplatMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Stopmotion Splat", priority = 400)]
    public static void CreateStopmotionSplatMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Stopmotion Splat Render Material", 
        ResourcesUtilEditor.StopmotionSplatMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Decal Paint (2x Multiplicative)", priority = 450)]
    public static void CreateDecalPaintMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Decal Paint Render Material", 
        ResourcesUtilEditor.DecalPaintMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Decal Darken (Multiplicative)", priority = 450)]
    public static void CreateDecalDarkenMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Decal Darken Render Material", 
        ResourcesUtilEditor.DecalDarkenMaterial
      );
    }

    [MenuItem("Assets/Create/MudBun/Preset Render Material/Decal Lighten (Additive)", priority = 450)]
    public static void CreateDecalLightenMaterial()
    {
      CreateRenderMaterial
      (
        "MudBun Decal Lighten Render Material", 
        ResourcesUtilEditor.DecalLightenMaterial
      );
    }

    //-------------------------------------------------------------------------
    // end: materials
  }
}

