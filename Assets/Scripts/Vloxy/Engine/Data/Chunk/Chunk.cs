using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public struct Chunk {

        public NativeChunkData Data { get; }
        
        public int3 Position { get; }
        
        public Chunk(int3 position, NativeChunkData data) {
            Position = position;
            Data = data;
        }

    }

}