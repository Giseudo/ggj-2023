using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat
{
    public class Damageable : MonoBehaviour
    {
        [SerializeField]
        private int _health = 10;

        private bool _isDead = false;

        public void SetHealth(int value) => _health = value;
        public int Health => _health;
        public bool IsDead => _isDead;

        public Action hurted = delegate { };
        public Action<Damageable> died = delegate { };


        public void Hurt(int damage)
        {
            if (_isDead) return;

            _health -= damage;

            hurted.Invoke();

            if (_health < 0) _health = 0;
            if (_health > 0) return;

            Die();
        }

        public void Die()
        {
            if (_isDead) return;

            died.Invoke(this);
        }
    }
}