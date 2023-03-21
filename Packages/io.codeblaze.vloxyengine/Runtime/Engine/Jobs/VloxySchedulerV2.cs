using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Jobs.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Priority_Queue;

using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs {

    public class VloxySchedulerV2 {

        private readonly MeshBuildSchedulerV2 _MeshBuildScheduler;
        private readonly ChunkDataSchedulerV2 _ChunkDataScheduler;

        private readonly SimplePriorityQueue<int3> _ViewQueue;
        private readonly SimplePriorityQueue<int3> _DataQueue;

        private readonly VloxySettings _Settings;

        public VloxySchedulerV2(
            VloxySettings settings, 
            MeshBuildSchedulerV2 meshBuildScheduler,
            ChunkDataSchedulerV2 chunkDataScheduler
        ) {
            _MeshBuildScheduler = meshBuildScheduler;
            _ChunkDataScheduler = chunkDataScheduler;

            _ViewQueue = new SimplePriorityQueue<int3>();
            _DataQueue = new SimplePriorityQueue<int3>();

            _Settings = settings;
        }

        internal void FocusUpdate(int3 focus) {
            var distance = _Settings.Chunk.LoadDistance;
            var draw = _Settings.Chunk.DrawDistance;

            for (int x = -distance; x <= distance; x++) {
                for (int z = -distance; z <= distance; z++) {
                    for (int y = -distance; y <= distance; y++) {
                        var pos = focus + (_Settings.Chunk.ChunkSize.MemberMultiply(x, y, z));

                        if (
                            (x >= -draw && x <= draw) &&
                            (y >= -draw && y <= draw) &&
                            (z >= -draw && z <= draw)
                        ) {
                            if (_ViewQueue.Contains(pos)) {
                                _ViewQueue.UpdatePriority(pos, 1.0f / (pos - focus).SqrMagnitude());
                            } else {
                                _ViewQueue.Enqueue(pos, 1.0f / (pos - focus).SqrMagnitude());
                            }
                        }

                        if (_DataQueue.Contains(pos)) {
                            _DataQueue.UpdatePriority(pos, 1.0f / (pos - focus).SqrMagnitude());
                        } else {
                            _DataQueue.Enqueue(pos, 1.0f / (pos - focus).SqrMagnitude());
                        }
                    }
                }
            }
        }

        internal void SchedulerUpdate() {
            if (_DataQueue.Count > 0 && _ChunkDataScheduler.IsReady) {
                var count = math.min(_Settings.Scheduler.StreamingBatchSize, _DataQueue.Count);

                var jobs = new List<int3>(count);

                for (int i = 0; i < count; i++) {
                    jobs.Add(_DataQueue.Dequeue());
                }
                
                _ChunkDataScheduler.Start(jobs);
            }  
            
            if (_ViewQueue.Count > 0 && _MeshBuildScheduler.IsReady) {
                var count = math.min(_Settings.Scheduler.MeshingBatchSize, _ViewQueue.Count);

                var jobs = new List<int3>(count);

                for (int i = 0; i < count; i++) {
                    jobs.Add(_ViewQueue.Dequeue());
                }

                _MeshBuildScheduler.Start(jobs);
            }
        }

        internal void SchedulerLateUpdate() {
            if (_MeshBuildScheduler.IsComplete) {
                _MeshBuildScheduler.Complete();

                // Safe to modify store here as no one is using it here
                // This can completely be non-blocking if mesh build works on
                // slice of store which contains only the chunks it requires
                _ChunkDataScheduler.SyncChunkStore();
            } 
            
            if (_ChunkDataScheduler.IsComplete) {
                _ChunkDataScheduler.Complete();
            }
        }

        internal void Dispose() {
            _ChunkDataScheduler.Dispose();
            _MeshBuildScheduler.Dispose();
        }

    }

}