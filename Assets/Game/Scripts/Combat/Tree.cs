using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Game.Core;

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
        private Transform _mesh;

        [SerializeField]
        private AudioClip _absorbClip;

        [SerializeField]
        private List<TreeLevel> _levels = new List<TreeLevel>();

        private int _currentLevel = 1;
        private int _rootSplitLimit = 0;
        private int _rootEnergyCost = 0;
        private float _rootMaxDistance = 0;
        private int _upgradeCost = 0;
        private bool _reachedMaxLevel = false;
        private List<RootNode> _nodeList;
        private List<Unit> _unities = new List<Unit>();
        private List<Tree> _absorvedTrees = new List<Tree>();
        private Tree _parentTree;
        private Animator _animator;

        public Action<int> collectedEnergy = delegate { };
        public Action<int> consumedEnergy = delegate { };
        public Action<int> energyChanged = delegate { };
        public Action<Tree> absorvedTree = delegate { };
        public Action rootSplitted = delegate { };
        public Action<int> levelUp = delegate { };

        public float RootMaxDistance => _rootMaxDistance;
        public int RootEnergyCost => _rootEnergyCost;
        public int EnergyAmount => _energyAmount;
        public bool HasEnergy(int value) => value >= _energyAmount;
        public Transform Mesh => _mesh;
        public List<RootNode> NodeList => _nodeList;
        public List<Unit> Unities => _unities;
        public List<Tree> AbsorvedTrees => _absorvedTrees;
        public Tree ParentTree => _parentTree;
        public int RootSplitLimit => _rootSplitLimit;
        public int UpgradeCost => _upgradeCost;
        public int MaxLevel => _levels.Count;
        public int CurrentLevel => _currentLevel;
        public int InitialRootSplitLimit => _initialRootSplitLimit;
        public bool ReachedMaxLevel => _reachedMaxLevel;
        public Animator Animator => _animator;
        public AudioClip AbsorbClip => _absorbClip;

        public void Awake()
        {
            _rootSplitLimit = _initialRootSplitLimit;
            _nodeList = GetComponentsInChildren<RootNode>(true).ToList();
            TryGetComponent<Animator>(out _animator);

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

        public void AddUnit(Unit unit)
        {
            _unities.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            _unities.Remove(unit);
        }

        public void SetParentTree(Tree tree)
        {
            _parentTree = tree;
        }

        public void SetEnergy(int value)
        {
            _energyAmount = value;

            if (_energyAmount < 0)
                _energyAmount = 0;
            
            energyChanged.Invoke(value);
        }

        public void AbsorbTree(Tree tree)
        {
            _rootSplitLimit += tree.InitialRootSplitLimit;
            _absorvedTrees.Add(tree);

            tree.SetParentTree(this);
            SoundManager.PlaySound(tree.AbsorbClip, 1f);

            StartCoroutine(Absorb(tree));
        }

        private IEnumerator Absorb(Tree tree)
        {
            tree.Animator?.SetBool("Absorbed", true);

            yield return new WaitForSeconds(2f);

            for (int i = 0; i < tree.NodeList.Count; i++)
            {
                RootNode node = tree.NodeList[i];

                node.GrowBranch();
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
            if (_reachedMaxLevel) return;
            if (_currentLevel >= MaxLevel) return;

            _mesh?.DOScale(Vector3.one + Vector3.one * ((float)_currentLevel * .2f), 1f)
                .SetEase(Ease.OutElastic);

            _currentLevel += 1;

            UpdateStats();

            levelUp.Invoke(_currentLevel);

            if (_currentLevel >= MaxLevel)
            {
                _reachedMaxLevel = true;
                return;
            }
        }

        public void UpdateStats()
        {
            if (_levels.Count < 1) return;
            if (_currentLevel - 1 < 0) return;

            TreeLevel level = _levels[_currentLevel - 1];

            _rootEnergyCost = level.RootEnergyCost;
            _rootMaxDistance = level.RootMaxDistance;
            _rootSplitLimit += level.RootSplitBonus;
            _upgradeCost = level.UpgradeCost;
        }
    }
}