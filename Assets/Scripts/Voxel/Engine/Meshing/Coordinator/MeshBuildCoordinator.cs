using CodeBlaze.Voxel.Engine.Data;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public abstract class MeshBuildCoordinator<B> where B : IBlock {

        protected readonly ChunkPool<B> ChunkPool; // circular reference

        protected MeshBuildCoordinator(ChunkPool<B> chunkPool) {
            ChunkPool = chunkPool;
        }

        public abstract void Add(ChunkJobData<B> jobData);

        public abstract void Process();
        
        protected abstract void Render(Chunk<B> chunk, MeshData meshData);
        
        protected virtual void PostProcess() { }

    }

}