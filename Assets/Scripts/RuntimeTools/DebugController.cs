using CodeBlaze.Vloxy.Demo.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlaze.Vloxy.Demo.RuntimeTools {

    public class DebugController : MonoBehaviour {

        [SerializeField] private RectTransform Vloxy;
        [SerializeField] private RectTransform Console;

        private VloxyInput.RuntimeToolsActions _RuntimeToolsMap;

        private void OnEnable() {
            _RuntimeToolsMap = GameManager.Current.InputMaps.RuntimeTools;
            
            _RuntimeToolsMap.Enable();
            
            _RuntimeToolsMap.Stats.performed += StatsOnPerformed;
            _RuntimeToolsMap.Console.performed += ConsoleOnPerformed;
        }

        private void OnDisable() {
            _RuntimeToolsMap.Stats.performed -= StatsOnPerformed;
            _RuntimeToolsMap.Console.performed -= ConsoleOnPerformed;
            
            _RuntimeToolsMap.Disable();
        }

        private void StatsOnPerformed(InputAction.CallbackContext obj) {
            Vloxy.gameObject.SetActive(!Vloxy.gameObject.activeSelf);
        }
        
        private void ConsoleOnPerformed(InputAction.CallbackContext obj) {
            Console.gameObject.SetActive(!Console.gameObject.activeSelf);
        }

    }

}