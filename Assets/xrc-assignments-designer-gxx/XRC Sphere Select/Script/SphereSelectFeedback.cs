using System.Collections.Generic;
using UnityEngine;

public class SphereSelectFeedback : MonoBehaviour
{
    // [Header("Refs")]
    public SphereSelectLogic logic;          
    public Material sphereMaterial;          

    // [Header("Colors")]
    public Color normalColor = new Color(0, 1, 1, 0.2f); 
    public Color hoverColor  = new Color(1, 0, 0, 0.3f); 

    private GameObject sphereVisual;
    private MeshRenderer sphereRenderer;

    void Start()
    {
  
        sphereVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        var col = sphereVisual.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
        
        sphereRenderer = sphereVisual.GetComponent<MeshRenderer>();
        sphereRenderer.material = sphereMaterial;
    }

    void Update()
    {
        if (logic.IsSelecting())
        {
            if (sphereVisual.activeSelf)
                sphereVisual.SetActive(false);
            return; 
        }
        else
        {
            if (!sphereVisual.activeSelf)
                sphereVisual.SetActive(true);
        }
        
        Vector3 center = logic.GetCenter();
        float radius   = logic.GetRadius();

        sphereVisual.transform.position = center;

        float diameter = radius * 2f;
        sphereVisual.transform.localScale = new Vector3(diameter, diameter, diameter);
        
        bool touchingSomething = logic.HasAnyHit();
        sphereRenderer.material.color = touchingSomething ? hoverColor : normalColor;
        
    }

    
    private void HighlightHitObjects(List<Collider> hits, bool active)
    {
        
    }
}
