using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Components;
using CodeBlaze.Vloxy.Engine.Jobs.Core;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace CodeBlaze.Vloxy.Engine.Jobs.Collider {

    public class ColliderBuildScheduler : JobScheduler {

        private ChunkPool _ChunkPool;
        
        private NativeList<int> _Jobs;
        private Dictionary<int3, ChunkBehaviour> _Meshes;

        private JobHandle _Handle;

        public ColliderBuildScheduler(ChunkPool chunkPool) {
            _ChunkPool = chunkPool;
            
            _Jobs = new NativeList<int>(Allocator.Persistent);
        }
        
        internal bool IsReady = true;
        
        internal bool IsComplete => _Handle.IsCompleted;

        internal void Start(List<int3> jobs) {
            StartRecord();
            
            IsReady = false;

            _Meshes = _ChunkPool.GetActiveMeshes(jobs);

            foreach (var (_, behaviour) in _Meshes) {
                if (behaviour.Mesh.vertexCount > 0) { // Avoid colliders for empty meshes
                    _Jobs.Add(behaviour.Mesh.GetInstanceID());
                }
            }

            var job = new ColliderBuildJob {
                MeshIDs = _Jobs
            };
            
            _Handle = job.Schedule(_Jobs.Length, 1);
        }

        internal void Complete() {
            _Handle.Complete();
            
            foreach (var (position, behaviour) in _Meshes) {
                if (behaviour.Mesh.vertexCount <= 0) continue;
                
                behaviour.Collider.sharedMesh = behaviour.Mesh;
                _ChunkPool.ColliderBaked(position);
            }
            
            _Jobs.Clear();
            _Meshes = null;
            
            IsReady = true;
            
            StopRecord();
        }

        internal void Dispose() {
            _Handle.Complete();

            _Jobs.Dispose();
        }
        
    }

}