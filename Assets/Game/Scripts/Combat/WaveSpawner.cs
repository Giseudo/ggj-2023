using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Game.Core;

namespace Game.Combat
{
    [Serializable]
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField]
        private List<Round> _rounds = new List<Round>();

        [SerializeField]
        private SplineContainer _spline;

        [SerializeField]
        private CreepSpawner _creepSpawnerPrefab;

        private int _currentRound = 0;

        private Dictionary<CreepData, CreepSpawner> _spawners = new Dictionary<CreepData, CreepSpawner>();

        public CreepSpawner CreepSpawnerPrefab => _creepSpawnerPrefab;
        public Dictionary<CreepData, CreepSpawner> Spawners => _spawners;
        public SplineContainer Spline => _spline;
        public List<Round> Rounds => _rounds;

        public Round CurrentRound => _rounds[_currentRound];

        public Action finished = delegate { };
        public Action roundOver = delegate { };
        public Action<Creep> creepDied = delegate { };

        public void OnCreepDeath(Creep creep) => creepDied.Invoke(creep);

        public void Start()
        {
            MatchManager.AddWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.creepDied += OnCreepDeath;
                round.over += OnRoundOver;

                round.Awake(this);
            }
        }

        public void OnDestroy()
        {
            MatchManager.RemoveWaveSpawner(this);

            for (int i = 0; i < _rounds.Count; i++)
            {
                Round round = _rounds[i];

                round.creepDied -= OnCreepDeath;
                round.over -= OnRoundOver;

                round.Destroy();
            }
        }

        public void StartRound()
        {
            Round round = _rounds[_currentRound];

            round?.Start();
        }

        public void NextRound(int roundNumber)
        {
            if (roundNumber > _rounds.Count - 1)
                return;

            Round round = _rounds[roundNumber];
            round?.Start();
        }

        public void OnRoundOver()
        {
            if (_currentRound >= _rounds.Count - 1)
            {
                finished.Invoke();
                return;
            }

            _currentRound++;

            roundOver.Invoke();
        }
    }
}