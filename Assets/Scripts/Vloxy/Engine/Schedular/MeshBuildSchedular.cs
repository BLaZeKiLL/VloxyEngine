using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Schedular {

    public abstract class MeshBuildSchedular<B> where B : IBlock {

        protected readonly ChunkBehaviourPool<B> ChunkBehaviourPool;

        protected MeshBuildSchedular(ChunkBehaviourPool<B> chunkBehaviourPool) {
            ChunkBehaviourPool = chunkBehaviourPool;
        }

        public abstract void Schedule(List<MeshBuildJobData<B>> jobs);
        
        protected abstract void Render(Chunk<B> chunk, MeshData meshData);
        
        protected virtual void PreProcess(List<MeshBuildJobData<B>> jobs) { }
        
        protected virtual void PostProcess(List<MeshBuildJobData<B>> jobs) { }

    }

}