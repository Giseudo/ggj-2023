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

namespace MudBun
{
  public class MudBunConfig : ScriptableObject
  {
    private static MudBunConfig s_instance;
    public static MudBunConfig Instance
    {
      get
      {
        if (s_instance != null)
          return s_instance;

        s_instance = (MudBunConfig) Resources.Load("MudBun Config");
        return s_instance;
      }
    }

    public bool CheckCompatibility = true;

    //public bool WarnInsufficientBudgets = true;
  }
}