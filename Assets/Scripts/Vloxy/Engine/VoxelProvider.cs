using CBSL.Core.Provider;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Scheduler;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VoxelProvider : Provider<VoxelProvider> {

        public VoxelSettings Settings { get; set; }

        public virtual Chunk CreateChunk(int3 position, ChunkData data) => new(position, data);

        public virtual ChunkStore ChunkStore(INoiseProfile noiseProfile) => new(noiseProfile, Settings.Chunk);

        public virtual ChunkData CreateChunkData() => new(Settings.Chunk.ChunkSize);

        public virtual INoiseProfile NoiseProfile() => null;

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);

        public virtual IMeshBuildScheduler MeshBuildScheduler(ChunkBehaviourPool chunkBehaviourPool) => new MeshBuildJobScheduler(
            Settings.Scheduler.BatchSize, 
            Settings.Chunk.ChunkSize, 
            chunkBehaviourPool
        );

    }

}