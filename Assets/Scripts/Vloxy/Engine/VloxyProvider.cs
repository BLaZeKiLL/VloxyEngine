
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Page;
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
            Height = Settings.NoiseSettings.Height,
            Seed = Settings.NoiseSettings.Seed,
            Scale = Settings.NoiseSettings.Scale,
            Lacunarity = Settings.NoiseSettings.Lacunarity,
            Persistance = Settings.NoiseSettings.Persistance,
            Octaves = Settings.NoiseSettings.Octaves,
        });

        public virtual ChunkState ChunkState() => new();
        
        public virtual ChunkStore ChunkStore(
            ChunkState chunkState,
            ChunkPageScheduler chunkPageScheduler
        ) => new(Settings, chunkState, chunkPageScheduler);

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);
        
        public virtual MeshBuildScheduler MeshBuildScheduler(
            ChunkState chunkState,
            ChunkBehaviourPool chunkBehaviourPool, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            chunkState,
            chunkBehaviourPool,
            burstFunctionPointers
        );

        public virtual ChunkPageScheduler ChunkDataScheduler(
            NoiseProfile noiseProfile, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            noiseProfile,
            burstFunctionPointers
        );

    }

}