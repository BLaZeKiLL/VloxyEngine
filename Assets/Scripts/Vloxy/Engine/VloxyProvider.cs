
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Store;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Provider;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VloxyProvider : Provider<VloxyProvider> {

        public VloxySettings Settings { get; set; }

        public virtual BurstFunctionPointers SetupBurstFunctionPointers() => new();
        
        public virtual NoiseProfile NoiseProfile() => new (new NoiseProfile.Settings {
            Height = Settings.Noise.Height,
            Seed = Settings.Noise.Seed,
            Scale = Settings.Noise.Scale,
            Lacunarity = Settings.Noise.Lacunarity,
            Persistance = Settings.Noise.Persistance,
            Octaves = Settings.Noise.Octaves,
        });

        public virtual ChunkState ChunkState() => new(Settings);
        
        public virtual ChunkManager ChunkStore(
            ChunkState chunkState,
            ChunkStoreScheduler chunkStoreScheduler
        ) => new(Settings, chunkState, chunkStoreScheduler);

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);
        
        public virtual MeshBuildScheduler MeshBuildScheduler(
            ChunkState chunkState,
            ChunkManager chunkManager,
            ChunkBehaviourPool chunkBehaviourPool, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            chunkState,
            chunkManager,
            chunkBehaviourPool,
            burstFunctionPointers
        );

        public virtual ChunkStoreScheduler ChunkDataScheduler(
            NoiseProfile noiseProfile, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            noiseProfile,
            burstFunctionPointers
        );

    }

}