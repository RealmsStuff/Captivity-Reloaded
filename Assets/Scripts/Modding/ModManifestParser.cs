using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	public sealed class ManifestLoadResult
	{
		public ModManifest Manifest { get; internal set; }
		public SemanticVersion Version { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModManifestParser
	{
		public const int SupportedSchemaVersion = 1;
		public const int SupportedModApiVersion = 1;

		public static ManifestLoadResult Parse(string i_json, string i_source, bool i_isCore)
		{
			ManifestLoadResult result = new ManifestLoadResult();
			if (string.IsNullOrWhiteSpace(i_json))
			{
				result.Report.Add(ValidationSeverity.Error, "manifest.empty", "Manifest JSON is empty.", i_source);
				return result;
			}

			try
			{
				JsonSerializerSettings settings = new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				};
				result.Manifest = JsonConvert.DeserializeObject<ModManifest>(i_json, settings);
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "manifest.json", exception.Message, i_source);
				return result;
			}

			if (result.Manifest == null)
			{
				result.Report.Add(ValidationSeverity.Error, "manifest.null", "Manifest resolved to null.", i_source);
				return result;
			}

			Validate(result, i_source, i_isCore);
			return result;
		}

		private static void Validate(ManifestLoadResult io_result, string i_source, bool i_isCore)
		{
			ModManifest manifest = io_result.Manifest;
			if (manifest.SchemaVersion != SupportedSchemaVersion)
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.schema-version", "Unsupported schemaVersion " + manifest.SchemaVersion + ".", i_source);
			}
			if (manifest.ModApiVersion != SupportedModApiVersion)
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.api-version", "Unsupported modApiVersion " + manifest.ModApiVersion + ".", i_source);
			}
			if (!ContentId.IsValidNamespace(manifest.Id))
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.id", "Pack ID is invalid.", i_source);
			}
			else if (i_isCore && manifest.Id != "core")
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.core-id", "The packaged Core manifest must use ID 'core'.", i_source);
			}
			else if (!i_isCore && manifest.Id == "core")
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.reserved-id", "External packs cannot use the reserved ID 'core'.", i_source);
			}

			if (string.IsNullOrWhiteSpace(manifest.DisplayName))
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.display-name", "displayName is required.", i_source);
			}
			if (!SemanticVersion.TryParse(manifest.Version, out SemanticVersion version))
			{
				io_result.Report.Add(ValidationSeverity.Error, "manifest.version", "Version is not valid semantic versioning.", i_source);
			}
			else
			{
				io_result.Version = version;
			}

			ValidateDependencies(manifest, io_result.Report, i_source);
			ValidateOrdering(manifest, io_result.Report, i_source);
			ValidateContentRoots(manifest, io_result.Report, i_source);
			ValidatePreviewImages(manifest, io_result.Report, i_source);
			ValidateAssetBundles(manifest, io_result.Report, i_source);
		}

		private static void ValidateAssetBundles(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.AssetBundles == null) { i_manifest.AssetBundles = new List<ModAssetBundlePayload>(); return; }
			if (i_manifest.AssetBundles.Count > 3)
				io_report.Add(ValidationSeverity.Error, "manifest.bundle-count", "assetBundles may contain at most one payload per supported platform.", i_source);
			HashSet<string> platforms = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ModAssetBundlePayload payload in i_manifest.AssetBundles)
			{
				if (payload == null)
				{
					io_report.Add(ValidationSeverity.Error, "manifest.bundle", "assetBundles cannot contain null entries.", i_source);
					continue;
				}
				bool supported = payload.Platform == "windows" || payload.Platform == "android" || payload.Platform == "webgl";
				if (!supported || !platforms.Add(payload.Platform))
					io_report.Add(ValidationSeverity.Error, "manifest.bundle-platform", "Bundle platforms must be unique: windows, android, or webgl.", i_source);
				string prefix = "bundles/" + payload.Platform + "/";
				if (!ModPath.IsSafeRelativePath(payload.Path) || !payload.Path.StartsWith(prefix, StringComparison.Ordinal)
					|| !payload.Path.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase) || !paths.Add(payload.Path))
					io_report.Add(ValidationSeverity.Error, "manifest.bundle-path", "Bundle paths must be unique .bundle files below " + prefix, i_source);
				if (payload.SizeBytes < 1 || payload.SizeBytes > ModArchiveValidator.MaximumBundleBytes)
					io_report.Add(ValidationSeverity.Error, "manifest.bundle-size", "Bundle size is outside the supported range.", i_source);
				if (!IsSha256(payload.Sha256))
					io_report.Add(ValidationSeverity.Error, "manifest.bundle-sha256", "Bundle SHA-256 must contain exactly 64 hexadecimal characters.", i_source);
			}
		}

		private static bool IsSha256(string i_value)
		{
			if (i_value == null || i_value.Length != 64) return false;
			foreach (char value in i_value)
				if (!((value >= '0' && value <= '9') || (value >= 'a' && value <= 'f') || (value >= 'A' && value <= 'F'))) return false;
			return true;
		}

		private static void ValidatePreviewImages(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.PreviewImages == null) { i_manifest.PreviewImages = new List<string>(); return; }
			if (i_manifest.PreviewImages.Count > 8)
				io_report.Add(ValidationSeverity.Error, "manifest.preview-count", "previewImages may contain at most 8 PNG files.", i_source);
			HashSet<string> paths = new HashSet<string>(StringComparer.Ordinal);
			foreach (string path in i_manifest.PreviewImages)
				if (!ModPath.IsSafeRelativePath(path) || !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || !paths.Add(path))
					io_report.Add(ValidationSeverity.Error, "manifest.preview-path", "previewImages entries must be unique safe relative PNG paths: " + (path ?? "<null>"), i_source);
		}

		private static void ValidateOrdering(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.Priority < -100000 || i_manifest.Priority > 100000)
				io_report.Add(ValidationSeverity.Error, "manifest.priority", "priority must be between -100000 and 100000.", i_source);
			ValidatePackIds(i_manifest.LoadAfter, i_manifest.Id, "load-after", io_report, i_source);
			ValidatePackIds(i_manifest.LoadBefore, i_manifest.Id, "load-before", io_report, i_source);
			ValidatePackIds(i_manifest.Conflicts, i_manifest.Id, "conflicts", io_report, i_source);
			if (i_manifest.Id != "core" && i_manifest.Conflicts != null && i_manifest.Conflicts.Contains("core"))
				io_report.Add(ValidationSeverity.Error, "manifest.conflicts-core", "External packs cannot conflict with required Core.", i_source);
			if (i_manifest.Overrides == null) i_manifest.Overrides = new List<string>();
			HashSet<string> overrides = new HashSet<string>(StringComparer.Ordinal);
			foreach (string value in i_manifest.Overrides)
				if (!ContentId.TryParse(value, out _) || !overrides.Add(value))
					io_report.Add(ValidationSeverity.Error, "manifest.overrides", "overrides entries must be unique public content or asset-slot IDs: " + (value ?? "<null>"), i_source);
		}

		private static void ValidatePackIds(List<string> io_values, string i_ownId, string i_field,
			ValidationReport io_report, string i_source)
		{
			if (io_values == null) return;
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (string id in io_values)
				if (!ContentId.IsValidNamespace(id) || id == i_ownId || !ids.Add(id))
					io_report.Add(ValidationSeverity.Error, "manifest." + i_field, i_field + " entries must be unique pack IDs other than this pack: " + (id ?? "<null>"), i_source);
		}

		private static void ValidateDependencies(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.Dependencies == null)
			{
				i_manifest.Dependencies = new List<ModDependency>();
			}
			if (i_manifest.OptionalDependencies == null) i_manifest.OptionalDependencies = new List<ModDependency>();
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			ValidateDependencyList(i_manifest.Dependencies, i_manifest, ids, "dependency", io_report, i_source);
			ValidateDependencyList(i_manifest.OptionalDependencies, i_manifest, ids, "optional-dependency", io_report, i_source);
		}

		private static void ValidateDependencyList(IEnumerable<ModDependency> i_dependencies, ModManifest i_manifest,
			HashSet<string> io_ids, string i_field, ValidationReport io_report, string i_source)
		{
			foreach (ModDependency dependency in i_dependencies)
			{
				if (dependency == null || !ContentId.IsValidNamespace(dependency.Id))
				{
					io_report.Add(ValidationSeverity.Error, "manifest." + i_field + "-id", "A " + i_field + " has an invalid pack ID.", i_source);
					continue;
				}
				if (dependency.Id == i_manifest.Id)
				{
					io_report.Add(ValidationSeverity.Error, "manifest.self-dependency", "A pack cannot depend on itself.", i_source);
				}
				if (!io_ids.Add(dependency.Id))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.duplicate-dependency", "Dependency '" + dependency.Id + "' is declared more than once across required and optional dependencies.", i_source);
				}
				if (!VersionRange.TryParse(dependency.Version, out _))
				{
					io_report.Add(ValidationSeverity.Error, "manifest." + i_field + "-version", "Dependency '" + dependency.Id + "' has an invalid version range.", i_source);
				}
			}
		}

		private static void ValidateContentRoots(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.ContentRoots == null || i_manifest.ContentRoots.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "manifest.content-roots", "At least one content root is required.", i_source);
				return;
			}
			HashSet<string> roots = new HashSet<string>(StringComparer.Ordinal);
			foreach (string root in i_manifest.ContentRoots)
			{
				if (!ModPath.IsSafeRelativePath(root))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.content-root-path", "Content root must be a safe relative path: " + (root ?? "<null>"), i_source);
				}
				else if (!roots.Add(root))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.duplicate-content-root", "Content root is declared more than once: " + root, i_source);
				}
			}
		}
	}

	public static class ModPath
	{
		public static bool IsSafeRelativePath(string i_path)
		{
			if (string.IsNullOrWhiteSpace(i_path) || System.IO.Path.IsPathRooted(i_path) || i_path.IndexOf('\\') >= 0 || i_path.IndexOf(':') >= 0)
			{
				return false;
			}
			string[] segments = i_path.Split('/');
			foreach (string segment in segments)
			{
				if (string.IsNullOrEmpty(segment) || segment == "." || segment == "..") return false;
			}
			return true;
		}
	}
}
