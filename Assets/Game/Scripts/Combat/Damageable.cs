using System;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField]
        private int _health = 10;

        [SerializeField]
        private float _hurtTime = 1f;

        private int _maxHealth = 10;
        private bool _isDead = false;
        private float _lastHurtTime;
        private CapsuleCollider _collider;

        public float HurtTime => _hurtTime;
        public float LastHurtTime => _lastHurtTime;
        public void SetHealth(int value) => _health = value;
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsDead => _isDead;

        public Action<Damageable> hurted = delegate { };
        public Action<Damageable> died = delegate { };

        public void Awake()
        {
            _maxHealth = _health;

            TryGetComponent<CapsuleCollider>(out _collider);
        }

        public void Start()
        {
            GameManager.AddDamageable(this);
        }

        public void OnDestroy()
        {
            GameManager.RemoveDamageable(this);
        }

        public void Hurt(int damage)
        {
            if (_isDead) return;

            _health -= damage;
            _lastHurtTime = Time.time;

            hurted.Invoke(this);

            if (_health < 0) _health = 0;
            if (_health > 0) return;

            Die();
        }

        public void Die()
        {
            if (_isDead) return;

            died.Invoke(this);

            _isDead = true;

            if (!_collider) return;

            _collider.enabled = false;
        }

        public void Revive()
        {
            _health = _maxHealth;
            _isDead = false;

            if (!_collider) return;

            _collider.enabled = true;
        }
    }
}