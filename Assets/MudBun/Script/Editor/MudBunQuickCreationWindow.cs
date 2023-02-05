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

using UnityEditor;
using UnityEngine;

namespace MudBun
{
  public class MudBunQuickCreationWindow : MudBunEditorWindowBase<MudBunQuickCreationWindow>
  {
    private static readonly int InitWidth = 176; //196;
    private static readonly int InitHeight = 435; //465;
    private static readonly int ButtonSize = 40; //45;

    public delegate GameObject CreationFunction();

    private bool m_swapSolidBrushes;

    private static GUIStyle m_buttonStyle;
    private static GUIStyle ButtonStyle
    {
      get
      {
        if (m_buttonStyle != null)
          return m_buttonStyle;

        m_buttonStyle = new GUIStyle("button");
        m_buttonStyle.padding = new RectOffset(0, 0, 0, 0);

        return m_buttonStyle;
      }
    }

    private class ButtonInfo
    {
      public string IconGuid;
      public Texture2D Icon;
      public string Tooltip;
      public CreationFunction CreationFunc;

      public ButtonInfo(string iconGuid, string tooltip, CreationFunction creationFunc)
      {
        IconGuid = iconGuid;
        Icon = null;
        Tooltip = tooltip;
        CreationFunc = creationFunc;
      }

      public void Draw()
      {
        if (Icon == null)
          Icon = GetTexture(IconGuid);

        bool clicked = GUILayout.Button(new GUIContent("", Icon, Tooltip), ButtonStyle, GUILayout.Width(ButtonSize), GUILayout.Height(ButtonSize));
        if (!clicked)
          return;

        if (CreationFunc == null)
          return;

        CreationFunc();
      }
    }

    private static ButtonInfo[] PrimitiveButtons = 
    {
      new ButtonInfo("00670ac6c2f92b4439e88bf26075763f", "Box",                                  CreationMenu.CreateBox), 
      new ButtonInfo("f9793d4d853ac1745b806b665335a429", "Sphere",                               CreationMenu.CreateSphere), 
      new ButtonInfo("753c5039001e6324f850e9ca1d879620", "Cylinder",                             CreationMenu.CreateCylinder), 
      new ButtonInfo("a7403ca4a6a2b974cbbc288426bcdca7", "Torus",                                CreationMenu.CreateTorus), 
      new ButtonInfo("9d124cde30b471d478492c68458202e9", "Cone",                                 CreationMenu.CreateCone), 
      new ButtonInfo("bdab03020dfda18409e25fe617be66cb", "Curve (Simple: 2 Points + 1 Control)", CreationMenu.CreateCurveSimple), 
      new ButtonInfo("65e5edd7a5e6a9b4e80c70605c0c253b", "Curve (Full: Any Points)",             CreationMenu.CreateCurveFull), 
    };

    private static ButtonInfo[] EffectsButtons = 
    {
      new ButtonInfo("5fe7312cf51c0aa40ba2eefee59bedcb", "Particle System",                            CreationMenu.CreateParticleSystem), 
      new ButtonInfo("6451025c9f95bb14f81072a28d94d786", "Noise Volume",                               CreationMenu.CreateNoiseVolume), 
      new ButtonInfo("bdab03020dfda18409e25fe617be66cb", "Noise Curve (Simple: 2 Points + 1 Control)", CreationMenu.CreateNoiseCurveSimple), 
    };

    private static ButtonInfo[] DistortionButtons = 
    {
      new ButtonInfo("b9a2814e76821984095c279a8308f575", "Fish Eye", CreationMenu.CreateFishEye), 
      new ButtonInfo("580df259607fbe44a82ab702c534d731", "Pinch",    CreationMenu.CreatePinch), 
      new ButtonInfo("d5244e4515a0afe49a01a7785dd61763", "Twist",    CreationMenu.CreateTwist), 
      new ButtonInfo("6e0af69153a23e94a92ddd0e39c40bc4", "Quantize", CreationMenu.CreateQuantize), 
    };

    private static ButtonInfo[] ModifierButtons = 
    {
      new ButtonInfo("63de3a25d802034419268887044e25d8", "Onion", CreationMenu.CreateOnion), 
    };

    private static ButtonInfo[] ContainerButtons = 
    {
      new ButtonInfo("e2ee3d73d026da44ba43d636f2e35fb9", "Brush Group", CreationMenu.CreateBrushGroup), 
      new ButtonInfo("e969af6b44048034ba1ed25990d13d7c", "Renderer",    CreationMenu.CreateRenderer), 
    };

    public static void Open()
    {
      if (Instance == null)
      {
        Instance = GetWindow<MudBunQuickCreationWindow>();
        Instance.titleContent = new GUIContent("MudBun Quick Creation");
        Instance.minSize = new Vector2(46.0f, 200.0f);
        Instance.position = new Rect(300.0f, 300.0f, InitWidth, InitHeight);
      }
      else
      {
        Instance.Focus();
      }
    }

    private static MudBunQuickCreationWindow Instance;

    private void DrawButtonGroup(ICollection<ButtonInfo> aButton)
    {
      float windowWidth = position.width;

      GUILayout.BeginVertical();

      float hPos = 0.0f;
      foreach (var b in aButton)
      {
        if (hPos <= 0.0f)
        {
          GUILayout.BeginHorizontal();
        }

        b.Draw();
        hPos += ButtonSize + 5.0f;

        if (hPos >= windowWidth - ButtonSize)
        {
          GUILayout.EndHorizontal();
          hPos = 0.0f;
        }
      }
      if (hPos > 0.0f)
        GUILayout.EndHorizontal();

      GUILayout.EndVertical();
    }

    private static void Header(string label)
    {
      EditorGUILayout.LabelField
      (
        new GUIContent() { text = label },
        new GUIStyle("label") { fontStyle = FontStyle.Bold }
      );
    }

    private static void Space()
    {
      EditorGUILayout.Space();
    }

    private void DrawOptions()
    {
      m_swapSolidBrushes = EditorGUILayout.ToggleLeft("Swap (Primitives / Effects)", m_swapSolidBrushes);
    }

    private void Paint()
    {
      CreationMenu.IsQuickCreation = true;
      CreationMenu.SwapSolidBrushes = m_swapSolidBrushes;

      Header("Primitives");
      DrawButtonGroup(PrimitiveButtons);
      Space();

      Header("Effects");
      DrawButtonGroup(EffectsButtons);
      Space();

      Header("Distortion");
      DrawButtonGroup(DistortionButtons);
      Space();

      Header("Modifiers");
      DrawButtonGroup(ModifierButtons);
      Space();

      Header("Containers");
      DrawButtonGroup(ContainerButtons);
      Space();

      Header("Options");
      DrawOptions();

      CreationMenu.IsQuickCreation = false;
      CreationMenu.SwapSolidBrushes = false;
    }
    
    private void OnGUI()
    {
      Paint();
    }
  }
}
