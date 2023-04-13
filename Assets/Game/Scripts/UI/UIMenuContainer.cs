using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Game.Core;
using DG.Tweening;

public class UIMenuContainer : MonoBehaviour
{
    [SerializeField]
    private Image _overlayImage;

    [SerializeField]
    private VideoPlayer _videoPlayer;

    private CanvasGroup _canvasGroup;

    public void Awake()
    {
        DataHandler.LoadGameData();
    }

    public void Start()
    {
        TryGetComponent<CanvasGroup>(out _canvasGroup);

        Show(11f);
    }

    public void LoadFirstLevel()
    {
        _canvasGroup.DOFade(0f, .3f)
            .OnComplete(() => {
                DOTween.To(() => _videoPlayer.GetDirectAudioVolume(0), x => _videoPlayer.SetDirectAudioVolume(0, x), 0f, 1f);

                _overlayImage.DOFade(1f, 1f)
                    .OnComplete(() => SceneManager.LoadScene(2));
            });
    }

    public void Hide()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.DOFade(0f, .3f);
    }

    public void Show(float delay = 0f)
    {
        _canvasGroup.DOFade(1f, .3f)
            .SetDelay(delay)
            .SetUpdate(true)
            .OnComplete(() => {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
    }

    public void Quit()
    {
        Application.Quit();
    }
}
