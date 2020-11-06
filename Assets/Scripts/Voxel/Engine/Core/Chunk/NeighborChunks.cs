namespace CodeBlaze.Voxel.Engine.Core {

    public class NeighborChunks<T> where T : IBlock {

        public Chunk<T> ChunkPX { get; set; }
        public Chunk<T> ChunkPY { get; set; }
        public Chunk<T> ChunkPZ { get; set; }
        public Chunk<T> ChunkNX { get; set; }
        public Chunk<T> ChunkNY { get; set; }
        public Chunk<T> ChunkNZ { get; set; }
        
    }

}