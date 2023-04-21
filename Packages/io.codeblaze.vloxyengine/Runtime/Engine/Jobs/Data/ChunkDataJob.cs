﻿using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Data {

    [BurstCompile]
    public struct ChunkDataJob : IJobParallelFor {

        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public NoiseProfile NoiseProfile;

        [ReadOnly] public NativeArray<int3> Jobs;
        
        [WriteOnly] public NativeParallelHashMap<int3, Chunk>.ParallelWriter Results;

        public void Execute(int index) {
            var position = Jobs[index];

            var data = GenerateChunkData(position);

            Results.TryAdd(position, new Chunk(position, data));
        }
        
        private ChunkData GenerateChunkData(int3 position) {
            var data = new ChunkData(ChunkSize);
            
            var noise = NoiseProfile.GetNoise(position);
            int current_block = TexturedComputeBlockOverride(ref noise);
            
            int count = 0;
        
            // Loop order should be same as flatten order for AddBlocks to work properly
            for (int y = 0; y < ChunkSize.y; y++) {
                for (int z = 0; z < ChunkSize.z; z++) {
                    for (int x = 0; x < ChunkSize.x; x++) {
                        noise = NoiseProfile.GetNoise(position + new int3(x, y, z));
                        
                        var block = TexturedComputeBlockOverride(ref noise);
        
                        if (block == current_block) {
                            count++;
                        } else {
                            data.AddBlocks(current_block, count);
                            current_block = block;
                            count = 1;
                        }
                    }
                }
            }
            
            data.AddBlocks(current_block, count); // Finale interval

            return data;
        }
        
        private static int TexturedComputeBlockOverride(ref NoiseValue noise) {
            var Y = noise.Position.y;
            
            // if (Y > noise.Value ) return Y > noise.WaterLevel ? (int) TexturedBlock.AIR : (int) TexturedBlock.WATER;
            if (Y > noise.Value ) return (int) TexturedBlock.AIR;
            if (Y == noise.Value) return (int) TexturedBlock.GRASS;
            if (Y <= noise.Value - 1 && Y >= noise.Value - 3) return (int)TexturedBlock.DIRT;

            return (int) TexturedBlock.STONE;
        }

    }

}