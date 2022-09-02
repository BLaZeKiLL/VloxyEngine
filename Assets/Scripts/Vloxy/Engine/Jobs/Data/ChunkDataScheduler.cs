using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Data {

    public class ChunkDataScheduler {

        internal bool Processing { get; private set; }
        
        private int3 _ChunkSize;
        private ChunkState _ChunkState;
        private ChunkStore _ChunkStore;
        private NoiseProfile _NoiseProfile;
        private BurstFunctionPointers _BurstFunctionPointers;
        
        private JobHandle _Handle;

        private NativeList<int3> _Jobs;
        private NativeParallelHashMap<int3, Chunk> _Results;
        private Queue<int3> _Queue;
        private bool _Scheduled;
        private int _BatchSize;
        
#if VLOXY_LOGGING
        private Queue<long> _Timings;
        private Stopwatch _Watch;
#endif

        public ChunkDataScheduler(VloxySettings settings,
            ChunkState chunkState,
            ChunkStore chunkStore,
            NoiseProfile noiseProfile,
            BurstFunctionPointers burstFunctionPointers
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _BatchSize = settings.Scheduler.StreamingBatchSize;
            _ChunkState = chunkState;
            _ChunkStore = chunkStore;
            _NoiseProfile = noiseProfile;
            _BurstFunctionPointers = burstFunctionPointers;
            
            _Queue = new Queue<int3>();
            
            _Jobs = new NativeList<int3>(Allocator.Persistent);
            _Results = new NativeParallelHashMap<int3, Chunk>(settings.Chunk.LoadDistance.CubedSize(), Allocator.Persistent);
            
#if VLOXY_LOGGING
            _Watch = new Stopwatch();
            _Timings = new Queue<long>(10);
#endif
        }
        
        internal bool Update() {
            if (_Scheduled || _Queue.Count <= 0) return false;

            Process();

            return true;
        }

        internal bool LateUpdate() {
            return _Scheduled && Complete();
        } 
        
        internal void Dispose() {
            _Jobs.Dispose();
            _Results.Dispose();
        }

        /// <summary>
        /// Initial Generation
        /// </summary>
        /// <param name="jobs"></param>
        internal void GenerateChunks(NativeArray<int3> jobs) {
            var job = new ChunkDataJob {
                Jobs = jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _ChunkStore.Chunks.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };

            var handle = job.Schedule(jobs.Length, 4);
            
            handle.Complete();
            
            for (var index = 0; index < jobs.Length; index++) {
                var position = jobs[index];
                
                if (_ChunkState.GetState(position) == ChunkState.State.STREAMING) {
                    _ChunkState.SetState(position, ChunkState.State.LOADED);
                } else { // This is unnecessary, how can we avoid this ? 
#if VLOXY_LOGGING
                    VloxyLogger.Warn<ChunkDataScheduler>($"Redundant Chunk : {position} : {_ChunkState.GetState(position)}");
#endif
                }
            }
            
            jobs.Dispose();
        }

        internal void Reclaim(List<int3> positions) {
            for (int i = 0; i < positions.Count; i++) {
                var position = positions[i];
                var state = _ChunkState.GetState(position);

                switch (state) {
                    case ChunkState.State.UNLOADED:
#if VLOXY_LOGGING
                        VloxyLogger.Warn<ChunkDataScheduler>($"Invalid state : {state} for : {position}");
#endif
                        break;
                    case ChunkState.State.STREAMING:
                        _ChunkState.RemoveState(position);
                        break;
                    case ChunkState.State.LOADED:
                        _ChunkStore.RemoveChunk(position);
                        _ChunkState.RemoveState(position);
                        break;
                    case ChunkState.State.MESHING:
                        _ChunkStore.RemoveChunk(position);
                        _ChunkState.RemoveState(position);
                        break;
                    case ChunkState.State.ACTIVE:
                        _ChunkStore.RemoveChunk(position);
                        _ChunkState.RemoveState(position);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        internal void Schedule(List<int3> jobs) {
            for (int i = 0; i < jobs.Count; i++) {
                var position = jobs[i];
                var state = _ChunkState.GetState(position);
                
                switch (state) {
                    case ChunkState.State.UNLOADED:
                        _Queue.Enqueue(position);
                        _ChunkState.SetState(position, ChunkState.State.STREAMING);
                        break;
                    case ChunkState.State.STREAMING:
#if VLOXY_LOGGING
                        VloxyLogger.Warn<ChunkDataScheduler>($"Waiting streaming for : {position}");
#endif
                        break;
                    case ChunkState.State.LOADED:
                    case ChunkState.State.MESHING:
                    case ChunkState.State.ACTIVE:
#if VLOXY_LOGGING
                        VloxyLogger.Warn<ChunkDataScheduler>($"Invalid state : {state} for : {position}");
#endif
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            Processing = _Queue.Count > 0;
        }

        private void Process() {
            var count = _BatchSize;
            
            while (count > 0 && _Queue.Count > 0) {
                var position = _Queue.Dequeue();
                
                if (_ChunkState.GetState(position) != ChunkState.State.STREAMING) continue;
                
                _Jobs.Add(position);
                count--;
            }
            
#if VLOXY_LOGGING
            _Watch.Restart();
#endif
            
            var job = new ChunkDataJob {
                Jobs = _Jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _Results.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };
            
            _Handle = job.Schedule(_Jobs.Length, 1);

            _Scheduled = true;
        }

        private bool Complete() {
            if (!_Handle.IsCompleted) return false;
            
            _Handle.Complete();
            
            for (var index = 0; index < _Jobs.Length; index++) {
                var position = _Jobs[index];
                
                if (_ChunkState.GetState(position) == ChunkState.State.STREAMING) {
                    var chunk = _Results[position];
                    _ChunkStore.Chunks.Add(position, chunk);
                    _ChunkState.SetState(position, ChunkState.State.LOADED);
                } else { // This is unnecessary, how can we avoid this ? 
#if VLOXY_LOGGING
                    VloxyLogger.Warn<ChunkDataScheduler>($"Redundant Chunk : {position} : {_ChunkState.GetState(position)}");
#endif
                }
            }

            _Jobs.Clear();
            _Results.Clear();
            
            _Scheduled = false;
            
            Processing = _Queue.Count > 0;
            
#if VLOXY_LOGGING
            _Watch.Stop();
            Timestamp(_Watch.ElapsedMilliseconds);
#endif
            return true;
        }

#if VLOXY_LOGGING
        public float AvgTime => (float) _Timings.Sum() / 10;

        private void Timestamp(long ms) {
            if (_Timings.Count <= 10) _Timings.Enqueue(ms);
            else {
                _Timings.Dequeue();
                _Timings.Enqueue(ms);
            }
        }
#endif

    }

}