using UnityEngine;
using UnityEngine.InputSystem;

public class SphereSelectInput : MonoBehaviour
{
    public SphereSelectLogic logic;
    public float inputSensitivity = 1f;
    public InputActionProperty changeRadius; 

    private void OnEnable() => changeRadius.action.Enable();
    private void OnDisable() => changeRadius.action.Disable();

    void Update()
    {
        float y = changeRadius.action.ReadValue<Vector2>().y;
        if (Mathf.Abs(y) > 0.05f) 
        {
            float delta = y * inputSensitivity * Time.deltaTime;
            logic.SetRadius(logic.GetRadius() + delta);
        }
    }
}
