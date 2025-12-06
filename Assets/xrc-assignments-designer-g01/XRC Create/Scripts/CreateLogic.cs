using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace XRC.Toolkit.Create
{
    public class CreateLogic : MonoBehaviour
    {
        [Header("Creation Settings")]
        [SerializeField]
        private PrimitiveType m_PrimitiveType = PrimitiveType.Cube;

        [SerializeField]
        private float m_MinScale = 0.05f;

        [SerializeField]
        private float m_MaxScale = 2f;

        [Header("Materials")]
        [SerializeField]
        private Material m_ObjectMaterial;

        [SerializeField]
        private Material m_PreviewMaterial;

        [Header("Duplicate/Delete Settings")]
        [SerializeField]
        private Transform m_Origin;

        [SerializeField]
        private XRBaseInteractor m_Interactor;

        [SerializeField]
        private Transform m_LeftTransform;

        [SerializeField]
        private Transform m_RightTransform;

        [SerializeField]
        private bool m_KeepParent = true;

        [SerializeField]
        private bool m_NameWithCopySuffix = true;

        private GameObject m_PreviewObject;
        private bool m_IsPreviewActive;
        private bool m_UseUniformScaling = true;
        private XRInteractionManager m_InteractionManager;

        public PrimitiveType primitiveType
        {
            get => m_PrimitiveType;
            set
            {
                if (m_PrimitiveType != value && m_PreviewObject != null)
                {
                    Destroy(m_PreviewObject);
                    m_PreviewObject = null;
                }
                m_PrimitiveType = value;
            }
        }

        public Material objectMaterial
        {
            get => m_ObjectMaterial;
            set => m_ObjectMaterial = value;
        }

        public Material previewMaterial
        {
            get => m_PreviewMaterial;
            set => m_PreviewMaterial = value;
        }

        public bool useUniformScaling
        {
            get => m_UseUniformScaling;
            set => m_UseUniformScaling = value;
        }

        public bool isPreviewActive => m_IsPreviewActive;
        public GameObject previewObject => m_PreviewObject;

        private void Awake()
        {
            if (m_InteractionManager == null)
            {
                m_InteractionManager = FindObjectOfType<XRInteractionManager>();
            }
        }

        public void UpdatePreviewTransform(Vector3 leftPos, Vector3 rightPos, Transform origin)
        {
            EnsurePreviewObjectExists();

            if (!m_IsPreviewActive)
            {
                m_IsPreviewActive = true;
                m_PreviewObject.SetActive(true);
            }

            var center = (leftPos + rightPos) * 0.5f;
            m_PreviewObject.transform.position = center;

            var direction = rightPos - leftPos;
            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                var up = origin != null ? origin.up : Vector3.up;
                m_PreviewObject.transform.rotation =
                    Quaternion.LookRotation(direction.normalized, up);
            }

            if (m_UseUniformScaling)
                ApplyUniformScale(leftPos, rightPos);
            else
                ApplyNonUniformScale(leftPos, rightPos);
        }

        public void ApplyUniformScale(Vector3 leftPos, Vector3 rightPos)
        {
            var delta = rightPos - leftPos;
            var absX = Mathf.Abs(delta.x);
            var absY = Mathf.Abs(delta.y);
            var absZ = Mathf.Abs(delta.z);
            var maxAxis = Mathf.Max(absX, Mathf.Max(absY, absZ));
            var clamped = Mathf.Clamp(maxAxis, m_MinScale, m_MaxScale);

            m_PreviewObject.transform.localScale = new Vector3(clamped, clamped, clamped);
        }

        public void ApplyNonUniformScale(Vector3 leftPos, Vector3 rightPos)
        {
            var delta = rightPos - leftPos;

            var x = Mathf.Clamp(Mathf.Abs(delta.x), m_MinScale, m_MaxScale);
            var y = Mathf.Clamp(Mathf.Abs(delta.y), m_MinScale, m_MaxScale);
            var z = Mathf.Clamp(Mathf.Abs(delta.z), m_MinScale, m_MaxScale);

            m_PreviewObject.transform.localScale = new Vector3(x, y, z);
        }

        public GameObject FinalizeCreateFromPreview()
        {
            if (!m_IsPreviewActive || m_PreviewObject == null)
                return null;

            var finalObject = Instantiate(
                m_PreviewObject,
                m_PreviewObject.transform.position,
                m_PreviewObject.transform.rotation);

            finalObject.transform.localScale = m_PreviewObject.transform.localScale;

            if (m_ObjectMaterial != null)
            {
                var renderer = finalObject.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = m_ObjectMaterial;
            }

            // Add and configure XRGrabInteractable
            var grabInteractable = finalObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = finalObject.AddComponent<XRGrabInteractable>();
            }

            // Configure grab behavior to use grab point instead of object center
            // This prevents large objects from blocking the controller
            grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.throwOnDetach = false;
            
            // KEY FIX: Use dynamic attach - grabs at the point where hand touches object
            grabInteractable.useDynamicAttach = true;
            grabInteractable.matchAttachPosition = true;
            grabInteractable.matchAttachRotation = true;
            grabInteractable.snapToColliderVolume = false;
            
            // Smooth grab animation
            grabInteractable.attachEaseInTime = 0.1f;

            var rb = finalObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            m_PreviewObject.SetActive(false);
            m_IsPreviewActive = false;

            Debug.Log($"[CreateLogic] Created {m_PrimitiveType} with dynamic attach enabled");

            return finalObject;
        }

        public void CancelPreview()
        {
            if (!m_IsPreviewActive || m_PreviewObject == null)
                return;

            m_PreviewObject.SetActive(false);
            m_IsPreviewActive = false;
        }

        private void EnsurePreviewObjectExists()
        {
            if (m_PreviewObject != null)
                return;

            m_PreviewObject = GameObject.CreatePrimitive(m_PrimitiveType);
            m_PreviewObject.name = "XRC Create Preview";

            if (m_PreviewMaterial != null)
            {
                var renderer = m_PreviewObject.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material = m_PreviewMaterial;
            }

            m_PreviewObject.SetActive(false);
        }

        public void DuplicateSelected()
        {
            var originals = GatherSelected();
            if (originals.Count == 0)
                return;

            StartCoroutine(DuplicateRoutine(originals));
        }

        private IEnumerator DuplicateRoutine(List<IXRSelectInteractable> originals)
        {
            var clones = new List<IXRSelectInteractable>();
            var disabledRBs = new List<Rigidbody>();

            foreach (var o in originals)
            {
                foreach (var interactor in o.interactorsSelecting.ToList())
                {
                    if (m_InteractionManager != null)
                        m_InteractionManager.SelectExit(interactor, o);
                }
            }

            foreach (var o in originals)
            {
                var t = (o as Component).transform;
                var parent = m_KeepParent ? t.parent : null;

                var cloneGO = Instantiate(t.gameObject, t.position, t.rotation, parent);
                cloneGO.transform.localScale = t.localScale;

                if (m_NameWithCopySuffix)
                    cloneGO.name = t.gameObject.name + "_copy";

                // Configure duplicated object with same grab settings
                var grabInteractable = cloneGO.GetComponent<XRGrabInteractable>();
                if (grabInteractable == null)
                {
                    grabInteractable = cloneGO.AddComponent<XRGrabInteractable>();
                }
                
                // Apply same settings as newly created objects
                grabInteractable.useDynamicAttach = true;
                grabInteractable.matchAttachPosition = true;
                grabInteractable.matchAttachRotation = true;
                grabInteractable.snapToColliderVolume = false;

                foreach (var rb in cloneGO.GetComponentsInChildren<Rigidbody>(true))
                {
                    if (rb && rb.detectCollisions)
                    {
                        rb.detectCollisions = false;
                        disabledRBs.Add(rb);
                    }
                }

                var interactable = cloneGO.GetComponent<IXRSelectInteractable>();
                if (interactable != null)
                    clones.Add(interactable);
            }

            yield return null;

            foreach (var rb in disabledRBs)
            {
                if (rb) rb.detectCollisions = true;
            }

            if (m_Interactor != null && m_InteractionManager != null)
            {
                foreach (var c in clones)
                    m_InteractionManager.SelectEnter(m_Interactor, c);
            }
        }

        public void DeleteSelected()
        {
            var targets = GatherSelected();
            if (targets.Count == 0)
                return;

            SendDeleteHaptics();

            foreach (var sel in targets)
            {
                foreach (var inter in sel.interactorsSelecting.ToList())
                {
                    if (m_InteractionManager != null)
                        m_InteractionManager.SelectExit(inter, sel);
                }

                var go = (sel as Component)?.gameObject;
                if (go)
                    Destroy(go);
            }
        }

        private void SendDeleteHaptics()
        {
            if (m_LeftTransform != null)
            {
                var controller = m_LeftTransform.GetComponent<XRBaseController>();
                if (controller != null)
                    controller.SendHapticImpulse(0.6f, 0.06f);
            }

            if (m_RightTransform != null)
            {
                var controller = m_RightTransform.GetComponent<XRBaseController>();
                if (controller != null)
                    controller.SendHapticImpulse(0.6f, 0.06f);
            }
        }

        private List<IXRSelectInteractable> GatherSelected()
        {
            var set = new HashSet<IXRSelectInteractable>();

            if (m_Interactor != null)
            {
                foreach (var sel in m_Interactor.interactablesSelected)
                    set.Add(sel);
            }

            return set.ToList();
        }
    }
}