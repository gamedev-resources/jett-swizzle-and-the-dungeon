using UnityEngine;

namespace Dungeon.Player
{
    /// <summary>
    /// Plays footstep audio from Animation Events on the walk/run clips.
    /// Lives on the same GameObject as the Animator so the events resolve by name,
    /// and gates on <see cref="PlayerLocomotion.PlanarSpeed"/> so a clip that is
    /// still blending out does not keep stepping while the player stands still.
    /// </summary>
    public class PlayerFootstepAudio : MonoBehaviour
    {
        private PlayerLocomotion _locomotion;

        private void Awake()
        {
            _locomotion = GetComponent<PlayerLocomotion>();
            if (_locomotion == null)
            {
                Debug.LogWarning("[PlayerFootstepAudio] No PlayerLocomotion on this object; " +
                    "footsteps cannot be gated on movement speed and will stay silent.", this);
            }
        }

        /// <summary>
        /// Animation Event target on Walk_Loop and Run_Loop. The name is referenced by
        /// those clips as a string, so renaming this method silently breaks footsteps.
        /// </summary>
        public void PlayFootStep()
        {
            if (_locomotion == null || AudioManager.Instance == null || _locomotion.PlanarSpeed < 0.1f)
            {
                return;
            }

            AudioManager.Instance.Play(AudioManager.SoundId.Footstep, transform.position);
        }
    }
}
