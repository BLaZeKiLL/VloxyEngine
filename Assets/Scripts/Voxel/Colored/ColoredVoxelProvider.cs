using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Meshing.Builder;

using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored {

    public class ColoredVoxelProvider : VoxelProvider<ColoredBlock> {
        
        public override Chunk<ColoredBlock> CreateChunk(Vector3Int position) {
            var chunk = new Chunk<ColoredBlock>(Settings.World.ChunkSize, position);

            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < Settings.World.ChunkSize.x; x++) {
                for (int z = 0; z < Settings.World.ChunkSize.z; z++) {
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
        
        public override IMeshBuilder<ColoredBlock> MeshBuilder() => new ColoredGreedyMeshBuilder();

    }

}