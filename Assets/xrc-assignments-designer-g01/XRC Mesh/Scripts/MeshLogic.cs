using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

public class MeshLogic : MonoBehaviour
{
    [SerializeField] private MeshWidget meshWidget;
    [SerializeField] private MeshFeedback feedback;
    [SerializeField] private GameObject handlePrefab;

    [SerializeField] private float vertexGroupEpsilon = 0.0001f;
    [SerializeField] private float handleMoveThreshold = 0.0001f;

    public bool IsInEditMode => _isInEditMode;

    private bool _isInEditMode;

    private MeshFilter _meshFilter;
    private Mesh _meshInstance;
    private Vector3[] _vertices;

    private Collider[] _colliders;
    private XRGrabInteractable _grabInteractable;   
    private Transform _targetTransform;
    
    private struct HandleData
    {
        public Transform handleTransform;
        public int[] vertexIndices;
        public Vector3 lastWorldPosition;
    }

    private readonly List<HandleData> _handles = new List<HandleData>();

    private void Update()
    {
        if (!_isInEditMode || _meshInstance == null) return;

        UpdateHandlesAndMesh();
    }

    public void BeginEditMode()
    {
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
            _grabInteractable.enabled = false;

        CreateHandles();

        _isInEditMode = true;
    }

    public void EndEditMode()
    {
        if (!_isInEditMode)
            return;

        DestroyHandles();

        if (_grabInteractable != null)
            _grabInteractable.enabled = true;

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
            var data = new HandleData
            {
                handleTransform = handle.transform,
                vertexIndices = group.ToArray(),
                lastWorldPosition = worldPos
            };

            _handles.Add(data);
        }
    }

    private void DestroyHandles()
    {
        for (int i = 0; i < _handles.Count; i++)
        {
            if (_handles[i].handleTransform != null)
