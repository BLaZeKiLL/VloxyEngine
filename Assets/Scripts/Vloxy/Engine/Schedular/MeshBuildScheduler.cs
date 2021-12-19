using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

#if VLOXY_PROFILING
using CodeBlaze.Vloxy.Profiling;
using CodeBlaze.Vloxy.Engine.Utils.Logger;
#endif

namespace CodeBlaze.Vloxy.Engine.Scheduler {

    public class MeshBuildScheduler {
        
        private ChunkBehaviourPool ChunkBehaviourPool;
        private BurstFunctionPointers BurstFunctionPointers;
        
        private int BatchSize;
        private int3 ChunkSize;
        
        private JobHandle Handle;
        private Mesh.MeshDataArray MeshDataArray;
        private NativeHashMap<int3, int> Results;
        private NativeList<int3> Jobs;
        private NativeArray<VertexAttributeDescriptor> VertexParams;

        private bool Scheduled;
        
        public MeshBuildScheduler(int batchSize, int3 chunkSize, ChunkBehaviourPool chunkBehaviourPool, BurstFunctionPointers burstFunctionPointers) {
            BatchSize = batchSize;
            ChunkSize = chunkSize;
            
            ChunkBehaviourPool = chunkBehaviourPool;
            BurstFunctionPointers = burstFunctionPointers;

            VertexParams = new NativeArray<VertexAttributeDescriptor>(5, Allocator.Persistent);
            
            // int's cause issues
            VertexParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            VertexParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            VertexParams[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
            VertexParams[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            VertexParams[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4);
            
            Results = new NativeHashMap<int3, int>(1024,Allocator.Persistent);
            Jobs = new NativeList<int3>(Allocator.Persistent);
        }

        public void Dispose() {
            VertexParams.Dispose();
            Results.Dispose();
            Jobs.Dispose();
        }

        // Call early in frame
        public void Schedule(List<int3> jobs, ChunkStoreAccessor accessor) {
#if VLOXY_PROFILING
            VloxyProfiler.MeshBuildJobMarker.Begin();
#endif
            for (int i = 0; i < jobs.Count; i++) {
                Jobs.Add(jobs[i]);
            }
            
            MeshDataArray = Mesh.AllocateWritableMeshData(Jobs.Length);

            var job = new MeshBuildJob {
                BurstFunctionPointers = BurstFunctionPointers,
                Accessor = accessor,
                ChunkSize = ChunkSize,
                Jobs = Jobs,
                VertexParams = VertexParams,
                MeshDataArray = MeshDataArray,
                Results = Results.AsParallelWriter()
            };

            Handle = job.Schedule(Jobs.Length, BatchSize);

            Scheduled = true;
        }

        // Call late in frame
        public void Complete() {
#if VLOXY_PROFILING
            if (!Scheduled) return;
#else
            if (!Scheduled || !Handle.IsCompleted) return;
#endif

            Handle.Complete();

            var meshes = new Mesh[Jobs.Length];

            for (int i = 0; i < Jobs.Length; i++) {
                meshes[Results[Jobs[i]]] = ChunkBehaviourPool.Claim(Jobs[i]).Mesh();
            }

            Mesh.ApplyAndDisposeWritableMeshData(MeshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);

            foreach (var mesh in meshes) {
                mesh.RecalculateBounds();
            }
            
            Results.Clear();
            Jobs.Clear();

            Scheduled = false;
            
#if VLOXY_PROFILING
            VloxyProfiler.MeshBuildJobMarker.End();
            
            VloxyLogger.Info<MeshBuildScheduler>($"Meshes built : {meshes.Length}, In : {VloxyProfiler.MeshBuildJobRecorder.TimeMS()}");
#endif
        }

    }

}