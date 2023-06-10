using CodeBlaze.Vloxy.Engine.World;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Demo {

    public class World : VloxyWorld {
        
        protected override void WorldInitialize() {
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogEndDistance = Settings.Chunk.DrawDistance * 32 - 16;
        }

        protected override void WorldAwake() {
            var height = NoiseProfile.GetNoise(int3.zero).Height + 16;
            
            Focus.transform.SetPositionAndRotation(new Vector3(0, height, 0), Quaternion.Euler(0, 0 ,0));
        }

    }

}