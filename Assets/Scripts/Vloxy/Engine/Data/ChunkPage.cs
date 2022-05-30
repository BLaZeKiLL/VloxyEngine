using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkPage {

        public int3 Position { get; }
        public NativeHashMap<int3, Chunk> Chunks { get; }
        
        private int PageSize;
        private int3 ChunkSize;

        public ChunkPage(int3 position, int pageSize, int3 chunkSize) {
            Position = position;
            PageSize = pageSize;
            ChunkSize = chunkSize;

            Chunks = new NativeHashMap<int3, Chunk>(PageSize.CubedSize(), Allocator.Persistent);
        }

        public int ChunkCount() => Chunks.Count();

        public void Dispose() {
            foreach (var pair in Chunks) {
                pair.Value.Data.Dispose();
            }
            
            Chunks.Dispose();
        }

        public NativeArray<int3> GetPositions(Allocator handle) {
            var result = new NativeArray<int3>(PageSize.CubedSize(), handle);
            var index = 0;
            
            for (int x = -PageSize; x <= PageSize; x++) {
                for (int z = -PageSize; z <= PageSize; z++) {
                    for (int y = -PageSize; y <= PageSize; y++) {
                        result[index] = (new int3(x, y, z) * ChunkSize); // + Page Offset
                        index++;
                    }
                }
            }

            return result;
        }
        
        

    }

}