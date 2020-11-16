using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Engine.Meshing.Builder;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.Meshing.Builder {

    public class ColoredGreedyMeshBuilder : GreedyMeshBuilder<ColoredBlock> {

        protected override ColoredBlock EmptyBlock() => ColoredBlockTypes.Air();

        protected override ColoredBlock NullBlock() => ColoredBlockTypes.Null();

        protected override void CreateQuad(ColoredBlock block, Vector3Int normal) {
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
        }

        protected override bool CompareBlock(ColoredBlock block1, ColoredBlock block2) {
            return true;
            
            var color1 = block1.Color;
            var color2 = block2.Color;

            return color1.r == color2.r && color1.g == color2.g && color1.b == color2.b && color1.a == color2.a;
        }

    }

}