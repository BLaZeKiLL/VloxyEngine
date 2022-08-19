using System;

using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Jobs.Data;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

namespace CodeBlaze.Vloxy.Engine.Jobs {

    public class VloxyScheduler {

        public enum SchedulerState {

            IDLE,
            MESHING,
            STREAMING

        }
        
        public SchedulerState State { get; private set; }

        private readonly MeshBuildScheduler _MeshBuildScheduler;
        private readonly ChunkDataScheduler _ChunkDataScheduler;

        public VloxyScheduler(MeshBuildScheduler meshBuildScheduler, ChunkDataScheduler chunkDataScheduler) {
            _MeshBuildScheduler = meshBuildScheduler;
            _ChunkDataScheduler = chunkDataScheduler;

            State = SchedulerState.IDLE;
        }

        internal void Update() {
            if (State == SchedulerState.IDLE) {
                if (_MeshBuildScheduler.Processing) {
                    State = SchedulerState.MESHING;
                } else if (_ChunkDataScheduler.Processing) {
                    State = SchedulerState.STREAMING;
                }
            }

            switch (State) {
                case SchedulerState.IDLE:
                    break;
                case SchedulerState.MESHING:
                    _MeshBuildScheduler.Update();
                    break;
                case SchedulerState.STREAMING:
                    _ChunkDataScheduler.Update();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void LateUpdate() {
            switch (State) {
                case SchedulerState.IDLE:
                    break;
                case SchedulerState.MESHING:
                    _MeshBuildScheduler.LateUpdate();
                    break;
                case SchedulerState.STREAMING:
                    _ChunkDataScheduler.LateUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (State == SchedulerState.MESHING && !_MeshBuildScheduler.Processing) {
                State = SchedulerState.IDLE;
            } else if (State == SchedulerState.STREAMING && !_ChunkDataScheduler.Processing) {
                State = SchedulerState.IDLE;
            }
        }

        internal void Dispose() {
            _MeshBuildScheduler.Dispose();
            _ChunkDataScheduler.Dispose();
        }

    }

}