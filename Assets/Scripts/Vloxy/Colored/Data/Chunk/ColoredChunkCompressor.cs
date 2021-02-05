using System.Collections.Generic;

using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Engine.Components;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Data.Chunk {

    public class ColoredChunkCompressor : ChunkCompressor<ColoredBlock> {

        public ColoredChunkCompressor(int blockSize, Vector3Int chunkSize) : base(blockSize, chunkSize) { }

        protected override ColoredBlock GetBlock(List<byte> bytes) => new ColoredBlock(bytes);

    }

}