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
        [SerializeField] private float _walkSpeed = 2.5f;
        [SerializeField] private float _runSpeed = 5f;
        [SerializeField] private float _rotationSmoothTime = 0.1f;
        [SerializeField] private float _gravity = -20f;

        [Header("References")]
        [Tooltip("Transform whose forward/right define movement direction. Defaults to the main camera.")]
        [SerializeField] private Transform _cameraTransform;

        [Tooltip("Child transform that holds the visual model. This is rotated to face the move direction, " +
                 "leaving the root (and the follow camera bound to it) un-rotated. Defaults to a child named 'Pivot'.")]
        [SerializeField] private Transform _modelPivot;

        [Header("Animation")]
        [Tooltip("Optional Animator driven by movement. Auto-found in children if not set.")]
        [SerializeField] private Animator _animator;
        [Tooltip("Float parameter blended in the locomotion tree (Idle=0, Walk, Run).")]
        [SerializeField] private string _speedParameter = "Speed";
        [Tooltip("Animator Speed value used while walking (matches the walk blend threshold).")]
        [SerializeField] private float _walkAnimValue = 2f;
        [Tooltip("Animator Speed value used while running (matches the run blend threshold).")]
        [SerializeField] private float _runAnimValue = 4f;
        [Tooltip("How quickly the animator Speed value follows its target (seconds).")]
        [SerializeField] private float _speedDampTime = 0.1f;

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
            _animator = GetComponent<Animator>();
            _controls = new PlayerControls();
            _cameraTransform = ResolveCameraTransform();
            _modelPivot = ResolveModelPivot();
            _speedHash = Animator.StringToHash(_speedParameter);
        }

        private void OnEnable() => _controls.Player.Enable();
        private void OnDisable() => _controls.Player.Disable();
        private void OnDestroy() => _controls.Dispose();

        public void PlayFootStep()
        {
            if (AudioManager.Instance == null || PlanarSpeed < 0.1f)
            {
                return;
            }

            AudioManager.Instance.Play(AudioManager.SoundId.Footstep, transform.position);
        }

        private void Update()
        {
            (Vector2 moveInput, bool sprinting) = ReadInput();
            
            bool moving = moveInput.sqrMagnitude > 0.01f;
            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            if (input.sqrMagnitude > 1f) input.Normalize();
            
            Vector3 move = Vector3.zero;
            if (moving)
            {
                Vector3 direction = CalculateMoveDirection(input);
                RotatePivot(direction);
                float speed = sprinting ? _runSpeed : _walkSpeed;
                move = direction * speed;
            }
            
            _verticalVelocity = ApplyGravity();
            PlanarSpeed = new Vector3(move.x, 0f, move.z).magnitude;
            UpdateAnimator(PlanarSpeed, moving, sprinting);
            
            move.y = _verticalVelocity;
            ExecuteMove(move);
        }

        /// <summary>Reads move input and sprint state from the Input System.</summary>
        private (Vector2, bool) ReadInput()
        {
            Vector2 moveInput = _controls.Player.Move.ReadValue<Vector2>();
            bool sprinting = _controls.Player.Sprint.IsPressed();
            return (moveInput, sprinting);
        }

        /// <summary>
        /// Builds a camera-relative direction flattened onto the ground plane.
        /// </summary>
        private Vector3 CalculateMoveDirection(Vector3 input)
        {
            Vector3 camForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
            Vector3 camRight = _cameraTransform != null ? _cameraTransform.right : Vector3.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();
            return (camForward * input.z + camRight * input.x).normalized;
        }

        /// <summary>Smoothly rotates the model pivot to face the given direction.</summary>
        private void RotatePivot(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(_modelPivot.eulerAngles.y, targetAngle,
                ref _turnSmoothVelocity, _rotationSmoothTime);
            _modelPivot.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        /// <summary>Applies gravity and resets vertical velocity when grounded.</summary>
        private float ApplyGravity()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
            return _verticalVelocity;
        }

        /// <summary>Sets the animator speed parameter based on movement state.</summary>
        private void UpdateAnimator(float planarSpeed, bool moving, bool sprinting)
        {
            if (_animator == null) return;
            float animTarget = moving ? (sprinting ? _runAnimValue : _walkAnimValue) : 0f;
            _animator.SetFloat(_speedHash, animTarget, _speedDampTime, Time.deltaTime);
        }

        /// <summary>Applies vertical velocity and executes the controller move.</summary>
        private void ExecuteMove(Vector3 horizontalMove)
        {
            horizontalMove.y = _verticalVelocity;
            _controller.Move(horizontalMove * Time.deltaTime);
        }

        /// <summary>Resolves camera transform, falling back to the main camera.</summary>
        private Transform ResolveCameraTransform()
        {
            if (_cameraTransform != null) return _cameraTransform;
            if (Camera.main != null) return Camera.main.transform;
            return null;
        }

        /// <summary>
        /// Resolves model pivot, falling back to root with a warning if no child named 'Pivot' exists.
        /// </summary>
        private Transform ResolveModelPivot()
        {
            if (_modelPivot != null) return _modelPivot;
            _modelPivot = transform.Find("Pivot");
            if (_modelPivot != null) return _modelPivot;
            _modelPivot = transform;
            Debug.LogWarning("[ThirdPersonController] No 'Pivot' child found; rotating the root " +
                "instead, which makes the follow camera spin with the player.", this);
            return _modelPivot;
        }
    }
}
