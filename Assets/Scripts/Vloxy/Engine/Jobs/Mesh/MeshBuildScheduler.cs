using System;
using System.Collections.Generic;
using System.Diagnostics;

using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine.Rendering;

namespace CodeBlaze.Vloxy.Engine.Jobs.Mesh {

    public class MeshBuildScheduler {

        internal bool Processing { get; private set; }

        private readonly ChunkState _ChunkState;
        private readonly ChunkAccessor _ChunkAccessor;
        private readonly ChunkBehaviourPool _ChunkBehaviourPool;
        private readonly BurstFunctionPointers _BurstFunctionPointers;
        
        private int _BatchSize;
        private int3 _ChunkSize;
        private Queue<int3> _Queue;
        
        private JobHandle _Handle;

        private NativeList<int3> _Jobs;
        private NativeParallelHashMap<int3, int> _Results;
        private UnityEngine.Mesh.MeshDataArray _MeshDataArray;
        private NativeArray<VertexAttributeDescriptor> _VertexParams;

        private bool _Scheduled;

#if VLOXY_LOGGING
        private Stopwatch _Watch;
#endif
        
        public MeshBuildScheduler(
            VloxySettings settings,
            ChunkState chunkState,
            ChunkAccessor chunkAccessor,
            ChunkBehaviourPool chunkBehaviourPool, 
            BurstFunctionPointers burstFunctionPointers
        ) {
            _BatchSize = settings.Scheduler.BatchSize;
            _ChunkSize = settings.Chunk.ChunkSize;

            _ChunkState = chunkState;
            _ChunkAccessor = chunkAccessor;
            _ChunkBehaviourPool = chunkBehaviourPool;
            _BurstFunctionPointers = burstFunctionPointers;

            // TODO : Make Configurable
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
            _Queue = new Queue<int3>();

#if VLOXY_LOGGING
            _Watch = new Stopwatch();
#endif
        }
        
        internal bool Update() {
            if (_Scheduled || _Queue.Count <= 0) return false;

            Process();

            return true;
        }

        internal bool LateUpdate() {
            return _Scheduled && Complete();
        } 
        
        public void Schedule(List<int3> jobs) {
            for (int i = 0; i < jobs.Count; i++) {
                _Queue.Enqueue(jobs[i]);
            }
            
            Processing = _Queue.Count > 0;
        }

        private void Process() {
            var count = _BatchSize;

            while (count > 0 && _Queue.Count > 0) {
                _Jobs.Add(_Queue.Dequeue());
                count--;
            }
            
#if VLOXY_LOGGING
            _Watch.Restart();
#endif
            
            _MeshDataArray = UnityEngine.Mesh.AllocateWritableMeshData(_Jobs.Length);

            var job = new MeshBuildJob {
                BurstFunctionPointers = _BurstFunctionPointers,
                Accessor = _ChunkAccessor,
                ChunkSize = _ChunkSize,
                Jobs = _Jobs,
                VertexParams = _VertexParams,
                MeshDataArray = _MeshDataArray,
                Results = _Results.AsParallelWriter()
            };

            _Handle = job.Schedule(_Jobs.Length, 1);

            _Scheduled = true;
        }
        
        private bool Complete() {
            if (!_Handle.IsCompleted) return false;

            _Handle.Complete();

            var meshes = new UnityEngine.Mesh[_Jobs.Length];

            for (var index = 0; index < _Jobs.Length; index++) {
                var position = _Jobs[index];
                
                if (_ChunkState.GetState(position) == ChunkState.State.MESHING) {
                    meshes[_Results[position]] = _ChunkBehaviourPool.Claim(position).Mesh();
                    _ChunkState.SetState(position, ChunkState.State.ACTIVE);
                } else { // This is unnecessary, how can we avoid this ? 
                    meshes[_Results[position]] = new UnityEngine.Mesh();
#if VLOXY_LOGGING
                    VloxyLogger.Warn<MeshBuildScheduler>($"Redundant Mesh : {position} : {_ChunkState.GetState(position)}");
#endif
                }
            }

            UnityEngine.Mesh.ApplyAndDisposeWritableMeshData(_MeshDataArray, meshes, MeshUpdateFlags.DontRecalculateBounds);
            
            for (var index = 0; index < meshes.Length; index++) {
                meshes[index].RecalculateBounds();
            }
            
            _Results.Clear();
            _Jobs.Clear();

            _Scheduled = false;
            
            Processing = _Queue.Count > 0;
            
#if VLOXY_LOGGING
            _Watch.Stop();
            VloxyLogger.Info<MeshBuildScheduler>($"Meshes built : {meshes.Length}, In : {_Watch.ElapsedMilliseconds} MS");
#endif

            return true;
        }
        
        public void Dispose() {
            _VertexParams.Dispose();
            _Results.Dispose();
            _Jobs.Dispose();
        }

    }

}