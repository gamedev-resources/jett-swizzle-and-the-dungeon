using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace Dungeon.Visual.CameraRig
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
        private int[] _gatedIndices;
        private bool _resolved;
        private bool _warnedNoMatch;

        private void Awake() => _input = GetComponent<CinemachineInputAxisController>();

        private void Update()
        {
            if (!_resolved)
                ResolveGatedIndices();

            var mouse = Mouse.current;
            bool orbiting = mouse != null && mouse.rightButton.isPressed;

            var controllers = _input.Controllers;
            var gatedIndices = _gatedIndices;
            for (int i = 0; i < gatedIndices.Length; i++)
                controllers[gatedIndices[i]].Enabled = orbiting;
        }

        // Resolves the indices of the gated axis controllers exactly once (the
        // Controllers list may still be empty at Awake), so Update never searches
        // by name or allocates per frame.
        private void ResolveGatedIndices()
        {
            var controllers = _input.Controllers;
            var matches = new List<int>(controllers.Count);
            for (int i = 0; i < controllers.Count; i++)
            {
                if (System.Array.IndexOf(gatedAxisNames, controllers[i].Name) >= 0)
                    matches.Add(i);
            }

            _gatedIndices = matches.ToArray();
            _resolved = true;

            // Guard against a silent failure where no axis name matches: without this,
            // the gate would never disable the axes and the camera would orbit freely.
            // Fires at most once for a given misconfiguration.
            if (_gatedIndices.Length == 0 && controllers.Count > 0 && !_warnedNoMatch)
            {
                Debug.LogWarning("[CameraOrbitGate] No input-axis controller matched gatedAxisNames " +
                    $"[{string.Join(", ", gatedAxisNames)}]. RMB gating is inactive. " +
                    "Check the axis names on the CinemachineInputAxisController.", this);
                _warnedNoMatch = true;
            }
        }
    }
}
