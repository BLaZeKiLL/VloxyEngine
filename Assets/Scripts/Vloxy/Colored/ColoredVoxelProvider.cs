using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Colored.Meshing.Builder;
using CodeBlaze.Vloxy.Colored.Meshing.Coordinator;
using CodeBlaze.Vloxy.Colored.Noise;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Builder;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

namespace CodeBlaze.Vloxy.Colored {

    public class ColoredVoxelProvider : VoxelProvider<ColoredBlock> {
        
        public override IMeshBuilder<ColoredBlock> MeshBuilder() => new ColoredGreedyMeshBuilder(Settings.Chunk.ChunkSize);

        public override INoiseProfile<ColoredBlock> NoiseProfile() => new ColoredNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk);

        public override IChunkData<ColoredBlock> ChunkData(ColoredBlock[] blocks) {
            var data = new CompressibleChunkData<ColoredBlock>(blocks, 4, Settings.Chunk.ChunkSize, bytes => new ColoredBlock(bytes), block => block.GetBytes());
            
            if (Settings.Chunk.UseCompression) data.Compress();

            return data;
        }

        public override MeshBuildCoordinator<ColoredBlock> MeshBuildCoordinator(ChunkBehaviourPool<ColoredBlock> chunkBehaviourPool) => new ColoredMeshBuildCoordinator(chunkBehaviourPool, Settings.Schedular.BatchSize, Settings.Chunk.UseCompression);

    }

}