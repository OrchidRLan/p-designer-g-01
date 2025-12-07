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

        [Header("Scale Compensation")]
        [Tooltip("ScaleCompensation component to register created objects with (optional)")]
        [SerializeField]
        private ScaleCompensation m_ScaleCompensation;

        [Tooltip("Auto-find ScaleCompensation if not assigned")]
        [SerializeField]
        private bool m_AutoFindScaleCompensation = true;

        [Tooltip("Register created objects with ScaleCompensation so they scale with player")]
        [SerializeField]
        private bool m_RegisterCreatedObjects = true;

        private GameObject m_PreviewObject;
        private bool m_IsPreviewActive;
        private bool m_UseUniformScaling = true;
        private int m_GrabCount = 0;
        private XRInteractionManager m_InteractionManager;
        private HashSet<GameObject> m_RegisteredObjects = new HashSet<GameObject>();

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

            // Auto-find ScaleCompensation if needed
            if (m_AutoFindScaleCompensation && m_ScaleCompensation == null)
            {
                m_ScaleCompensation = FindObjectOfType<ScaleCompensation>();
                
                if (m_ScaleCompensation != null)
                {
                    Debug.Log("[CreateLogic] Found ScaleCompensation component");
                }
                else if (m_RegisterCreatedObjects)
                {
                    Debug.LogWarning("[CreateLogic] ScaleCompensation not found - created objects will not auto-scale with player");
                }
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

            if (finalObject.GetComponent<MeshWidget>() == null)
            {
                finalObject.AddComponent<MeshWidget>();
            }

            var rb = finalObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            // Register with ScaleCompensation
            RegisterObjectWithScaleCompensation(finalObject);

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

        /// <summary>
        /// Registers a created object with ScaleCompensation so it scales with the player.
        /// </summary>
        private void RegisterObjectWithScaleCompensation(GameObject obj)
        {
            if (!m_RegisterCreatedObjects || m_ScaleCompensation == null || obj == null)
                return;

            // Check if already registered
            if (m_RegisteredObjects.Contains(obj))
                return;

            // Save base scale
            Vector3 baseScale = obj.transform.localScale;

            // Register with ScaleCompensation as a 3D transform
            m_ScaleCompensation.AddCompensatedObject(
                obj.transform,
                ScaleCompensation.ObjectType.Transform3D,
                baseScale
            );

            m_RegisteredObjects.Add(obj);
            
            Debug.Log($"[CreateLogic] Registered {obj.name} with ScaleCompensation (base scale: {baseScale})");
        }

        /// <summary>
        /// Unregisters an object from ScaleCompensation.
        /// </summary>
        private void UnregisterObjectFromScaleCompensation(GameObject obj)
        {
            if (m_ScaleCompensation == null || obj == null)
                return;

            if (m_RegisteredObjects.Contains(obj))
            {
                m_ScaleCompensation.RemoveCompensatedObject(obj.transform);
                m_RegisteredObjects.Remove(obj);
                Debug.Log($"[CreateLogic] Unregistered {obj.name} from ScaleCompensation");
            }
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
            var disabledRBs = new List<Rigidbody>();

            // CRITICAL FIX: Save positions BEFORE releasing the objects
            // This ensures we get the current position where the user is holding the object
            var savedTransforms = new List<(Vector3 position, Quaternion rotation, Vector3 scale, Transform parent)>();
            foreach (var o in originals)
            {
                var t = (o as Component).transform;
                var parent = m_KeepParent ? t.parent : null;
                
                // Save the current transform state while object is still grabbed
                savedTransforms.Add((t.position, t.rotation, t.localScale, parent));
            }

            for (int i = 0; i < originals.Count; i++)
            {
                var saved = savedTransforms[i];
                var original = originals[i];
                var t = (original as Component).transform;

                // Use saved position and rotation
                var cloneGO = Instantiate(t.gameObject, saved.position, saved.rotation, saved.parent);
                cloneGO.transform.localScale = saved.scale;

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

                if (cloneGO.GetComponent<MeshWidget>() == null)
                {
                    cloneGO.AddComponent<MeshWidget>();
                }
                // Register duplicated object with ScaleCompensation
                RegisterObjectWithScaleCompensation(cloneGO);

                var allColliders = cloneGO.GetComponentsInChildren<Collider>();
                foreach (var col in allColliders)
                {
                    col.enabled = false;
                }
                grabInteractable.enabled = false;
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
                {
                    // Unregister from ScaleCompensation before destroying
                    UnregisterObjectFromScaleCompensation(go);
                    Destroy(go);
                }
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

        // ==================== PUBLIC SCALE COMPENSATION METHODS ====================

        /// <summary>
        /// Manually set the ScaleCompensation component.
        /// </summary>
        public void SetScaleCompensation(ScaleCompensation scaleComp)
        {
            m_ScaleCompensation = scaleComp;
            Debug.Log($"[CreateLogic] ScaleCompensation manually set");
        }

        /// <summary>
        /// Get the current compensation factor from ScaleCompensation.
        /// </summary>
        public float GetCompensationFactor()
        {
            if (m_ScaleCompensation != null)
            {
                return m_ScaleCompensation.GetCompensationFactor();
            }
            return 1f;
        }

        /// <summary>
        /// Enable or disable registration of created objects with ScaleCompensation.
        /// </summary>
        public void SetRegisterCreatedObjects(bool register)
        {
            m_RegisterCreatedObjects = register;
        }
    }
}