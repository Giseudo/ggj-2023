using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class Creep : MonoBehaviour
    {
        [SerializeField]
        private CreepData _data;

        public CreepData Data => _data;
        public float MaxSpeed => _maxSpeed;
        public float SpeedMultiplier => _speedMultiplier;

        private bool _isSlowedDown;
        private float _lastSlowDownTime;
        private float _slowDownDuration;

        [SerializeField]
        private float _maxSpeed;

        private float _initialSpeed;
        private float _speedMultiplier = 1f;

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
    }
}