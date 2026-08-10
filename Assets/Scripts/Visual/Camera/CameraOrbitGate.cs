using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Dungeon.Player
{
    /// <summary>
    /// Gates the Cinemachine orbital input so the camera only orbits while the
    /// right mouse button is held. It toggles the input axis Controllers' Enabled
    /// flags (which live in a [NoSaveDuringPlay] manager), so it never writes any
    /// persisted Cinemachine state and never triggers the Save-During-Play prompt.
    /// </summary>
    [RequireComponent(typeof(CinemachineInputAxisController))]
    public class CameraOrbitGate : MonoBehaviour
    {
        [Tooltip("Names of the input-axis controllers to gate (the look/orbit axes).")]
        [SerializeField] private string[] gatedAxisNames = { "Look Orbit X", "Look Orbit Y" };

        private CinemachineInputAxisController _input;
        private bool _verifiedMatch;

        private void Awake() => _input = GetComponent<CinemachineInputAxisController>();

        private void Update()
        {
            var mouse = Mouse.current;
            bool orbiting = mouse != null && mouse.rightButton.isPressed;

            var controllers = _input.Controllers;
            bool matchedAny = false;
            for (int i = 0; i < controllers.Count; i++)
            {
                if (System.Array.IndexOf(gatedAxisNames, controllers[i].Name) >= 0)
                {
                    controllers[i].Enabled = orbiting;
                    matchedAny = true;
                }
            }

            // Guard against a silent failure where no axis name matches: without this,
            // the gate would never disable the axes and the camera would orbit freely.
            if (!matchedAny && !_verifiedMatch && controllers.Count > 0)
            {
                Debug.LogWarning("[CameraOrbitGate] No input-axis controller matched gatedAxisNames " +
                    $"[{string.Join(", ", gatedAxisNames)}]. RMB gating is inactive. " +
                    "Check the axis names on the CinemachineInputAxisController.", this);
            }
            if (matchedAny)
                _verifiedMatch = true;
        }
    }
}
