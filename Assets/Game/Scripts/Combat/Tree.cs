using UnityEngine;

namespace Game.Combat
{
    public class Tree : MonoBehaviour
    {
        [SerializeField]
        private float _rootMaxDistance = 20f;

        public float RootMaxDistance => _rootMaxDistance;
    }
}