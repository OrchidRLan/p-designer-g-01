using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

//TODO fix the sizes of stems
public class ScaleWidget : MonoBehaviour
{
    [SerializeField]
    private GameObject HandlePrefab;

    [SerializeField]
    private float HandleOffset = 0.1f;

    [SerializeField]
    private Material StemMaterial;

    [SerializeField]
    private float StemWidth = 0.01f;
    
    private ScaleLogic ScaleLogic;
    private GameObject handleX, handleY, handleZ;
    private LineRenderer stemX, stemY, stemZ;

    private void Awake()
    {
        ScaleLogic = GetComponent<ScaleLogic>();
    }
    private void OnEnable()
    {
        if (ScaleLogic != null)
        {
            ScaleLogic.OnEnterEditMode += CreateHandles;
            ScaleLogic.OnExitEditMode += DestroyHandles;
        }
    }

    private void OnDisable()
    {
        if (ScaleLogic != null)
        {
            ScaleLogic.OnEnterEditMode -= CreateHandles;
            ScaleLogic.OnExitEditMode -= DestroyHandles;
        }
        DestroyHandles();
    }

    private void LateUpdate()
    {
        if (ScaleLogic == null || !ScaleLogic.isEditModeActive || ScaleLogic.editObject == null)
        {
            return;
        }

        ProcessHandle(handleX, Vector3.right, stemX);
        ProcessHandle(handleY, Vector3.up, stemY);
        ProcessHandle(handleZ, Vector3.forward, stemZ);
    }

    private void CreateHandles()
    {
        if (ScaleLogic.editObject == null || HandlePrefab == null)
        {
            return;
        }

        handleX = CreateHandle(Vector3.right, Color.red, "HandleX", out stemX);
        handleY = CreateHandle(Vector3.up, Color.green, "HandleY", out stemY);
        handleZ = CreateHandle(Vector3.forward, Color.blue, "HandleZ", out stemZ);
    }

    private GameObject CreateHandle(Vector3 axis, Color color, string name, out LineRenderer stem)
    {
        GameObject handle = Instantiate(HandlePrefab, transform);
        handle.name = name;

        XRGrabInteractable grab = handle.GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.throwOnDetach = false;
        }

        Renderer r = handle.GetComponentInChildren<Renderer>();
        r.material.color = color;
     
        stem = handle.AddComponent<LineRenderer>();
        stem.material = StemMaterial;
       
        stem.material.color = color;
        stem.startWidth = StemWidth;
        stem.endWidth = StemWidth;
        stem.positionCount = 2;
        stem.useWorldSpace = true;

        PositionHandle(handle, axis);
        return handle;
    }

    private void DestroyHandles()
    {
        if (handleX != null)
        {
            Destroy(handleX);
        }
        if (handleY != null)
        {
            Destroy(handleY);
        }
        if (handleZ != null)
        {
            Destroy(handleZ);
        }
    }

    private void PositionHandle(GameObject handle, Vector3 axis)
    {
        if (handle == null)
        {
            return;
        }
        handle.transform.position = CalculateTargetPosition(axis);
        handle.transform.rotation = ScaleLogic.editObject.transform.rotation;
    }

    private Vector3 CalculateTargetPosition(Vector3 localAxis)
    {
        Transform targetT = ScaleLogic.editObject.transform;
        MeshFilter mf = ScaleLogic.editObject.GetComponent<MeshFilter>();
        Bounds bounds = (mf != null) ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);

        Vector3 localExtent = Vector3.Scale(bounds.extents, localAxis);
        Vector3 localCenter = Vector3.Scale(bounds.center, localAxis);

        float currentAxisScale = Vector3.Dot(targetT.localScale, localAxis);
        float distToEdge = (Vector3.Dot(localExtent, localAxis) + Vector3.Dot(localCenter, localAxis)) * currentAxisScale;
        float totalDist = distToEdge + HandleOffset;

        return targetT.position + (targetT.rotation * (localAxis * totalDist));
    }

    private void ProcessHandle(GameObject handle, Vector3 localAxis, LineRenderer stem)
    {
        if (handle == null)
        {
            return;
        }

        XRGrabInteractable grab = handle.GetComponent<XRGrabInteractable>();
        bool isGrabbed = (grab != null && grab.isSelected);
        
        if (isGrabbed)
        {
            var interactor = grab.interactorsSelecting[0];
            Transform handTransform = interactor.GetAttachTransform(grab);
            Vector3 handPosition = handTransform != null ? handTransform.position : interactor.transform.position;

            Transform targetT = ScaleLogic.editObject.transform;
            Vector3 axisWorld = targetT.rotation * localAxis;

            Vector3 targetToHand = handPosition - targetT.position;
            float projectedDist = Vector3.Dot(targetToHand, axisWorld);

            MeshFilter mf = ScaleLogic.editObject.GetComponent<MeshFilter>();
            Bounds b = (mf != null) ? mf.sharedMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
            float unscaledDist = Vector3.Dot(b.extents, localAxis) + Vector3.Dot(b.center, localAxis);
            
            if (unscaledDist < 0.0001f)
            {
                unscaledDist = 0.0001f;
            }

            float newScaleVal = (projectedDist - HandleOffset) / unscaledDist;
            if (newScaleVal < 0.01f)
            {
                newScaleVal = 0.01f;
            }

            Vector3 s = ScaleLogic.scale;
            if (localAxis == Vector3.right)
            {
                s.x = newScaleVal;
            }
            else if (localAxis == Vector3.up)
            {
                s.y = newScaleVal;
            }
            else if (localAxis == Vector3.forward)
            {
                s.z = newScaleVal;
            }

            ScaleLogic.SetScale(s);
            handle.transform.position = targetT.position + (axisWorld * projectedDist);
            handle.transform.rotation = targetT.rotation;
        }
        else
        {
            handle.transform.position = CalculateTargetPosition(localAxis);
            handle.transform.rotation = ScaleLogic.editObject.transform.rotation;
        }

        if (stem != null)
        {
            Transform targetT = ScaleLogic.editObject.transform;
            Vector3 axisWorld = targetT.rotation * localAxis;
            stem.widthMultiplier = 1.0f;
            stem.startWidth = StemWidth;
            stem.endWidth = StemWidth;
            Vector3 stemStart = handle.transform.position - (axisWorld * HandleOffset);

            stem.SetPosition(0, stemStart);
            stem.SetPosition(1, handle.transform.position);
        }
    }
}    