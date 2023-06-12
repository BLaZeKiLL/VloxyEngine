using UnityEngine;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCameraController : MonoBehaviour {

        public struct Input {

            /// <summary>
            /// Should be normalized, input system does that already
            /// </summary>
            public Vector3 Move { get; set; }
            
            /// <summary>
            /// Just the axis input
            /// </summary>
            public Vector2 Look { get; set; }

        }
        
        [Range(-90f, 90f)]
        [SerializeField] private float MinVerticalAngle = -85f;
        [Range(-90f, 90f)]
        [SerializeField] private float MaxVerticalAngle = 85f;
        [SerializeField] private float RotationSpeed = 1f;
        [SerializeField] private float RotationSharpness = 10000f;

        private Transform _Camera;

        private void Awake() {
            _Camera = GetComponentInChildren<Camera>().transform;
        }

        public void UpdateWithInput(float deltaTime, ref Input input) {
            var horizontalInput = Quaternion.Euler(Vector3.up * (input.Look.x * RotationSpeed));
            var verticalInput = Quaternion.Euler(Vector3.right *
                Mathf.Clamp(-1f * input.Look.y * RotationSpeed, MinVerticalAngle, MaxVerticalAngle));

            var horizontal = transform.rotation;
            
            horizontal = Quaternion.Slerp(horizontal, horizontal * horizontalInput,
                1f - Mathf.Exp(-RotationSharpness * deltaTime));
            
            transform.rotation = horizontal;
            
            var vertical = _Camera.rotation;
            
            vertical = Quaternion.Slerp(vertical, vertical * verticalInput,
                1f - Mathf.Exp(-RotationSharpness * deltaTime));
            
            _Camera.rotation = vertical;

            transform.position = input.Move;
        }

    }

}