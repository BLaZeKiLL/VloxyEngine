using CodeBlaze.Vloxy.Colored.Block;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;

using UnityEngine;

namespace CodeBlaze.Vloxy.Colored.Noise {

    public class ColoredNoiseProfile2D : FastNoiseProfile2D<ColoredBlock> {

        public ColoredNoiseProfile2D(NoiseSettings2D settings, Vector3Int chunkSize) : base(settings, chunkSize) { }
        
        protected override ColoredBlock GetBlock(int heightMapValue, int blockHeight) {
            return heightMapValue >= blockHeight ? ColoredBlockTypes.Red() : ColoredBlockTypes.Air();
        }

    }

}