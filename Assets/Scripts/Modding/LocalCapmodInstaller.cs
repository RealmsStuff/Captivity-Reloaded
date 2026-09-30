using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public sealed class LocalCapmodArchive
	{
		public string Path { get; internal set; }
		public ModManifest Manifest { get; internal set; }
		public ModCatalogPack Pack { get; internal set; }
		public ModCatalogVersion Version { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class LocalCapmodInstaller
	{
		public static List<string> FindArchives(string i_modsDirectory)
		{
			List<string> paths = new List<string>();
			if (string.IsNullOrWhiteSpace(i_modsDirectory) || !Directory.Exists(i_modsDirectory)) return paths;
			foreach (string path in Directory.GetFiles(i_modsDirectory, "*", SearchOption.TopDirectoryOnly))
				if (string.Equals(System.IO.Path.GetExtension(path), CapmodPackageBuilder.Extension, StringComparison.OrdinalIgnoreCase))
					paths.Add(path);
			paths.Sort(StringComparer.OrdinalIgnoreCase);
			if (paths.Count > 128) paths.RemoveRange(128, paths.Count - 128);
			return paths;
		}

		public static string FindInstalledFolder(string i_modsDirectory, string i_id)
		{
			if (string.IsNullOrWhiteSpace(i_modsDirectory) || !Directory.Exists(i_modsDirectory)
				|| !ContentId.IsValidNamespace(i_id)) return null;
			ModDiscoveryResult discovery = ModDiscovery.Discover(i_modsDirectory);
			foreach (ModPack pack in discovery.Packs)
				if (pack.Manifest.Id == i_id) return pack.RootPath;
			return null;
		}

		public static LocalCapmodArchive Inspect(string i_path)
		{
			LocalCapmodArchive result = new LocalCapmodArchive { Path = i_path };
			try
			{
				FileInfo file = new FileInfo(i_path);
				if (!file.Exists || !string.Equals(file.Extension, CapmodPackageBuilder.Extension, StringComparison.OrdinalIgnoreCase)
					|| file.Length < 1 || file.Length > ModCatalogParser.MaxArchiveBytes
					|| (file.Attributes & FileAttributes.ReparsePoint) != 0)
				{
					Error(result.Report, "local.archive", "Choose a regular .capmod file within the archive size limit.", i_path);
					return result;
				}
				string digest;
				using (FileStream stream = file.OpenRead())
				using (SHA256 sha = SHA256.Create())
					digest = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
				using (FileStream stream = file.OpenRead())
				using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
				{
					ZipArchiveEntry manifestEntry = null;
					foreach (ZipArchiveEntry entry in zip.Entries)
						if (string.Equals(entry.FullName, "manifest.json", StringComparison.OrdinalIgnoreCase))
						{
							if (manifestEntry != null) { Error(result.Report, "local.manifest", "Archive contains multiple root manifests.", i_path); return result; }
							manifestEntry = entry;
						}
					if (manifestEntry == null || manifestEntry.Length < 1 || manifestEntry.Length > ModFileLimits.MaximumManifestBytes)
					{
						Error(result.Report, "local.manifest", "Archive needs one root manifest.json within the size limit.", i_path);
						return result;
					}
					using (StreamReader reader = new StreamReader(manifestEntry.Open()))
					{
						char[] buffer = new char[(int)ModFileLimits.MaximumManifestBytes + 1];
						int count = 0, read;
						while (count < buffer.Length && (read = reader.Read(buffer, count, buffer.Length - count)) > 0) count += read;
						if (count == buffer.Length) { Error(result.Report, "local.manifest", "Manifest exceeds the size limit.", i_path); return result; }
						ManifestLoadResult parsed = ModManifestParser.Parse(new string(buffer, 0, count), i_path, false);
						result.Report.Merge(parsed.Report);
						if (!result.Report.IsValid) return result;
						result.Manifest = parsed.Manifest;
					}
				}
				result.Pack = new ModCatalogPack { Id = result.Manifest.Id, DisplayName = result.Manifest.DisplayName };
				result.Version = new ModCatalogVersion
				{
					Version = result.Manifest.Version, ModApiVersion = result.Manifest.ModApiVersion,
					SizeBytes = file.Length, Sha256 = digest, Dependencies = result.Manifest.Dependencies,
					Conflicts = result.Manifest.Conflicts
				};
				result.Report.Merge(ModArchiveValidator.Validate(i_path, result.Pack, result.Version));
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is ArgumentException || exception is NotSupportedException)
			{
				Error(result.Report, "local.read", exception.Message, i_path);
			}
			return result;
		}

		public static ValidationReport Install(LocalCapmodArchive i_archive, string i_modsDirectory,
			bool i_confirmReplacement, IReadOnlyList<ModPack> i_availablePacks = null)
		{
			ValidationReport report = new ValidationReport();
			if (i_archive == null || !i_archive.Report.IsValid || i_archive.Manifest == null)
			{
				Error(report, "local.invalid", "The selected .capmod did not pass archive validation.", i_archive?.Path);
				return report;
			}
			if (string.IsNullOrWhiteSpace(i_modsDirectory))
			{
				Error(report, "local.storage", "Mod storage is not available on this platform.", i_modsDirectory);
				return report;
			}
			string modsPath = System.IO.Path.GetFullPath(i_modsDirectory);
			if (!string.Equals(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(i_archive.Path)),
				modsPath, StringComparison.OrdinalIgnoreCase))
			{
				Error(report, "local.location", "Place the .capmod directly inside Mods before installing it.", i_archive.Path);
				return report;
			}
			string platform = Application.platform == RuntimePlatform.WebGLPlayer ? "webgl"
				: Application.platform == RuntimePlatform.Android ? "android" : "windows";
			if (i_archive.Manifest.AssetBundles.Count > 0
				&& !i_archive.Manifest.AssetBundles.Exists(bundle => bundle.Platform == platform))
			{
				Error(report, "local.platform", "This .capmod has no " + platform + " AssetBundle.", i_archive.Path);
				return report;
			}
			if (i_archive.Version.SizeBytes > ModCatalogPlatformPolicy.MaximumArchiveBytes(Application.platform))
			{
				Error(report, "local.size", "This .capmod exceeds the platform archive limit.", i_archive.Path);
				return report;
			}
			foreach (ModDependency dependency in i_archive.Manifest.Dependencies)
			{
				bool satisfied = false;
				foreach (ModPack pack in i_availablePacks ?? ModLoaderRuntime.LoadedPacks)
					if (pack.Manifest.Id == dependency.Id && VersionRange.TryParse(dependency.Version, out VersionRange range)
						&& range.Contains(pack.Version)) { satisfied = true; break; }
				if (!satisfied) Error(report, "local.dependency", "Required dependency is not enabled: "
					+ dependency.Id + " " + dependency.Version, i_archive.Path);
			}
			foreach (ModPack pack in i_availablePacks ?? ModLoaderRuntime.LoadedPacks)
				if (pack.Manifest.Id != i_archive.Manifest.Id
					&& ((i_archive.Manifest.Conflicts != null && i_archive.Manifest.Conflicts.Contains(pack.Manifest.Id))
					|| (pack.Manifest.Conflicts != null && pack.Manifest.Conflicts.Contains(i_archive.Manifest.Id))))
					Error(report, "local.conflict", "Conflicts with enabled mod: " + pack.Manifest.Id, i_archive.Path);
			if (!report.IsValid) return report;
			string target = System.IO.Path.Combine(i_modsDirectory, i_archive.Manifest.Id);
			string existing = FindInstalledFolder(i_modsDirectory, i_archive.Manifest.Id);
			bool replacing = existing != null || Directory.Exists(target);
			if (replacing && !i_confirmReplacement)
			{
				Error(report, "local.confirm", "Confirm replacement; the previous mod will be kept for rollback.", target);
				return report;
			}
			bool legacyName = replacing && existing != null
				&& !string.Equals(System.IO.Path.GetFullPath(existing), System.IO.Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase);
			string legacyManifest = null;
			if (legacyName && Directory.Exists(target))
			{
				Error(report, "local.duplicate", "Two installed folders use the same mod ID; resolve the duplicate before replacing.", target);
				return report;
			}
			try
			{
				if (legacyName) legacyManifest = File.ReadAllText(System.IO.Path.Combine(existing, "manifest.json"));
				if (legacyName) Directory.Move(existing, target);
				report.Merge(replacing
					? ModInstallRecovery.Update(i_archive.Path, i_modsDirectory, i_archive.Pack, i_archive.Version, true, true)
					: ModArchiveInstaller.Install(i_archive.Path, i_modsDirectory, i_archive.Pack, i_archive.Version, i_availablePacks));
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				Error(report, "local.replace", exception.Message, existing ?? target);
			}
			if (legacyName && !report.IsValid)
			{
				try
				{
					if (Directory.Exists(target) && !Directory.Exists(existing)
						&& File.Exists(System.IO.Path.Combine(target, "manifest.json"))
						&& File.ReadAllText(System.IO.Path.Combine(target, "manifest.json")) == legacyManifest)
						Directory.Move(target, existing);
				}
				catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
					|| exception is ArgumentException || exception is NotSupportedException)
				{
					Error(report, "local.recovery", "Old mod remains at " + target + ": " + exception.Message, target);
				}
			}
			if (!report.IsValid) return report;
			try
			{
				string parent = Directory.GetParent(System.IO.Path.GetFullPath(i_modsDirectory)).FullName;
				string imported = System.IO.Path.Combine(parent, ".mod-imported");
				if (Directory.Exists(imported) && (new DirectoryInfo(imported).Attributes & FileAttributes.ReparsePoint) != 0)
					throw new InvalidDataException("Imported-archive storage is a link.");
				Directory.CreateDirectory(imported);
				string destination = System.IO.Path.Combine(imported, System.IO.Path.GetFileNameWithoutExtension(i_archive.Path)
					+ "-" + i_archive.Version.Sha256.Substring(0, 12) + CapmodPackageBuilder.Extension);
				if (File.Exists(destination)) destination = System.IO.Path.Combine(imported,
					System.IO.Path.GetFileNameWithoutExtension(i_archive.Path) + "-" + Guid.NewGuid().ToString("N")
					+ CapmodPackageBuilder.Extension);
				File.Move(i_archive.Path, destination);
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is ArgumentException || exception is NotSupportedException)
			{
				report.Add(ValidationSeverity.Warning, "local.archive-retained",
					"Mod installed, but the source archive remains in Mods: " + exception.Message, i_archive.Path);
			}
			return report;
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}
}
