using System;
using UnityEngine;

namespace Game.Combat
{
    public class Attacker : MonoBehaviour
    {
        [SerializeField]
        private int _attackDamage;

        [SerializeField]
        private float _fovRadius;

        [SerializeField]
        private float _attackSpeed = 1;

        private Damageable _currentTarget;

        public int AttackDamage => _attackDamage;
        public float FovRadius => _fovRadius;
        public float AttackSpeed => _attackSpeed;

        public Action<Damageable> attacked = delegate { };
        public Action finishedAttack = delegate { };

        public void Attack(Damageable damageable)
        {
            _currentTarget = damageable;
            attacked.Invoke(damageable);
        }

        public void FinishAttack()
        {
            _currentTarget.Hurt(_attackDamage);
            _currentTarget = null;

            finishedAttack.Invoke();
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _fovRadius);
        }
    }
}