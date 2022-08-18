using System;

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

        private NoiseProfile _NoiseProfile;
        private int3 _ChunkSize;
        private BurstFunctionPointers _BurstFunctionPointers;
        
        private JobHandle _Handle;
        private bool _Scheduled;

        public ChunkStoreScheduler(
            VloxySettings settings, 
            NoiseProfile noiseProfile, 
            BurstFunctionPointers burstFunctionPointers
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;

            _NoiseProfile = noiseProfile;
            _BurstFunctionPointers = burstFunctionPointers;
        }
        
        public bool CanSchedule() => !_Scheduled;

        public void Schedule(ChunkStore store) {
            if (_Scheduled) {
#if VLOXY_LOGGING
                VloxyLogger.Error<ChunkStoreScheduler>($"Job Already Scheduled : {_Handle}");
#endif
                return;
            }

            var jobs = store.GetPositions(Allocator.TempJob);
            
            var job = new ChunkStoreJob {
                Jobs = jobs,
                ChunkSize = _ChunkSize,
                NoiseProfile = _NoiseProfile,
                Results = store.Chunks.AsParallelWriter(),
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