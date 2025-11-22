using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DuplicateLogic : MonoBehaviour
{
    [Header("Refs")]
    public DuplicateInput input;                       
    public XRInteractionManager interactionManager;    
    public List<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor> selectionInteractors = new();

    [Header("Options")]
    public bool keepParent = true;         
    public bool nameWithCopySuffix = true; 

    private void Awake()
    {
        if (!interactionManager) interactionManager = FindObjectOfType<XRInteractionManager>();
    }

    private void OnEnable()
    {
        if (input) input.OnDuplicate += TryDuplicate;
    }
    private void OnDisable()
    {
        if (input) input.OnDuplicate -= TryDuplicate;
    }
    private void TryDuplicate()
    {
        var originals = GatherSelected();
        if (originals.Count == 0) return; 

        StartCoroutine(DuplicateRoutine(originals));
    }
    private List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable> GatherSelected()
    {
        var set = new HashSet<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable>();
        foreach (var it in selectionInteractors.Where(i => i != null))
        {
            foreach (var sel in it.interactablesSelected)
                set.Add(sel);
        }
        return set.ToList();
    }
    
    private IEnumerator DuplicateRoutine(List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable> originals)
    {
        var clones = new List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable>();
        var disabledRBs = new List<Rigidbody>();
        
        foreach (var o in originals)
        {
            foreach (var interactor in o.interactorsSelecting.ToList())
                interactionManager.SelectExit(interactor, o);
        }
        
        foreach (var o in originals)
        {
            var t = (o as Component).transform;
            var parent = keepParent ? t.parent : null;

            var cloneGO = Object.Instantiate(t.gameObject, t.position, t.rotation, parent);
            cloneGO.transform.localScale = t.localScale;
            if (nameWithCopySuffix) cloneGO.name = t.gameObject.name + "_copy";
            
            if (!cloneGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>())
                cloneGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            
            foreach (var rb in cloneGO.GetComponentsInChildren<Rigidbody>(true))
            {
                if (rb && rb.detectCollisions)
                {
                    rb.detectCollisions = false;
                    disabledRBs.Add(rb);
                }
            }

            clones.Add(cloneGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable>());
        }
        
        yield return null;
        foreach (var rb in disabledRBs) if (rb) rb.detectCollisions = true;
        
        var primaryInteractor = selectionInteractors.FirstOrDefault(i => i != null);
        if (primaryInteractor != null)
        {
            foreach (var c in clones)
                interactionManager.SelectEnter(primaryInteractor, c);
        }
    }
}
