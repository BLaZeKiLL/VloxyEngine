
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs;
using CodeBlaze.Vloxy.Engine.Jobs.Data;
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
            NoiseProfile noiseProfile,
            BurstFunctionPointers burstFunctionPointers
        ) => new(Settings, chunkState);

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);

        public virtual VloxyScheduler VloxyScheduler(
            MeshBuildScheduler meshBuildScheduler,
            ChunkDataScheduler chunkDataScheduler
        ) => new VloxyScheduler(meshBuildScheduler, chunkDataScheduler);
        
        public virtual MeshBuildScheduler MeshBuildScheduler(
            ChunkState chunkState,
            ChunkAccessor chunkAccessor,
            ChunkBehaviourPool chunkBehaviourPool, 
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            chunkState,
            chunkAccessor,
            chunkBehaviourPool,
            burstFunctionPointers
        );

        public virtual ChunkDataScheduler ChunkDataScheduler(
            ChunkState chunkState,
            ChunkStore chunkStore,
            NoiseProfile noiseProfile,
            BurstFunctionPointers burstFunctionPointers
        ) => new(
            Settings,
            chunkState,
            chunkStore,
            noiseProfile,
            burstFunctionPointers
        );

    }

}