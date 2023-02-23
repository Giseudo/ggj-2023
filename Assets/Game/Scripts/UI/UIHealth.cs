using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Combat;
using TMPro;

public class UIHealth : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private RectMask2D _heartMask;

    private RectTransform _rect;
    private Damageable _damageable;

    public float Height => _heartMask.rectTransform.rect.height;

    public void OnHurt(Damageable damageable)
    {
        float value = Height * (1f - ((float)damageable.Health / (float)damageable.MaxHealth));

        _heartMask.padding = new Vector4(0f, 0f, 0f, value);
    }

    public void Start()
    {
        TryGetComponent<RectTransform>(out _rect);

        GameManager.MainTree.TryGetComponent<Damageable>(out _damageable);

        if (_damageable == null) return;

        _damageable.hurted += OnHurt;

        UpdateText();
    }

    public void OnDestroy()
    {
        if (_damageable == null) return;

        _damageable.hurted -= OnHurt;
    }

    public void UpdateText()
    {
        if (_damageable == null) return;

        _text.text = $"{_damageable.Health}";
    }
}
