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

        public string Name => _name;
        public Creep Prefab => _prefab;
    }
}