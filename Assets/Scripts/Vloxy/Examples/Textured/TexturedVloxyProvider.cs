﻿using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Mesher;
using CodeBlaze.Vloxy.Engine.Noise;
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

    }

}