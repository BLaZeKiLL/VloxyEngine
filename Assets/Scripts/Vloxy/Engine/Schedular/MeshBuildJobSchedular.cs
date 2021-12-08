using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Schedular {

    public class MeshBuildJobSchedular : MeshBuildSchedular {

        private bool Scheduled;
        private int BatchSize;
        private int3 ChunkSize;
        private JobHandle Handle;

        private Mesh.MeshDataArray MeshDataArray;
        private NativeHashMap<int3, int> Results;
        private NativeArray<int3> Jobs;

        public MeshBuildJobSchedular(int batchSize, int3 chunkSize, ChunkBehaviourPool chunkBehaviourPool) : base(chunkBehaviourPool) {
            BatchSize = batchSize;
            ChunkSize = chunkSize;
        }

        // Call early in frame
        public override void Schedule(NativeArray<int3> jobs, NativeChunkStoreAccessor accessor) {
            Jobs = jobs;
            MeshDataArray = Mesh.AllocateWritableMeshData(Jobs.Length);
            Results = new NativeHashMap<int3, int>(Jobs.Length, Allocator.TempJob);

            var job = new ChunkMeshJob {
                Accessor = accessor,
                ChunkSize = ChunkSize,
                Jobs = Jobs,
                MeshDataArray = MeshDataArray,
                Results = Results.AsParallelWriter()
            };

            Handle = job.Schedule(Jobs.Length, BatchSize);

            Scheduled = true;
        }

        // Call late in frame
        public override void Complete() {
            if (!Scheduled) return;

            Handle.Complete();
            
            // Render
            
            MeshDataArray.Dispose();
            Results.Dispose();
            Jobs.Dispose();

            Scheduled = false;
        }
        
        [BurstCompile]
        private struct ChunkMeshJob : IJobParallelFor {

            [ReadOnly] public NativeChunkStoreAccessor Accessor;
            [ReadOnly] public int3 ChunkSize;
            [ReadOnly] public NativeArray<int3> Jobs;
            
            [WriteOnly] public Mesh.MeshDataArray MeshDataArray;
            [WriteOnly] public NativeHashMap<int3, int>.ParallelWriter Results;

            public void Execute(int index) {
                var mesh = MeshDataArray[index];
                var position = Jobs[index];
                var vertex_params = new NativeArray<VertexAttributeDescriptor>(5, Allocator.TempJob);
                
                vertex_params[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.SInt32);
                vertex_params[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.SInt32, 3, 1);
                vertex_params[1] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float16, 3, 2);
                vertex_params[2] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.UInt8, 2, 3);
                vertex_params[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float16, 4, 4);
                
                // TODO : Figure out lengths (max from chunk size)
                mesh.SetVertexBufferParams(int.MaxValue, vertex_params);
                mesh.SetIndexBufferParams(int.MaxValue, IndexFormat.UInt32);

                GreedyMesher.GenerateMesh(Accessor, position, mesh, ChunkSize);

                Results.TryAdd(position, index);
            }

        }

    }

}