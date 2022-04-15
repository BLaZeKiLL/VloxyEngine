
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Scheduler;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Provider;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VloxyProvider : Provider<VloxyProvider> {

        public VloxySettings Settings { get; set; }

        public virtual BurstFunctionPointers SetupBurstFunctionPointers() => new();

        public virtual Chunk CreateChunk(int3 position, ChunkData data) => new(position, data);
        
        public virtual ChunkData CreateChunkData() => new(Settings.Chunk.ChunkSize);

        public virtual ChunkStore ChunkStore(INoiseProfile noiseProfile) => new(noiseProfile, Settings.Chunk);

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);
        
        public virtual MeshBuildScheduler MeshBuildScheduler(ChunkBehaviourPool chunkBehaviourPool, BurstFunctionPointers burstFunctionPointers) => new(
            Settings.Scheduler.BatchSize, 
            Settings.Chunk.ChunkSize, 
            Settings.Chunk.DrawDistance,
            chunkBehaviourPool,
            burstFunctionPointers
        );
        
        public virtual INoiseProfile NoiseProfile() => null;

    }

}