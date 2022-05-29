using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Examples.Textured.Components;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Examples.Textured {

    public class TexturedVloxyProvider : VloxyProvider {
        
        public override BurstFunctionPointers SetupBurstFunctionPointers() {
            return new BurstFunctionPointers {
                VertexOverridePointer = BurstCompiler.CompileFunctionPointer<MeshOverrides.VertexOverride>(TexturedBurstFunctions.TexturedVertexOverride),
                ComputeBlockOverridePointer = BurstCompiler.CompileFunctionPointer<NoiseOverrides.ComputeBlockOverride>(TexturedBurstFunctions.TexturedComputeBlockOverride)
            };
        }

        public override NoiseProfile NoiseProfile() {
            var settings = (NoiseSettings) Settings.NoiseSettings;

            return new NoiseProfile(new NoiseProfile.Settings {
                Height = settings.Height,
                Seed = settings.Seed,
                Scale = settings.Scale,
                Lacunarity = settings.Lacunarity,
                Persistance = settings.Persistance,
                Octaves = settings.Octaves,
            });
        }

    }

}