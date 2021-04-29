using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Engine.Mesher;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Meshing.Builder {

    public class ColoredGreedyMesher : GreedyMesher<ColoredBlock> {

        protected override void CreateQuad(Mask mask, Vector3Int normal) {
            MeshData.Colors.Add(mask.Block.Color);
            MeshData.Colors.Add(mask.Block.Color);
            MeshData.Colors.Add(mask.Block.Color);
            MeshData.Colors.Add(mask.Block.Color);
        }

        protected override bool CompareBlock(ColoredBlock block1, ColoredBlock block2) {
            var color1 = block1.Color;
            var color2 = block2.Color;
        
            return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b && color1.a == color2.a;
        }

    }

}