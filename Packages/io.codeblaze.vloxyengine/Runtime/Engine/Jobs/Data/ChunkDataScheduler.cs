using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        private int3 _ChunkSize;
        private ChunkStore _ChunkStore;
        private NoiseProfile _NoiseProfile;

        private JobHandle _Handle;
        
        // can be native arrays
        private NativeList<int3> _Jobs;
        private NativeParallelHashMap<int3, Chunk> _Results;

#if VLOXY_LOGGING
        private Queue<long> _Timings;
        private Stopwatch _Watch;
#endif

        public ChunkDataScheduler(
            VloxySettings settings,
            ChunkStore chunkStore,
            NoiseProfile noiseProfile
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _ChunkStore = chunkStore;
            _NoiseProfile = noiseProfile;

            _Jobs = new NativeList<int3>(Allocator.Persistent);
            _Results = new NativeParallelHashMap<int3, Chunk>(
                settings.Chunk.LoadDistance.CubedSize(), 
                Allocator.Persistent
            );

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

        internal void Start(List<int3> jobs) {
            IsReady = false;
            
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkDataScheduler>($"Scheduling {jobs.Count} chunks to generate");
            VloxyLogger.Info<ChunkDataScheduler>(string.Join(", ", jobs));
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
            };
            
            _Handle = job.Schedule(_Jobs.Length, 1);
        }

        internal void Complete() {
            _Handle.Complete();
            
            _ChunkStore.AddChunks(_Results);

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