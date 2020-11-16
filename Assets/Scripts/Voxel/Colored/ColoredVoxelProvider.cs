using System;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Meshing.Builder;
using CodeBlaze.Voxel.Colored.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Settings;
using CodeBlaze.Voxel.Engine.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored {

    public class ColoredVoxelProvider : IVoxelProvider<ColoredBlock> {

        private int id;
        
        public VoxelSettings Settings { get; set; }
        
        public Chunk<ColoredBlock> Chunk(Vector3Int position) {
            var chunk = new ColoredChunk(Settings.World.ChunkSize, position, ++id);

            for (int x = 0; x < Settings.World.ChunkSize.x; x++) {
                for (int z = 0; z < Settings.World.ChunkSize.z; z++) {
                    var block = ColoredBlockTypes.RandomSolid();
                    
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * Settings.World.Frequency, (position.z + z) * Settings.World.Frequency) * Settings.World.ChunkSize.y
                    );

                    height = Mathf.Clamp(height, 1, Settings.World.ChunkSize.y - 1);

                    for (int y = 0; y < height; y++) {
                        chunk.SetBlock(block, x, y, z);
                    }

                    for (int y = height; y < Settings.World.ChunkSize.y; y++) {
                        chunk.SetBlock(ColoredBlockTypes.Air(), x, y, z);
                    }
                }
            }
            
            return chunk;
        }

        public IMeshBuilder<ColoredBlock> MeshBuilder() => new ColoredGreedyMeshBuilder();

        public MeshBuildCoordinator<ColoredBlock> MeshBuildCoordinator(World<ColoredBlock> world) {
            switch (Settings.BuildCoordinator.ProcessMethod) {
                case BuildCoordinatorSettings.BuildMethod.MultiThreaded:
                    return new ColoredUniTaskMultiThreadedMeshBuildCoordinator(world);
                case BuildCoordinatorSettings.BuildMethod.SingleThreaded:
                    return new ColoredSingleThreadedMeshBuildCoordinator(world);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}