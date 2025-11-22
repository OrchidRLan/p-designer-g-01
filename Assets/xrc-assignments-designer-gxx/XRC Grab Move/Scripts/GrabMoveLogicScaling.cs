using UnityEngine;

public class GrabMoveLogicScaling: MonoBehaviour
{
   [Header("References")]
   [SerializeField] private Transform playerTransform;
   [SerializeField] private GrabMoveCalculationsScaling scalingCalculations;
   [SerializeField] private GrabMoveFeedback feedback;
   

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
      Vector3 pivot = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(leftPos, rightPos);
      if (playerTransform == null || scalingCalculations == null)
         return;
      float newScale = scalingCalculations.CalculateProportionalScale(leftPos,rightPos, prevLeftPos, prevRightPos, currentScale, minScale, maxScale);
      Vector3 offset = scalingCalculations.CalculateScaleOffset(pivot, playerTransform.position, newScale,currentScale);
      
      playerTransform.localScale = Vector3.one * newScale;
      playerTransform.position += offset;

      if (feedback != null)
      {
         feedback.UpdatePivotVisual(pivot);
         feedback.UpdateLineVisual(leftPos, rightPos);
         feedback.UpdateScaleText(GetWorldScalePercent());
      }
      
      prevScaleValue = currentScale;
      currentScale = newScale;
      prevLeftPos = leftPos;
      prevRightPos = rightPos;
   }
   
   public void Begin(Vector3 leftPos, Vector3 rightPos)
   {
      prevLeftPos = leftPos;
      prevRightPos = rightPos;
      
      if (feedback != null)
      {
         feedback.InitializeFeedbackInstances();
         feedback.SetFeedbackActive(true);
      }
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
   
   public float GetWorldScalePercent()
   {
      return (1.0f / currentScale) * 100f;
   }
   
   public void End()
   {
      if (feedback != null)
      {
         feedback.SetFeedbackActive(false);
      }
   }
    
}
