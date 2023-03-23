using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;

#if VLOXY_LOGGING
using CodeBlaze.Vloxy.Engine.Utils.Logger;
#endif

using Priority_Queue;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Pool;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkPool {

        private IObjectPool<ChunkBehaviour> _Pool;
        private Dictionary<int3, ChunkBehaviour> _Map;
        private SimplePriorityQueue<int3> _Queue;

        private int3 _Focus;
        private int _ChunkPoolSize;
        
        public ChunkPool(Transform transform, VloxySettings settings) {
            _ChunkPoolSize = (settings.Chunk.DrawDistance + 2).CubedSize();

            _Map = new Dictionary<int3, ChunkBehaviour>(_ChunkPoolSize);
            _Queue = new SimplePriorityQueue<int3>();
            
            _Pool = new ObjectPool<ChunkBehaviour>( // pool size = x^2 + 1
                () => {
                    var go = new GameObject("Chunk", typeof(ChunkBehaviour)) {
                        transform = {
                            parent = transform
                        }
                    };
                    
                    go.SetActive(false);
            
                    var chunkBehaviour = go.GetComponent<ChunkBehaviour>();
                    
                    chunkBehaviour.SetRenderSettings(settings.Renderer);

                    return chunkBehaviour;
                },
                chunkBehaviour => chunkBehaviour.gameObject.SetActive(true),
                chunkBehaviour => chunkBehaviour.gameObject.SetActive(false),
                null, false, _ChunkPoolSize, _ChunkPoolSize
            );
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkPool>("Initialized Size : " + _ChunkPoolSize);
#endif
        }

        public bool IsActive(int3 pos) => _Map.ContainsKey(pos);

        internal void ViewUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, 1.0f / (position - _Focus).SqrMagnitude());
            }
        }
        
        internal ChunkBehaviour Claim(int3 position) {
            if (_Map.ContainsKey(position)) {
                throw new InvalidOperationException($"Chunk ({position}) already active");
            }

            // Reclaim
            if (_Queue.Count >= _ChunkPoolSize) {
                _Pool.Release(_Map[_Queue.Dequeue()]);
            }
                
            // Claim
            var behaviour = _Pool.Get();
                
            behaviour.transform.position = position.GetVector3();
            behaviour.name = $"Chunk({position})";
                
            _Map.Add(position, behaviour);
            _Queue.Enqueue(position, 1.0f / (position - _Focus).SqrMagnitude());

            return behaviour;
        }

    }

}