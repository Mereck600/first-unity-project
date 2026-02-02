using UnityEngine;

public class CircularMotion : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 Origin;
    public float Radius;
    private float Theta;
    void Start()
    {
        Theta = 0;
    }

    // Update is called once per frame
    void Update()
      {
        /*
        if (Theta == 2 * Mathf.PI)
        {
            Theta = 0;
        }
        else
        {
            Theta += 1f;
        }*/
        Theta += 1f;

        gameObject.transform.position = new Vector3( (Radius * Mathf.Cos(Mathf.Deg2Rad * Theta)),
                (Radius * Mathf.Sin(Mathf.Deg2Rad* Theta)),
                 (transform.position.z));
    }
}
