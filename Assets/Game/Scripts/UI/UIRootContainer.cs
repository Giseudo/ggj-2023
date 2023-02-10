using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using Unity.Mathematics;
using Game.Core;
using Game.Combat;
using Shapes;

namespace Game.UI
{
    public class UIRootContainer : ImmediateModeShapeDrawer, IPointerMoveHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private GameObject _rootPrefab;

        [SerializeField]
        private UIUnitSelection _unitSelection;

        [SerializeField]
        private UIRootActions _rootActions;

        [SerializeField]
        private UIRootPoint _rootPoint;

        [SerializeField]
        private RectTransform _mainCanvasRect;

        private RootNode _activeNode;
        private Vector3 _draggingPosition;
        private bool _isDragging = false;
        private bool _isValidPlacement = false;

        public override void OnEnable()
        {
            base.OnEnable();

            _unitSelection.selectedUnit += OnSelectUnit;
            _unitSelection.opened += OnUnitSelectionOpen;
            _rootActions.opened += OnActionsOpen;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            _unitSelection.selectedUnit -= OnSelectUnit;
            _unitSelection.opened -= OnUnitSelectionOpen;
            _rootActions.opened += OnActionsOpen;
        }

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;
            
            _draggingPosition = groundHit.point;

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

            _isDragging = true;
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

            // Max distance
            _isValidPlacement = Vector3.Distance(_activeNode.transform.position, _draggingPosition) < _activeNode.Tree.RootMaxDistance;

            if (GameManager.MainTree.EnergyAmount < GameManager.MainTree.RootEnergyCost)
                _isValidPlacement = false;

            // Obstacles
            Collider[] colliders = Physics.OverlapSphere(_draggingPosition, 2f, 1 << LayerMask.NameToLayer("Obstacle"));

            if (colliders.Length > 0)
                _isValidPlacement = false;

            // Root nodes
            for (int i = 0; i < GameManager.MainTree.NodeList.Count; i++)
            {
                RootNode node = GameManager.MainTree.NodeList[i];

                if (node.Parent == null) continue;

                if (Vector3.Distance(_draggingPosition, FindNearestPointOnLine(node.transform.position, node.Parent.transform.position, _draggingPosition)) < 5f)
                    _isValidPlacement = false;
            }

            // Lanes
            for (int i = 0; i < MatchManager.WaveSpawners.Count; i++)
            {
                WaveSpawner spawner = MatchManager.WaveSpawners[i];

                SplineUtility.GetNearestPoint(spawner.Spline.Spline, _draggingPosition, out float3 closest, out float t);

                if (Vector3.Distance(_draggingPosition, closest) < 3f)
                    _isValidPlacement = false;
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

            _isDragging = false;
            CreateNode();
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (_rootActions.IsOpened)
                _rootActions.Hide();

            if (_unitSelection.IsOpened)
                _unitSelection.Hide();

            if (_activeNode == null) return;

            if (_isDragging)
            {
                _isDragging = false;
                CreateNode();
                return;
            }

            // TODO Select unit if is not empty
            if (_activeNode.Unit != null) return;
        }

        public void OnUnitSelectionOpen()
        {
            _unitSelection.Rect.anchoredPosition = _rootActions.Rect.anchoredPosition + Vector2.up * 55f;
            _rootActions.Hide();
            _rootPoint.Hide();
        }

        public void OnActionsOpen()
        {
            _rootActions.AddButton.interactable = _activeNode.Unit == null;
        }

        private void CreateNode()
        {
            if (!_isValidPlacement) return;

            GameObject instance = GameObject.Instantiate(_rootPrefab, _activeNode.transform);

            instance.TryGetComponent<RootNode>(out RootNode node);

            if (node == null) return;

            _activeNode.AddNode(node);

            node.transform.position = _draggingPosition;
            node.GrowBranch();

            _activeNode = node;

            GameManager.MainTree.ConsumeEnergy(GameManager.MainTree.RootEnergyCost);
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

        public void SplitRoot()
        {
            _isDragging = true;
        }

        public void OnSelectUnit(UnitData data)
        {
            Debug.Log(GameManager.MainTree.EnergyAmount);

            if (data.RequiredEnergy > GameManager.MainTree.EnergyAmount)
                return;

            _unitSelection.Hide();

            GameObject instance = GameObject.Instantiate(data.Prefab, _activeNode.transform);

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