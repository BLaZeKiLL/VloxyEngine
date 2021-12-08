﻿using System.Collections.Generic;

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
                    chunkBehaviour.SetRenderSettings(settings.Renderer, settings.Chunk.ChunkSize / 2);

                    return chunkBehaviour;
                },
                chunkRenderer => chunkRenderer.gameObject.SetActive(true),
                chunkRenderer => chunkRenderer.gameObject.SetActive(false)
            );
            
            CBSL.Logging.Logger.Info<ChunkBehaviourPool>("Initialized Size : " + viewRegionSize);
        }
        
        public ChunkBehaviour Claim(Chunk chunk) {
            var behaviour = _pool.Claim();

            behaviour.transform.position = chunk.Position.GetVector3();
            behaviour.name = chunk.Name();

            chunk.State = ChunkState.ACTIVE;
            _active.Add(chunk.Position, behaviour);
            
            return behaviour;
        }

        public void Reclaim(Chunk chunk) {
            if (chunk.State == ChunkState.ACTIVE) {
                _pool.Reclaim(_active[chunk.Position]);
                _active.Remove(chunk.Position);
            }
            chunk.State = ChunkState.INACTIVE;
        }

    }

}