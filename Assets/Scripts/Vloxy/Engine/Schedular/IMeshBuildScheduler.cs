using CodeBlaze.Vloxy.Engine.Components;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Scheduler {

    public interface IMeshBuildScheduler {

        void Schedule(NativeArray<int3> jobs, ChunkStoreAccessor accessor);
        
        void Complete();

        void Dispose();

    }

}