using System;

using KinematicCharacterController;

using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBlaze.Vloxy.Demo.Player {

    public class VloxyCharacterController : MonoBehaviour, ICharacterController {

        public enum State {

            FPS,
            FLY

        }

        public struct Input {

            /// <summary>
            /// Should be normalized, input system does that already
            /// </summary>
            public Vector3 Move { get; set; }

            /// <summary>
            /// Read somewhere quaternion's are better than euler angles
            /// </summary>
            public Quaternion Look { get; set; }

            public float Altitude { get; set; }

            public bool JumpDown { get; set; }

            public bool SprintDown { get; set; }

        }

        [SerializeField] private World _World;
        
        [Header("Stable Movement")] [SerializeField]
        private float MaxStableWalkSpeed = 10f;

        [SerializeField] private float MaxStableSprintSpeed = 25f;
        [SerializeField] private float StableMovementSharpness = 15;
        // [SerializeField] private float OrientationSharpness = 10;

        [Header("Air Movement")] [SerializeField]
        private float MaxAirWalkSpeed = 10f;

        [SerializeField] private float MaxAirSprintSpeed = 25f;
        [SerializeField] private float AirAccelerationSpeed = 5f;
        [SerializeField] private float Drag = 0.1f;

        [Header("Jumping")] [SerializeField] private bool AllowJumpingWhenSliding = false;
        [SerializeField] private float JumpSpeed = 10f;
        [SerializeField] private float JumpPreGroundingGraceTime = 0f;
        [SerializeField] private float JumpPostGroundingGraceTime = 0f;

        [Header("Misc")] [SerializeField] private Vector3 Gravity = new(0, -30f, 0);

        [SerializeField] private State _State;
        
        private KinematicCharacterMotor _Motor;
        
        private Vector3 _MoveInput;
        private Vector3 _LookInput;

        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;

        private bool _shouldSprint = false;

        private void Awake() {
            _Motor = GetComponent<KinematicCharacterMotor>();
        }

        private void Start() {
            _Motor.CharacterController = this;

            TransitionState(_State);
            
            Debug.Log(_World.GetSpawnPoint());
            _Motor.SetPosition(_World.GetSpawnPoint());
        }

        public void ToggleState() {
            switch (_State) {
                case State.FPS:
                    TransitionState(State.FLY);

                    break;
                case State.FLY:
                    TransitionState(State.FPS);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetInput(ref Input input) {
            // Calculate camera direction and rotation on the character plane
            var cameraPlanarDirection =
                Vector3.ProjectOnPlane(input.Look * Vector3.forward, _Motor.CharacterUp).normalized;

            if (cameraPlanarDirection.sqrMagnitude == 0f) // Why check 2 axis ?
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(input.Look * Vector3.up, _Motor.CharacterUp).normalized;
            }

            var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, _Motor.CharacterUp);

            if (_State == State.FLY) {
                input.Move = new Vector3(input.Move.x, input.Altitude, input.Move.z).normalized;
            }

            // Move and look inputs
            _MoveInput = cameraPlanarRotation * input.Move;
            _LookInput = cameraPlanarDirection;

            _shouldSprint = input.SprintDown;

            if (input.JumpDown) {
                _timeSinceJumpRequested = 0f;
                _jumpRequested = true;
            }
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime) {
            if (_LookInput != Vector3.zero) {
                currentRotation = Quaternion.LookRotation(_LookInput, _Motor.CharacterUp);
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime) {
            switch (_State) {
                case State.FPS: {
                    if (_Motor.GroundingStatus.IsStableOnGround) {
                        // Reorient velocity on slope
                        currentVelocity =
                            _Motor.GetDirectionTangentToSurface(currentVelocity, _Motor.GroundingStatus.GroundNormal) *
                            currentVelocity.magnitude;

                        // Calculate target velocity
                        var inputRight = Vector3.Cross(_MoveInput, _Motor.CharacterUp);
                        var reorientedInput =
                            Vector3.Cross(_Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                            _MoveInput.magnitude;

                        var targetMovementVelocity =
                            reorientedInput * (_shouldSprint ? MaxStableSprintSpeed : MaxStableWalkSpeed);

                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                            1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    } else {
                        // Add move input
                        if (_MoveInput.sqrMagnitude > 0f) {
                            var targetMovementVelocity =
                                _MoveInput * (_shouldSprint ? MaxAirSprintSpeed : MaxAirWalkSpeed);

                            // Prevent climbing on un-stable slopes with air movement
                            if (_Motor.GroundingStatus.FoundAnyGround) {
                                var perpendicularObstructionNormal = Vector3
                                                                     .Cross(
                                                                         Vector3.Cross(_Motor.CharacterUp,
                                                                             _Motor.GroundingStatus.GroundNormal),
                                                                         _Motor.CharacterUp).normalized;
                                targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity,
                                    perpendicularObstructionNormal);
                            }

                            var velocityDiff =
                                Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                            currentVelocity += velocityDiff * (AirAccelerationSpeed * deltaTime);
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                    }

                    // Handle jumping
                    _jumpedThisFrame = false;
                    _timeSinceJumpRequested += deltaTime;

                    if (_State == State.FPS && _jumpRequested) {
                        // See if we actually are allowed to jump
                        if (!_jumpConsumed &&
                            ((AllowJumpingWhenSliding
                                    ? _Motor.GroundingStatus.FoundAnyGround
                                    : _Motor.GroundingStatus.IsStableOnGround) ||
                                _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime)) {
                            // Calculate jump direction before ungrounding
                            var jumpDirection = _Motor.CharacterUp;

                            if (_Motor.GroundingStatus is { FoundAnyGround: true, IsStableOnGround: false }) {
                                jumpDirection = _Motor.GroundingStatus.GroundNormal;
                            }

                            // Makes the character skip ground probing/snapping on its next update. 
                            // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                            _Motor.ForceUnground(0.1f);

                            // Add to the return velocity and reset jump state
                            currentVelocity += (jumpDirection * JumpSpeed) -
                                Vector3.Project(currentVelocity, _Motor.CharacterUp);
                            _jumpRequested = false;
                            _jumpConsumed = true;
                            _jumpedThisFrame = true;
                        }
                    }

                    break;
                }
                case State.FLY: {
                    var targetMovementVelocity =
                        _MoveInput * (_shouldSprint ? MaxStableSprintSpeed : MaxStableWalkSpeed);

                    // Smooth movement Velocity
                    currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                        1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void BeforeCharacterUpdate(float deltaTime) { }

        public void PostGroundingUpdate(float deltaTime) {
            // Handle landing and leaving ground
            if (_Motor.GroundingStatus.IsStableOnGround && !_Motor.LastGroundingStatus.IsStableOnGround) {
                //OnLanded();
            } else if (!_Motor.GroundingStatus.IsStableOnGround && _Motor.LastGroundingStatus.IsStableOnGround) {
                //OnLeaveStableGround();
            }
        }

        public void AfterCharacterUpdate(float deltaTime) {
            // Handle jump-related values
            {
                // Handle jumping pre-ground grace period
                if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime) {
                    _jumpRequested = false;
                }

                // Handle jumping while sliding
                if (AllowJumpingWhenSliding
                        ? _Motor.GroundingStatus.FoundAnyGround
                        : _Motor.GroundingStatus.IsStableOnGround) {
                    // If we're on a ground surface, reset jumping values
                    if (!_jumpedThisFrame) {
                        _jumpConsumed = false;
                    }

                    _timeSinceLastAbleToJump = 0f;
                } else {
                    // Keep track of time since we were last able to jump (for grace period)
                    _timeSinceLastAbleToJump += deltaTime;
                }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll) {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) { }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport) { }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport) { }

        public void OnDiscreteCollisionDetected(Collider hitCollider) { }

        private void TransitionState(State next) {
            OnStateExit(next, _State);
            OnStateEnter(next, _State);
            _State = next;
        }

        private void OnStateEnter(State next, State from) {
            switch (next) {
                case State.FPS:
                    _Motor.SetGroundSolvingActivation(true);

                    break;
                case State.FLY:
                    _Motor.SetGroundSolvingActivation(false);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(next), next, null);
            }
        }

        private void OnStateExit(State next, State from) { }

    }

}