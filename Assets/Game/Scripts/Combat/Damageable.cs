using System;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField]
        private int _health = 10;

        private int _maxHealth = 10;
        private bool _isDead = false;

        public void SetHealth(int value) => _health = value;
        public int Health => _health;
        public bool IsDead => _isDead;

        public Action<Damageable> hurted = delegate { };
        public Action<Damageable> died = delegate { };

        public void Awake()
        {
            _maxHealth = _health;
        }

        public void OnEnable()
        {
            GameManager.AddDamageable(this);
        }

        public void OnDisable()
        {
            GameManager.RemoveDamageable(this);
        }

        public void Hurt(int damage)
        {
            if (_isDead) return;

            _health -= damage;

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
        }

        public void Revive()
        {
            _health = _maxHealth;
            _isDead = false;
        }
    }
}