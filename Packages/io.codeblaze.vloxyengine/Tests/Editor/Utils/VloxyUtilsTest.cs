using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Tests.Editor.Tests.Editor.TestBed;
using CodeBlaze.Vloxy.Engine.Utils;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Tests.Editor.Tests.Editor.Utils {

    public class VloxyUtilsTest {

        [OneTimeSetUp]
        public void Setup() {
            TestVloxyProvider.Initialize(TestVloxyProvider.Create(), provider => {
                provider.Settings.Chunk.ChunkSize = new int3(32, 32, 32);
            });
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralPositiveFirst() {
            var position = new int3(1, 32 , 1);
            
            Assert.AreEqual(new int3(0, 32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralPositive() {
            var position = new int3(1, 35 , 1);
            
            Assert.AreEqual(new int3(0, 32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralPositiveLast() {
            var position = new int3(1, 63 , 1);
            
            Assert.AreEqual(new int3(0, 32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialPositiveFirst() {
            var position = new int3(1, 0 , 1);
            
            Assert.AreEqual(new int3(0, 0, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialPositive() {
            var position = new int3(1, 5 , 1);
            
            Assert.AreEqual(new int3(0, 0, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialPositiveLast() {
            var position = new int3(1, 31 , 1);
            
            Assert.AreEqual(new int3(0, 0, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialNegativeFirst() {
            var position = new int3(1, -1 , 1);
            
            Assert.AreEqual(new int3(0, -32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialNegative() {
            var position = new int3(1, -5 , 1);
            
            Assert.AreEqual(new int3(0, -32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_SpecialNegativeLast() {
            var position = new int3(1, -32 , 1);
            
            Assert.AreEqual(new int3(0, -32, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralNegativeFirst() {
            var position = new int3(1, -33 , 1);
            
            Assert.AreEqual(new int3(0, -64, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralNegative() {
            var position = new int3(1, -35 , 1);
            
            Assert.AreEqual(new int3(0, -64, 0), VloxyUtils.GetChunkCoords(position));
        }
        
        [Test]
        public void ShouldConvertWorldCoordsToChunkCoords_GeneralNegativeLast() {
            var position = new int3(1, -64 , 1);
            
            Assert.AreEqual(new int3(0, -64, 0), VloxyUtils.GetChunkCoords(position));
        }

    }

}