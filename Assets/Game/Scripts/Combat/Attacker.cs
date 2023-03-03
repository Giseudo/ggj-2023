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
        private Vector3 _vfxOffset;

        [SerializeField]
        private AudioClip _attackSound;

        private float _lastAttackTime = float.MinValue;
        private Damageable _currentTarget;
        private bool _isAttacking;

        public int MeleeDamage => _meleeDamage;
        public float FovRadius => _fovRadius;
        public float AttackSpeed => _attackSpeed;
        public float AttackWaitTime => _attackWaitTime;
        public bool IsAttacking => _isAttacking;
        public Damageable CurrentTarget => _currentTarget;
        public Vector3 VFXOffset => _vfxOffset;

        public Action<Damageable> attacked = delegate { };
        public Action finishedAttack = delegate { };
        public Action<Attacker, string> playedVfx = delegate { };
        public Action<Attacker, AudioClip> playedSound = delegate { };

        public void PlayAttackVFX() => playedVfx.Invoke(this, _vfxEventName);
        public void PlayAttackSound()
        {
            SoundManager.PlaySound(_attackSound);
            playedSound.Invoke(this, _attackSound);
        }

        public void Start()
        {
            GameManager.AddAttacker(this);
        }

        public void OnDestroy()
        {
            GameManager.RemoveAttacker(this);
        }

        public bool Attack(Damageable damageable, bool ignoreWaitTime = false)
        {
            if (!ignoreWaitTime && _lastAttackTime + _attackWaitTime >= Time.time)
                return false;

            _lastAttackTime = Time.time;

            _currentTarget = damageable;
            _isAttacking = true;
            attacked.Invoke(damageable);

            return true;
        }

        public void FinishAttack()
        {
            if (!_isAttacking) return;

            finishedAttack.Invoke();

            _isAttacking = false;
        }

        public void DamageTarget()
        {
            if (_currentTarget == null) return;
            if (!_currentTarget.gameObject.activeInHierarchy) return;

            _currentTarget.Hurt(_meleeDamage);
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _fovRadius);
        }
    }
}