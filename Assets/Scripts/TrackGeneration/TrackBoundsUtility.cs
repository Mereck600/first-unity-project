using UnityEngine;

namespace TrackGen
{
    public static class TrackBoundsUtility
    {
        public static bool AreSocketsCompatible(RoadSocket a, RoadSocket b, float widthTolerance, int laneTolerance = 0)
        {
            if (a.socketType != b.socketType)
                return false;

            if (Mathf.Abs(a.width - b.width) > widthTolerance)
                return false;

            if (Mathf.Abs(a.laneCount - b.laneCount) > laneTolerance)
                return false;

            return true;
        }

        public static Bounds TransformBounds(Bounds localBounds, Transform t)
        {
            Vector3 worldCenter = t.TransformPoint(localBounds.center);

            Vector3 scaledSize = Vector3.Scale(localBounds.size, Abs(t.lossyScale));
            return new Bounds(worldCenter, scaledSize);
        }

        public static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public static bool OverlapsExisting(
            RoadPieceDefinition candidate,
            Vector3 position,
            Quaternion rotation,
            LayerMask overlapMask,
            float shrinkFactor = 0.92f)
        {
            Bounds local = candidate.GetLocalBounds();

            Vector3 worldCenter = position + rotation * Vector3.Scale(local.center, candidate.transform.localScale);
            Vector3 halfExtents = Vector3.Scale(local.extents, Abs(candidate.transform.localScale)) * shrinkFactor;

            Collider[] hits = Physics.OverlapBox(
                worldCenter,
                halfExtents,
                rotation,
                overlapMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.transform.IsChildOf(candidate.transform)) continue;
                return true;
            }

            return false;
        }
    }
}