using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Priority_Queue;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs {
    
    public class VloxySchedulerV2 {
        
        private readonly MeshBuildSchedulerV2 _MeshBuildScheduler;
        private readonly ChunkDataSchedulerV2 _ChunkDataScheduler;

        private readonly ChunkStore _ChunkStore;
        private readonly ChunkPoolV2 _ChunkPool;

        private readonly SimplePriorityQueue<int3> _ViewQueue;
        private readonly SimplePriorityQueue<int3> _DataQueue;

        private readonly HashSet<int3> _ViewSet;
        private readonly HashSet<int3> _DataSet;

        private readonly VloxySettings _Settings;

        public VloxySchedulerV2(
            VloxySettings settings, 
            MeshBuildSchedulerV2 meshBuildScheduler,
            ChunkDataSchedulerV2 chunkDataScheduler,
            ChunkStore chunkStore,
            ChunkPoolV2 chunkPool
        ) {
            _MeshBuildScheduler = meshBuildScheduler;
            _ChunkDataScheduler = chunkDataScheduler;

            _ChunkStore = chunkStore;
            _ChunkPool = chunkPool;

            _ViewQueue = new SimplePriorityQueue<int3>();
            _DataQueue = new SimplePriorityQueue<int3>();

            _ViewSet = new HashSet<int3>();
            _DataSet = new HashSet<int3>();

            _Settings = settings;
        }

        internal void FocusUpdate(int3 focus) {
            var distance = _Settings.Chunk.LoadDistance;
            var draw = _Settings.Chunk.DrawDistance;

            for (int x = -distance; x <= distance; x++) {
                for (int z = -distance; z <= distance; z++) {
                    for (int y = -distance; y <= distance; y++) {
                        var pos = focus + _Settings.Chunk.ChunkSize.MemberMultiply(x, y, z);

                        if (
                            (x >= -draw && x <= draw) &&
                            (y >= -draw && y <= draw) &&
                            (z >= -draw && z <= draw)
                        ) {
                            if (_ViewQueue.Contains(pos)) {
                                _ViewQueue.UpdatePriority(pos, (pos - focus).SqrMagnitude());
                            } else if (ShouldScheduleForMeshing(pos) && IsChunkGenerated(pos)) {
                                _ViewQueue.Enqueue(pos, (pos - focus).SqrMagnitude());
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
            
            // TODO : We can merge the bellow updates in the above loops
            _ChunkStore.ViewUpdate(focus);
            _ChunkPool.ViewUpdate(focus);
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
                    _ViewSet.Add(_ViewQueue.Dequeue());
                }

                _MeshBuildScheduler.Start(_ViewSet.ToList());
            }
        }

        internal void SchedulerLateUpdate() {
            if (_ChunkDataScheduler.IsComplete && !_ChunkDataScheduler.IsReady) {
                _ChunkDataScheduler.Complete();
                _DataSet.Clear();
                
#if VLOXY_LOGGING
                VloxyLogger.Info<VloxySchedulerV2>($"Streaming Avg Batch Time : {_ChunkDataScheduler.AvgTime:F3} MS");
#endif
            }
            
            if (_MeshBuildScheduler.IsComplete && !_MeshBuildScheduler.IsReady) {
                _MeshBuildScheduler.Complete();
                _ViewSet.Clear();
                
#if VLOXY_LOGGING
                VloxyLogger.Info<VloxySchedulerV2>($"Meshing Avg Batch Time : {_MeshBuildScheduler.AvgTime:F3} MS");
#endif
            }
        }

        internal void Dispose() {
            _ChunkDataScheduler.Dispose();
            _MeshBuildScheduler.Dispose();
        }

        private bool ShouldScheduleForGenerating(int3 position) => !(_ChunkStore.ContainsChunk(position) || _DataSet.Contains(position));
        private bool ShouldScheduleForMeshing(int3 position) => !(_ChunkPool.IsActive(position) || _ViewSet.Contains(position));

        private bool IsChunkGenerated(int3 position) {
            var result = true;
            
            for (int x = -1; x <= 1; x++) {
                for (int z = -1; z <= 1; z++) {
                    for (int y = -1; y <= 1; y++) {
                        var pos = position + _Settings.Chunk.ChunkSize.MemberMultiply(x, y, z);
                        result &= _ChunkStore.ContainsChunk(pos);
                    }
                }
            }

            return result;
        }

    }

}