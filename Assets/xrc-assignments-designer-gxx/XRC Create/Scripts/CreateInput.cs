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

        [Header("controller refs")]
        [SerializeField]
        private Transform m_LeftController;

        [SerializeField]
        private Transform m_RightController;

        [SerializeField]
        private Transform m_XROrigin;

        private CreateLogic m_CreateLogic;
        private CreateFeedback m_CreateFeedback;
        private bool m_IsCreating;

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

        private void Awake()
        {
            m_CreateLogic = GetComponent<CreateLogic>();
            m_CreateFeedback = GetComponent<CreateFeedback>();
        }

        private void OnEnable()
        {
            m_CreateAction.action.Enable();
            m_DisableUniformScalingAction.action.Enable();

            m_CreateAction.action.started += OnCreateStarted;
            m_CreateAction.action.canceled += OnCreateCanceled;

            m_DisableUniformScalingAction.action.performed += OnDisableUniformScalingPerformed;
            m_DisableUniformScalingAction.action.canceled += OnDisableUniformScalingCanceled;
        }

        private void OnDisable()
        {
            m_CreateAction.action.started -= OnCreateStarted;
            m_CreateAction.action.canceled -= OnCreateCanceled;

            m_DisableUniformScalingAction.action.performed -= OnDisableUniformScalingPerformed;
            m_DisableUniformScalingAction.action.canceled -= OnDisableUniformScalingCanceled;

            m_CreateAction.action.Disable();
            m_DisableUniformScalingAction.action.Disable();
        }

        private void Update()
        {
            if (!m_IsCreating)
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
    }
}
