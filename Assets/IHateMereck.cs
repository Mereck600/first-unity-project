using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class VRCarController : MonoBehaviour
{
    public float speed = 100f;
    public Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);

        if (trigger > 0.1f)
        {
            rb = GetComponent<Rigidbody>();
            transform.Translate(Vector3.forward * speed * trigger * Time.deltaTime);
        }
    }
}
