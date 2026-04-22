using UnityEditor;
using UnityEngine;
using TrackGen;

[CustomEditor(typeof(RoadPieceDefinition))]
public class RoadPieceEditorTools : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoadPieceDefinition piece = (RoadPieceDefinition)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Auto Compute Bounds From Colliders"))
        {
            AutoComputeBoundsFromColliders(piece);
        }

        if (GUILayout.Button("Auto Compute Bounds From Renderers"))
        {
            AutoComputeBoundsFromRenderers(piece);
        }

        if (GUILayout.Button("Add Socket From Selected Child"))
        {
            AddSocketFromSelectedChild(piece);
        }
    }

    private void AutoComputeBoundsFromColliders(RoadPieceDefinition piece)
    {
        Collider[] colliders = piece.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found.");
            return;
        }

        Bounds combined = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            combined.Encapsulate(colliders[i].bounds);
        }

        piece.localBoundsCenter = piece.transform.InverseTransformPoint(combined.center);
        piece.localBoundsSize = piece.transform.InverseTransformVector(combined.size);

        piece.localBoundsSize = new Vector3(
            Mathf.Abs(piece.localBoundsSize.x),
            Mathf.Abs(piece.localBoundsSize.y),
            Mathf.Abs(piece.localBoundsSize.z)
        );

        EditorUtility.SetDirty(piece);
    }

    private void AutoComputeBoundsFromRenderers(RoadPieceDefinition piece)
    {
        Renderer[] renderers = piece.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found.");
            return;
        }

        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combined.Encapsulate(renderers[i].bounds);
        }

        piece.localBoundsCenter = piece.transform.InverseTransformPoint(combined.center);
        piece.localBoundsSize = piece.transform.InverseTransformVector(combined.size);

        piece.localBoundsSize = new Vector3(
            Mathf.Abs(piece.localBoundsSize.x),
            Mathf.Abs(piece.localBoundsSize.y),
            Mathf.Abs(piece.localBoundsSize.z)
        );

        EditorUtility.SetDirty(piece);
    }

    private void AddSocketFromSelectedChild(RoadPieceDefinition piece)
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogWarning("Select a child transform first.");
            return;
        }

        Transform child = Selection.activeTransform;
        if (!child.IsChildOf(piece.transform))
        {
            Debug.LogWarning("Selected object must be a child of the road piece.");
            return;
        }

        piece.sockets.Add(new RoadSocket
        {
            id = child.name,
            localPosition = piece.transform.InverseTransformPoint(child.position),
            localForward = piece.transform.InverseTransformDirection(child.forward).normalized,
            localUp = piece.transform.InverseTransformDirection(child.up).normalized
        });

        EditorUtility.SetDirty(piece);
    }
}