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

                    return instance;
                },
                (instance) => {
                    instance.gameObject.SetActive(true);
                },
                (instance) => {
                    instance.gameObject.SetActive(false);
                },
                (instance) => {
                    Destroy(instance);
                },
                true,
                50
            );
        }

        public void LaunchProjectile()
        {
            _pool.Get();
        }
    }
}