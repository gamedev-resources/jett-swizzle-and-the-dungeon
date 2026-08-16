using Dungeon.Core.Events;
using Dungeon.Core.Events.Combat;
using UnityEngine;

namespace Dungeon.Gameplay.Player
{
    /// <summary>
    /// Sole owner of the player Animator. Other player components describe *what*
    /// happened (moving, sprinting, attacking) and this driver decides which
    /// parameters that maps to, so no other script caches an animator hash.
    /// It deliberately does not read input: callers drive it.
    /// </summary>
    public class PlayerAnimationController : MonoBehaviour, IGamePlayEventListener<AttackHitEvent>, IGamePlayEventListener<HealthChangedEvent>
    {
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

        private int _speedHash;
        private int _attackHash;
        private int _hitHash;
        private int _dieHash;
        private int _respawnHash;

        private void Awake()
        {
            _animator = ResolveAnimator();
            _speedHash = Animator.StringToHash(_speedParameter);
            _attackHash = Animator.StringToHash("Attack");
            _hitHash = Animator.StringToHash("IsHit");
            _dieHash = Animator.StringToHash("Die");
            _respawnHash = Animator.StringToHash("Spawn");
        }

        void OnEnable()
        {
            GameplayEventBus.Register<AttackHitEvent>(this);
            GameplayEventBus.Register<HealthChangedEvent>(this);
        }

        void OnDisable()
        {
            GameplayEventBus.Unregister<AttackHitEvent>(this);
            GameplayEventBus.Unregister<HealthChangedEvent>(this);
        }

        /// <summary>
        /// Blends the locomotion tree towards idle, walk or run.
        /// Call once per frame from whatever owns movement.
        /// </summary>
        public void SetLocomotion(bool moving, bool sprinting)
        {
            if (_animator == null) return;
            float animTarget = moving ? (sprinting ? _runAnimValue : _walkAnimValue) : 0f;
            _animator.SetFloat(_speedHash, animTarget, _speedDampTime, Time.deltaTime);
        }

        /// <summary>Fires the one-shot attack animation.</summary>
        public void TriggerAttack()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_attackHash);
        }

                /// <summary>Fires the one-shot attack animation.</summary>
        public void TriggerHit()
        {
            if (_animator == null) return;
            _animator.SetTrigger(_hitHash);
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
                Debug.LogWarning("[PlayerAnimationDriver] No Animator found on this object or its " +
                    "children; locomotion and attack animations will not play.", this);
            }
            return animator;
        }

        public void OnGameplayEvent(AttackHitEvent gameplayEvent)
        {
            if (gameplayEvent.Target == transform)
            {
                TriggerHit();
            }
        }

        /// <summary>
        /// Plays the death animation when this player's health is depleted.
        /// Ignored for events raised by other entities (same guard as the hit handler).
        /// </summary>
        public void OnGameplayEvent(HealthChangedEvent gameplayEvent)
        {
            if (gameplayEvent.IsDead && gameplayEvent.Source == transform)
            {
                if (_animator != null)
                {
                    _animator.SetTrigger(_dieHash);
                }
            }
            else if (gameplayEvent.PreviousHealth <= 0 && gameplayEvent.NewHealth > 0)
            {
                if (_animator != null)
                {
                    _animator.SetTrigger(_respawnHash);
                }
            }
        }
    }
}
