using System.Collections.Generic;
using UnityEngine;

public class GrabMoveLogicCoreMovement : MonoBehaviour
{
    [SerializeField] 
    private Transform player;
    [SerializeField] 
    private Transform leftTr;
    [SerializeField] 
    private Transform rightTr;
    [SerializeField] 
    private List<Transform> attachedObjs = new List<Transform>();
    [SerializeField] 
    private float minScale = 0.0001f;
    [SerializeField] 
    private float maxScale = 10f;
    // [SerializeField] 
    // private bool enableScale = true;
    [SerializeField] 
    private bool enableTrans = true;
    [SerializeField] 
    private bool enableRot = true;
    [SerializeField] 
    private bool yawOnly = false;
    
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
        if (player == null || leftTr == null || rightTr == null) return;

        Vector3 leftPos = leftTr.position;
        Vector3 rightPos = rightTr.position;
        Vector3 leftFwd = leftTr.forward;
        Vector3 rightFwd = rightTr.forward;

        pivotPos = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(leftPos, rightPos);

        if (enableTrans)
        {
            Vector3 trans = GrabMoveCalculationsCoreMovement.CalculateTranslation(pivotPos, prevPivotPos);
            if (trans.sqrMagnitude > 0.000001f)
            {
                player.position += trans;
            }
        }

        if (enableRot)
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

            if (yawOnly)
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

        // if (enableScale)
        // {
        //     float newScale = GrabMoveCalculationsCoreMovement.CalculateProportionalScale(
        //         leftPos, rightPos, prevLeftPos, prevRightPos,
        //         currScale, minScale, maxScale
        //     );
        //
        //     if (Mathf.Abs(newScale - currScale) > 0.0001f)
        //     {
        //         Vector3 offset = GrabMoveCalculationsCoreMovement.CalculateScaleOffset(
        //             pivotPos, player.position, newScale, currScale
        //         );
        //
        //         player.position = offset;
        //         player.localScale = Vector3.one * newScale;
        //         currScale = newScale;
        //     }
        // }

        Vector3 newLeftPos = leftTr.position;
        Vector3 newRightPos = rightTr.position;
        Vector3 newLeftFwd = leftTr.forward;
        Vector3 newRightFwd = rightTr.forward;

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
        if (player == null || leftTr == null || rightTr == null) return;

        _isRunning = true;

        prevLeftPos = leftTr.position;
        prevRightPos = rightTr.position;
        prevLeftFwd = leftTr.forward;
        prevRightFwd = rightTr.forward;

        pivotPos = GrabMoveCalculationsCoreMovement.CalculatePivotPosition(prevLeftPos, prevRightPos);
        prevPivotPos = pivotPos;

        currScale = player.localScale.x;

        origParents.Clear();
        foreach (Transform t in attachedObjs)
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

        for (int i = 0; i < attachedObjs.Count && i < origParents.Count; i++)
        {
            Transform t = attachedObjs[i];
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

        Quaternion rotation = Quaternion.AngleAxis(angle, axis.normalized);
        Vector3 offset = player.position - pivotPos;
        offset = rotation * offset;
        
        player.position = pivotPos + offset;
        player.rotation = rotation * player.rotation;
    }
}