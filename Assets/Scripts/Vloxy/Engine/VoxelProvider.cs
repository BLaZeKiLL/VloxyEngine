using System;
using System.Collections.Generic;

using CBSL.Core.Provider;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Schedular;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VoxelProvider : Provider<VoxelProvider> {

        public VoxelSettings Settings { get; set; }

        public virtual Chunk CreateChunk(int3 position) => new(position);

        public virtual NativeChunkStore ChunkStore(INoiseProfile noiseProfile) => new(noiseProfile, Settings.Chunk);

        public virtual IChunkData CreateChunkData() => new NativeChunkData();

        public virtual INoiseProfile NoiseProfile() => null;

        public virtual ChunkBehaviourPool ChunkPool(Transform transform) => new(transform, Settings);

        public virtual IMesher<B> MeshBuilder() => new GreedyMesher<B>();
        
        public virtual MeshBuildSchedular<B> MeshBuildCoordinator(ChunkBehaviourPool<B> chunkBehaviourPool) => new UniTaskMeshBuildSchedular<B>(chunkBehaviourPool);

    }

}