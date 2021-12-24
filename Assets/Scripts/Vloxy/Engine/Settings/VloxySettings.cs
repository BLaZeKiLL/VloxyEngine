using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "VloxySettings", menuName = "Vloxy/EngineSettings", order = 0)]
    public class VloxySettings : ScriptableObject {

        public INoiseSettings NoiseSettings;
        public ChunkSettings Chunk;
        public RendererSettings Renderer;
        public SchedulerSettings Scheduler;

    }

}