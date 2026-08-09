using Dungeon.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dungeon.Player
{
    /// <summary>
    /// Camera-relative third person movement for a CharacterController.
    /// Consumes Move, Sprint and Attack input as events from the <see cref="GameplayEventBus"/>,
    /// raised by the single input owner rather than read from the input asset directly.
    /// Hold Sprint (Left Shift / gamepad left trigger) to run.
    /// Rotates a child "Pivot" transform to face the move direction, leaving the
    /// root un-rotated so the Cinemachine follow camera keeps a stable orientation.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonController : MonoBehaviour,
        IGamePlayEventListener<MoveInputEvent>,
        IGamePlayEventListener<SprintInputEvent>,
        IGamePlayEventListener<AttackInputEvent>
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
        private Vector2 _moveInput;
        private bool _sprintHeld;
        private float _verticalVelocity;
        private float _turnSmoothVelocity;
        private float _currentYaw;
        private int _speedHash;
        private int _attackHash;

        /// <summary>Current horizontal (planar) speed in units/second.</summary>
        public float PlanarSpeed { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = ResolveAnimator();
            _cameraTransform = ResolveCameraTransform();
            _modelPivot = ResolveModelPivot();
            _currentYaw = _modelPivot.eulerAngles.y;
            _speedHash = Animator.StringToHash(_speedParameter);
            _attackHash = Animator.StringToHash("Attack");
        }

        private void OnEnable()
        {
            GameplayEventBus.Register<MoveInputEvent>(this);
            GameplayEventBus.Register<SprintInputEvent>(this);
            GameplayEventBus.Register<AttackInputEvent>(this);
        }

        private void OnDisable()
        {
            GameplayEventBus.Unregister<MoveInputEvent>(this);
            GameplayEventBus.Unregister<SprintInputEvent>(this);
            GameplayEventBus.Unregister<AttackInputEvent>(this);

            // Drop any held input so a re-enable starts from idle rather than a stale axis.
            _moveInput = Vector2.zero;
            _sprintHeld = false;
        }

        /// <summary>Caches the latest move axis; the canceled phase carries zero, i.e. idle.</summary>
        public void OnGameplayEvent(MoveInputEvent gameplayEvent)
        {
            _moveInput = gameplayEvent.Value;
        }

        /// <summary>Caches the sprint hold state.</summary>
        public void OnGameplayEvent(SprintInputEvent gameplayEvent)
        {
            _sprintHeld = gameplayEvent.IsSprinting;
        }

        /// <summary>Triggers the attack animation on a performed attack input.</summary>
        public void OnGameplayEvent(AttackInputEvent gameplayEvent)
        {
            if (gameplayEvent.Phase != InputActionPhase.Performed)
            {
                return;
            }

            //TODO: Refactor this into a combat controller
            if (_animator != null)
            {
                _animator.SetTrigger(_attackHash);
            }
        }

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
            bool moving = _moveInput.sqrMagnitude > 0.01f;

            Vector3 move = GetHorizontalMovement(_moveInput, _sprintHeld, moving);
            
            ApplyGravity();
            PlanarSpeed = moving ? (_sprintHeld ? _runSpeed : _walkSpeed) : 0f;
            UpdateAnimator(PlanarSpeed, moving, _sprintHeld);
            
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

        /// <summary>
        /// Resolves the animator, falling back to a search of this object and its children.
        /// </summary>
        private Animator ResolveAnimator()
        {
            if (_animator != null) return _animator;

            var animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[ThirdPersonController] No Animator found on this object or its " +
                    "children; locomotion and attack animations will not play.", this);
            }
            return animator;
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
