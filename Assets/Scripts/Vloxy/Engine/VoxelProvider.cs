using CBSL.Core.Provider;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Schedular;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VoxelProvider : Provider<VoxelProvider> {

        public VoxelSettings Settings { get; set; }

        public virtual Chunk CreateChunk(int3 position, NativeChunkData data) => new(position, data);

        public virtual NativeChunkStore ChunkStore(INoiseProfile noiseProfile) => new(noiseProfile, Settings.Chunk);

        public virtual NativeChunkData CreateChunkData() => new(Settings.Chunk.ChunkSize);

        public virtual INoiseProfile NoiseProfile() => null;

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);

        public virtual MeshBuildSchedular MeshBuildSchedular(ChunkBehaviourPool chunkBehaviourPool) => new MeshBuildJobSchedular(
            Settings.Scheduler.BatchSize, 
            Settings.Chunk.ChunkSize, 
            chunkBehaviourPool
        );

    }

}