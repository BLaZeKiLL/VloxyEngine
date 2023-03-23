using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    [BurstCompile]
    public struct ChunkAccessor {
        
        private NativeParallelHashMap<int3, Chunk> Chunks;
        private int3 ChunkSize;

        public ChunkAccessor(NativeParallelHashMap<int3, Chunk> chunks, int3 chunkSize) {
            Chunks = chunks;
            ChunkSize = chunkSize;
        }

        public void Dispose() {
            Chunks.Dispose();
        }

        public int GetBlockInChunk(int3 chunk_pos, int3 block_pos) {
            var key = int3.zero;

            for (int index = 0; index < 3; index++) {
                if (block_pos[index] >= 0 && block_pos[index] < ChunkSize[index]) continue;

                key[index] += block_pos[index] % (ChunkSize[index] - 1);
                block_pos[index] -= key[index] * ChunkSize[index];
            }

            key *= ChunkSize;

            return TryGetChunk(chunk_pos + key, out var chunk) ? chunk.Data.GetBlock(block_pos) : 0;
        }

        public bool TryGetChunk(int3 pos, out Chunk chunk) => Chunks.TryGetValue(pos, out chunk);

        public bool ContainsChunk(int3 coord) => Chunks.ContainsKey(coord);
        
        public bool TryGetNeighborPX(int3 pos, out Chunk chunk) {
            var px = pos + new int3(1,0,0) * ChunkSize;

            return Chunks.TryGetValue(px, out chunk);
        }

        public bool TryGetNeighborPY(int3 pos, out Chunk chunk) {
            var py = pos + new int3(0,1,0) * ChunkSize;

            return Chunks.TryGetValue(py, out chunk);
        }

        public bool TryGetNeighborPZ(int3 pos, out Chunk chunk) {
            var pz = pos + new int3(0, 0, 1) * ChunkSize;

            return Chunks.TryGetValue(pz, out chunk);
        }

        public bool TryGetNeighborNX(int3 pos, out Chunk chunk) {
            var nx = pos + new int3(-1,0,0) * ChunkSize;

            return Chunks.TryGetValue(nx, out chunk);
        }

        public bool TryGetNeighborNY(int3 pos, out Chunk chunk) {
            var ny = pos + new int3(0,-1,0) * ChunkSize;

            return Chunks.TryGetValue(ny, out chunk);
        }

        
        public bool TryGetNeighborNZ(int3 pos, out Chunk chunk) {
            var nz = pos + new int3(0, 0, -1) * ChunkSize;

            return Chunks.TryGetValue(nz, out chunk);
        }

    }

}