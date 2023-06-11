using System;

using KinematicCharacterController;

using UnityEngine;
using UnityEngine.InputSystem;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCharacterController : MonoBehaviour, ICharacterController {
        
        public struct Input {

            /// <summary>
            /// Should be normalized, input system does that already
            /// </summary>
            public Vector3 Move { get; set; }
            
            /// <summary>
            /// Read somewhere quaternion's are better than euler angles
            /// </summary>
            public Quaternion Look { get; set; }

        }

        [Header("Stable Movement")]
        [SerializeField] private float MaxStableMoveSpeed = 10f;
        [SerializeField] private float StableMovementSharpness = 15;
        [SerializeField] private float OrientationSharpness = 10;

        [Header("Air Movement")]
        [SerializeField] private float MaxAirMoveSpeed = 10f;
        [SerializeField] private float AirAccelerationSpeed = 5f;
        [SerializeField] private float Drag = 0.1f;

        [Header("Misc")]
        [SerializeField] private Vector3 Gravity = new(0, -30f, 0);

        private KinematicCharacterMotor _Motor;

        private Vector3 _MoveInput;
        private Vector3 _LookInput;

        private void Awake() {
            _Motor = GetComponent<KinematicCharacterMotor>();
        }

        private void Start() {
            _Motor.CharacterController = this;
        }

        public void SetInput(ref Input input) {
            // Calculate camera direction and rotation on the character plane
            var cameraPlanarDirection = Vector3.ProjectOnPlane(input.Look * Vector3.forward, _Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f) // Why check 2 axis ?
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(input.Look * Vector3.up, _Motor.CharacterUp).normalized;
            }
            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _Motor.CharacterUp);

            // Move and look inputs
            _MoveInput = cameraPlanarRotation * input.Move;
            _LookInput = cameraPlanarDirection;
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
            if (_LookInput != Vector3.zero)
            {
                currentRotation = Quaternion.LookRotation(_LookInput, _Motor.CharacterUp);
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            if (_Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient velocity on slope
                currentVelocity = _Motor.GetDirectionTangentToSurface(currentVelocity, _Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Calculate target velocity
                var inputRight = Vector3.Cross(_MoveInput, _Motor.CharacterUp);
                var reorientedInput = Vector3.Cross(_Motor.GroundingStatus.GroundNormal, inputRight).normalized * _MoveInput.magnitude;
                var targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            else
            {
                // Add move input
                if (_MoveInput.sqrMagnitude > 0f)
                {
                    var targetMovementVelocity = _MoveInput * MaxAirMoveSpeed;

                    // Prevent climbing on un-stable slopes with air movement
                    if (_Motor.GroundingStatus.FoundAnyGround)
                    {
                        var perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(_Motor.CharacterUp, _Motor.GroundingStatus.GroundNormal), _Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpendicularObstructionNormal);
                    }

                    var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
        }

        public void BeforeCharacterUpdate(float deltaTime) {
        }

        public void PostGroundingUpdate(float deltaTime) {
        }

        public void AfterCharacterUpdate(float deltaTime) {
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport) {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) {
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider) {
        }

    }

}