using System;
using UnityEngine;

namespace Game.Combat
{
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private UnitData _data;

        public UnitData Data => _data;

        private Vector3 _targetPosition;
        private Unit _parent;

        public Unit Parent => _parent;
        public Vector3 TargetPosition => _targetPosition;

        public Action<Unit> parentChanged = delegate { };
        public Action<Vector3> targetPositionChanged = delegate { };
        public Action<Vector3> reachedTargetPosition = delegate { };

        public void SetTargetPosition(Vector3 position)
        {
            targetPositionChanged.Invoke(position);
            _targetPosition = position;
        }

        public void SetParent(Unit parent)
        {
            parentChanged.Invoke(parent);

            _parent = parent;
            //_parent?.AddChild(this);
        }

        // public void AddChild(Unit child)
        // {
        //     _children.Add(child);
        // }

        // public void RemoveChild(Unit child)
        // {
        //     _children.Remove(child);
        // }
    }
}