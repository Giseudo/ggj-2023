using UnityEngine;
using UnityEngine.VFX;
using Game.Combat;

namespace Game.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [SerializeField]
        [InspectorName("BGM")]
        private AudioSource _bgm;

        [SerializeField]
        private AudioSource _sfx;

        public static AudioSource BGM { get; private set; }
        public static AudioSource SFX { get; private set; }

        public void Awake()
        {
            Instance = this;

            BGM = _bgm;
            SFX = _sfx;
        }

        public void OnDestroy()
        { }

        public static void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            SFX.PlayOneShot(clip);
        }
    }
}