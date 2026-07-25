using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public const int MaxSlots = 4;
    public Item[] slots = new Item[MaxSlots];
    public int currentSlotIndex = 0;

    public bool IsHoldingTwoHandedItem()
    {
        return slots[currentSlotIndex] != null && slots[currentSlotIndex].twoHanded;
    }

    public bool AddItem(Item item)
    {
        if (IsHoldingTwoHandedItem()) return false;

        int requiredSlots = item.twoHanded ? 2 : 1;
        int foundEmptySlots = 0;
        int[] emptySlotIndices = new int[requiredSlots];

        // Prefer current slot if it's empty
        if (slots[currentSlotIndex] == null)
        {
            emptySlotIndices[foundEmptySlots] = currentSlotIndex;
            foundEmptySlots++;
        }

        // Find other empty slots
        for (int i = 0; i < MaxSlots && foundEmptySlots < requiredSlots; i++)
        {
            if (slots[i] == null && i != currentSlotIndex)
            {
                emptySlotIndices[foundEmptySlots] = i;
                foundEmptySlots++;
            }
        }

        if (foundEmptySlots == requiredSlots)
        {
            for (int i = 0; i < requiredSlots; i++)
            {
                slots[emptySlotIndices[i]] = item;
            }

            if (item.twoHanded)
            {
                currentSlotIndex = emptySlotIndices[0];
            }
            return true;
        }

        return false; // Not enough slots
    }

    [Tooltip("How far in front of the player the item is dropped.")]
    public float dropForwardDistance = 1f;

    public void DropCurrentItem(Transform playerTransform)
    {
        if (slots[currentSlotIndex] != null)
        {
            Item droppedItem = slots[currentSlotIndex];

            // Remove from all slots it occupies
            for (int i = 0; i < MaxSlots; i++)
            {
                if (slots[i] == droppedItem)
                    slots[i] = null;
            }

            if (droppedItem.itemPrefab != null)
            {
                // Calculate a point in front of the player (ignore vertical direction)
                Vector3 forward = playerTransform.forward;
                forward.y = 0f;
                forward.Normalize();

                Vector3 dropOrigin = playerTransform.position + forward * dropForwardDistance;

                // Raycast down from waist height to find the ground surface
                Vector3 rayStart = dropOrigin + Vector3.up * 2f;
                Vector3 spawnPosition = dropOrigin; // fallback if no ground found

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f))
                {
                    // Land on the ground surface + per-item height offset to prevent clipping
                    spawnPosition = hit.point + Vector3.up * droppedItem.dropHeightOffset;
                }

                Instantiate(droppedItem.itemPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    public void SwitchSlot(int slotIndex)
    {
        if (IsHoldingTwoHandedItem()) return; 
        
        if (slotIndex >= 0 && slotIndex < MaxSlots)
        {
            currentSlotIndex = slotIndex;
        }
    }

    public void ScrollSlot(int direction)
    {
        if (IsHoldingTwoHandedItem()) return;

        currentSlotIndex += direction;
        if (currentSlotIndex >= MaxSlots) currentSlotIndex = 0;
        if (currentSlotIndex < 0) currentSlotIndex = MaxSlots - 1;
    }
}
