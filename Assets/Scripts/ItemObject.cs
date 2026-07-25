using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public Item referenceItem;
    public void OnHandlePickItem()
    {
        //InventorySystem.current.Add(referenceItem);
        Destroy(gameObject);
    }
}
