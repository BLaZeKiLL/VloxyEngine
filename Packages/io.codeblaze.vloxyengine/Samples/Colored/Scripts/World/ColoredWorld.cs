using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

namespace CodeBlaze.Vloxy.Examples.Colored.World {

    public class ColoredWorld : VloxyWorld {

        protected override VloxyProvider Provider() => new ColoredVloxyProvider();

    }

}