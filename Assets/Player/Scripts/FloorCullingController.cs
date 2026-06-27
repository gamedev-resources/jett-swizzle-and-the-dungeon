using UnityEngine;

namespace Dungeon.Player
{
    /// <summary>
    /// Hides upper-floor geometry while the player is on a lower floor by toggling a
    /// layer in the camera's culling mask. This is rendering-only, so colliders and
    /// physics on the upper floor stay intact (the player can still walk up there).
    ///
    /// Enclosure on the lower floor is preserved by leaving walls visible and by the
    /// scene's fog + dark background, rather than by a ceiling mesh.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FloorCullingController : MonoBehaviour
    {
        [Tooltip("Layer that holds the upper-level surfaces/props to hide when below.")]
        [SerializeField] private string upperLevelLayer = "UpperLevel";

        [Tooltip("Tracked target. Auto-found by tag if not set.")]
        [SerializeField] private Transform target;
        [SerializeField] private string targetTag = "Player";

        [Tooltip("World Y at or above which the target counts as being on the upper level. " +
                 "Pick a value between the two floor heights.")]
        [SerializeField] private float upperLevelHeight = 2f;

        private Camera _camera;
        private int _upperMaskBit;
        private bool _upperVisible = true;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            int layer = LayerMask.NameToLayer(upperLevelLayer);
            _upperMaskBit = layer >= 0 ? (1 << layer) : 0;

            if (target == null)
            {
                var go = GameObject.FindWithTag(targetTag);
                if (go != null) target = go.transform;
            }
        }

        private void LateUpdate()
        {
            if (target == null || _upperMaskBit == 0) return;

            bool shouldShowUpper = target.position.y >= upperLevelHeight;
            if (shouldShowUpper == _upperVisible) return;

            _upperVisible = shouldShowUpper;
            if (shouldShowUpper)
                _camera.cullingMask |= _upperMaskBit;   // show upper level
            else
                _camera.cullingMask &= ~_upperMaskBit;  // hide upper level
        }
    }
}
