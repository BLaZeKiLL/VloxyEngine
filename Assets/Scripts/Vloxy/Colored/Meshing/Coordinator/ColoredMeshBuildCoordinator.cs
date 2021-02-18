using System.Collections.Generic;

using CBSL.Core.Collections.Compressed;

using CodeBlaze.Vloxy.Colored.Data.Block;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Vloxy.Colored.Meshing.Coordinator {

    public class ColoredMeshBuildCoordinator : UniTaskMultiThreadedMeshBuildCoordinator<ColoredBlock> {

        private bool _useCompression;

        public ColoredMeshBuildCoordinator(ChunkBehaviourPool<ColoredBlock> chunkBehaviourPool, int batchSize,
            bool useCompression) : base(chunkBehaviourPool, batchSize) {
            _useCompression = useCompression;
        }

        protected override void PreProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            if (!_useCompression) return;
            
            jobs.ForEach(job => {
                if (((CompressibleChunkData<ColoredBlock>) job.Chunk.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.Chunk.Data).DeCompress();
                if (job.ChunkNX != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNX.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkNX.Data).DeCompress();
                if (job.ChunkNY != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNY.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkNY.Data).DeCompress();
                if (job.ChunkNZ != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNZ.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkNZ.Data).DeCompress();
                if (job.ChunkPX != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPX.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkPX.Data).DeCompress();
                if (job.ChunkPY != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPY.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkPY.Data).DeCompress();
                if (job.ChunkPZ != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPZ.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((CompressibleChunkData<ColoredBlock>) job.ChunkPZ.Data).DeCompress();
            });
        }

        protected override void PostProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            if (!_useCompression) return;
            
            foreach (var batch in CreateBatches(jobs)) {
                ScheduleCompressionJob(batch).Forget();
            }
        }

        private async UniTaskVoid ScheduleCompressionJob(Batch batch) {
            await UniTask.RunOnThreadPool(() => batch.ForEach(job => {
                if (((CompressibleChunkData<ColoredBlock>) job.Chunk.Data).State == CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.Chunk.Data).Compress();
                if (job.ChunkNX != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNX.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkNX.Data).Compress();
                if (job.ChunkNY != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNY.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkNY.Data).Compress();
                if (job.ChunkNZ != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkNZ.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkNZ.Data).Compress();
                if (job.ChunkPX != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPX.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkPX.Data).Compress();
                if (job.ChunkPY != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPY.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkPY.Data).Compress();
                if (job.ChunkPZ != null && ((CompressibleChunkData<ColoredBlock>) job.ChunkPZ.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((CompressibleChunkData<ColoredBlock>) job.ChunkPZ.Data).Compress();
            }));
        }

    }

}