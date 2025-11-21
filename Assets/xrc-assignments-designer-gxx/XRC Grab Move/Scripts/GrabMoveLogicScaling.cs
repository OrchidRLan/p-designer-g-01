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
   public void UpdateScaling(Vector3 leftPos, Vector3 rightPos)
   {
      Vector3 pivot = moveCalculations.CalculatePivotPosition(leftPos, rightPos);
      float newScale = scalingCalculations.CalculateProportionalScale(Vector3 leftPos, Vector3 rightPos, Vector3 prevLeftPos,
         Vector3 prevRightPos, float currentScale, float minScale, float maxScale);
      Vector3 offset = scalingCalculations.CalculateScaleOffset(Vector3 pivot, Vector3 targetPos, float newScale,
         float prevScale);
   }
   public float ClampScale(float min, float max)
   {
      currentScale = Mathf.Clamp(currentScale, min, max);
      return currentScale;
   }

   public void UpdatePlayerPositionAfterScale(Vector3 pivotposition)
   {
      
   }
    
}
