using CodeBlaze.Vloxy.Engine.Settings;

namespace CodeBlaze.Vloxy.Engine.Tests.Editor.Tests.Editor.TestBed {

    public class TestVloxyProvider : VloxyProvider {

        public static TestVloxyProvider Create() {
            return new TestVloxyProvider {
                Settings = new VloxySettings {
                    Chunk = new ChunkSettings(),
                    Noise = new NoiseSettings(),
                    Renderer = new RendererSettings(),
                    Scheduler = new SchedulerSettings()
                }
            };
        }

    }

}