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

        public void SetHealth(int value) => _health = value;
        public int Health => _health;

        public Action hurted = delegate { };
        public Action died = delegate { };

        public void Hurt(int damage)
        {
            _health -= damage;
            hurted.Invoke();

            if (_health > 0) return;

            died.Invoke();
        }
    }
}