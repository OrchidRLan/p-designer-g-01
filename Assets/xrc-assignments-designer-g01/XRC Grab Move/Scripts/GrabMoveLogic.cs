using System.Collections.Generic;
using UnityEngine;

public class GrabMoveLogic : MonoBehaviour
{
    [Header("Player & Controller References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform leftTransform;
    [SerializeField] private Transform rightTransform;
    
    [Header("Attached Objects")]
    [SerializeField] private List<Transform> attachedObjects = new List<Transform>();
    
    [Header("Feature Toggles")]
    [SerializeField] private bool enableTranslation = true;
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private bool enableScaling = true;
    [SerializeField] private bool restrictRotationToYaw = false;
    
    [Header("Scale Settings")]
    [SerializeField] private float minScale = 0.01f;
    [SerializeField] private float maxScale = 10.0f;
    
    [Header("Smoothing Settings - 消除Jitter")]
    [SerializeField] [Range(0f, 1f)] private float positionSmoothness = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float rotationSmoothness = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float scaleSmoothness = 0.3f;
    [SerializeField] private float minMovementThreshold = 0.0001f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GrabMoveFeedback feedback;
    
    private bool _isRunning;
    private Vector3 pivotPos;
    private Vector3 prevPivotPos;
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private Vector3 prevLeftFwd;
    private Vector3 prevRightFwd;
    private float currentScale = 1f;
    private float prevScaleValue;
    
    private Vector3 smoothedLeftPos;
    private Vector3 smoothedRightPos;
    private float smoothedScale;
    
    private List<Transform> origParents = new List<Transform>();

    public bool IsRunning => _isRunning;
    
    private void Update()
    {
        if (!_isRunning) return;
        if (player == null || leftTransform == null || rightTransform == null) return;

        Vector3 rawLeftPos = leftTransform.position;
        Vector3 rawRightPos = rightTransform.position;
        Vector3 leftFwd = leftTransform.forward;
        Vector3 rightFwd = rightTransform.forward;
        
        smoothedLeftPos = Vector3.Lerp(smoothedLeftPos, rawLeftPos, 1f - positionSmoothness);
        smoothedRightPos = Vector3.Lerp(smoothedRightPos, rawRightPos, 1f - positionSmoothness);
        
        Vector3 leftPos = smoothedLeftPos;
        Vector3 rightPos = smoothedRightPos;
        
        pivotPos = GrabMoveCalculations.CalculatePivotPosition(leftPos, rightPos);
        
        if (enableScaling)
        {
            float rawNewScale = GrabMoveCalculations.CalculateProportionalScale(
                leftPos, rightPos, prevLeftPos, prevRightPos, 
                currentScale, minScale, maxScale
            );
            
            smoothedScale = Mathf.Lerp(smoothedScale, rawNewScale, 1f - scaleSmoothness);
            float newScale = smoothedScale;
            
            if (Mathf.Abs(newScale - currentScale) > 0.0001f)
            {
                Vector3 offset = GrabMoveCalculations.CalculateScaleOffset(
                    pivotPos, player.position, newScale, currentScale
                );
                
                player.localScale = Vector3.one * newScale;
                player.position += offset;
                
                prevScaleValue = currentScale;
                currentScale = newScale;
            }
        }

        if (enableTranslation)
        {
            Vector3 trans = GrabMoveCalculations.CalculateTranslation(pivotPos, prevPivotPos);
            if (trans.sqrMagnitude > minMovementThreshold)
            {
                player.position += trans;
            }
        }

        if (enableRotation)
        {
            var (axisPos, anglePos) = GrabMoveCalculations.CalculatePositionBasedRotation(
                leftPos, rightPos, prevLeftPos, prevRightPos
            );

            Vector3 leftToRight = rightPos - leftPos;
            var (axisDir, angleDir) = GrabMoveCalculations.CalculateDirectionBasedRotation(
                leftFwd, rightFwd, prevLeftFwd, prevRightFwd, leftToRight
            );

            Quaternion rotPos = Quaternion.AngleAxis(anglePos, axisPos);
            Quaternion rotDir = Quaternion.AngleAxis(angleDir, axisDir);
            Quaternion combined = rotDir * rotPos;

            combined.ToAngleAxis(out float combinedAngle, out Vector3 combinedAxis);
            
            combinedAngle *= (1f - rotationSmoothness);
            
            if (restrictRotationToYaw)
            {
                var (yawAxis, yawAngle) = GrabMoveCalculations.ProjectRotationToYaw(
                    combinedAxis, combinedAngle, player.up
                );
                ApplyRotation(yawAxis, yawAngle);
            }
            else
            {
                ApplyRotation(combinedAxis, combinedAngle);
            }
        }
        
        if (feedback != null && enableScaling)
        {
            feedback.UpdatePivotVisual(pivotPos);
            feedback.UpdateLineVisual(leftPos, rightPos);
            feedback.UpdateScaleText(GetWorldScalePercent(), pivotPos);
        }
        
        Vector3 newLeftPos = leftTransform.position;
        Vector3 newRightPos = rightTransform.position;
        Vector3 newLeftFwd = leftTransform.forward;
        Vector3 newRightFwd = rightTransform.forward;

        Vector3 newPivot = GrabMoveCalculations.CalculatePivotPosition(newLeftPos, newRightPos);

        prevPivotPos = newPivot;
        prevLeftPos = newLeftPos;
        prevRightPos = newRightPos;
        prevLeftFwd = newLeftFwd;
        prevRightFwd = newRightFwd;
        pivotPos = newPivot;
    }

    public void Begin()
    {
        if (player == null || leftTransform == null || rightTransform == null) return;

        _isRunning = true;
        
        prevLeftPos = leftTransform.position;
        prevRightPos = rightTransform.position;
        prevLeftFwd = leftTransform.forward;
        prevRightFwd = rightTransform.forward;
        
        smoothedLeftPos = prevLeftPos;
        smoothedRightPos = prevRightPos;
        smoothedScale = currentScale;

        pivotPos = GrabMoveCalculations.CalculatePivotPosition(prevLeftPos, prevRightPos);
        prevPivotPos = pivotPos;

        currentScale = player.localScale.x;
        prevScaleValue = currentScale;
        
        origParents.Clear();
        foreach (Transform t in attachedObjects)
        {
            if (t == null)
            {
                origParents.Add(null);
            }
            else
            {
                origParents.Add(t.parent);
                t.SetParent(player, true);
            }
        }
        
        if (feedback != null && enableScaling)
        {
            feedback.InitializeFeedbackInstances();
            feedback.SetFeedbackActive(true);
        }
    }

    public void End()
    {
        if (!_isRunning) return;

        _isRunning = false;

        for (int i = 0; i < attachedObjects.Count && i < origParents.Count; i++)
        {
            Transform t = attachedObjects[i];
            Transform origParent = origParents[i];
            if (t != null)
            {
                t.SetParent(origParent, true);
            }
        }
        origParents.Clear();
        
        if (feedback != null)
        {
            feedback.SetFeedbackActive(false);
        }
    }

    private void ApplyRotation(Vector3 axis, float angle)
    {
        if (player == null) return;
        if (axis.sqrMagnitude < 0.000001f) return;
        if (Mathf.Abs(angle) < 0.0001f) return;

        Quaternion rotation = Quaternion.AngleAxis(-angle, axis.normalized);
        Vector3 offset = player.position - pivotPos;
        offset = rotation * offset;
        
        player.position = pivotPos + offset;
        player.rotation = rotation * player.rotation;
    }
    
    public float GetWorldScalePercent()
    {
        if (currentScale < 0.0001f)
        {
            return 0f;
        } 
        return (1f / currentScale) * 100f;
    }
    
    public float GetCurrentScale()
    {
        return currentScale;
    }
}