using UnityEngine;

public static class GrabMoveCalculationsCoreMovement
{
    public static (Vector3 axis, float angle) CalculateDirectionBasedRotation(
        Vector3 leftFwd,
        Vector3 rightFwd,
        Vector3 prevLeftFwd,
        Vector3 prevRightFwd,
        Vector3 leftToRight
    )
    {
        Vector3 handAxis = leftToRight;
        float handAxisMag = handAxis.magnitude;
        if (handAxisMag < 0.0001f)
        {
            return (Vector3.zero, 0f);
        }

        Vector3 handAxisNorm = handAxis / handAxisMag;

        Vector3 prevFwd = (prevLeftFwd + prevRightFwd) * 0.5f;
        Vector3 currFwd = (leftFwd + rightFwd) * 0.5f;

        prevFwd = Vector3.ProjectOnPlane(prevFwd, handAxisNorm);
        currFwd = Vector3.ProjectOnPlane(currFwd, handAxisNorm);

        float prevMag = prevFwd.magnitude;
        float currMag = currFwd.magnitude;

        if (prevMag < 0.0001f || currMag < 0.0001f)
        {
            return (Vector3.zero, 0f);
        }

        prevFwd /= prevMag;
        currFwd /= currMag;

        float angle = Vector3.SignedAngle(prevFwd, currFwd, handAxisNorm);
        return (handAxisNorm, angle);
    }

    public static Vector3 CalculatePivotPosition(Vector3 pos1, Vector3 pos2)
    {
        return (pos1 + pos2) * 0.5f;
    }

    public static (Vector3 axis, float angle) CalculatePositionBasedRotation(
        Vector3 leftPos,
        Vector3 rightPos,
        Vector3 prevLeftPos,
        Vector3 prevRightPos
    )
    {
        Vector3 prevVec = prevRightPos - prevLeftPos;
        Vector3 currVec = rightPos - leftPos;

        float prevMag = prevVec.magnitude;
        float currMag = currVec.magnitude;

        if (prevMag < 0.0001f || currMag < 0.0001f)
        {
            return (Vector3.zero, 0f);
        }

        Quaternion rot = Quaternion.FromToRotation(prevVec, currVec);
        rot.ToAngleAxis(out float angle, out Vector3 axis);

        if (axis.sqrMagnitude < 0.000001f)
        {
            return (Vector3.zero, 0f);
        }

        axis.Normalize();
        return (axis, angle);
    }

    public static float CalculateProportionalScale(
        Vector3 leftPos,
        Vector3 rightPos,
        Vector3 prevLeftPos,
        Vector3 prevRightPos,
        float currScale,
        float minScale,
        float maxScale
    )
    {
        float prevDist = Vector3.Distance(prevLeftPos, prevRightPos);
        float currDist = Vector3.Distance(leftPos, rightPos);

        if (prevDist < 0.0001f || currDist < 0.0001f)
        {
            return Mathf.Clamp(currScale, minScale, maxScale);
        }

        float ratio = prevDist / currDist;
        float newScale = currScale * ratio;
        return Mathf.Clamp(newScale, minScale, maxScale);
    }

    public static Vector3 CalculateScaleOffset(
        Vector3 pivotPos,
        Vector3 targetPos,
        float newScale,
        float prevScale
    )
    {
        if (prevScale < 0.0001f)
        {
            return targetPos;
        }

        float factor = newScale / prevScale;
        Vector3 offset = targetPos - pivotPos;
        offset *= factor;
        return pivotPos + offset;
    }

    public static Vector3 CalculateTranslation(
        Vector3 currPivotPos,
        Vector3 prevPivotPos
    )
    {
        return prevPivotPos - currPivotPos;
    }

    public static (Vector3 axis, float angle) ProjectRotationToYaw(
        Vector3 axis,
        float angle,
        Vector3 upAxis
    )
    {
        float upMag = upAxis.magnitude;
        float axisMag = axis.magnitude;

        if (upMag < 0.0001f || axisMag < 0.0001f || Mathf.Abs(angle) < 0.0001f)
        {
            return (Vector3.zero, 0f);
        }

        Vector3 upNorm = upAxis / upMag;
        Vector3 axisNorm = axis / axisMag;

        float proj = Vector3.Dot(axisNorm, upNorm);
        float yawAngle = angle * proj;
        Vector3 yawAxis = upNorm;

        return (yawAxis, yawAngle);
    }
}