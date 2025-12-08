using UnityEngine;

public class MeshWidget : MonoBehaviour
{
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    private void Awake()
    {
        //store object's initial pose in the scene
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    public void RestoreOriginalPose()
    {
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
    }
}

