using System;
using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.Settings;
using CodeBlaze.Voxel.Engine.World;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.World {

    public class ColoredWorld : World<ColoredBlock> {

        private int id; // remove

        protected override MeshBuildCoordinator<ColoredBlock> MeshBuildCoordinatorProvider() {
            switch (VoxelProvider.Current.Settings.BuildCoordinator.ProcessMethod) {
                case BuildCoordinatorSettings.BuildMethod.MultiThreaded:
                    return new ColoredUniTaskMultiThreadedMeshBuildCoordinator(this);
                case BuildCoordinatorSettings.BuildMethod.SingleThreaded:
                    return new ColoredSingleThreadedMeshBuildCoordinator(this);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override Chunk<ColoredBlock> CreateChunk(Vector3Int position) {
            var chunk = new ColoredChunk(WorldSettings.ChunkSize, position, ++id);
            
            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < WorldSettings.ChunkSize.x; x++) {
                for (int z = 0; z < WorldSettings.ChunkSize.z; z++) {
                    var height = Mathf.FloorToInt(
                        Mathf.PerlinNoise((position.x + x) * WorldSettings.Frequency, (position.z + z) * WorldSettings.Frequency) * WorldSettings.ChunkSize.y
                    );

                    height = Mathf.Clamp(height, 1, WorldSettings.ChunkSize.y - 1);

                    for (int y = 0; y < height; y++) {
                        chunk.SetBlock(block, x, y, z);
                    }

                    for (int y = height; y < WorldSettings.ChunkSize.y; y++) {
                        chunk.SetBlock(ColoredBlockTypes.Air(), x, y, z);
                    }
                }
            }
            
            return chunk;
        }

    }

}