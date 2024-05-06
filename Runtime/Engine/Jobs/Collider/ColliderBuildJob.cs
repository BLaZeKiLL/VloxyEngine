using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Jobs.Collider {

    /// <summary>
    /// It would be so much better if we can merge this job with mesh generation
    /// https://forum.unity.com/threads/can-we-have-a-overload-of-physics-bakemesh-that-works-with-meshdata-struct.1429576/
    /// </summary>
    [BurstCompile]
    internal struct ColliderBuildJob : IJobParallelFor {

        [ReadOnly] public NativeList<int> MeshIDs;

        public void Execute(int index) {
            Physics.BakeMesh(MeshIDs[index], false);
        }

    }

}