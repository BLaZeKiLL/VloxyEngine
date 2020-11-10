using CodeBlaze.Voxel.Colored.Block;
using CodeBlaze.Voxel.Colored.Chunk;
using CodeBlaze.Voxel.Colored.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Chunk;
using CodeBlaze.Voxel.Engine.Meshing;
using CodeBlaze.Voxel.Engine.Meshing.Builder;
using CodeBlaze.Voxel.Engine.Meshing.Coordinator;
using CodeBlaze.Voxel.Engine.World;

namespace CodeBlaze.Voxel.Colored.Meshing.Coordinator {

    public class ColoredUniTaskMultiThreadedMeshBuildCoordinator : UniTaskMultiThreadedMeshBuildCoordinator<ColoredBlock> {

        public ColoredUniTaskMultiThreadedMeshBuildCoordinator(World<ColoredBlock> world) : base(world) { }

        protected override IMeshBuilder<ColoredBlock> MeshBuilderProvider() => new ColoredGreedyMeshBuilder();

        protected override void Render(Chunk<ColoredBlock> chunk, MeshData data) {
            if (!(chunk is ColoredChunk coloredChunk)) return;

            var renderer = World.RendererPool.Claim();
            renderer.transform.position = coloredChunk.Position;
            renderer.name += $" {coloredChunk.ID}";
            renderer.Render(data);
        }

    }

}