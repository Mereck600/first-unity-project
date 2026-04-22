using System.Collections.Generic;
using UnityEngine;

public class ProceduralRoadLayout
{
    public class Node
    {
        public int id;
        public Vector3 position;
    }

    public class Edge
    {
        public int from;
        public int to;
    }

    public readonly List<Node> nodes = new();
    public readonly List<Edge> edges = new();

    public static ProceduralRoadLayout GenerateLoop(int count, float radius, float jitter, int seed)
    {
        var layout = new ProceduralRoadLayout();
        var rng = new System.Random(seed);

        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;

            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            pos.x += Mathf.Lerp(-jitter, jitter, (float)rng.NextDouble());
            pos.z += Mathf.Lerp(-jitter, jitter, (float)rng.NextDouble());

            layout.nodes.Add(new Node
            {
                id = i,
                position = pos
            });
        }

        for (int i = 0; i < count; i++)
        {
            layout.edges.Add(new Edge
            {
                from = i,
                to = (i + 1) % count
            });
        }

        return layout;
    }
}