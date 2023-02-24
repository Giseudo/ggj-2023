using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Freya;
using Game.Combat;

namespace Game.Combat
{
    public class RootNode : MonoBehaviour
    {
        [SerializeField]
        private RootNode _parent;

        [SerializeField]
        private Transform _branch;

        [SerializeField]
        private Unit _unit;

        private SphereCollider _collider;
        private Tree _tree;

        private List<RootNode> _children = new List<RootNode>();

        public Unit Unit => _unit;
        public RootNode Parent => _parent;
        public List<RootNode> Children => _children;
        public Tree Tree => _tree;
        public void SetTree(Tree tree) => _tree = tree;

        public void Awake()
        {
            _tree = GetComponentInParent<Tree>();

            TryGetComponent<SphereCollider>(out _collider);
        }

        public void AddNode(RootNode node)
        {
            if (node == null) return;

            node.SetParent(this);

            _children.Add(node);
            _tree.AddNode(node);
        }

        public void RemoveNode(RootNode node)
        {
            _children.Remove(node);
            _tree.RemoveNode(node);
        }

        public void SetParent(RootNode node)
        {
            _parent = node;
        }

        public void SetUnit(Unit unit)
        {
            _unit = unit;
        }

        public void GrowBranch()
        {
            Vector2 direction = (_parent.transform.position.XZ() - transform.position.XZ()).normalized;
            float angle = Mathf.Rad2Deg * Mathfs.DirToAng(direction);
            float length = Vector3.Distance(transform.position, _parent.transform.position);

            _branch.position = _parent.transform.position;
            _branch.localScale = new Vector3(1f, 1f, 1f);
            _branch.eulerAngles = new Vector3(0f, -angle, 90f);

            _branch.DOScaleY(length, 1f).SetUpdate(true);
            _branch.DOMove(transform.position, 1f).SetUpdate(true);
        }

        public void ShrinkBranch()
        {
            _branch.DOScaleY(0f, 1f).SetUpdate(true);
            _branch.DOMove(_parent.transform.position, 1f).SetUpdate(true);
        }

        public void Disable() => _collider.enabled = false;
        public void Enable() => _collider.enabled = true;
    }
}