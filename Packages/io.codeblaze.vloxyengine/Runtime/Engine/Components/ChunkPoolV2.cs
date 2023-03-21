using System;
using System.Collections.Generic;

using CodeBlaze.Vloxy.Engine.Behaviour;
using CodeBlaze.Vloxy.Engine.Settings;
using CodeBlaze.Vloxy.Engine.Utils.Extensions;
using CodeBlaze.Vloxy.Engine.Utils.Logger;

using Priority_Queue;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Pool;

namespace CodeBlaze.Vloxy.Engine.Components {

    public class ChunkPoolV2 {

        private IObjectPool<ChunkBehaviour> _Pool;
        private Dictionary<int3, ChunkBehaviour> _Map;
        private SimplePriorityQueue<int3> _Queue;

        private int3 _Focus;
        
        public ChunkPoolV2(Transform transform, VloxySettings settings) {
            var viewRegionSize = settings.Chunk.DrawDistance.CubedSize();

            _Map = new Dictionary<int3, ChunkBehaviour>(viewRegionSize);
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
                null, false, viewRegionSize, viewRegionSize
            );
#if VLOXY_LOGGING
            VloxyLogger.Info<ChunkBehaviourPool>("Initialized Size : " + viewRegionSize);
#endif
        }

        internal void ViewUpdate(int3 focus) {
            _Focus = focus;

            foreach (var position in _Queue) {
                _Queue.UpdatePriority(position, (position - _Focus).SqrMagnitude());
            }
        }
        
        internal ChunkBehaviour Claim(int3 position) {
            if (_Queue.Contains(position)) {
                throw new InvalidOperationException("Chunk already active");
            }

            // Reclaim
            _Pool.Release(_Map[_Queue.Dequeue()]);
                
            // Claim
            var behaviour = _Pool.Get();
                
            behaviour.transform.position = position.GetVector3();
            behaviour.name = $"Chunk({position})";
                
            _Map.Add(position, behaviour);
            _Queue.Enqueue(position, (position - _Focus).SqrMagnitude());

            return behaviour;
        }

    }

}