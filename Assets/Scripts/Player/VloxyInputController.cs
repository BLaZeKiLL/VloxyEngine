using System;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyInputController : MonoBehaviour {
        
        private VloxyInput _Input;
        
        private VloxyCharacterController _CharacterController;
        private VloxyCameraController _CameraController;

        private void Awake() {
            _CharacterController = GetComponentInChildren<VloxyCharacterController>();
            _CameraController = GetComponentInChildren<VloxyCameraController>();
        }

        private void OnEnable() {
            _Input ??= new VloxyInput();

            _Input.Player.Enable();
            
            _Input.Player.Toggle.performed += ToggleOnPerformed;
            _Input.Player.Quit.performed += QuitOnPerformed;
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable() {
            _Input.Player.Toggle.performed -= ToggleOnPerformed;
            _Input.Player.Quit.performed -= QuitOnPerformed;

            _Input.Player.Disable();

            Cursor.lockState = CursorLockMode.None;
        }

        private void Update() {
            CharacterInput();
        }

        private void LateUpdate() {
            CameraInput();
        }

        private void ToggleOnPerformed(InputAction.CallbackContext obj) {
            _CharacterController.ToggleState();
        }

        private void QuitOnPerformed(InputAction.CallbackContext obj) {
            #if !UNITY_EDITOR
            SceneManager.LoadScene(0);
            #endif
        }
        
        private void CharacterInput() {
            if (!_Input.Player.enabled) return;
            
            var move = _Input.Player.Move.ReadValue<Vector2>();

            var input = new VloxyCharacterController.Input {
                Move = Vector3.ClampMagnitude(new Vector3(move.x, 0, move.y), 1f),
                Look = _CameraController.transform.rotation,
                JumpDown = _Input.Player.Jump.WasPressedThisFrame(),
                SprintDown = Math.Abs(_Input.Player.Sprint.ReadValue<float>() - 1f) < float.Epsilon,
                Altitude = _Input.Player.Altitude.ReadValue<float>()
            };

            _CharacterController.SetInput(ref input);
        }

        private void CameraInput() {
            if (!_Input.Player.enabled) return;

            var input = new VloxyCameraController.Input {
                Move = _CharacterController.transform.position,
                Look = _Input.Player.Look.ReadValue<Vector2>()
            };

            _CameraController.UpdateWithInput(Time.deltaTime, ref input);
        }

    }

}