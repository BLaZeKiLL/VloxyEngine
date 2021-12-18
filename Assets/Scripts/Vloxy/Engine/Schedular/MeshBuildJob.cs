using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;

using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Scheduler {

    [BurstCompile]
    internal struct MeshBuildJob : IJobParallelFor {

        [ReadOnly] public BurstFunctionPointers BurstFunctionPointers;
        [ReadOnly] public ChunkStoreAccessor Accessor;
        [ReadOnly] public int3 ChunkSize;
        [ReadOnly] public NativeList<int3> Jobs;
        [ReadOnly] public NativeArray<VertexAttributeDescriptor> VertexParams;

        [WriteOnly] public NativeHashMap<int3, int>.ParallelWriter Results;

        public Mesh.MeshDataArray MeshDataArray;

        public void Execute(int index) {
            var vertexOverride = BurstFunctionPointers.VertexOverridePointer;
            
            var mesh = MeshDataArray[index];
            var position = Jobs[index];

            var buffer = GreedyMesher.GenerateMesh(Accessor, position, ChunkSize, vertexOverride);
            var vertex_count = buffer.VertexBuffer.Length;
            var index_count = buffer.IndexBuffer.Length;

            mesh.SetVertexBufferParams(vertex_count, VertexParams);
            mesh.SetIndexBufferParams(index_count, IndexFormat.UInt32);

            mesh.GetVertexData<Vertex>().CopyFrom(buffer.VertexBuffer);
            mesh.GetIndexData<int>().CopyFrom(buffer.IndexBuffer);

            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, index_count));

            Results.TryAdd(position, index);

            buffer.Dispose();
        }

    }

}