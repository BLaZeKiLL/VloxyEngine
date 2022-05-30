using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "VloxySettings", menuName = "Vloxy/EngineSettings", order = 0)]
    public class VloxySettings : ScriptableObject {

        public NoiseSettings NoiseSettings = new ();
        public ChunkSettings Chunk = new ();
        public RendererSettings Renderer = new ();
        public SchedulerSettings Scheduler = new ();

    }

}