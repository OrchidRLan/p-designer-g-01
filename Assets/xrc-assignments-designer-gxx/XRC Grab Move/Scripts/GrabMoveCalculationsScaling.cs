using UnityEngine;

public static class GrabMoveCalculationsScaling
{
    public static float CalculateProportionalScale(
        Vector3 leftPos, 
        Vector3 rightPos, 
        Vector3 prevLeftPos,
        Vector3 prevRightPos, 
        float currentScale, 
        float minScale, 
        float maxScale)
    {
        float prevDistance = Vector3.Distance(prevLeftPos, prevRightPos);
        float currentDistance = Vector3.Distance(leftPos, rightPos);
        
        if (prevDistance < 0.001f || currentDistance < 0.001f)
        {
            return currentScale;
        }
        
        float scaleRatio = prevDistance / currentDistance;
        float newScale = currentScale * scaleRatio;
        newScale = Mathf.Clamp(newScale, minScale, maxScale);
        
        return newScale;
    }
    
    public static Vector3 CalculateScaleOffset(
        Vector3 pivot, 
        Vector3 targetPos, 
        float newScale,
        float prevScale)
    {
        if (prevScale < 0.001f)
        {
            return Vector3.zero;
        }
        
        Vector3 direction = targetPos - pivot;
        float scaleRatio = newScale / prevScale;
        Vector3 scaledDirection = direction * scaleRatio;
        Vector3 newPosition = pivot + scaledDirection;
        Vector3 offset = newPosition - targetPos;
        
        return offset;
    }
}