using UnityEngine;

/// <summary>
/// 临时占位符 - 等Gino完成后替换
/// </summary>
public class GrabMoveCalculationsCoreMovement : MonoBehaviour
{
    public Vector3 CalculatePivotPosition(Vector3 leftPos, Vector3 rightPos)
    {
        return (leftPos + rightPos) / 2f;
    }
    
    public Vector3 CalculateTranslation(Vector3 currentPivot, Vector3 previousPivot)
    {
        return currentPivot - previousPivot;
    }
    
    public Quaternion CalculatePositionBasedRotation(Vector3 leftPos, Vector3 rightPos, 
        Vector3 prevLeftPos, Vector3 prevRightPos)
    {
        return Quaternion.identity; // 临时返回
    }
}