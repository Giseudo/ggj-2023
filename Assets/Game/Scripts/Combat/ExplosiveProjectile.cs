using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using Freya;

namespace Game.Combat
{
    public class ExplosiveProjectile : MonoBehaviour, IProjectile
    {
        [SerializeField]
        private float _lifeTime = 5f;

        [SerializeField]
        private float _hitRadius = 10f;

        [SerializeField]
        private int _attackDamage = 1;

        [SerializeField]
        private LayerMask _enemyLayer;

        public GameObject GameObject => gameObject;
        public Action<IProjectile, Damageable> collided = delegate { };
        public Action<IProjectile, Damageable> Collided { get => collided; set => collided = value; }
        public Action<IProjectile> died = delegate { };
        public Action<IProjectile> Died { get => died; set => died = value; }

        public float LifeTime => _lifeTime;
        public float HitRadius => _hitRadius;

        public void Fire()
        {
            StartCoroutine(Explode());
        }

        public void SetTarget(Transform target)
        {
            if (target == null) return;

            transform.position = target.position;
        }

        private IEnumerator Explode()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _hitRadius, _enemyLayer);

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (!collider.TryGetComponent<Damageable>(out Damageable damageable))
                    continue;

                damageable.Hurt(_attackDamage);

                Collided.Invoke(this, damageable);
            }

            yield return new WaitForSeconds(_lifeTime);

            Died.Invoke(this);
        }
    }
}
