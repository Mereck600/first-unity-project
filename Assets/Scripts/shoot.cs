using UnityEngine;
using UnityEngine.InputSystem;

public class shoot : MonoBehaviour
{
    public float launchForce;
    private Rigidbody ballKirk;
    InputAction jumpAction;     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ballKirk = GetComponent<Rigidbody>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    // Update is called once per frame
    void Update()
    {
        if (jumpAction.IsPressed())
        {
       
            ballKirk.AddForce(-(transform.forward * launchForce), ForceMode.Impulse);
        }
    }
}
