using System.Collections.Generic;
using System.Linq;

using CBSL.Core.Collections.Pools;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkBehaviourPool<B> where B : IBlock {

        private IObjectPool<ChunkBehaviour> _pool;

        private Dictionary<Vector3Int, ChunkBehaviour> _active;
        
        public int Size { get; }
        
        public ChunkBehaviourPool(Transform transform) {
            Size = 
                (2 * VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1) *
                (2 * VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1) *
                (2 * VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1);
            
            _active = new Dictionary<Vector3Int, ChunkBehaviour>(Size);
            
            _pool = new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
                Size,
                index => {
                    var go = new GameObject("Chunk", typeof(ChunkBehaviour));
                    go.transform.parent = transform;
                    go.SetActive(false);
            
                    var chunkBehaviour = go.GetComponent<ChunkBehaviour>();
                    chunkBehaviour.SetRenderSettings(VoxelProvider<B>.Current.Settings.Renderer);

                    return chunkBehaviour;
                },
                chunkRenderer => chunkRenderer.gameObject.SetActive(true),
                chunkRenderer => chunkRenderer.gameObject.SetActive(false)
            );
            CBSL.Logging.Logger.Info<ChunkBehaviourPool<B>>("Initialized Size : " + Size);
        }

        public List<Vector3Int> PoolUpdate(Vector3Int focus) {
            var current = new List<Vector3Int>(Size);

            var world = VoxelProvider<B>.Current.Settings.Chunk;
            
            for (int x = -world.DrawDistance; x <= world.DrawDistance; x++) {
                for (int z = -world.DrawDistance; z <= world.DrawDistance; z++) {
                    for (int y = -world.DrawDistance; y <= world.DrawDistance; y++) {
                        current.Add(focus + new Vector3Int(x, y, z) * world.ChunkSize);
                    }
                }
            }

            var reclaim = _active.Keys.Where(x => !current.Contains(x)).ToList();
            var claim = current.Where(x => !_active.Keys.Contains(x)).ToList();
            
            CBSL.Logging.Logger.Info<ChunkBehaviourPool<B>>($"Reclaim : {reclaim.Count}, Claim : {claim.Count}");
            
            foreach (var x in reclaim) {
                Reclaim(x);
            }

            return claim;
        }

        public ChunkBehaviour Claim(string name, Vector3Int position) {
            var behaviour = _pool.Claim();

            behaviour.transform.position = position;
            behaviour.name = name;
            
            _active.Add(position, behaviour);
            
            return behaviour;
        }

        public void Reclaim(Vector3Int position) {
            _pool.Reclaim(_active[position]);

            _active.Remove(position);
        }

    }

}