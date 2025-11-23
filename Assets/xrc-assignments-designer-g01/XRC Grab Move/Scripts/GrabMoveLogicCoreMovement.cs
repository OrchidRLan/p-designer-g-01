using System.Collections.Generic;
using UnityEngine;

public class GrabMoveLogicCoreMovement : MonoBehaviour
{
    [SerializeField] 
    private Transform player;
    [SerializeField] 
    private Transform leftTransform;
    [SerializeField] 
    private Transform rightTransform;
    [SerializeField] 
    private List<Transform> attachedObjects = new List<Transform>();
    [SerializeField] 
    private bool enableTranslation = true;
    [SerializeField] 
    private bool enableRotation = true;
    [SerializeField] 
    private bool RestrictRotationToYaw = false;
    
    private bool _isRunning;
    private Vector3 pivotPos;
    private Vector3 prevPivotPos;
    private Vector3 prevLeftPos;
    private Vector3 prevRightPos;
    private Vector3 prevLeftFwd;
    private Vector3 prevRightFwd;
    private float currScale = 1f;

    List<Transform> origParents = new List<Transform>();

    public bool IsRunning => _isRunning;

    private void Update()
    {
        if (!_isRunning) return;
        if (player == null || leftTransform == null || rightTransform == null) return;

        Vector3 leftPos = leftTransform.position;
        Vector3 rightPos = rightTransform.position;
        Vector3 leftFwd = leftTransform.forward;
        Vector3 rightFwd = rightTransform.forward;

        pivotPos = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(leftPos, rightPos);

        if (enableTranslation)
        {
            Vector3 trans = GrabMoveCalculationsCoreMovement.CalculateTranslation(pivotPos, prevPivotPos);
            if (trans.sqrMagnitude > 0.000001f)
            {
                player.position += trans;
            }
        }

        if (enableRotation)
        {
            var (axisPos, anglePos) = GrabMoveCalculationsCoreMovement.CalculatePositionBasedRotation(
                leftPos, rightPos, prevLeftPos, prevRightPos
            );

            Vector3 leftToRight = rightPos - leftPos;
            var (axisDir, angleDir) = GrabMoveCalculationsCoreMovement.CalculateDirectionBasedRotation(
                leftFwd, rightFwd, prevLeftFwd, prevRightFwd, leftToRight
            );

            Quaternion rotPos = Quaternion.AngleAxis(anglePos, axisPos);
            Quaternion rotDir = Quaternion.AngleAxis(angleDir, axisDir);
            Quaternion combined = rotDir * rotPos;

            combined.ToAngleAxis(out float combinedAngle, out Vector3 combinedAxis);
            
            if (RestrictRotationToYaw)
            {
                var (yawAxis, yawAngle) = GrabMoveCalculationsCoreMovement.ProjectRotationToYaw(
                    combinedAxis, combinedAngle, player.up
                );
                ApplyRotation(yawAxis, yawAngle);
            }
            else
            {
                ApplyRotation(combinedAxis, combinedAngle);
            }
        }
        
        Vector3 newLeftPos = leftTransform.position;
        Vector3 newRightPos = rightTransform.position;
        Vector3 newLeftFwd = leftTransform.forward;
        Vector3 newRightFwd = rightTransform.forward;

        Vector3 newPivot = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(newLeftPos, newRightPos);

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

        pivotPos = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(prevLeftPos, prevRightPos);
        prevPivotPos = pivotPos;

        currScale = player.localScale.x;

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
}
