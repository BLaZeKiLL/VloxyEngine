using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
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

        public void GenerateChunks(NativeArray<int3> jobs) {
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
        
        public void Schedule(List<int3> jobs) {
            for (int i = 0; i < jobs.Count; i++) {
                _Queue.Enqueue(jobs[i]);
            }
            
            Processing = _Queue.Count > 0;
        }

        private void Process() {
            var count = _BatchSize;
            
            while (count > 0 && _Queue.Count > 0) {
                _Jobs.Add(_Queue.Dequeue());
                count--;
            }
            
#if VLOXY_LOGGING
            _Watch.Restart();
#endif
            
            var job = new ChunkDataJob {
                Jobs = _Jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _ChunkStore.Chunks.AsParallelWriter(),
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
                    _ChunkState.SetState(position, ChunkState.State.LOADED);
                } else { // This is unnecessary, how can we avoid this ? 
#if VLOXY_LOGGING
                    VloxyLogger.Warn<ChunkDataScheduler>($"Redundant Chunk : {position} : {_ChunkState.GetState(position)}");
#endif
                }
            }

            _Jobs.Clear();
            _Scheduled = false;
            
            Processing = _Queue.Count > 0;
            
#if VLOXY_LOGGING
            _Watch.Stop();
            Timestamp(_Watch.ElapsedMilliseconds);
#endif
            return true;
        }

        public void Dispose() {
            _Jobs.Dispose();
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