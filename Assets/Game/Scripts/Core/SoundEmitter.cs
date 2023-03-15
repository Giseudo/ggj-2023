using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;

public class SoundEmitter : MonoBehaviour
{
    [SerializeField]
    private List<AudioClip> _clips = new List<AudioClip>();

    [SerializeField]
    private float _intervalTime = 1f;

    public void OnEnable()
    {
        StartCoroutine(PlaySounds());
    }

    public void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator PlaySounds()
    {
        int index = 0;
        AudioClip clip = _clips[index];

        while (clip != null)
        {
            SoundManager.PlaySound(clip);

            index = (index + 1) % _clips.Count;
            clip = _clips[index];

            yield return new WaitForSecondsRealtime(_intervalTime);
        }
    }
}