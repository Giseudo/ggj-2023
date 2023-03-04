using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Game.Combat;
using Game.Core;
using Game.Input;

namespace Game.UI
{
    public class UIRootActions : MonoBehaviour
    {
        [SerializeField]
        private UIUnitSelection _unitSelection;

        [SerializeField]
        private UITargetSelection _targetSelection;

        [SerializeField]
        private UIRangeRadius _unitRangeRadius;

        [SerializeField]
        private UIRootActionButton _addButton;

        [SerializeField]
        private UIRootActionButton _upgradeButton;

        [SerializeField]
        private UIRootActionButton _killButton;

        [SerializeField]
        private UIRootActionButton _splitButton;

        [SerializeField]
        private UIRootActionButton _targetButton;

        [SerializeField]
        private RawImage _glowImage;

        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private AudioClip _unitCreationSound;

        private bool _isOpened;
        private RectTransform _rect;
        private RootNode _activeNode;
        private Vector3 _initialCameraPosition;
        private UIButton _highlightButton;
        private Tween _tween;
        private Tween _glowTween;
        private Tween _cameraTween;
        private Tween _fovTween;

        public UIUnitSelection UnitSelection => _unitSelection;
        public UITargetSelection TargetSelection => _targetSelection;
        public RectTransform Rect => _rect;
        public bool IsOpened => _isOpened;

        public Action opened = delegate { };
        public Action closed = delegate { };
        public Action canceled = delegate { };
        public Action<Unit> createdUnit = delegate { };

        public UIRootActionButton AddButton => _addButton;
        public UIRootActionButton KillButton => _killButton;
        public UIRootActionButton SplitButton => _splitButton;
        public UIRootActionButton TargetButton => _targetButton;
        public UIRootActionButton UpgradeButton => _upgradeButton;
        public UIButton HighlightingButton => _highlightButton;

        public void KillCameraTween() => _cameraTween?.Kill();

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);

            if (_rect == null) return;

            _rect.localScale = Vector3.zero;
        }

        public void Start()
        {
            _initialCameraPosition = GameManager.MainCamera.transform.position;

            MatchManager.LevelCompleted += OnCancel;
            MatchManager.GameOver += OnCancel;
            GameManager.MainTree.collectedEnergy += OnEnergyChange;
            GameManager.MainTree.consumedEnergy += OnEnergyChange;

            _inputReader.canceled += OnCancel;
        }

        public void OnDestroy()
        {
            MatchManager.LevelCompleted -= OnCancel;
            MatchManager.GameOver -= OnCancel;
            GameManager.MainTree.collectedEnergy -= OnEnergyChange;
            GameManager.MainTree.consumedEnergy -= OnEnergyChange;

            _inputReader.canceled -= OnCancel;
        }

        private void OnEnergyChange(int amount)
        {
            if (_activeNode == null) return;

            bool upgradeEnabled = false;
            bool rootEnabled = GameManager.MainTree.EnergyAmount > GameManager.MainTree.RootEnergyCost;

            if (_activeNode.Unit != null)
                upgradeEnabled =  GameManager.MainTree.EnergyAmount > _activeNode.Unit.Data.UpgradeCost;

            if (_activeNode.Parent == null)
                upgradeEnabled =  GameManager.MainTree.EnergyAmount > GameManager.MainTree.UpgradeCost;

            if (upgradeEnabled)
                _upgradeButton.EnergyButton.Enable();
            else
                _upgradeButton.EnergyButton.Disable();

            if (rootEnabled)
                _splitButton.EnergyButton.Enable();
            else
                _splitButton.EnergyButton.Disable();
        }

        public void OnEnable()
        {
            _addButton.clicked += OnAddUnit;
            _killButton.clicked += OnKillUnit;
            _upgradeButton.clicked += OnUpgradeUnit;
            _targetButton.clicked += OnChangeTarget;

            _unitSelection.clicked += OnCreateUnit;
            _unitSelection.selected += OnSelectUnit;
            _unitSelection.closed += OnUnitSelectionClose;

            _targetSelection.confirmed += OnConfirmTarget;
            _targetSelection.closed += OnTargetSelectionClose;
        }

        public void OnDisable()
        {
            _addButton.clicked -= OnAddUnit;
            _killButton.clicked -= OnKillUnit;
            _upgradeButton.clicked -= OnUpgradeUnit;
            _targetButton.clicked -= OnChangeTarget;

            _unitSelection.clicked -= OnCreateUnit;
            _unitSelection.selected -= OnSelectUnit;
            _unitSelection.closed -= OnUnitSelectionClose;

            _targetSelection.confirmed -= OnConfirmTarget;
            _targetSelection.closed -= OnTargetSelectionClose;
        }

        private void OnCancel()
        {
            Hide();

            if (_unitSelection.IsOpened)
                _unitSelection.Hide();

            canceled.Invoke();
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
                _upgradeButton.gameObject.SetActive(node.Unit.Data.UpgradePrefab);
                _targetButton.gameObject.SetActive(node.Unit.Data.Type == UnitType.Spawner);

                _killButton.EnergyButton.SetText($"{node.Unit.Data.SellPrice}");
                _upgradeButton.EnergyButton.SetText($"{node.Unit.Data.UpgradeCost}");
            }

            if (node.Parent == null)
            {
                _addButton.gameObject.SetActive(false);
                _killButton.gameObject.SetActive(false);
                _upgradeButton.gameObject.SetActive(!GameManager.MainTree.ReachedMaxLevel);
                _targetButton.gameObject.SetActive(false);

                _upgradeButton.EnergyButton.SetText($"{GameManager.MainTree.UpgradeCost}");
            }

            OnEnergyChange(GameManager.MainTree.EnergyAmount);

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

                    if (_unitSelection.IsOpened) return;

                    closed.Invoke();
                });
        }

        public void Highlight(UIButton button)
        {
            _highlightButton?.Pulse(false);
            _highlightButton = button;
            _highlightButton?.Pulse(true);

            _glowTween?.Kill();

            if (!_highlightButton)
            {
                _glowTween = _glowImage.DOFade(0f, .5f).SetUpdate(true);
                return;
            }

            Color32 color = _glowImage.color;
            color.a = 100;
            _glowImage.color = color;

            _glowImage.rectTransform.anchoredPosition = _highlightButton.Rect.anchoredPosition;
            _glowTween = _glowImage.DOFade(1f, .5f)
                .SetDelay(.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        private void OnAddUnit()
        {
            Hide();

            _unitRangeRadius.transform.position = _activeNode.transform.position + Vector3.up * .2f;
            _unitSelection.Rect.anchoredPosition = Rect.anchoredPosition + Vector2.up * 55f;
            _unitSelection.Show();

            float direction = _activeNode.transform.position.x < GameManager.MainCamera.transform.position.x ? -1 : 1;

            _cameraTween?.Kill();
            _cameraTween = GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x + (20f * direction), 3f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            _fovTween?.Kill();
            _fovTween = GameManager.MainCamera.DOOrthoSize(27f, 3f)
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

            Unit unit = CreateUnit(data.Prefab);

            if (unit == null) return;

            _activeNode.SetUnit(unit);
            createdUnit(unit);

            GameManager.MainTree.ConsumeEnergy(data.RequiredEnergy);

            if (unit.Data.Type != UnitType.Spawner) return;

            _targetSelection.Show(unit);

            TimeManager.SlowMotion();
        }

        private Unit CreateUnit(GameObject prefab)
        {
            GameObject instance = GameObject.Instantiate(prefab, _activeNode.transform);

            instance.transform.localScale = Vector3.zero;
            instance.transform.DOScale(Vector3.one, 1f)
                .SetUpdate(true)
                .SetEase(Ease.OutElastic);

            SoundManager.PlaySound(_unitCreationSound);

            instance.TryGetComponent<Unit>(out Unit unit);

            return unit;
        }

        private void OnKillUnit()
        {
            Hide();

            if (_activeNode.Unit == null) return;

            GameManager.MainTree.CollectEnergy(_activeNode.Unit.Data.SellPrice);
            GameObject.Destroy(_activeNode.Unit.gameObject);

            _activeNode.SetUnit(null);
        }

        private void OnUpgradeUnit()
        {
            // Tree upgrade
            if (_activeNode.Parent == null)
            {
                if (GameManager.MainTree.EnergyAmount < GameManager.MainTree.UpgradeCost)
                    return;
                
                GameManager.MainTree.ConsumeEnergy(GameManager.MainTree.UpgradeCost);
                GameManager.MainTree.Upgrade();

                Hide();
            }

            // Unit upgrade
            if (_activeNode.Unit != null)
            {
                if (GameManager.MainTree.EnergyAmount < _activeNode.Unit.Data.UpgradeCost)
                    return;
                
                Unit unit = CreateUnit(_activeNode.Unit.Data.UpgradePrefab);

                if (unit == null) return;
  
                GameObject.Destroy(_activeNode.Unit.gameObject);
                GameManager.MainTree.ConsumeEnergy(_activeNode.Unit.Data.UpgradeCost);

                _activeNode.SetUnit(unit);

                Hide();

                if (unit.Data.Type != UnitType.Spawner) return;

                _targetSelection.Show(unit);

                TimeManager.SlowMotion();
            }
        }

        private void OnChangeTarget()
        {
            if (_activeNode == null) return;
            if (_activeNode.Unit == null) return;

            Hide();

            _targetSelection.Show(_activeNode.Unit);

            TimeManager.SlowMotion();
        }

        private void OnTargetSelectionClose()
        {
            TimeManager.Resume();

            Hide();
        }
        
        private void OnConfirmTarget(Vector3 position)
        {
            if (!_activeNode.Unit.TryGetComponent<ProjectileLauncher>(out ProjectileLauncher launcher)) return;

            launcher.Target.position = position;
            _activeNode.Unit.SetTargetPosition(position);
        }
    }
}