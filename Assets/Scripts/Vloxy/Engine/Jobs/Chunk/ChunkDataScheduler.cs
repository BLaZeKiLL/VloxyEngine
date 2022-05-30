using System;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Chunk {

    public class ChunkDataScheduler {

        private NoiseProfile _NoiseProfile;
        private int3 _ChunkSize;
        private int _PageSize;
        private BurstFunctionPointers _BurstFunctionPointers;

        private NativeHashMap<int3, ChunkData> _Results;

        private JobHandle _Handle;
        private bool _Scheduled;

        public ChunkDataScheduler(NoiseProfile noiseProfile, int3 chunkSize, int pageSize, BurstFunctionPointers burstFunctionPointers) {
            _NoiseProfile = noiseProfile;
            _ChunkSize = chunkSize;
            _PageSize = pageSize;
            _BurstFunctionPointers = burstFunctionPointers;
        }

        public void Schedule(NativeArray<int3> jobs) {
            if (_Scheduled) {
                throw new InvalidOperationException("Job Already Scheduled");
            }

            _Results = new NativeHashMap<int3, ChunkData>(_PageSize.CubedSize(), Allocator.TempJob);

            var job = new ChunkDataJob {
                Jobs = jobs,
                Results = _Results.AsParallelWriter(),
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                BurstFunctionPointers = _BurstFunctionPointers
            };

            _Handle = job.Schedule(jobs.Length, 4);

            _Scheduled = true;
        }

        public NativeHashMap<int3, ChunkData> Complete() {
            _Handle.Complete();

            _Scheduled = false;

            return _Results;
        }

        public void Dispose() {
            _Results.Dispose();
        }

    }

}