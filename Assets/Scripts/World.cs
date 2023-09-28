using CodeBlaze.Vloxy.Engine.World;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Demo {

    public class World : VloxyWorld {
        
        protected override void WorldInitialize() {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = Settings.Chunk.DrawDistance * 32 - 16;
        }

        public Vector3 GetSpawnPoint() {
            return new Vector3(0, NoiseProfile.GetNoise(int3.zero).Height + 64, 0);
        }

    }

}