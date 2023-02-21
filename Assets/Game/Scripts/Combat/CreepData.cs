using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "CreepData", menuName = "Game/Data/Creep Data")]
    public class CreepData : ScriptableObject
    {
        [SerializeField]
        private string _name = "Creep Name";

        [SerializeField]
        private Creep _prefab;

        [SerializeField]
        private CreepData _creepDeathSpawn;

        [SerializeField]
        private int _deathSpawnCount = 0;

        public string Name => _name;
        public Creep Prefab => _prefab;
        public CreepData CreepDeathSpawn => _creepDeathSpawn;
        public int DeathSpawnCount => _deathSpawnCount;
    }
}