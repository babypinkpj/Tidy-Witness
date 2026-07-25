using UnityEngine;

public class InventoryItem
{
    public Item data { get; private set; }
    public InventoryItem(Item data)
    {
        this.data = data;
    }

}
