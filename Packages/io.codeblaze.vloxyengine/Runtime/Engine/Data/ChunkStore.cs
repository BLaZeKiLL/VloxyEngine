using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Priority_Queue;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Data {

    public class ChunkStore {

        public Dictionary<int3, Chunk> Chunks { get; }

        private SimplePriorityQueue<int3> _Queue;

        private int3 _Focus;
        private int3 _ChunkSize;
        private int _StreamRegionSize;

        public ChunkStore(VloxySettings settings) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _StreamRegionSize = settings.Chunk.LoadDistance.CubedSize();

            Chunks = new Dictionary<int3, Chunk>(_StreamRegionSize);
            _Queue = new SimplePriorityQueue<int3>();
        }

        public int ChunkCount() => Chunks.Count;

        public bool ContainsChunk(int3 position) => Chunks.ContainsKey(position);

        public void RemoveChunk(int3 position) => Chunks.Remove(position);
        
        internal void Dispose() {
            foreach (var pair in Chunks) {
                pair.Value.Data.Dispose();
            }
        }
        
        internal void ViewUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, 1.0f / (position - _Focus).SqrMagnitude());
            }
        }

        internal void AddChunks(NativeParallelHashMap<int3, Chunk> chunks) {
            VloxyLogger.Info<ChunkStore>($"Adding {chunks.Count()} chunks");
            foreach (var pair in chunks) {
                var position = pair.Key;
                var chunk = pair.Value;

                if (Chunks.ContainsKey(chunk.Position)) {
                    throw new InvalidOperationException($"Chunk {position} already exists");
                }
                
                if (_Queue.Count >= _StreamRegionSize) {
                    Chunks.Remove(_Queue.Dequeue());
                }
                
                Chunks.Add(position, chunk);
                _Queue.Enqueue(position, 1.0f / (position - _Focus).SqrMagnitude());
            }
        }
        
        internal ChunkAccessor GetAccessor(List<int3> positions) {
            var chunks = new NativeParallelHashMap<int3, Chunk>(
                positions.Count * 27, 
                Allocator.Persistent // TODO : Allocator cleanup, fit in the 4 frame limit
            );

            foreach (var position in positions) {
                for (int x = -1; x <= 1; x++) {
                    for (int z = -1; z <= 1; z++) {
                        for (int y = -1; y <= 1; y++) {
                            var pos = position + _ChunkSize.MemberMultiply(x,y,z);

                            if (!Chunks.ContainsKey(pos)) {
                                throw new InvalidOperationException($"Chunk {pos} has not been generated");
                            }
                                
                            if (!chunks.ContainsKey(pos)) chunks.Add(pos, Chunks[pos]);
                        }
                    }
                }
            }

            return new ChunkAccessor(chunks, _ChunkSize);
        }

    }

}