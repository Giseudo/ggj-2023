using System;
using UnityEngine;
using Freya;

namespace Game.Combat
{
    [ExecuteInEditMode]
    public class BalisticProjectile : MonoBehaviour, IProjectile
    {
        [SerializeField]
        private Transform _target;

        [SerializeField]
        private float _height;

        [SerializeField]
        [Range(0, 1)]
        private float t;

        public void SetTarget(Transform target) => _target = target;
        public Action<IProjectile, Damageable> collided = delegate { };
        public Action<IProjectile, Damageable> Collided { get => collided; set => collided = value; }
        public Action<IProjectile> died = delegate { };
        public Action<IProjectile> Died { get => died; set => died = value; }
        public GameObject GameObject => gameObject;

        public void OnDrawGizmosSelected()
        {
            float distance = (_target.position - transform.position).magnitude;
            float speed = distance / t;
            float time = distance / speed;

            Vector3 p = Vector3.Lerp(transform.position, _target.position, time);
            Vector3 h = Vector3.Lerp(Vector3.zero, Vector3.up * _height, Mathf.Pow(((Mathf.Cos((time + 0.5f) * Mathfs.TAU)) + 1f) * 0.5f, 0.3f));

            Gizmos.DrawSphere(p + h, 1f);
        }
    }
}