
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Chunk;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Provider;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VloxyProvider : Provider<VloxyProvider> {

        public VloxySettings Settings { get; set; }

        public virtual BurstFunctionPointers SetupBurstFunctionPointers() => new();
        
        public virtual NoiseProfile NoiseProfile() => new (new NoiseProfile.Settings {
            Height = Settings.NoiseSettings.Height,
            Seed = Settings.NoiseSettings.Seed,
            Scale = Settings.NoiseSettings.Scale,
            Lacunarity = Settings.NoiseSettings.Lacunarity,
            Persistance = Settings.NoiseSettings.Persistance,
            Octaves = Settings.NoiseSettings.Octaves,
        });

        public virtual Chunk CreateChunk(int3 position, ChunkData data) => new(position, data);
        
        public virtual ChunkStore ChunkStore(
            ChunkDataScheduler chunkDataScheduler
        ) => new(chunkDataScheduler, Settings.Chunk);

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);
        
        public virtual MeshBuildScheduler MeshBuildScheduler(
            ChunkBehaviourPool chunkBehaviourPool, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings.Scheduler.BatchSize, 
            Settings.Chunk.ChunkSize, 
            Settings.Chunk.DrawDistance,
            chunkBehaviourPool,
            burstFunctionPointers
        );

        public virtual ChunkDataScheduler ChunkDataScheduler(
            NoiseProfile noiseProfile, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            noiseProfile,
            Settings.Chunk.ChunkSize, 
            Settings.Chunk.ChunkPageSize, 
            burstFunctionPointers
        );

    }

}