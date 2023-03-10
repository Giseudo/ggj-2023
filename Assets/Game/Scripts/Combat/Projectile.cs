using System;
using Freya;
using UnityEngine;
using UnityEngine.VFX;

namespace Game.Combat
{
    public class Projectile : MonoBehaviour, IProjectile
    {
        [SerializeField]
        private Vector3 _velocity = Vector3.forward;
        
        [SerializeField]
        private float _hitRadius = .5f;

        [SerializeField]
        private float _lifeTime = 5f;

        [SerializeField]
        private int _attackDamage = 1;

        [SerializeField]
        private VisualEffect _vfxGraph;

        private float _spawnTime;
        public Transform _followTarget;

        public Transform FollowTarget => _followTarget;
        public int AttackDamage => _attackDamage;
        public void SetAttackDamage (int damage) => _attackDamage = damage;
        public void SetTarget(Transform target) => _followTarget = target;

        public Action<IProjectile, Damageable> collided = delegate { };
        public Action<IProjectile, Damageable> Collided { get => collided; set => collided = value; }
        public Action<IProjectile> died = delegate { };
        public Action<IProjectile> Died { get => died; set => died = value; }
        public GameObject GameObject => gameObject;

        private bool _wasFired;

        public void OnEnable()
        { }

        public void OnDisable()
        {
            _wasFired = false;
        }

        public void Fire()
        {
            _spawnTime = Time.time;
            _wasFired = true;
        }

        public void Update()
        {
            if (!_wasFired) return;

            if (_spawnTime + _lifeTime < Time.time)
                Die();

            if (_spawnTime + _lifeTime - 2.5f < Time.time && _vfxGraph != null)
                _vfxGraph?.Stop();

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