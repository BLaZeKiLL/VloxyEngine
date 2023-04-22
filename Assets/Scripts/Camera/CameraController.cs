using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CodeBlaze.Camera
{
    public class CameraController : MonoBehaviour
    {
        class CameraState
        {
            public float yaw;
            public float pitch;
            public float roll;
            public float x;
            public float y;
            public float z;

            public void SetFromTransform(Transform t)
            {
                pitch = t.eulerAngles.x;
                yaw = t.eulerAngles.y;
                roll = t.eulerAngles.z;
                x = t.position.x;
                y = t.position.y;
                z = t.position.z;
            }

            public void Translate(Vector3 translation)
            {
                Vector3 rotatedTranslation = Quaternion.Euler(pitch, yaw, roll) * translation;

                x += rotatedTranslation.x;
                y += rotatedTranslation.y;
                z += rotatedTranslation.z;
            }

            public void LerpTowards(CameraState target, float positionLerpPct, float rotationLerpPct)
            {
                yaw = Mathf.Lerp(yaw, target.yaw, rotationLerpPct);
                pitch = Mathf.Lerp(pitch, target.pitch, rotationLerpPct);
                roll = Mathf.Lerp(roll, target.roll, rotationLerpPct);
                
                x = Mathf.Lerp(x, target.x, positionLerpPct);
                y = Mathf.Lerp(y, target.y, positionLerpPct);
                z = Mathf.Lerp(z, target.z, positionLerpPct);
            }

            public void UpdateTransform(Transform t)
            {
                t.eulerAngles = new Vector3(pitch, yaw, roll);
                t.position = new Vector3(x, y, z);
            }
        }
        
        CameraState m_TargetCameraState = new();
        CameraState m_InterpolatingCameraState = new();

        [Header("Movement Settings")]
        [Tooltip("Exponential boost factor on translation, controllable by mouse wheel.")]
        public float boost = 3.5f;

        [Tooltip("Time it takes to interpolate camera position 99% of the way to the target."), Range(0.001f, 1f)]
        public float positionLerpTime = 0.2f;

        [Header("Rotation Settings")]
        [Tooltip("X = Change in mouse position.\nY = Multiplicative factor for camera rotation.")]
        public AnimationCurve mouseSensitivityCurve = new(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

        [Tooltip("Time it takes to interpolate camera rotation 99% of the way to the target."), Range(0.001f, 1f)]
        public float rotationLerpTime = 0.01f;

        [Tooltip("Whether or not to invert our Y axis for mouse input to rotation.")]
        public bool invertY = false;
        
        [SerializeField] private GameObject m_TouchControls;
        
        InputAction movementAction;
        // InputAction verticalMovementAction;
        InputAction lookAction;
        // InputAction boostFactorAction;
        bool        mouseRightButtonPressed;

        private void Start()
        {
            var map = new CameraInput();

            map.Player.Enable();
            
            lookAction = map.Player.Look;
            movementAction = map.Player.Move;
            
            if (UnityEngine.Device.SystemInfo.deviceType == DeviceType.Handheld) {
                Application.targetFrameRate = Screen.currentResolution.refreshRate;
                m_TouchControls.SetActive(true);
            }
            
            // verticalMovementAction = map.AddAction("Vertical Movement");
            // boostFactorAction = map.AddAction("Boost Factor", binding: "<Mouse>/scroll");

            // verticalMovementAction.AddCompositeBinding("Dpad")
            //                       .With("Up", "<Keyboard>/pageUp")
            //                       .With("Down", "<Keyboard>/pageDown")
            //                       .With("Up", "<Keyboard>/e")
            //                       .With("Down", "<Keyboard>/q")
            //                       .With("Up", "<Gamepad>/rightshoulder")
            //                       .With("Down", "<Gamepad>/leftshoulder");
            
            // boostFactorAction.AddBinding("<Gamepad>/Dpad").WithProcessor("scaleVector2(x=1, y=4)");
        }

        private void OnEnable()
        {
            m_TargetCameraState.SetFromTransform(transform);
            m_InterpolatingCameraState.SetFromTransform(transform);
        }

        private Vector3 GetInputTranslationDirection()
        {
            Vector3 direction = Vector3.zero;
            
            var moveDelta = movementAction.ReadValue<Vector2>();
            direction.x = moveDelta.x;
            direction.z = moveDelta.y;
            // direction.y = verticalMovementAction.ReadValue<Vector2>().y;

            return direction;
        }
        
        private void Update()
        {
            // Exit Sample
            if (IsEscapePressedThisFrame())
            {
                SceneManager.LoadScene(0);
            }

            // Hide and lock cursor when right mouse button pressed
            if (IsRightMouseButtonDown())
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            // Unlock and show cursor when right mouse button released
            if (IsRightMouseButtonUp())
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }

            // Rotation
            if (IsCameraRotationAllowed())
            {
                var mouseMovement = GetInputLookRotation() * Time.deltaTime * 5;
                if (invertY)
                    mouseMovement.y = -mouseMovement.y;
                
                var mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

                m_TargetCameraState.yaw += mouseMovement.x * mouseSensitivityFactor;
                m_TargetCameraState.pitch += mouseMovement.y * mouseSensitivityFactor;
            }
            
            // Translation
            var translation = GetInputTranslationDirection() * Time.deltaTime;

            // Speed up movement when shift key held
            if (IsBoostPressed())
            {
                translation *= 10.0f;
            }
            
            // Modify movement by a boost factor (defined in Inspector and modified in play mode through the mouse scroll wheel)
            // boost += GetBoostFactor();
            translation *= Mathf.Pow(2.0f, boost);

            m_TargetCameraState.Translate(translation);

            // Framerate-independent interpolation
            // Calculate the lerp amount, such that we get 99% of the way to our target in the specified time
            var positionLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / positionLerpTime) * Time.deltaTime);
            var rotationLerpPct = 1f - Mathf.Exp((Mathf.Log(1f - 0.99f) / rotationLerpTime) * Time.deltaTime);
            m_InterpolatingCameraState.LerpTowards(m_TargetCameraState, positionLerpPct, rotationLerpPct);

            m_InterpolatingCameraState.UpdateTransform(transform);
        }

        // private float GetBoostFactor()
        // {
        //     return boostFactorAction.ReadValue<Vector2>().y * 0.01f;
        // }

        private Vector2 GetInputLookRotation()
        {
            return lookAction.ReadValue<Vector2>();
        }

        private bool IsBoostPressed()
        {
            bool boost = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed; 
            boost |= Gamepad.current != null && Gamepad.current.xButton.isPressed;
            return boost;
        }

        private bool IsEscapePressedThisFrame()
        {
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        }

        private bool IsCameraRotationAllowed()
        {
            bool canRotate = Mouse.current != null && Mouse.current.rightButton.isPressed;
            canRotate |= Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0;
            return canRotate;
        }

        private bool IsRightMouseButtonDown()
        {
            return Mouse.current != null && Mouse.current.rightButton.isPressed;
        }

        private bool IsRightMouseButtonUp()
        {
            return Mouse.current != null && !Mouse.current.rightButton.isPressed;
        }

    }

}