using AOT;

using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Samples.Textured.Data;

using Unity.Burst;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Samples.Textured.Components {

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
                case (int) TexturedBlock.GRASS when normal.y is 1:
                    v1.UV0.z = 15;
                    v2.UV0.z = 15;
                    v3.UV0.z = 15;
                    v4.UV0.z = 15;
                    break;
                case (int) TexturedBlock.GRASS when normal.y is -1:
                    v1.UV0.z = 52;
                    v2.UV0.z = 52;
                    v3.UV0.z = 52;
                    v4.UV0.z = 52;
                    break;
                case (int) TexturedBlock.GRASS:
                    v1.UV0.z = 43;
                    v2.UV0.z = 43;
                    v3.UV0.z = 43;
                    v4.UV0.z = 43;
                    break;
                case (int) TexturedBlock.DIRT:
                    v1.UV0.z = 52;
                    v2.UV0.z = 52;
                    v3.UV0.z = 52;
                    v4.UV0.z = 52;
                    break;
                case (int) TexturedBlock.STONE:
                    v1.UV0.z = 39;
                    v2.UV0.z = 39;
                    v3.UV0.z = 39;
                    v4.UV0.z = 39;
                    break;
                case (int) TexturedBlock.WATER:
                    v1.UV0.z = 54;
                    v2.UV0.z = 54;
                    v3.UV0.z = 54;
                    v4.UV0.z = 54;
                    break;
            }
        }

        [BurstCompile]
        [MonoPInvokeCallback(typeof(NoiseOverrides.ComputeBlockOverride))]
        public static int TexturedComputeBlockOverride(ref NoiseValue noise) {
            var Y = noise.Position.y;
            
            // if (Y > noise.Value ) return Y > noise.WaterLevel ? (int) TexturedBlock.AIR : (int) TexturedBlock.WATER;
            if (Y > noise.Value ) return (int) TexturedBlock.AIR;
            if (Y == noise.Value) return (int) TexturedBlock.GRASS;
            if (Y <= noise.Value - 1 && Y >= noise.Value - 3) return (int)TexturedBlock.DIRT;

            return (int) TexturedBlock.STONE;
        }

    }

}