using System;

using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class MeshBuildJobData<B> where B : IBlock {

        public Chunk<B> Chunk { get; set; }
        public Chunk<B> ChunkPX { get; set; }
        public Chunk<B> ChunkPY { get; set; }
        public Chunk<B> ChunkPZ { get; set; }
        public Chunk<B> ChunkNX { get; set; }
        public Chunk<B> ChunkNY { get; set; }
        public Chunk<B> ChunkNZ { get; set; }

        public void ForEach(Action<Chunk<B>> opt) {
            opt(Chunk);
            if (ChunkPX != null) opt(ChunkPX);
            if (ChunkPY != null) opt(ChunkPY);
            if (ChunkPZ != null) opt(ChunkPZ);
            if (ChunkNX != null) opt(ChunkNX);
            if (ChunkNY != null) opt(ChunkNY);
            if (ChunkNZ != null) opt(ChunkNZ);
        }

    }

}