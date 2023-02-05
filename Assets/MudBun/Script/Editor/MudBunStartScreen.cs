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
  public class MudBunStartScreen : MudBunEditorWindowBase<MudBunStartScreen>
  {
    private static readonly int Width = 400;
    private static readonly int Height = 650;

    private static readonly int HeaderHeight = 180;
    private static readonly string HeaderGuid = "da88b67353ce35b49827d6cccb8ab952";

    public struct Links
    {
      public static readonly string Overview = "http://longbunnylabs.com/mudbun/";
      public static readonly string QuickGuideVideo = "https://www.youtube.com/watch?v=s5Qrap0EW3M";
      public static readonly string Manual = "http://longbunnylabs.com/mudbun-manual/";
      public static readonly string ScriptingApi = "http://longbunnylabs.com/mudbun-documentation/api/MudBun.html";
      public static readonly string Discord = "https://discord.gg/MEGuEFU";
      public static readonly string Email = "mailto://LongBunnyLabs@gmail.com";
      public static readonly string Website = "http://longbunnylabs.com/";
      public static readonly string Blog = "http://allenchou.net/";
      public static readonly string Twitter = "https://twitter.com/TheAllenChou";
      public static readonly string Review = "https://assetstore.unity.com/packages/tools/particles-effects/mudbun-volumetric-vfx-modeling-177891#reviews";
    }

    private static GUIStyle VersionStyle => 
      new GUIStyle("Label")
      {
        alignment = TextAnchor.UpperLeft, 
        normal = new GUIStyleState() { textColor = Color.black }
      };

    private static GUIStyle DefaultStyle => 
      new GUIStyle("Label")
      {
        alignment = TextAnchor.UpperCenter, 
        fontSize = 12, 
        normal = new GUIStyleState() { textColor = Color.white }
      };

    private static GUIStyle HeaderStyle => 
      new GUIStyle("Label")
      {
        alignment = TextAnchor.UpperCenter, 
        fontSize = 20, 
        fixedHeight = 25, 
        richText = true, 
        normal = new GUIStyleState() { textColor = Color.white }
      };

    private static MudBunStartScreen Instance;

    public static void Open()
    {
      if (Instance == null)
      {
        Instance = GetWindow<MudBunStartScreen>();
        Instance.titleContent = new GUIContent("MudBun Start Screen");
        Instance.minSize = new Vector2(Width, Height);
        Instance.maxSize = new Vector2(Width, Height);
        Instance.position = new Rect(200.0f, 200.0f, Width, Height);
      }
      else
      {
        Instance.Focus();
      }
    }

    private static string RenderPipelineName
    {
      get
      {
        switch (ResourcesUtil.RenderPipeline)
        {
          case ResourcesUtil.RenderPipelineEnum.BuiltIn: return "Built-In RP";
          case ResourcesUtil.RenderPipelineEnum.URP:     return "URP";
          case ResourcesUtil.RenderPipelineEnum.HDRP:    return "HDRP";
          default:                                       return "Unknown";
        }
      }
    }

    private static void Header(string label, int space = 8)
    {
      EditorGUILayout.LabelField($"<b>{label}</b>", HeaderStyle);
      EditorGUILayout.Space(space);
    }

    private static void Label(string label)
    {
      EditorGUILayout.LabelField(label, DefaultStyle);
    }

    private static void Space(int size = 15)
    {
      EditorGUILayout.Space(size);
    }

    private void Paint()
    {
      EditorGUILayout.BeginVertical();

        // background
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, Width, Height + 50), EditorGUIUtility.whiteTexture);
        GUI.color = Color.white;

        // header
        GUI.DrawTexture(new Rect(0, 0, Width, HeaderHeight), GetTexture(HeaderGuid));

        // version
        Space(115);
        EditorGUILayout.BeginHorizontal();
          Space(80);
          EditorGUILayout.LabelField($"Version: {MudBun.Version}", VersionStyle);
          GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        // intro
        EditorGUILayout.Space(5);
        Header("Welcome to MudBun!");
        Label($"Render pipeline detected: {RenderPipelineName}");
        EditorGUILayout.BeginHorizontal();
          GUILayout.Space(75);
          GUILayout.Label("Show Start Screen:");
          MudBunStartScreenLauncher.LaunchMode = (MudBunStartScreenLauncher.LaunchModeEnum) EditorGUILayout.EnumPopup(MudBunStartScreenLauncher.LaunchMode);
          GUILayout.Space(75);
        EditorGUILayout.EndHorizontal();
        Space(2);
        EditorGUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();
          if (GUILayout.Button("Refresh Compatibility"))
            CompatibilityManager.KickCompatibilityScan(CompatibilityManager.PackageImportTarget.Required);
          GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        Space();

        // getting started
        Header("Getting Started");
        EditorGUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();
          if (GUILayout.Button("Overview"))
            Application.OpenURL(Links.Overview);
          if (GUILayout.Button("Quick Guide Video"))
            Application.OpenURL(Links.QuickGuideVideo);
          if (GUILayout.Button("Import Examples"))
            CompatibilityManager.KickCompatibilityScan(CompatibilityManager.PackageImportTarget.Examples);
          GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        Label("NOTE: Each render pipeline has different examples.");
        Space();

        // getting help
        Header("Getting Help");
        EditorGUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();
          if (GUILayout.Button("Manual"))
            Application.OpenURL(Links.Manual);
          if (GUILayout.Button("Scripting API"))
            Application.OpenURL(Links.ScriptingApi);
          if (GUILayout.Button("Discord"))
            Application.OpenURL(Links.Discord);
          if (GUILayout.Button("Email"))
            Application.OpenURL(Links.Email);
          GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        Space();

        // credits
        Header("Long Bunny Labs", 5);
        Label("Ming-Lun \"Allen\" Chou");
        Space(1);
        EditorGUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();
          if (GUILayout.Button("Website"))
            Application.OpenURL(Links.Website);
          if (GUILayout.Button("Blog"))
            Application.OpenURL(Links.Blog);
          if (GUILayout.Button("Twitter"))
            Application.OpenURL(Links.Twitter);
          GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        Space();

        // review
        Header("Thanks for Getting MudBun!");
        Label("If you like MudBun, please consider leaving a few kind words.");
        Space(1);
        EditorGUILayout.BeginHorizontal();
          GUILayout.FlexibleSpace();
          if (GUILayout.Button("Sure Thing"))
              Application.OpenURL(Links.Review);
          GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        Space();

      EditorGUILayout.EndVertical();

      ProjectPrefs.SetInt(MudBunStartScreenLauncher.LastRevisionKey, MudBun.Revision);
    }
    
    private void OnGUI()
    {
      Paint();
    }
  }

#if !MUDBUN_DEV
  [InitializeOnLoad]
#endif
  public class MudBunStartScreenLauncher
  {
    public enum LaunchModeEnum
    {
      // Never = 0, 
      OnNewerVersion = 1, 
      AtStartup = 2, 
    }

    public static readonly string LaunchModeKey = "StartScreenLaunchMode";
    public static readonly string LastRevisionKey = "StartScreenLastRevision";

    public static LaunchModeEnum LaunchMode
    {
      get => (LaunchModeEnum) ProjectPrefs.GetInt(LaunchModeKey, (int) LaunchModeEnum.OnNewerVersion);
      set { ProjectPrefs.SetInt(LaunchModeKey, (int) value); }
    }

    static MudBunStartScreenLauncher()
    {
      EditorApplication.update += Update;
    }

    static void Update()
    {
      EditorApplication.update -= Update;

      bool shouldLaunch = false;
      switch (LaunchMode)
      {
        /*
        case LaunchModeEnum.Never:
          shouldLaunch = false;
          break;
        */

        case LaunchModeEnum.OnNewerVersion:
          int lastRevision = ProjectPrefs.GetInt(LastRevisionKey, -1);
          shouldLaunch = (MudBun.Revision > lastRevision);
          break; 

        case LaunchModeEnum.AtStartup:
          shouldLaunch = (Time.realtimeSinceStartup < 10.0f);
          break;
      }

      ProjectPrefs.SetInt(LastRevisionKey, MudBun.Revision);

      if (shouldLaunch)
        MudBunStartScreen.Open();
    }
  }
}
