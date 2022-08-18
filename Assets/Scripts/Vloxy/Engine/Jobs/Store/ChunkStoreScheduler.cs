using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Store {

    public class ChunkStoreScheduler {

        internal JobHandle Handle { get; private set; }
        
        private int3 _ChunkSize;
        private ChunkState _ChunkState;
        private ChunkStore _ChunkStore;
        private NoiseProfile _NoiseProfile;
        private BurstFunctionPointers _BurstFunctionPointers;
        
        private NativeList<int3> _Jobs;
        private Queue<int3> _Queue;
        private bool _Scheduled;
        private int _BatchSize;
        
#if VLOXY_LOGGING
        private Stopwatch _Watch;
#endif

        public ChunkStoreScheduler(VloxySettings settings,
            ChunkState chunkState,
            ChunkStore chunkStore,
            NoiseProfile noiseProfile,
            BurstFunctionPointers burstFunctionPointers
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _BatchSize = settings.Scheduler.BatchSize;
            _ChunkState = chunkState;
            _ChunkStore = chunkStore;
            _NoiseProfile = noiseProfile;
            _BurstFunctionPointers = burstFunctionPointers;
            
            _Queue = new Queue<int3>();
            _Jobs = new NativeList<int3>(Allocator.Persistent);
            
#if VLOXY_LOGGING
            _Watch = new Stopwatch();
#endif
        }
        
        public bool Update(JobHandle dep) {
            if (_Scheduled || _Queue.Count <= 0) return false;

            Process(dep);

            return true;

        }

        public bool LateUpdate() {
            return _Scheduled && Complete();
        }

        public void GenerateChunks(NativeArray<int3> jobs) {
            var job = new ChunkStoreJob {
                Jobs = jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _ChunkStore.Chunks.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };

            var handle = job.Schedule(jobs.Length, 4);

            jobs.Dispose(handle);
            
            handle.Complete();
        }
        
        public void Schedule(List<int3> jobs) {
            for (int i = 0; i < jobs.Count; i++) {
                _Queue.Enqueue(jobs[i]);
            }
        }

        private void Process(JobHandle dep) {
            var count = _BatchSize;
            
            while (count > 0 && _Queue.Count > 0) {
                _Jobs.Add(_Queue.Dequeue());
                count--;
            }
            
#if VLOXY_LOGGING
            _Watch.Restart();
#endif
            
            var job = new ChunkStoreJob {
                Jobs = _Jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _ChunkStore.Chunks.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };
            
            Handle = job.Schedule(_Jobs.Length, 1, dep);

            _Scheduled = true;
        }

        private bool Complete() {
            if (!Handle.IsCompleted) return false;
            
            Handle.Complete();

            int index;
            
            for (index = 0; index < _Jobs.Length; index++) {
                var position = _Jobs[index];
                
                if (_ChunkState.GetState(position) == ChunkState.State.STREAMING) {
                    _ChunkState.SetState(position, ChunkState.State.LOADED);
                } else { // This is unnecessary, how can we avoid this ? 
#if VLOXY_LOGGING
                    VloxyLogger.Warn<ChunkStoreScheduler>($"Redundant Chunk : {position} : {_ChunkState.GetState(position)}");
#endif
                }
            }

            _Jobs.Clear();
            _Scheduled = false;
            
#if VLOXY_LOGGING
            _Watch.Stop();
            VloxyLogger.Info<ChunkStoreScheduler>($"Chunks streamed : {index}, In : {_Watch.ElapsedMilliseconds} MS");
#endif
            return true;
        }

        public void Dispose() {
            _Jobs.Dispose();
        }

    }

}