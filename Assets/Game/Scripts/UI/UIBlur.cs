using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIBlur : MonoBehaviour
{
    private RawImage _image;
    private Material _material;

    public void Show(float delay = 0f)
    {
        TryGetComponent<RawImage>(out _image);

        _material = _image.material;
        _material.SetFloat("_Blend", 0f);

        DOTween.To(() => _material.GetFloat("_Blend"), x => _material.SetFloat("_Blend", x), 1f, .5f)
            .OnStart(() => gameObject.SetActive(true))
            .SetDelay(delay)
            .SetUpdate(true);
    }

    public void Hide(float delay = 0f)
    {
        DOTween.To(() => _material.GetFloat("_Blend"), x => _material.SetFloat("_Blend", x), 0f, .5f)
            .SetDelay(delay)
            .OnStart(() => gameObject.SetActive(true))
            .SetUpdate(true);
    }
}
