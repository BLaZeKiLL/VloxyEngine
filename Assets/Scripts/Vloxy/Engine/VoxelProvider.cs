using System;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Builder;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine {

    public class VoxelProvider<B> where B : IBlock {

        public static VoxelProvider<B> Current { get; private set; }
        
        public static void Initialize(VoxelProvider<B> provider, VoxelSettings settings) {
            Current = provider;
            Current.Settings = settings;
            Current.Initialize();
        }
        
        public VoxelSettings Settings { get; set; }

        protected virtual void Initialize() { }

        public virtual Chunk<B> CreateChunk(Vector3Int position) => new Chunk<B>(Settings.World.ChunkSize, position);

        public virtual INoiseProfile<B> NoiseProfile() => null;

        public virtual ChunkPool<B> ChunkPool(Transform transform) => new ChunkPool<B>(transform);

        public virtual IMeshBuilder<B> MeshBuilder() => new GreedyMeshBuilder<B>();
        
        public virtual MeshBuildCoordinator<B> MeshBuildCoordinator(ChunkPool<B> chunkPool) => new UniTaskMultiThreadedMeshBuildCoordinator<B>(chunkPool);

    }

}