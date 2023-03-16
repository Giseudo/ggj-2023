using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using DG.Tweening;

public class UIMenuContainer : MonoBehaviour
{
    [SerializeField]
    private Image _overlayImage;

    [SerializeField]
    private VideoPlayer _videoPlayer;

    private CanvasGroup _canvasGroup;

    public void Start()
    {
        TryGetComponent<CanvasGroup>(out _canvasGroup);

        _canvasGroup.DOFade(1f, .3f)
            .SetDelay(11f)
            .SetUpdate(true)
            .OnComplete(() => {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
    }

    public void LoadFirstLevel()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOFade(0f, .3f)
            .OnComplete(() => {
                DOTween.To(() => _videoPlayer.GetDirectAudioVolume(0), x => _videoPlayer.SetDirectAudioVolume(0, x), 0f, 1f);

                _overlayImage.DOFade(1f, 1f)
                    .OnComplete(() => SceneManager.LoadScene(2));
            });
    }
}
