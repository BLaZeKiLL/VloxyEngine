using System;
using CodeBlaze.Vloxy.Demo.Managers;
using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCharacterInteractions : MonoBehaviour {

        [SerializeField] private float MaxInteractionDistance = 250f;
        
        private Camera _Camera;
        
        private void Awake() {
            _Camera = Camera.main;
        }

        public void Fire() {
            var ray = _Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            
            if (Physics.Raycast(ray, out var hit) && hit.distance < MaxInteractionDistance) {
                if (hit.collider.CompareTag("Chunk")) {
                    
                }
            }
        }

    }

}