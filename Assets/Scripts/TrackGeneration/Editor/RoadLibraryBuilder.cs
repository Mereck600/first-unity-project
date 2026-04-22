using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using TrackGen;

namespace TrackGen.Editor
{
    public static class RoadLibraryBuilder
    {
        [MenuItem("Track Generation/Prepare Selected Road Prefabs")]
        public static void PrepareSelectedRoadPrefabs()
        {
            var prefabPaths = GetSelectedPrefabAssetPaths();
            if (prefabPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Prepare Road Prefabs", "Select one or more road prefab assets or folders containing prefabs.", "OK");
                return;
            }

            int prepared = 0;
            foreach (var path in prefabPaths)
            {
                if (PreparePrefab(path))
                    prepared++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Prepare Road Prefabs", $"Prepared {prepared} prefab(s).", "OK");
        }

        [MenuItem("Track Generation/Create Road Library From Selected Prefabs")]
        public static void CreateRoadLibraryFromSelection()
        {
            var prefabPaths = GetSelectedPrefabAssetPaths();
            if (prefabPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Create Road Library", "Select one or more road prefab assets or folders containing prefabs.", "OK");
                return;
            }

            string savePath = EditorUtility.SaveFilePanelInProject("Save Road Library", "RoadLibrary", "asset", "Choose a path to save the Road Library asset.");
            if (string.IsNullOrEmpty(savePath))
                return;

            RoadLibrary roadLibrary = ScriptableObject.CreateInstance<RoadLibrary>();

            foreach (var path in prefabPaths)
            {
                GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefabAsset == null)
                    continue;

                RoadPieceDefinition definition = prefabAsset.GetComponent<RoadPieceDefinition>();
                if (definition != null)
                    roadLibrary.prefabs.Add(definition);
            }

            if (roadLibrary.prefabs.Count == 0)
            {
                EditorUtility.DisplayDialog("Create Road Library", "No selected prefabs contain a RoadPieceDefinition component.", "OK");
                return;
            }

            AssetDatabase.CreateAsset(roadLibrary, savePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Create Road Library", $"Created Road Library with {roadLibrary.prefabs.Count} prefab(s).", "OK");
        }

        private static List<string> GetSelectedPrefabAssetPaths()
        {
            var paths = new List<string>();
            foreach (var obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    var guids = AssetDatabase.FindAssets("t:GameObject", new[] { path });
                    foreach (var guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!paths.Contains(assetPath))
                            paths.Add(assetPath);
                    }
                }
                else if (Path.GetExtension(path).Equals(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (!paths.Contains(path))
                        paths.Add(path);
                }
            }
            return paths;
        }

        private static bool PreparePrefab(string assetPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(assetPath);
            if (root == null)
                return false;

            bool dirty = false;
            var definition = root.GetComponent<RoadPieceDefinition>();
            if (definition == null)
            {
                definition = root.AddComponent<RoadPieceDefinition>();
                definition.pieceId = root.name;
                dirty = true;
            }

            if (string.IsNullOrEmpty(definition.pieceId))
            {
                definition.pieceId = root.name;
                dirty = true;
            }

            if (definition.localBoundsSize == Vector3.zero)
            {
                ComputeBoundsFromRenderers(definition);
                dirty = true;
            }

            if (dirty)
            {
                PrefabUtility.SaveAsPrefabAsset(root, assetPath);
            }

            PrefabUtility.UnloadPrefabContents(root);
            return true;
        }

        private static void ComputeBoundsFromRenderers(RoadPieceDefinition piece)
        {
            var renderers = piece.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            piece.localBoundsCenter = piece.transform.InverseTransformPoint(bounds.center);
            piece.localBoundsSize = piece.transform.InverseTransformVector(bounds.size);
            piece.localBoundsSize = new Vector3(
                Mathf.Abs(piece.localBoundsSize.x),
                Mathf.Abs(piece.localBoundsSize.y),
                Mathf.Abs(piece.localBoundsSize.z)
            );
        }
    }
}
