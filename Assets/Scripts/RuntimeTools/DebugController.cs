using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlaze.Vloxy.Demo.Debug {

    public class DebugController : MonoBehaviour {

        [SerializeField] private RectTransform Vloxy;
        [SerializeField] private RectTransform Console;

        private VloxyInput _Input;

        private void OnEnable() {
            _Input ??= new VloxyInput();
            
            _Input.Debug.Enable();
            
            _Input.Debug.Stats.performed += StatsOnPerformed;
            _Input.Debug.Console.performed += ConsoleOnPerformed;
        }

        private void OnDisable() {
            _Input.Debug.Stats.performed -= StatsOnPerformed;
            _Input.Debug.Console.performed -= ConsoleOnPerformed;
            
            _Input.Debug.Disable();
        }

        private void StatsOnPerformed(InputAction.CallbackContext obj) {
            Vloxy.gameObject.SetActive(!Vloxy.gameObject.activeSelf);
        }
        
        private void ConsoleOnPerformed(InputAction.CallbackContext obj) {
            Console.gameObject.SetActive(!Console.gameObject.activeSelf);
        }

    }

}