using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlaze.Vloxy.Demo.RuntimeTools {

    public class DebugController : MonoBehaviour {

        [SerializeField] private RectTransform Vloxy;
        [SerializeField] private RectTransform Console;

        private VloxyInput _Input;

        private void OnEnable() {
            _Input ??= new VloxyInput();
            
            _Input.RuntimeTools.Enable();
            
            _Input.RuntimeTools.Stats.performed += StatsOnPerformed;
            _Input.RuntimeTools.Console.performed += ConsoleOnPerformed;
        }

        private void OnDisable() {
            _Input.RuntimeTools.Stats.performed -= StatsOnPerformed;
            _Input.RuntimeTools.Console.performed -= ConsoleOnPerformed;
            
            _Input.RuntimeTools.Disable();
        }

        private void StatsOnPerformed(InputAction.CallbackContext obj) {
            Vloxy.gameObject.SetActive(!Vloxy.gameObject.activeSelf);
        }
        
        private void ConsoleOnPerformed(InputAction.CallbackContext obj) {
            Console.gameObject.SetActive(!Console.gameObject.activeSelf);
        }

    }

}