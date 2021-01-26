using System.Collections.Generic;
using System.Linq;

using CodeBlaze.Library.Collections.Pools;
using CodeBlaze.Voxel.Engine.Behaviour;

using UnityEngine;

namespace CodeBlaze.Voxel.Engine.Data {

    public class ChunkPool<B> where B : IBlock {

        private IObjectPool<ChunkBehaviour> _pool;

        private Dictionary<Vector3Int, ChunkBehaviour> _active;
        
        public int Size { get; }
        
        public ChunkPool(Transform transform) {
            Size = 
                (2 * VoxelProvider<B>.Current.Settings.World.DrawDistance + 1) *
                (2 * VoxelProvider<B>.Current.Settings.World.DrawDistance + 1) *
                (2 * VoxelProvider<B>.Current.Settings.World.DrawDistance + 1);
            Debug.Log("[ChunkPool][Start] Pool Size : " + Size);
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
        }

        public IEnumerable<Vector3Int> Update(Vector3Int focus) {
            var current = new List<Vector3Int>(Size);

            var world = VoxelProvider<B>.Current.Settings.World;
            
            for (int x = -world.DrawDistance; x <= world.DrawDistance; x++) {
                for (int z = -world.DrawDistance; z <= world.DrawDistance; z++) {
                    for (int y = -world.DrawDistance; y <= world.DrawDistance; y++) {
                        current.Add(focus + new Vector3Int(x, y, z) * world.ChunkSize);
                    }
                }
            }

            var reclaim = _active.Keys.Where(x => !current.Contains(x)).ToList();
            var claim = current.Where(x => !_active.Keys.Contains(x)).ToList();
            
            Debug.Log($"[ChunkPool][Update] Reclaim : {reclaim.Count} Claim : {claim.Count}");
            
            foreach (var x in reclaim) {
                Reclaim(x);
            }

            return claim;
        }

        public ChunkBehaviour Claim(Chunk<B> chunk) {
            var behaviour = _pool.Claim();

            behaviour.transform.position = chunk.Position;
            behaviour.name = chunk.Name();
            
            _active.Add(chunk.Position, behaviour);
            
            return behaviour;
        }

        public void Reclaim(Vector3Int position) {
            _pool.Reclaim(_active[position]);

            _active.Remove(position);
        }

    }

}