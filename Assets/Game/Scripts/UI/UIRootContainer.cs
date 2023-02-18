using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;

namespace Game.UI
{
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

            _rootActions.SplitButton.onClick.AddListener(OnSplitAction);
        }

        public void OnDisable()
        {
            _rootPoint.clicked -= OnPointClick;
            _rootActions.opened -= OnActionsOpen;
            _rootActions.closed -= OnActionsClose;
            _rootCreation.nodeCreated -= OnNodeCreation;

            _rootActions.SplitButton.onClick.RemoveListener(OnSplitAction);
        }

        private void OnPointClick()
        {
            _rootActions.Show(_activeNode);
        }

        private void OnNodeCreation(RootNode node)
        {
            _activeNode = node;
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
                _rootPoint.Hide();

                return;
            }

            _rootPoint.Rect.anchoredPosition = UICanvas.GetScreenPosition(closestNode.transform.position);
            _rootPoint.ShowInner();

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
    }
}