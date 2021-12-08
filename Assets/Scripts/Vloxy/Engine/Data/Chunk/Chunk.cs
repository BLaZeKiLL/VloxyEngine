using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public struct Chunk {

        // TODO : initialize chunk data
        public NativeChunkData Data { get; set; }
        
        public int3 Position { get; }
        
        internal ChunkState State { get; set; }
        
        public Chunk(int3 position, NativeChunkData data) {
            Position = position;
            Data = data;
            State = ChunkState.INACTIVE;
        }

    }

}