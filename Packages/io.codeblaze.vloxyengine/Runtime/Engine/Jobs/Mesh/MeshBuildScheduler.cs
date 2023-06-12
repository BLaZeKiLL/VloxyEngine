using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Jobs.Core;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Jobs.Mesh {

    public class MeshBuildScheduler : JobScheduler {

        private readonly ChunkStore _ChunkStore;
        private readonly ChunkPool _ChunkPool;

        private int3 _ChunkSize;
        private JobHandle _Handle;

        private NativeList<int3> _Jobs;
        private ChunkAccessor _ChunkAccessor;
        private NativeParallelHashMap<int3, int> _Results;
        private UnityEngine.Mesh.MeshDataArray _MeshDataArray;
        private NativeArray<VertexAttributeDescriptor> _VertexParams;

        public MeshBuildScheduler(
            VloxySettings settings,
            ChunkStore chunkStore,
            ChunkPool chunkPool
        ) {
            _ChunkStore = chunkStore;
            _ChunkPool = chunkPool;

            _ChunkSize = settings.Chunk.ChunkSize;

            // TODO : Make Configurable (Source Generators)
            _VertexParams = new NativeArray<VertexAttributeDescriptor>(6, Allocator.Persistent);
            
            // Int interpolation cause issues
            _VertexParams[0] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            _VertexParams[1] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3);
            _VertexParams[2] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4);
            _VertexParams[3] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 3);
            _VertexParams[4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2);
            _VertexParams[5] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 4);
            
            _Results = new NativeParallelHashMap<int3, int>(settings.Chunk.DrawDistance.CubedSize(),Allocator.Persistent);
            _Jobs = new NativeList<int3>(Allocator.Persistent);
        }

        internal bool IsReady = true;
        internal bool IsComplete => _Handle.IsCompleted;

        internal void Start(List<int3> jobs) {
            StartRecord();
            
            IsReady = false;

            _ChunkAccessor = _ChunkStore.GetAccessor(jobs);
            
            foreach (var j in jobs) {
                _Jobs.Add(j);
            }
            
            _MeshDataArray = UnityEngine.Mesh.AllocateWritableMeshData(_Jobs.Length);

            var job = new MeshBuildJob {
                Accessor = _ChunkAccessor,
                ChunkSize = _ChunkSize,
                Jobs = _Jobs,
                VertexParams = _VertexParams,
                MeshDataArray = _MeshDataArray,
                Results = _Results.AsParallelWriter()
            };

            _Handle = job.Schedule(_Jobs.Length, 1);
        }
        
        internal void Complete() {
            _Handle.Complete();

            var meshes = new UnityEngine.Mesh[_Jobs.Length];

            for (var index = 0; index < _Jobs.Length; index++) {
                var position = _Jobs[index];
                
                meshes[_Results[position]] = _ChunkPool.Claim(position).Mesh;
            }

            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(
                _MeshDataArray, 
                meshes, 
                MeshUpdateFlags.DontRecalculateBounds
            );
            
            for (var index = 0; index < meshes.Length; index++) {
                meshes[index].RecalculateBounds();
            }
            
            _ChunkAccessor.Dispose();
            _Results.Clear();
            _Jobs.Clear();
            
            IsReady = true;
            
            StopRecord();
        }
        
        internal void Dispose() {
            _Handle.Complete();
            
            _VertexParams.Dispose();
            _Results.Dispose();
            _Jobs.Dispose();
        }

    }

}