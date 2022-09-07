using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Samples.Colored.Components;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Samples.Colored {

    public class ColoredVloxyProvider : VloxyProvider {
        
        public override BurstFunctionPointers SetupBurstFunctionPointers() {
            return new BurstFunctionPointers {
                VertexOverridePointer = BurstCompiler.CompileFunctionPointer<MeshOverrides.VertexOverride>(ColoredBurstFunctions.ColoredVertexOverride),
                ComputeBlockOverridePointer = BurstCompiler.CompileFunctionPointer<NoiseOverrides.ComputeBlockOverride>(ColoredBurstFunctions.ColoredComputeBlockOverride)
            };
        }

    }

}