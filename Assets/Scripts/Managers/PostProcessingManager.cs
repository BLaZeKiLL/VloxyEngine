using System;
using CodeBlaze.Vloxy.Demo.Utils;
using CodeBlaze.Vloxy.Engine.Data;
using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Demo.Managers {

    public enum PostProcessingMode {
        MAIN,
        UNDERWATER
    }
    
    public class PostProcessingManager : SingletonBehaviour<PostProcessingManager> {

        [SerializeField] private VolumeProfile _main;
        [SerializeField] private VolumeProfile _underwater;
        
        private Volume _volume;
        
        public PostProcessingMode Mode { get; private set; }

        private void Start() {
            _volume = GetComponent<Volume>();
            
            SetProfileMain();
        }

        public void UpdateMode(Block block) {
            switch (block) {
                case Block.WATER:
                    if (Mode != PostProcessingMode.UNDERWATER) SetProfileUnderwater();
                    break;
                default:
                    if (Mode != PostProcessingMode.MAIN) SetProfileMain();
                    break;
            }
        }

        private void SetProfileMain() {
            _volume.profile = _main;

            var drawDistance = WorldAPI.Current.World.Settings.Chunk.DrawDistance;
            
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = drawDistance * 32 - 16;

            Mode = PostProcessingMode.MAIN;
        }

        private void SetProfileUnderwater() {
            _volume.profile = _underwater;
            
            var drawDistance = WorldAPI.Current.World.Settings.Chunk.DrawDistance;
            
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = Mathf.Max(64, drawDistance * 32 - 128);

            Mode = PostProcessingMode.UNDERWATER;
        }

    }

}