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

        [SerializeField]
        private bool _ignoreSpeedChange = false;

        public CreepData Data => _data;
        public float MaxSpeed => _maxSpeed;
        public float SpeedMultiplier => _speedMultiplier;
        public SplineContainer Spline => _spline;
        public float Displacement => _displacement;
        public float CurveLength => _curveLength;
        public bool IsMoving => _isMoving;
        public int EnergyDropAmount => _energyDropAmount;
        public int TotalCreepsToSpawn => _totalCreepsToSpawn;

        private bool _isSlowedDown;
        private float _lastSlowDownTime;
        private float _slowDownDuration;
        private SplineContainer _spline;
        private float _initialSpeed;
        private float _speedMultiplier = 1f;
        private bool _isMoving = false;
        private float _displacement = 0f;
        private float _curveLength;
        private int _totalCreepsToSpawn;

        public void SetDisplacement(float value) => _displacement = value;

        public void SetSpline(SplineContainer spline, float displacement = 0f)
        {
            _spline = spline;
            _curveLength = spline.Splines[0].GetLength();
            _displacement = displacement;
        }

        public void Awake()
        {
            _initialSpeed = _maxSpeed;

            CalculateTotalCreepsToSpawn();
        }

        private void CalculateTotalCreepsToSpawn()
        {
            CreepData currentData = _data;
            int spawnCount = 0;
            int chainLimit = 0;
            int childMultiplier = 1;

            while (currentData != null && chainLimit < 10)
            {
                spawnCount += currentData.DeathSpawnCount * childMultiplier;
                chainLimit += 1;

                childMultiplier = currentData.DeathSpawnCount;
                currentData = currentData.CreepDeathSpawn;
            }

            _totalCreepsToSpawn = spawnCount;
        }

        public void SlowDown(float speedMultipler, float duration)
        {
            if (_ignoreSpeedChange && speedMultipler > 1f) return;

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