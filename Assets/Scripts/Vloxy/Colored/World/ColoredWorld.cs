using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

namespace CodeBlaze.Vloxy.Colored.World {

    public class ColoredWorld : World<ColoredBlock> {

        protected override VoxelProvider<ColoredBlock> Provider() => new ColoredVoxelProvider();

    }

}