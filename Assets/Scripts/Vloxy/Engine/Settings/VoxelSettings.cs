using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [CreateAssetMenu(fileName = "VoxelSettings", menuName = "Voxel Engine/Settings", order = 0)]
    public class VoxelSettings : ScriptableObject {

        public WorldSettings World;
        public RendererSettings Renderer;
        public INoiseSettings NoiseSettings;

    }

}