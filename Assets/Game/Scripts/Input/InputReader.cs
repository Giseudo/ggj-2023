using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input/Input Reader")]
    public class InputReader : ScriptableObject, GameInput.IGameplayActions
    {
        public Action<Vector2> moved = delegate { };
        public Action<Vector2> looked = delegate { };
        public Action paused = delegate { };
        public Action played = delegate { };
        public Action canceled = delegate { };
        public Action<float> changedTime = delegate { };
        public Action toggledPlay = delegate { };
        public Action fastForwarded = delegate { };

        private GameInput _gameInput;

        private void OnEnable()
        {
            if (_gameInput == null)
            {
                _gameInput = new GameInput();
                _gameInput.Gameplay.SetCallbacks(this);
            }

            EnableGameplayInput();
        }

        private void OnDisable()
        {
            DisableAllInput();
        }

        public void EnableGameplayInput()
        {
            _gameInput.Gameplay.Enable();
        }

        public void DisableAllInput()
        {
            _gameInput.Gameplay.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moved.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            looked.Invoke(context.ReadValue<Vector2>());
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                paused.Invoke();
        }

        public void OnPlay(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                played.Invoke();
        }

        public void OnFastForward(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                fastForwarded.Invoke();
        }

        public void OnToggle(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                toggledPlay.Invoke();
        }

        public void OnTime(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                changedTime.Invoke(context.ReadValue<float>());
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Performed)
                canceled.Invoke();
        }
    }
}