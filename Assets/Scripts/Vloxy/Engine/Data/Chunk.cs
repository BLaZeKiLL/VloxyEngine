using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public struct Chunk {

        public ChunkData Data { get; }
        
        public int3 Position { get; }
        
        public Chunk(int3 position, ChunkData data) {
            Position = position;
            Data = data;
        }

    }

}