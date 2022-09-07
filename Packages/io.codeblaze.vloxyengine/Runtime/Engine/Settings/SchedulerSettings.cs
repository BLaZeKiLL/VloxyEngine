using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [Serializable]
    public class SchedulerSettings {

        [HideInInspector]
        public int MeshingBatchSize;
        
        [HideInInspector]
        public int StreamingBatchSize;

    }

}