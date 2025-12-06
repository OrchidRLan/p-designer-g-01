using UnityEngine;

public class MeshFeedback : MonoBehaviour
{
    public void ShowNoTargetWarning()
    {
        Debug.LogWarning("[XRC Mesh] No edit target assigned.");
    }

    public void ShowInvalidTargetWarning()
    {
        Debug.LogWarning("[XRC Mesh] Target must have a MeshFilter with a valid mesh.");
    }

    public void ShowEditNotAllowedWarning()
    {
        Debug.LogWarning("[XRC Mesh] Cannot enter edit mode in the current state.");
    }
}