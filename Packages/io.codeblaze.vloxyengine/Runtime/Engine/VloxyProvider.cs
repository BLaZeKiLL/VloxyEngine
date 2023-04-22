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

        public virtual NoiseProfile NoiseProfile() => new (new NoiseProfile.Settings {
            Height = Settings.Noise.Height,
            WaterLevel = Settings.Noise.WaterLevel,
            Seed = Settings.Noise.Seed,
            Scale = Settings.Noise.Scale,
            Lacunarity = Settings.Noise.Lacunarity,
            Persistance = Settings.Noise.Persistance,
            Octaves = Settings.Noise.Octaves,
        });

        public virtual ChunkManager ChunkManager() => new(Settings);

        public virtual ChunkPool ChunkPoolV2(Transform transform) => new (transform, Settings);

        public virtual VloxyScheduler VloxySchedulerV2(
            MeshBuildScheduler meshBuildScheduler,
            ChunkDataScheduler chunkDataScheduler,
            ChunkStore chunkStore,
            ChunkPool chunkPool
        ) => new(Settings, meshBuildScheduler, chunkDataScheduler, chunkStore, chunkPool);

        public virtual ChunkDataScheduler ChunkDataSchedulerV2(
            ChunkStore chunkStore,
            NoiseProfile noiseProfile
        ) => new(
            Settings,
            chunkStore,
            noiseProfile
        );

        public virtual MeshBuildScheduler MeshBuildSchedulerV2(
            ChunkStore chunkStore,
            ChunkPool chunkPool
        ) => new(
            Settings,
            chunkStore,
            chunkPool
        );

    }

}