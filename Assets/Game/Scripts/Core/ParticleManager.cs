using UnityEngine;
using UnityEngine.VFX;
using Game.Combat;

namespace Game.Core
{
    public class ParticleManager : MonoBehaviour
    {
        [SerializeField]
        private VisualEffect _damageHitEffect;

        private VFXEventAttribute _eventAttribute;

        public static ParticleManager Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            _eventAttribute = _damageHitEffect.CreateVFXEventAttribute();

            GameManager.DamageableAdded += OnDamageableAdd;
            GameManager.DamageableRemoved += OnDamageableRemove;
        }

        public void OnDestroy()
        {
            GameManager.DamageableAdded -= OnDamageableAdd;
            GameManager.DamageableRemoved -= OnDamageableRemove;
        }

        public void TriggerParticle(Damageable damageable)
        {
            _damageHitEffect.SetVector3("HitPoint", damageable.transform.position);
            _eventAttribute.SetVector3("HitPoint", damageable.transform.position);
            _damageHitEffect.SendEvent("OnHit", _eventAttribute);
        }

        public void OnDamageableAdd(Damageable damageable)
        {
            damageable.hurted += TriggerParticle;
        }

        public void OnDamageableRemove(Damageable damageable)
        {
            damageable.hurted += TriggerParticle;
        }
    }
}