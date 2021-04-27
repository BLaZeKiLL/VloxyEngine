using System;

using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class MeshBuildJobData<B> where B : IBlock {

        public Vector3Int Size { get; }
        public Chunk<B> Chunk { get; set; }
        public Chunk<B> ChunkPX { get; set; }
        public Chunk<B> ChunkPY { get; set; }
        public Chunk<B> ChunkPZ { get; set; }
        public Chunk<B> ChunkNX { get; set; }
        public Chunk<B> ChunkNY { get; set; }
        public Chunk<B> ChunkNZ { get; set; }

        public MeshBuildJobData(Vector3Int size) {
            Size = size;
        }
        
        public B GetBlock(Vector3Int pos) {
            int x = pos.x, y = pos.y, z = pos.z;
            
            if (x < 0) return ChunkNX?.Data == null ? default : ChunkNX.Data.GetBlock(x + Size.x, y, z);
            if (x >= Size.x) return ChunkPX?.Data == null ? default : ChunkPX.Data.GetBlock(x - Size.x,y,z);
            
            if (y < 0) return ChunkNY?.Data == null ? default : ChunkNY.Data.GetBlock(x, y + Size.y, z);
            if (y >= Size.y) return ChunkPY?.Data == null ? default : ChunkPY.Data.GetBlock(x,y - Size.y,z);
            
            if (z < 0) return ChunkNZ?.Data == null ? default : ChunkNZ.Data.GetBlock(x, y, z + Size.z);
            if (z >= Size.z) return ChunkPZ?.Data == null ? default : ChunkPZ.Data.GetBlock(x,y,z - Size.z);

            return Chunk.Data.GetBlock(x, y, z);
        }
        
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