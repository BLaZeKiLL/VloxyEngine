using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise.Profile;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Engine.Components {

    [BurstCompile]
    public struct BurstFunctionPointers {
        
        public FunctionPointer<MeshOverrides.VertexOverride> VertexOverridePointer;

        public FunctionPointer<NoiseOverrides.ComputeBlockOverride> ComputeBlockOverridePointer;

    }

}