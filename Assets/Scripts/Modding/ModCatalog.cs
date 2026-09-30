using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModCatalogDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("packs", Required = Required.Always)] public List<ModCatalogPack> Packs { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModCatalogPack
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("authors", Required = Required.Always)] public List<string> Authors { get; set; }
		[JsonProperty("summary", Required = Required.Always)] public string Summary { get; set; }
		[JsonProperty("sourceRepository", Required = Required.Always)] public string SourceRepository { get; set; }
		[JsonProperty("tags")] public List<string> Tags { get; set; }
		[JsonProperty("contentWarnings")] public List<string> ContentWarnings { get; set; }
		[JsonProperty("previewImages")] public List<string> PreviewImages { get; set; }
		[JsonProperty("versions", Required = Required.Always)] public List<ModCatalogVersion> Versions { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModCatalogVersion
	{
		[JsonProperty("version", Required = Required.Always)] public string Version { get; set; }
		[JsonProperty("modApiVersion", Required = Required.Always)] public int ModApiVersion { get; set; }
		[JsonProperty("gameVersion", Required = Required.Always)] public string GameVersion { get; set; }
		[JsonProperty("download", Required = Required.Always)] public string Download { get; set; }
		[JsonProperty("sha256", Required = Required.Always)] public string Sha256 { get; set; }
		[JsonProperty("sizeBytes", Required = Required.Always)] public long SizeBytes { get; set; }
		[JsonProperty("dependencies")] public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();
		[JsonProperty("conflicts")] public List<string> Conflicts { get; set; } = new List<string>();
	}

	public sealed class ModCatalogParseResult
	{
		public ModCatalogDocument Catalog { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModCatalogPlatformPolicy
	{
		public const long DesktopAndAndroidBatchBytes = 512L * 1024L * 1024L;
		public const long WebGlArchiveBytes = 32L * 1024L * 1024L;
		public const long WebGlBatchBytes = 64L * 1024L * 1024L;

		public static long MaximumArchiveBytes(RuntimePlatform i_platform)
		{
			return i_platform == RuntimePlatform.WebGLPlayer ? WebGlArchiveBytes : ModCatalogParser.MaxArchiveBytes;
		}

		public static long MaximumBatchBytes(RuntimePlatform i_platform)
		{
			return i_platform == RuntimePlatform.WebGLPlayer ? WebGlBatchBytes : DesktopAndAndroidBatchBytes;
		}

		public static bool UsesMemoryDownload(RuntimePlatform i_platform)
		{
			return i_platform == RuntimePlatform.WebGLPlayer;
		}
	}

	public static class ModCatalogCache
	{
		public static bool TryRead(string i_path, out ModCatalogDocument o_catalog, out ValidationReport o_report)
		{
			o_catalog = null;
			o_report = new ValidationReport();
			try
			{
				if (string.IsNullOrWhiteSpace(i_path) || !File.Exists(i_path)) return false;
				ModCatalogParseResult parsed = ModCatalogParser.Parse(File.ReadAllText(i_path), i_path);
				o_report.Merge(parsed.Report);
				o_catalog = parsed.Catalog;
				return parsed.Report.IsValid && parsed.Catalog != null;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				o_report.Add(ValidationSeverity.Error, "catalog.cache-read", exception.Message, i_path);
				return false;
			}
		}

		public static ValidationReport WriteValidated(string i_path, string i_json)
		{
			ModCatalogParseResult parsed = ModCatalogParser.Parse(i_json, i_path);
			ValidationReport report = new ValidationReport();
			report.Merge(parsed.Report);
			if (!report.IsValid) return report;
			if (string.IsNullOrWhiteSpace(i_path))
			{
				report.Add(ValidationSeverity.Error, "catalog.cache-write", "Catalog cache path is missing.", i_path);
				return report;
			}
			string path = null;
			string temporary = null;
			string previous = null;
			try
			{
				path = Path.GetFullPath(i_path);
				string directory = Path.GetDirectoryName(path);
				if (string.IsNullOrEmpty(directory)) throw new InvalidDataException("Catalog cache has no parent directory.");
				Directory.CreateDirectory(directory);
				temporary = path + ".new-" + Guid.NewGuid().ToString("N");
				File.WriteAllText(temporary, i_json);
				if (File.Exists(path))
				{
					previous = path + ".old-" + Guid.NewGuid().ToString("N");
					File.Move(path, previous);
				}
				File.Move(temporary, path);
				temporary = null;
				if (previous != null) { File.Delete(previous); previous = null; }
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is ArgumentException || exception is NotSupportedException)
			{
				report.Add(ValidationSeverity.Error, "catalog.cache-write", exception.Message, i_path);
				if (previous != null && File.Exists(previous) && (path == null || !File.Exists(path)))
				{
					try { File.Move(previous, path ?? i_path); previous = null; }
					catch (Exception recovery) { report.Add(ValidationSeverity.Error, "catalog.cache-recovery", recovery.Message, previous); }
				}
			}
			finally
			{
				try { if (temporary != null && File.Exists(temporary)) File.Delete(temporary); }
				catch (Exception) { }
				try { if (previous != null && File.Exists(previous) && path != null && File.Exists(path)) File.Delete(previous); }
				catch (Exception) { }
			}
			return report;
		}
	}

	// A catalog is untrusted network data. Reject the whole document before any listing is shown.
	public static class ModCatalogParser
	{
		public const int SupportedSchemaVersion = 1;
		public const int MaxJsonCharacters = 262144;
		public const long MaxArchiveBytes = 256L * 1024L * 1024L;
		public const int MaxPreviewBytes = 8 * 1024 * 1024;

		public static ModCatalogParseResult Parse(string i_json, string i_source)
		{
			ModCatalogParseResult result = new ModCatalogParseResult();
			if (string.IsNullOrWhiteSpace(i_json) || i_json.Length > MaxJsonCharacters)
			{
				Error(result, "catalog.size", "Catalog is empty or exceeds 256 KiB of text.", i_source);
				return result;
			}
			try
			{
				result.Catalog = JsonConvert.DeserializeObject<ModCatalogDocument>(i_json,
					new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error, MaxDepth = 16 });
			}
			catch (JsonException exception)
			{
				Error(result, "catalog.json", exception.Message, i_source);
				return result;
			}
			if (result.Catalog == null || result.Catalog.SchemaVersion != SupportedSchemaVersion || result.Catalog.Packs == null)
			{
				Error(result, "catalog.schema", "Catalog must use schemaVersion 1 and contain a packs array.", i_source);
				result.Catalog = null;
				return result;
			}
			if (result.Catalog.Packs.Count > 500)
				Error(result, "catalog.pack-count", "Catalog may contain at most 500 packs.", i_source);
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (ModCatalogPack pack in result.Catalog.Packs)
			{
				if (pack == null) { Error(result, "catalog.pack", "Pack entry is null.", i_source); continue; }
				if (!ContentId.IsValidNamespace(pack.Id) || pack.Id == "core" || !ids.Add(pack.Id))
					Error(result, "catalog.id", "Pack ID must be unique, valid, and not 'core'.", i_source);
				if (!Text(pack.DisplayName, 120) || !Text(pack.Summary, 4000))
					Error(result, "catalog.text", "Pack displayName or summary is missing or too long.", i_source);
				if (!StringList(pack.Authors, 1, 10, 120) || !StringList(pack.Tags, 0, 16, 40) ||
					!StringList(pack.ContentWarnings, 0, 16, 120))
					Error(result, "catalog.metadata", "Pack authors, tags, or content warnings are invalid.", i_source);
				if (!GithubRepositoryUrl(pack.SourceRepository))
					Error(result, "catalog.repository", "sourceRepository must be an HTTPS GitHub repository URL.", i_source);
				if (pack.PreviewImages != null)
				{
					if (pack.PreviewImages.Count > 8)
						Error(result, "catalog.previews", "A catalog pack may contain at most eight preview images.", i_source);
					else
					{
						HashSet<string> previews = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						foreach (string preview in pack.PreviewImages)
							if (!GithubAssetUrl(preview, pack.SourceRepository, ".png") || !previews.Add(preview))
								Error(result, "catalog.previews", "Preview images must be unique HTTPS PNG assets from the pack's GitHub repository.", i_source);
					}
				}
				if (pack.Versions == null || pack.Versions.Count == 0 || pack.Versions.Count > 16)
				{
					Error(result, "catalog.versions", "Pack must have between 1 and 16 versions.", i_source);
					continue;
				}
				HashSet<string> versions = new HashSet<string>(StringComparer.Ordinal);
				foreach (ModCatalogVersion version in pack.Versions)
				{
					if (version == null) { Error(result, "catalog.version", "Version entry is null.", i_source); continue; }
					if (!SemanticVersion.TryParse(version.Version, out _) || !versions.Add(version.Version))
						Error(result, "catalog.version", "Version must be unique semantic versioning.", i_source);
					if (version.ModApiVersion < 1 || version.ModApiVersion > 1000 || !GameVersion(version.GameVersion))
						Error(result, "catalog.compatibility", "modApiVersion or gameVersion is invalid.", i_source);
					if (!GithubArchiveUrl(version.Download))
						Error(result, "catalog.download", "download must be an HTTPS GitHub-hosted ZIP or .capmod URL.", i_source);
					else if (!GithubAssetUrl(version.Download, pack.SourceRepository, ".zip", CapmodPackageBuilder.Extension))
						Error(result, "catalog.download-repository", "download must belong to sourceRepository.", i_source);
					if (!Sha256(version.Sha256) || version.SizeBytes < 1 || version.SizeBytes > MaxArchiveBytes)
						Error(result, "catalog.archive", "sha256 or sizeBytes is invalid.", i_source);
					if (version.Dependencies == null || version.Dependencies.Count > 32 || version.Conflicts == null
						|| version.Conflicts.Count > 32)
						Error(result, "catalog.dependencies", "Dependency or conflict list is missing or too long.", i_source);
					else
					{
						HashSet<string> requirements = new HashSet<string>(StringComparer.Ordinal);
						foreach (ModDependency dependency in version.Dependencies)
							if (dependency == null || !ContentId.IsValidNamespace(dependency.Id) || dependency.Id == pack.Id
								|| !VersionRange.TryParse(dependency.Version, out _) || !requirements.Add(dependency.Id))
								Error(result, "catalog.dependencies", "Version has an invalid, duplicate, or self-referencing requirement.", i_source);
						HashSet<string> conflicts = new HashSet<string>(StringComparer.Ordinal);
						foreach (string conflict in version.Conflicts)
							if (!ContentId.IsValidNamespace(conflict) || conflict == pack.Id || !conflicts.Add(conflict))
								Error(result, "catalog.conflicts", "Version has an invalid, duplicate, or self-referencing conflict.", i_source);
					}
				}
			}
			if (!result.Report.IsValid) result.Catalog = null;
			return result;
		}

		private static bool Text(string i_value, int i_max) => !string.IsNullOrWhiteSpace(i_value) && i_value.Length <= i_max;
		private static bool StringList(List<string> i_values, int i_min, int i_max, int i_textMax)
		{
			if (i_values == null) return i_min == 0;
			if (i_values.Count < i_min || i_values.Count > i_max) return false;
			foreach (string value in i_values) if (!Text(value, i_textMax)) return false;
			return true;
		}
		private static bool GameVersion(string i_value)
		{
			if (i_value == "*") return true;
			if (i_value != null && i_value.StartsWith(">=", StringComparison.Ordinal)) i_value = i_value.Substring(2);
			return SemanticVersion.TryParse(i_value, out _);
		}
		private static bool GithubRepositoryUrl(string i_value)
		{
			if (!Uri.TryCreate(i_value, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps ||
				!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase) || !uri.IsDefaultPort ||
				!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || !string.IsNullOrEmpty(uri.UserInfo)) return false;
			string[] parts = uri.AbsolutePath.Trim('/').Split('/');
			return parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0;
		}
		private static bool GithubArchiveUrl(string i_value)
		{
			return GithubAssetUrl(i_value, null, ".zip", CapmodPackageBuilder.Extension);
		}
		private static bool GithubAssetUrl(string i_value, string i_repository, params string[] i_extensions)
		{
			if (!Uri.TryCreate(i_value, UriKind.Absolute, out Uri uri) || uri.Scheme != Uri.UriSchemeHttps
				|| !uri.IsDefaultPort || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)
				|| !string.IsNullOrEmpty(uri.UserInfo)) return false;
			string[] parts = uri.AbsolutePath.Trim('/').Split('/');
			string owner;
			string repository;
			if (string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase))
			{
				if (parts.Length != 6 || parts[2] != "releases" || parts[3] != "download" || parts[4].Length == 0)
					return false;
				owner = parts[0];
				repository = parts[1];
			}
			else if (string.Equals(uri.Host, "raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
			{
				if (parts.Length < 5 || parts[2].Length == 0 || parts[3].Length == 0)
					return false;
				owner = parts[0];
				repository = parts[1];
			}
			else return false;
			if (owner.Length == 0 || repository.Length == 0 ||
				!i_extensions.Any(extension => parts[parts.Length - 1].EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
				return false;
			if (string.IsNullOrEmpty(i_repository)) return true;
			if (!Uri.TryCreate(i_repository, UriKind.Absolute, out Uri repositoryUri)) return false;
			string[] repositoryParts = repositoryUri.AbsolutePath.Trim('/').Split('/');
			return repositoryParts.Length == 2
				&& string.Equals(owner, repositoryParts[0], StringComparison.OrdinalIgnoreCase)
				&& string.Equals(repository, repositoryParts[1], StringComparison.OrdinalIgnoreCase);
		}
		private static bool Sha256(string i_value)
		{
			if (i_value == null || i_value.Length != 64) return false;
			foreach (char digit in i_value)
				if (!((digit >= '0' && digit <= '9') || (digit >= 'a' && digit <= 'f') || (digit >= 'A' && digit <= 'F'))) return false;
			return true;
		}
		private static void Error(ModCatalogParseResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}

	/// <summary>Preflight for untrusted release archives. This method never extracts or changes an installed pack.</summary>
	public static class ModArchiveValidator
	{
		public const int MaximumFiles = 4096;
		public const long MaximumExtractedBytes = 512L * 1024L * 1024L;
		public const long MaximumEntryBytes = 64L * 1024L * 1024L;
		public const long MaximumBundleBytes = 256L * 1024L * 1024L;
		private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			".json", ".tsj", ".tx", ".png", ".ogg", ".wav", ".md", ".txt", ".bundle"
		};

		internal static bool IsAllowedDataFile(string i_path)
		{
			return !string.IsNullOrEmpty(i_path) && AllowedExtensions.Contains(Path.GetExtension(i_path));
		}

		public static ValidationReport Validate(string i_archivePath, ModCatalogPack i_pack, ModCatalogVersion i_version)
		{
			ValidationReport report = new ValidationReport();
			if (i_pack == null || i_version == null || string.IsNullOrWhiteSpace(i_archivePath))
			{
				Error(report, "archive.input", "A catalog pack, version, and archive path are required.", i_archivePath);
				return report;
			}
			try
			{
				FileInfo file = new FileInfo(i_archivePath);
				if (!file.Exists || file.Length < 1 || file.Length != i_version.SizeBytes
					|| file.Length > ModCatalogParser.MaxArchiveBytes)
				{
					Error(report, "archive.size", "Downloaded ZIP size does not match the catalog or exceeds the limit.", i_archivePath);
					return report;
				}
				using (FileStream stream = file.OpenRead())
				{
					using (SHA256 sha = SHA256.Create())
					{
						string digest = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
						if (!string.Equals(digest, i_version.Sha256, StringComparison.OrdinalIgnoreCase))
						{
							Error(report, "archive.sha256", "Downloaded ZIP SHA-256 does not match the catalog.", i_archivePath);
							return report;
						}
					}
					stream.Position = 0;
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
						ValidateEntries(zip, i_pack, i_version, report, i_archivePath);
				}
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is NotSupportedException || exception is ArgumentException)
			{
				Error(report, "archive.read", exception.Message, i_archivePath);
			}
			return report;
		}

		private static void ValidateEntries(ZipArchive i_zip, ModCatalogPack i_pack, ModCatalogVersion i_version,
			ValidationReport io_report, string i_source)
		{
			if (i_zip.Entries.Count > MaximumFiles)
				Error(io_report, "archive.file-count", "ZIP contains too many entries.", i_source);
			long total = 0;
			int manifests = 0;
			ModManifest parsedManifest = null;
			HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, ZipArchiveEntry> bundles = new Dictionary<string, ZipArchiveEntry>(StringComparer.OrdinalIgnoreCase);
			foreach (ZipArchiveEntry entry in i_zip.Entries)
			{
				string path = entry.FullName;
				bool directory = path.EndsWith("/", StringComparison.Ordinal);
				if (!SafeEntryPath(path, directory) || !paths.Add(path.TrimEnd('/')))
				{
					Error(io_report, "archive.path", "ZIP contains an unsafe or duplicate path.", i_source);
					continue;
				}
				int unixType = (entry.ExternalAttributes >> 16) & 0xF000;
				if ((unixType != 0 && unixType != 0x8000 && unixType != 0x4000)
					|| (entry.ExternalAttributes & (int)FileAttributes.ReparsePoint) != 0)
					Error(io_report, "archive.link", "ZIP links and special files are not allowed.", i_source);
				if (directory) continue;
				if (!IsAllowedDataFile(path))
					Error(io_report, "archive.extension", "Archive contains an unsupported file type: " + path, i_source);
				if (path.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase)) bundles[path] = entry;
				if (entry.Length < 0 || entry.Length > EntryLimit(path) || total > MaximumExtractedBytes - entry.Length)
					Error(io_report, "archive.extracted-size", "ZIP would exceed extracted-size limits.", i_source);
				else total += entry.Length;
				if (!string.Equals(path, "manifest.json", StringComparison.OrdinalIgnoreCase)) continue;
				manifests++;
				if (entry.Length < 1 || entry.Length > ModFileLimits.MaximumManifestBytes)
				{
					Error(io_report, "archive.manifest-size", "manifest.json exceeds its size limit.", i_source);
					continue;
				}
				using (StreamReader reader = new StreamReader(entry.Open()))
				{
					char[] buffer = new char[(int)ModFileLimits.MaximumManifestBytes + 1];
					int count = 0;
					while (count < buffer.Length)
					{
						int read = reader.Read(buffer, count, buffer.Length - count);
						if (read == 0) break;
						count += read;
					}
					if (count >= buffer.Length)
					{
						Error(io_report, "archive.manifest-size", "Decompressed manifest.json exceeds its size limit.", i_source);
						continue;
					}
					ManifestLoadResult manifest = ModManifestParser.Parse(new string(buffer, 0, count), path, false);
					io_report.Merge(manifest.Report);
					if (manifest.Report.IsValid) parsedManifest = manifest.Manifest;
					if (manifest.Report.IsValid && (manifest.Manifest.Id != i_pack.Id
						|| manifest.Manifest.Version != i_version.Version
						|| manifest.Manifest.ModApiVersion != i_version.ModApiVersion))
						Error(io_report, "archive.identity", "ZIP manifest does not match the catalog ID, version, or Mod API.", i_source);
				}
			}
			if (manifests != 1)
				Error(io_report, "archive.manifest", "ZIP must contain exactly one root manifest.json.", i_source);
			if (parsedManifest != null) ValidateBundlePayloads(parsedManifest, bundles, io_report, i_source);
		}

		private static void ValidateBundlePayloads(ModManifest i_manifest, Dictionary<string, ZipArchiveEntry> i_bundles,
			ValidationReport io_report, string i_source)
		{
			HashSet<string> declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ModAssetBundlePayload payload in i_manifest.AssetBundles)
			{
				if (payload == null || string.IsNullOrEmpty(payload.Path)) continue;
				declared.Add(payload.Path);
				if (!i_bundles.TryGetValue(payload.Path, out ZipArchiveEntry entry))
				{
					Error(io_report, "archive.bundle-missing", "A declared AssetBundle is missing: " + payload.Path, i_source);
					continue;
				}
				if (entry.Length != payload.SizeBytes)
					Error(io_report, "archive.bundle-size", "AssetBundle size does not match its manifest entry: " + payload.Path, i_source);
				using (Stream stream = entry.Open())
				using (SHA256 sha = SHA256.Create())
				{
					string digest = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
					if (!string.Equals(digest, payload.Sha256, StringComparison.OrdinalIgnoreCase))
						Error(io_report, "archive.bundle-sha256", "AssetBundle SHA-256 does not match its manifest entry: " + payload.Path, i_source);
				}
			}
			foreach (string path in i_bundles.Keys)
				if (!declared.Contains(path)) Error(io_report, "archive.bundle-undeclared", "Archive contains an undeclared AssetBundle: " + path, i_source);
		}

		internal static bool SafeEntryPath(string i_path, bool i_directory)
		{
			if (string.IsNullOrEmpty(i_path) || i_path[0] == '/' || i_path.IndexOf('\\') >= 0
				|| i_path.IndexOf(':') >= 0 || i_path.IndexOf('\0') >= 0
				|| i_path.IndexOfAny(new[] { '<', '>', '"', '|', '?', '*' }) >= 0) return false;
			string[] parts = i_path.TrimEnd('/').Split('/');
			if (parts.Length == 0) return false;
			foreach (string part in parts)
				if (part.Length == 0 || part == "." || part == ".." || part.EndsWith(".", StringComparison.Ordinal)
					|| part.EndsWith(" ", StringComparison.Ordinal) || IsWindowsDeviceName(part)) return false;
			return !i_directory || i_path.EndsWith("/", StringComparison.Ordinal);
		}

		internal static long EntryLimit(string i_path)
		{
			if (string.Equals(i_path, "manifest.json", StringComparison.OrdinalIgnoreCase))
				return ModFileLimits.MaximumManifestBytes;
			string extension = Path.GetExtension(i_path);
			if (string.Equals(extension, ".png", StringComparison.OrdinalIgnoreCase))
				return RuntimePngAssetLoader.MaximumPngBytes;
			if (string.Equals(extension, ".wav", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(extension, ".ogg", StringComparison.OrdinalIgnoreCase)) return MaximumEntryBytes;
			if (string.Equals(extension, ".bundle", StringComparison.OrdinalIgnoreCase)) return MaximumBundleBytes;
			return ModFileLimits.MaximumContentDefinitionBytes;
		}

		private static bool IsWindowsDeviceName(string i_part)
		{
			string stem = i_part.Split('.')[0].ToUpperInvariant();
			if (stem == "CON" || stem == "PRN" || stem == "AUX" || stem == "NUL") return true;
			if (stem.Length == 4 && (stem.StartsWith("COM", StringComparison.Ordinal)
				|| stem.StartsWith("LPT", StringComparison.Ordinal)) && stem[3] >= '1' && stem[3] <= '9') return true;
			return false;
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}

	/// <summary>First-install only. Extracts into a private staging directory and never replaces an existing pack.</summary>
	public static class ModArchiveInstaller
	{
		public static ValidationReport Install(string i_archivePath, string i_modsDirectory,
			ModCatalogPack i_pack, ModCatalogVersion i_version, IReadOnlyList<ModPack> i_availablePacks = null,
			bool i_recordIntegrity = true)
		{
			ValidationReport report = ModArchiveValidator.Validate(i_archivePath, i_pack, i_version);
			if (!report.IsValid) return report;
			if (string.IsNullOrWhiteSpace(i_modsDirectory) || !ContentId.IsValidNamespace(i_pack.Id))
			{
				Error(report, "install.target", "Mods directory or catalog pack ID is invalid.", i_modsDirectory);
				return report;
			}
			string staging = null;
			try
			{
				string mods = Path.GetFullPath(i_modsDirectory);
				DirectoryInfo parent = Directory.GetParent(mods);
				if (parent == null || string.Equals(mods, Path.GetPathRoot(mods), StringComparison.OrdinalIgnoreCase))
				{
					Error(report, "install.target", "Mods directory cannot be a filesystem root.", mods);
					return report;
				}
				string target = Path.GetFullPath(Path.Combine(mods, i_pack.Id));
				if (!AssetPatchDiscovery.IsInside(target, mods) || Directory.Exists(target) || File.Exists(target))
				{
					Error(report, "install.exists", "An installed pack already uses this ID. Updates require a separate rollback flow.", target);
					return report;
				}
				staging = Path.GetFullPath(Path.Combine(parent.FullName, ".mod-staging-" + Guid.NewGuid().ToString("N")));
				if (!AssetPatchDiscovery.IsInside(staging, parent.FullName) || Directory.Exists(staging))
				{
					Error(report, "install.staging", "Could not allocate a safe staging directory.", staging);
					return report;
				}
				Directory.CreateDirectory(staging);
				using (FileStream stream = new FileStream(i_archivePath, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					using (SHA256 sha = SHA256.Create())
					{
						string digest = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
						if (stream.Length != i_version.SizeBytes || !string.Equals(digest, i_version.Sha256, StringComparison.OrdinalIgnoreCase))
						{
							Error(report, "install.archive-changed", "Downloaded ZIP changed after preflight.", i_archivePath);
							return report;
						}
					}
					stream.Position = 0;
					using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read, true))
						Extract(zip, staging, report);
				}
				if (!report.IsValid) return report;
				string manifestPath = Path.Combine(staging, "manifest.json");
				ManifestLoadResult manifest = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false);
				report.Merge(manifest.Report);
				if (!report.IsValid || manifest.Manifest.Id != i_pack.Id || manifest.Manifest.Version != i_version.Version)
				{
					Error(report, "install.identity", "Extracted manifest does not match the catalog.", manifestPath);
					return report;
				}
				if (!MatchesCatalogMetadata(manifest.Manifest, i_version))
				{
					Error(report, "install.metadata", "Archive dependencies or conflicts differ from the catalog listing.", manifestPath);
					return report;
				}
				foreach (ModDependency dependency in manifest.Manifest.Dependencies)
				{
					ModPack installedDependency = null;
					foreach (ModPack loaded in i_availablePacks ?? ModLoaderRuntime.LoadedPacks)
						if (loaded.Manifest.Id == dependency.Id) { installedDependency = loaded; break; }
					if (installedDependency == null || !VersionRange.TryParse(dependency.Version, out VersionRange range)
						|| !range.Contains(installedDependency.Version))
						Error(report, "install.dependency", "Required dependency is not enabled at a compatible version: "
							+ dependency.Id + " " + dependency.Version, manifestPath);
				}
				if (!report.IsValid) return report;
				ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[]
				{
					new ModPack(manifest.Manifest, manifest.Version, staging)
				});
				report.Merge(content.Report);
				if (!report.IsValid) return report;
				Directory.CreateDirectory(mods);
				if (Directory.Exists(target) || File.Exists(target))
				{
					Error(report, "install.exists", "Another pack was installed with this ID during validation.", target);
					return report;
				}
				Directory.Move(staging, target);
				staging = null;
				if (i_recordIntegrity)
					report.Merge(ModInstallIntegrity.Record(mods, i_pack.Id, i_version.Version, i_version.Sha256));
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is ArgumentException || exception is NotSupportedException)
			{
				Error(report, "install.failed", exception.Message, i_archivePath);
			}
			finally
			{
				if (staging != null && Directory.Exists(staging))
				{
					try
					{
						string parentPath = Directory.GetParent(staging)?.FullName;
						if (parentPath == null || !AssetPatchDiscovery.IsInside(staging, parentPath)
							|| (new DirectoryInfo(staging).Attributes & FileAttributes.ReparsePoint) != 0)
							throw new InvalidDataException("Staging cleanup target is no longer a safe directory.");
						Directory.Delete(staging, true);
					}
					catch (Exception exception) { Error(report, "install.cleanup", exception.Message, staging); }
				}
			}
			return report;
		}

		private static bool MatchesCatalogMetadata(ModManifest i_manifest, ModCatalogVersion i_version)
		{
			List<ModDependency> expected = i_version.Dependencies ?? new List<ModDependency>();
			List<ModDependency> actual = i_manifest.Dependencies ?? new List<ModDependency>();
			if (expected.Count != actual.Count) return false;
			foreach (ModDependency dependency in expected)
			{
				bool found = false;
				foreach (ModDependency item in actual)
					if (item.Id == dependency.Id && item.Version == dependency.Version) { found = true; break; }
				if (!found) return false;
			}
			List<string> expectedConflicts = i_version.Conflicts ?? new List<string>();
			List<string> actualConflicts = i_manifest.Conflicts ?? new List<string>();
			if (expectedConflicts.Count != actualConflicts.Count) return false;
			foreach (string conflict in expectedConflicts) if (!actualConflicts.Contains(conflict)) return false;
			return true;
		}

		private static void Extract(ZipArchive i_zip, string i_staging, ValidationReport io_report)
		{
			long total = 0;
			byte[] buffer = new byte[65536];
			foreach (ZipArchiveEntry entry in i_zip.Entries)
			{
				bool directory = entry.FullName.EndsWith("/", StringComparison.Ordinal);
				if (!ModArchiveValidator.SafeEntryPath(entry.FullName, directory))
				{
					Error(io_report, "install.path", "ZIP entry path became unsafe during extraction.", entry.FullName);
					return;
				}
				string path = Path.GetFullPath(Path.Combine(i_staging,
					entry.FullName.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar)));
				if (!AssetPatchDiscovery.IsInside(path, i_staging))
				{
					Error(io_report, "install.path", "ZIP entry escapes staging.", entry.FullName);
					return;
				}
				if (directory) { Directory.CreateDirectory(path); continue; }
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				long written = 0;
				using (Stream input = entry.Open())
				using (FileStream output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				{
					int read;
					while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
					{
						written += read;
						total += read;
						if (written > ModArchiveValidator.EntryLimit(entry.FullName)
							|| total > ModArchiveValidator.MaximumExtractedBytes)
						{
							Error(io_report, "install.extracted-size", "ZIP exceeded extraction limits.", entry.FullName);
							return;
						}
						output.Write(buffer, 0, read);
					}
				}
				if (written != entry.Length)
				{
					Error(io_report, "install.extracted-size", "ZIP entry length changed during extraction.", entry.FullName);
					return;
				}
			}
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}

	/// <summary>Recoverable catalog updates and removal. Recovery folders live beside Mods, never inside it.</summary>
	public static class ModInstallRecovery
	{
		public static string BackupVersion(string i_modsDirectory, string i_id)
		{
			if (!TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed)) return null;
			return ReadVersion(backup, i_id);
		}

		public static bool HasRemoved(string i_modsDirectory, string i_id)
		{
			return TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed)
				&& SafeDirectory(removed);
		}

		public static string RemovedVersion(string i_modsDirectory, string i_id)
		{
			if (!TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed)) return null;
			return ReadVersion(removed, i_id);
		}

		public static IEnumerable<string> RemovedIds(string i_modsDirectory)
		{
			if (string.IsNullOrWhiteSpace(i_modsDirectory)) yield break;
			string removedRoot;
			try
			{
				string mods = Path.GetFullPath(i_modsDirectory);
				DirectoryInfo parent = Directory.GetParent(mods);
				if (parent == null || mods == Path.GetPathRoot(mods)) yield break;
				removedRoot = Path.Combine(parent.FullName, ".mod-removed");
			}
			catch (Exception exception) when (RecoverableException(exception)) { yield break; }
			if (!SafeDirectory(removedRoot)) yield break;
			string[] directories;
			try { directories = Directory.GetDirectories(removedRoot); }
			catch (Exception exception) when (RecoverableException(exception)) { yield break; }
			foreach (string directory in directories)
			{
				string id = Path.GetFileName(directory);
				if (ContentId.IsValidNamespace(id) && ReadVersion(directory, id) != null) yield return id;
			}
		}

		public static ValidationReport RecoverInterruptedUpdates(string i_modsDirectory)
		{
			ValidationReport report = new ValidationReport();
			if (string.IsNullOrWhiteSpace(i_modsDirectory)) return report;
			string backupRoot;
			try
			{
				string mods = Path.GetFullPath(i_modsDirectory);
				DirectoryInfo parent = Directory.GetParent(mods);
				if (parent == null || mods == Path.GetPathRoot(mods)) return report;
				backupRoot = Path.Combine(parent.FullName, ".mod-backups");
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ Error(report, "recovery.path", exception.Message, i_modsDirectory); return report; }
			if (!SafeDirectory(backupRoot)) return report;
			string[] directories;
			try { directories = Directory.GetDirectories(backupRoot); }
			catch (Exception exception) when (RecoverableException(exception))
			{ Error(report, "recovery.scan", exception.Message, backupRoot); return report; }
			foreach (string directory in directories)
			{
				string id = Path.GetFileName(directory);
				if (!TryPaths(i_modsDirectory, id, out string mods, out string target, out string backup, out string removed)
					|| !string.Equals(directory, backup, StringComparison.OrdinalIgnoreCase)
					|| ReadVersion(backup, id) == null || Directory.Exists(target) || File.Exists(target)
					|| Directory.Exists(removed) || File.Exists(removed)) continue;
				try
				{
					Directory.CreateDirectory(mods);
					Directory.Move(backup, target);
					report.Add(ValidationSeverity.Warning, "recovery.restored",
						"Recovered the previous version after an interrupted update: " + id, target);
				}
				catch (Exception exception) when (RecoverableException(exception))
				{ Error(report, "recovery.failed", exception.Message, backup); }
			}
			return report;
		}

		public static ValidationReport Update(string i_archivePath, string i_modsDirectory,
			ModCatalogPack i_pack, ModCatalogVersion i_version, bool i_confirmPreserveModified = false,
			bool i_allowSameVersion = false)
		{
			ValidationReport report = new ValidationReport();
			if (i_pack == null || i_version == null || !TryPaths(i_modsDirectory, i_pack.Id,
				out string mods, out string target, out string backup, out string removed))
			{
				Error(report, "update.target", "Mods directory or pack ID is invalid.", i_modsDirectory);
				return report;
			}
			string current = ReadVersion(target, i_pack.Id);
			if (current == null || !SemanticVersion.TryParse(current, out SemanticVersion oldVersion)
				|| !SemanticVersion.TryParse(i_version.Version, out SemanticVersion newVersion)
				|| newVersion.CompareTo(oldVersion) < (i_allowSameVersion ? 0 : 1))
			{
				Error(report, "update.version", i_allowSameVersion
					? "Installed pack is missing, invalid, or newer than this local archive."
					: "Installed pack is missing, invalid, or not older than this release.", target);
				return report;
			}
			if (File.Exists(backup) || (Directory.Exists(backup) && ReadVersion(backup, i_pack.Id) == null))
			{
				Error(report, "update.backup-invalid", "The previous-version folder is unsafe or invalid.", backup);
				return report;
			}
			ModIntegrityCheck integrity = ModInstallIntegrity.Check(mods, i_pack.Id);
			if (integrity.NeedsConfirmation && !i_confirmPreserveModified)
			{
				Error(report, "update.modified", integrity.Message + " Explicit confirmation is required before update.", target);
				return report;
			}
			string stagingMods = Path.Combine(Directory.GetParent(mods).FullName, ".mod-update-" + Guid.NewGuid().ToString("N"));
			string stagedTarget = Path.Combine(stagingMods, i_pack.Id);
			string archivedBackup = backup + ".older-" + Guid.NewGuid().ToString("N");
			bool movedOld = false;
			bool movedPreviousBackup = false;
			try
			{
				report.Merge(ModArchiveInstaller.Install(i_archivePath, stagingMods, i_pack, i_version, null, false));
				if (!report.IsValid) return report;
				if (ReadVersion(target, i_pack.Id) != current || !SafeDirectory(stagedTarget)
					|| File.Exists(backup) || (Directory.Exists(backup) && ReadVersion(backup, i_pack.Id) == null))
				{
					Error(report, "update.changed", "Installation changed while the replacement was being validated.", target);
					return report;
				}
				ModIntegrityCheck commitIntegrity = ModInstallIntegrity.Check(mods, i_pack.Id);
				if (commitIntegrity.NeedsConfirmation && !i_confirmPreserveModified)
				{
					Error(report, "update.modified", "Local files changed during validation; confirm the update again.", target);
					return report;
				}
				if (commitIntegrity.NeedsConfirmation)
				{
					report.Merge(ModInstallIntegrity.PreserveModifiedCopy(mods, i_pack.Id));
					if (!report.IsValid) return report;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(backup));
				if (Directory.Exists(backup))
				{
					Directory.Move(backup, archivedBackup);
					movedPreviousBackup = true;
				}
				Directory.Move(target, backup);
				movedOld = true;
				Directory.Move(stagedTarget, target);
				movedOld = false;
				movedPreviousBackup = false;
				report.Merge(ModInstallIntegrity.Record(mods, i_pack.Id, i_version.Version, i_version.Sha256));
			}
			catch (Exception exception) when (RecoverableException(exception))
			{
				Error(report, "update.failed", exception.Message, target);
			}
			finally
			{
				if (movedOld && !Directory.Exists(target) && SafeDirectory(backup))
				{
					try { Directory.Move(backup, target); }
					catch (Exception exception) { Error(report, "update.recovery", "Previous version remains at " + backup + ": " + exception.Message, backup); }
				}
				if (movedPreviousBackup && !Directory.Exists(backup) && SafeDirectory(archivedBackup))
				{
					try { Directory.Move(archivedBackup, backup); }
					catch (Exception exception) { Error(report, "update.recovery", "Earlier backup remains at " + archivedBackup + ": " + exception.Message, archivedBackup); }
				}
				CleanStaging(stagingMods, stagedTarget, report);
			}
			return report;
		}

		public static ValidationReport Rollback(string i_modsDirectory, string i_id)
		{
			ValidationReport report = new ValidationReport();
			if (!TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed))
			{ Error(report, "rollback.target", "Mods directory or pack ID is invalid.", i_modsDirectory); return report; }
			if (!SafeDirectory(target) || ReadVersion(backup, i_id) == null)
			{ Error(report, "rollback.missing", "Installed pack or saved previous version is missing or invalid.", backup); return report; }
			string swap = Path.Combine(Directory.GetParent(mods).FullName, ".mod-swap-" + Guid.NewGuid().ToString("N"));
			try
			{
				Directory.Move(target, swap);
				try
				{
					Directory.Move(backup, target);
					Directory.Move(swap, backup);
				}
				catch
				{
					if (!Directory.Exists(target) && SafeDirectory(swap)) Directory.Move(swap, target);
					throw;
				}
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ Error(report, "rollback.failed", exception.Message + (Directory.Exists(swap) ? " Recovery copy: " + swap : ""), target); }
			return report;
		}

		public static ValidationReport Uninstall(string i_modsDirectory, string i_id)
		{
			ValidationReport report = new ValidationReport();
			if (!TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed))
			{ Error(report, "remove.target", "Mods directory or pack ID is invalid.", i_modsDirectory); return report; }
			if (!SafeDirectory(target) || Directory.Exists(removed) || File.Exists(removed))
			{ Error(report, "remove.exists", "Pack is missing or an earlier removed copy already exists.", target); return report; }
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(removed));
				Directory.Move(target, removed);
			}
			catch (Exception exception) when (RecoverableException(exception)) { Error(report, "remove.failed", exception.Message, target); }
			return report;
		}

		public static ValidationReport Restore(string i_modsDirectory, string i_id)
		{
			ValidationReport report = new ValidationReport();
			if (!TryPaths(i_modsDirectory, i_id, out string mods, out string target, out string backup, out string removed))
			{ Error(report, "restore.target", "Mods directory or pack ID is invalid.", i_modsDirectory); return report; }
			if (ReadVersion(removed, i_id) == null || Directory.Exists(target) || File.Exists(target))
			{ Error(report, "restore.exists", "Removed copy is invalid or another pack already uses this ID.", removed); return report; }
			try
			{
				Directory.CreateDirectory(mods);
				Directory.Move(removed, target);
			}
			catch (Exception exception) when (RecoverableException(exception)) { Error(report, "restore.failed", exception.Message, removed); }
			return report;
		}

		internal static bool TryPaths(string i_modsDirectory, string i_id, out string o_mods,
			out string o_target, out string o_backup, out string o_removed)
		{
			o_mods = o_target = o_backup = o_removed = null;
			if (string.IsNullOrWhiteSpace(i_modsDirectory) || i_id == "core" || !ContentId.IsValidNamespace(i_id)) return false;
			try
			{
				o_mods = Path.GetFullPath(i_modsDirectory);
				DirectoryInfo parent = Directory.GetParent(o_mods);
				if (parent == null || o_mods == Path.GetPathRoot(o_mods)) return false;
				o_target = Path.GetFullPath(Path.Combine(o_mods, i_id));
				o_backup = Path.GetFullPath(Path.Combine(parent.FullName, ".mod-backups", i_id));
				o_removed = Path.GetFullPath(Path.Combine(parent.FullName, ".mod-removed", i_id));
				return AssetPatchDiscovery.IsInside(o_target, o_mods)
					&& AssetPatchDiscovery.IsInside(o_backup, parent.FullName)
					&& AssetPatchDiscovery.IsInside(o_removed, parent.FullName)
					&& !UnsafeContainer(o_mods) && !UnsafeContainer(Path.GetDirectoryName(o_backup))
					&& !UnsafeContainer(Path.GetDirectoryName(o_removed));
			}
			catch (Exception exception) when (RecoverableException(exception)) { return false; }
		}

		internal static string ReadVersion(string i_directory, string i_id)
		{
			if (!SafeDirectory(i_directory)) return null;
			try
			{
				string path = Path.Combine(i_directory, "manifest.json");
				if (!File.Exists(path) || (new FileInfo(path).Attributes & FileAttributes.ReparsePoint) != 0
					|| new FileInfo(path).Length > ModFileLimits.MaximumManifestBytes) return null;
				ManifestLoadResult result = ModManifestParser.Parse(File.ReadAllText(path), path, false);
				return result.Report.IsValid && result.Manifest?.Id == i_id ? result.Manifest.Version : null;
			}
			catch (Exception exception) when (RecoverableException(exception)) { return null; }
		}

		internal static bool SafeDirectory(string i_path)
		{
			try { return Directory.Exists(i_path) && (new DirectoryInfo(i_path).Attributes & FileAttributes.ReparsePoint) == 0; }
			catch (Exception exception) when (RecoverableException(exception)) { return false; }
		}

		private static bool UnsafeContainer(string i_path)
		{
			return Directory.Exists(i_path) && !SafeDirectory(i_path);
		}

		private static void CleanStaging(string i_mods, string i_pack, ValidationReport io_report)
		{
			try
			{
				if (SafeDirectory(i_pack)) Directory.Delete(i_pack, true);
				if (SafeDirectory(i_mods) && Directory.GetFileSystemEntries(i_mods).Length == 0) Directory.Delete(i_mods);
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ Error(io_report, "update.cleanup", "Staging remains at " + i_mods + ": " + exception.Message, i_mods); }
		}

		private static bool RecoverableException(Exception i_exception)
		{
			return i_exception is IOException || i_exception is UnauthorizedAccessException
				|| i_exception is InvalidDataException || i_exception is ArgumentException || i_exception is NotSupportedException;
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message, string i_source)
		{
			io_report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}
}
