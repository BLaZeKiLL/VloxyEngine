using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Scheduler {

    public class MeshBuildScheduler {

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static ProfilerMarker Marker = new("MeshBuildJob");
        private ProfilerRecorder Recorder;
#endif
        private ChunkBehaviourPool ChunkBehaviourPool;
        
        private int BatchSize;
        private int3 ChunkSize;
        
        private JobHandle Handle;
        private Mesh.MeshDataArray MeshDataArray;
        private NativeHashMap<int3, int> Results;
        private NativeArray<int3> Jobs;
        private NativeArray<VertexAttributeDescriptor> VertexParams;

        private bool Scheduled;


        public MeshBuildScheduler(int batchSize, int3 chunkSize, ChunkBehaviourPool chunkBehaviourPool) {
            BatchSize = batchSize;
            ChunkSize = chunkSize;
            ChunkBehaviourPool = chunkBehaviourPool;

            VertexParams = new NativeArray<VertexAttributeDescriptor>(5, Allocator.Persistent);
            
            // int's cause issues
            VertexParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            VertexParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            VertexParams[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
            VertexParams[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            VertexParams[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 4);
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Recorder = ProfilerRecorder.StartNew(Marker);
#endif
        }

        public void Dispose() {
            VertexParams.Dispose();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Recorder.Dispose();
#endif
        }

        // Call early in frame
        public void Schedule(NativeArray<int3> jobs, ChunkStoreAccessor accessor) {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Marker.Begin();
#endif
            
            Jobs = jobs;
            MeshDataArray = Mesh.AllocateWritableMeshData(Jobs.Length);
            Results = new NativeHashMap<int3, int>(Jobs.Length, Allocator.TempJob);

            var job = new ChunkMeshJob {
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
            if (!Scheduled) return;

            Handle.Complete();

            var meshes = new Mesh[Jobs.Length];

            for (int i = 0; i < Jobs.Length; i++) {
                meshes[Results[Jobs[i]]] = ChunkBehaviourPool.Claim(Jobs[i]).Mesh();
            }

            Mesh.ApplyAndDisposeWritableMeshData(MeshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);

            foreach (var mesh in meshes) {
                mesh.RecalculateBounds();
            }
            
            Results.Dispose();
            Jobs.Dispose();

            Scheduled = false;
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Marker.End();
            
            VloxyLogger.Info<MeshBuildScheduler>($"Meshes built : {meshes.Length}, In : {Recorder.CurrentValueAsDouble * (1e-6f):F}ms");
#endif
        }
        
        [BurstCompile]
        private struct ChunkMeshJob : IJobParallelFor {

            [ReadOnly] public ChunkStoreAccessor Accessor;
            [ReadOnly] public int3 ChunkSize;
            [ReadOnly] public NativeArray<int3> Jobs;
            [ReadOnly] public NativeArray<VertexAttributeDescriptor> VertexParams;

            [WriteOnly] public NativeHashMap<int3, int>.ParallelWriter Results;
            
            public Mesh.MeshDataArray MeshDataArray;

            public void Execute(int index) {
                var mesh = MeshDataArray[index];
                var position = Jobs[index];
                
                var buffer = GreedyMesher.GenerateMesh(Accessor, position, ChunkSize);
                var vertex_count = buffer.VertexBuffer.Length;
                var index_count = buffer.IndexBuffer.Length;

                mesh.SetVertexBufferParams(vertex_count, VertexParams);
                mesh.SetIndexBufferParams(index_count, IndexFormat.UInt32);

                mesh.GetVertexData<GreedyMesher.Vertex>().CopyFrom(buffer.VertexBuffer);
                mesh.GetIndexData<int>().CopyFrom(buffer.IndexBuffer);
                
                mesh.subMeshCount = 1;
                mesh.SetSubMesh(0, new SubMeshDescriptor(0, index_count));

                Results.TryAdd(position, index);
                
                buffer.Dispose();
            }

        }

    }

}