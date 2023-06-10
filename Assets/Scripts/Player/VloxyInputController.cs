using System;

using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyInputController : MonoBehaviour {

        private VloxyInput _Input;
        private VloxyCharacterController _Controller;

        private void Awake() {
            _Controller = GetComponentInChildren<VloxyCharacterController>();
        }

        private void OnEnable() {
            _Input ??= new VloxyInput();

            _Input.Player.Enable();
        }

        private void OnDisable() {
            _Input?.Player.Disable();
        }

        private void Update() {
            Input();
        }

        private void Input() {
            if (!_Input.Player.enabled) return;
            
            var move = _Input.Player.Move.ReadValue<Vector2>();
            var look = _Input.Player.Look.ReadValue<Vector2>();

            var input = new VloxyCharacterController.Input {
                Move = Vector3.ClampMagnitude(new Vector3(move.x, 0, move.y), 1f),
                Look = Quaternion.Euler(Vector3.up * look.x * 2)
            };
            
            _Controller.SetInput(ref input);
        }

    }

}