using System.Collections.Generic;

namespace CodeBlaze.Vloxy.Engine.Jobs {

    public class VloxyBatch<T> {

        private Queue<T> _Queue;

        public VloxyBatch() {
            _Queue = new Queue<T>();
        }
        
        public VloxyBatch(int capacity) {
            _Queue = new Queue<T>(capacity);
        }

        public int Count => _Queue.Count;

        public void Enqueue(T item) => _Queue.Enqueue(item);

        public T Dequeue() => _Queue.Dequeue();

    }

}