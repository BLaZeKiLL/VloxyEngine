using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Test.TestBed {

    public class TestCompressor  : ChunkCompressor<TestBlock> {

        public TestCompressor(int blockSize, Vector3Int chunkSize) : base(blockSize, chunkSize) { }

        protected override TestBlock GetBlock(List<byte> bytes) => new TestBlock(bytes);

    }

}