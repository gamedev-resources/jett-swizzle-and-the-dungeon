using Dungeon.Core.Input;
using UnityEngine;

namespace Dungeon.Player
{
    /// <summary>
    /// Camera-relative third person movement for a CharacterController.
    /// Consumes Move and Sprint as C# events from the <see cref="PlayerInputController"/>
    /// on this same GameObject, rather than reading the input asset directly.
    /// Hold Sprint (Left Shift / gamepad left trigger) to run.
    /// Rotates a child "Pivot" transform to face the move direction, leaving the
    /// root un-rotated so the Cinemachine follow camera keeps a stable orientation.
    /// Animation is not owned here: the resulting locomotion state is forwarded to
    /// <see cref="PlayerAnimationDriver"/> each frame.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerLocomotion : MonoBehaviour
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

        private CharacterController _controller;
        private PlayerInputController _input;
        private PlayerAnimationDriver _animationDriver;
        private Vector2 _moveInput;
        private bool _sprintHeld;
        private float _verticalVelocity;
        private float _turnSmoothVelocity;
        private float _currentYaw;

        /// <summary>Current horizontal (planar) speed in units/second.</summary>
        public float PlanarSpeed { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputController>();
            _animationDriver = ResolveAnimationDriver();
            _cameraTransform = ResolveCameraTransform();
            _modelPivot = ResolveModelPivot();
            _currentYaw = _modelPivot.eulerAngles.y;
        }

        private void OnEnable()
        {
            if (_input == null)
            {
                Debug.LogWarning("[PlayerLocomotion] No PlayerInputController on this object; " +
                    "the player will not respond to input.", this);
                return;
            }

            _input.OnMove += OnMoveInput;
            _input.OnSprint += OnSprintInput;
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnMove -= OnMoveInput;
                _input.OnSprint -= OnSprintInput;
            }

            // Drop any held input so a re-enable starts from idle rather than a stale axis.
            _moveInput = Vector2.zero;
            _sprintHeld = false;
        }

        /// <summary>Caches the latest move axis; a released stick reports zero, i.e. idle.</summary>
        private void OnMoveInput(Vector2 value)
        {
            _moveInput = value;
        }

        /// <summary>Caches the sprint hold state.</summary>
        private void OnSprintInput(bool isSprinting)
        {
            _sprintHeld = isSprinting;
        }

        private void Update()
        {
            bool moving = _moveInput.sqrMagnitude > 0.01f;

            Vector3 move = GetHorizontalMovement(_moveInput, _sprintHeld, moving);

            ApplyGravity();
            PlanarSpeed = moving ? (_sprintHeld ? _runSpeed : _walkSpeed) : 0f;
            _animationDriver?.SetLocomotion(moving, _sprintHeld);

            ExecuteMove(move);
        }

        private Vector3 GetHorizontalMovement(Vector2 moveInput, bool sprinting, bool moving)
        {
            if (!moving) return Vector3.zero;

            Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y);
            input.Normalize();

            Vector3 direction = CalculateMoveDirection(input);
            RotatePivot(direction);

            float speed = sprinting ? _runSpeed : _walkSpeed;
            return direction * speed;
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
            return camForward * input.z + camRight * input.x;
        }

        /// <summary>Smoothly rotates the model pivot to face the given direction.</summary>
        private void RotatePivot(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _currentYaw = Mathf.SmoothDampAngle(_currentYaw, targetAngle,
                ref _turnSmoothVelocity, _rotationSmoothTime);
            _modelPivot.rotation = Quaternion.Euler(0f, _currentYaw, 0f);
        }

        /// <summary>Applies gravity and resets vertical velocity when grounded.</summary>
        private void ApplyGravity()
        {
            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += _gravity * Time.deltaTime;
            }
        }

        /// <summary>Applies vertical velocity and executes the controller move.</summary>
        private void ExecuteMove(Vector3 horizontalMove)
        {
            horizontalMove.y = _verticalVelocity;
            _controller.Move(horizontalMove * Time.deltaTime);
        }

        /// <summary>
        /// Resolves the animation driver on this object. Movement still runs without one,
        /// so a missing driver is a warning rather than an error.
        /// </summary>
        private PlayerAnimationDriver ResolveAnimationDriver()
        {
            var driver = GetComponent<PlayerAnimationDriver>();
            if (driver == null)
            {
                Debug.LogWarning("[PlayerLocomotion] No PlayerAnimationDriver on this object; " +
                    "the player will move but locomotion animations will not blend.", this);
            }
            return driver;
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
            Debug.LogWarning("[PlayerLocomotion] No 'Pivot' child found; rotating the root " +
                "instead, which makes the follow camera spin with the player.", this);
            return _modelPivot;
        }
    }
}
