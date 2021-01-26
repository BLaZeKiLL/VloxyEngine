using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Engine.Noise.Profile;
using CodeBlaze.Voxel.Engine.Noise.Settings;

namespace CodeBlaze.Voxel.Colored.Noise {

    public class ColoredNoiseProfile2D : FastNoiseProfile2D<ColoredBlock> {

        public ColoredNoiseProfile2D(NoiseSettings2D settings) : base(settings) { }
        
        protected override ColoredBlock GetBlock(int heightMapValue, int blockHeight) {
            return heightMapValue >= blockHeight ? ColoredBlockTypes.Red() : ColoredBlockTypes.Air();
        }

    }

}