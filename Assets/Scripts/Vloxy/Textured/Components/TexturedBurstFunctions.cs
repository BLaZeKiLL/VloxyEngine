using AOT;

using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Textured.Data;

using Unity.Burst;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Textured.Components {

    [BurstCompile]
    public static class TexturedBurstFunctions {

        [BurstCompile]
        [MonoPInvokeCallback(typeof(MeshOverrides.VertexOverride))]
        public static void TexturedVertexOverride(
            int block,
            ref int3 normal,
            ref Vertex v1, 
            ref Vertex v2, 
            ref Vertex v3,
            ref Vertex v4
        ) {
            switch (block) {
                case (int) TexturedBlock.GRASS when normal.y is 1 or -1:
                    v1.UV0.z = 0;
                    v2.UV0.z = 0;
                    v3.UV0.z = 0;
                    v4.UV0.z = 0;
                    break;
                case (int) TexturedBlock.GRASS:
                    v1.UV0.z = 1;
                    v2.UV0.z = 1;
                    v3.UV0.z = 1;
                    v4.UV0.z = 1;
                    break;
                case (int) TexturedBlock.DIRT:
                    v1.UV0.z = 2;
                    v2.UV0.z = 2;
                    v3.UV0.z = 2;
                    v4.UV0.z = 2;
                    break;
                case (int) TexturedBlock.STONE:
                    v1.UV0.z = 3;
                    v2.UV0.z = 3;
                    v3.UV0.z = 3;
                    v4.UV0.z = 3;
                    break;
            }
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(NoiseOverrides.ComputeBlockOverride))]
        public static int TexturedComputeBlockOverride(ref NoiseValue noise) {
            if (noise.Position.y > noise.Value ) return (int) TexturedBlock.AIR;
            if (noise.Position.y == noise.Value) return (int) TexturedBlock.GRASS;
            if (noise.Position.y <= noise.Value - 1 && noise.Position.y >= noise.Value - 3) return (int)TexturedBlock.DIRT;

            return (int) TexturedBlock.STONE;
        }

    }

}