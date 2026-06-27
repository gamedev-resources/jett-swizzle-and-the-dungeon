using UnityEngine;
using UnityEngine.UIElements;

public class EquipmentWindow : MonoBehaviour
{
    private static readonly string[] SLOT_NAMES =
    {
        "slot-head", "slot-weapon", "slot-shield",
        "slot-potion", "slot-accessory-1", "slot-utility"
    };

    [Header("Templates")]
    [SerializeField] private VisualTreeAsset _equipmentWindowTemplate;
    [SerializeField] private VisualTreeAsset _itemSlotTemplate;

    [Header("Layout")]
    [SerializeField] private float _windowWidth = 520f;

    [Header("Preview")]
    [SerializeField] private Transform _previewCharacter;

    public void BuildEquipment(GameWindow window)
    {
        if (_equipmentWindowTemplate == null || _itemSlotTemplate == null)
        {
            Debug.LogError("Equipment Window Template or Item Slot Template is not assigned.");
            return;
        }

        var width = _windowWidth > 0 ? _windowWidth : 520f;

        window.Root.style.width = width;
        window.Root.style.height = StyleKeyword.Auto;

        var content = _equipmentWindowTemplate.Instantiate().ExtractRoot("equipment-content");

        // The equipment slots are authored as EquipmentSlot elements in the UXML,
        // so we just hand each one the shared ItemSlot template to build its visual.
        foreach (var slotName in SLOT_NAMES)
        {
            var slot = content.Q<EquipmentSlot>(slotName);
            if (slot == null) continue;

            slot.Build(_itemSlotTemplate);
        }

        window.ContentArea.Add(content);

        var previewArea = content.Q<VisualElement>("preview-area");
        if (_previewCharacter != null)
            previewArea.AddManipulator(new PreviewRotateManipulator(previewArea, _previewCharacter));
    }
}

