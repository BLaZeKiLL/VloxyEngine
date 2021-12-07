using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public struct Chunk {

        // TODO : initialize chunk data
        public IChunkData Data { get; set; }
        
        public int3 Position { get; }
        
        internal ChunkState State { get; set; }
        
        public Chunk(int3 position) {
            Position = position;
            Data = null;
            State = ChunkState.INACTIVE;
        }

        public string Name() {
            return $"Chunk {Position}";
        }

    }

}