using System;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Page {

    public class ChunkPageScheduler {

        private NoiseProfile _NoiseProfile;
        private int3 _ChunkSize;
        private int _PageSize;
        private BurstFunctionPointers _BurstFunctionPointers;
        
        private JobHandle _Handle;
        private bool _Scheduled;

        public ChunkPageScheduler(NoiseProfile noiseProfile, int3 chunkSize, int pageSize, BurstFunctionPointers burstFunctionPointers) {
            _NoiseProfile = noiseProfile;
            _ChunkSize = chunkSize;
            _PageSize = pageSize;
            _BurstFunctionPointers = burstFunctionPointers;
        }
        
        public bool CanSchedule() => !_Scheduled;

        public void Schedule(ChunkPage page) {
            if (_Scheduled) {
#if VLOXY_LOGGING
                VloxyLogger.Error<ChunkPageScheduler>($"Job Already Scheduled : {_Handle}");
#endif
                return;
            }

            var jobs = page.GetPositions(Allocator.TempJob);
            
            var job = new ChunkPageJob {
                Jobs = jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = page.Chunks.AsParallelWriter(),
                BurstFunctionPointers = _BurstFunctionPointers,
            };

            _Handle = job.Schedule(jobs.Length, 4);

            jobs.Dispose(_Handle);

            _Scheduled = true;
        }

        public void Complete() {
            _Handle.Complete();

            _Scheduled = false;
        }

        public void Dispose() {
            
        }

    }

}