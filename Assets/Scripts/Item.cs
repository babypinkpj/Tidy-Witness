using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Create Items/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject itemPrefab;
    public bool twoHanded;

    [Tooltip("If true, pressing Use while holding this item will mop the floor and reduce suspicion.")]
    public bool isMoppable;

    [Tooltip("How high above the ground surface the item sits when dropped. Increase this if the item clips into the floor.")]
    public float dropHeightOffset = 0.1f;
}