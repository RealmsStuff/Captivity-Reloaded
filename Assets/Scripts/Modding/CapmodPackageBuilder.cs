using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace CaptivityReloaded.Modding
{
	public sealed class CapmodPackageResult
	{
		public ModManifest Manifest { get; internal set; }
		public string OutputPath { get; internal set; }
		public string Sha256 { get; internal set; }
		public long PackageBytes { get; internal set; }
		public int FileCount { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	/// <summary>
	/// Validates a loose data mod and packages it as the public .capmod container.
	/// A .capmod is deliberately a ZIP archive so the runtime installer can retain its
	/// existing staging, integrity, rollback, and traversal protections.
	/// </summary>
	public static class CapmodPackageBuilder
	{
		public const string Extension = ".capmod";
		private static readonly DateTimeOffset ReproducibleTimestamp =
			new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public static CapmodPackageResult ValidateSource(string i_sourceDirectory)
		{
			CapmodPackageResult result = new CapmodPackageResult();
			string root;
			try
			{
				root = string.IsNullOrWhiteSpace(i_sourceDirectory) ? string.Empty : Path.GetFullPath(i_sourceDirectory);
			}
			catch (Exception exception)
			{
				Error(result, "capmod.source", exception.Message, i_sourceDirectory);
				return result;
			}
			if (root.Length == 0 || !Directory.Exists(root))
			{
				Error(result, "capmod.source", "Choose an existing mod source directory.", root);
				return result;
			}

			string manifestPath = Path.Combine(root, "manifest.json");
			if (!File.Exists(manifestPath))
			{
				Error(result, "capmod.manifest", "The source directory must contain manifest.json at its root.", root);
				return result;
			}

			try
			{
				FileInfo manifestFile = new FileInfo(manifestPath);
				if (manifestFile.Length < 1 || manifestFile.Length > ModFileLimits.MaximumManifestBytes)
				{
					Error(result, "capmod.manifest-size", "manifest.json must be between 1 byte and 256 KiB.", manifestPath);
					return result;
				}
				ManifestLoadResult loaded = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false);
				result.Report.Merge(loaded.Report);
				result.Manifest = loaded.Manifest;
				if (!loaded.Report.IsValid) return result;

				foreach (string contentRoot in loaded.Manifest.ContentRoots)
				{
					string directory = Path.GetFullPath(Path.Combine(root, contentRoot));
					if (!IsInside(directory, root) || !Directory.Exists(directory))
						Error(result, "capmod.content-root", "A declared content root does not exist inside the mod: " + contentRoot,
							manifestPath);
				}
				foreach (string preview in loaded.Manifest.PreviewImages)
				{
					string previewPath = Path.GetFullPath(Path.Combine(root, preview));
					if (!IsInside(previewPath, root) || !File.Exists(previewPath))
						Error(result, "capmod.preview", "A declared preview image is missing: " + preview, manifestPath);
				}

				string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
				Array.Sort(files, StringComparer.Ordinal);
				if (files.Length == 0 || files.Length > ModArchiveValidator.MaximumFiles)
					Error(result, "capmod.file-count", "A mod must contain files and may contain at most "
						+ ModArchiveValidator.MaximumFiles + " files.", root);
				long total = 0;
				foreach (string file in files)
				{
					string relative = RelativePath(root, file);
					FileInfo info = new FileInfo(file);
					if ((info.Attributes & FileAttributes.ReparsePoint) != 0)
						Error(result, "capmod.link", "Links and reparse points cannot be packaged: " + relative, file);
					if (!ModArchiveValidator.SafeEntryPath(relative, false))
						Error(result, "capmod.path", "The mod contains an unsafe path: " + relative, file);
					if (!ModArchiveValidator.IsAllowedDataFile(relative))
						Error(result, "capmod.extension", "The mod contains an unsupported file type: " + relative, file);
					if (info.Length < 0 || info.Length > ModArchiveValidator.EntryLimit(relative)
						|| total > ModArchiveValidator.MaximumExtractedBytes - info.Length)
						Error(result, "capmod.size", "A file or the complete mod exceeds its safe extracted-size limit: " + relative, file);
					else total += info.Length;
				}

				if (result.Report.IsValid)
				{
					ValidateDeclaredBundles(root, loaded.Manifest, result);
					ModPack pack = new ModPack(loaded.Manifest, loaded.Version, root);
					result.Report.Merge(ModContentDiscovery.Discover(new[] { pack }).Report);
				}
				result.FileCount = files.Length;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				Error(result, "capmod.read", exception.Message, root);
			}
			return result;
		}

		private static void ValidateDeclaredBundles(string i_root, ModManifest i_manifest, CapmodPackageResult io_result)
		{
			HashSet<string> declared = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (ModAssetBundlePayload payload in i_manifest.AssetBundles)
			{
				if (payload == null || string.IsNullOrEmpty(payload.Path)) continue;
				declared.Add(payload.Path);
				string path = Path.GetFullPath(Path.Combine(i_root, payload.Path));
				if (!IsInside(path, i_root) || !File.Exists(path))
				{
					Error(io_result, "capmod.bundle-missing", "A declared AssetBundle is missing: " + payload.Path, i_root);
					continue;
				}
				FileInfo file = new FileInfo(path);
				if (file.Length != payload.SizeBytes)
					Error(io_result, "capmod.bundle-size", "AssetBundle size does not match the manifest: " + payload.Path, path);
				if (!string.Equals(ComputeSha256(path), payload.Sha256, StringComparison.OrdinalIgnoreCase))
					Error(io_result, "capmod.bundle-sha256", "AssetBundle SHA-256 does not match the manifest: " + payload.Path, path);
			}
			string bundleRoot = Path.Combine(i_root, "bundles");
			if (!Directory.Exists(bundleRoot)) return;
			foreach (string path in Directory.GetFiles(bundleRoot, "*.bundle", SearchOption.AllDirectories))
			{
				string relative = RelativePath(i_root, path);
				if (!declared.Contains(relative))
					Error(io_result, "capmod.bundle-undeclared", "The mod contains an undeclared AssetBundle: " + relative, path);
			}
		}

		public static CapmodPackageResult Build(string i_sourceDirectory, string i_outputPath, bool i_overwrite = false)
		{
			CapmodPackageResult result = ValidateSource(i_sourceDirectory);
			if (!result.Report.IsValid) return result;
			string output;
			string root = Path.GetFullPath(i_sourceDirectory);
			try
			{
				output = Path.GetFullPath(i_outputPath ?? string.Empty);
			}
			catch (Exception exception)
			{
				Error(result, "capmod.output", exception.Message, i_outputPath);
				return result;
			}
			if (!string.Equals(Path.GetExtension(output), Extension, StringComparison.OrdinalIgnoreCase))
			{
				Error(result, "capmod.output-extension", "Packaged mods must use the " + Extension + " extension.", output);
				return result;
			}
			if (IsInside(output, root))
			{
				Error(result, "capmod.output-location", "The output file cannot be placed inside its source directory.", output);
				return result;
			}
			if (File.Exists(output) && !i_overwrite)
			{
				Error(result, "capmod.output-exists", "The output file already exists.", output);
				return result;
			}

			string temporary = output + ".tmp-" + Guid.NewGuid().ToString("N");
			try
			{
				string parent = Path.GetDirectoryName(output);
				if (string.IsNullOrEmpty(parent)) throw new InvalidOperationException("The output path has no parent directory.");
				Directory.CreateDirectory(parent);
				string[] files = Directory.GetFiles(root, "*", SearchOption.AllDirectories);
				Array.Sort(files, delegate(string left, string right)
				{
					return string.CompareOrdinal(RelativePath(root, left), RelativePath(root, right));
				});
				using (FileStream stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Create, false))
				{
					foreach (string file in files)
					{
						ZipArchiveEntry entry = zip.CreateEntry(RelativePath(root, file), CompressionLevel.Optimal);
						entry.LastWriteTime = ReproducibleTimestamp;
						using (Stream input = File.OpenRead(file))
						using (Stream destination = entry.Open()) input.CopyTo(destination);
					}
				}

				FileInfo package = new FileInfo(temporary);
				string digest = ComputeSha256(temporary);
				ModCatalogPack catalogPack = new ModCatalogPack { Id = result.Manifest.Id };
				ModCatalogVersion catalogVersion = new ModCatalogVersion
				{
					Version = result.Manifest.Version,
					ModApiVersion = result.Manifest.ModApiVersion,
					SizeBytes = package.Length,
					Sha256 = digest
				};
				result.Report.Merge(ModArchiveValidator.Validate(temporary, catalogPack, catalogVersion));
				if (!result.Report.IsValid) return result;

				File.Copy(temporary, output, i_overwrite);
				result.OutputPath = output;
				result.PackageBytes = package.Length;
				result.Sha256 = digest;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException || exception is InvalidOperationException)
			{
				Error(result, "capmod.build", exception.Message, output);
			}
			finally
			{
				try { if (File.Exists(temporary)) File.Delete(temporary); }
				catch { }
			}
			return result;
		}

		private static string ComputeSha256(string i_path)
		{
			using (FileStream stream = File.OpenRead(i_path))
			using (SHA256 sha = SHA256.Create())
				return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
		}

		private static string RelativePath(string i_root, string i_path)
		{
			return i_path.Substring(i_root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length + 1)
				.Replace(Path.DirectorySeparatorChar, '/');
		}

		private static bool IsInside(string i_path, string i_root)
		{
			string root = i_root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				+ Path.DirectorySeparatorChar;
			return i_path.StartsWith(root, StringComparison.OrdinalIgnoreCase);
		}

		private static void Error(CapmodPackageResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		}
	}
}
