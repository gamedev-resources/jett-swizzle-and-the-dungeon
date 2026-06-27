using UnityEngine;

namespace Dungeon.Player
{
    /// <summary>
    /// Camera-relative third person movement for a CharacterController.
    /// Reads Move and Sprint from the Input System (PlayerControls asset).
    /// Hold Sprint (Left Shift / gamepad left trigger) to run.
    /// Rotates a child "Pivot" transform to face the move direction, leaving the
    /// root un-rotated so the Cinemachine follow camera keeps a stable orientation.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        [SerializeField] private float gravity = -20f;

        [Header("References")]
        [Tooltip("Transform whose forward/right define movement direction. Defaults to the main camera.")]
        [SerializeField] private Transform cameraTransform;

        [Tooltip("Child transform that holds the visual model. This is rotated to face the move direction, " +
                 "leaving the root (and the follow camera bound to it) un-rotated. Defaults to a child named 'Pivot'.")]
        [SerializeField] private Transform modelPivot;

        [Header("Animation")]
        [Tooltip("Optional Animator driven by movement. Auto-found in children if not set.")]
        [SerializeField] private Animator animator;
        [Tooltip("Float parameter blended in the locomotion tree (Idle=0, Walk, Run).")]
        [SerializeField] private string speedParameter = "Speed";
        [Tooltip("Animator Speed value used while walking (matches the walk blend threshold).")]
        [SerializeField] private float walkAnimValue = 2f;
        [Tooltip("Animator Speed value used while running (matches the run blend threshold).")]
        [SerializeField] private float runAnimValue = 4f;
        [Tooltip("How quickly the animator Speed value follows its target (seconds).")]
        [SerializeField] private float speedDampTime = 0.1f;

        private CharacterController _controller;
        private PlayerControls _controls;
        private float _verticalVelocity;
        private float _turnSmoothVelocity;
        private int _speedHash;

        /// <summary>Current horizontal (planar) speed in units/second.</summary>
        public float PlanarSpeed { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _controls = new PlayerControls();
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (modelPivot == null)
            {
                modelPivot = transform.Find("Pivot");
            }
            if (modelPivot == null)
            {
                // The camera depends on rotating the Pivot, not the root. Fall back to
                // the root so movement still works, but warn — the follow camera will spin.
                modelPivot = transform;
                Debug.LogWarning("[ThirdPersonController] No 'Pivot' child found; rotating the root " +
                    "instead, which makes the follow camera spin with the player.", this);
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();

            }
            _speedHash = Animator.StringToHash(speedParameter);
        }

        private void OnEnable() => _controls.Player.Enable();
        private void OnDisable() => _controls.Player.Disable();
        private void OnDestroy() => _controls.Dispose();

        private void Update()
        {
            Vector2 moveInput = _controls.Player.Move.ReadValue<Vector2>();
            bool sprinting = _controls.Player.Sprint.IsPressed();
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            if (input.sqrMagnitude > 1f) input.Normalize();

            bool moving = input.sqrMagnitude > 0.01f;
            Vector3 move = Vector3.zero;

            if (moving)
            {
                // Build a camera-relative direction flattened onto the ground plane.
                Vector3 camForward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
                Vector3 camRight = cameraTransform != null ? cameraTransform.right : Vector3.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 desiredDir = (camForward * input.z + camRight * input.x).normalized;

                // Rotate only the pivot (model) in place around Y. The root stays
                // un-rotated so the follow camera keeps a stable orientation.
                float targetAngle = Mathf.Atan2(desiredDir.x, desiredDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(modelPivot.eulerAngles.y, targetAngle,
                    ref _turnSmoothVelocity, rotationSmoothTime);
                modelPivot.rotation = Quaternion.Euler(0f, angle, 0f);

                float speed = sprinting ? runSpeed : walkSpeed;
                move = desiredDir * speed;
            }

            // Gravity so the controller stays grounded on the dungeon floor.
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            PlanarSpeed = new Vector3(move.x, 0f, move.z).magnitude;

            // Feed the animator a value aligned with the locomotion blend thresholds
            // (Idle=0, Walk, Run) rather than raw m/s, so clips match the gait.
            if (animator != null)
            {
                float animTarget = moving ? (sprinting ? runAnimValue : walkAnimValue) : 0f;
                animator.SetFloat(_speedHash, animTarget, speedDampTime, Time.deltaTime);
            }

            move.y = _verticalVelocity;
            _controller.Move(move * Time.deltaTime);
        }
    }
}
