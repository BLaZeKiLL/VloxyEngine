using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Jobs.Mesh {

    public class MeshBuildScheduler {
        
        private ChunkBehaviourPool ChunkBehaviourPool;
        private BurstFunctionPointers BurstFunctionPointers;
        
        private int BatchSize;
        private int3 ChunkSize;
        
        private JobHandle Handle;
        private UnityEngine.Mesh.MeshDataArray MeshDataArray;
        private NativeHashMap<int3, int> Results;
        private NativeList<int3> Jobs;
        private NativeArray<VertexAttributeDescriptor> VertexParams;

        private bool Scheduled;

#if VLOXY_LOGGING
        private Stopwatch watch;
#endif
        
        public MeshBuildScheduler(int batchSize, int3 chunkSize, int drawDistance, ChunkBehaviourPool chunkBehaviourPool, BurstFunctionPointers burstFunctionPointers) {
            BatchSize = batchSize;
            ChunkSize = chunkSize;
            
            ChunkBehaviourPool = chunkBehaviourPool;
            BurstFunctionPointers = burstFunctionPointers;

            // TODO : Make Configurable
            VertexParams = new NativeArray<VertexAttributeDescriptor>(6, Allocator.Persistent);
            
            // int's cause issues
            VertexParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            VertexParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            VertexParams[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
            VertexParams[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);
            VertexParams[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2);
            VertexParams[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4);
            
            Results = new NativeHashMap<int3, int>(drawDistance.CubedSize(),Allocator.Persistent);
            Jobs = new NativeList<int3>(Allocator.Persistent);
            
#if VLOXY_LOGGING
            watch = new Stopwatch();
#endif
        }

        // Call early in frame
        public void Schedule(List<int3> jobs, ChunkStoreAccessor accessor) {
            if (Scheduled) {
                throw new InvalidOperationException($"Job Already Scheduled : {Handle}");
            }
#if VLOXY_LOGGING
            watch.Restart();
#endif
            
            for (int i = 0; i < jobs.Count; i++) {
                Jobs.Add(jobs[i]);
            }
            
            MeshDataArray = UnityEngine.Mesh.AllocateWritableMeshData(Jobs.Length);

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
            if (!Scheduled || !Handle.IsCompleted) return;

            Handle.Complete();

            var meshes = new UnityEngine.Mesh[Jobs.Length];

            for (var index = 0; index < Jobs.Length; index++) {
                meshes[Results[Jobs[index]]] = ChunkBehaviourPool.Claim(Jobs[index]).Mesh();
            }

            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(MeshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);

            // TODO : pre compute bounds
            for (var index = 0; index < meshes.Length; index++) {
                meshes[index].RecalculateBounds();
            }

            Results.Clear();
            Jobs.Clear();

            Scheduled = false;
            
#if VLOXY_LOGGING
            watch.Stop();
            VloxyLogger.Info<MeshBuildScheduler>($"Meshes built : {meshes.Length}, In : {watch.ElapsedMilliseconds} MS");
#endif
        }
        
        public void Dispose() {
            VertexParams.Dispose();
            Results.Dispose();
            Jobs.Dispose();
        }

    }

}