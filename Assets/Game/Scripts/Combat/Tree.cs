using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class Tree : MonoBehaviour
    {
        [SerializeField]
        private int _energyAmount = 400;

        [SerializeField]
        private int _rootEnergyCost = 50;

        [SerializeField]
        private float _rootMaxDistance = 20f;

        private List<RootNode> _nodeList;

        public Action<int> collectedEnergy = delegate { };
        public Action<int> consumedEnergy = delegate { };

        public float RootMaxDistance => _rootMaxDistance;
        public int RootEnergyCost => _rootEnergyCost;
        public int EnergyAmount => _energyAmount;
        public bool HasEnergy(int value) => value >= _energyAmount;
        public List<RootNode> NodeList => _nodeList;

        public void Awake()
        {
            _nodeList = GetComponentsInChildren<RootNode>().ToList();
        }

        public void CollectEnergy(int value)
        {
            _energyAmount += value;

            collectedEnergy.Invoke(value);
        }

        public void ConsumeEnergy(int value)
        {
            _energyAmount -= value;

            if (_energyAmount < 0)
                _energyAmount = 0;
            
            consumedEnergy.Invoke(value);
        }

        public void AddNode(RootNode node)
        {
            _nodeList.Add(node);
        }

        public void RemoveNode(RootNode node)
        {
            _nodeList.Remove(node);
        }
    }
}