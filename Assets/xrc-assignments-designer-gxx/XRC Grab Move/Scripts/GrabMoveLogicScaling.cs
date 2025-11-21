using UnityEngine;

public class GrabMoveLogicScaling
{
   private Transform playerTransform;
   private GrabMoveCalculationsScaling scalingCalculations;
   private GrabMoveCalculationsCoreMovement moveCalculations;

   private bool isActive = false;

   private Vector3 prevPivot;
   private Vector3 prevLeftPos;
   private Vector3 prevRightPos;
   
   private float currentScale = 1.0f;
   private float minScale = 0.01f;
   private float maxScale = 10.0f;
   private float prevScaleValue;
   public void UpdateScaling(Vector3 leftPos, Vector3 rightPos)
   {
      Vector3 pivot = moveCalculations.CalculatePivotPosition(leftPos, rightPos);
      float newScale = scalingCalculations.CalculateProportionalScale(leftPos,rightPos, prevLeftPos, prevRightPos, currentScale, minScale, maxScale);
      Vector3 offset = scalingCalculations.CalculateScaleOffset(pivot, playerTransform.position, newScale,currentScale);
      playerTransform.localScale = Vector3.one * newScale;
      playerTransform.position += offset;
      
      prevScaleValue = currentScale;
      currentScale = newScale;
      
      prevLeftPos = leftPos;
      prevRightPos = rightPos;
   }
   
   public float ClampScale(float min, float max)
   {
      currentScale = Mathf.Clamp(currentScale, min, max);
      return currentScale;
   }

   public void UpdatePlayerPositionAfterScale(Vector3 pivotposition)
   {
      Vector3 offset = scalingCalculations.CalculateScaleOffset(pivotposition,
         playerTransform.position, currentScale, prevScaleValue);
      playerTransform.position += offset;
   }
    
}
