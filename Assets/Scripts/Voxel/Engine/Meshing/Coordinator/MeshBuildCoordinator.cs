using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Engine.Meshing.Coordinator {

    public abstract class MeshBuildCoordinator<B> where B : IBlock {

        protected readonly World<B> World; // circular reference

        protected MeshBuildCoordinator(World<B> world) {
            World = world;
        }

        public abstract void Add(Chunk<B> chunk);

        public abstract void Process();

        protected abstract IMeshBuilder<B> MeshBuilderProvider();

        protected abstract void Render(Chunk<B> chunk, MeshData data);
        
        protected virtual void PostProcess() { }

    }

}