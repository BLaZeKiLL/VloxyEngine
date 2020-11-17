using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Settings {

    [CreateAssetMenu(fileName = "VoxelSettings", menuName = "Voxel Engine/Settings", order = 0)]
    public class VoxelSettings : ScriptableObject {

        public WorldSettings World;
        public RendererSettings Renderer;

    }

}