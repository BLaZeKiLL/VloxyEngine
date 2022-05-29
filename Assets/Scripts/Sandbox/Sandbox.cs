using System;

using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Sandbox {

    public class Sandbox : MonoBehaviour {

        private void Start() {
            var random = new Unity.Mathematics.Random(1337);
            var profile = new NoiseProfile(new NoiseProfile.Settings {
                Height = 256,
                Seed = 1337,
                Scale = 50,
                Lacunarity = 2f,
                Persistance = 0.5f,
                Octaves = 6
            });

            for (int i = 0; i < 10; i++) {
                var noise = profile.GetNoise(random.NextInt3(new int3(64, 256, 64)));
            
                VloxyLogger.Info<Sandbox>($"Value : {noise.Value}");
            }
        }

    }

}