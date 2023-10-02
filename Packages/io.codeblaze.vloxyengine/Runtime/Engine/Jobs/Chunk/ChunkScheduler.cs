using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Core;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Chunk {

    public class ChunkScheduler : JobScheduler {
        private int3 _ChunkSize;
        private ChunkManager _ChunkStore;
        private NoiseProfile _NoiseProfile;

        private JobHandle _Handle;
        
        // can be native arrays
        private NativeList<int3> _Jobs;
        private NativeParallelHashMap<int3, Data.Chunk> _Results;

        public ChunkScheduler(
            VloxySettings settings,
            ChunkManager chunkStore,
            NoiseProfile noiseProfile
        ) {
            _ChunkSize = settings.Chunk.ChunkSize;
            _ChunkStore = chunkStore;
            _NoiseProfile = noiseProfile;

            _Jobs = new NativeList<int3>(Allocator.Persistent);
            _Results = new NativeParallelHashMap<int3, Data.Chunk>(
                settings.Chunk.LoadDistance.CubedSize(), 
                Allocator.Persistent
            );
        }

        internal bool IsReady = true;
        internal bool IsComplete => _Handle.IsCompleted;

        internal void Start(List<int3> jobs) {
            StartRecord();

            IsReady = false;
            
            foreach (var j in jobs) {
                _Jobs.Add(j);
            }
            
            var job = new ChunkJob {
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
            
            IsReady = true;
            StopRecord();
        }
        
        internal void Dispose() {
            _Handle.Complete();
            
            _Jobs.Dispose();
            _Results.Dispose();
        }

    }

}