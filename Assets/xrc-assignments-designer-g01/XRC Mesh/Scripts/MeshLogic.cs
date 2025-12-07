using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MeshLogic : MonoBehaviour
{
    [SerializeField] private MeshWidget meshWidget;
    [SerializeField] private MeshFeedback feedback;
    [SerializeField] private GameObject handlePrefab;
    [SerializeField] private GameObject conflictingTool;

    [SerializeField] private float vertexGroupEpsilon = 0.0001f;
    [SerializeField] private float handleMoveThreshold = 0.0001f;

    public bool IsInEditMode => _isInEditMode;
    public bool HasTarget => meshWidget != null;

    private bool _isInEditMode;

    private MeshFilter _meshFilter;
    private Mesh _meshInstance;
    private Vector3[] _vertices;

    private Collider[] _colliders;
    private XRGrabInteractable _grabInteractable;
    private Transform _targetTransform;
    private bool _wasGrabEnabled;

    private struct HandleData
    {
        public Transform handleTransform;
        public int[] vertexIndices;
        public Vector3 lastWorldPosition;
    }

    private readonly List<HandleData> _handles = new List<HandleData>();

    public void OnEditObjectChanging(GameObject newObject)
    {
        if (meshWidget != null && newObject == meshWidget.gameObject)
        {
            return;
        }

        if (_isInEditMode)
        {
            EndEditMode();
        }

        if (newObject == null)
        {
            meshWidget = null;
            return;
        }

        var widget = newObject.GetComponent<MeshWidget>();
        if (widget != null)
        {
            meshWidget = widget;
        }
    }

    //------------FR2----------------------
    private void Update()
    {
        if (!_isInEditMode || _meshInstance == null) return;
    
        UpdateHandlesAndMesh();
    }
    private void UpdateHandlesAndMesh()
    {
        bool meshChanged = false;
    
        for (int i = 0; i < _handles.Count; i++)
        {
            HandleData handle = _handles[i];
        
            if (handle.handleTransform == null) continue;
        
            Vector3 currentWorldPos = handle.handleTransform.position;
            
            if (Vector3.Distance(currentWorldPos, handle.lastWorldPosition) > handleMoveThreshold)
            {
                Vector3 newLocalPos = _targetTransform.InverseTransformPoint(currentWorldPos);
            
        
                foreach (int vertexIndex in handle.vertexIndices)
                {
                    _vertices[vertexIndex] = newLocalPos;
                }
                
                handle.lastWorldPosition = currentWorldPos;
                _handles[i] = handle;
            
                meshChanged = true;
            }
        }
        
        if (meshChanged)
        {
            _meshInstance.vertices = _vertices;
            _meshInstance.RecalculateNormals();
            _meshInstance.RecalculateBounds();
            
            UpdateColliders();
        }
    }
    
    private void UpdateColliders()
    {
        if (_colliders == null) return;
    
        foreach (var col in _colliders)
        {
            if (col is MeshCollider meshCol)
            {
                meshCol.sharedMesh = null;
                meshCol.sharedMesh = _meshInstance;
            }
        }
    }
//----------------------
    public void BeginEditMode()
    {
        Debug.Log("[XRC Mesh] BeginEditMode called");
        if (_isInEditMode)
            return;

        if (meshWidget == null)
        {
            feedback?.ShowNoTargetWarning();
            return;
        }

        GameObject target = meshWidget.gameObject;

        //only allow objects with a MeshFilter and mesh
        _meshFilter = target.GetComponent<MeshFilter>();
        if (_meshFilter == null || _meshFilter.sharedMesh == null)
        {
            feedback?.ShowInvalidTargetWarning();
            return;
        }

        _targetTransform = _meshFilter.transform;

        //work on an instance so we don't wreck the shared mesh asset
        _meshInstance = Instantiate(_meshFilter.sharedMesh);
        _meshFilter.sharedMesh = _meshInstance;

        _vertices = _meshInstance.vertices;

        _colliders = target.GetComponents<Collider>();
        _grabInteractable = target.GetComponent<XRGrabInteractable>();

        //object shouldn't be grabbable while editing
        if (_grabInteractable != null)
        {
            _wasGrabEnabled = _grabInteractable.enabled;
            _grabInteractable.enabled = false;
        }

        if (_colliders != null)
        {
            foreach (var col in _colliders)
            {
                col.enabled = false;
            }
        }

        if (conflictingTool != null)
        {
            conflictingTool.SetActive(false);
        }

        CreateHandles();

        _isInEditMode = true;
    }

    public void EndEditMode()
    {
        if (!_isInEditMode)
            return;

        DestroyHandles();

        if (_grabInteractable != null)
            _grabInteractable.enabled = _wasGrabEnabled;

        if (_colliders != null)
        {
            foreach (var col in _colliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        if (conflictingTool != null)
        {
            conflictingTool.SetActive(true);
        }

        _isInEditMode = false;
    }

    private void CreateHandles()
    {
        DestroyHandles();

        if (handlePrefab == null || _meshInstance == null)
        {
            feedback?.ShowEditNotAllowedWarning();
            return;
        }

        var verts = _meshInstance.vertices;
        var groups = new List<List<int>>();

        //simple vertex grouping, if positions are super close, same group
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 v = verts[i];
            bool added = false;

            for (int g = 0; g < groups.Count && !added; g++)
            {
                int firstIndex = groups[g][0];
                if (Vector3.Distance(verts[firstIndex], v) <= vertexGroupEpsilon)
                {
                    groups[g].Add(i);
                    added = true;
                }
            }

            if (!added)
                groups.Add(new List<int> { i });
        }

        foreach (var group in groups)
        {
            int idx = group[0];
            Vector3 localPos = verts[idx];
            Vector3 worldPos = _targetTransform.TransformPoint(localPos);

            GameObject handle = Instantiate(handlePrefab, worldPos, Quaternion.identity);
            //-----FR2-------
            var grabInteractable = handle.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = true;
            }
            //------
            var data = new HandleData
            {
                handleTransform = handle.transform,
                vertexIndices = group.ToArray(),
                lastWorldPosition = worldPos
            };

            _handles.Add(data);
        }
    }

    // private void DestroyHandles()
    // {
    //     for (int i = 0; i < _handles.Count; i++)
    //     {
    //         if (_handles[i].handleTransform != null)
    private void DestroyHandles()
    {
        for (int i = 0; i < _handles.Count; i++)
        {
            if (_handles[i].handleTransform)
                Destroy(_handles[i].handleTransform.gameObject);
        }

        _handles.Clear();
    }

    public bool IsGrabbing
    {
        get
        {
            if (meshWidget == null)
            {
                return false;
            }
            var interactable = meshWidget.GetComponent<XRGrabInteractable>();
            if (interactable != null)
            {
                return interactable.isSelected;
            }
            return false;
        }
    }
}