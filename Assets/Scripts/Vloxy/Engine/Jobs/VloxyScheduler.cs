using System;

using CodeBlaze.Vloxy.Engine.Jobs.Mesh;
using CodeBlaze.Vloxy.Engine.Jobs.Store;
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
        private readonly ChunkStoreScheduler _ChunkStoreScheduler;

        public VloxyScheduler(MeshBuildScheduler meshBuildScheduler, ChunkStoreScheduler chunkStoreScheduler) {
            _MeshBuildScheduler = meshBuildScheduler;
            _ChunkStoreScheduler = chunkStoreScheduler;

            State = SchedulerState.IDLE;
        }

        internal void Update() {
            // VloxyLogger.Info<VloxyScheduler>($"Mesh : {_MeshBuildScheduler.Processing}");
            
            if (State == SchedulerState.IDLE) {
                if (_MeshBuildScheduler.Processing) {
                    State = SchedulerState.MESHING;
                } else if (_ChunkStoreScheduler.Processing) {
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
                    _ChunkStoreScheduler.Update();
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
                    _ChunkStoreScheduler.LateUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (State == SchedulerState.MESHING && !_MeshBuildScheduler.Processing) {
                State = SchedulerState.IDLE;
            } else if (State == SchedulerState.STREAMING && !_ChunkStoreScheduler.Processing) {
                State = SchedulerState.IDLE;
            }
        }

        internal void Dispose() {
            _MeshBuildScheduler.Dispose();
            _ChunkStoreScheduler.Dispose();
        }

    }

}