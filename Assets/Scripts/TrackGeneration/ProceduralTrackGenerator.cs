using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TrackGen
{
    public class ProceduralTrackGenerator : MonoBehaviour
    {
        [Header("Library")]
        public RoadLibrary roadLibrary;

        [Header("Generation")]
        [Min(2)] public int targetPieceCount = 25;
        [Min(1)] public int maxPlacementAttemptsPerStep = 40;
        public int randomSeed = 12345;
        public bool useRandomSeed = true;
        public bool generateOnStart = false;

        [Header("Matching")]
        public float widthTolerance = 0.75f;
        public float angleToleranceDegrees = 10f;

        [Header("Overlap")]
        public LayerMask overlapMask;
        public float overlapShrinkFactor = 0.92f;

        [Header("Hierarchy")]
        public Transform generatedParent;

        private readonly List<PlacedPiece> placedPieces = new();
        private readonly List<OpenSocket> openSockets = new();

        private System.Random rng;

        private class PlacedPiece
        {
            public RoadPieceDefinition definition;
            public GameObject instance;
            public HashSet<int> usedSocketIndices = new();
        }

        private class OpenSocket
        {
            public PlacedPiece piece;
            public int socketIndex;
        }

        private void Start()
        {
            if (generateOnStart)
                GenerateTrack();
        }

        [ContextMenu("Generate Track")]
        public void GenerateTrack()
        {
            ClearTrack();

            if (roadLibrary == null || roadLibrary.prefabs == null || roadLibrary.prefabs.Count == 0)
            {
                Debug.LogError("RoadLibrary is missing or empty.");
                return;
            }

            rng = useRandomSeed ? new System.Random(System.Environment.TickCount) : new System.Random(randomSeed);

            RoadPieceDefinition startPrefab = roadLibrary.prefabs.FirstOrDefault(p => p != null && p.isStartPiece);
            if (startPrefab == null)
                startPrefab = roadLibrary.prefabs.FirstOrDefault(p => p != null);

            if (startPrefab == null)
            {
                Debug.LogError("No valid start prefab found.");
                return;
            }

            var startInstance = InstantiatePiece(startPrefab, Vector3.zero, Quaternion.identity);
            RegisterOpenSockets(startInstance);

            while (placedPieces.Count < targetPieceCount && openSockets.Count > 0)
            {
                if (!TryExpandOnce())
                    break;
            }

            Debug.Log($"Track generation finished. Placed {placedPieces.Count} pieces.");
        }

        [ContextMenu("Clear Track")]
        public void ClearTrack()
        {
            if (generatedParent == null)
                generatedParent = transform;

            for (int i = generatedParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(generatedParent.GetChild(i).gameObject);
            }

            placedPieces.Clear();
            openSockets.Clear();
        }

        private bool TryExpandOnce()
        {
            Shuffle(openSockets);

            for (int openIndex = 0; openIndex < openSockets.Count; openIndex++)
            {
                OpenSocket targetOpen = openSockets[openIndex];
                RoadSocket targetSocket = targetOpen.piece.definition.sockets[targetOpen.socketIndex];

                List<RoadPieceDefinition> candidates = roadLibrary.GetWeightedPool();
                Shuffle(candidates);

                int attempts = 0;

                foreach (var prefab in candidates)
                {
                    if (prefab == null || !prefab.HasSockets)
                        continue;

                    if (!prefab.canRepeatConsecutively &&
                        placedPieces.Count > 0 &&
                        placedPieces[^1].definition.pieceId == prefab.pieceId)
                    {
                        continue;
                    }

                    for (int candidateSocketIndex = 0; candidateSocketIndex < prefab.sockets.Count; candidateSocketIndex++)
                    {
                        if (attempts++ > maxPlacementAttemptsPerStep)
                            break;

                        RoadSocket candidateSocket = prefab.sockets[candidateSocketIndex];

                        if (!TrackBoundsUtility.AreSocketsCompatible(targetSocket, candidateSocket, widthTolerance))
                            continue;

                        if (!TryComputePlacement(
                            targetOpen.piece.instance.transform,
                            targetSocket,
                            prefab.transform,
                            candidateSocket,
                            out Vector3 placePos,
                            out Quaternion placeRot))
                        {
                            continue;
                        }

                        if (!IsAngleValid(
                            targetOpen.piece.instance.transform,
                            targetSocket,
                            prefab.transform,
                            candidateSocket,
                            placePos,
                            placeRot))
                        {
                            continue;
                        }

                        GameObject temp = Instantiate(prefab.gameObject, placePos, placeRot);
                        temp.transform.SetParent(generatedParent, true);

                        if (WouldOverlap(temp, prefab))
                        {
                            DestroyImmediate(temp);
                            continue;
                        }

                        var placed = RegisterPlaced(temp.GetComponent<RoadPieceDefinition>());
                        placed.usedSocketIndices.Add(candidateSocketIndex);
                        targetOpen.piece.usedSocketIndices.Add(targetOpen.socketIndex);

                        openSockets.RemoveAt(openIndex);
                        RegisterOpenSockets(placed);
                        RemoveUsedSocketsFromOpenList(placed);
                        RemoveUsedSocketsFromOpenList(targetOpen.piece);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool TryComputePlacement(
            Transform targetPieceTransform,
            RoadSocket targetSocket,
            Transform candidatePrefabTransform,
            RoadSocket candidateSocket,
            out Vector3 finalPosition,
            out Quaternion finalRotation)
        {
            Vector3 targetPos = targetSocket.GetWorldPosition(targetPieceTransform);
            Vector3 targetForward = targetSocket.GetWorldForward(targetPieceTransform);

            Quaternion candidateSocketWorldRot = Quaternion.LookRotation(
                candidatePrefabTransform.TransformDirection(candidateSocket.localForward),
                candidatePrefabTransform.TransformDirection(candidateSocket.localUp)
            );

            Quaternion desiredSocketRot = Quaternion.LookRotation(-targetForward, Vector3.up);
            finalRotation = desiredSocketRot * Quaternion.Inverse(candidateSocketWorldRot);

            Vector3 rotatedSocketOffset = finalRotation * candidateSocket.localPosition;
            finalPosition = targetPos - rotatedSocketOffset;

            return true;
        }

        private bool IsAngleValid(
            Transform targetPieceTransform,
            RoadSocket targetSocket,
            Transform candidatePrefabTransform,
            RoadSocket candidateSocket,
            Vector3 candidatePos,
            Quaternion candidateRot)
        {
            Vector3 targetForward = targetSocket.GetWorldForward(targetPieceTransform);
            Vector3 candidateForward = candidateRot * candidateSocket.localForward.normalized;

            float angle = Vector3.Angle(-targetForward, candidateForward);
            return angle <= angleToleranceDegrees;
        }

        private bool WouldOverlap(GameObject instance, RoadPieceDefinition prefabDef)
        {
            var instanceDef = instance.GetComponent<RoadPieceDefinition>();
            Bounds local = instanceDef.GetLocalBounds();

            Vector3 center = instance.transform.TransformPoint(local.center);
            Vector3 halfExtents = Vector3.Scale(local.extents, TrackBoundsUtility.Abs(instance.transform.lossyScale)) * overlapShrinkFactor;

            Collider[] hits = Physics.OverlapBox(
                center,
                halfExtents,
                instance.transform.rotation,
                overlapMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.transform.IsChildOf(instance.transform)) continue;

                foreach (var existing in placedPieces)
                {
                    if (hit.transform.IsChildOf(existing.instance.transform))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private PlacedPiece InstantiatePiece(RoadPieceDefinition prefab, Vector3 pos, Quaternion rot)
        {
            GameObject go = Instantiate(prefab.gameObject, pos, rot);
            go.transform.SetParent(generatedParent == null ? transform : generatedParent, true);
            return RegisterPlaced(go.GetComponent<RoadPieceDefinition>());
        }

        private PlacedPiece RegisterPlaced(RoadPieceDefinition definition)
        {
            var placed = new PlacedPiece
            {
                definition = definition,
                instance = definition.gameObject
            };

            placedPieces.Add(placed);
            return placed;
        }

        private void RegisterOpenSockets(PlacedPiece placed)
        {
            for (int i = 0; i < placed.definition.sockets.Count; i++)
            {
                if (!placed.usedSocketIndices.Contains(i))
                {
                    openSockets.Add(new OpenSocket
                    {
                        piece = placed,
                        socketIndex = i
                    });
                }
            }
        }

        private void RemoveUsedSocketsFromOpenList(PlacedPiece placed)
        {
            openSockets.RemoveAll(s =>
                s.piece == placed && placed.usedSocketIndices.Contains(s.socketIndex));
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}