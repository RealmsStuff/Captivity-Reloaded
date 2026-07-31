using UnityEngine;
using UnityEditor;

public class SpriteMaterialConverter : EditorWindow
{
    [MenuItem("Tools/Sprite Material Converter")]
    public static void ShowWindow()
    {
        GetWindow<SpriteMaterialConverter>("Sprite Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("Convert Sprite Materials", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool searches for SpriteRenderers using a material or shader containing 'Diffuse' and replaces it with 'Sprite-Lit-Default'.", MessageType.Info);

        GUILayout.Space(10);

        if (GUILayout.Button("1. Convert in Active Scene", GUILayout.Height(30)))
        {
            ConvertInScene();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("2. Convert ALL Prefabs (Project Wide)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Warning", "This will modify all prefabs in your project. It cannot be undone. Make sure you have a backup!", "Do it", "Cancel"))
            {
                ConvertInPrefabs();
            }
        }
    }

    private void ConvertInScene()
    {
        Material litMat = FindLitMaterial();
        if (litMat == null) return;

        SpriteRenderer[] renderers = FindObjectsOfType<SpriteRenderer>();
        int count = 0;

        foreach (SpriteRenderer sr in renderers)
        {
            if (IsTargetMaterial(sr.sharedMaterial))
            {
                Undo.RecordObject(sr, "Convert Sprite Material");
                sr.sharedMaterial = litMat;
                EditorUtility.SetDirty(sr);
                count++;
            }
        }
        Debug.Log($"<color=green><b>Success:</b></color> Converted {count} SpriteRenderers in the current scene.");
    }

    private void ConvertInPrefabs()
    {
        Material litMat = FindLitMaterial();
        if (litMat == null) return;

        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;
        int prefabCount = 0;

        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            SpriteRenderer[] renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            bool modified = false;

            foreach (SpriteRenderer sr in renderers)
            {
                if (IsTargetMaterial(sr.sharedMaterial))
                {
                    sr.sharedMaterial = litMat;
                    modified = true;
                    count++;
                }
            }

            if (modified)
            {
                EditorUtility.SetDirty(prefab);
                prefabCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"<color=green><b>Success:</b></color> Converted {count} SpriteRenderers across {prefabCount} prefabs.");
    }

    private bool IsTargetMaterial(Material mat)
    {
        if (mat == null) return false;

        // Checks if the material name OR the shader name contains "Diffuse"
        return mat.name.Contains("Diffuse") || (mat.shader != null && mat.shader.name.Contains("Diffuse"));
    }

    private Material FindLitMaterial()
    {
        // Try to find the default URP 2D lit material
        string[] guids = AssetDatabase.FindAssets("Sprite-Lit-Default t:Material");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        Debug.LogError("Could not find 'Sprite-Lit-Default' material! Are you sure the 2D URP package is installed and setup?");
        return null;
    }
}