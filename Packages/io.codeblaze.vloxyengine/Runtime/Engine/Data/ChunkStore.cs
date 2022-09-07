using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        public int3 Position { get; }
        public NativeParallelHashMap<int3, Chunk> Chunks { get; }

        public ChunkStore(int3 position, int pageSize) {
            Position = position;

            Chunks = new NativeParallelHashMap<int3, Chunk>(
                pageSize.CubedSize(), 
                Allocator.Persistent
            );
        }

        public int ChunkCount() => Chunks.Count();

        public void Dispose() {
            foreach (var pair in Chunks) {
                pair.Value.Data.Dispose();
            }
            
            Chunks.Dispose();
        }

        public bool ContainsChunk(int3 position) => Chunks.ContainsKey(position);

        public void RemoveChunk(int3 position) => Chunks.Remove(position);

    }

}