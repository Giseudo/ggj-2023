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
        private Transform _branch;

        [SerializeField]
        private Unit _unit;

        private Tree _tree;
        private RootNode _parent;
        private List<RootNode> _children = new List<RootNode>();

        public Unit Unit => _unit;
        public RootNode Parent => _parent;
        public List<RootNode> Children => _children;
        public Tree Tree => _tree;

        public void Awake()
        {
            _tree = GetComponentInParent<Tree>();
        }

        public void AddNode(RootNode node)
        {
            if (node == null) return;

            node.SetParent(this);

            _children.Add(node);
        }

        public void RemoveNode(RootNode node)
        {
            _children.Remove(node);
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

            _branch.DOScaleY(length, 1f);
            _branch.DOMove(transform.position, 1f);
        }

        public void ShrinkBranch()
        {
            _branch.DOScaleY(0f, 1f);
            _branch.DOMove(_parent.transform.position, 1f);
        }
    }
}