using System;
using System.Collections.Generic;

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
        private SimplePriorityQueue<int3> _Queue;

        private int3 _Focus;
        private int3 _ChunkSize;
        private int _ChunkStoreSize;

        internal ChunkManager(VloxySettings settings) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _ChunkStoreSize = (settings.Chunk.LoadDistance + 2).CubedSize();

            _Chunks = new Dictionary<int3, Chunk>(_ChunkStoreSize);
            _Queue = new SimplePriorityQueue<int3>();
        }

        #region API

        public bool SetBlock(Block block, Vector3Int position) {
            var chunk_pos = VloxyUtils.GetChunkCoords(position);
            var block_pos = VloxyUtils.GetBlockIndex(position);

            if (!_Chunks.ContainsKey(chunk_pos)) {
                VloxyLogger.Warn<ChunkManager>($"Chunk : {chunk_pos} not loaded");
                return false;
            }
            
            VloxyLogger.Info<ChunkManager>($"Setting block {block_pos} in chunk {chunk_pos}");

            _Chunks[chunk_pos].SetBlock(block_pos, VloxyUtils.GetBlockId(block));
            
            return true;
        }
        
        public int ChunkCount() => _Chunks.Count;
        public bool ContainsChunk(int3 position) => _Chunks.ContainsKey(position);

        #endregion
        
        internal void RemoveChunk(int3 position) => _Chunks.Remove(position);
        
        internal void Dispose() {
            foreach (var (_, chunk) in _Chunks) {
                chunk.Dispose();
            }
        }
        
        internal void FocusUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, 1.0f / (position - focus).SqrMagnitude());
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
                }
                
                _Chunks.Add(position, chunk);
                _Queue.Enqueue(position, 1.0f / (position - _Focus).SqrMagnitude());
                
                // for (var x = 6; x <= 10; x++) {
                //     for (var z = 6; z <= 10; z++) {
                //         for (var y = 6; y <= 10; y++) {
                //             var pos = new Vector3Int(position.x + x, position.y + y, position.z + z);
                //
                //             SetBlock(Block.STONE, pos);
                //         }
                //     }
                // }
            }
        }
        
        internal ChunkAccessor GetAccessor(List<int3> positions) {
            var slice = new NativeParallelHashMap<int3, Chunk>(
                positions.Count * 27, 
                Allocator.Persistent // TODO : Allocator cleanup, fit in the 4 frame limit
            );

            foreach (var position in positions) {
                for (var x = -1; x <= 1; x++) {
                    for (var z = -1; z <= 1; z++) {
                        for (var y = -1; y <= 1; y++) {
                            var pos = position + _ChunkSize.MemberMultiply(x,y,z);

                            if (!_Chunks.ContainsKey(pos)) {
                                // Anytime this exception is thrown, mesh building completely stops
                                throw new InvalidOperationException($"Chunk {pos} has not been generated");
                            }
                                
                            if (!slice.ContainsKey(pos)) slice.Add(pos, _Chunks[pos]);
                        }
                    }
                }
            }

            return new ChunkAccessor(slice, _ChunkSize);
        }

    }

}