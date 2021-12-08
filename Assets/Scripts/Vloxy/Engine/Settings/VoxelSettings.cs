using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "VoxelSettings", menuName = "Voxel Engine/Settings", order = 0)]
    public class VoxelSettings : ScriptableObject {

        public INoiseSettings NoiseSettings;
        public ChunkSettings Chunk;
        public RendererSettings Renderer;
        public SchedulerSettings Scheduler;

    }

}