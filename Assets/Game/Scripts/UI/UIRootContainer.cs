using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using Unity.Mathematics;
using Game.Core;
using Shapes;
using DG.Tweening;

namespace Game.UI
{
    using Game.Combat;

    public class UIRootContainer : ImmediateModeShapeDrawer, IPointerMoveHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private GameObject _rootPrefab;

        [SerializeField]
        private UIUnitSelection _unitSelection;

        [SerializeField]
        private UIRangeRadius _unitRangeRadius;

        [SerializeField]
        private UIRootSelector _rootSelector;

        [SerializeField]
        private UIRootActions _rootActions;

        [SerializeField]
        private UIRootPoint _rootPoint;

        [SerializeField]
        private UIRootLimit _rootLimit;

        [SerializeField]
        private AudioClip _unitCreationSound;

        [SerializeField]
        private AudioClip _rootCreationSound;

        [SerializeField]
        private AudioClip _errorSound;

        [SerializeField]
        private Canvas _mainCanvas;

        private Vector3 _initialCameraPosition;
        private Tree _mainTree;
        private RectTransform _mainCanvasRect;
        private RootNode _activeNode;
        private Vector3 _draggingPosition;
        private bool _isDragging = false;
        private bool _isValidPlacement = false;
        private Tree _activeTree;

        public void Awake()
        {
            _mainCanvas.TryGetComponent<RectTransform>(out _mainCanvasRect);
        }

        public void Start()
        {
            _mainTree = GameManager.MainTree;
            _mainTree.rootSplitted += OnRootSplit;
            _initialCameraPosition = GameManager.MainCamera.transform.position;
        }

        public void OnDestroy()
        {
            _mainTree.rootSplitted += OnRootSplit;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            _unitSelection.clicked += OnCreateUnit;
            _unitSelection.selected += OnSelectUnit;
            _unitSelection.opened += OnUnitSelectionOpen;
            _unitSelection.closed += OnUnitSelectionClose;
            _rootActions.opened += OnActionsOpen;
            _rootActions.closed += OnActionsClose;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            _unitSelection.clicked -= OnCreateUnit;
            _unitSelection.selected -= OnSelectUnit;
            _unitSelection.opened -= OnUnitSelectionOpen;
            _unitSelection.closed -= OnUnitSelectionClose;
            _rootActions.opened -= OnActionsOpen;
            _rootActions.closed -= OnActionsClose;
        }

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;
            
            _draggingPosition = groundHit.point;
            _rootLimit.Rect.anchoredPosition = (evt.position / _mainCanvas.scaleFactor) + new Vector2(-20f, 20f);

            if (_rootActions.IsOpened) return;
            if (_unitSelection.IsOpened) return;

            if (_isDragging) {
                DragRoot(evt);
                return;
            }

            Collider[] colliders = Physics.OverlapSphere(groundHit.point, 3f, 1 << LayerMask.NameToLayer("RootNode"));
            GameObject closestNode = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider.TryGetComponent<RootNode>(out RootNode node))
                {
                    if (node.Tree != GameManager.MainTree && node.Tree.ParentTree != GameManager.MainTree)
                        continue;
                }

                float distance = (groundHit.point - collider.transform.position).sqrMagnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNode = collider.gameObject;
                }
            }

            if (closestNode == null)
            {
                _activeNode = null;
                _rootPoint.Hide();

                return;
            }

            _rootPoint.Rect.anchoredPosition = GetScreenPosition(closestNode.transform.position);
            _rootPoint.ShowInner();

            _rootActions.Rect.anchoredPosition = GetScreenPosition(closestNode.transform.position);

            closestNode.TryGetComponent<RootNode>(out _activeNode);
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;
            if (_rootActions.IsOpened) return;
            if (_unitSelection.IsOpened) return;

            StartDrag();
        }

        public void StartDrag()
        {
            _isDragging = true;
            _rootLimit.Show();
        }

        public void OnDrag(PointerEventData evt)
        {
            DragRoot(evt);
        }

        private void DragRoot(PointerEventData evt)
        {
            if (_activeNode == null) return;
            if (_rootActions.IsOpened) return;
            if (_unitSelection.IsOpened) return;

            _isValidPlacement = true;
            _activeTree = null;

            // Max root split
            if (_mainTree.RootSplitLimit == 0)
            {
                _isValidPlacement = false;
                return;
            }

            // Tree
            Collider[] treeColliders = Physics.OverlapSphere(_draggingPosition, 1f, 1 << LayerMask.NameToLayer("RootNode"));

            for (int i = 0; i < treeColliders.Length; i++)
            {
                Collider collider = treeColliders[i];

                if (collider.TryGetComponent<RootNode>(out RootNode node))
                {
                    if (node.Parent)
                        continue;
                    
                    if (node.Tree.ParentTree != null)
                        continue;

                    _activeTree = node.Tree;

                    return;
                }
            }

            // Max distance
            if (Vector3.Distance(_activeNode.transform.position, _draggingPosition) > _activeNode.Tree.RootMaxDistance)
            {
                _isValidPlacement = false;
                return;
            }

            // Not enough energy
            if (GameManager.MainTree.EnergyAmount < GameManager.MainTree.RootEnergyCost)
            {
                _isValidPlacement = false;
                return;
            }

            // Obstacles
            Collider[] obstaclesColliders = Physics.OverlapSphere(_draggingPosition, 2f, 1 << LayerMask.NameToLayer("Obstacle"));

            if (obstaclesColliders.Length > 0)
            {
                _isValidPlacement = false;
                return;
            }

            // Root nodes
            for (int i = 0; i < GameManager.MainTree.NodeList.Count; i++)
            {
                RootNode node = GameManager.MainTree.NodeList[i];

                if (node.Parent == null) continue;

                if (Vector3.Distance(_draggingPosition, FindNearestPointOnLine(node.transform.position, node.Parent.transform.position, _draggingPosition)) < 5f)
                {
                    _isValidPlacement = false;
                    return;
                }
                
                if (Vector3.Distance(_draggingPosition, node.transform.position) < 5f)
                {
                    _isValidPlacement = false;
                    return;
                }
            }

            // Lanes
            for (int i = 0; i < MatchManager.WaveSpawners.Count; i++)
            {
                WaveSpawner spawner = MatchManager.WaveSpawners[i];

                SplineUtility.GetNearestPoint(spawner.Spline.Spline, _draggingPosition, out float3 closest, out float t);

                if (Vector3.Distance(_draggingPosition, closest) < 3f)
                {
                    _isValidPlacement = false;
                    return;
                }
            }
        }

        public Vector3 FindNearestPointOnLine(Vector3 origin, Vector3 end, Vector3 point)
        {
            Vector3 heading = (end - origin);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            Vector3 lhs = point - origin;
            float dotP = Vector3.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return origin + heading * dotP;
        }

        public void OnEndDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;

            SplitRoot();
        }

        private void SplitRoot()
        {
            _isDragging = false;
            _rootLimit.Hide();
            _rootSelector.Hide();

            if (!_isValidPlacement) return;

            _mainTree.SplitRoot();
        }

        private void OnRootSplit()
        {
            _rootLimit.SetText($"{_mainTree.RootSplitLimit}");
            CreateNode();
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (_rootActions.IsOpened)
                _rootActions.Hide();
            
            if (_unitSelection.IsOpened)
                _unitSelection.Hide();

            _rootSelector.Hide();

            if (_activeNode == null)
                return;

            if (_isDragging && evt.button == PointerEventData.InputButton.Left)
                SplitRoot();
            
            if (_isDragging && evt.button == PointerEventData.InputButton.Right)
                _isDragging = false;

            // TODO Select unit if is not empty
            if (_activeNode.Unit != null) return;
        }

        public void OnUnitSelectionOpen()
        {
            _unitSelection.Rect.anchoredPosition = _rootActions.Rect.anchoredPosition + Vector2.up * 55f;
            _rootActions.Hide();
            _rootPoint.Hide();

            float direction = _activeNode.transform.position.x < GameManager.MainCamera.transform.position.x ? -1 : 1;

            GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x + (20f * direction), 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
            GameManager.MainCamera.DOOrthoSize(23f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            _unitRangeRadius.transform.position = _activeNode.transform.position + Vector3.up * .2f;

            TimeManager.SlowMotion();
        }

        public void OnUnitSelectionClose()
        {
            GameManager.MainCamera.transform.DOMoveX(_initialCameraPosition.x, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
            GameManager.MainCamera.DOOrthoSize(25f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
            
            OnDeselectUnit();

            _rootSelector.Hide();

            TimeManager.Resume();
        }

        private void OnSelectUnit(UnitData data)
        {
            if (!_unitSelection.IsOpened) return;

            _unitRangeRadius.SetRadius(data == null ? 0f : data.RangeRadius);

            if (data == null)
                OnDeselectUnit();
        }

        public void OnDeselectUnit()
        {
            _unitRangeRadius.SetRadius(0f);
        }

        public void OnActionsOpen()
        {
            bool canAdd = _activeNode.Unit == null;

            _rootPoint.Disable();
            _rootPoint.Hide();
            _rootSelector.transform.position = _activeNode.transform.position + Vector3.up * .2f;
            _rootSelector.Show();

            if (_activeNode.Parent == null) // is tree
                canAdd = false;
            
            if (_activeNode.Unit) // has unit
            {
                _rootActions.ShowUpgradeButton();
                _rootActions.ShowKillButton();
            }

            if (_activeNode.Unit == null) // empty slot
            {
                _rootActions.HideUpgradeButton();
                _rootActions.HideKillButton();
            }

            _rootActions.AddButton.interactable = canAdd;
        }

        public void OnActionsClose()
        {
            _rootPoint.Enable();

            if (_unitSelection.IsOpened) return;

            _rootSelector.Hide();
        }

        private void CreateNode()
        {
            if (!_isValidPlacement) return;

            GameObject instance = GameObject.Instantiate(_rootPrefab, _activeNode.transform);

            instance.TryGetComponent<RootNode>(out RootNode node);

            if (node == null) return;

            _activeNode.AddNode(node);

            node.transform.position = _draggingPosition;

            if (_activeTree != null)
            {
                GameManager.MainTree.AbsorbTree(_activeTree);
                MatchManager.DropEnergy(_activeTree.EnergyAmount, _activeTree.transform.position);

                node.transform.position = _activeTree.transform.position;
                node.Disable();

                _activeTree = null;
            }

            node.GrowBranch();
            _rootSelector.Hide();

            _activeNode = node;

            GameManager.MainTree.ConsumeEnergy(GameManager.MainTree.RootEnergyCost);
            SoundManager.PlaySound(_rootCreationSound);
        }

        public override void DrawShapes(Camera cam){
            using (Draw.Command(cam))
            {
                DrawDragLine();
            }
        }

        private void DrawDragLine()
        {
            if (!_isDragging) return;

            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Pixels;
            Draw.Thickness = 20;
            Draw.Line(_activeNode.transform.position, _draggingPosition, _isValidPlacement ? Color.green : Color.red);
        }

        public void OnCreateUnit(UnitData data)
        {
            if (data.RequiredEnergy > GameManager.MainTree.EnergyAmount)
                return;

            _unitSelection.Hide();
            _rootSelector.Hide();

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

        private Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= _mainCanvasRect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= _mainCanvasRect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - _mainCanvasRect.sizeDelta / 2f;
        }
    }
}