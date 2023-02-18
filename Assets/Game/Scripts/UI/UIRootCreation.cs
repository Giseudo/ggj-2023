using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;
using Unity.Mathematics;
using Game.Core;
using Shapes;

namespace Game.UI
{
    using Game.Combat;

    public class UIRootCreation : ImmediateModeShapeDrawer, IPointerMoveHandler, IPointerClickHandler
    {
        [SerializeField]
        private GameObject _rootPrefab;

        [SerializeField]
        private AudioClip _rootCreationSound;

        [SerializeField]
        private UIRootLimit _rootLimit;

        private UIRootContainer _rootContainer;
        private Vector3 _draggingPosition;
        private bool _isDragging = false;
        private bool _isValidPlacement = false;
        private Tree _mainTree;
        private Tree _activeTree;
        private Image _image;

        public Action<RootNode> nodeCreated = delegate { };

        public void OnBeginDrag(PointerEventData evt) => StartDrag();
        public void OnDrag(PointerEventData evt) => Drag(evt);
        public void OnEndDrag(PointerEventData evt) => EndDrag();

        public RootNode ActiveNode => _rootContainer?.ActiveNode;
        public bool IsDragging => _isDragging;
        private void OnTreeLevelUp(int level) => _rootLimit.SetText($"{_mainTree.RootSplitLimit}");

        public void Awake()
        {
            TryGetComponent<Image>(out _image);
        }

        public void Start()
        {
            _mainTree = GameManager.MainTree;
            _mainTree.rootSplitted += OnRootSplit;
            _mainTree.levelUp += OnTreeLevelUp;

            _image.enabled = false;
            _rootLimit.SetText($"{_mainTree.RootSplitLimit}");
        }

        public void Init(UIRootContainer rootContainer)
        {
            _rootContainer = rootContainer;
        }

        public void OnDestroy()
        {
            _mainTree.rootSplitted -= OnRootSplit;
            _mainTree.levelUp -= OnTreeLevelUp;
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (ActiveNode == null)
                return;

            if (_isDragging && evt.button == PointerEventData.InputButton.Left)
                SplitRoot();
            
            if (_isDragging && evt.button == PointerEventData.InputButton.Right)
            {
                _rootLimit.Hide();
                _isDragging = false;
            }
        }

        public void OnPointerMove(PointerEventData evt)
        {
            if (_isDragging) {
                DragRoot(evt);
                return;
            }
        }

        public void StartDrag()
        {
            if (ActiveNode == null) return;

            _image.enabled = true;
            _isDragging = true;
            _rootLimit.Show();
        }

        public void Drag(PointerEventData evt)
        {
            DragRoot(evt);
        }

        public void EndDrag()
        {
            SplitRoot();
        }

        private void DragRoot(PointerEventData evt)
        {
            if (ActiveNode == null) return;

            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;
            
            _draggingPosition = groundHit.point;
            _rootLimit.Rect.anchoredPosition = (evt.position / UICanvas.MainCanvas.scaleFactor) + new Vector2(-20f, 20f);

            // Root placement
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
            if (Vector3.Distance(ActiveNode.transform.position, _draggingPosition) > ActiveNode.Tree.RootMaxDistance)
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

        private void SplitRoot()
        {
            if (_isValidPlacement && _isDragging)
                _mainTree.SplitRoot();

            _isDragging = false;
            _image.enabled = false;

            _rootLimit.Hide();

            _draggingPosition = Vector3.zero;
        }

        private void OnRootSplit()
        {
            _rootLimit.SetText($"{_mainTree.RootSplitLimit}");
            CreateNode();
        }

        public override void DrawShapes(Camera cam){
            using (Draw.Command(cam))
            {
                DrawDragLine();
            }
        }

        private void DrawDragLine()
        {
            if (ActiveNode == null) return;
            if (!_isDragging) return;
            if (_draggingPosition == Vector3.zero) return;

            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.ThicknessSpace = ThicknessSpace.Pixels;
            Draw.Thickness = 20;
            Draw.Line(ActiveNode.transform.position, _draggingPosition, _isValidPlacement ? Color.green : Color.red);
        }

        private void CreateNode()
        {
            if (!_isValidPlacement) return;

            GameObject instance = GameObject.Instantiate(_rootPrefab, ActiveNode.transform);

            instance.TryGetComponent<RootNode>(out RootNode node);

            if (node == null) return;

            ActiveNode.AddNode(node);

            node.transform.position = _draggingPosition;

            if (_activeTree != null)
            {
                GameManager.MainTree.AbsorbTree(_activeTree);
                MatchManager.DropEnergy(_activeTree.EnergyAmount, _activeTree.transform.position);

                node.transform.position = _activeTree.transform.position;
                node.Disable();

                _activeTree = null;
            }

            GameManager.MainTree.ConsumeEnergy(GameManager.MainTree.RootEnergyCost);
            SoundManager.PlaySound(_rootCreationSound);

            node.GrowBranch();
            nodeCreated.Invoke(node);
        }
    }
}