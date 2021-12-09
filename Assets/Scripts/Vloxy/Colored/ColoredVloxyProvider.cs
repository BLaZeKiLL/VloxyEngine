using CodeBlaze.Vloxy.Colored.Noise;

using CodeBlaze.Vloxy.Engine;
using CodeBlaze.Vloxy.Engine.Noise.Profile;
using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Colored {

    public class ColoredVloxyProvider : VloxyProvider {

        public override INoiseProfile NoiseProfile() => new ColoredNoiseProfile2D(Settings.NoiseSettings as NoiseSettings2D, Settings.Chunk);
        
    }

}