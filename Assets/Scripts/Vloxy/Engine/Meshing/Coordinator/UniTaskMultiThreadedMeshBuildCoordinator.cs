using System.Collections.Generic;
using System.Diagnostics;

using CBSL.Extension.UniTask.Threading;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;

using Cysharp.Threading.Tasks;

namespace CodeBlaze.Vloxy.Engine.Meshing.Coordinator {

    public class UniTaskMultiThreadedMeshBuildCoordinator<B> : MeshBuildCoordinator<B> where B : IBlock {

        private bool _useCompression;
        private int _batchSize;

        public UniTaskMultiThreadedMeshBuildCoordinator(ChunkBehaviourPool<B> chunkBehaviourPool, int batchSize, bool useCompression) : base(chunkBehaviourPool) {
            _batchSize = batchSize;
            _useCompression = useCompression;
        }
        
        public override void Process(List<MeshBuildJobData<B>> jobs) => InternalProcess(jobs).Forget();

        protected override void Render(Chunk<B> chunk, MeshData meshData) {
            ChunkBehaviourPool.Claim(chunk.Name(), chunk.Position).Render(meshData);
        }
        
        private async UniTaskVoid InternalProcess(List<MeshBuildJobData<B>> jobs) {
            PreProcess(jobs);

            var watch = new Stopwatch();
            
            watch.Start();
            await BatchScheduler.Process(
                jobs, 
                _batchSize, 
                meshBuildJobData => VoxelProvider<B>.Current.MeshBuilder().GenerateMesh(meshBuildJobData), 
                null,
                (batch, meshData) => {
                    for (int i = 0; i < meshData.Length; i++) {
                        Render(batch.Input[i].Chunk, meshData[i]);
                    }
                });
            watch.Stop();

            CBSL.Logging.Logger.Info<MeshBuildCoordinator<B>>($"Total batch process time : {watch.Elapsed.TotalMilliseconds:0.###} ms");

            PostProcess(jobs);
        }

        protected override void PreProcess(List<MeshBuildJobData<B>> jobs) {
            if (!_useCompression) return;
            
            jobs.ForEach(job => {
                ((CompressibleChunkData<B>) job.Chunk.Data).DeCompress();
                ((CompressibleChunkData<B>) job.ChunkNX?.Data)?.DeCompress();
                ((CompressibleChunkData<B>) job.ChunkNY?.Data)?.DeCompress();
                ((CompressibleChunkData<B>) job.ChunkNZ?.Data)?.DeCompress();
                ((CompressibleChunkData<B>) job.ChunkPX?.Data)?.DeCompress();
                ((CompressibleChunkData<B>) job.ChunkPY?.Data)?.DeCompress();
                ((CompressibleChunkData<B>) job.ChunkPZ?.Data)?.DeCompress();
            });
        }

        protected override void PostProcess(List<MeshBuildJobData<B>> jobs) {
            if (!_useCompression) return;

            BatchScheduler.Process(jobs, jobs.Count, meshBuildJobData => {
                ((CompressibleChunkData<B>) meshBuildJobData.Chunk.Data).Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkNX?.Data)?.Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkNY?.Data)?.Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkNZ?.Data)?.Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkPX?.Data)?.Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkPY?.Data)?.Compress();
                ((CompressibleChunkData<B>) meshBuildJobData.ChunkPZ?.Data)?.Compress();
            }, null, null).Forget();
        }

    }

}