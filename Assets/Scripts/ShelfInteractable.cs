using UnityEngine;
using UnityEngine.InputSystem;

public class ShelfInteractable : MonoBehaviour
{
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactableLayer;

    private readonly Collider[] interactableColliders = new Collider[3];
    [SerializeField] private int interactionFound;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
