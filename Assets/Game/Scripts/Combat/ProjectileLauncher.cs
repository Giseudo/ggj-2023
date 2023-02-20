using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Combat
{
    public interface IProjectile
    {
        public GameObject GameObject { get; }
        public void SetTarget(Transform target);
        public Action<IProjectile> Died { get; set; }
        public Action<IProjectile, Damageable> Collided { get; set; }
        public void Fire();
    }

    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField]
        GameObject _prefab;

        [SerializeField]
        public Vector3 _offset;

        [SerializeField]
        private int _limit = 5;

        public Transform _target;
        private ObjectPool<IProjectile> _pool;

        public void SetTarget(Transform target) => _target = target;

        public ObjectPool<IProjectile> Pool => _pool;
        public Transform Target => _target;
        public int Limit => _limit;

        public Action<IProjectile> launched;
        public Action<IProjectile> destroyed;

        public void Awake()
        {
            _pool = new ObjectPool<IProjectile>(
                () => {
                    Vector3 position = transform.position + (transform.rotation * _offset);
                    GameObject instance = GameObject.Instantiate(_prefab, position, Quaternion.identity);

                    instance.TryGetComponent<IProjectile>(out IProjectile projectile);

                    projectile.Died += OnProjetileDeath;

                    return projectile;
                },
                (projectile) => {
                    Vector3 position = transform.position + (transform.rotation * _offset);

                    projectile.GameObject.transform.position = position;
                    projectile.GameObject.transform.rotation = transform.rotation;

                    projectile.GameObject.SetActive(true);
                    projectile.SetTarget(_target);
                    projectile.Fire();

                    if (!_target.TryGetComponent<Damageable>(out Damageable damageable))
                        return;

                    void OnTargetDeath(Damageable damageable)
                    {
                        projectile.SetTarget(null);
                        damageable.died -= OnTargetDeath;
                    }
 
                    damageable.died += OnTargetDeath;
                },
                (projectile) => {
                    projectile.GameObject.SetActive(false);
                },
                (projectile) => {
                    projectile.Died -= OnProjetileDeath;

                    Destroy(projectile.GameObject);
                },
                true,
                50
            );
        }

        private void OnProjetileDeath(IProjectile projectile)
        {
            destroyed.Invoke(projectile);

            _pool.Release(projectile);
        }

        public IProjectile LaunchProjectile()
        {
            IProjectile projectile = _pool.Get();

            launched.Invoke(projectile);

            return projectile;
        }
    }
}