using System;
using CodeBlaze.Vloxy.Demo.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyInputController : MonoBehaviour {
        
        private VloxyInput.PlayerActions _PlayerMap;

        private VloxyCharacterInteractions _CharacterInteractions;
        private VloxyCharacterController _CharacterController;
        private VloxyCameraController _CameraController;
        
        private void Awake() {
            _CharacterInteractions = GetComponent<VloxyCharacterInteractions>();
            
            _CharacterController = GetComponentInChildren<VloxyCharacterController>();
            _CameraController = GetComponentInChildren<VloxyCameraController>();
        }
        
        private void Update() {
            CharacterInput();
        }

        private void LateUpdate() {
            CameraInput();
        }

        private void OnEnable() {
            _PlayerMap = GameManager.Current.InputMaps.Player;
            
            _PlayerMap.Enable();
            
            _PlayerMap.Toggle.performed += ToggleOnPerformed;
            _PlayerMap.Quit.performed += QuitOnPerformed;
            _PlayerMap.Fire.performed += FireOnPerformed;
            
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable() {
            _PlayerMap.Toggle.performed -= ToggleOnPerformed;
            _PlayerMap.Quit.performed -= QuitOnPerformed;
            _PlayerMap.Fire.performed -= FireOnPerformed;

            _PlayerMap.Disable();

            Cursor.lockState = CursorLockMode.None;
        }

        private void ToggleOnPerformed(InputAction.CallbackContext obj) {
            _CharacterController.ToggleState();
        }

        private void QuitOnPerformed(InputAction.CallbackContext obj) {
#if !UNITY_EDITOR
            SceneManager.LoadScene(0);
#endif
        }
        
        private void FireOnPerformed(InputAction.CallbackContext obj) {
            _CharacterInteractions.Fire();
        }
        
        private void CharacterInput() {
            if (!_PlayerMap.enabled) return;
            
            var move = _PlayerMap.Move.ReadValue<Vector2>();

            var input = new VloxyCharacterController.Input {
                Move = Vector3.ClampMagnitude(new Vector3(move.x, 0, move.y), 1f),
                Look = _CameraController.transform.rotation,
                JumpDown = _PlayerMap.Jump.WasPressedThisFrame(),
                SprintDown = Math.Abs(_PlayerMap.Sprint.ReadValue<float>() - 1f) < float.Epsilon,
                Altitude = _PlayerMap.Altitude.ReadValue<float>()
            };

            _CharacterController.SetInput(ref input);
        }

        private void CameraInput() {
            if (!_PlayerMap.enabled) return;

            var input = new VloxyCameraController.Input {
                Move = _CharacterController.transform.position,
                Look = _PlayerMap.Look.ReadValue<Vector2>()
            };

            _CameraController.UpdateWithInput(Time.deltaTime, ref input);
        }

    }

}