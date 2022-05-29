using CodeBlaze.Vloxy.Colored.Components;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Colored {

    public class ColoredVloxyProvider : VloxyProvider {
        
        public override BurstFunctionPointers SetupBurstFunctionPointers() {
            return new BurstFunctionPointers {
                VertexOverridePointer = BurstCompiler.CompileFunctionPointer<MeshOverrides.VertexOverride>(ColoredBurstFunctions.ColoredVertexOverride),
                ComputeBlockOverridePointer = BurstCompiler.CompileFunctionPointer<NoiseOverrides.ComputeBlockOverride>(ColoredBurstFunctions.ColoredComputeBlockOverride)
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