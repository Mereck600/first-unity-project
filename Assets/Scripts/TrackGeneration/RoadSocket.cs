using System;
using UnityEngine;

namespace TrackGen
{
    public enum SocketType
    {
        Road,
        Intersection,
        Ramp,
        DeadEnd,
        Finish
    }

    [Serializable]
    public class RoadSocket
    {
        public string id = "Socket";
        public SocketType socketType = SocketType.Road;

        [Tooltip("Local-space position of the socket on the prefab.")]
        public Vector3 localPosition;

        [Tooltip("Local-space forward direction. This should point OUT of the road piece.")]
        public Vector3 localForward = Vector3.forward;

        [Tooltip("Local-space up direction.")]
        public Vector3 localUp = Vector3.up;

        [Tooltip("Approximate width at the socket. Used to reject obviously bad matches.")]
        public float width = 4f;

        [Tooltip("Optional lane count check.")]
        public int laneCount = 2;

        public Vector3 GetWorldPosition(Transform t)
        {
            return t.TransformPoint(localPosition);
        }

        public Vector3 GetWorldForward(Transform t)
        {
            return t.TransformDirection(localForward).normalized;
        }

        public Vector3 GetWorldUp(Transform t)
        {
            return t.TransformDirection(localUp).normalized;
        }
    }
}