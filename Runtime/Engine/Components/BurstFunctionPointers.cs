using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Engine.Components {

    [BurstCompile]
    public struct BurstFunctionPointers {
        
        public FunctionPointer<MeshOverrides.VertexOverride> VertexOverridePointer;

        public FunctionPointer<NoiseOverrides.ComputeBlockOverride> ComputeBlockOverridePointer;

    }

}