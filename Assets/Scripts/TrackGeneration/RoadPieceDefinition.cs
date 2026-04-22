using System.Collections.Generic;
using UnityEngine;

namespace TrackGen
{
    public enum RoadPieceKind
    {
        Straight,
        Curve,
        TJunction,
        Crossroad,
        Start,
        Finish,
        Custom
    }

    public class RoadPieceDefinition : MonoBehaviour
    {
        [Header("Piece Info")]
        public string pieceId;
        public RoadPieceKind kind = RoadPieceKind.Custom;
        public int weight = 1;
        public bool canRepeatConsecutively = true;

        [Header("Sockets")]
        public List<RoadSocket> sockets = new();

        [Header("Local Bounds For Overlap Check")]
        public Vector3 localBoundsCenter;
        public Vector3 localBoundsSize = Vector3.one * 5f;

        [Header("Optional")]
        public bool isStartPiece = false;
        public bool isFinishPiece = false;

        public Bounds GetLocalBounds()
        {
            return new Bounds(localBoundsCenter, localBoundsSize);
        }

        public bool HasSockets => sockets != null && sockets.Count > 0;

        private void Reset()
        {
            pieceId = gameObject.name;
        }
    }
}