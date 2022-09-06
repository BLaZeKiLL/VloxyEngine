using AOT;

using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Examples.Colored.Data;

using Unity.Burst;
using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Examples.Colored.Components {

    [BurstCompile]
    public static class ColoredBurstFunctions {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(MeshOverrides.VertexOverride))]
        public static void ColoredVertexOverride(
            int block,
            ref int3 normal,
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
        
        [BurstCompile]
        [MonoPInvokeCallback(typeof(NoiseOverrides.ComputeBlockOverride))]
        public static int ColoredComputeBlockOverride(ref NoiseValue noise) {
            return noise.Position.y > noise.Value ? ColoredBlocks.Air() : ColoredBlocks.FromColor32(new Color32(255, 10, 10, 255));
        }

    }

}