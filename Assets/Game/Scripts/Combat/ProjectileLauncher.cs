using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Game.Combat
{
    public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField]
        GameObject _prefab;

        [SerializeField]
        public Vector3 _offset;

        private ObjectPool<GameObject> _pool;

        public void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                () => {
                    Vector3 position = transform.position + (transform.rotation * _offset);
                    GameObject instance = GameObject.Instantiate(_prefab, position, Quaternion.identity);

                    if (instance.TryGetComponent<Projectile>(out Projectile projectile))
                        projectile.died += OnProjetileDeath;

                    return instance;
                },
                (instance) => {
                    Vector3 position = transform.position + (transform.rotation * _offset);

                    instance.transform.position = position;
                    instance.transform.rotation = transform.rotation;

                    instance.gameObject.SetActive(true);
                },
                (instance) => {
                    instance.gameObject.SetActive(false);
                },
                (instance) => {
                    if (instance.TryGetComponent<Projectile>(out Projectile projectile))
                        projectile.died -= OnProjetileDeath;

                    Destroy(instance);
                },
                true,
                50
            );
        }

        private void OnProjetileDeath(Projectile projectile)
        {
            _pool.Release(projectile.gameObject);
        }

        public void LaunchProjectile()
        {
            _pool.Get();
        }
    }
}