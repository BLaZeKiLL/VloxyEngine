using System;

namespace CodeBlaze.Voxel.Engine.Core.Settings {

    [Serializable]
    public class BuildQueueSettings {

        public BuildMethod ProcessMethod;
        
        public enum BuildMethod {

            MultiThreaded,
            SingleThreaded

        }

    }

}