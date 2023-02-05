using System;
using Freya;
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

        private float _spawnTime;
        public Transform _followTarget;

        public Transform FollowTarget => _followTarget;
        public int AttackDamage => _attackDamage;
        public void SetAttackDamage (int damage) => _attackDamage = damage;
        public void SetFollowTarget(Transform target) => _followTarget = target;

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

            if (_followTarget != null)
            {
                if (!_followTarget.gameObject.activeInHierarchy)
                {
                    _followTarget = null;
                    return;
                }

                Vector3 desiredPosition = _followTarget.position;
                desiredPosition.y = transform.position.y;
                transform.LookAt(desiredPosition);
            }

            transform.position += transform.rotation * _velocity * Time.deltaTime;

            Collider[] colliders = Physics.OverlapSphere(transform.position, _hitRadius, 1 << LayerMask.NameToLayer("Creep"));

            if (colliders.Length == 0) return;

            Collider collider = colliders[0];

            Die();

            if (!collider.TryGetComponent<Damageable>(out Damageable damageable)) return;

            damageable.Hurt(_attackDamage);
            collided.Invoke(this, damageable);
        }

        public void Die()
        {
            _followTarget = null;
            gameObject.SetActive(false);
            died.Invoke(this);
        }
    }
}