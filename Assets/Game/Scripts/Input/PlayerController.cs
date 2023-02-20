using UnityEngine;
using Game.Navigation;
using System;

namespace Game.Input
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private Character _character;

        [SerializeField]
        private Vector3 _offset;

        private Vector3 _moveDirection;
        private Vector3 _lookDirection;

        public void OnEnable()
        {
            _inputReader.moved += OnMove;
            _inputReader.looked += OnLook;
        }

        public void OnDisable()
        {
            _inputReader.moved -= OnMove;
            _inputReader.looked -= OnLook;
        }

        private void OnMove(Vector2 direction)
        {
            _moveDirection = new Vector3(direction.x, 0f, direction.y);
        }

        private void OnLook(Vector2 direction)
        {
            // _character.Rotate(new Vector3(0f, direction.x, 0f));

            _lookDirection += new Vector3(direction.y, direction.x, 0f);
        }

        public void Update()
        {
            _character.Move(transform.rotation * _moveDirection);
            transform.position = _character.transform.position + _character.transform.rotation * _offset;
        }

        public void FixedUpdate()
        {
            transform.rotation = Quaternion.Euler(_lookDirection);
        }
    }
}