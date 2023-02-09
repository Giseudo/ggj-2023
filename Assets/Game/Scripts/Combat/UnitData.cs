using UnityEngine;

namespace Game.Combat
{
    public enum UnitType
    {
        Melee,
        Ranged,
    }

    [CreateAssetMenu(fileName = "UnitData", menuName = "Game/Data/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [SerializeField]
        private string _name = "Unit Name";

        [SerializeField]
        private UnitType _type = UnitType.Melee;

        [SerializeField]
        private GameObject _prefab;

        [SerializeField]
        private Sprite _thumbnail;

        [SerializeField]
        public float _rangeRadius = 20f;

        [SerializeField]
        public int _requiredEnergy = 200;

        public string Name => _name;
        public UnitType Type => _type;
        public GameObject Prefab => _prefab;
        public Sprite Thumbnail => _thumbnail;
        public float RangeRadius => _rangeRadius;
        public int RequiredEnergy => _requiredEnergy;
    }
}