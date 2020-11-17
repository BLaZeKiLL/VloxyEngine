using System;

using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Engine;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Colored.World {

    public class ColoredWorld : World<ColoredBlock> {

        protected override Func<VoxelProvider<ColoredBlock>> Provider() => () => new ColoredVoxelProvider();

    }

}