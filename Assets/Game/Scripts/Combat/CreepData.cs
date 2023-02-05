using UnityEngine;

namespace Game.Combat
{
    [CreateAssetMenu(fileName = "CreepData", menuName = "Game/Data/Creep Data")]
    public class CreepData : ScriptableObject
    {
        [SerializeField]
        private string _name = "Creep Name";

        [SerializeField]
        private GameObject _prefab;

        public string Name => _name;
        public GameObject Prefab => _prefab;
    }
}