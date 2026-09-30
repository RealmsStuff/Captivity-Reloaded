using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class ModAssetBundleRegistry
	{
		private static readonly Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>(StringComparer.Ordinal);
		private static readonly List<AssetBundle> LoadedBundles = new List<AssetBundle>();

		public static void Initialize(IEnumerable<ModPack> i_packs, ValidationReport io_report)
		{
			foreach (AssetBundle bundle in LoadedBundles) if (bundle != null) bundle.Unload(false);
			LoadedBundles.Clear();
			Prefabs.Clear();
			string platform = CurrentPlatform();
			if (platform == null) return;
			foreach (ModPack pack in i_packs ?? new ModPack[0])
			{
				if (pack?.Manifest == null || pack.Manifest.Id == "core") continue;
				ModAssetBundlePayload payload = pack.Manifest.AssetBundles.Find(item => item != null && item.Platform == platform);
				if (payload == null) continue;
				Load(pack, payload, io_report);
			}
		}

		public static bool TryGetPrefab(string i_id, out GameObject o_prefab)
		{
			return Prefabs.TryGetValue(i_id ?? string.Empty, out o_prefab);
		}

		private static void Load(ModPack i_pack, ModAssetBundlePayload i_payload, ValidationReport io_report)
		{
			string path = Path.GetFullPath(Path.Combine(i_pack.RootPath, i_payload.Path));
			try
			{
				if (!IsInside(path, i_pack.RootPath) || !File.Exists(path))
				{
					Warning(io_report, "bundle.missing", "Platform AssetBundle is missing; portable data fallbacks will be used.", path);
					return;
				}
				FileInfo file = new FileInfo(path);
				if (file.Length != i_payload.SizeBytes || !string.Equals(Sha256(path), i_payload.Sha256, StringComparison.OrdinalIgnoreCase))
				{
					Error(io_report, "bundle.integrity", "Platform AssetBundle does not match its generated manifest metadata.", path);
					return;
				}
				AssetBundle bundle = Application.platform == RuntimePlatform.WebGLPlayer
					? AssetBundle.LoadFromMemory(File.ReadAllBytes(path)) : AssetBundle.LoadFromFile(path);
				if (bundle == null)
				{
					Warning(io_report, "bundle.load", "Unity could not load this platform AssetBundle; portable data fallbacks will be used.", path);
					return;
				}
				LoadedBundles.Add(bundle);
				foreach (GameObject prefab in bundle.LoadAllAssets<GameObject>()) Register(i_pack, prefab, io_report, path);
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				Warning(io_report, "bundle.read", exception.Message + " Portable data fallbacks will be used.", path);
			}
		}

		private static void Register(ModPack i_pack, GameObject i_prefab, ValidationReport io_report, string i_source)
		{
			if (i_prefab == null) return;
			CapmodPrefabDescriptor descriptor = i_prefab.GetComponent<CapmodPrefabDescriptor>();
			if (descriptor == null || !ContentId.TryParse(descriptor.Id, out ContentId id)
				|| id.Namespace != i_pack.Manifest.Id || !id.Path.StartsWith("prefab/", StringComparison.Ordinal))
			{
				Error(io_report, "bundle.prefab-id", "A bundled prefab has no valid root descriptor owned by its pack: " + i_prefab.name, i_source);
				return;
			}
			foreach (Component component in i_prefab.GetComponentsInChildren<Component>(true))
			{
				if (component == null)
				{
					Error(io_report, "bundle.missing-script", "A bundled prefab contains a missing script: " + descriptor.Id, i_source);
					return;
				}
				if (component is MonoBehaviour && !(component is CapmodPrefabDescriptor))
				{
					Error(io_report, "bundle.script", "A bundled prefab contains an unapproved behavior: " + component.GetType().FullName, i_source);
					return;
				}
			}
			if (Prefabs.ContainsKey(descriptor.Id))
			{
				Error(io_report, "bundle.duplicate-prefab", "Duplicate bundled prefab ID: " + descriptor.Id, i_source);
				return;
			}
			Prefabs.Add(descriptor.Id, i_prefab);
		}

		private static string CurrentPlatform()
		{
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer: return "windows";
				case RuntimePlatform.Android: return "android";
				case RuntimePlatform.WebGLPlayer: return "webgl";
				default: return null;
			}
		}

		private static bool IsInside(string i_path, string i_root)
		{
			string root = Path.GetFullPath(i_root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				+ Path.DirectorySeparatorChar;
			return i_path.StartsWith(root, StringComparison.OrdinalIgnoreCase);
		}

		private static string Sha256(string i_path)
		{
			using (FileStream stream = File.OpenRead(i_path))
			using (SHA256 sha = SHA256.Create())
				return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
		}

		private static void Warning(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Warning, i_code, i_message, i_source);
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}
}
