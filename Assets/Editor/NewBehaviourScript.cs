using System;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MaterialReplacer : EditorWindow
{
    // Paths specified in your prompt
    private const string OLD_MAT_1_PATH = "Assets/Resources/materials/SpriteDiffuse.mat";
    private const string OLD_MAT_2_PATH = "Assets/Material/Sprite-Unlit-Default.mat";
    private const string NEW_MAT_PATH = "Assets/Material/Sprite-Lit-Default.mat";

    [MenuItem("Tools/Swap Specific Materials")]
    public static void ReplaceMaterials()
    {
        // 1. Load the materials
        Material oldMat1 = AssetDatabase.LoadAssetAtPath<Material>(OLD_MAT_1_PATH);
        Material oldMat2 = AssetDatabase.LoadAssetAtPath<Material>(OLD_MAT_2_PATH);
        Material newMat = AssetDatabase.LoadAssetAtPath<Material>(NEW_MAT_PATH);

        // Check if the replacement material exists
        if (newMat == null)
        {
            UnityEngine.Debug.LogError($"[MaterialReplacer] Target material NOT found at path: {NEW_MAT_PATH}. Aborting operation.");
            return;
        }

        if (oldMat1 == null && oldMat2 == null)
        {
            UnityEngine.Debug.LogWarning("[MaterialReplacer] Neither of the source materials could be found at the specified paths. Check your file paths.");
            return;
        }

        // Confirmation Dialog
        bool proceed = EditorUtility.DisplayDialog(
            "Replace Materials",
            $"Are you sure you want to replace references to:\n- {OLD_MAT_1_PATH}\n- {OLD_MAT_2_PATH}\n\nWith:\n- {NEW_MAT_PATH}?",
            "Yes, Replace All", "Cancel"
        );

        if (!proceed) return;

        int replacementsCount = 0;

        // 2. Replace in current Scene(s)
        replacementsCount += ProcessSceneObjects(oldMat1, oldMat2, newMat);

        // 3. Replace in all Prefabs in the Project
        replacementsCount += ProcessPrefabs(oldMat1, oldMat2, newMat);

        // Save Project Changes
        AssetDatabase.SaveAssets();

        UnityEngine.Debug.Log($"<color=green>[MaterialReplacer] Complete!</color> Replaced material on {replacementsCount} renderer component(s).");
    }

    private static int ProcessSceneObjects(Material old1, Material old2, Material newMat)
    {
        int count = 0;
        // Find all Renderers (SpriteRenderer, MeshRenderer, etc.) including inactive ones
        Renderer[] sceneRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>(true);

        foreach (Renderer renderer in sceneRenderers)
        {
            bool changed = ReplaceInRenderer(renderer, old1, old2, newMat);
            if (changed)
            {
                count++;
                // Mark scene as dirty so Unity knows changes need saving
                EditorSceneManager.MarkSceneDirty(renderer.gameObject.scene);
            }
        }

        return count;
    }

    private static int ProcessPrefabs(Material old1, Material old2, Material newMat)
    {
        int count = 0;
        // Search for all prefab assets in the project
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            bool prefabModified = false;

            foreach (Renderer renderer in renderers)
            {
                if (ReplaceInRenderer(renderer, old1, old2, newMat))
                {
                    prefabModified = true;
                    count++;
                }
            }

            // Save changes back to the prefab asset if updated
            if (prefabModified)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        return count;
    }

    private static bool ReplaceInRenderer(Renderer renderer, Material old1, Material old2, Material newMat)
    {
        // Use sharedMaterials to avoid instantiating new materials in memory
        Material[] mats = renderer.sharedMaterials;
        bool hasChanged = false;

        for (int i = 0; i < mats.Length; i++)
        {
            if ((old1 != null && mats[i] == old1) || (old2 != null && mats[i] == old2))
            {
                mats[i] = newMat;
                hasChanged = true;
            }
        }

        if (hasChanged)
        {
            Undo.RecordObject(renderer, "Swap Materials");
            renderer.sharedMaterials = mats;
        }

        return hasChanged;
    }
}