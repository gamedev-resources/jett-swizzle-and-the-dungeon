using UnityEngine;
using UnityEngine.UIElements;

namespace Dungeon.UI
{
    /// <summary>
    /// Singleton world-space interaction prompt rendered by UI Toolkit.
    /// One instance lives in the scene; callers move it to the target and show/hide it.
    /// Requires a <see cref="UIDocument"/> on the same GameObject with a World-Space PanelSettings.
    /// Temporary singleton pattern — will be replaced by event bus subscription later.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InteractionPromptUI : MonoBehaviour
    {
        public static InteractionPromptUI Instance { get; private set; }

        [Header("Defaults")]
        [SerializeField] private string defaultKeyLabel = "E";
        [SerializeField] private string defaultPromptText = "Interact";
        [SerializeField] private bool hideOnAwake = true;

        [Header("Placement")]
        [Tooltip("World-space offset applied when Show() is called with a position.")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

        private UIDocument _document;
        private VisualElement _root;
        private VisualElement _icon;
        private Label _keyLabel;
        private Label _promptLabel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _document = GetComponent<UIDocument>();
            CacheElements();

            if (hideOnAwake)
            {
                Hide();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            if (_root == null)
            {
                CacheElements();
            }
        }

        private void CacheElements()
        {
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }

            var docRoot = _document.rootVisualElement;
            if (docRoot == null)
            {
                return;
            }

            _root = docRoot.Q<VisualElement>("interaction-prompt");
            _keyLabel = docRoot.Q<Label>("prompt-key-label");
            _promptLabel = docRoot.Q<Label>("prompt-text");
        }

        /// <summary>
        /// Shows the prompt with the given text, optional key label, and optional icon.
        /// Pass <paramref name="icon"/> as null (default) to hide the icon slot.
        /// </summary>
        public void Show(string promptText, string keyLabel = null)
        {
            if (_root == null)
            {
                CacheElements();
                if (_root == null)
                {
                    return;
                }
            }

            _promptLabel.text = string.IsNullOrEmpty(promptText) ? defaultPromptText : promptText;
            _keyLabel.text = string.IsNullOrEmpty(keyLabel) ? defaultKeyLabel : keyLabel;

            _root.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Moves the prompt to a world position (plus configured offset) and shows it.
        /// </summary>
        public void ShowAt(Vector3 worldPosition, string promptText, string keyLabel = null)
        {
            transform.position = worldPosition + worldOffset;
            Show(promptText, keyLabel);
        }

        /// <summary>
        /// Hides the prompt.
        /// </summary>
        public void Hide()
        {
            if (_root == null)
            {
                CacheElements();
                if (_root == null)
                {
                    return;
                }
            }

            _root.style.display = DisplayStyle.None;
        }
    }
}
