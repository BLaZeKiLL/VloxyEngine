using CodeBlaze.Vloxy.Engine.Utils.Collections;
using Unity.Collections;
using NUnit.Framework;
using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Tests.Editor.Utils.Collections {

    public class UnsafeIntervalListTest {

        [Test]
        public void ShouldCompressTo1() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            
            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            Assert.AreEqual(compressed.Length, list.Length);
            Assert.AreEqual(compressed.CompressedLength, 1);

            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldGetCorrectValue() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 5);
            list.AddReplicate(3, 5);
            
            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            Assert.AreEqual(compressed.CompressedLength, 3);
            
            Assert.AreEqual(compressed.Get(Random.Range(0, 5)), 1);
            Assert.AreEqual(compressed.Get(Random.Range(5, 10)), 2);
            Assert.AreEqual(compressed.Get(Random.Range(10, 15)), 3);

            list.Dispose();
            compressed.Dispose();
        }
        
        [Test]
        public void ShouldSetRandomInterval() { }
        
        [Test]
        public void ShouldSetInterval_NoCoalesce() { }
        
        [Test]
        public void ShouldSetInterval_LeftCoalesce() { }
        
        [Test]
        public void ShouldSetInterval_RightCoalesce() { }
        
        [Test]
        public void ShouldSetInterval_BothCoalesce() { }
    }

}