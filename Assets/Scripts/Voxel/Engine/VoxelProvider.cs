using System;

using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Settings;
using CodeBlaze.Voxel.Engine.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine {

    public class VoxelProvider<B> where B : IBlock {

        public static VoxelProvider<B> Current { get; private set; }
        
        public static void Initialize(Func<VoxelProvider<B>> provider, VoxelSettings settings) {
            Current = provider();
            Current.Settings = settings;
            Current.Initialize();
        }
        
        public VoxelSettings Settings { get; set; }

        protected virtual void Initialize() { }

        public virtual Chunk<B> CreateChunk(Vector3Int position) => new Chunk<B>(Settings.World.ChunkSize, position);

        public virtual ChunkPool<B> ChunkPool(Transform transform) => new ChunkPool<B>(transform);

        public virtual IMeshBuilder<B> MeshBuilder() => new GreedyMeshBuilder<B>();
        
        public virtual MeshBuildCoordinator<B> MeshBuildCoordinator(World<B> world) => new UniTaskMultiThreadedMeshBuildCoordinator<B>(world);

    }

}