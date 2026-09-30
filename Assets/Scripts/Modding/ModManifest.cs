using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModDependency
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("version", Required = Required.Always)]
		public string Version { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModAssetBundlePayload
	{
		[JsonProperty("platform", Required = Required.Always)]
		public string Platform { get; set; }

		[JsonProperty("path", Required = Required.Always)]
		public string Path { get; set; }

		[JsonProperty("sha256", Required = Required.Always)]
		public string Sha256 { get; set; }

		[JsonProperty("sizeBytes", Required = Required.Always)]
		public long SizeBytes { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModManifest
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("displayName", Required = Required.Always)]
		public string DisplayName { get; set; }

		[JsonProperty("version", Required = Required.Always)]
		public string Version { get; set; }

		[JsonProperty("modApiVersion", Required = Required.Always)]
		public int ModApiVersion { get; set; }

		[JsonProperty("authors")]
		public List<string> Authors { get; set; } = new List<string>();

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("previewImages")]
		public List<string> PreviewImages { get; set; } = new List<string>();

		[JsonProperty("dependencies")]
		public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();

		[JsonProperty("optionalDependencies")]
		public List<ModDependency> OptionalDependencies { get; set; } = new List<ModDependency>();

		[JsonProperty("loadAfter")]
		public List<string> LoadAfter { get; set; } = new List<string>();

		[JsonProperty("loadBefore")]
		public List<string> LoadBefore { get; set; } = new List<string>();

		[JsonProperty("conflicts")]
		public List<string> Conflicts { get; set; } = new List<string>();

		[JsonProperty("overrides")]
		public List<string> Overrides { get; set; } = new List<string>();

		[JsonProperty("priority")]
		public int Priority { get; set; }

		[JsonProperty("contentRoots", Required = Required.Always)]
		public List<string> ContentRoots { get; set; } = new List<string>();

		[JsonProperty("assetBundles")]
		public List<ModAssetBundlePayload> AssetBundles { get; set; } = new List<ModAssetBundlePayload>();
	}

	public sealed class ModPack
	{
		public ModManifest Manifest { get; }
		public SemanticVersion Version { get; }
		public string RootPath { get; }

		public ModPack(ModManifest i_manifest, SemanticVersion i_version, string i_rootPath)
		{
			Manifest = i_manifest;
			Version = i_version;
			RootPath = i_rootPath ?? string.Empty;
		}
	}

	public static class ModEnableState
	{
		private const string Prefix = "CaptivityReloaded.ModEnabled.";
		public static bool IsEnabled(string i_packId)
		{
			return i_packId == "core" || !PlayerPrefs.HasKey(Prefix + i_packId) || PlayerPrefs.GetInt(Prefix + i_packId) != 0;
		}
		public static void SetEnabled(string i_packId, bool i_enabled)
		{
			if (string.IsNullOrEmpty(i_packId) || i_packId == "core") return;
			PlayerPrefs.SetInt(Prefix + i_packId, i_enabled ? 1 : 0);
			PlayerPrefs.Save();
		}
	}

	public static class ModContentUnlockState
	{
		private const string Prefix = "CaptivityReloaded.ContentUnlocked.";
		public static bool IsUnlocked(ContentId i_id) { return PlayerPrefs.GetInt(Prefix + i_id, 0) != 0; }
		public static void Unlock(ContentId i_id)
		{
			PlayerPrefs.SetInt(Prefix + i_id, 1);
			PlayerPrefs.Save();
		}
	}
}
