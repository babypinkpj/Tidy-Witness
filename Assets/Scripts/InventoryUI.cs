using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public InventorySystem inventorySystem;
    
    [Header("UI Slots (Assign 4 Images)")]
    public Image[] slotIcons = new Image[InventorySystem.MaxSlots];
    
    [Header("Highlights (Assign 4 GameObjects)")]
    public GameObject[] selectionHighlights = new GameObject[InventorySystem.MaxSlots];

    [Header("Two-Handed State")]
    public GameObject fullHandTextObj; // Assign the "Full Hand" text object here
    public float transparentAlpha = 0.4f;
    public float opaqueAlpha = 1.0f;
    
    void Update()
    {
        if (inventorySystem == null) return;

        bool isHoldingTwoHanded = inventorySystem.IsHoldingTwoHandedItem();

        // Toggle the "Full Hand" text
        if (fullHandTextObj != null)
        {
            fullHandTextObj.SetActive(isHoldingTwoHanded);
        }

        for (int i = 0; i < InventorySystem.MaxSlots; i++)
        {
            // Determine how transparent this slot's icon should be
            float targetAlpha = transparentAlpha;

            // If we are NOT holding a two-handed item, and this is the active slot, make it opaque
            if (!isHoldingTwoHanded && i == inventorySystem.currentSlotIndex)
            {
                targetAlpha = opaqueAlpha;
            }

            // Update the Item Icon
            Item itemInSlot = inventorySystem.slots[i];
            if (itemInSlot != null && itemInSlot.itemIcon != null)
            {
                slotIcons[i].sprite = itemInSlot.itemIcon;
                slotIcons[i].enabled = true; // Show the icon

                // Apply transparency
                Color iconColor = slotIcons[i].color;
                iconColor.a = targetAlpha;
                slotIcons[i].color = iconColor;
            }
            else
            {
                slotIcons[i].sprite = null;
                slotIcons[i].enabled = false; // Hide the icon if slot is empty
            }

            // Update the Slot Highlight
            if (selectionHighlights.Length > i && selectionHighlights[i] != null)
            {
                // Only show highlight if we are not holding a two-handed item AND it's the current slot
                selectionHighlights[i].SetActive(!isHoldingTwoHanded && i == inventorySystem.currentSlotIndex);
            }
        }
    }
}

