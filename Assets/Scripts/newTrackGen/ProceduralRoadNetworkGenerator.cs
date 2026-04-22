using System.Collections.Generic;
using UnityEngine;
using Barmetler.RoadSystem;

public class ProceduralRoadNetworkGenerator : MonoBehaviour
{
    [Header("References")]
    public RoadSystem roadSystem;
    public Intersection intersectionPrefab;
    public Road roadPrefab;

    [Header("Generation")]
    public int seed = 12345;
    public int intersectionCount = 8;
    public float radius = 80f;
    public float jitter = 10f;
    public bool generateOnStart = false;

    [Header("Placement")]
    public float minIntersectionSpacing = 20f;

    private readonly List<Intersection> spawnedIntersections = new();
    private readonly List<Road> spawnedRoads = new();

    private void Start()
    {
        if (generateOnStart)
            Generate();
    }

    [ContextMenu("Generate Road Network")]
    public void Generate()
    {
        if (roadSystem == null || intersectionPrefab == null || roadPrefab == null)
        {
            Debug.LogError("Missing references on ProceduralRoadNetworkGenerator.");
            return;
        }

        ClearGenerated();

        var layout = ProceduralRoadLayout.GenerateLoop(
            intersectionCount,
            radius,
            jitter,
            seed
        );

        if (!ValidateNodeSpacing(layout))
        {
            Debug.LogWarning("Generated layout failed spacing check.");
            return;
        }

        Dictionary<int, Intersection> intersectionsById = new();

        // Spawn intersections
        foreach (var node in layout.nodes)
        {
            var intersection = Instantiate(
                intersectionPrefab,
                node.position,
                Quaternion.identity,
                roadSystem.transform
            );

            intersection.name = $"Intersection_{node.id}";
            intersection.Invalidate(false);

            spawnedIntersections.Add(intersection);
            intersectionsById[node.id] = intersection;
        }

        // Spawn roads and connect anchors
        foreach (var edge in layout.edges)
        {
            var a = intersectionsById[edge.from];
            var b = intersectionsById[edge.to];

            var startAnchor = GetBestFreeAnchorFacingTarget(a, b.transform.position);
            var endAnchor = GetBestFreeAnchorFacingTarget(b, a.transform.position);

            if (startAnchor == null || endAnchor == null)
            {
                Debug.LogWarning($"Could not find free anchors for road {edge.from} -> {edge.to}");
                continue;
            }

            var road = Instantiate(
                roadPrefab,
                Vector3.zero,
                Quaternion.identity,
                roadSystem.transform
            );

            road.name = $"Road_{edge.from}_{edge.to}";

            // Connect anchors to road
            startAnchor.SetRoad(road, true);
            endAnchor.SetRoad(road, false);

            // Refresh endpoints after assigning anchors
            road.RefreshEndPoints();

            spawnedRoads.Add(road);
        }

        roadSystem.RebuildAllRoads();
    }

    [ContextMenu("Clear Generated Network")]
    public void ClearGenerated()
    {
        if (roadSystem == null) return;

        List<GameObject> children = new();

        foreach (Transform child in roadSystem.transform)
        {
            children.Add(child.gameObject);
        }

        for (int i = children.Count - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(children[i]);
            else
                Destroy(children[i]);
#else
            Destroy(children[i]);
#endif
        }

        spawnedIntersections.Clear();
        spawnedRoads.Clear();
    }

    private bool ValidateNodeSpacing(ProceduralRoadLayout layout)
    {
        for (int i = 0; i < layout.nodes.Count; i++)
        {
            for (int j = i + 1; j < layout.nodes.Count; j++)
            {
                float dist = Vector3.Distance(layout.nodes[i].position, layout.nodes[j].position);
                if (dist < minIntersectionSpacing)
                    return false;
            }
        }

        return true;
    }

    private RoadAnchor GetBestFreeAnchorFacingTarget(Intersection intersection, Vector3 targetPosition)
    {
        if (intersection == null) return null;

        RoadAnchor best = null;
        float bestScore = float.NegativeInfinity;

        foreach (var anchor in intersection.AnchorPoints)
        {
            if (anchor == null) continue;
            if (anchor.GetConnectedRoad() != null) continue;

            Vector3 toTarget = (targetPosition - anchor.transform.position).normalized;
            float facingScore = Vector3.Dot(anchor.transform.forward, toTarget);

            if (facingScore > bestScore)
            {
                bestScore = facingScore;
                best = anchor;
            }
        }

        return best;
    }
}