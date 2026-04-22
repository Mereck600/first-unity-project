using System.Collections.Generic;
using UnityEngine;

public class RoadSystemBridge : MonoBehaviour
{
    public ProceduralRoadPath generator;

    // Assign your road system component here
    public MonoBehaviour roadComponent;

    public void BuildRoad()
    {
        List<Vector3> points = generator.Generate();

        // --- THIS PART DEPENDS ON THE ASSET ---
        // Common patterns:

        // Example 1:
        // roadComponent.SetPoints(points);

        // Example 2:
        // roadComponent.nodes.Clear();
        // foreach (var p in points)
        //     roadComponent.nodes.Add(p);

        // Example 3:
        // roadComponent.AddNode(p);

        // Then:
        // roadComponent.Build();

        Debug.Log("Hook up Road System API here");
    }
}