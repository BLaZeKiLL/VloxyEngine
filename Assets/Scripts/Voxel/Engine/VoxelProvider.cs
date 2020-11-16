using System;

using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Settings;
using CodeBlaze.Voxel.Engine.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine {

    public class VoxelProvider<B> where B : IBlock {

        public static IVoxelProvider<B> Current { get; private set; }
        
        public static void Initialize(Func<IVoxelProvider<B>> initializer, VoxelSettings settings) {
            Current = initializer();
            Current.Settings = settings;
        }

    }

    public interface IVoxelProvider<B> where B : IBlock {

        VoxelSettings Settings { get; set; }

        Chunk<B> Chunk(Vector3Int position);
        
        IMeshBuilder<B> MeshBuilder();
        
        MeshBuildCoordinator<B> MeshBuildCoordinator(World<B> world);

    }

}