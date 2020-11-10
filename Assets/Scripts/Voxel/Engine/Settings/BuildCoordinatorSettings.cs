using System;

namespace CodeBlaze.Voxel.Engine.Settings {

    [Serializable]
    public class BuildCoordinatorSettings {

        public BuildMethod ProcessMethod;
        
        public enum BuildMethod {

            MultiThreaded,
            SingleThreaded

        }

    }

}