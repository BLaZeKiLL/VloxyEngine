using AOT;

using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Textured.Data;

using Unity.Burst;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Textured.Components {

    [BurstCompile]
    public static class TexturedBurstFunctions {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(MeshExtensions.VertexOverride))]
        public static void TexturedVertexOverride(
            int block,
            ref int3 normal,
            ref Vertex v1, 
            ref Vertex v2, 
            ref Vertex v3,
            ref Vertex v4
        ) {
            v1.Color = float4.zero;
            v2.Color = float4.zero;
            v3.Color = float4.zero;
            v4.Color = float4.zero;
            
            switch (block) {
                case (int) TexturedBlock.GRASS when normal.y == 1:
                    v1.UV2 = new float3(0, 0, 0);
                    v2.UV2 = new float3(0, 1, 0);
                    v3.UV2 = new float3(1, 0, 0);
                    v4.UV2 = new float3(1, 1, 0);
                    break;
                case (int) TexturedBlock.GRASS:
                    v1.UV2 = new float3(0, 0, 1);
                    v2.UV2 = new float3(0, 1, 1);
                    v3.UV2 = new float3(1, 0, 1);
                    v4.UV2 = new float3(1, 1, 1);
                    break;
                case (int) TexturedBlock.DIRT:
                    v1.UV2 = new float3(0, 0, 2);
                    v2.UV2 = new float3(0, 1, 2);
                    v3.UV2 = new float3(1, 0, 2);
                    v4.UV2 = new float3(1, 1, 2);
                    break;
                case (int) TexturedBlock.STONE:
                    v1.UV2 = new float3(0, 0, 3);
                    v2.UV2 = new float3(0, 1, 3);
                    v3.UV2 = new float3(1, 0, 3);
                    v4.UV2 = new float3(1, 1, 3);
                    break;
            }
        }

    }

}