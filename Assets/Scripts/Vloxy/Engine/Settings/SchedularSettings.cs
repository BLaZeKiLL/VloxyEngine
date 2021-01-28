using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [Serializable]
    public class SchedularSettings {

        [Tooltip("Schedular Batch Size")]
        public int BatchSize = 32;

    }

}