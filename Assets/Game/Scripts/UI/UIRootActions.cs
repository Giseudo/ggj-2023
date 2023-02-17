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
<<<<<<< HEAD
        public UIRootActionButton _addButton;

        [SerializeField]
        public UIRootActionButton _upgradeButton;

        [SerializeField]
        public UIRootActionButton _killButton;

        [SerializeField]
        public UIRootActionButton _splitButton;

        [SerializeField]
        public UIRootActionButton _targetButton;
=======
        public Button _addButton;

        [SerializeField]
        public Button _upgradeButton;

        [SerializeField]
        public Button _killButton;

        [SerializeField]
        public Button _splitButton;

        [SerializeField]
        public Button _targetButton;
>>>>>>> 6425a48 (Refactored RootActions class)

        [SerializeField]
        private AudioClip _unitCreationSound;

        private bool _isOpened;
        private RectTransform _rect;
        private RootNode _activeNode;
        private Vector3 _initialCameraPosition;
        private Tween _tween;
        private Tween _cameraTween;
<<<<<<< HEAD
        private Tween _fovTween;
=======
>>>>>>> 6425a48 (Refactored RootActions class)

        public UIUnitSelection UnitSelection => _unitSelection;
        public RectTransform Rect => _rect;
        public bool IsOpened => _isOpened;

        public Action opened = delegate { };
        public Action closed = delegate { };

<<<<<<< HEAD
        public UIRootActionButton AddButton => _addButton;
        public UIRootActionButton KillButton => _killButton;
        public UIRootActionButton SplitButton => _splitButton;
        public UIRootActionButton TargetButton => _targetButton;
        public UIRootActionButton UpgradeButton => _upgradeButton;
=======
        public Button AddButton => _addButton;
        public Button KillButton => _killButton;
        public Button SplitButton => _splitButton;
        public Button TargetButton => _targetButton;
        public Button UpgradeButton => _upgradeButton;
>>>>>>> 6425a48 (Refactored RootActions class)

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);

            if (_rect == null) return;

            _rect.localScale = Vector3.zero;
        }

        public void Start()
        {
            _initialCameraPosition = GameManager.MainCamera.transform.position;
<<<<<<< HEAD

            GameManager.MainTree.collectedEnergy += OnEnergyChange;
            GameManager.MainTree.consumedEnergy += OnEnergyChange;
        }

        public void OnDestroy()
        {
            GameManager.MainTree.collectedEnergy -= OnEnergyChange;
            GameManager.MainTree.consumedEnergy -= OnEnergyChange;
        }

        private void OnEnergyChange(int amount)
        {
            if (_activeNode == null) return;

            bool enabled = false;

            if (_activeNode.Unit != null)
                enabled =  GameManager.MainTree.EnergyAmount > _activeNode.Unit.Data.UpgradeCost;

            if (_activeNode.Parent == null)
                enabled =  GameManager.MainTree.EnergyAmount > GameManager.MainTree.UpgradeCost;

            if (enabled)
                _upgradeButton.EnergyButton.Enable();
            else
                _upgradeButton.EnergyButton.Disable();
        }

        public void OnEnable()
        {
            _addButton.clicked += OnAddUnit;
            _killButton.clicked += OnKillUnit;
            _upgradeButton.clicked += OnUpgradeUnit;

            _unitSelection.clicked += OnCreateUnit;
            _unitSelection.selected += OnSelectUnit;
            _unitSelection.closed += OnUnitSelectionClose;
        }

        public void OnDisable()
        {
            _addButton.clicked -= OnAddUnit;
            _killButton.clicked -= OnKillUnit;
            _upgradeButton.clicked -= OnUpgradeUnit;

            _unitSelection.clicked -= OnCreateUnit;
            _unitSelection.selected -= OnSelectUnit;
            _unitSelection.closed -= OnUnitSelectionClose;
        }

        public void Show(RootNode node)
        {
            _activeNode = node;

            _splitButton.EnergyButton.SetText($"{GameManager.MainTree.RootEnergyCost}");

=======
        }

        public void OnEnable()
        {
            _addButton.onClick.AddListener(OnAddUnit);

            _unitSelection.clicked += OnCreateUnit;
            _unitSelection.selected += OnSelectUnit;
            _unitSelection.closed += OnUnitSelectionClose;
        }

        public void OnDisable()
        {
            _addButton.onClick.RemoveListener(OnAddUnit);

            _unitSelection.clicked -= OnCreateUnit;
            _unitSelection.selected -= OnSelectUnit;
            _unitSelection.closed -= OnUnitSelectionClose;
        }

        public void Show(RootNode node)
        {
            _activeNode = node;

>>>>>>> 6425a48 (Refactored RootActions class)
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
<<<<<<< HEAD
                _upgradeButton.gameObject.SetActive(node.Unit.Data.UpgradePrefab);
                _targetButton.gameObject.SetActive(false);
                // _targetButton.gameObject.SetActive(true); // TODO: only for sementinha :3

                _killButton.EnergyButton.SetText($"{node.Unit.Data.SellPrice}");
                _upgradeButton.EnergyButton.SetText($"{node.Unit.Data.UpgradeCost}");
=======
                _upgradeButton.gameObject.SetActive(true);
                _targetButton.gameObject.SetActive(false);
                // _targetButton.gameObject.SetActive(true); // TODO: only for sementinha :3
>>>>>>> 6425a48 (Refactored RootActions class)
            }

            if (node.Parent == null)
            {
                _addButton.gameObject.SetActive(false);
                _killButton.gameObject.SetActive(false);
<<<<<<< HEAD
                _upgradeButton.gameObject.SetActive(GameManager.MainTree.CurrentLevel < GameManager.MainTree.MaxLevel);
                _targetButton.gameObject.SetActive(false);

                _upgradeButton.EnergyButton.SetText($"{GameManager.MainTree.UpgradeCost}");
=======
                _upgradeButton.gameObject.SetActive(true);
                _targetButton.gameObject.SetActive(false);
>>>>>>> 6425a48 (Refactored RootActions class)
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
<<<<<<< HEAD
            _cameraTween = GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x + (15f * direction), 3f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            _fovTween?.Kill();
            _fovTween = GameManager.MainCamera.DOOrthoSize(30f, 3f)
=======
            _cameraTween = GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x + (20f * direction), 2f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
            GameManager.MainCamera.DOOrthoSize(30f, 2f)
>>>>>>> 6425a48 (Refactored RootActions class)
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
<<<<<<< HEAD

            _fovTween?.Kill();
            _fovTween = GameManager.MainCamera.DOOrthoSize(25f, 1f)
=======
            GameManager.MainCamera.DOOrthoSize(25f, 1f)
>>>>>>> 6425a48 (Refactored RootActions class)
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

<<<<<<< HEAD
        private void OnDeselectUnit()
=======
        public void OnDeselectUnit()
>>>>>>> 6425a48 (Refactored RootActions class)
        {
            _unitRangeRadius.SetRadius(0f);
        }

<<<<<<< HEAD
        private void OnCreateUnit(UnitData data)
=======
        public void OnCreateUnit(UnitData data)
>>>>>>> 6425a48 (Refactored RootActions class)
        {
            if (data.RequiredEnergy > GameManager.MainTree.EnergyAmount)
                return;

            _unitSelection.Hide();

<<<<<<< HEAD
            Unit unit = CreateUnit(data.Prefab);

            if (unit == null) return;

            _activeNode.SetUnit(unit);

            GameManager.MainTree.ConsumeEnergy(data.RequiredEnergy);
        }

        private Unit CreateUnit(GameObject prefab)
        {
            GameObject instance = GameObject.Instantiate(prefab, _activeNode.transform);
=======
            GameObject instance = GameObject.Instantiate(data.Prefab, _activeNode.transform);
>>>>>>> 6425a48 (Refactored RootActions class)

            instance.transform.localScale = Vector3.zero;
            instance.transform.DOScale(Vector3.one, 1f)
                .SetUpdate(true)
                .SetEase(Ease.OutElastic);

            SoundManager.PlaySound(_unitCreationSound);

<<<<<<< HEAD
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
            }
=======
            if (!instance.TryGetComponent<Unit>(out Unit unit)) return;

            _activeNode.SetUnit(unit);

            GameManager.MainTree.ConsumeEnergy(data.RequiredEnergy);
>>>>>>> 6425a48 (Refactored RootActions class)
        }
    }
}