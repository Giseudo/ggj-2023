using System;
using UnityEngine;

namespace Game.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _velocity = Vector3.forward;
        
        [SerializeField]
        private float _hitRadius = .5f;

        [SerializeField]
        private float _lifeTime = 5f;

        [SerializeField]
        private int _attackDamage = 1;

        public int AttackDamage => _attackDamage;

        public void SetAttackDamage (int damage) => _attackDamage = damage;

        private float _spawnTime;

        public Action<Projectile, Damageable> collided = delegate { };
        public Action<Projectile> died = delegate { };

        public void OnEnable()
        {
            _spawnTime = Time.time;
        }

        public void OnDisable()
        { }

        public void Update()
        {
            if (_spawnTime + _lifeTime < Time.time)
                Die();

            transform.position += transform.rotation * _velocity * Time.deltaTime;

            Collider[] colliders = Physics.OverlapSphere(transform.position, _hitRadius, 1 << LayerMask.NameToLayer("Creep"));

            if (colliders.Length == 0) return;

            Collider collider = colliders[0];

            if (!collider.TryGetComponent<Damageable>(out Damageable damageable)) return;

            damageable.Hurt(_attackDamage);
            collided.Invoke(this, damageable);

            Die();
        }

        public void Die()
        {
            gameObject.SetActive(false);
            died.Invoke(this);
        }
    }
}