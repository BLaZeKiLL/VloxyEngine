using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public abstract class MeshBuildCoordinator<B> where B : IBlock {

        protected readonly ChunkBehaviourPool<B> ChunkBehaviourPool; // circular reference

        protected MeshBuildCoordinator(ChunkBehaviourPool<B> chunkBehaviourPool) {
            ChunkBehaviourPool = chunkBehaviourPool;
        }

        public abstract void Process(List<MeshBuildJobData<B>> jobs);
        
        protected abstract void Render(Chunk<B> chunk, MeshData meshData);
        
        protected virtual void PreProcess(List<MeshBuildJobData<B>> jobs) { }
        
        protected virtual void PostProcess(List<MeshBuildJobData<B>> jobs) { }

    }

}