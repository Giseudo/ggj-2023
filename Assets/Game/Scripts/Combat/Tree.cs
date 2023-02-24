using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    [Serializable]
    public class TreeLevel
    {
        [SerializeField]
        private int _rootEnergyCost = 200;

        [SerializeField]
        private float _rootMaxDistance = 20f;

        [SerializeField]
        private int _upgradeCost = 2000;

        [SerializeField]
        private int _rootSplitBonus = 2;

        public int RootEnergyCost => _rootEnergyCost;
        public float RootMaxDistance => _rootMaxDistance;
        public int UpgradeCost => _upgradeCost;
        public int RootSplitBonus => _rootSplitBonus;
    }

    public class Tree : MonoBehaviour
    {
        [SerializeField]
        private int _energyAmount = 400;

        [SerializeField]
        private int _initialRootSplitLimit = 3;

        [SerializeField]
        private List<TreeLevel> _levels = new List<TreeLevel>();

        private int _currentLevel = 0;
        private int _rootSplitLimit = 0;
        private int _rootEnergyCost = 0;
        private float _rootMaxDistance = 0;
        private int _upgradeCost = 0;
        private List<RootNode> _nodeList;
        private List<Tree> _absorvedTrees = new List<Tree>();
        private Tree _parentTree;

        public Action<int> collectedEnergy = delegate { };
        public Action<int> consumedEnergy = delegate { };
        public Action<Tree> absorvedTree = delegate { };
        public Action rootSplitted = delegate { };
        public Action<int> levelUp = delegate { };

        public float RootMaxDistance => _rootMaxDistance;
        public int RootEnergyCost => _rootEnergyCost;
        public int EnergyAmount => _energyAmount;
        public bool HasEnergy(int value) => value >= _energyAmount;
        public List<RootNode> NodeList => _nodeList;
        public List<Tree> AbsorvedTrees => _absorvedTrees;
        public Tree ParentTree => _parentTree;
        public int RootSplitLimit => _rootSplitLimit;
        public int UpgradeCost => _upgradeCost;
        public int MaxLevel => _levels.Count - 1;
        public int CurrentLevel => _currentLevel;
        public int InitialRootSplitLimit => _initialRootSplitLimit;

        public void Awake()
        {
            _rootSplitLimit = _initialRootSplitLimit;
            _nodeList = GetComponentsInChildren<RootNode>(true).ToList();

            UpdateStats();
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

        public void SetParentTree(Tree tree)
        {
            _parentTree = tree;
        }

        public void AbsorbTree(Tree tree)
        {
            _rootSplitLimit += tree.InitialRootSplitLimit;
            _absorvedTrees.Add(tree);

            tree.SetParentTree(this);

            for (int i = 0; i < tree.NodeList.Count; i++)
            {
                RootNode node = tree.NodeList[i];

                node.gameObject.SetActive(true);
                node.SetTree(this);

                _nodeList.Add(node);
            }

            levelUp.Invoke(_currentLevel);
            absorvedTree.Invoke(tree);            
        }

        public void SplitRoot()
        {
            if (_rootSplitLimit <= 0) return;

            _rootSplitLimit -= 1;
            rootSplitted.Invoke();
        }

        public void Upgrade()
        {
            if (_currentLevel + 1 >= _levels.Count)
                return;

            _currentLevel += 1;

            UpdateStats();

            levelUp.Invoke(_currentLevel);
        }

        public void UpdateStats()
        {
            if (_levels.Count < 1) return;

            TreeLevel level = _levels[_currentLevel];

            _rootEnergyCost = level.RootEnergyCost;
            _rootMaxDistance = level.RootMaxDistance;
            _rootSplitLimit += level.RootSplitBonus;
            _upgradeCost = level.UpgradeCost;
        }
    }
}