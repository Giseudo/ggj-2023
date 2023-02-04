using System;
using UnityEngine;
using Game.Core;

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

        [SerializeField]
        private string _vfxEventName = "OnAttack";

        [SerializeField]
        private AudioClip _soundEffectClip;

        [SerializeField]
        private float _attackWaitTime = 0f;

        private Damageable _currentTarget;

        public int AttackDamage => _attackDamage;
        public float FovRadius => _fovRadius;
        public float AttackSpeed => _attackSpeed;
        public float AttackWaitTime => _attackWaitTime;
        public Damageable CurrentTarget => _currentTarget;

        public Action<Damageable> attacked = delegate { };
        public Action finishedAttack = delegate { };
        public Action<Attacker, string> playedVfx = delegate { };
        public Action<Attacker, AudioClip> playedSound = delegate { };

        public void PlayAttackVFX() => playedVfx.Invoke(this, _vfxEventName);
        public void PlayAttackSound() => playedSound.Invoke(this, _soundEffectClip);

        public void OnEnable()
        {
            GameManager.AddAttacker(this);
        }

        public void OnDisable()
        {
            GameManager.RemoveAttacker(this);
        }

        public void Attack(Damageable damageable)
        {
            _currentTarget = damageable;
            attacked.Invoke(damageable);
        }

        public void FinishAttack()
        {
            finishedAttack.Invoke();

            if (!_currentTarget) return;

            _currentTarget.Hurt(_attackDamage);
            _currentTarget = null;
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _fovRadius);
        }
    }
}