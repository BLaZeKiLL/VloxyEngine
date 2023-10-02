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

            Assert.AreEqual(list.Length, compressed.Length);
            Assert.AreEqual(1, compressed.CompressedLength);

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
            
            Assert.AreEqual(1, compressed.Get(Random.Range(0, 5)));
            Assert.AreEqual(2, compressed.Get(Random.Range(5, 10)));
            Assert.AreEqual(3, compressed.Get(Random.Range(10, 15)));

            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldReturnLeftRightInSameNode() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            Assert.AreEqual(1, compressed.LeftOf(2));
            Assert.AreEqual(1, compressed.RightOf(2));
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldReturnLeftOfOnLeftBoundary() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            Assert.AreEqual(1, compressed.LeftOf(5));
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldReturnRightOfOnRightBoundary() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            Assert.AreEqual(2, compressed.RightOf(4));
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldReturnLeftRightOfSingeLengthNode() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(3, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            Assert.AreEqual(1, compressed.LeftOf(5));
            Assert.AreEqual(3, compressed.RightOf(5));
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSetRandomInterval() {
            var list = new NativeList<int>(16, Allocator.Temp);

            var a = Random.Range(1, 6);
            var b = Random.Range(1, 6);
            var c = Random.Range(1, 6);
            
            list.AddReplicate(1, a);
            list.AddReplicate(2, b);
            list.AddReplicate(3, c);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            var index = Random.Range(1, a + b + c);
            
            compressed.Set(index, 4);
            
            Assert.AreEqual(4, compressed.Get(index));
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_NoCoalesce_XYZ() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(3, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 4);
            
            Assert.AreEqual(4, compressed.Get(5));
            Assert.AreEqual(3, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }
        
        [Test]
        public void ShouldSet_NoCoalesce_XYX() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(3, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 4);
            
            Assert.AreEqual(4, compressed.Get(5));
            Assert.AreEqual(3, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }
        
        [Test]
        public void ShouldSet_NoCoalesce_XXX() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(1, 1);
            list.AddReplicate(1, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 4);
            
            Assert.AreEqual(4, compressed.Get(5));
            Assert.AreEqual(3, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }
        
        [Test]
        public void ShouldSet_NoCoalesce_XXY() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(1, 1);
            list.AddReplicate(2, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 4);
            
            Assert.AreEqual(4, compressed.Get(5));
            Assert.AreEqual(3, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }
        
        [Test]
        public void ShouldSet_NoCoalesce_XYY() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(2, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 4);
            
            Assert.AreEqual(4, compressed.Get(5));
            Assert.AreEqual(3, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_LeftCoalesce() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(3, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 1);
            
            Assert.AreEqual(1, compressed.Get(5));
            Assert.AreEqual(2, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_RightCoalesce() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(3, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);
            
            compressed.Set(5, 3);
            
            Assert.AreEqual(3, compressed.Get(5));
            Assert.AreEqual(2, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_BothCoalesce() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(1, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            compressed.Set(5, 1);
            
            Assert.AreEqual(1, compressed.Get(5));
            Assert.AreEqual(1, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_InLastInterval() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(1, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            compressed.Set(9, 5);
            
            Assert.AreEqual(5, compressed.Get(9));
            Assert.AreEqual(5, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        }

        [Test]
        public void ShouldSet_InFirstInterval() {
            var list = new NativeList<int>(16, Allocator.Temp);
            
            list.AddReplicate(1, 5);
            list.AddReplicate(2, 1);
            list.AddReplicate(1, 5);

            var compressed = new UnsafeIntervalList(list, 16, Allocator.Temp);

            compressed.Set(1, 5);
            
            Assert.AreEqual(5, compressed.Get(1));
            Assert.AreEqual(5, compressed.CompressedLength);
            
            list.Dispose();
            compressed.Dispose();
        } 
    }

}