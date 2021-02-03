using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Test.TestBed {

    public class TestNoiseProfile2D : FastNoiseProfile2D<TestBlock> {

        public TestNoiseProfile2D(NoiseSettings2D settings, Vector3Int chunkSize) : base(settings, chunkSize) { }

        protected override TestBlock GetBlock(int heightMapValue, int blockHeight) {
            return heightMapValue >= blockHeight ? new TestBlock(TestBlockType.Stone) : new TestBlock(TestBlockType.Air);
        }

    }

}