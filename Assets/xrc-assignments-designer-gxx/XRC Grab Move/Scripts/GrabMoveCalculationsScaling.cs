using UnityEngine;

public class GrabMoveCalculationsScaling
{
    public float CalculateProportionalScale(Vector3 leftPos, Vector3 rightPos, Vector3 prevLeftPos,
        Vector3 prevRightPos, float currentScale, float minScale, float maxScale)
    {
        float prevDistance = Vector3.Distance(prevRightPos,prevLeftPos);
        float currentDistance = Vector3.Distance(rightPos,leftPos);
        float scaleRatio= prevDistance / currentDistance;
        float newScale = currentScale * scaleRatio;
        newScale = Mathf.Clamp(newScale, minScale, maxScale);
        return newScale;
    }
    
    public Vector3 CalculateScaleOffset(Vector3 pivot, Vector3 targetPos, float newScale,
        float prevScale)
    {
        Vector3 prevDirection = targetPos - pivot;
        float scaleRatio = newScale / prevScale;
        Vector3 scaleDirection = prevDirection * scaleRatio;
        Vector3 newPosition = pivot + scaleDirection;
        Vector3 offset = newPosition - targetPos;
        return offset;
    }
    
}
