using System.Linq;

using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace CodeBlaze.Vloxy.Engine.Unsafe {

    public struct UnsafeCompressedList {

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

        public UnsafeCompressedList(int capacity, Allocator allocator) {
            Internal = new UnsafeList<Node>(capacity, allocator);
            Length = 0;
        }

        public void Dispose() {
            Internal.Dispose();
        }

        public void AddNode(int id, int count) {
            Internal.Add(new Node(id, count));
            Length += count;
        }

        public int Get(int index) {
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
            return Internal.Aggregate(
                $"Length: {Length}, Compressed: {CompressedLength}, Elements", 
                (result, node) => $"{result} : [Data: {node.ID}, Count: {node.Count}]"
            );
        }

    }

}