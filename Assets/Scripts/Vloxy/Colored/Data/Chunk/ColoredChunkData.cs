using System.Collections.Generic;

using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Data.Chunk {

    public class ColoredChunkData : CompressibleChunkData<ColoredBlock> {

        public ColoredChunkData(ColoredBlock[] data, int dataSize, Vector3Int chunkSize) : base(data, dataSize, chunkSize) { }

        public ColoredChunkData(List<byte> bytes, int dataSize, Vector3Int chunkSize) : base(bytes, dataSize, chunkSize) { }

        protected override ColoredBlock FromBytes(byte[] bytes) => new ColoredBlock(bytes);

        protected override byte[] GetBytes(ColoredBlock obj) => obj.GetBytes();

    }

}