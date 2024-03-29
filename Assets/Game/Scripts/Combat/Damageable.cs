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
        private int _hitsToStun = 1;

        [SerializeField]
        private float _hurtTime = 1f;

        [SerializeField]
        private AudioClip _hitSound;

        [SerializeField]
        private AudioClip _deathSound;

        private int _hitCount = 0;
        private int _maxHealth;
        private bool _isDead = false;
        private CapsuleCollider _collider;

        public float HurtTime => _hurtTime;
        public int Health => _health;
        public int MaxHealth => _maxHealth;
        public bool IsDead => _isDead;

        public Action<Damageable> hurted = delegate { };
        public Action<Damageable, int> healthChanged = delegate { };
        public Action<Damageable> died = delegate { };

        public void Awake()
        {
            _maxHealth = _health;

            TryGetComponent<CapsuleCollider>(out _collider);
        }

        public void OnDisable()
        {
            Revive();
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
            _hitCount += 1;

            if (_hitCount >= _hitsToStun)
            {
                hurted.Invoke(this);
                _hitCount = 0;
            }

            SoundManager.PlaySound(_hitSound);

            if (_health < 0) _health = 0;
            if (_health > 0) return;

            Die();
        }

        public void Die()
        {
            if (_isDead) return;

            died.Invoke(this);

            _isDead = true;

            SoundManager.PlaySound(_deathSound);

            if (!_collider) return;

            _collider.enabled = false;
        }

        public void SetHealth(int value)
        {
            _health = value;
            healthChanged.Invoke(this, _health);
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