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

        [SerializeField]
        private int _energyDropAmount = 50;

        public CreepData Data => _data;
        public float MaxSpeed => _maxSpeed;
        public float SpeedMultiplier => _speedMultiplier;
        public SplineContainer Spline => _spline;
        public float Displacement => _displacement;
        public float CurveLength => _curveLength;
        public bool IsMoving => _isMoving;
        public int EnergyDropAmount => _energyDropAmount;

        private bool _isSlowedDown;
        private float _lastSlowDownTime;
        private float _slowDownDuration;
        private SplineContainer _spline;

        private float _initialSpeed;
        private float _speedMultiplier = 1f;
        private bool _isMoving = false;
        private float _displacement = 0f;
        private float _curveLength;

        public void SetSpline(SplineContainer spline)
        {
            _spline = spline;
            _curveLength = spline.Splines[0].GetLength();
            _displacement = 0f;
        }

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
            UpdatePosition();

            if (!_isSlowedDown) return;

            if (_lastSlowDownTime + _slowDownDuration > Time.time) return;

            _isSlowedDown = false;
            _maxSpeed = _initialSpeed;
            _speedMultiplier = 1f;
        }

        public void UpdatePosition()
        {
            if (_spline == null) return;
            if (!_isMoving) return;

            _displacement += (MaxSpeed * SpeedMultiplier) * Time.deltaTime;
            float t = Mathf.InverseLerp(0f, _curveLength, _displacement);

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