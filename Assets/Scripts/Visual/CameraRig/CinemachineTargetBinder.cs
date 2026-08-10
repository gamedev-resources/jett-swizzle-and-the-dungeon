using UnityEngine;
using Unity.Cinemachine;

namespace Dungeon.Player
{
    /// <summary>
    /// Auto-assigns a CinemachineCamera's Tracking/LookAt targets to a tagged
    /// object at edit and run time. Avoids hand-wiring scene references and keeps
    /// the follow camera working if the player is spawned or replaced.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineTargetBinder : MonoBehaviour
    {
        [SerializeField] private string targetTag = "Player";

        private CinemachineCamera _cam;

        private void OnEnable() => Bind();

        private void Bind()
        {
            if (_cam == null)
                _cam = GetComponent<CinemachineCamera>();

            if (_cam.Follow != null && _cam.LookAt != null)
                return;

            var target = GameObject.FindWithTag(targetTag);
            if (target == null)
                return;

            if (_cam.Follow == null)
                _cam.Follow = target.transform;
            if (_cam.LookAt == null)
                _cam.LookAt = target.transform;
        }
    }
}
