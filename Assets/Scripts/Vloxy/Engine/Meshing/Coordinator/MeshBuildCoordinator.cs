using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Data;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public abstract class MeshBuildCoordinator<B> where B : IBlock {

        protected readonly ChunkPool<B> ChunkPool; // circular reference

        protected MeshBuildCoordinator(ChunkPool<B> chunkPool) {
            ChunkPool = chunkPool;
        }

        public abstract void Process(List<ChunkJobData<B>> jobs);
        
        protected abstract void Render(Chunk<B> chunk, MeshData meshData);
        
        protected virtual void PostProcess() { }

    }

}