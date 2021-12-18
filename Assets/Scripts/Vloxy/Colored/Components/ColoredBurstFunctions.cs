using AOT;

using CodeBlaze.Vloxy.Engine.Mesher;

using Unity.Burst;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Colored.Components {

    [BurstCompile]
    public static class ColoredBurstFunctions {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(MeshExtensions.VertexOverride))]
        public static void ColoredVertexOverride(
            int block, 
            ref Vertex v1, 
            ref Vertex v2, 
            ref Vertex v3,
            ref Vertex v4
        ) {
            byte r = (byte)(block >> 24);
            byte g = (byte)(block >> 16);
            byte b = (byte)(block >> 8);
            byte a = (byte)block;
            
            var color = new float4(r, g, b, a) / new float4(255, 255, 255, 255);
            
            v1.Color = color;
            v2.Color = color;
            v3.Color = color;
            v4.Color = color;
        }

    }

}