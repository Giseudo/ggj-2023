using UnityEngine;
using UnityEngine.VFX;
using Game.Combat;

namespace Game.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
        }

        public void OnDestroy()
        { }
    }
}