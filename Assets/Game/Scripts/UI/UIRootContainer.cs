using UnityEngine;
using UnityEngine.EventSystems;
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
        private RectTransform _mainCanvasRect;

        [SerializeField]
        private RectTransform _actionsRect;

        [SerializeField]
        private RootSelectionShape _rootSelectionShape;

        private RootNode _activeNode;
        private Vector3 _draggingPosition;
        private bool _isDragging = false;
        private bool _isSelectingUnit = false;
        private bool _isValidPlacement = false;

        public override void OnEnable()
        {
            base.OnEnable();

            _unitSelection.selectedUnit += OnSelectUnit;
        }

        public override void OnDisable()
        {
            base.OnDisable();

            _unitSelection.selectedUnit -= OnSelectUnit;
        }

        public void OnPointerMove(PointerEventData evt)
        {
            if (_isDragging) return;
            if (_isSelectingUnit) return;

            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;

            Collider[] colliders = Physics.OverlapSphere(groundHit.point, 5f, 1 << LayerMask.NameToLayer("RootNode"));
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
                _rootSelectionShape.Hide();

                return;
            }

            _rootSelectionShape.Show();
            _rootSelectionShape.transform.position = closestNode.transform.position;

            closestNode.TryGetComponent<RootNode>(out _activeNode);
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;
            if (_isSelectingUnit) return;

            _isDragging = true;
        }

        public void OnDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;

            Ray ray = evt.enterEventCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;

            _draggingPosition = hit.point;
            _isValidPlacement = Vector3.Distance(_activeNode.transform.position, _draggingPosition) < _activeNode.Tree.RootMaxDistance;
        }

        public void OnEndDrag(PointerEventData evt)
        {
            if (_activeNode == null) return;

            _isDragging = false;


            if (!_isValidPlacement) return;

            CreateNode();
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (_activeNode == null) return;
            if (_isDragging) return;

            _actionsRect.localScale = Vector3.one;
            _actionsRect.anchoredPosition = GetScreenPosition(_activeNode.transform.position);

            // TODO Select unit if is not empty
            if (_activeNode.Unit != null) return;

            _isSelectingUnit = true;
            _unitSelection.Show();
        }

        private void CreateNode()
        {
            GameObject instance = GameObject.Instantiate(_rootPrefab, _activeNode.transform);

            instance.TryGetComponent<RootNode>(out RootNode node);

            if (node == null) return;

            _activeNode.AddNode(node);

            node.transform.position = _draggingPosition;
            node.GrowBranch();

            _activeNode = node;
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

        public void OnSelectUnit(UnitData data)
        {
            _isSelectingUnit = false;
            _unitSelection.Hide();

            GameObject instance = GameObject.Instantiate(data.Prefab, _activeNode.transform);

            if (!instance.TryGetComponent<Unit>(out Unit unit)) return;

            _activeNode.SetUnit(unit);
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