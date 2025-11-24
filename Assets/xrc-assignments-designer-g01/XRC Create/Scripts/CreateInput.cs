using UnityEngine;
using UnityEngine.InputSystem;

namespace XRC.Toolkit.Create
{
    [RequireComponent(typeof(CreateLogic))]
    public class CreateInput : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField]
        private InputActionProperty m_LeftTriggerAction;

        [SerializeField]
        private InputActionProperty m_RightTriggerAction;

        [SerializeField]
        private InputActionProperty m_DuplicateAction;

        [SerializeField]
        private InputActionProperty m_DeleteAction;

        [SerializeField]
        private InputActionProperty m_DisableUniformScalingAction;

        [Header("Controller Refs")]
        [SerializeField]
        private Transform m_LeftController;

        [SerializeField]
        private Transform m_RightController;

        [SerializeField]
        private Transform m_XROrigin;

        [Header("Trigger Threshold")]
        [SerializeField]
        private float m_TriggerThreshold = 0.5f;

        private CreateLogic m_CreateLogic;
        private CreateFeedback m_CreateFeedback;
        private bool m_IsCreating;
        private bool m_LeftTriggerPressed;
        private bool m_RightTriggerPressed;

        public InputActionProperty leftTriggerAction
        {
            get => m_LeftTriggerAction;
            set => m_LeftTriggerAction = value;
        }

        public InputActionProperty rightTriggerAction
        {
            get => m_RightTriggerAction;
            set => m_RightTriggerAction = value;
        }

        public InputActionProperty duplicateAction
        {
            get => m_DuplicateAction;
            set => m_DuplicateAction = value;
        }

        public InputActionProperty deleteAction
        {
            get => m_DeleteAction;
            set => m_DeleteAction = value;
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
            if (m_LeftTriggerAction.action != null)
            {
                m_LeftTriggerAction.action.Enable();
                m_LeftTriggerAction.action.started += OnLeftTriggerStarted;
                m_LeftTriggerAction.action.canceled += OnLeftTriggerCanceled;
            }

            if (m_RightTriggerAction.action != null)
            {
                m_RightTriggerAction.action.Enable();
                m_RightTriggerAction.action.started += OnRightTriggerStarted;
                m_RightTriggerAction.action.canceled += OnRightTriggerCanceled;
            }

            if (m_DisableUniformScalingAction.action != null)
            {
                m_DisableUniformScalingAction.action.Enable();
                m_DisableUniformScalingAction.action.performed += OnDisableUniformScalingPerformed;
                m_DisableUniformScalingAction.action.canceled += OnDisableUniformScalingCanceled;
            }

            if (m_DuplicateAction.action != null)
            {
                m_DuplicateAction.action.Enable();
                m_DuplicateAction.action.performed += OnDuplicatePerformed;
            }

            if (m_DeleteAction.action != null)
            {
                m_DeleteAction.action.Enable();
                m_DeleteAction.action.performed += OnDeletePerformed;
            }
        }

        private void OnDisable()
        {
            if (m_LeftTriggerAction.action != null)
            {
                m_LeftTriggerAction.action.started -= OnLeftTriggerStarted;
                m_LeftTriggerAction.action.canceled -= OnLeftTriggerCanceled;
                m_LeftTriggerAction.action.Disable();
            }

            if (m_RightTriggerAction.action != null)
            {
                m_RightTriggerAction.action.started -= OnRightTriggerStarted;
                m_RightTriggerAction.action.canceled -= OnRightTriggerCanceled;
                m_RightTriggerAction.action.Disable();
            }

            if (m_DisableUniformScalingAction.action != null)
            {
                m_DisableUniformScalingAction.action.performed -= OnDisableUniformScalingPerformed;
                m_DisableUniformScalingAction.action.canceled -= OnDisableUniformScalingCanceled;
                m_DisableUniformScalingAction.action.Disable();
            }

            if (m_DuplicateAction.action != null)
            {
                m_DuplicateAction.action.performed -= OnDuplicatePerformed;
                m_DuplicateAction.action.Disable();
            }

            if (m_DeleteAction.action != null)
            {
                m_DeleteAction.action.performed -= OnDeletePerformed;
                m_DeleteAction.action.Disable();
            }
        }

        private void Update()
        {
            bool bothTriggersPressed = m_LeftTriggerPressed && m_RightTriggerPressed;

            if (bothTriggersPressed && !m_IsCreating)
            {
                StartCreating();
            }
            else if (!bothTriggersPressed && m_IsCreating)
            {
                StopCreating();
            }

            if (m_IsCreating)
            {
                if (m_LeftController == null || m_RightController == null)
                {
                    return;
                }

                if (m_CreateLogic != null)
                {
                    m_CreateLogic.UpdatePreviewTransform(
                        m_LeftController.position,
                        m_RightController.position,
                        m_XROrigin);
                }
            }
        }

        private void OnLeftTriggerStarted(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            m_LeftTriggerPressed = value >= m_TriggerThreshold;
        }

        private void OnLeftTriggerCanceled(InputAction.CallbackContext ctx)
        {
            m_LeftTriggerPressed = false;
        }

        private void OnRightTriggerStarted(InputAction.CallbackContext ctx)
        {
            float value = ctx.ReadValue<float>();
            m_RightTriggerPressed = value >= m_TriggerThreshold;
        }

        private void OnRightTriggerCanceled(InputAction.CallbackContext ctx)
        {
            m_RightTriggerPressed = false;
        }

        private void StartCreating()
        {
            m_IsCreating = true;

            if (m_LeftController != null && m_RightController != null && m_CreateLogic != null)
            {
                m_CreateLogic.UpdatePreviewTransform(
                    m_LeftController.position,
                    m_RightController.position,
                    m_XROrigin);
            }

            if (m_CreateFeedback != null && m_CreateLogic != null)
            {
                m_CreateFeedback.OnPreviewEntered(m_CreateLogic.previewObject);
            }
        }

        private void StopCreating()
        {
            m_IsCreating = false;

            if (m_CreateLogic == null)
            {
                return;
            }

            var finalObject = m_CreateLogic.FinalizeCreateFromPreview();

            if (m_CreateFeedback != null && finalObject != null)
            {
                m_CreateFeedback.OnPreviewExited(finalObject);
            }
        }

        private void OnDisableUniformScalingPerformed(InputAction.CallbackContext ctx)
        {
            if (m_CreateLogic != null)
            {
                m_CreateLogic.useUniformScaling = false;
            }
        }

        private void OnDisableUniformScalingCanceled(InputAction.CallbackContext ctx)
        {
            if (m_CreateLogic != null)
            {
                m_CreateLogic.useUniformScaling = true;
            }
        }

        private void OnDuplicatePerformed(InputAction.CallbackContext ctx)
        {
            if (m_CreateLogic != null)
            {
                m_CreateLogic.DuplicateSelected();
            }
        }

        private void OnDeletePerformed(InputAction.CallbackContext ctx)
        {
            if (m_CreateLogic != null)
            {
                m_CreateLogic.DeleteSelected();
            }
        }
    }
}
