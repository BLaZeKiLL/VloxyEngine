using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Priority_Queue;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        public NativeParallelHashMap<int3, Chunk> Chunks { get; }

        private SimplePriorityQueue<int3> _Queue;
        private Dictionary<int3, Chunk> _SyncChunks;

        private int3 _Focus;
        private int _StreamRegionSize;

        public ChunkStore(VloxySettings settings) {
            _StreamRegionSize = settings.Chunk.LoadDistance.CubedSize();

            Chunks = new NativeParallelHashMap<int3, Chunk>(
                _StreamRegionSize, 
                Allocator.Persistent
            );

            _Queue = new SimplePriorityQueue<int3>();
            _SyncChunks = new Dictionary<int3, Chunk>();
        }

        public int ChunkCount() => Chunks.Count();

        public bool ContainsChunk(int3 position) => Chunks.ContainsKey(position) || _SyncChunks.ContainsKey(position);

        public void RemoveChunk(int3 position) => Chunks.Remove(position);
        
        internal void Dispose() {
            foreach (var pair in Chunks) {
                pair.Value.Data.Dispose();
            }
            
            Chunks.Dispose();
        }
        
        internal void ViewUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, (position - _Focus).SqrMagnitude());
            }
        }

        internal void AddChunk(Chunk chunk) {
            if (!_SyncChunks.ContainsKey(chunk.Position) && !Chunks.ContainsKey(chunk.Position)) {
                _SyncChunks.Add(chunk.Position, chunk);
            } else {
                throw new InvalidOperationException($"Chunk {chunk.Position} already exists");
            }
        }
        
        internal void Sync() {
            foreach (var (position, chunk) in _SyncChunks) {
                if (_Queue.Count >= _StreamRegionSize) {
                    Chunks.Remove(_Queue.Dequeue());
                }
                
                Chunks.Add(position, chunk);
                _Queue.Enqueue(position, 1.0f / (position - _Focus).SqrMagnitude());
            }
            
            _SyncChunks.Clear();
        }

    }

}