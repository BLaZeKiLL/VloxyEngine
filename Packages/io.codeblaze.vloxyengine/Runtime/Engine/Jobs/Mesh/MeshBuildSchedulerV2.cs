using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Jobs.Mesh {

    public class MeshBuildSchedulerV2 {

        private readonly ChunkStore _ChunkStore;
        private readonly ChunkPoolV2 _ChunkPool;
        private readonly BurstFunctionPointers _BurstFunctionPointers;

        private int3 _ChunkSize;
        private JobHandle _Handle;

        private NativeList<int3> _Jobs;
        private ChunkAccessor _ChunkAccessor;
        private NativeParallelHashMap<int3, int> _Results;
        private UnityEngine.Mesh.MeshDataArray _MeshDataArray;
        private NativeArray<VertexAttributeDescriptor> _VertexParams;

#if VLOXY_LOGGING
        private Queue<long> _Timings;
        private Stopwatch _Watch;
#endif
        
        public MeshBuildSchedulerV2(
            VloxySettings settings,
            ChunkStore chunkStore,
            ChunkPoolV2 chunkPool, 
            BurstFunctionPointers burstFunctionPointers
        ) {
            _ChunkStore = chunkStore;
            _ChunkPool = chunkPool;
            _BurstFunctionPointers = burstFunctionPointers;

            _ChunkSize = settings.Chunk.ChunkSize;

            // TODO : Make Configurable (Source Generators)
            _VertexParams = new NativeArray<VertexAttributeDescriptor>(6, Allocator.Persistent);
            
            // Int interpolation cause issues
            _VertexParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            _VertexParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            _VertexParams[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
            _VertexParams[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);
            _VertexParams[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2);
            _VertexParams[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4);
            
            _Results = new NativeParallelHashMap<int3, int>(settings.Chunk.DrawDistance.CubedSize(),Allocator.Persistent);
            _Jobs = new NativeList<int3>(Allocator.Persistent);
#if VLOXY_LOGGING
            _Watch = new Stopwatch();
            _Timings = new Queue<long>(10);
#endif
        }

        internal bool IsReady = true;
        internal bool IsComplete => _Handle.IsCompleted;

        internal void Dispose() {
            _VertexParams.Dispose();
            _Results.Dispose();
            _Jobs.Dispose();
        }
        
        internal void Start(List<int3> jobs) {
            IsReady = false;

#if VLOXY_LOGGING
            VloxyLogger.Info<MeshBuildSchedulerV2>($"Scheduling {jobs.Count} meshes to build");
            VloxyLogger.Info<MeshBuildSchedulerV2>(string.Join(", ", jobs));
            _Watch.Restart();
#endif
            _ChunkAccessor = _ChunkStore.GetAccessor(jobs);
            
            foreach (var j in jobs) {
                _Jobs.Add(j);
            }
            
            _MeshDataArray = UnityEngine.Mesh.AllocateWritableMeshData(_Jobs.Length);

            var job = new MeshBuildJob {
                BurstFunctionPointers = _BurstFunctionPointers,
                Accessor = _ChunkAccessor,
                ChunkSize = _ChunkSize,
                Jobs = _Jobs,
                VertexParams = _VertexParams,
                MeshDataArray = _MeshDataArray,
                Results = _Results.AsParallelWriter()
            };

            _Handle = job.Schedule(_Jobs.Length, 1);
        }
        
        internal void Complete() {
            _Handle.Complete();

            var meshes = new UnityEngine.Mesh[_Jobs.Length];

            for (var index = 0; index < _Jobs.Length; index++) {
                var position = _Jobs[index];
                
                meshes[_Results[position]] = _ChunkPool.Claim(position).Mesh();
            }

            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(
                _MeshDataArray, 
                meshes, 
                MeshUpdateFlags.DontRecalculateBounds
            );
            
            for (var index = 0; index < meshes.Length; index++) {
                meshes[index].RecalculateBounds();
            }
            
            _ChunkAccessor.Dispose();
            _Results.Clear();
            _Jobs.Clear();

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