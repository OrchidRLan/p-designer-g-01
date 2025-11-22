

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
public class SphereSelectLogic : MonoBehaviour
{
    [Header("Hand + Radius")]
    public XRDirectInteractor interactor;
    public Transform defaultCenter;
    public float minRadius = 0.01f;
    public float maxRadius = 0.30f;
    public float defaultRadius = 0.04f;

    [Header("Grab Input (same key as Select/Grip)")]
    public InputActionProperty grabAction;

    // ---- runtime state ----
    float currentRadius;
    Vector3 currentCenter;

    // For Feedback
    readonly List<Collider> currentHits = new();

    readonly HashSet<IXRSelectInteractable> selectCurr = new();
    
    Transform groupAnchor;
    readonly List<Transform> grabbed = new();
    readonly List<Transform> originalParents = new();
    readonly List<Rigidbody> rbs = new();
    readonly List<bool> rbWasKinematic = new();
    
    
  
    // change objects into yellow
    [SerializeField] Color tintColor = Color.yellow;

    MaterialPropertyBlock _tintMPB;
    readonly HashSet<Renderer> _tinted = new();   
    readonly List<Renderer>   _frame  = new();    
    static readonly List<Material> _matsBuf = new();  
    static readonly int _EmissionColorID = Shader.PropertyToID("_EmissionColor");
    
    void Start()
    {
        currentRadius = defaultRadius;
        currentCenter = defaultCenter.position;
        
    }
    
    void SetTintLikeXRI(Renderer rend, bool on)
    {
        if (!rend) return;

        
        rend.GetMaterials(_matsBuf);                
        for (int i = 0; i < _matsBuf.Count; i++)
        {
            if (on) _matsBuf[i].EnableKeyword("_EMISSION");
            else    _matsBuf[i].DisableKeyword("_EMISSION");
        }
        _matsBuf.Clear();
        
        if (_tintMPB == null) _tintMPB = new MaterialPropertyBlock();
        rend.GetPropertyBlock(_tintMPB);
        var emission = on ? tintColor * Mathf.LinearToGammaSpace(1f) : Color.black;
        _tintMPB.SetColor(_EmissionColorID, emission);
        rend.SetPropertyBlock(_tintMPB);
    }


    
    bool IsTransformRelated(Transform a, Transform b)
        => a == b || (a != null && b != null && (a.IsChildOf(b) || b.IsChildOf(a)));

    bool IsGrabbedRenderer(Renderer r)
    {
        if (!r) return false;
        foreach (var g in grabbed)
            if (g && IsTransformRelated(r.transform, g))
                return true;
        return false;
    }

    bool IsXRSelected(Renderer r)
    {
        var sel = r ? r.GetComponentInParent<IXRSelectInteractable>() : null;
        return sel != null && sel.isSelected;
    }
    void SyncTintLikeXRI()
    {
        _frame.Clear();

       
        foreach (var c in currentHits)
        {
            var rends = c.GetComponentsInParent<Renderer>(true); 
            foreach (var r in rends)
            {
                if (!r) continue;
                _frame.Add(r);
                if (_tinted.Add(r))
                    SetTintLikeXRI(r, true);
            }
        }
        
        foreach (var r in _tinted.ToArray())
        {
            bool keep = _frame.Contains(r) || IsGrabbedRenderer(r) || IsXRSelected(r);
            if (keep) continue;

            SetTintLikeXRI(r, false);
            _tinted.Remove(r);
        }
    }

    


    void OnEnable()
    {
        if (grabAction.reference != null)
        {
            grabAction.action.performed += OnGrabPerformed;
            grabAction.action.canceled  += OnGrabCanceled;
            grabAction.action.Enable();
        }
    }
    void OnDisable()
    {
        if (grabAction.reference != null)
        {
            grabAction.action.performed -= OnGrabPerformed;
            grabAction.action.canceled  -= OnGrabCanceled;
            grabAction.action.Disable();
        }
    }

    // ====== Public API for other scripts (Input/Feedback) ======
    public float  GetRadius()  => currentRadius;
    public Vector3 GetCenter() => currentCenter;
    public List<Collider> GetCurrentHits() => currentHits;
    public bool HasAnyHit() => currentHits.Count > 0;
    public bool IsSelecting() =>
        interactor != null && (interactor.hasSelection || grabbed.Count > 0);

    public void SetRadius(float r)
    {
        currentRadius = Mathf.Clamp(r, minRadius, maxRadius);
    }

    void Update()
    {
        currentRadius = Mathf.Clamp(currentRadius, minRadius, maxRadius);
        
        if (defaultCenter != null)
        {
            Vector3 dir = interactor.transform.right;             
            dir = dir.normalized;

            if (currentRadius < defaultRadius)
            {
              
                Vector3 pFixed = defaultCenter.position - dir * defaultRadius;

              
                currentCenter = pFixed + dir * currentRadius;
            }
            else
            {
              
                currentCenter = defaultCenter.position;
            }
        }

        UpdateOverlapHits();
        SyncTintLikeXRI(); 
 
    }

    void UpdateOverlapHits()
    {
        currentHits.Clear();
        selectCurr.Clear();

        var hits = Physics.OverlapSphere(
            currentCenter, currentRadius, ~0, QueryTriggerInteraction.Ignore);

        foreach (var c in hits)
        {
            if (interactor != null && c.transform.IsChildOf(interactor.transform))
                continue;

            currentHits.Add(c);

           
            var sel = c.GetComponentInParent<IXRSelectInteractable>();
            if (sel == null)
            {
                var grab = c.GetComponentInParent<XRGrabInteractable>();
                if (grab != null) sel = grab;
            }
            if (sel != null) selectCurr.Add(sel);
        }
    }

    // =================== Grab (press / release) ===================
    void OnGrabPerformed(InputAction.CallbackContext _)
    {
        UpdateOverlapHits();
        BeginGroupGrab();
    }

    void OnGrabCanceled(InputAction.CallbackContext _)
    {
       
        EndGroupGrab();
    }
    
    void TryBatchSelectEnter()
    {
        if (interactor?.interactionManager == null) return;
        var selInteractor = interactor as IXRSelectInteractor;
        foreach (var it in selectCurr)
            interactor.interactionManager.SelectEnter(selInteractor, it);
    }
    void TryBatchSelectExit()
    {
        if (interactor?.interactionManager == null) return;
        var selInteractor = interactor as IXRSelectInteractor;
        foreach (var it in selectCurr.ToList())
            interactor.interactionManager.SelectExit(selInteractor, it);
    }

    void BeginGroupGrab()
    {
        if (groupAnchor == null)
        {
            groupAnchor = new GameObject("[SphereGroupAnchor]").transform;
            groupAnchor.SetParent(interactor.transform, false);
        }

        grabbed.Clear();
        originalParents.Clear();
        rbs.Clear();
        rbWasKinematic.Clear();

        foreach (var c in currentHits)
        {
           
            Transform t = c.attachedRigidbody ? c.attachedRigidbody.transform : c.transform;
            if (grabbed.Contains(t)) continue; 

            originalParents.Add(t.parent);
            grabbed.Add(t);

            var rb = t.GetComponent<Rigidbody>();
            if (rb)
            {
                rbs.Add(rb);
                rbWasKinematic.Add(rb.isKinematic);
                rb.isKinematic = true; 
            }
            else
            {
                rbs.Add(null);
                rbWasKinematic.Add(false);
            }

            t.SetParent(groupAnchor, true); 
            
            foreach (var rend in t.GetComponentsInChildren<Renderer>(true)) {
                SetTintLikeXRI(rend, true);
                _tinted.Add(rend);
            }
        }
    }

    void EndGroupGrab()
    {
        
        for (int i = 0; i < grabbed.Count; i++)
        {
            var t = grabbed[i];
            t.SetParent(originalParents[i], true);
            var rb = rbs[i];
            if (rb) rb.isKinematic = rbWasKinematic[i];

           
            foreach (var rend in t.GetComponentsInChildren<Renderer>(true))
            {
                
                bool stillInside = false;
                foreach (var h in currentHits)
                {
                    if (h && IsTransformRelated(rend.transform, h.transform)) { stillInside = true; break; }
                }

                if (!stillInside)
                {
                    SetTintLikeXRI(rend, false);
                    _tinted.Remove(rend);
                }
            }
        }

        grabbed.Clear();
        originalParents.Clear();
        rbs.Clear();
        rbWasKinematic.Clear();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        var center = Application.isPlaying ? currentCenter : (defaultCenter ? defaultCenter.position : transform.position);
        var radius = Application.isPlaying ? currentRadius : defaultRadius;
        Gizmos.DrawWireSphere(center, radius);
    }
#endif
}
