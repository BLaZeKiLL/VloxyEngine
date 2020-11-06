using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Engine.Core.Mesher;

using UnityEngine;

namespace CodeBlaze.Voxel.Colored.Mesher {

    public class ColoredGreedyMesher : GreedyMesher<ColoredBlock> {

        protected override ColoredBlock EmptyBlock() => ColoredBlockTypes.Air();

        protected override ColoredBlock NullBlock() => ColoredBlockTypes.Null();

        protected override void CreateQuad(ColoredBlock block, Vector3Int normal) {
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
            MeshData.Colors.Add(block.Color);
        }

    }

}