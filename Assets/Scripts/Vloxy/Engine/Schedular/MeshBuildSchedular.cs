using CodeBlaze.Vloxy.Engine.Components;

using Unity.Collections;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Schedular {

    public abstract class MeshBuildSchedular {

        protected readonly ChunkBehaviourPool ChunkBehaviourPool;

        protected MeshBuildSchedular(ChunkBehaviourPool chunkBehaviourPool) {
            ChunkBehaviourPool = chunkBehaviourPool;
        }

        public abstract void Schedule(NativeArray<int3> jobs, NativeChunkStoreAccessor accessor);

        public abstract void Complete();
        
        //protected abstract void Render(Chunk chunk, MeshData meshData);

    }

}