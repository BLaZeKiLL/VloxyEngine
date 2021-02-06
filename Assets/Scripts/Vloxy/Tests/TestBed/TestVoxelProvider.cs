using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

namespace CodeBlaze.Voxel.Engine.Test.TestBed {

    public class TestVoxelProvider : VoxelProvider<TestBlock> {

        public override INoiseProfile<TestBlock> NoiseProfile() =>
            new TestNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk);

    }

}