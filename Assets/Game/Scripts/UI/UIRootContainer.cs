using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;

namespace Game.UI
{
    using System;
    using Game.Combat;

    public class UIRootContainer : MonoBehaviour, IPointerMoveHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private UIRootPoint _rootPoint;

        [SerializeField]
        private UIRootSelector _rootSelector;

        [SerializeField]
        private UIRootActions _rootActions;

        [SerializeField]
        private UIRootCreation _rootCreation;

        [SerializeField]
        private UITreeHighlight _treeHighlight;

        [SerializeField]
        private UICameraPan _cameraPan;

        private RootNode _activeNode;
        private Vector2 _rootPointAnchoredPos;

        public RootNode ActiveNode => _activeNode;

        private void OnPointClick() => _rootActions.Show(_activeNode);
        public void Start() => _rootCreation.Init(this);
        private void OnSplitAction() => _rootCreation.StartDrag();
        public void OnDrag(PointerEventData evt) => _rootCreation.Drag(evt);
        public void OnEndDrag(PointerEventData evt) => _rootCreation.EndDrag();

        public void OnEnable()
        {
            _rootPoint.clicked += OnPointClick;
            _rootActions.opened += OnActionsOpen;
            _rootActions.closed += OnActionsClose;
            _rootActions.canceled += OnActionsCancel;
            _rootCreation.nodeCreated += OnNodeCreation;
            _rootActions.SplitButton.clicked += OnSplitAction;
            _rootActions.createdUnit += OnUnitCreation;
            _treeHighlight.highlightChanged += OnTreeHighlightChange;
            _cameraPan.started += OnCameraPanStart;
            _cameraPan.updated += OnCameraPanDrag;
        }

        public void OnDisable()
        {
            _rootPoint.clicked -= OnPointClick;
            _rootActions.opened -= OnActionsOpen;
            _rootActions.closed -= OnActionsClose;
            _rootActions.canceled -= OnActionsCancel;
            _rootCreation.nodeCreated -= OnNodeCreation;
            _rootActions.SplitButton.clicked -= OnSplitAction;
            _rootActions.createdUnit -= OnUnitCreation;
            _treeHighlight.highlightChanged -= OnTreeHighlightChange;
            _cameraPan.started -= OnCameraPanStart;
            _cameraPan.updated -= OnCameraPanDrag;
        }

        private void OnNodeCreation(RootNode node)
        {
            _activeNode = node;

            if (_rootActions.HighlightingButton == _rootActions.SplitButton.Button)
                _rootActions.Highlight(null);

            if (_treeHighlight.IsHighlighting)
                _treeHighlight.Highlight(false);

            _rootPoint.Pulse(false);

            if (!MatchManager.HasStarted)
                MatchManager.StartRound();

            if (GameManager.MainTree.Unities.Count > 0) return;
            if (GameManager.Scenes.CurrentLevel > 0) return;

            _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(node.transform.position);
            _rootPoint.Show();
            _rootPoint.Pulse(true);
            _rootActions.Highlight(_rootActions.AddButton.Button);
        }

        private void OnUnitCreation(Unit unit)
        {
            if (_rootActions.HighlightingButton == _rootActions.AddButton.Button)
                _rootActions.Highlight(null);
        }

        private void OnTreeHighlightChange(bool value)
        {
            if (value == false) return;
            if (GameManager.MainTree.NodeList.Count > 1) return;

            _rootActions.Highlight(_rootActions.SplitButton.Button);

            _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(GameManager.MainTree.transform.position);
            _rootPoint.Show();
            _rootPoint.Pulse(true);
        }

        private void OnCameraPanStart() {
            _rootPointAnchoredPos = _rootPoint.Rect.anchoredPosition;
            _rootActions.KillCameraTween();
        }

        private void OnCameraPanDrag(Vector3 displacement)
        {
            _rootPoint.Rect.anchoredPosition = _rootPointAnchoredPos;
            _rootPoint.Rect.position -= displacement;
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            if (_rootActions.UnitSelection.IsOpened) return;
            if (_rootActions.IsOpened) return;

            _rootPoint.Hide();
            _rootCreation.StartDrag();
        }

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;

            _rootCreation.UpdateRootLimitPosition((evt.position / UICanvas.MainCanvas.scaleFactor));
            
            if (_rootActions.IsOpened) return;
            if (_rootActions.UnitSelection.IsOpened) return;
            if (_rootActions.TargetSelection.IsOpened) return;
            if (_rootCreation.IsDragging) return;

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

                if (!_rootPoint.IsPulsing)
                    _rootPoint.Hide();

                return;
            }

            if (!_rootPoint.IsPulsing)
            {
                _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(closestNode.transform.position);
                _rootPoint.ShowInner();
            }

            _rootActions.Rect.anchoredPosition = UICanvas.GetScreenPosition(closestNode.transform.position);

            closestNode.TryGetComponent<RootNode>(out _activeNode);
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (_rootActions.IsOpened)
                _rootActions.Hide();
            
            if (_rootActions.UnitSelection.IsOpened)
                _rootActions.UnitSelection.Hide();

            _rootSelector.Hide();

            if (_activeNode == null)
                return;
        }

        public void OnActionsOpen()
        {
            if (_rootPoint.IsPulsing)
                _rootPoint.Pulse(false);

            _cameraPan.Disable();
            _rootPoint.Disable();
            _rootPoint.Hide();

            _rootSelector.transform.position = _activeNode.transform.position + Vector3.up * .2f;
            _rootSelector.Show();
        }

        public void OnActionsClose()
        {
            _rootPoint.Enable();

            if (_rootActions.UnitSelection.IsOpened) return;

            _cameraPan.Enable();
            _rootSelector.Hide();
        }

        public void OnActionsCancel()
        {
            if (_rootCreation.IsDragging)
                _rootCreation.Cancel();
        }
    }
}