using CBSL.Core.Provider;

using CodeBlaze.Vloxy.Engine.Components;
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

        public virtual ChunkStore<B> ChunkStore(INoiseProfile<B> noiseProfile) => new ChunkStore<B>(noiseProfile, Settings.Chunk);

        public virtual IChunkData<B> ChunkData(B[] blocks) => null;
        
        public virtual INoiseProfile<B> NoiseProfile() => null;

        public virtual ChunkBehaviourPool<B> ChunkPool(Transform transform) => new ChunkBehaviourPool<B>(transform);

        public virtual IMeshBuilder<B> MeshBuilder() => new GreedyMeshBuilder<B>(Settings.Chunk.ChunkSize);
        
        public virtual MeshBuildCoordinator<B> MeshBuildCoordinator(ChunkBehaviourPool<B> chunkBehaviourPool) => new UniTaskMultiThreadedMeshBuildCoordinator<B>(chunkBehaviourPool, Settings.Schedular.BatchSize);

    }

}