using System;
using UnityEngine;

namespace Game.Combat
{
    public class Attacker : MonoBehaviour
    {
        [SerializeField]
        private int _attackDamage;
        private float _fovRadius;

        public int AttackDamage => _attackDamage;
        public float FovRadius => _fovRadius;

        public Action<Damageable> attacked = delegate { };
        public Action finishedAttack = delegate { };

        public void Attack(Damageable damageable)
        {
            attacked.Invoke(damageable);
        }

        public void FinishAttack()
        {
            finishedAttack.Invoke();
        }
    }
}