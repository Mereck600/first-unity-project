using System.Collections.Generic;
using UnityEngine;

namespace TrackGen
{
    [CreateAssetMenu(menuName = "Track Generation/Road Library")]
    public class RoadLibrary : ScriptableObject
    {
        public List<RoadPieceDefinition> prefabs = new();

        public List<RoadPieceDefinition> GetWeightedPool()
        {
            var result = new List<RoadPieceDefinition>();

            foreach (var prefab in prefabs)
            {
                if (prefab == null) continue;

                int count = Mathf.Max(1, prefab.weight);
                for (int i = 0; i < count; i++)
                {
                    result.Add(prefab);
                }
            }

            return result;
        }
    }
}