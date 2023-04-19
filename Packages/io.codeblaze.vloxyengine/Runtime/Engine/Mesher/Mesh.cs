using System.Runtime.InteropServices;

using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Mesher {

    [BurstCompile]
    public struct Vertex {

        public float3 Position;
        public float3 Normal;
        public float4 Color;
        public float3 UV0;
        public float2 UV1;
        public float4 UV2;

    }
    
    [BurstCompile]
    internal struct MeshBuffer {

        public NativeList<Vertex> VertexBuffer;
        public NativeList<int> Index0Buffer;
        public NativeList<int> Index1Buffer;

        internal void Dispose() {
            VertexBuffer.Dispose();
            Index0Buffer.Dispose();
            Index1Buffer.Dispose();
        }

    }

    [BurstCompile]
    public static class MeshOverrides {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VertexOverride(int block, ref int3 normal, ref Vertex v1, ref Vertex v2, ref Vertex v3, ref Vertex v4);

    }

}