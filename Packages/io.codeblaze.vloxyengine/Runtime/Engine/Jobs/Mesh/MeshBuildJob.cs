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

            var meshBuffer = GreedyMesher.GenerateMesh(Accessor, position, ChunkSize);
            
            // Vertex Buffer
            var vertexCount = meshBuffer.VertexBuffer.Length;

            mesh.SetVertexBufferParams(vertexCount, VertexParams);
            mesh.GetVertexData<Vertex>().CopyFrom(meshBuffer.VertexBuffer.AsArray());

            // Index Buffer
            var index0Count = meshBuffer.IndexBuffer0.Length;
            var index1Count = meshBuffer.IndexBuffer1.Length;
            
            mesh.SetIndexBufferParams(index0Count + index1Count, IndexFormat.UInt32);

            var indexBuffer = mesh.GetIndexData<int>();
            
            NativeArray<int>.Copy(meshBuffer.IndexBuffer0.AsArray(), 0, indexBuffer, 0, index0Count);
            if (index1Count > 1)
                NativeArray<int>.Copy(meshBuffer.IndexBuffer1.AsArray(), 0, indexBuffer, index0Count, index1Count);

            // Sub Mesh
            mesh.subMeshCount = 2;
            
            var descriptor0 = new SubMeshDescriptor(0, index0Count);
            var descriptor1 = new SubMeshDescriptor(index0Count, index1Count);
            
            mesh.SetSubMesh(0, descriptor0, MeshUpdateFlags.DontRecalculateBounds);
            mesh.SetSubMesh(1, descriptor1, MeshUpdateFlags.DontRecalculateBounds);
            
            Results.TryAdd(position, index);

            meshBuffer.Dispose();
        }

    }

}