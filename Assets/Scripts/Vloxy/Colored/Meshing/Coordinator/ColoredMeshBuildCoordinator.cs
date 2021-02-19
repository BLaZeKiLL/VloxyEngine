using System.Collections.Generic;

using CBSL.Extension.UniTask.Threading;

using CodeBlaze.Vloxy.Colored.Data.Block;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Vloxy.Colored.Meshing.Coordinator {

    public class ColoredMeshBuildCoordinator : UniTaskMultiThreadedMeshBuildCoordinator<ColoredBlock> {

        private bool _useCompression;

        private int _batchSize;

        public ColoredMeshBuildCoordinator(ChunkBehaviourPool<ColoredBlock> chunkBehaviourPool, int batchSize,
            bool useCompression) : base(chunkBehaviourPool, batchSize) {
            _useCompression = useCompression;
            _batchSize = batchSize;
        }

        protected override void PreProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            if (!_useCompression) return;
            
            jobs.ForEach(job => {
                ((CompressibleChunkData<ColoredBlock>) job.Chunk.Data).DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkNX?.Data)?.DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkNY?.Data)?.DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkNZ?.Data)?.DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkPX?.Data)?.DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkPY?.Data)?.DeCompress();
                ((CompressibleChunkData<ColoredBlock>) job.ChunkPZ?.Data)?.DeCompress();
            });
        }

        protected override void PostProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            if (!_useCompression) return;

            BatchScheduler.Process(jobs, _batchSize, meshBuildJobData => {
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.Chunk.Data).Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkNX?.Data)?.Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkNY?.Data)?.Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkNZ?.Data)?.Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkPX?.Data)?.Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkPY?.Data)?.Compress();
                ((CompressibleChunkData<ColoredBlock>) meshBuildJobData.ChunkPZ?.Data)?.Compress();
            }, null, null).Forget();
        }

    }

}