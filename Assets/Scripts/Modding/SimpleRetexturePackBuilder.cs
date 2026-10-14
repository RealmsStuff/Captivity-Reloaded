using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public sealed class SimpleRetextureInput
	{
		public string Slot { get; set; }
		public string SourcePng { get; set; }
	}

	public sealed class SimpleRetextureBuildResult
	{
		public ValidationReport Report { get; } = new ValidationReport();
		public string PatchPath { get; internal set; }
		public int ReplacementCount { get; internal set; }
	}

	public sealed class SimpleRetextureMatchResult
	{
		public List<SimpleRetextureInput> Matches { get; } = new List<SimpleRetextureInput>();
		public List<string> UnmatchedPngs { get; } = new List<string>();
	}

	/// <summary>Creates a complete asset-patch document and keeps its pack manifest authorization in sync.</summary>
	public static class SimpleRetexturePackBuilder
	{
		public static SimpleRetextureMatchResult MatchEnemyPngs(CoreEnemyAssetSlotEnemy i_enemy, IEnumerable<string> i_pngs)
		{
			SimpleRetextureMatchResult result = new SimpleRetextureMatchResult();
			if (i_enemy == null) return result;
			List<string> files = (i_pngs ?? new string[0])
				.Where(i_file => !string.IsNullOrWhiteSpace(i_file) && i_file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(i_file => MatchKey(RemoveDuplicateSuffix(Path.GetFileNameWithoutExtension(i_file))), StringComparer.Ordinal)
				.ThenBy(DuplicateIndex).ThenBy(i_file => i_file, StringComparer.OrdinalIgnoreCase).ToList();
			HashSet<string> usedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> usedSlots = new HashSet<string>(StringComparer.Ordinal);

			foreach (IGrouping<string, CoreEnemyAssetSlotRecord> group in (i_enemy.Slots ?? new List<CoreEnemyAssetSlotRecord>())
				.Where(i_slot => i_slot != null && !string.IsNullOrEmpty(i_slot.SpriteName))
				.GroupBy(i_slot => MatchKey(i_slot.SpriteName), StringComparer.Ordinal))
			{
				List<string> candidates = files.Where(i_file => FileMatches(i_file, group.Key))
					.OrderBy(DuplicateIndex).ThenBy(i_file => i_file, StringComparer.OrdinalIgnoreCase).ToList();
				List<CoreEnemyAssetSlotRecord> slots = group.ToList();
				if (candidates.Count == 0) continue;
				for (int index = 0; index < slots.Count; ++index)
				{
					string source = candidates[Math.Min(index, candidates.Count - 1)];
					result.Matches.Add(new SimpleRetextureInput { Slot = slots[index].Slot, SourcePng = source });
					usedSlots.Add(slots[index].Slot);
					usedFiles.Add(source);
				}
			}

			foreach (string file in files.Where(i_file => !usedFiles.Contains(i_file)))
			{
				string fileName = Path.GetFileNameWithoutExtension(file);
				string key = MatchKey(fileName);
				string duplicateKey = MatchKey(RemoveDuplicateSuffix(fileName));
				List<CoreEnemyAssetSlotRecord> slots = (i_enemy.Slots ?? new List<CoreEnemyAssetSlotRecord>())
					.Where(i_slot => i_slot != null && !usedSlots.Contains(i_slot.Slot)
						&& (MatchKey(LastSegment(i_slot.Slot)) == key || MatchKey(LastSegment(i_slot.Slot)) == duplicateKey)).ToList();
				if (slots.Count == 0) { result.UnmatchedPngs.Add(file); continue; }
				foreach (CoreEnemyAssetSlotRecord slot in slots)
				{
					result.Matches.Add(new SimpleRetextureInput { Slot = slot.Slot, SourcePng = file });
					usedSlots.Add(slot.Slot);
				}
			}
			result.Matches.Sort((i_left, i_right) => string.CompareOrdinal(i_left.Slot, i_right.Slot));
			return result;
		}

		public static SimpleRetextureBuildResult Build(string i_packRoot, string i_patchSlug, string i_target,
			IEnumerable<SimpleRetextureInput> i_inputs, int i_sourceScale, bool i_overwrite)
		{
			SimpleRetextureBuildResult result = new SimpleRetextureBuildResult();
			string root = FullPath(i_packRoot);
			string manifestPath = Path.Combine(root, "manifest.json");
			if (!Directory.Exists(root) || !File.Exists(manifestPath))
			{
				result.Report.Add(ValidationSeverity.Error, "retexture.pack", "Choose a mod folder containing manifest.json.", root);
				return result;
			}
			if (!ContentId.TryParse(i_target, out ContentId target))
				result.Report.Add(ValidationSeverity.Error, "retexture.target", "Target must be a valid public content ID.", i_target);
			if (!IsSimpleSlug(i_patchSlug) || !ContentId.TryParse("author:patch/" + i_patchSlug, out _))
				result.Report.Add(ValidationSeverity.Error, "retexture.patch-id", "Patch name must use lowercase ID characters, for example jenny-blue.", i_patchSlug);
			if (i_sourceScale < 1 || i_sourceScale > 16)
				result.Report.Add(ValidationSeverity.Error, "retexture.scale", "Source scale must be between 1 and 16.", i_sourceScale.ToString());

			ManifestLoadResult manifestResult = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false);
			foreach (ValidationIssue issue in manifestResult.Report.Issues)
				result.Report.Add(issue.Severity, issue.Code, issue.Message, issue.Source);
			if (!result.Report.IsValid) return result;

			List<SimpleRetextureInput> inputs = (i_inputs ?? new SimpleRetextureInput[0])
				.Where(i_input => i_input != null).ToList();
			if (inputs.Count == 0)
			{
				result.Report.Add(ValidationSeverity.Error, "retexture.empty", "Add at least one replacement PNG.", root);
				return result;
			}

			Dictionary<string, string> replacements = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (SimpleRetextureInput input in inputs)
			{
				string slot = (input.Slot ?? string.Empty).Trim().Trim('/');
				if (!ContentId.TryParse("slot:" + slot, out _))
				{
					result.Report.Add(ValidationSeverity.Error, "retexture.slot", "Invalid relative slot: " + slot, input.SourcePng);
					continue;
				}
				if (replacements.ContainsKey(slot))
				{
					result.Report.Add(ValidationSeverity.Error, "retexture.duplicate-slot", "The slot was added more than once: " + slot, input.SourcePng);
					continue;
				}
				if (string.IsNullOrWhiteSpace(input.SourcePng) || !File.Exists(input.SourcePng)
					|| !input.SourcePng.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					result.Report.Add(ValidationSeverity.Error, "retexture.png", "Replacement must be an existing PNG file.", input.SourcePng);
					continue;
				}
				string relative = "assets/retextures/" + i_patchSlug + "/" + FileNameForSlot(slot) + ".png";
				replacements.Add(slot, relative);
			}
			if (!result.Report.IsValid) return result;

			string contentDirectory = Path.Combine(root, "content");
			string assetDirectory = Path.Combine(root, "assets", "retextures", i_patchSlug);
			string patchPath = Path.Combine(contentDirectory, "retexture-" + i_patchSlug + ".json");
			if (!i_overwrite && (File.Exists(patchPath) || replacements.Values.Any(i_path => File.Exists(Path.Combine(root, Native(i_path))))))
			{
				result.Report.Add(ValidationSeverity.Error, "retexture.exists", "Retexture files already exist. Confirm replacement in the wizard to update them.", patchPath);
				return result;
			}

			Directory.CreateDirectory(contentDirectory);
			Directory.CreateDirectory(assetDirectory);
			try
			{
				foreach (SimpleRetextureInput input in inputs)
				{
					string slot = input.Slot.Trim().Trim('/');
					string destination = Path.Combine(root, Native(replacements[slot]));
					WritePng(input.SourcePng, destination, i_sourceScale);
				}
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is InvalidDataException || exception is ArgumentException || exception is UnityException)
			{
				result.Report.Add(ValidationSeverity.Error, "retexture.copy", exception.Message, assetDirectory);
				return result;
			}

			ModManifest manifest = manifestResult.Manifest;
			manifest.Overrides = manifest.Overrides ?? new List<string>();
			foreach (string slot in replacements.Keys)
			{
				string fullSlot = target + "/" + slot;
				if (!manifest.Overrides.Contains(fullSlot)) manifest.Overrides.Add(fullSlot);
			}
			manifest.Overrides = manifest.Overrides.Distinct(StringComparer.Ordinal).OrderBy(i_value => i_value, StringComparer.Ordinal).ToList();
			if (target.Namespace == "core")
			{
				manifest.Dependencies = manifest.Dependencies ?? new List<ModDependency>();
				if (!manifest.Dependencies.Any(i_dependency => i_dependency != null && i_dependency.Id == "core"))
					manifest.Dependencies.Add(new ModDependency { Id = "core", Version = ">=0.1.0" });
			}

			AssetPatchDocument patch = new AssetPatchDocument
			{
				SchemaVersion = AssetPatchParser.SupportedSchemaVersion,
				Type = "assetPatch",
				Id = manifest.Id + ":patch/" + i_patchSlug,
				Target = target.ToString(),
				Replacements = replacements
			};
			WriteJson(manifestPath, manifest);
			WriteJson(patchPath, patch);

			AssetPatchLoadResult parsed = AssetPatchParser.Parse(File.ReadAllText(patchPath), manifest.Id, patchPath);
			foreach (ValidationIssue issue in parsed.Report.Issues)
				result.Report.Add(issue.Severity, issue.Code, issue.Message, issue.Source);
			result.PatchPath = patchPath;
			result.ReplacementCount = replacements.Count;
			return result;
		}

		private static void WritePng(string i_source, string i_destination, int i_scale)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(i_destination));
			Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			Texture2D output = null;
			try
			{
				byte[] bytes = File.ReadAllBytes(i_source);
				if (bytes.Length == 0 || bytes.Length > RuntimePngAssetLoader.MaximumPngBytes)
					throw new InvalidDataException(Path.GetFileName(i_source) + " must be between 1 byte and 32 MiB.");
				if (!source.LoadImage(bytes, false)) throw new InvalidDataException("Unity could not decode " + i_source);
				if (source.width > RuntimePngAssetLoader.MaximumPngDimension || source.height > RuntimePngAssetLoader.MaximumPngDimension
					|| (long)source.width * source.height > RuntimePngAssetLoader.MaximumPngPixels)
					throw new InvalidDataException(Path.GetFileName(i_source) + " exceeds the PNG dimension limits.");
				if (i_scale == 1)
				{
					if (!string.Equals(FullPath(i_source), FullPath(i_destination), StringComparison.OrdinalIgnoreCase))
						File.WriteAllBytes(i_destination, bytes);
					return;
				}
				if (source.width % i_scale != 0 || source.height % i_scale != 0)
					throw new InvalidDataException(Path.GetFileName(i_source) + " dimensions are not divisible by export scale " + i_scale + ".");
				int width = source.width / i_scale;
				int height = source.height / i_scale;
				Color32[] sourcePixels = source.GetPixels32();
				Color32[] outputPixels = new Color32[width * height];
				for (int y = 0; y < height; ++y)
					for (int x = 0; x < width; ++x)
						outputPixels[y * width + x] = sourcePixels[(y * i_scale) * source.width + (x * i_scale)];
				output = new Texture2D(width, height, TextureFormat.RGBA32, false);
				output.SetPixels32(outputPixels);
				output.Apply(false, false);
				File.WriteAllBytes(i_destination, output.EncodeToPNG());
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(source);
				if (output != null) UnityEngine.Object.DestroyImmediate(output);
			}
		}

		private static string FileNameForSlot(string i_slot)
		{
			return i_slot.Replace('/', '-').Replace('\\', '-');
		}

		private static string LastSegment(string i_value)
		{
			int slash = (i_value ?? string.Empty).LastIndexOf('/');
			return slash < 0 ? i_value ?? string.Empty : i_value.Substring(slash + 1);
		}

		private static string RemoveDuplicateSuffix(string i_value)
		{
			string value = i_value ?? string.Empty;
			int dash = value.LastIndexOf('-');
			if (dash < 0 || dash == value.Length - 1) return value;
			for (int index = dash + 1; index < value.Length; ++index)
				if (!char.IsDigit(value[index])) return value;
			return value.Substring(0, dash);
		}

		private static int DuplicateIndex(string i_path)
		{
			string value = Path.GetFileNameWithoutExtension(i_path) ?? string.Empty;
			int dash = value.LastIndexOf('-');
			if (dash < 0 || dash == value.Length - 1) return 1;
			return int.TryParse(value.Substring(dash + 1), out int index) ? index : 1;
		}

		private static bool FileMatches(string i_path, string i_expectedKey)
		{
			string fileName = Path.GetFileNameWithoutExtension(i_path);
			return MatchKey(fileName) == i_expectedKey || MatchKey(RemoveDuplicateSuffix(fileName)) == i_expectedKey;
		}

		private static bool IsSimpleSlug(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value)) return false;
			foreach (char character in i_value)
				if (!((character >= 'a' && character <= 'z') || (character >= '0' && character <= '9')
					|| character == '-' || character == '_' || character == '.')) return false;
			return true;
		}

		private static string MatchKey(string i_value)
		{
			return new string((i_value ?? string.Empty).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
		}

		private static void WriteJson(string i_path, object i_value)
		{
			File.WriteAllText(i_path, JsonConvert.SerializeObject(i_value, Formatting.Indented) + Environment.NewLine);
		}

		private static string Native(string i_path) { return i_path.Replace('/', Path.DirectorySeparatorChar); }
		private static string FullPath(string i_path) { return Path.GetFullPath(i_path ?? string.Empty); }
	}
}
