using UnityEngine;
using UnityEngine.InputSystem;

namespace XRC.Toolkit.Create
{
    [RequireComponent(typeof(CreateLogic))]
    public class CreateInput : MonoBehaviour
    {
        [Header("input actions")]
        [SerializeField]
        private InputActionProperty m_CreateAction;

        [SerializeField]
        private InputActionProperty m_DisableUniformScalingAction;

        [SerializeField]
        private InputActionProperty m_MenuAction;   

        [Header("controller refs")]
        [SerializeField]
        private Transform m_LeftController;

        [SerializeField]
        private Transform m_RightController;

        [SerializeField]
        private Transform m_XROrigin;

        [Header("ui refs")]
        [SerializeField]
        private GameObject m_ShapeMenuRoot;

        private CreateLogic m_CreateLogic;
        private CreateFeedback m_CreateFeedback;
        private bool m_IsCreating;
        private bool m_IsMenuOpen;

        public InputActionProperty createAction
        {
            get => m_CreateAction;
            set => m_CreateAction = value;
        }

        public InputActionProperty disableUniformScalingAction
        {
            get => m_DisableUniformScalingAction;
            set => m_DisableUniformScalingAction = value;
        }

        public InputActionProperty menuAction
        {
            get => m_MenuAction;
            set => m_MenuAction = value;
        }

        private void Awake()
        {
            m_CreateLogic = GetComponent<CreateLogic>();
            m_CreateFeedback = GetComponent<CreateFeedback>();

            if (m_ShapeMenuRoot != null)
            {
                m_ShapeMenuRoot.SetActive(false);   
            }
        }

        private void OnEnable()
        {
            if (m_CreateAction.action != null)
            {
                m_CreateAction.action.Enable();
                m_CreateAction.action.started += OnCreateStarted;
                m_CreateAction.action.canceled += OnCreateCanceled;
            }

            if (m_DisableUniformScalingAction.action != null)
            {
                m_DisableUniformScalingAction.action.Enable();
                m_DisableUniformScalingAction.action.performed += OnDisableUniformScalingPerformed;
                m_DisableUniformScalingAction.action.canceled += OnDisableUniformScalingCanceled;
            }

            if (m_MenuAction.action != null)
            {
                m_MenuAction.action.Enable();
                m_MenuAction.action.performed += OnMenuPerformed;
            }
        }

        private void OnDisable()
        {
            if (m_CreateAction.action != null)
            {
                m_CreateAction.action.started -= OnCreateStarted;
                m_CreateAction.action.canceled -= OnCreateCanceled;
                m_CreateAction.action.Disable();
            }

            if (m_DisableUniformScalingAction.action != null)
            {
                m_DisableUniformScalingAction.action.performed -= OnDisableUniformScalingPerformed;
                m_DisableUniformScalingAction.action.canceled -= OnDisableUniformScalingCanceled;
                m_DisableUniformScalingAction.action.Disable();
            }

            if (m_MenuAction.action != null)
            {
                m_MenuAction.action.performed -= OnMenuPerformed;
                m_MenuAction.action.Disable();
            }
        }

        private void Update()
        {
            if (!m_IsCreating || m_IsMenuOpen)
            {
                return;
            }

            if (m_LeftController == null || m_RightController == null)
            {
                return;
            }

            m_CreateLogic.UpdatePreviewTransform(
                m_LeftController.position,
                m_RightController.position,
                m_XROrigin);
        }

        //create pressed
        private void OnCreateStarted(InputAction.CallbackContext ctx)
        {
            if (m_IsMenuOpen)
            {
                return;
            }

            m_IsCreating = true;

            //make preview appear right away
            if (m_LeftController != null && m_RightController != null)
            {
                m_CreateLogic.UpdatePreviewTransform(
                    m_LeftController.position,
                    m_RightController.position,
                    m_XROrigin);
            }

            //apply preview feedback 
            if (m_CreateFeedback != null)
            {
                m_CreateFeedback.OnPreviewEntered(m_CreateLogic.previewObject);
            }
        }

        //create released
        private void OnCreateCanceled(InputAction.CallbackContext ctx)
        {
            if (!m_IsCreating)
            {
                return;
            }

            m_IsCreating = false;

            var finalObject = m_CreateLogic.FinalizeCreateFromPreview();

            if (m_CreateFeedback != null)
            {
                m_CreateFeedback.OnPreviewExited(finalObject);
            }
        }

        //modifier pressed leads to non uniform
        private void OnDisableUniformScalingPerformed(InputAction.CallbackContext ctx)
        {
            m_CreateLogic.useUniformScaling = false;
        }

        //modifier released leads back to uniform
        private void OnDisableUniformScalingCanceled(InputAction.CallbackContext ctx)
        {
            m_CreateLogic.useUniformScaling = true;
        }

        private void OnMenuPerformed(InputAction.CallbackContext ctx)
        {
            if (m_ShapeMenuRoot == null || m_LeftController == null)
            {
                return;
            }

            m_IsMenuOpen = !m_IsMenuOpen;

            if (m_IsMenuOpen)
            {
                //place menu just in front of left controller
                var offset = m_LeftController.forward * 0.2f + m_LeftController.up * -0.05f;
                m_ShapeMenuRoot.transform.position = m_LeftController.position + offset;
                m_ShapeMenuRoot.transform.rotation = Quaternion.LookRotation(
                    m_LeftController.forward,
                    Vector3.up);

                m_ShapeMenuRoot.SetActive(true);
            }
            else
            {
                m_ShapeMenuRoot.SetActive(false);
            }
        }

        public void CloseMenu()
        {
            if (!m_IsMenuOpen)
            {
                return;
            }

            m_IsMenuOpen = false;

            if (m_ShapeMenuRoot != null)
            {
                m_ShapeMenuRoot.SetActive(false);
            }
        }
    }
}
