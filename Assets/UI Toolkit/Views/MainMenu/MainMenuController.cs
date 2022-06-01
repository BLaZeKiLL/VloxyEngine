using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;

using Unity.Mathematics;

using UnityEngine.UIElements;

using Random = UnityEngine.Random;

namespace CodeBlaze {

    public class MainMenuController {

        private readonly HeightMapController _heightMapController;
        private readonly ChunkPageController _chunkPageController;
        private readonly RenderingController _renderingController;
        private readonly SchedularController _schedularController;

        private readonly List<Label> _buttons;
        private readonly List<VisualElement> _tabs;

        private int _index = 0;

        public MainMenuController(VisualElement root) {
            _tabs = new List<VisualElement> {
                root.Q<VisualElement>("HeightMapPage"),
                root.Q<VisualElement>("ChunkPage"),
                root.Q<VisualElement>("RenderingPage"),
                root.Q<VisualElement>("SchedularPage"),
            };

            _buttons = new List<Label> {
                root.Q<Label>("HeightMapButton"),
                root.Q<Label>("ChunkButton"),
                root.Q<Label>("RenderingButton"),
                root.Q<Label>("SchedularButton"),
            };
            
            _heightMapController = new HeightMapController(_tabs[0]);
            _chunkPageController = new ChunkPageController(_tabs[1]);
            _renderingController = new RenderingController(_tabs[2]);
            _schedularController = new SchedularController(_tabs[3]);

            for (int i = 0; i < _buttons.Count; i++) {
                var index = i;
                _buttons[i].RegisterCallback<ClickEvent>(_ => ChangeTab(index));
            }
        }

        private void ChangeTab(int index) {
            _buttons[_index].RemoveFromClassList("vloxy-text-primary");
            _tabs[_index].AddToClassList("vloxy-display-none");
            
            _index = index;
            
            _buttons[_index].AddToClassList("vloxy-text-primary");
            _tabs[_index].RemoveFromClassList("vloxy-display-none");
        }

        public void SetValue(VloxySettings settings) {
            _heightMapController.SetValue(settings);
            _chunkPageController.SetValue(settings);
            _renderingController.SetValue(settings);
            _schedularController.SetValue(settings);
        }

        private class HeightMapController {

            private readonly TextField _height;
            private readonly TextField _seed;
            private readonly TextField _frequency;
            private readonly TextField _gain;
            private readonly TextField _lacunarity;
            private readonly TextField _octaves;

            public HeightMapController(VisualElement root) {
                _height = root.Q<TextField>("Height");
                _seed = root.Q<TextField>("Seed");
                _frequency = root.Q<TextField>("Scale");
                _gain = root.Q<TextField>("Persistence");
                _lacunarity = root.Q<TextField>("Lacunarity");
                _octaves = root.Q<TextField>("Octaves");

                _seed.value = Random.Range(0, 1000000).ToString();
            }

            public void SetValue(VloxySettings settings) {
                var noise = settings.NoiseSettings as NoiseSettings;

                noise.Height = int.Parse(_height.value);
                noise.Seed = int.Parse(_seed.value);
                noise.Scale = float.Parse(_frequency.value);
                noise.Persistance = float.Parse(_gain.value);
                noise.Lacunarity = float.Parse(_lacunarity.value);
                noise.Octaves = int.Parse(_octaves.value);
            }

        }
    
        private class ChunkPageController {

            private readonly SliderInt _pageSize;
            private readonly SliderInt _drawDistance;

            private readonly TextField _chunkSizeX;
            private readonly TextField _chunkSizeY;
            private readonly TextField _chunkSizeZ;
            
            public ChunkPageController(VisualElement root) {
                _pageSize = root.Q<SliderInt>("PageSize");
                _drawDistance = root.Q<SliderInt>("DrawDistance");

                _chunkSizeX = root.Q<TextField>("ChunkSizeX");
                _chunkSizeY = root.Q<TextField>("ChunkSizeY");
                _chunkSizeZ = root.Q<TextField>("ChunkSizeZ");
            }

            public void SetValue(VloxySettings settings) {
                settings.Chunk.PageSize = _pageSize.value;
                settings.Chunk.DrawDistance = _drawDistance.value;
                settings.Chunk.ChunkSize = new int3(
                    int.Parse(_chunkSizeX.value), 
                    int.Parse(_chunkSizeY.value), 
                    int.Parse(_chunkSizeZ.value)
                );
            }

        }

        private class RenderingController {

            private readonly Toggle _castShadows;

            public RenderingController(VisualElement root) {
                _castShadows = root.Q<Toggle>("CastShadows");
            }
            
            public void SetValue(VloxySettings settings) {
                settings.Renderer.CastShadows = _castShadows.value;
            }

        }

        private class SchedularController {

            private readonly TextField _batchSize;
            
            public SchedularController(VisualElement root) {
                _batchSize = root.Q<TextField>("BatchSize");
            }
            
            public void SetValue(VloxySettings settings) {
                settings.Scheduler.BatchSize = int.Parse(_batchSize.value);
            }

        }
        

    }

}