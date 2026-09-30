using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class ModStoragePaths
	{
		public static string GetModsDirectory()
		{
			return GetModsDirectory(Application.platform, Application.dataPath, Application.persistentDataPath);
		}

		public static string GetModsDirectory(RuntimePlatform i_platform, string i_dataPath, string i_persistentDataPath)
		{
			if (i_platform == RuntimePlatform.WindowsEditor || i_platform == RuntimePlatform.WindowsPlayer)
			{
				if (string.IsNullOrWhiteSpace(i_dataPath)) return null;
				string parent = Directory.GetParent(i_dataPath)?.FullName;
				return string.IsNullOrEmpty(parent) ? null : Path.Combine(parent, "Mods");
			}
			if (i_platform == RuntimePlatform.Android || i_platform == RuntimePlatform.WebGLPlayer)
				return string.IsNullOrWhiteSpace(i_persistentDataPath) ? null : Path.Combine(i_persistentDataPath, "Mods");
			return null;
		}
	}

	public static class ModFileLimits
	{
		public const long MaximumManifestBytes = 256L * 1024L;
		public const long MaximumContentDefinitionBytes = 1024L * 1024L;
		public const int MaximumPacks = 1024;
		public const int MaximumDefinitionsPerPack = 4096;
	}

	public sealed class ModDiscoveryResult
	{
		public List<ModPack> Packs { get; } = new List<ModPack>();
		public List<ModPackStatus> InvalidStatuses { get; } = new List<ModPackStatus>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModDiscovery
	{
		public static ModDiscoveryResult Discover(string i_modsDirectory)
		{
			ModDiscoveryResult result = new ModDiscoveryResult();
			if (string.IsNullOrWhiteSpace(i_modsDirectory) || !Directory.Exists(i_modsDirectory))
			{
				result.Report.Add(ValidationSeverity.Info, "discovery.no-directory", "No external Mods directory was found.", i_modsDirectory);
				return result;
			}

			string[] directories;
			try
			{
				directories = Directory.GetDirectories(i_modsDirectory);
				Array.Sort(directories, StringComparer.Ordinal);
				if (directories.Length > ModFileLimits.MaximumPacks)
				{
					result.Report.Add(ValidationSeverity.Error, "discovery.pack-limit",
						"Mods directory contains more than " + ModFileLimits.MaximumPacks + " pack directories.", i_modsDirectory);
					Array.Resize(ref directories, ModFileLimits.MaximumPacks);
				}
			}
			catch (Exception exception)
			{
				result.Report.Add(ValidationSeverity.Error, "discovery.read-directory", exception.Message, i_modsDirectory);
				return result;
			}

			foreach (string directory in directories)
			{
				string manifestPath = System.IO.Path.Combine(directory, "manifest.json");
				if (!File.Exists(manifestPath))
				{
					result.Report.Add(ValidationSeverity.Warning, "discovery.no-manifest", "Mod directory has no manifest.json.", directory);
					result.InvalidStatuses.Add(new ModPackStatus(Path.GetFileName(directory), Path.GetFileName(directory), string.Empty,
						directory, ModPackState.Invalid, "This mod directory has no manifest.json."));
					continue;
				}

				try
				{
					long manifestBytes = new FileInfo(manifestPath).Length;
					if (manifestBytes <= 0 || manifestBytes > ModFileLimits.MaximumManifestBytes)
						throw new InvalidDataException("manifest.json must be between 1 byte and 256 KiB.");
					string json = File.ReadAllText(manifestPath);
					ManifestLoadResult manifest = ModManifestParser.Parse(json, manifestPath, i_isCore: false);
					result.Report.Merge(manifest.Report);
					if (manifest.Report.IsValid)
					{
						result.Packs.Add(new ModPack(manifest.Manifest, manifest.Version, directory));
					}
					else
					{
						ReadIdentity(json, directory, out string id, out string displayName, out string version);
						result.InvalidStatuses.Add(new ModPackStatus(id, displayName, version, directory, ModPackState.Invalid,
							"manifest.json failed validation. Open the details report for the exact errors."));
					}
				}
				catch (Exception exception)
				{
					result.Report.Add(ValidationSeverity.Error, "discovery.read-manifest", exception.Message, manifestPath);
					result.InvalidStatuses.Add(new ModPackStatus(Path.GetFileName(directory), Path.GetFileName(directory), string.Empty,
						directory, ModPackState.Invalid, "manifest.json could not be read: " + exception.Message));
				}
			}
			return result;
		}

		private static void ReadIdentity(string i_json, string i_directory, out string o_id, out string o_displayName, out string o_version)
		{
			o_id = Path.GetFileName(i_directory); o_displayName = o_id; o_version = string.Empty;
			try
			{
				JObject root = JObject.Parse(i_json);
				o_id = (string)root["id"] ?? o_id;
				o_displayName = (string)root["displayName"] ?? o_id;
				o_version = (string)root["version"] ?? string.Empty;
			}
			catch { }
		}
	}
}
