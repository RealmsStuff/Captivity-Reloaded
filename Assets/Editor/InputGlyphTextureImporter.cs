using UnityEditor;
using UnityEngine;

public sealed class InputGlyphTextureImporter : AssetPostprocessor
{
	public static void ReimportAll()
	{
		string[] guids = AssetDatabase.FindAssets("t:Texture", new[] { "Assets/Resources/InputGlyphs" });
		foreach (string guid in guids)
			AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
	}

	private void OnPreprocessTexture()
	{
		if (!assetPath.StartsWith("Assets/Resources/InputGlyphs/", System.StringComparison.Ordinal)) return;
		TextureImporter importer = (TextureImporter)assetImporter;
		importer.textureCompression = TextureImporterCompression.Uncompressed;
		importer.crunchedCompression = false;
		importer.mipmapEnabled = false;
		importer.npotScale = TextureImporterNPOTScale.None;
		importer.filterMode = FilterMode.Bilinear;
		importer.alphaIsTransparency = true;
		importer.wrapMode = TextureWrapMode.Clamp;
	}

	public override uint GetVersion()
	{
		return 2u;
	}
}
