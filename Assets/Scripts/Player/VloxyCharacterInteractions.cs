using System;
using CodeBlaze.Vloxy.Demo.Managers;
using CodeBlaze.Vloxy.Demo.Utils;
using CodeBlaze.Vloxy.Engine.Data;
using CodeBlaze.Vloxy.Engine.Utils;
using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCharacterInteractions : MonoBehaviour {

        [SerializeField] private LayerMask InteractionMask;
        [SerializeField] private float MaxInteractionDistance = 10f;
        
        private Camera _Camera;
        
        private void Awake() {
            _Camera = Camera.main;
        }

        public void BreakBlock() {
            var ray = _Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            
            if (Physics.Raycast(ray, out var hit, MaxInteractionDistance, InteractionMask.value)) {
                if (hit.collider.CompareTag("Chunk")) {
                    // We adjust to find the block we hit, along ray direction
                    var block_pos = Vector3Int.FloorToInt(hit.point + (0.1f * ray.direction));
                    
                    GameLogger.Info<VloxyCharacterInteractions>($"Break Block : {block_pos}");
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 5);

                    WorldAPI.Current.World.ChunkManager.SetBlock(Block.AIR, block_pos);
                }
            }
        }

        public void PlaceBlock() {
            var ray = _Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            
            if (Physics.Raycast(ray, out var hit, MaxInteractionDistance, InteractionMask.value)) {
                if (hit.collider.CompareTag("Chunk")) {
                    // We adjust to find adjacent block we hit, along hit normal
                    var block_pos = Vector3Int.FloorToInt(hit.point + (0.1f * hit.normal));
                    
                    GameLogger.Info<VloxyCharacterInteractions>($"Place Block : {block_pos}");
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green, 5);

                    WorldAPI.Current.World.ChunkManager.SetBlock(Block.STONE, block_pos);
                }
            }
        }

    }

}