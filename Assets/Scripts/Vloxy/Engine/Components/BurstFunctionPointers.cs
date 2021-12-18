using CodeBlaze.Vloxy.Engine.Mesher;

using Unity.Burst;

namespace CodeBlaze.Vloxy.Engine.Components {

    [BurstCompile]
    public struct BurstFunctionPointers {

        public FunctionPointer<MeshExtensions.VertexOverride> VertexOverridePointer;

    }

}