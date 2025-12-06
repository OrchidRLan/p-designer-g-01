using System.Collections.Generic;
using UnityEngine;

public class ScaleCompensation : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<CompensatedObject> objectsToCompensate = new List<CompensatedObject>();
    [SerializeField] private float initialPlayerScale = 1f;
    
    private float currentPlayerScale = 1f;
    
    [System.Serializable]
    public class CompensatedObject
    {
        public Transform target;
        public ObjectType type = ObjectType.Transform3D;
        public Vector3 baseScale = Vector3.one;
        public float baseCanvasScale = 1f;
        
        [HideInInspector] public Canvas canvas;
        [HideInInspector] public LineRenderer lineRenderer;
        [HideInInspector] public bool isInitialized = false;
    }
    
    public enum ObjectType
    {
        Transform3D,
        Canvas,
        LineRenderer
    }
    
    private void Start()
    {
        if (playerTransform == null) return;
        
        InitializeObjects();
        initialPlayerScale = GetAverageScale(playerTransform.lossyScale);
        currentPlayerScale = initialPlayerScale;
    }
    
    private void InitializeObjects()
    {
        foreach (var obj in objectsToCompensate)
        {
            if (obj.target == null) continue;
            
            if (obj.baseScale == Vector3.one)
            {
                obj.baseScale = obj.target.localScale;
            }
            
            if (obj.type == ObjectType.Canvas)
            {
                obj.canvas = obj.target.GetComponent<Canvas>();
                if (obj.canvas != null && obj.baseCanvasScale == 1f)
                {
                    obj.baseCanvasScale = obj.canvas.scaleFactor;
                }
            }
            else if (obj.type == ObjectType.LineRenderer)
            {
                obj.lineRenderer = obj.target.GetComponent<LineRenderer>();
            }
            
            obj.isInitialized = true;
        }
    }
    
    private void Update()
    {
        if (playerTransform == null) return;
        
        float newPlayerScale = GetAverageScale(playerTransform.lossyScale);
        
        if (Mathf.Abs(newPlayerScale - currentPlayerScale) > 0.0001f)
        {
            currentPlayerScale = newPlayerScale;
            UpdateCompensation();
        }
    }
    
    private void UpdateCompensation()
    {
        float compensationFactor = currentPlayerScale / initialPlayerScale;
        
        foreach (var obj in objectsToCompensate)
        {
            if (obj.target == null || !obj.isInitialized) continue;
            
            ApplyCompensation(obj, compensationFactor);
        }
    }
    
    private void ApplyCompensation(CompensatedObject obj, float factor)
    {
        switch (obj.type)
        {
            case ObjectType.Transform3D:
                obj.target.localScale = obj.baseScale * factor;
                break;
                
            case ObjectType.Canvas:
                if (obj.canvas != null)
                {
                    obj.canvas.scaleFactor = obj.baseCanvasScale * factor;
                }
                break;
                
            case ObjectType.LineRenderer:
                if (obj.lineRenderer != null)
                {
                    obj.lineRenderer.widthMultiplier = obj.baseScale.x * factor;
                }
                break;
        }
    }
    
    private float GetAverageScale(Vector3 scale)
    {
        return (scale.x + scale.y + scale.z) / 3f;
    }
    
    public void AddCompensatedObject(Transform target, ObjectType type, Vector3 baseScale)
    {
        var obj = new CompensatedObject
        {
            target = target,
            type = type,
            baseScale = baseScale
        };
        
        objectsToCompensate.Add(obj);
        
        if (obj.type == ObjectType.Canvas)
        {
            obj.canvas = target.GetComponent<Canvas>();
            if (obj.canvas != null)
            {
                obj.baseCanvasScale = obj.canvas.scaleFactor;
            }
        }
        else if (obj.type == ObjectType.LineRenderer)
        {
            obj.lineRenderer = target.GetComponent<LineRenderer>();
        }
        
        obj.isInitialized = true;
        
        float compensationFactor = currentPlayerScale / initialPlayerScale;
        ApplyCompensation(obj, compensationFactor);
    }
    
    public void RemoveCompensatedObject(Transform target)
    {
        objectsToCompensate.RemoveAll(obj => obj.target == target);
    }
    
    public float GetCompensationFactor()
    {
        return currentPlayerScale / initialPlayerScale;
    }
}