using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Extensions;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Voxel.Engine.Test.TestBed;

using NUnit.Framework;

using UnityEditor;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Test.Runtime.Components {

    public class CompressionTest {
        
        private Vector3Int _size;
        private ChunkCompressor<TestBlock> _compressor;
        private INoiseProfile<TestBlock> _noiseProfile;

        [OneTimeSetUp]
        public void Setup() {
            // TODO : Create a fixture
            VoxelProvider<TestBlock>.Initialize(new TestVoxelProvider(), provider => {
                provider.Settings = AssetDatabase.LoadAssetAtPath<VoxelSettings>("Assets/Scripts/Vloxy/Tests/TestBed/TestVoxelSettings.asset");
            });

            _size = VoxelProvider<TestBlock>.Current.Settings.Chunk.ChunkSize;
            
            _noiseProfile = VoxelProvider<TestBlock>.Current.NoiseProfile();
            
            _compressor = VoxelProvider<TestBlock>.Current.ChunkCompressor();

            _noiseProfile.Generate();
        }

        [Test]
        public void ShouldCompressChunkData() {
            var blocks = new TestBlock[_size.Size()];

            for (int i = 0; i < _size.Size(); i++) {
                blocks[i] = new TestBlock(TestBlockType.Stone);
            }

            var actual = (List<byte>) ((CompressedChunkData<TestBlock>) _compressor.Compress(new DeCompressedChunkData<TestBlock>(blocks, _size))).GetData();
            
            var expected = new List<byte>();
            expected.Add((byte) TestBlockType.Stone);
            expected.AddRange(BitConverter.GetBytes(_size.Size()));
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void ShouldDeCompressChunkData() {
            var bytes = new List<byte>();
            bytes.Add((byte) TestBlockType.Stone);
            bytes.AddRange(BitConverter.GetBytes(_size.Size()));

            var actual = (TestBlock[]) ((DeCompressedChunkData<TestBlock>) _compressor.DeCompress(new CompressedChunkData<TestBlock>(bytes))).GetData();
            var expected = new TestBlock[_size.Size()];
            
            for (int i = 0; i < _size.Size(); i++) {
                expected[i] = new TestBlock(TestBlockType.Stone);
            }
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void ComplexDataCompression() {
            var blocks = _noiseProfile.Fill(Vector3Int.zero);
            
            var compressedData = (CompressedChunkData<TestBlock>) _compressor.Compress(blocks);

            var actual = (TestBlock[]) ((DeCompressedChunkData<TestBlock>) _compressor.DeCompress(compressedData)).GetData();
            var expected = (TestBlock[]) blocks.GetData();
            
            Assert.That(actual, Is.EquivalentTo(expected));
        }

    }

}