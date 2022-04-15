using System.Collections;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CodeBlaze {
    public class MainMenuView : MonoBehaviour {

        [SerializeField] private VloxySettings _settings;

        private Button _button;
        private ProgressBar _loader;

        private MainMenuController _controller;

        private void OnEnable() {
            var document = GetComponent<UIDocument>();

            _controller = new MainMenuController(document.rootVisualElement);

            _button = document.rootVisualElement.Q<Button>("Generate");
            _loader = document.rootVisualElement.Q<ProgressBar>("LoadingBar");

            _button.RegisterCallback<ClickEvent>(OnGenerateWorld);
        }

        private void Update() {
            if (!IsEscapePressed()) return;

            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
#endif
        }

        private void OnGenerateWorld(ClickEvent _) {
            _controller.SetValue(_settings);

            VloxyLogger.Info<MainMenuView>("Generating World");
            
            StartCoroutine(GenerateWorld());
        }

        private IEnumerator GenerateWorld() {
            _loader.visible = true;

            var loader = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);

            while(!loader.isDone) {
                _loader.lowValue = loader.progress * 100;
                yield return null;
            }
        }
        
        private bool IsEscapePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null ? Keyboard.current.escapeKey.isPressed : false; 
#else
            return Input.GetKey(KeyCode.Escape);
#endif
        }

    }
}
