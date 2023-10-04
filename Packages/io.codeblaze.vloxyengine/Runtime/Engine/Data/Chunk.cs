using CodeBlaze.Vloxy.Engine.Utils.Collections;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    [BurstCompile]
    public struct Chunk {

        public int3 Position { get; }
        public bool Dirty { get; private set; }
        
        private int3 ChunkSize;
        private UnsafeIntervalList Data;

        public Chunk(int3 position, int3 chunkSize) {
            Dirty = false;
            Position = position;
            ChunkSize = chunkSize;
            Data = new UnsafeIntervalList(128, Allocator.Persistent);
        }

        public void AddBlocks(int block, int count) {
            Data.AddInterval(block, count);
        }

        public bool SetBlock(int x, int y, int z, int block) {
            var result = Data.Set(ChunkSize.Flatten(x,y,z), block);
            if (result) Dirty = true;
            return result;
        }
        
        public bool SetBlock(int3 pos, int block) {
            var result= Data.Set(ChunkSize.Flatten(pos), block);
            if (result) Dirty = true;
            return result;
        }

        public int GetBlock(int x, int y, int z) {
            return Data.Get(ChunkSize.Flatten(x, y, z));
        }

        public int GetBlock(int3 pos) {
            return Data.Get(ChunkSize.Flatten(pos.x, pos.y, pos.z));
        }

        public void Dispose() {
            Data.Dispose();
        }

        public override string ToString() {
            return $"Pos : {Position}, Dirty : {Dirty}, Data : {Data.ToString()}";
        }

    }

}