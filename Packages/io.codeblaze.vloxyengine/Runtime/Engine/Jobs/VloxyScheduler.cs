using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Jobs.Chunk;
using CodeBlaze.Vloxy.Engine.Jobs.Collider;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Priority_Queue;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs {
    
    public class VloxyScheduler {
        
        private readonly MeshBuildScheduler _MeshBuildScheduler;
        private readonly ChunkDataScheduler _ChunkDataScheduler;
        private readonly ColliderBuildScheduler _ColliderBuildScheduler;

        private readonly ChunkStore _ChunkStore;
        private readonly ChunkPool _ChunkPool;

        private readonly SimplePriorityQueue<int3> _ViewQueue;
        private readonly SimplePriorityQueue<int3> _DataQueue;
        private readonly SimplePriorityQueue<int3> _ColliderQueue;

        private readonly HashSet<int3> _ViewSet;
        private readonly HashSet<int3> _DataSet;
        private readonly HashSet<int3> _ColliderSet;

        private readonly VloxySettings _Settings;

        internal VloxyScheduler(
            VloxySettings settings, 
            MeshBuildScheduler meshBuildScheduler,
            ChunkDataScheduler chunkDataScheduler,
            ColliderBuildScheduler colliderBuildScheduler,
            ChunkStore chunkStore,
            ChunkPool chunkPool
        ) {
            _MeshBuildScheduler = meshBuildScheduler;
            _ChunkDataScheduler = chunkDataScheduler;
            _ColliderBuildScheduler = colliderBuildScheduler;

            _ChunkStore = chunkStore;
            _ChunkPool = chunkPool;

            _ViewQueue = new SimplePriorityQueue<int3>();
            _DataQueue = new SimplePriorityQueue<int3>();
            _ColliderQueue = new SimplePriorityQueue<int3>();

            _ViewSet = new HashSet<int3>();
            _DataSet = new HashSet<int3>();
            _ColliderSet = new HashSet<int3>();

            _Settings = settings;
        }

        internal void FocusUpdate(int3 focus) {
            var load = _Settings.Chunk.LoadDistance;
            var draw = _Settings.Chunk.DrawDistance;
            var update = _Settings.Chunk.UpdateDistance;

            for (int x = -load; x <= load; x++) {
                for (int z = -load; z <= load; z++) {
                    for (int y = -load; y <= load; y++) {
                        var pos = focus + _Settings.Chunk.ChunkSize.MemberMultiply(x, y, z);

                        if (
                            (x >= -draw && x <= draw) &&
                            (y >= -draw && y <= draw) &&
                            (z >= -draw && z <= draw)
                        ) {
                            if (_ViewQueue.Contains(pos)) {
                                _ViewQueue.UpdatePriority(pos, (pos - focus).SqrMagnitude());
                            } else if (ShouldScheduleForMeshing(pos) && CanGenerateMeshForChunk(pos)) {
                                _ViewQueue.Enqueue(pos, (pos - focus).SqrMagnitude());
                            }
                        }
                        
                        if (
                            (x >= -update && x <= update) &&
                            (y >= -update && y <= update) &&
                            (z >= -update && z <= update)
                        ) {
                            if (_ColliderQueue.Contains(pos)) {
                                _ColliderQueue.UpdatePriority(pos, (pos - focus).SqrMagnitude());
                            } else if (ShouldScheduleForBaking(pos) && CanBakeColliderForChunk(pos)) {
                                _ColliderQueue.Enqueue(pos, (pos - focus).SqrMagnitude());
                            }
                        }

                        if (_DataQueue.Contains(pos)) {
                            _DataQueue.UpdatePriority(pos, (pos - focus).SqrMagnitude());
                        } else if (ShouldScheduleForGenerating(pos)) {
                            _DataQueue.Enqueue(pos, (pos - focus).SqrMagnitude());
                        }
                    }
                }
            }
            
            _ChunkStore.FocusUpdate(focus);
            _ChunkPool.FocusUpdate(focus);
        }

        internal void SchedulerUpdate() {
            if (_DataQueue.Count > 0 && _ChunkDataScheduler.IsReady) {
                var count = math.min(_Settings.Scheduler.StreamingBatchSize, _DataQueue.Count);
                
                for (int i = 0; i < count; i++) {
                    _DataSet.Add(_DataQueue.Dequeue());
                }
                
                _ChunkDataScheduler.Start(_DataSet.ToList());
            }  
            
            if (_ViewQueue.Count > 0 && _MeshBuildScheduler.IsReady) {
                var count = math.min(_Settings.Scheduler.MeshingBatchSize, _ViewQueue.Count);
                
                for (int i = 0; i < count; i++) {
                    var chunk = _ViewQueue.Dequeue();
                    
                    // The chunk may be removed from memory by the time we schedule,
                    // Should we check this only here ?
                    if (CanGenerateMeshForChunk(chunk)) _ViewSet.Add(chunk);
                }

                _MeshBuildScheduler.Start(_ViewSet.ToList());
            }

            if (_ColliderQueue.Count > 0 && _ColliderBuildScheduler.IsReady) {
                var count = math.min(_Settings.Scheduler.ColliderBatchSize, _ColliderQueue.Count);

                for (int i = 0; i < count; i++) {
                    var position = _ColliderQueue.Dequeue();

                    if (CanBakeColliderForChunk(position)) _ColliderSet.Add(position);
                }
                
                _ColliderBuildScheduler.Start(_ColliderSet.ToList());
            }
        }

        internal void SchedulerLateUpdate() {
            if (_ChunkDataScheduler.IsComplete && !_ChunkDataScheduler.IsReady) {
                _ChunkDataScheduler.Complete();
                _DataSet.Clear();
            }
            
            if (_MeshBuildScheduler.IsComplete && !_MeshBuildScheduler.IsReady) {
                _MeshBuildScheduler.Complete();
                _ViewSet.Clear();
            }

            if (_ColliderBuildScheduler.IsComplete && !_ColliderBuildScheduler.IsReady) {
                _ColliderBuildScheduler.Complete();
                _ColliderSet.Clear();
            }
        }

        internal void Dispose() {
            _ChunkDataScheduler.Dispose();
            _MeshBuildScheduler.Dispose();
            _ColliderBuildScheduler.Dispose();
        }

        private bool ShouldScheduleForGenerating(int3 position) => !(_ChunkStore.ContainsChunk(position) || _DataSet.Contains(position));
        private bool ShouldScheduleForMeshing(int3 position) => !(_ChunkPool.IsActive(position) || _ViewSet.Contains(position));

        private bool ShouldScheduleForBaking(int3 position) =>
            !(_ChunkPool.IsCollidable(position) || _ColliderSet.Contains(position));

        /// <summary>
        /// Checks if the specified chunks and it's neighbours are generated
        /// </summary>
        /// <param name="position">Position of chunk to check</param>
        /// <returns>Is it ready to be meshed</returns>
        private bool CanGenerateMeshForChunk(int3 position) {
            var result = true;
            
            for (var x = -1; x <= 1; x++) {
                for (var z = -1; z <= 1; z++) {
                    for (var y = -1; y <= 1; y++) {
                        var pos = position + _Settings.Chunk.ChunkSize.MemberMultiply(x, y, z);
                        result &= _ChunkStore.ContainsChunk(pos);
                    }
                }
            }

            return result;
        }

        private bool CanBakeColliderForChunk(int3 position) => _ChunkPool.IsActive(position);

        #region RuntimeStatsAPI

        public float DataAvgTiming => _ChunkDataScheduler.AvgTime;
        public float MeshAvgTiming => _MeshBuildScheduler.AvgTime;
        public float BakeAvgTiming => _ColliderBuildScheduler.AvgTime;

        public int DataQueueCount => _DataQueue.Count;
        public int MeshQueueCount => _ViewQueue.Count;
        public int BakeQueueCount => _ColliderQueue.Count;

        #endregion

    }

}