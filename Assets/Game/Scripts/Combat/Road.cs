using UnityEngine;
using UnityEngine.Splines;

namespace Game.Combat
{
    [RequireComponent(typeof(Spline))]
    public class Road : MonoBehaviour
    {
        [SerializeField]
        private SplineContainer _spline;

        public float t = 0f;

        public void Awake()
        {
            TryGetComponent<SplineContainer>(out _spline);
        }
    }
}