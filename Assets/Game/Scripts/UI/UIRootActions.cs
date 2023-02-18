using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Game.Combat;
using Game.Core;

namespace Game.UI
{
    public class UIRootActions : MonoBehaviour
    {
        [SerializeField]
        private UIUnitSelection _unitSelection;

        [SerializeField]
        private UIRangeRadius _unitRangeRadius;

        [SerializeField]
        public UIRootActionButton _addButton;

        [SerializeField]
        public UIRootActionButton _upgradeButton;

        [SerializeField]
        public UIRootActionButton _killButton;

        [SerializeField]
        public UIRootActionButton _splitButton;

        [SerializeField]
        public UIRootActionButton _targetButton;

        [SerializeField]
        private AudioClip _unitCreationSound;

        private bool _isOpened;
        private RectTransform _rect;
        private RootNode _activeNode;
        private Vector3 _initialCameraPosition;
        private Tween _tween;
        private Tween _cameraTween;
        private Tween _fovTween;

        public UIUnitSelection UnitSelection => _unitSelection;
        public RectTransform Rect => _rect;
        public bool IsOpened => _isOpened;

        public Action opened = delegate { };
        public Action closed = delegate { };

        public UIRootActionButton AddButton => _addButton;
        public UIRootActionButton KillButton => _killButton;
        public UIRootActionButton SplitButton => _splitButton;
        public UIRootActionButton TargetButton => _targetButton;
        public UIRootActionButton UpgradeButton => _upgradeButton;

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);

            if (_rect == null) return;

            _rect.localScale = Vector3.zero;
        }

        public void Start()
        {
            _initialCameraPosition = GameManager.MainCamera.transform.position;
        }

        public void OnEnable()
        {
            _addButton.clicked += OnAddUnit;
            _killButton.clicked += OnKillUnit;

            _unitSelection.clicked += OnCreateUnit;
            _unitSelection.selected += OnSelectUnit;
            _unitSelection.closed += OnUnitSelectionClose;
        }

        public void OnDisable()
        {
            _addButton.clicked -= OnAddUnit;
            _killButton.clicked -= OnKillUnit;

            _unitSelection.clicked -= OnCreateUnit;
            _unitSelection.selected -= OnSelectUnit;
            _unitSelection.closed -= OnUnitSelectionClose;
        }

        public void Show(RootNode node)
        {
            _activeNode = node;

            _splitButton.EnergyButton.SetText($"{GameManager.MainTree.RootEnergyCost}");

            if (node.Unit == null)
            {
                _addButton.gameObject.SetActive(true);
                _killButton.gameObject.SetActive(false);
                _upgradeButton.gameObject.SetActive(false);
                _targetButton.gameObject.SetActive(false);
            }

            if (node.Unit != null)
            {
                _addButton.gameObject.SetActive(false);
                _killButton.gameObject.SetActive(true);
                _upgradeButton.gameObject.SetActive(true);
                _targetButton.gameObject.SetActive(false);
                // _targetButton.gameObject.SetActive(true); // TODO: only for sementinha :3

                _killButton.EnergyButton.SetText($"{node.Unit.Data.SellPrice}");
                _upgradeButton.EnergyButton.SetText($"{node.Unit.Data.UpgradeCost}");
            }

            if (node.Parent == null)
            {
                _addButton.gameObject.SetActive(false);
                _killButton.gameObject.SetActive(false);
                _upgradeButton.gameObject.SetActive(true);
                _targetButton.gameObject.SetActive(false);

                _upgradeButton.EnergyButton.SetText($"{GameManager.MainTree.UpgradeCost}");
            }

            if (_rect == null) return;

            _tween?.Kill();
            _tween = _rect.DOScale(Vector3.one, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);

            _isOpened = true;
            opened.Invoke();
        }

        public void Hide()
        {
            if (_rect == null) return;

            _tween?.Kill();
            _tween = _rect.DOScale(Vector3.zero, .2f)
                .SetUpdate(true)
                .OnComplete(() => {
                    _isOpened = false;
                    closed.Invoke();
                });
        }

        private void OnAddUnit()
        {
            Hide();

            _unitRangeRadius.transform.position = _activeNode.transform.position + Vector3.up * .2f;
            _unitSelection.Rect.anchoredPosition = Rect.anchoredPosition + Vector2.up * 55f;
            _unitSelection.Show();

            float direction = _activeNode.transform.position.x < GameManager.MainCamera.transform.position.x ? -1 : 1;

            _cameraTween?.Kill();
            _cameraTween = GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x + (15f * direction), 3f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            _fovTween?.Kill();
            _fovTween = GameManager.MainCamera.DOOrthoSize(30f, 3f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            TimeManager.SlowMotion();
        }

        public void OnUnitSelectionClose()
        {
            _cameraTween?.Kill();
            _cameraTween = GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            _fovTween?.Kill();
            _fovTween = GameManager.MainCamera.DOOrthoSize(25f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
            
            OnDeselectUnit();

            closed.Invoke();

            TimeManager.Resume();
        }

        private void OnSelectUnit(UnitData data)
        {
            if (!_unitSelection.IsOpened) return;

            _unitRangeRadius.SetRadius(data == null ? 0f : data.RangeRadius);

            if (data == null)
                OnDeselectUnit();
        }

        private void OnDeselectUnit()
        {
            _unitRangeRadius.SetRadius(0f);
        }

        private void OnCreateUnit(UnitData data)
        {
            if (data.RequiredEnergy > GameManager.MainTree.EnergyAmount)
                return;

            _unitSelection.Hide();

            GameObject instance = GameObject.Instantiate(data.Prefab, _activeNode.transform);

            instance.transform.localScale = Vector3.zero;
            instance.transform.DOScale(Vector3.one, 1f)
                .SetUpdate(true)
                .SetEase(Ease.OutElastic);

            SoundManager.PlaySound(_unitCreationSound);

            if (!instance.TryGetComponent<Unit>(out Unit unit)) return;

            _activeNode.SetUnit(unit);

            GameManager.MainTree.ConsumeEnergy(data.RequiredEnergy);
        }

        private void OnKillUnit()
        {
            if (_activeNode.Unit == null) return;

            GameManager.MainTree.CollectEnergy(_activeNode.Unit.Data.SellPrice);
            GameObject.Destroy(_activeNode.Unit.gameObject);

            _activeNode.SetUnit(null);

            Hide();
        }
    }
}