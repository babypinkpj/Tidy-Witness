using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // ── Events ────────────────────────────────────────────────────────────────
    public static event System.Action OnItemPickedUp;
    [Header("Movement Stats")]
    public float maxSpeed = 7f;
    public float acceleration = 50f;
    public float deceleration = 40f;
    public float turnSpeed = 15f;

    [Header("References")]
    public Transform cameraTransform;

    [Header("Inventory Settings")]
    public InventorySystem inventory;
    public Transform dropPoint;
    public float pickupRange = 3f;
    public LayerMask interactableLayer;

    private Rigidbody rb;
    private Vector2 rawInput;
    private Vector3 currentVelocity;
    private MopController _mopController;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        _mopController = GetComponent<MopController>();

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        rawInput = value.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        HandleSmoothMovement();
    }

    void HandleSmoothMovement()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        Vector3 targetDirection = (forward.normalized * rawInput.y + right.normalized * rawInput.x).normalized;

        Vector3 targetVelocity = targetDirection * maxSpeed;

        float currentAccel = (targetDirection.magnitude > 0) ? acceleration : deceleration;

        currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, currentAccel * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);

        if (targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime));
        }
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Debug.Log("Interact Button Pressed!");

        // ── If a dialogue is already open, advance it ────────────────────────
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen)
        {
            DialogueManager.Instance.Advance();
            return;
        }

        // ── Otherwise check for NPC or Item in front of the player ───────────
        TryInteractWithWorld();
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed && inventory != null)
        {
            inventory.DropCurrentItem(transform);
        }
    }

    public void OnUse(InputAction.CallbackContext context)
    {
        if (_mopController == null) return;

        if (context.performed)
            _mopController.SetMopping(true);
        else if (context.canceled)
            _mopController.SetMopping(false);
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if (inventory == null || inventory.IsHoldingTwoHandedItem()) return;

        // Mouse scroll typically returns a Vector2 delta
        Vector2 scrollDelta = context.ReadValue<Vector2>();
        
        if (scrollDelta.y > 0)
            inventory.ScrollSlot(1);
        else if (scrollDelta.y < 0)
            inventory.ScrollSlot(-1);
    }

    public void OnSlot1(InputAction.CallbackContext context)
    {
        if (context.performed && inventory != null && !inventory.IsHoldingTwoHandedItem()) inventory.SwitchSlot(0);
    }

    public void OnSlot2(InputAction.CallbackContext context)
    {
        if (context.performed && inventory != null && !inventory.IsHoldingTwoHandedItem()) inventory.SwitchSlot(1);
    }

    public void OnSlot3(InputAction.CallbackContext context)
    {
        if (context.performed && inventory != null && !inventory.IsHoldingTwoHandedItem()) inventory.SwitchSlot(2);
    }

    public void OnSlot4(InputAction.CallbackContext context)
    {
        if (context.performed && inventory != null && !inventory.IsHoldingTwoHandedItem()) inventory.SwitchSlot(3);
    }

    void TryInteractWithWorld()
    {
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * pickupRange, Color.red, 2f);

        RaycastHit hit;
        if (!Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, pickupRange, interactableLayer))
        {
            Debug.Log("Raycast didn't hit anything. Check Layer, Distance, or Collider.");
            return;
        }

        Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

        // ── 1. Try NPC dialogue first ────────────────────────────────────────
        NPCDialogueTrigger npc = hit.collider.GetComponentInParent<NPCDialogueTrigger>();
        if (npc == null) npc = hit.collider.GetComponentInChildren<NPCDialogueTrigger>();

        if (npc != null)
        {
            npc.TryStartDialogue(transform);
            return;
        }

        // ── 2. Try Piano puzzle ─────────────────────────────────────────────
        PianoInteractable piano = hit.collider.GetComponentInParent<PianoInteractable>();
        if (piano == null) piano = hit.collider.GetComponentInChildren<PianoInteractable>();

        if (piano != null)
        {
            piano.Interact(this);
            return;
        }

        // ── 2. Try item pickup ───────────────────────────────────────────────
        if (inventory == null)
        {
            Debug.LogWarning("Inventory reference is missing on PlayerController!");
            return;
        }

        TryPickUpItem(hit);
    }

    void TryPickUpItem(RaycastHit hit)
    {
        if (inventory.IsHoldingTwoHandedItem())
        {
            Debug.Log("Cannot pick up: Holding a two-handed item.");
            return;
        }

        // Check parent and children just in case the collider is separated from the script
        ItemObject itemObj = hit.collider.GetComponentInParent<ItemObject>();
        if (itemObj == null) itemObj = hit.collider.GetComponentInChildren<ItemObject>();

        if (itemObj != null)
        {
            if (itemObj.referenceItem != null)
            {
                if (inventory.AddItem(itemObj.referenceItem))
                {
                    Debug.Log("Successfully added " + itemObj.referenceItem.name + " to inventory.");
                    itemObj.OnHandlePickItem();

                    // Notify any NPC sight scripts that a pickup just happened
                    OnItemPickedUp?.Invoke();
                }
                else
                {
                    Debug.Log("Inventory full! Not enough empty slots.");
                }
            }
            else
            {
                Debug.LogWarning("The ItemObject you hit is missing its 'Reference Item' (ScriptableObject)!");
            }
        }
        else
        {
            Debug.LogWarning("Hit an object on the Interactable layer, but it has no ItemObject script.");
        }
    }
}