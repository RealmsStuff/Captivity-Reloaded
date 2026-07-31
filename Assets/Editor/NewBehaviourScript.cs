using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public class SpriteToAssetConverter
{
    [MenuItem("Assets/Create Trimmed Sprite Asset")]
    public static void CreateTrimmedAsset()
    {
        if (Selection.activeObject is Sprite sourceSprite)
        {
            Texture2D texture = sourceSprite.texture;
            string texturePath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            // Temporary make texture readable to analyze pixels
            bool wasReadable = importer.isReadable;
            if (!wasReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            Rect rect = sourceSprite.rect;
            int minX = (int)rect.xMax;
            int maxX = (int)rect.xMin;
            int minY = (int)rect.yMax;
            int maxY = (int)rect.yMin;

            // Find the boundary of non-transparent pixels
            Color[] pixels = texture.GetPixels((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
            bool foundPixel = false;

            for (int y = 0; y < (int)rect.height; y++)
            {
                for (int x = 0; x < (int)rect.width; x++)
                {
                    Color color = pixels[y * (int)rect.width + x];
                    if (color.a > 0.05f) // Transparency threshold
                    {
                        int globalX = (int)rect.x + x;
                        int globalY = (int)rect.y + y;

                        if (globalX < minX) minX = globalX;
                        if (globalX > maxX) maxX = globalX;
                        if (globalY < minY) minY = globalY;
                        if (globalY > maxY) maxY = globalY;
                        foundPixel = true;
                    }
                }
            }

            // Restore readability setting
            if (!wasReadable)
            {
                importer.isReadable = false;
                importer.SaveAndReimport();
            }

            if (!foundPixel)
            {
                UnityEngine.Debug.LogError("No non-transparent pixels were found in the selected sprite.");
                return;
            }

            // Create the tight bounding box
            Rect trimmedRect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);

            // Calculate the pivot position relative to the new trimmed bounds
            Vector2 pixelPivot = sourceSprite.pivot;
            Vector2 customPivot = new Vector2(
                (pixelPivot.x - trimmedRect.x) / trimmedRect.width,
                (pixelPivot.y - trimmedRect.y) / trimmedRect.height
            );

            // Create the new trimmed Sprite
            Sprite trimmedSprite = Sprite.Create(texture, trimmedRect, customPivot, sourceSprite.pixelsPerUnit);

            // Generate path and save
            string originalPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            string folderPath = originalPath.Substring(0, originalPath.LastIndexOf('/'));
            string newPath = $"{folderPath}/{sourceSprite.name}.asset";

            AssetDatabase.CreateAsset(trimmedSprite, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Debug.Log($"Created trimmed sprite asset ({trimmedRect.width}x{trimmedRect.height}) at: {newPath}");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Please select the Sprite sub-asset (expanded from the PNG) in the Project window.");
        }
    }
}