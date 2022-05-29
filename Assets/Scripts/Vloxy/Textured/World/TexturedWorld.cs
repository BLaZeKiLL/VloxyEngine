using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.World;

namespace CodeBlaze.Vloxy.Textured.Vloxy.Textured.World {

    public class TexturedWorld : VloxyWorld {

        protected override VloxyProvider Provider() => new TexturedVloxyProvider();

    }

}