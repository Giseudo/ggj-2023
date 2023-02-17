using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;
<<<<<<< HEAD
=======
using Shapes;
>>>>>>> 6425a48 (Refactored RootActions class)

namespace Game.UI
{
    using Game.Combat;

    public class UIRootContainer : MonoBehaviour, IPointerMoveHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
<<<<<<< HEAD
        private UIRootPoint _rootPoint;
=======
        private GameObject _rootPrefab;
>>>>>>> 6425a48 (Refactored RootActions class)

        [SerializeField]
        private UIRootSelector _rootSelector;

        [SerializeField]
        private UIRootActions _rootActions;

        [SerializeField]
        private UIRootCreation _rootCreation;

<<<<<<< HEAD
=======
        [SerializeField]
        private UIRootLimit _rootLimit;


        [SerializeField]
        private AudioClip _rootCreationSound;

        [SerializeField]
        private AudioClip _errorSound;

        [SerializeField]
        private Canvas _mainCanvas;

        private Vector3 _initialCameraPosition;
>>>>>>> 6425a48 (Refactored RootActions class)
        private Tree _mainTree;
        private RootNode _activeNode;

        public RootNode ActiveNode => _activeNode;

        public void Start()
        {
            _mainTree = GameManager.MainTree;
            _rootCreation.Init(this);
        }

        public void OnDestroy()
        { }

        public void OnEnable()
        {
            _rootPoint.clicked += OnPointClick;
            _rootActions.opened += OnActionsOpen;
            _rootActions.closed += OnActionsClose;
            _rootCreation.nodeCreated += OnNodeCreation;
            _rootActions.SplitButton.clicked += OnSplitAction;
        }

        public void OnDisable()
        {
            _rootPoint.clicked -= OnPointClick;
            _rootActions.opened -= OnActionsOpen;
            _rootActions.closed -= OnActionsClose;
            _rootCreation.nodeCreated -= OnNodeCreation;
            _rootActions.SplitButton.clicked -= OnSplitAction;
        }

        private void OnPointClick()
        {
            _rootActions.Show(_activeNode);
        }

        private void OnNodeCreation(RootNode node)
        {
<<<<<<< HEAD
            _activeNode = node;
=======
            base.OnDisable();

            _rootPoint.clicked -= OnPointClick;
            _rootActions.opened -= OnActionsOpen;
            _rootActions.closed -= OnActionsClose;
>>>>>>> 6425a48 (Refactored RootActions class)
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            if (_rootActions.UnitSelection.IsOpened) return;

            _rootPoint.Hide();
            _rootCreation.StartDrag();
        }

        private void OnSplitAction() => _rootCreation.StartDrag();
        public void OnDrag(PointerEventData evt) => _rootCreation.Drag(evt);
        public void OnEndDrag(PointerEventData evt) => _rootCreation.EndDrag();

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;
            
            if (_rootActions.IsOpened) return;
            if (_rootActions.UnitSelection.IsOpened) return;
<<<<<<< HEAD
            if (_rootCreation.IsDragging) return;
=======

            if (_isDragging) {
                DragRoot(evt);
                return;
            }
>>>>>>> 6425a48 (Refactored RootActions class)

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

            _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(closestNode.transform.position);
            _rootPoint.ShowInner();

            _rootActions.Rect.anchoredPosition = UICanvas.GetScreenPosition(closestNode.transform.position);

            closestNode.TryGetComponent<RootNode>(out _activeNode);
        }

<<<<<<< HEAD
=======
        public void OnBeginDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;
            if (_rootActions.IsOpened) return;
            if (_rootActions.UnitSelection.IsOpened) return;

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
            if (_rootActions.UnitSelection.IsOpened) return;

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
                    
                    if (node.Tree == _mainTree)
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

>>>>>>> 6425a48 (Refactored RootActions class)
        public void OnPointerClick(PointerEventData evt)
        {
            if (_rootActions.IsOpened)
                _rootActions.Hide();
            
            if (_rootActions.UnitSelection.IsOpened)
                _rootActions.UnitSelection.Hide();

            _rootSelector.Hide();

            if (_activeNode == null)
                return;
<<<<<<< HEAD
=======

            if (_isDragging && evt.button == PointerEventData.InputButton.Left)
                SplitRoot();
            
            if (_isDragging && evt.button == PointerEventData.InputButton.Right)
                _isDragging = false;

            // TODO Select unit if is not empty
            if (_activeNode.Unit != null) return;
>>>>>>> 6425a48 (Refactored RootActions class)
        }

        public void OnActionsOpen()
        {
            _rootPoint.Disable();
            _rootPoint.Hide();

            _rootSelector.transform.position = _activeNode.transform.position + Vector3.up * .2f;
            _rootSelector.Show();
        }

        public void OnActionsClose()
        {
            _rootPoint.Enable();

            if (_rootActions.UnitSelection.IsOpened) return;

            _rootSelector.Hide();
        }
<<<<<<< HEAD
=======

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

        private Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= _mainCanvasRect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= _mainCanvasRect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - _mainCanvasRect.sizeDelta / 2f;
        }
>>>>>>> 6425a48 (Refactored RootActions class)
    }
}