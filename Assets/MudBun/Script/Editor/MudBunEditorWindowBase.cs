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
  public class MudBunEditorWindowBase<T> : EditorWindow
    where T : EditorWindow
  {
    private static Dictionary<string, Texture2D> s_textures = new Dictionary<string, Texture2D>();
    internal static Texture2D GetTexture(string guid)
    {
      Texture2D texture;

      if (!s_textures.TryGetValue(guid, out texture) || texture == null)
      {
        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
        s_textures[guid] = texture;
      }

      return texture;
    }
  }
}
