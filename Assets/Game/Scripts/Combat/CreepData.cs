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

        [SerializeField]
        public float _walkSpeed = 3f;

        public string Name => _name;
        public GameObject Prefab => _prefab;
        public float WalkSpeed => _walkSpeed;
    }
}