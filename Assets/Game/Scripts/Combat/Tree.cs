using System;
using UnityEngine;

namespace Game.Combat
{
    public class Tree : MonoBehaviour
    {
        [SerializeField]
        private float _rootMaxDistance = 20f;

        [SerializeField]
        private int _energyAmount = 400;

        public Action<int, Vector3> collectedEnergy = delegate { };
        public Action<int, Vector3> consumedEnergy = delegate { };

        public float RootMaxDistance => _rootMaxDistance;
        public int EnergyAmount => _energyAmount;
        public bool HasEnergy(int value) => value >= _energyAmount;

        public void CollectEnergy(int value, Vector3 position)
        {
            _energyAmount += value;

            collectedEnergy.Invoke(value, position);
        }

        public void ConsumeEnergy(int value, Vector3 position)
        {
            _energyAmount -= value;

            if (_energyAmount < 0)
                _energyAmount = 0;
            
            consumedEnergy.Invoke(value, position);
        }
    }
}