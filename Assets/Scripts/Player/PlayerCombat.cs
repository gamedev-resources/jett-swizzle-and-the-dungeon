using Dungeon.Core.Input;
using UnityEngine;

namespace Dungeon.Player
{
    /// <summary>
    /// Owns the player's offensive actions. Today that is only "swing on attack input",
    /// which it forwards to the <see cref="PlayerAnimationDriver"/>; damage, hit
    /// detection and cooldowns belong here as the combat model grows.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        private PlayerInputController _input;
        private PlayerAnimationDriver _animationDriver;

        private void Awake()
        {
            _input = GetComponent<PlayerInputController>();
            _animationDriver = GetComponent<PlayerAnimationDriver>();
        }

        private void OnEnable()
        {
            if (_input == null)
            {
                Debug.LogWarning("[PlayerCombat] No PlayerInputController on this object; " +
                    "attacks will never fire.", this);
                return;
            }

            _input.OnAttack += OnAttackInput;
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnAttack -= OnAttackInput;
            }
        }

        /// <summary>Plays the attack animation on a performed attack input.</summary>
        private void OnAttackInput()
        {
            _animationDriver?.TriggerAttack();
        }
    }
}
