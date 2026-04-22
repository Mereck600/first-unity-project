using System.Collections.Generic;
using UnityEngine;

public class ProceduralRoadPath : MonoBehaviour
{
    public int seed = 123;
    public int numPoints = 20;
    public float segmentLength = 25f;
    public float turnStrength = 30f;

    public bool generateOnStart = true;

    private System.Random rng;

    void Start()
    {
        if (generateOnStart)
        {
            Generate();
        }
    }

    public List<Vector3> Generate()
    {
        rng = new System.Random(seed);

        List<Vector3> points = new List<Vector3>();

        Vector3 currentPos = Vector3.zero;
        Vector3 direction = Vector3.forward;

        points.Add(currentPos);

        for (int i = 0; i < numPoints; i++)
        {
            // Random turn
            float angle = Mathf.Lerp(-turnStrength, turnStrength, (float)rng.NextDouble());
            direction = Quaternion.Euler(0, angle, 0) * direction;

            // Move forward
            currentPos += direction.normalized * segmentLength;

            points.Add(currentPos);
        }

        return points;
    }
}