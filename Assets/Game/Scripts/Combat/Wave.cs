using System;
using UnityEngine;

namespace Game.Combat
{
    [Serializable]
    public class Wave
    {
        [SerializeField]
        private CreepData _creepData;

        [SerializeField]
        private int _countLimit;

        [SerializeField]
        private float _spawnInterval;

        [SerializeField]
        private float _initialDelay;

        public CreepData CreepData => _creepData;
        public int CountLimit => _countLimit;
        public float SpawnInterval => _spawnInterval;
        public float InitialDelay => _initialDelay;
    }
}