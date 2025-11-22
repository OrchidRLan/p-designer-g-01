using UnityEngine;

/// <summary>
/// 临时占位符 - 等Gino完成后替换
/// </summary>
public class GrabMoveLogicCoreMovement : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GrabMoveCalculationsCoreMovement calculations;
    
    public void Begin(Vector3 leftPos, Vector3 rightPos)
    {
        Debug.Log("CoreMovement: Begin (Placeholder)");
    }
    
    public void End()
    {
        Debug.Log("CoreMovement: End (Placeholder)");
    }
    
    public void UpdateMovement(Vector3 leftPos, Vector3 rightPos)
    {
        // 临时空实现
    }
}