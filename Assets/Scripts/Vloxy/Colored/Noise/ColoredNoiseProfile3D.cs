using CodeBlaze.Vloxy.Colored.Data;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Noise.Settings;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Colored.Noise {

    public class ColoredNoiseProfile3D : FastNoiseProfile3D {

        private static readonly int COLOR = ColoredBlocks.RandomColor();

        public ColoredNoiseProfile3D(INoiseSettings settings, ChunkSettings chunkSettings) : base(settings, chunkSettings) { }

        protected override int GetBlock(byte value) {
            return value == 0 ? 0 : COLOR;
        }

    }

}