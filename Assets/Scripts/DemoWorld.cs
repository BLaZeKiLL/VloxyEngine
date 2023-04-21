using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.World;

using UnityEngine;

namespace CodeBlaze.Vloxy.Demo {

    public class DemoWorld : VloxyWorld {
        
        protected override void WorldInitialize() {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = Settings.Chunk.DrawDistance * 32 - 16;
        }

    }

}