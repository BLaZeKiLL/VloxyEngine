using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Meshing.Builder;
using CodeBlaze.Voxel.Colored.Noise;
using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Noise.Profile;
using CodeBlaze.Voxel.Engine.Noise.Settings;

namespace CodeBlaze.Voxel.Colored {

    public class ColoredVoxelProvider : VoxelProvider<ColoredBlock> {
        
        public override IMeshBuilder<ColoredBlock> MeshBuilder() => new ColoredGreedyMeshBuilder();

        public override INoiseProfile<ColoredBlock> NoiseProfile() => new ColoredNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D);

    }

}