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
  [ExecuteInEditMode]
  public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
  {
    // Check to see if we're about to be destroyed.
    private static bool s_init = false;
    private static bool s_shuttingDown = false;
    private static object s_lock = new object();
    private static T s_instance;

    public static T Instance
    {
      get
      {
        if (s_shuttingDown)
          return null;

        if (s_init)
          return s_instance;

        lock (s_lock)
        {
          if (s_instance == null)
          {
            s_instance = (T) FindObjectOfType(typeof(T));

            if (s_instance == null)
            {
              // Need to create a new GameObject to attach the singleton to.
              var singletonGo = new GameObject();
              s_instance = singletonGo.AddComponent<T>();
              singletonGo.name = typeof(T).ToString() + " (Singleton)";
              singletonGo.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideAndDontSave;
            }
          }

          return s_instance;
        }
      }
    }

    public static T Init()
    {
      var instance = Instance;
      s_init = true;
      return instance;
    }

    virtual protected void OnEnable()
    {
      s_shuttingDown = false;

      if (Application.isPlaying)
        DontDestroyOnLoad(gameObject);
    }

    virtual protected void OnDisable()
    {
      s_shuttingDown = true;
    }

    virtual protected void OnDestroy()
    {
      s_shuttingDown = true;
    }

    virtual protected void OnApplicationQuit()
    {
      s_shuttingDown = true;
    }
  }
}

