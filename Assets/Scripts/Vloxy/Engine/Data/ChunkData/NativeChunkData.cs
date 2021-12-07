using CodeBlaze.Vloxy.Engine.Unsafe;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public struct NativeChunkData : IChunkData {

        private int3 ChunkSize;
        private UnsafeCompressedList Data;

        public NativeChunkData(int3 chunkSize) {
            ChunkSize = chunkSize;
            Data = new UnsafeCompressedList(32, Allocator.Persistent);
        }

        public void AddBlocks(int block, int count) {
            Data.AddNode(block, count);
        }

        public void SetBlock(int block, int x, int y, int z) {
            Data.Set(block, ChunkSize.Flatten(x,y,z));
        }

        public int GetBlock(int x, int y, int z) {
            return Data.Get(ChunkSize.Flatten(x, y, z));
        }

        public void Dispose() {
            Data.Dispose();
        }

    }

}