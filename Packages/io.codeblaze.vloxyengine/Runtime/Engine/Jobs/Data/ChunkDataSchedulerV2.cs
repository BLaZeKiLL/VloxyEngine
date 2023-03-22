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

    public class ChunkDataSchedulerV2 {
        private int3 _ChunkSize;
        private ChunkStore _ChunkStore;
        private NoiseProfile _NoiseProfile;
        private BurstFunctionPointers _BurstFunctionPointers;

        private JobHandle _Handle;
        
        // can be native arrays
        private NativeList<int3> _Jobs;
        private NativeParallelHashMap<int3, Chunk> _Results;

        private Queue<Chunk> _ResultsQueue;

#if VLOXY_LOGGING
        private Queue<long> _Timings;
        private Stopwatch _Watch;
#endif

        public ChunkDataSchedulerV2(
            VloxySettings settings,
            ChunkStore chunkStore,
            NoiseProfile noiseProfile,
            BurstFunctionPointers burstFunctionPointers
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _ChunkStore = chunkStore;
            _NoiseProfile = noiseProfile;
            _BurstFunctionPointers = burstFunctionPointers;

            _Jobs = new NativeList<int3>(Allocator.Persistent);
            _Results = new NativeParallelHashMap<int3, Chunk>(
                settings.Chunk.LoadDistance.CubedSize(), 
                Allocator.Persistent
            );

            _ResultsQueue = new Queue<Chunk>();
            
#if VLOXY_LOGGING
            _Watch = new Stopwatch();
            _Timings = new Queue<long>(10);
#endif
        }

        internal bool IsReady = true;
        internal bool IsComplete => _Handle.IsCompleted;
        
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

            jobs.Dispose();
        }

        internal void Start(List<int3> jobs) {
            IsReady = false;
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkDataSchedulerV2>($"Scheduling {jobs.Count} chunks to generate");
            _Watch.Restart();
#endif
            foreach (var j in jobs) {
                _Jobs.Add(j);
            }
            
            var job = new ChunkDataJob {
                Jobs = _Jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = _Results.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };
            
            _Handle = job.Schedule(_Jobs.Length, 1);
        }

        internal void Complete() {
            _Handle.Complete();
            
            for (var index = 0; index < _Jobs.Length; index++) {
                _ChunkStore.AddChunk(_Results[_Jobs[index]]);
            }

            _Jobs.Clear();
            _Results.Clear();

#if VLOXY_LOGGING
            _Watch.Stop();
            Timestamp(_Watch.ElapsedMilliseconds);
#endif
            IsReady = true;
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