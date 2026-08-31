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
			ValidateContentRoots(manifest, io_result.Report, i_source);
		}

		private static void ValidateDependencies(ModManifest i_manifest, ValidationReport io_report, string i_source)
		{
			if (i_manifest.Dependencies == null)
			{
				i_manifest.Dependencies = new List<ModDependency>();
				return;
			}
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (ModDependency dependency in i_manifest.Dependencies)
			{
				if (dependency == null || !ContentId.IsValidNamespace(dependency.Id))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.dependency-id", "A dependency has an invalid pack ID.", i_source);
					continue;
				}
				if (dependency.Id == i_manifest.Id)
				{
					io_report.Add(ValidationSeverity.Error, "manifest.self-dependency", "A pack cannot depend on itself.", i_source);
				}
				if (!ids.Add(dependency.Id))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.duplicate-dependency", "Dependency '" + dependency.Id + "' is declared more than once.", i_source);
				}
				if (!VersionRange.TryParse(dependency.Version, out _))
				{
					io_report.Add(ValidationSeverity.Error, "manifest.dependency-version", "Dependency '" + dependency.Id + "' has an invalid version range.", i_source);
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
