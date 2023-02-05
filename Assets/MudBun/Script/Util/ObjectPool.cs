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

using UnityEngine;

namespace MudBun
{
  public static class ObjectPool<T> where T : new()
  {
    private static List<T> s_pool = new List<T>(16);
    private static int s_iLast = -1;

    public static T Get()
    {
      return 
        s_iLast >= 0 
          ? s_pool[s_iLast--] 
          : new T();
    }

    public static void Put(T obj)
    {
      ++s_iLast;

      if (s_iLast == s_pool.Capacity)
      {
        var oldPool = s_pool;
        int newCapacity = Mathf.Min(128, oldPool.Capacity * 2);
        s_pool = new List<T>(oldPool.Capacity * 2);
        for (int i = 0; i < oldPool.Capacity; ++i)
          s_pool[i] = oldPool[i];
      }

      s_pool[s_iLast] = obj;
    }
  }
}
