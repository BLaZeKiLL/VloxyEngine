using CBSL.Core.Source.Core.Runtime.Provider;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Builder;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VoxelProvider<B> : Provider<VoxelProvider<B>> where B : IBlock {
        
        public VoxelSettings Settings { get; set; }

        public virtual Chunk<B> CreateChunk(Vector3Int position) => new Chunk<B>(position);

        public virtual ChunkCompressor<B> ChunkCompressor(int blockSize) => null;

        public virtual INoiseProfile<B> NoiseProfile() => null;

        public virtual ChunkPool<B> ChunkPool(Transform transform) => new ChunkPool<B>(transform);

        public virtual IMeshBuilder<B> MeshBuilder() => new GreedyMeshBuilder<B>(Settings.Chunk.ChunkSize);
        
        public virtual MeshBuildCoordinator<B> MeshBuildCoordinator(ChunkPool<B> chunkPool) => new UniTaskMultiThreadedMeshBuildCoordinator<B>(chunkPool, Settings.Schedular.BatchSize);

    }

}