using System;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Settings {

    [Serializable]
    public class SchedulerSettings {

        [Tooltip("Scheduler Batch Size")]
        public int BatchSize = 32;

    }

}