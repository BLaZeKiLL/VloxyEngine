using CodeBlaze.Voxel.Engine.Settings;

namespace CodeBlaze.Voxel.Engine {

    public class VoxelProvider {

        public static VoxelProvider Current { get; private set; }

        public VoxelSettings Settings { get; private set;  }

        public static void Initialize(VoxelSettings settings) {
            Current = new VoxelProvider();
            Current.Settings = settings;
        }
        
    }

}