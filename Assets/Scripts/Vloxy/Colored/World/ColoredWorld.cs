﻿using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

namespace CodeBlaze.Vloxy.Colored.World {

    public class ColoredWorld : VloxyWorld {

        protected override VoxelProvider Provider() => new ColoredVoxelProvider();

    }

}