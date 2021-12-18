using CodeBlaze.Vloxy.Colored.Components;
using CodeBlaze.Vloxy.Colored.Data;
using CodeBlaze.Vloxy.Colored.Noise;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Colored {

    public class ColoredVloxyProvider : VloxyProvider {

        public override INoiseProfile NoiseProfile() => new ColoredNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk);

        public override BurstFunctionPointers SetupBurstFunctionPointers() {
            return new BurstFunctionPointers {
                VertexOverridePointer = BurstCompiler.CompileFunctionPointer<MeshExtensions.VertexOverride>(ColoredBurstFunctions.ColoredVertexOverride)
            };
        }

    }

}