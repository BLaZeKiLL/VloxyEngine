using System;
using System.Collections.Generic;
using System.Linq;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;
using Priority_Queue;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkManager {

        private Dictionary<int3, Chunk> _Chunks;
        private SimpleFastPriorityQueue<int3, int> _Queue;
        private NativeParallelHashMap<int3, Chunk> _AccessorMap;

        private HashSet<int3> _ReMeshChunks;
        private HashSet<int3> _ReCollideChunks;

        private int3 _Focus;
        private int3 _ChunkSize;
        private int _ChunkStoreSize;

        internal ChunkManager(VloxySettings settings) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _ChunkStoreSize = (settings.Chunk.LoadDistance + 2).CubedSize();

            _ReMeshChunks = new HashSet<int3>();
            _ReCollideChunks = new HashSet<int3>();
            
            _Chunks = new Dictionary<int3, Chunk>(_ChunkStoreSize);
            _Queue = new SimpleFastPriorityQueue<int3, int>();
            
            _AccessorMap = new NativeParallelHashMap<int3, Chunk>(
                settings.Scheduler.MeshingBatchSize * 27, 
                Allocator.Persistent
            );
        }

        #region API

        /// <summary>
        /// Set a block at a position
        /// </summary>
        /// <param name="block">Block Type</param>
        /// <param name="position">World Position</param>
        /// <param name="remesh">Regenerate Mesh and Collider ?</param>
        /// <returns>Operation Success</returns>
        public bool SetBlock(Block block, Vector3Int position, bool remesh = true) {
            var chunk_pos = VloxyUtils.GetChunkCoords(position);
            var block_pos = VloxyUtils.GetBlockIndex(position);

            if (!_Chunks.ContainsKey(chunk_pos)) {
                VloxyLogger.Warn<ChunkManager>($"Chunk : {chunk_pos} not loaded");
                return false;
            }

            var chunk = _Chunks[chunk_pos];
            
            var result = chunk.SetBlock(block_pos, VloxyUtils.GetBlockId(block));

            _Chunks[chunk_pos] = chunk;

            if (remesh && result) ReMeshChunks(position.Int3());
            
            return true;
        }

        public int ChunkCount() => _Chunks.Count;
        public bool ContainsChunk(int3 position) => _Chunks.ContainsKey(position);

        #endregion
        
        internal bool ShouldReMesh(int3 position) => _ReMeshChunks.Contains(position);
        internal bool ShouldReCollide(int3 position) => _ReCollideChunks.Contains(position);
        internal void RemoveChunk(int3 position) => _Chunks.Remove(position);
        
        internal void Dispose() {
            _AccessorMap.Dispose();
            
            foreach (var (_, chunk) in _Chunks) {
                chunk.Dispose();
            }
        }
        
        internal void FocusUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, -(position - focus).SqrMagnitude());
            }
        }

        internal void AddChunks(NativeParallelHashMap<int3, Chunk> chunks) {
            foreach (var pair in chunks) {
                var position = pair.Key;
                var chunk = pair.Value;

                if (_Chunks.ContainsKey(chunk.Position)) {
                    throw new InvalidOperationException($"Chunk {position} already exists");
                }
                
                if (_Queue.Count >= _ChunkStoreSize) {
                    _Chunks.Remove(_Queue.Dequeue());
                    // if dirty save chunk
                }
                
                _Chunks.Add(position, chunk);
                _Queue.Enqueue(position, -(position - _Focus).SqrMagnitude());
            }
        }

        internal ChunkAccessor GetAccessor(List<int3> positions) {
            _AccessorMap.Clear();
            
            foreach (var position in positions) {
                for (var x = -1; x <= 1; x++) {
                    for (var z = -1; z <= 1; z++) {
                        for (var y = -1; y <= 1; y++) {
                            var pos = position + _ChunkSize.MemberMultiply(x,y,z);

                            if (!_Chunks.ContainsKey(pos)) {
                                // Anytime this exception is thrown, mesh building completely stops
                                throw new InvalidOperationException($"Chunk {pos} has not been generated");
                            }
                                
                            if (!_AccessorMap.ContainsKey(pos)) _AccessorMap.Add(pos, _Chunks[pos]);
                        }
                    }
                }
            }

            return new ChunkAccessor(_AccessorMap.AsReadOnly(), _ChunkSize);
        }

        internal bool ReMeshedChunk(int3 position) {
            if (!_ReMeshChunks.Contains(position)) return false;
            
            _ReMeshChunks.Remove(position);
            _ReCollideChunks.Add(position);

            return true;
        }

        internal bool ReCollideChunk(int3 position) {
            if (!_ReCollideChunks.Contains(position)) return false;
            
            _ReCollideChunks.Remove(position);

            return true;
        }
        
        private void ReMeshChunks(int3 block_position) {
            foreach (var dir in VloxyUtils.Directions) {
                _ReMeshChunks.Add(VloxyUtils.GetChunkCoords(block_position + dir));
            }
        }
    }

}