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
  public class MudBunConfigMenu
  {
    [MenuItem("Tools/MudBun/MudBun Start Screen", priority = 1)]
    public static void OpenStartScreen()
    {
      MudBunStartScreen.Open();
    }

    [MenuItem("Tools/MudBun/Quick Creation Panel", priority = 51)]
    public static void OpenQuickCreationWindow()
    {
      MudBunQuickCreationWindow.Open();
    }

    [MenuItem("Tools/MudBun/Refresh Compatibility", priority = 101)]
    public static void RefreshCompatibility()
    {
      CompatibilityManager.KickCompatibilityScan(CompatibilityManager.PackageImportTarget.Required);
    }

    [MenuItem("Tools/MudBun/Manual", priority = 152)]
    public static void OpenManual()
    {
      Application.OpenURL(MudBunStartScreen.Links.Manual);
    }

    [MenuItem("Tools/MudBun/Scripting API", priority = 153)]
    public static void OpenScriptingApi()
    {
      Application.OpenURL(MudBunStartScreen.Links.ScriptingApi);
    }

    //[MenuItem("Tools/MudBun/Import Examples", priority = 102)]
    public static void ImportExamples()
    {
      CompatibilityManager.KickCompatibilityScan(CompatibilityManager.PackageImportTarget.Examples);
    }

    //[MenuItem("Tools/MudBun/Configure MudBun"), priority = 103]
    public static void SelectConfigFile()
    {
      var config = Resources.Load("MudBun Config");
      if (config == null)
        return;

      Selection.activeObject = config;
    }
  }
}