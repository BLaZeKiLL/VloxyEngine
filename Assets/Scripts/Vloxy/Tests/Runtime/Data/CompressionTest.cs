using System;
using System.Collections;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Voxel.Engine.Test.TestBed;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

namespace CodeBlaze.Voxel.Engine.Test.Runtime.Data {

    internal class TestCompressor : ChunkCompressor<TestBlock> {

        public TestCompressor(int blockSize, Vector3Int chunkSize) : base(blockSize, chunkSize) { }

        protected override TestBlock GetBlock(List<byte> bytes) => new TestBlock((TestBlockType) bytes[0]);

    }
    
    public class CompressionTest {

        private ChunkCompressor<TestBlock> _compressor;
        private Vector3Int _size = Vector3Int.one * 8;

        [SetUp]
        public void Setup() {
            _compressor = new TestCompressor(1, _size);
        }
        
        [Test]
        public void ShouldCompressChunkData() {
            var blocks = new TestBlock[_size.x * _size.y * _size.z];

            for (int i = 0; i < _size.x * _size.y * _size.z; i++) {
                blocks[i] = new TestBlock(TestBlockType.Stone);
            }

            var actual = (List<byte>) ((CompressedChunkData<TestBlock>) _compressor.Compress(new DeCompressedChunkData<TestBlock>(blocks, _size))).GetData();
            var expected = new List<byte>();
            expected.Add((byte) TestBlockType.Stone);
            expected.AddRange(BitConverter.GetBytes(_size.x * _size.y * _size.z));
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void ShouldDeCompressChunkData() {
            var bytes = new List<byte>();
            bytes.Add((byte) TestBlockType.Stone);
            bytes.AddRange(BitConverter.GetBytes(_size.x * _size.y * _size.z));

            var actual = (TestBlock[]) ((DeCompressedChunkData<TestBlock>) _compressor.DeCompress(new CompressedChunkData<TestBlock>(bytes))).GetData();
            var expected = new TestBlock[_size.x * _size.y * _size.z];
            
            for (int i = 0; i < _size.x * _size.y * _size.z; i++) {
                expected[i] = new TestBlock(TestBlockType.Stone);
            }
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

    }

}