using UnityEngine;
//Trigger เอาไว้ให้ player system ใดก็ได้ call ได้ (เช่น player interaction, cutscene, หรืออื่น ๆ)
public class InteractableEvidence : MonoBehaviour, IInteractable
{
    [SerializeField] string evidenceID;
    [SerializeField] GameState[] onlyActiveInStates; // optional guard

    public void OnInteract()
    {
        if (onlyActiveInStates.Length > 0)
        {
            if (!System.Array.Exists(onlyActiveInStates,
                s => s == GameStateManager.Instance.Current)) return;
        }
        EvidenceManager.Instance.Unlock(evidenceID);
    }
}

// IInteractable interface (เพื่อให้ player system ใดก็ได้ call ได้)
public interface IInteractable
{
    void OnInteract();
}
