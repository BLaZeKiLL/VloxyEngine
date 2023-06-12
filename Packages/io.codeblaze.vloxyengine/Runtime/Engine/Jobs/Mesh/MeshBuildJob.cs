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
            
            // Vertex Buffer
            var vertex_count = mesh_buffer.VertexBuffer.Length;

            mesh.SetVertexBufferParams(vertex_count, VertexParams);
            mesh.GetVertexData<Vertex>().CopyFrom(mesh_buffer.VertexBuffer.AsArray());

            // Index Buffer
            var index_0_count = mesh_buffer.IndexBuffer0.Length;
            var index_1_count = mesh_buffer.IndexBuffer1.Length;
            
            mesh.SetIndexBufferParams(index_0_count + index_1_count, IndexFormat.UInt32);

            var index_buffer = mesh.GetIndexData<int>();
            
            NativeArray<int>.Copy(mesh_buffer.IndexBuffer0.AsArray(), 0, index_buffer, 0, index_0_count);
            if (index_1_count > 1)
                NativeArray<int>.Copy(mesh_buffer.IndexBuffer1.AsArray(), 0, index_buffer, index_0_count, index_1_count);

            // Sub Mesh
            mesh.subMeshCount = 2;
            
            var descriptor0 = new SubMeshDescriptor(0, index_0_count);
            var descriptor1 = new SubMeshDescriptor(index_0_count, index_1_count);
            
            mesh.SetSubMesh(0, descriptor0, MeshUpdateFlags.DontRecalculateBounds);
            mesh.SetSubMesh(1, descriptor1, MeshUpdateFlags.DontRecalculateBounds);

            Results.TryAdd(position, index);

            mesh_buffer.Dispose();
        }

    }

}