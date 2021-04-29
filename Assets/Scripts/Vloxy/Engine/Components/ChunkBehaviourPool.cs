using System.Collections.Generic;

using CBSL.Core.Collections.Pools;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Data;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkBehaviourPool<B> where B : IBlock {

        private IObjectPool<ChunkBehaviour> _pool;

        private Dictionary<Vector3Int, ChunkBehaviour> _active;

        public ChunkBehaviourPool(Transform transform) {
            var viewRegionSize = 
                (2 *  VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1) *
                (2 *  VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1) *
                (2 *  VoxelProvider<B>.Current.Settings.Chunk.DrawDistance + 1);
            
            _active = new Dictionary<Vector3Int, ChunkBehaviour>(viewRegionSize);
            
            _pool = new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
                viewRegionSize,
                index => {
                    var go = new GameObject("Chunk", typeof(ChunkBehaviour));
                    go.transform.parent = transform;
                    go.SetActive(false);
            
                    var chunkBehaviour = go.GetComponent<ChunkBehaviour>();
                    chunkBehaviour.SetRenderSettings(
                        VoxelProvider<B>.Current.Settings.Renderer,
                        VoxelProvider<B>.Current.Settings.Chunk.ChunkSize / 2
                    );

                    return chunkBehaviour;
                },
                chunkRenderer => chunkRenderer.gameObject.SetActive(true),
                chunkRenderer => chunkRenderer.gameObject.SetActive(false)
            );
            CBSL.Logging.Logger.Info<ChunkBehaviourPool<B>>("Initialized Size : " + viewRegionSize);
        }
        
        public ChunkBehaviour Claim(Chunk<B> chunk) {
            var behaviour = _pool.Claim();

            behaviour.transform.position = chunk.Position;
            behaviour.name = chunk.Name();

            chunk.State = ChunkState.ACTIVE;
            _active.Add(chunk.Position, behaviour);
            
            return behaviour;
        }

        public void Reclaim(Chunk<B> chunk) {
            if (chunk.State == ChunkState.ACTIVE) {
                _pool.Reclaim(_active[chunk.Position]);
                _active.Remove(chunk.Position);
            }
            chunk.State = ChunkState.INACTIVE;
        }

    }

}