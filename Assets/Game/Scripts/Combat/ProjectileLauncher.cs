using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Combat
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField]
        Projectile _prefab;

        [SerializeField]
        public Vector3 _offset;

        public Transform _followTarget;

        public void SetFollowTarget(Transform target) => _followTarget = target;

        public Transform FollowTarget => _followTarget;

        private ObjectPool<Projectile> _pool;

        public void Awake()
        {
            _pool = new ObjectPool<Projectile>(
                () => {
                    Vector3 position = transform.position + (transform.rotation * _offset);
                    Projectile projectile = GameObject.Instantiate<Projectile>(_prefab, position, Quaternion.identity);

                    projectile.died += OnProjetileDeath;

                    return projectile;
                },
                (projectile) => {
                    Vector3 position = transform.position + (transform.rotation * _offset);

                    projectile.transform.position = position;
                    projectile.transform.rotation = transform.rotation;

                    projectile.gameObject.SetActive(true);
                    projectile.SetFollowTarget(_followTarget);
                },
                (projectile) => {
                    projectile.gameObject.SetActive(false);
                },
                (projectile) => {
                    projectile.died -= OnProjetileDeath;

                    Destroy(projectile);
                },
                true,
                50
            );
        }

        private void OnProjetileDeath(Projectile projectile)
        {
            _pool.Release(projectile);
        }

        public Projectile LaunchProjectile()
        {
            return _pool.Get();
        }
    }
}