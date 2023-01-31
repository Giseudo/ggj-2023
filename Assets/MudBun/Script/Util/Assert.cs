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

namespace MudBun
{
  public class Assert
  {
    public static void True(bool b, string message = "")
    {
      #if UNITY_EDITOR
      if (b)
        return;
      
      if (string.IsNullOrEmpty(message))
        throw new System.Exception("MudBun: Assert.True failed.");
      else
        throw new System.Exception("MudBun: Assert.True failed: " + message);
      #endif
    }

    public static void Equal<T>(T a, T b, string message = "")
    {
      #if UNITY_EDITOR
      if (EqualityComparer<T>.Default.Equals(a, b))
        return;

      if (string.IsNullOrEmpty(message))
        throw new System.Exception("MudBun: Assert.Equal failed.");
      else
        throw new System.Exception("MudBun: Assert.Equal failed: " + message);
      #endif
    }

    public static void Unequal<T>(T a, T b, string message = "")
    {
      #if UNITY_EDITOR
      if (!EqualityComparer<T>.Default.Equals(a, b))
        return;

      if (string.IsNullOrEmpty(message))
        throw new System.Exception("MudBun: Assert.Unequal failed.");
      else
        throw new System.Exception("MudBun: Assert.Unequal failed: " + message);
      #endif
    }

    public static void Warn(string message)
    {
      #if UNITY_EDITOR
      throw new System.Exception("MudBun: Assert Warning: " + message);
      #endif
    }
  }
}

