using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

using UnityEngine;

namespace CodeBlaze.Vloxy.Examples.Textured.World {

    public class TexturedWorld : VloxyWorld {

        protected override VloxyProvider Provider() => new TexturedVloxyProvider();

        protected override void WorldInitialize() {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = Settings.Chunk.DrawDistance * 32 - 16;
        }

    }

}