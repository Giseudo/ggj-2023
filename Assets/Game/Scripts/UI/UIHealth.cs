using UnityEngine;
using Game.Core;
using Game.Combat;
using TMPro;

public class UIHealth : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private Damageable _damageable;

    public void OnHurt(Damageable damageable) => UpdateText();

    public void Start()
    {
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
