using System;
using CodeBlaze.Vloxy.Demo.Managers;
using CodeBlaze.Vloxy.Demo.Utils;
using CodeBlaze.Vloxy.Engine.Utils;
using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCharacterInteractions : MonoBehaviour {

        [SerializeField] private LayerMask InteractionMask;
        [SerializeField] private float MaxInteractionDistance = 100;
        
        private Camera _Camera;
        
        private void Awake() {
            _Camera = Camera.main;
        }

        public void Fire() {
            var ray = _Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            
            // hit.distance < MaxInteractionDistance
            if (Physics.Raycast(ray, out var hit, MaxInteractionDistance, InteractionMask.value)) {
                if (hit.collider.CompareTag("Chunk")) {
                    var block_pos = Vector3Int.FloorToInt(hit.point + (0.1f * ray.direction));
                    GameLogger.Info<VloxyCharacterInteractions>($"Block Hit : {block_pos}");
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan, 5);
                }
            }
        }

    }

}