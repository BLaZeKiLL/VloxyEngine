using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    [BurstCompile]
    internal struct ChunkAccessor {
        
        private NativeParallelHashMap<int3, Chunk>.ReadOnly _Chunks;
        private int3 _ChunkSize;

        internal ChunkAccessor(NativeParallelHashMap<int3, Chunk>.ReadOnly chunks, int3 chunkSize) {
            _Chunks = chunks;
            _ChunkSize = chunkSize;
        }

        internal int GetBlockInChunk(int3 chunk_pos, int3 block_pos) {
            var key = int3.zero;

            for (var index = 0; index < 3; index++) {
                if (block_pos[index] >= 0 && block_pos[index] < _ChunkSize[index]) continue;

                key[index] += block_pos[index] % (_ChunkSize[index] - 1);
                block_pos[index] -= key[index] * _ChunkSize[index];
            }

            key *= _ChunkSize;

            return TryGetChunk(chunk_pos + key, out var chunk) ? chunk.GetBlock(block_pos) : 0;
        }

        internal bool TryGetChunk(int3 pos, out Chunk chunk) => _Chunks.TryGetValue(pos, out chunk);

        internal bool ContainsChunk(int3 coord) => _Chunks.ContainsKey(coord);

        #region Try Neighbours
        
        internal bool TryGetNeighborPX(int3 pos, out Chunk chunk) {
            var px = pos + new int3(1,0,0) * _ChunkSize;

            return _Chunks.TryGetValue(px, out chunk);
        }

        internal bool TryGetNeighborPY(int3 pos, out Chunk chunk) {
            var py = pos + new int3(0,1,0) * _ChunkSize;

            return _Chunks.TryGetValue(py, out chunk);
        }

        internal bool TryGetNeighborPZ(int3 pos, out Chunk chunk) {
            var pz = pos + new int3(0, 0, 1) * _ChunkSize;

            return _Chunks.TryGetValue(pz, out chunk);
        }

        internal bool TryGetNeighborNX(int3 pos, out Chunk chunk) {
            var nx = pos + new int3(-1,0,0) * _ChunkSize;

            return _Chunks.TryGetValue(nx, out chunk);
        }

        internal bool TryGetNeighborNY(int3 pos, out Chunk chunk) {
            var ny = pos + new int3(0,-1,0) * _ChunkSize;

            return _Chunks.TryGetValue(ny, out chunk);
        }

        
        internal bool TryGetNeighborNZ(int3 pos, out Chunk chunk) {
            var nz = pos + new int3(0, 0, -1) * _ChunkSize;

            return _Chunks.TryGetValue(nz, out chunk);
        }
        
        #endregion

    }

}