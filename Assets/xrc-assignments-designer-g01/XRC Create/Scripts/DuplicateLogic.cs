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
    
    [Header("UI")]
    public GameObject deleteIcon;
    private void Awake()
    {
        if (!interactionManager) interactionManager = FindObjectOfType<XRInteractionManager>();
        if (deleteIcon != null)
        {
            deleteIcon.SetActive(false);
        }
    }
    private void Update()
    {
        bool hasSelection = false;
        foreach (var interactor in selectionInteractors)
        {
            if (interactor == null)
            {
                continue;
            }

            if (interactor.interactablesSelected.Count > 0)
            {
                hasSelection = true;
                break;
            }
        }
        if (deleteIcon.activeSelf != hasSelection)
        {
            deleteIcon.SetActive(hasSelection);
        }
    }

    private void OnEnable()
    {
        if (input) {
            input.OnDuplicate += TryDuplicate;
            input.OnDelete    += TryDelete;   
        }
    }
    private void OnDisable()
    {
        if (input) {
            input.OnDuplicate -= TryDuplicate;
            input.OnDelete    -= TryDelete;   
        }
    }
    private void TryDuplicate()
    {
        var originals = GatherSelected();
        if (originals.Count == 0) return; 

        StartCoroutine(DuplicateRoutine(originals));
    }
    private void TryDelete()
    {
        var targets = GatherSelected();
        if (targets.Count == 0) return;

        foreach (var it in selectionInteractors.Where(i => i != null))
        {
            var ctrl = it as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
            
            if (ctrl != null && ctrl.xrController != null)
                ctrl.xrController.SendHapticImpulse(0.6f, 0.06f);
        }

        foreach (var sel in targets)
        {
            foreach (var inter in sel.interactorsSelecting.ToList())
                interactionManager.SelectExit(inter, sel);

            var go = (sel as Component)?.gameObject;
            if (go) UnityEngine.Object.Destroy(go);
        }
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
        var disabledRBs = new List<Rigidbody>();

        foreach (var o in originals)
        {
            var t = (o as Component).transform;
            var parent = keepParent ? t.parent : null;

            var cloneGO = Object.Instantiate(t.gameObject, t.position, t.rotation, parent);
            cloneGO.transform.localScale = t.localScale;

            if (nameWithCopySuffix)
            {
                cloneGO.name = t.gameObject.name + "_copy";
            }
            

            if (!cloneGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>())
            {
                cloneGO.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            }
            foreach (var rb in cloneGO.GetComponentsInChildren<Rigidbody>(true))
            {
                if (rb && rb.detectCollisions)
                {
                    rb.detectCollisions = false;
                    disabledRBs.Add(rb);
                }
            }
        }

        yield return null;
        foreach (var rb in disabledRBs)
        {
            if (rb) rb.detectCollisions = true;
        }
    }
}
