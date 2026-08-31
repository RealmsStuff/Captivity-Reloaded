using System;
using System.Collections.Generic;
using System.IO;

namespace CaptivityReloaded.Modding
{
	public sealed class ModDiscoveryResult
	{
		public List<ModPack> Packs { get; } = new List<ModPack>();
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
					continue;
				}

				try
				{
					ManifestLoadResult manifest = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, i_isCore: false);
					result.Report.Merge(manifest.Report);
					if (manifest.Report.IsValid)
					{
						result.Packs.Add(new ModPack(manifest.Manifest, manifest.Version, directory));
					}
				}
				catch (Exception exception)
				{
					result.Report.Add(ValidationSeverity.Error, "discovery.read-manifest", exception.Message, manifestPath);
				}
			}
			return result;
		}
	}
}
