using System;
using UnityEngine;
using Game.Core;

namespace Game.Combat
{
    public class Attacker : MonoBehaviour
    {
        [SerializeField]
        private int _meleeDamage;

        [SerializeField]
        private float _fovRadius;

        [SerializeField]
        private float _attackSpeed = 1;

        [SerializeField]
        private float _attackWaitTime;

        [SerializeField]
        private string _vfxEventName = "OnAttack";

        [SerializeField]
        private AudioClip _soundEffectClip;

        private float _lastAttackTime;
        private Damageable _currentTarget;

        public int MeleeDamage => _meleeDamage;
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

        public void Start()
        {
            GameManager.AddAttacker(this);
        }

        public void OnDestroy()
        {
            GameManager.RemoveAttacker(this);
        }

        public bool Attack(Damageable damageable)
        {
            if (_lastAttackTime + _attackWaitTime >= Time.time)
                return false;

            _lastAttackTime = Time.time;

            _currentTarget = damageable;
            attacked.Invoke(damageable);

            return true;
        }

        public void FinishAttack()
        {
            finishedAttack.Invoke();
        }

        public void DamageTarget()
        {
            if (_currentTarget == null) return;

            _currentTarget.Hurt(_meleeDamage);
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _fovRadius);
        }
    }
}