using UnityEngine;
using UnityEngine.Splines;

namespace Game.Combat
{
    public class Creep : MonoBehaviour
    {
        [SerializeField]
        private CreepData _data;

        [SerializeField]
        private float _maxSpeed;

        public CreepData Data => _data;
        public float MaxSpeed => _maxSpeed;
        public float SpeedMultiplier => _speedMultiplier;
        public SplineContainer Spline => _spline;

        private bool _isSlowedDown;
        private float _lastSlowDownTime;
        private float _slowDownDuration;
        private SplineContainer _spline;

        private float _initialSpeed;
        private float _speedMultiplier = 1f;
        private bool _isMoving = false;
        public float t = 0f;

        public void SetSpline(SplineContainer spline) => _spline = spline;

        public void Awake()
        {
            _initialSpeed = _maxSpeed;
        }

        public void SlowDown(float speedMultipler, float duration)
        {
            _speedMultiplier = speedMultipler;
            _lastSlowDownTime = Time.time;
            _slowDownDuration = duration;
            _isSlowedDown = true;
        }

        public void Update()
        {
            if (!_isSlowedDown) return;

            if (_lastSlowDownTime + _slowDownDuration > Time.time) return;

            _isSlowedDown = false;
            _maxSpeed = _initialSpeed;
            _speedMultiplier = 1f;
        }

        public void FixedUpdate()
        {
            if (_spline == null) return;
            if (!_isMoving) return;

            t += MaxSpeed * SpeedMultiplier * Time.deltaTime * Time.deltaTime;

            Vector3 position = Spline.EvaluatePosition(t);
            Vector3 tangent = Spline.EvaluateTangent(t);

            transform.LookAt(transform.position + tangent);
            transform.position = position;
        }

        public void Move()
        {
            _isMoving = true;
        }

        public void Stop()
        {
            _isMoving = false;
        }
    }
}