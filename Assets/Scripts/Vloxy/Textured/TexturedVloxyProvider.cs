using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Textured.Components;
using CodeBlaze.Vloxy.Textured.Noise;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Textured.Vloxy.Textured {

    public class TexturedVloxyProvider : VloxyProvider {

        public override INoiseProfile NoiseProfile() => new TexturedNoiseProfile2D(Settings.NoiseSettings, Settings.Chunk);

        public override BurstFunctionPointers SetupBurstFunctionPointers() {
            return new BurstFunctionPointers {
                VertexOverridePointer = BurstCompiler.CompileFunctionPointer<MeshExtensions.VertexOverride>(TexturedBurstFunctions.TexturedVertexOverride)
            };
        }

    }

}