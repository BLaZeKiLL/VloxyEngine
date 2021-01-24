namespace CodeBlaze.Voxel.Engine.Data {

    public class ChunkJobData<B> where B : IBlock {

        public Chunk<B> Chunk { get; set; }
        public Chunk<B> ChunkPX { get; set; }
        public Chunk<B> ChunkPY { get; set; }
        public Chunk<B> ChunkPZ { get; set; }
        public Chunk<B> ChunkNX { get; set; }
        public Chunk<B> ChunkNY { get; set; }
        public Chunk<B> ChunkNZ { get; set; }
        
    }

}