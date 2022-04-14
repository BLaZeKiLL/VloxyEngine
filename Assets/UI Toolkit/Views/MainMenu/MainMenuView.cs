using System;
using System.Collections;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace CodeBlaze {
    public class MainMenuView : MonoBehaviour {

        [SerializeField] private VloxySettings _settings;

        private Button _button;
        private ProgressBar _loader;

        private SliderInt _pageSize;
        private SliderInt _drawDistance;

        private TextField _chunkSizeX;
        private TextField _chunkSizeY;
        private TextField _chunkSizeZ;

        private void OnEnable() {
            var document = GetComponent<UIDocument>();
                
            _button = document.rootVisualElement.Q<Button>("Generate");
            _loader = document.rootVisualElement.Q<ProgressBar>("LoadingBar");

            _pageSize = document.rootVisualElement.Q<SliderInt>("PageSize");
            _drawDistance = document.rootVisualElement.Q<SliderInt>("DrawDistance");

            _chunkSizeX = document.rootVisualElement.Q<TextField>("ChunkSizeX");
            _chunkSizeY = document.rootVisualElement.Q<TextField>("ChunkSizeY");
            _chunkSizeZ = document.rootVisualElement.Q<TextField>("ChunkSizeZ");

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
            _settings.Chunk.ChunkPageSize = _pageSize.value;
            _settings.Chunk.DrawDistance = _drawDistance.value;
            _settings.Chunk.ChunkSize = new int3(
                int.Parse(_chunkSizeX.value), 
                int.Parse(_chunkSizeY.value), 
                int.Parse(_chunkSizeZ.value)
            );

            VloxyLogger.Info<MainMenuView>("Generating World");
            _loader.visible = true;
            StartCoroutine(GenerateWorld());
        }

        private IEnumerator GenerateWorld() {
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
