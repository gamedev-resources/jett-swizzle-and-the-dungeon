using UnityEngine;

namespace Dungeon.UI
{
    /// <summary>
    /// Rotates this GameObject each frame so it faces the active camera.
    /// Useful for world-space UI Toolkit panels that should always be readable.
    /// </summary>
    [DisallowMultipleComponent]
    public class Billboard : MonoBehaviour
    {
        [Tooltip("Camera to face. Defaults to Camera.main if left empty.")]
        [SerializeField] private Camera targetCamera;

        [Tooltip("Lock rotation to the world Y axis so the UI stays upright.")]
        [SerializeField] private bool lockYAxis = true;

        [Tooltip("Flip 180° so text is not mirrored when facing the camera.")]
        [SerializeField] private bool flip = false;

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    return;
                }
            }

            Vector3 toCamera = targetCamera.transform.position - transform.position;

            if (lockYAxis)
            {
                toCamera.y = 0f;
            }

            if (toCamera.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion look = Quaternion.LookRotation(flip ? toCamera : -toCamera, Vector3.up);
            transform.rotation = look;
        }
    }
}
