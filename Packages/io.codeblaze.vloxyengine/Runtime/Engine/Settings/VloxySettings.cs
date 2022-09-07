using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "VloxySettings", menuName = "Vloxy/EngineSettings", order = 0)]
    public class VloxySettings : ScriptableObject {

        public NoiseSettings Noise;
        public ChunkSettings Chunk;
        public RendererSettings Renderer;
        public SchedulerSettings Scheduler;

    }

}