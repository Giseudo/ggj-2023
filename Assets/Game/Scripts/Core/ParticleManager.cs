using UnityEngine;
using UnityEngine.VFX;
using Game.Combat;

namespace Game.Core
{
    public class ParticleManager : MonoBehaviour
    {
        [SerializeField]
        private VisualEffect _damageHitEffect;

        [SerializeField]
        private VisualEffect _attackEffect;

        private VFXEventAttribute _damageEventAttribute;
        private VFXEventAttribute _attackEventAttribute;

        public static ParticleManager Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            _damageEventAttribute = _damageHitEffect.CreateVFXEventAttribute();
            _attackEventAttribute = _attackEffect.CreateVFXEventAttribute();
        }

        public void Start()
        {
            GameManager.DamageableAdded += OnDamageableAdd;
            GameManager.DamageableRemoved += OnDamageableRemove;

            GameManager.AttackerAdded += OnAttackerAdd;
            GameManager.AttackerRemoved += OnAttackerRemove;
        }

        public void OnDestroy()
        {
            GameManager.DamageableAdded -= OnDamageableAdd;
            GameManager.DamageableRemoved -= OnDamageableRemove;

            GameManager.AttackerAdded -= OnAttackerAdd;
            GameManager.AttackerRemoved -= OnAttackerRemove;
        }

        public void PlayHitParticles(Damageable damageable)
        {
            _damageHitEffect.SetVector3("HitPoint", damageable.transform.position);
            _damageEventAttribute.SetVector3("HitPoint", damageable.transform.position);

            _damageHitEffect.SendEvent("OnHit", _damageEventAttribute);
        }

        public void PlayAttackParticles(Attacker attacker, string eventName)
        {
            _attackEffect.SetVector3("AttackerPosition", attacker.transform.position);
            _attackEventAttribute.SetVector3("AttackerProsition", attacker.transform.position);

            _attackEffect.SetVector3("TargetPosition", attacker.CurrentTarget.transform.position);
            _attackEventAttribute.SetVector3("TargetPosition", attacker.CurrentTarget.transform.position);

            _attackEffect.SendEvent(eventName, _attackEventAttribute);
        }

        public void OnDamageableAdd(Damageable damageable)
        {
            damageable.hurted += PlayHitParticles;
        }

        public void OnDamageableRemove(Damageable damageable)
        {
            damageable.hurted -= PlayHitParticles;
        }

        public void OnAttackerAdd(Attacker attacker)
        {
            attacker.playedVfx += PlayAttackParticles;
        }

        public void OnAttackerRemove(Attacker attacker)
        {
            attacker.playedVfx -= PlayAttackParticles;
        }
    }
}