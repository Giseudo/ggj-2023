using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Combat;

namespace Game.Core
{
    [Serializable]
    public class Wave
    {
        [SerializeField]
        public CreepData _creep;

        [SerializeField]
        public int _count;

        [SerializeField]
        public float _spawnInterval;

        [SerializeField]
        public float _initialDelay;
    }

    [Serializable]
    public class WaveSpawner
    {
        private int _activeWave;

        [SerializeField]
        private List<Wave> _waves = new List<Wave>();

        public int ActiveWave => _activeWave;

        public Wave CurrentWave => _waves[_activeWave - 1];

        public void Start()
        {

        }

        public void Destroy()
        {

        }

        public void Update()
        {

        }
    }
}