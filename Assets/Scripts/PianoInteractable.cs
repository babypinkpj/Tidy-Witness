using UnityEngine;

public class PianoInteractable : MonoBehaviour
{
    public PianoPuzzle puzzle;

    void Awake()
    {
        if (puzzle == null)
            puzzle = GetComponent<PianoPuzzle>();
        if (puzzle == null)
            puzzle = GetComponentInChildren<PianoPuzzle>();
    }

    public void Interact(PlayerController player)
    {
        if (puzzle != null)
        {
            if (puzzle.playerController == null)
                puzzle.playerController = player;

            puzzle.Activate();
        }
        else
        {
            Debug.LogWarning("[PianoInteractable] No PianoPuzzle component attached or referenced!");
        }
    }
}
