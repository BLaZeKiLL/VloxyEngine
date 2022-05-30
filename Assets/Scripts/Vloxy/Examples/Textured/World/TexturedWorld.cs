using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

namespace CodeBlaze.Vloxy.Examples.Textured.World {

    public class TexturedWorld : VloxyWorld {

        protected override VloxyProvider Provider() => new TexturedVloxyProvider();

    }

}