using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Mesher;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Jobs.Mesh {

    [BurstCompile]
    internal struct MeshBuildJob : IJobParallelFor {

        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public NativeArray<VertexAttributeDescriptor> VertexParams;

        [ReadOnly] public ChunkAccessor Accessor;
        [ReadOnly] public NativeList<int3> Jobs;

        [WriteOnly] public NativeParallelHashMap<int3, int>.ParallelWriter Results;

        public UnityEngine.Mesh.MeshDataArray MeshDataArray;

        public void Execute(int index) {
            var mesh = MeshDataArray[index];
            var position = Jobs[index];

            var mesh_buffer = GreedyMesher.GenerateMesh(Accessor, position, ChunkSize);
            var vertex_count = mesh_buffer.VertexBuffer.Length;
            var index_0_count = mesh_buffer.IndexBuffer0.Length;

            var descriptor0 = new SubMeshDescriptor(0, index_0_count);
            
            mesh.SetVertexBufferParams(vertex_count, VertexParams);
            mesh.SetIndexBufferParams(index_0_count, IndexFormat.UInt32);

            mesh.GetVertexData<Vertex>().CopyFrom(mesh_buffer.VertexBuffer);
            mesh.GetIndexData<int>().CopyFrom(mesh_buffer.IndexBuffer0);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, descriptor0, MeshUpdateFlags.DontRecalculateBounds);

            Results.TryAdd(position, index);

            mesh_buffer.Dispose();
        }

    }

}