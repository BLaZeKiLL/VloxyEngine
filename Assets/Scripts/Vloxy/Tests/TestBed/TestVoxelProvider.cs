using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

namespace CodeBlaze.Voxel.Engine.Test.TestBed {

    public class TestVoxelProvider : VoxelProvider<TestBlock> {

        public override ChunkCompressor<TestBlock> ChunkCompressor(int blockSize) =>
            new TestCompressor(blockSize, Settings.Chunk.ChunkSize);

        public override INoiseProfile<TestBlock> NoiseProfile() =>
            new TestNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk.ChunkSize);

    }

}