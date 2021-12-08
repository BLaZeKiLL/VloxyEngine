using System.Collections.Generic;

using CBSL.Core.Collections.Pools;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

using Unity.Mathematics;

using UnityEngine;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkBehaviourPool {

        private IObjectPool<ChunkBehaviour> _pool;

        private Dictionary<int3, ChunkBehaviour> _active;

        public ChunkBehaviourPool(Transform transform, VoxelSettings settings) {
            var viewRegionSize = settings.Chunk.DrawDistance.CubedSize();
            
            _active = new Dictionary<int3, ChunkBehaviour>(viewRegionSize);
            
            _pool = new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
                viewRegionSize,
                _ => {
                    var go = new GameObject("Chunk", typeof(ChunkBehaviour));
                    go.transform.parent = transform;
                    go.SetActive(false);
            
                    var chunkBehaviour = go.GetComponent<ChunkBehaviour>();
                    chunkBehaviour.SetRenderSettings(settings.Renderer);

                    return chunkBehaviour;
                },
                chunkRenderer => chunkRenderer.gameObject.SetActive(true),
                chunkRenderer => chunkRenderer.gameObject.SetActive(false)
            );
            
            CBSL.Logging.Logger.Info<ChunkBehaviourPool>("Initialized Size : " + viewRegionSize);
        }
        
        public ChunkBehaviour Claim(int3 pos) {
            var behaviour = _pool.Claim();

            behaviour.transform.position = pos.GetVector3();
            behaviour.name = $"Chunk({pos})";

            _active.Add(pos, behaviour);
            
            return behaviour;
        }

        public void Reclaim(int3 pos) {
            _pool.Reclaim(_active[pos]);
            _active.Remove(pos);
        }

    }

}