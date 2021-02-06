using System.Collections.Generic;

using CBSL.Core.Collections.Compressed;

using CodeBlaze.Vloxy.Colored.Data.Block;
using CodeBlaze.Vloxy.Colored.Data.Chunk;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Meshing.Coordinator;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Vloxy.Colored.Meshing.Coordinator {

    public class ColoredMeshBuildCoordinator : UniTaskMultiThreadedMeshBuildCoordinator<ColoredBlock> {

        public ColoredMeshBuildCoordinator(ChunkBehaviourPool<ColoredBlock> chunkBehaviourPool, int batchSize) : base(chunkBehaviourPool, batchSize) { }

        protected override void PreProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            jobs.ForEach(job => {
                if (((ColoredChunkData) job.Chunk.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.Chunk.Data).DeCompress();
                if (job.ChunkNX != null && ((ColoredChunkData) job.ChunkNX.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkNX.Data).DeCompress();
                if (job.ChunkNY != null && ((ColoredChunkData) job.ChunkNY.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkNY.Data).DeCompress();
                if (job.ChunkNZ != null && ((ColoredChunkData) job.ChunkNZ.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkNZ.Data).DeCompress();
                if (job.ChunkPX != null && ((ColoredChunkData) job.ChunkPX.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkPX.Data).DeCompress();
                if (job.ChunkPY != null && ((ColoredChunkData) job.ChunkPY.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkPY.Data).DeCompress();
                if (job.ChunkPZ != null && ((ColoredChunkData) job.ChunkPZ.Data).State == CompressedArray<ColoredBlock>.DataState.COMPRESSED) ((ColoredChunkData) job.ChunkPZ.Data).DeCompress();
            });
        }

        protected override void PostProcess(List<MeshBuildJobData<ColoredBlock>> jobs) {
            foreach (var batch in CreateBatches(jobs)) {
                ScheduleCompressionJob(batch).Forget();
            }
        }

        private async UniTaskVoid ScheduleCompressionJob(Batch batch) {
            await UniTask.RunOnThreadPool(() => batch.ForEach(job => {
                if (((ColoredChunkData) job.Chunk.Data).State == CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.Chunk.Data).Compress();
                if (job.ChunkNX != null && ((ColoredChunkData) job.ChunkNX.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkNX.Data).Compress();
                if (job.ChunkNY != null && ((ColoredChunkData) job.ChunkNY.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkNY.Data).Compress();
                if (job.ChunkNZ != null && ((ColoredChunkData) job.ChunkNZ.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkNZ.Data).Compress();
                if (job.ChunkPX != null && ((ColoredChunkData) job.ChunkPX.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkPX.Data).Compress();
                if (job.ChunkPY != null && ((ColoredChunkData) job.ChunkPY.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkPY.Data).Compress();
                if (job.ChunkPZ != null && ((ColoredChunkData) job.ChunkPZ.Data).State ==
                    CompressedArray<ColoredBlock>.DataState.DECOMPRESSED)
                    ((ColoredChunkData) job.ChunkPZ.Data).Compress();
            }));
        }

    }

}