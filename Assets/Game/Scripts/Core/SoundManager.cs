using UnityEngine;
using UnityEngine.VFX;
using Game.Combat;
using DG.Tweening;

namespace Game.Core
{
    public class SoundManager : MonoBehaviour
    {
        [SerializeField]
        [InspectorName("BGM")]
        private AudioSource _bgm;

        [SerializeField]
        private AudioSource _sfx;

        public static SoundManager Instance { get; private set; }

        public static AudioSource BGM { get; private set; }
        public static AudioSource SFX { get; private set; }

        public void Awake()
        {
            Instance = this;

            BGM = _bgm;
            SFX = _sfx;
        }

        public void Update()
        {
            SFX.pitch = .5f + TimeManager.CurrentScale * .5f;
        }

        public static void StopMusic()
        {
            BGM.Stop();
        }

        public static void PlayMusic(AudioClip clip)
        {
            if (BGM.isPlaying && BGM.clip == clip) return;

            BGM.clip = clip;
            BGM.Play();
        }

        public static void PlaySound(AudioClip clip, float volume = .2f)
        {
            if (clip == null) return;

            System.Random random = new System.Random();
            float t = (float)random.NextDouble();

            SFX.PlayOneShot(clip, volume + (t * .2f));
        }
    }
}