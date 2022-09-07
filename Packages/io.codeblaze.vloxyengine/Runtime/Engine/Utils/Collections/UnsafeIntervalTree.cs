using System;
using System.Text;

using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace CodeBlaze.Vloxy.Engine.Utils.Collections {

    [BurstCompile]
    public struct UnsafeIntervalTree {

        private struct Node {
            public int ID;
            public int Count;

            public Node(int id, int count) {
                ID = id;
                Count = count;
            }

        }

        private UnsafeList<Node> Internal;

        public int Length;

        public int CompressedLength => Internal.Length;

        public UnsafeIntervalTree(int capacity, Allocator allocator) {
            Internal = new UnsafeList<Node>(capacity, allocator);
            Length = 0;
        }

        public void Dispose() {
            Internal.Dispose();
        }

        public void AddNode(int id, int count) {
            Length += count;
            Internal.Add(new Node(id, Length));
        }

        public int Get(int index) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (index >= Length) throw new IndexOutOfRangeException($"{index} is out of range for the given data of length {Length}");
#endif

            return Internal[BinarySearch(index)].ID;
        }

        public void Set(int index, int id) {
            // TODO : To Implement (REF: https://github.com/mikolalysenko/NodeMinecraftThing/blob/master/client/voxels.js)
        }

        private int BinarySearch(int index) {
            int min = 0;
            int max = Internal.Length;

            while (min <= max) {
                int mid = (max + min) / 2;
                int count = Internal[mid].Count;

                if (index == count) return mid + 1;
                
                if (index < count) max = mid - 1;
                else min = mid + 1;
            }

            return min;
        }

        public override string ToString() {
            var sb = new StringBuilder($"Length: {Length}, Compressed: {CompressedLength}\n");
            
            foreach (var node in Internal) {
                sb.AppendLine($"[Data: {node.ID}, Count: {node.Count}]");
            }

            return sb.ToString();
        }

    }

}