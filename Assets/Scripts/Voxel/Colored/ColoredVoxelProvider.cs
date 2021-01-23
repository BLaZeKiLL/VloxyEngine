using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Meshing.Builder;

using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored {

    public class ColoredVoxelProvider : VoxelProvider<ColoredBlock> {

        private FastNoiseLite _noise;

        protected override void Initialize() {
            _noise = new FastNoiseLite();
            _noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            _noise.SetFrequency(Settings.World.Frequency);
            _noise.SetFractalOctaves(4);
            _noise.SetFractalLacunarity(2);
            _noise.SetFractalGain(0.5f);
        }

        public override Chunk<ColoredBlock> CreateChunk(Vector3Int position) {
            var chunk = new Chunk<ColoredBlock>(Settings.World.ChunkSize, position);

            var block = ColoredBlockTypes.RandomSolid();
            
            for (int x = 0; x < Settings.World.ChunkSize.x; x++) {
                for (int z = 0; z < Settings.World.ChunkSize.z; z++) {
                    var height = GetNoise(position.x + x, position.z + z);

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

        private int GetNoise(int x, int z) {
            return Mathf.Clamp(Mathf.FloorToInt(((_noise.GetNoise(x, z) + 1) / 2) * Settings.World.ChunkSize.y), 1, Settings.World.ChunkSize.y - 1);
        }

    }

}