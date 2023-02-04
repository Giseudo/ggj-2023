using UnityEngine;

namespace Game.VFX
{
    [RequireComponent(typeof(Light))]
    [ExecuteInEditMode]
    public class FlickeringLight : MonoBehaviour
    {
        [SerializeField]
        private float _initialIntensity = 10f;

        [SerializeField]
        private float _speed = 20f;

        [SerializeField]
        private float _multiplier = .2f;

        [SerializeField]
        private Light _light;

        private float _intensity;

        void Start()
        {
            if (_light == null) TryGetComponent<Light>(out _light);
            if (_light == null) return;
        }

        void Update()
        {
            if (_light == null) return;

            _intensity += Time.deltaTime * _speed;

            float deltaIntensity = Mathf.Sin(_intensity) * _multiplier;

            _light.intensity = _initialIntensity;
            _light.intensity += deltaIntensity;
            _light.intensity += deltaIntensity * Mathf.Sin(_intensity * 5f) * 0.5f;
        }
    }
}