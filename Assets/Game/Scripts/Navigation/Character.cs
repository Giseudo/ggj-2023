using UnityEngine;

namespace Game.Navigation
{
    public class Character : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 2f;

        [SerializeField]
        private float _maxAcceleration = 5f;

        private Vector3 _velocity;
        private Vector3 _moveDirection;
        public Vector3 MoveDirection => _moveDirection;
        public Vector3 Velocity => _velocity;
        public float MaxSpeedChange => _maxAcceleration * Time.deltaTime;

        public void Awake()
        { }

        public void FixedUpdate()
        {
            AdjustVelocity();
        }

        public void AdjustVelocity()
        {
            Vector3 ProjectOnContactPlane(Vector3 vector)
            {
                Vector3 normal = Vector3.up;
                return vector - normal * Vector3.Dot(vector, normal);
            }

            Vector3 direction = _moveDirection * _speed;

            Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
            Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

            float currentX = Vector3.Dot(_velocity, xAxis);
            float currentZ = Vector3.Dot(_velocity, zAxis);

            float newX = Mathf.MoveTowards(currentX, direction.x, MaxSpeedChange);
            float newZ = Mathf.MoveTowards(currentZ, direction.z, MaxSpeedChange);

            _velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        }

        public void Move(Vector3 direction)
        {
            _moveDirection = direction;
        }
    }
}