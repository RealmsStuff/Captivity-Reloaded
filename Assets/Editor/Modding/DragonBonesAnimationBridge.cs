using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed class DragonBonesSafeImportResult
	{
		public ValidationReport Report { get; } = new ValidationReport();
		public int ConvertedAnimationCount { get; internal set; }
		public int ReplacedAnimationCount { get; internal set; }
		public string BackupDirectory { get; internal set; }
		public List<string> TargetFiles { get; } = new List<string>();
	}

	public sealed class DragonBonesAnimationBridgeWindow : EditorWindow
	{
		[SerializeField] private int m_tab;
		[SerializeField] private int m_exportSource;
		[SerializeField] private string m_rigPath = "ExampleMods/animation-reference-gallery/reference/rigs/zombie/rig.json";
		[SerializeField] private string m_enemyDefinitionPath = "ExampleMods/prey-green-zombie/content/original-green-stalker.json";
		[SerializeField] private string m_animationDirectory = "ExampleMods/animation-reference-gallery/content/animations/zombie";
		[SerializeField] private string m_exportDirectory = "DragonBonesExports/zombie";
		[SerializeField] private string m_skeletonPath = "DragonBonesExports/zombie/zombie_ske.json";
		[SerializeField] private string m_sidecarPath = "DragonBonesExports/zombie/captivity-roundtrip.json";
		[SerializeField] private string m_importDirectory = "DragonBonesImports/zombie";
		[SerializeField] private float m_pixelsPerUnit = 32f;
		[SerializeField] private int m_imageScale = 4;
		[SerializeField] private bool m_includePairedPlayer;
		[SerializeField] private bool m_replaceOriginalFiles;
		[SerializeField] private Vector2 m_scroll;
		private string m_status;

		[MenuItem("Captivity Reloaded/Modding/DragonBones Round Trip")]
		public static void Open()
		{
			DragonBonesAnimationBridgeWindow window = GetWindow<DragonBonesAnimationBridgeWindow>();
			window.titleContent = new GUIContent("DragonBones Bridge");
			window.minSize = new Vector2(620f, 440f);
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("DragonBones Animation Round Trip", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Use DragonBones for authoring while keeping the game runtime and .capmod animations independent.", EditorStyles.wordWrappedMiniLabel);
			EditorGUILayout.Space(5f);
			m_tab = GUILayout.Toolbar(m_tab, new[] { "Export to DragonBones", "Import from DragonBones" });
			m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
			if (m_tab == 0) DrawExport(); else DrawImport();
			EditorGUILayout.EndScrollView();
			if (!string.IsNullOrEmpty(m_status)) EditorGUILayout.HelpBox(m_status, MessageType.Info);
		}

		private void DrawExport()
		{
			EditorGUILayout.Space(8f);
			m_exportSource = GUILayout.Toolbar(m_exportSource, new[] { "Core/reference rig", "Original enemy atlas" });
			if (m_exportSource == 1) { DrawOriginalExport(); return; }
			EditorGUILayout.HelpBox("Exports one armature containing every enemyAnimation JSON in the selected directory. Sprite images are extracted from the Core/reference prefab for local editing.", MessageType.Info);
			EditorGUILayout.LabelField("Reference gallery presets", EditorStyles.boldLabel);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Zombie")) SetGalleryPreset("zombie");
				if (GUILayout.Button("Hound")) SetGalleryPreset("death-hound");
				if (GUILayout.Button("Fly")) SetGalleryPreset("fly");
				if (GUILayout.Button("Maggot")) SetGalleryPreset("maggot");
				if (GUILayout.Button("Musca")) SetGalleryPreset("musca");
				if (GUILayout.Button("Gremlin")) SetGalleryPreset("gremlin");
			}
			if (GUILayout.Button("Export All Six Reference Families"))
			{
				try { DragonBonesAnimationBridge.ExportReferenceGallery(m_pixelsPerUnit, m_imageScale); m_status = "Exported all six corrected DragonBones reference families."; AssetDatabase.Refresh(); }
				catch (Exception exception) { m_status = "Batch export failed: " + exception.Message; }
			}
			if (GUILayout.Button("Export All Six Paired Enemy + Player References"))
			{
				try { DragonBonesAnimationBridge.ExportPairedReferenceGallery(m_pixelsPerUnit, m_imageScale); m_status = "Exported all six synchronized enemy + player DragonBones projects."; AssetDatabase.Refresh(); }
				catch (Exception exception) { m_status = "Paired batch export failed: " + exception.Message; }
			}
			EditorGUILayout.Space(3f);
			DrawPath("Rig reference", ref m_rigPath, false, "json");
			DrawPath("Animation directory", ref m_animationDirectory, true, null);
			DrawPath("Export directory", ref m_exportDirectory, true, null);
			m_pixelsPerUnit = EditorGUILayout.FloatField("Pixels per unit", m_pixelsPerUnit);
			m_imageScale = EditorGUILayout.IntSlider("Pixel preview scale", m_imageScale, 1, 8);
			EditorGUILayout.HelpBox("Pixel preview scale enlarges exported PNGs and the DragonBones coordinate system with nearest-neighbor sampling. It does not change the animation's size when imported back into Captivity.", MessageType.None);
			EditorGUILayout.Space(6f);
			if (GUILayout.Button("Export Armature, Animations, and Images", GUILayout.Height(32f)))
			{
				try
				{
					DragonBonesAnimationBridge.Export(ProjectPath(m_rigPath), ProjectPath(m_animationDirectory), ProjectPath(m_exportDirectory), m_pixelsPerUnit, m_imageScale);
					m_skeletonPath = RelativeProjectPath(FilePath.Combine(ProjectPath(m_exportDirectory), DragonBonesAnimationBridge.LastArmatureName + "_ske.json"));
					m_sidecarPath = RelativeProjectPath(FilePath.Combine(ProjectPath(m_exportDirectory), "captivity-roundtrip.json"));
					m_status = "Exported " + DragonBonesAnimationBridge.LastAnimationCount + " animations and " + DragonBonesAnimationBridge.LastImageCount + " images. Import the _ske.json and images into DragonBones.";
					AssetDatabase.Refresh();
				}
				catch (Exception exception) { m_status = "Export failed: " + exception.Message; }
			}
		}

		private void DrawOriginalExport()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.HelpBox("Builds a DragonBones 5.5 project directly from an originalSkeletonAtlas enemy. Atlas regions, non-centered pivots, hierarchy, and sprite layer order are preserved without requiring a Unity prefab.", MessageType.Info);
			DrawPath("Original enemy JSON", ref m_enemyDefinitionPath, false, "json");
			DrawPath("Animation directory", ref m_animationDirectory, true, null);
			DrawPath("Export directory", ref m_exportDirectory, true, null);
			m_imageScale = EditorGUILayout.IntSlider("Pixel preview scale", m_imageScale, 1, 8);
			m_includePairedPlayer = EditorGUILayout.ToggleLeft("Include paired player draft", m_includePairedPlayer);
			if (m_includePairedPlayer) EditorGUILayout.HelpBox("Requires paired-draft enemy JSON and player/paired-draft player JSON. The resulting shared armature is for authoring and must later be connected to a gameplay interaction.", MessageType.None);
			EditorGUILayout.Space(6f);
			if (GUILayout.Button("Generate DragonBones Project", GUILayout.Height(32f)))
			{
				try
				{
					DragonBonesAnimationBridge.ExportOriginalEnemy(ProjectPath(m_enemyDefinitionPath), ProjectPath(m_animationDirectory), ProjectPath(m_exportDirectory), m_imageScale, m_includePairedPlayer);
					m_skeletonPath = RelativeProjectPath(FilePath.Combine(ProjectPath(m_exportDirectory), DragonBonesAnimationBridge.LastArmatureName + "_ske.json"));
					m_sidecarPath = RelativeProjectPath(FilePath.Combine(ProjectPath(m_exportDirectory), m_includePairedPlayer ? "captivity-paired-roundtrip.json" : "captivity-roundtrip.json"));
					m_status = "Generated " + DragonBonesAnimationBridge.LastArmatureName + " with " + DragonBonesAnimationBridge.LastAnimationCount + " animation(s).";
					AssetDatabase.Refresh();
				}
				catch (Exception exception) { m_status = "Original enemy export failed: " + exception.Message; }
			}
		}

		private void SetGalleryPreset(string i_family)
		{
			m_rigPath = "ExampleMods/animation-reference-gallery/reference/rigs/" + i_family + "/rig.json";
			m_animationDirectory = "ExampleMods/animation-reference-gallery/content/animations/" + i_family;
			m_exportDirectory = "DragonBonesExports/" + i_family;
			m_skeletonPath = m_exportDirectory + "/" + i_family + "_ske.json";
			m_sidecarPath = m_exportDirectory + "/captivity-roundtrip.json";
			m_importDirectory = "DragonBonesImports/" + i_family;
			m_status = "Selected the " + i_family + " reference family.";
		}

		private void DrawImport()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.HelpBox("Import the skeleton JSON exported from DragonBones. Captivity-only safe events and VFX are restored from the round-trip sidecar.", MessageType.Info);
			DrawPath("DragonBones skeleton JSON", ref m_skeletonPath, false, "json");
			DrawPath("Round-trip sidecar", ref m_sidecarPath, false, "json");
			DrawPath("Output directory", ref m_importDirectory, true, null);
			m_replaceOriginalFiles = EditorGUILayout.ToggleLeft("Replace the original animation files recorded in the sidecar", m_replaceOriginalFiles);
			if (m_replaceOriginalFiles) EditorGUILayout.HelpBox("This overwrites the source animation JSON files. The separate output directory is safer for comparison.", MessageType.Warning);
			EditorGUILayout.Space(6f);
			if (GUILayout.Button("Import Edited Bone Timelines", GUILayout.Height(32f)))
			{
				try
				{
					int count = DragonBonesAnimationBridge.Import(ProjectPath(m_skeletonPath), ProjectPath(m_sidecarPath), ProjectPath(m_importDirectory), m_replaceOriginalFiles);
					m_status = "Imported " + count + " animations. Captivity events, effect definitions, and effect triggers were preserved.";
					AssetDatabase.Refresh();
				}
				catch (Exception exception) { m_status = "Import failed: " + exception.Message; }
			}
		}

		private static void DrawPath(string i_label, ref string io_path, bool i_folder, string i_extension)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				io_path = EditorGUILayout.TextField(i_label, io_path);
				if (GUILayout.Button("Browse", GUILayout.Width(70f)))
				{
					string start = ProjectPath(io_path);
					string selected = i_folder ? EditorUtility.OpenFolderPanel(i_label, Directory.Exists(start) ? start : ProjectRoot(), string.Empty)
						: EditorUtility.OpenFilePanel(i_label, File.Exists(start) ? FilePath.GetDirectoryName(start) : ProjectRoot(), i_extension ?? string.Empty);
					if (!string.IsNullOrEmpty(selected)) io_path = RelativeProjectPath(selected);
				}
			}
		}

		private static string ProjectRoot() { return FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..")); }
		private static string ProjectPath(string i_path) { return FilePath.GetFullPath(FilePath.IsPathRooted(i_path ?? string.Empty) ? i_path : FilePath.Combine(ProjectRoot(), i_path ?? string.Empty)); }
		private static string RelativeProjectPath(string i_path)
		{
			string root = ProjectRoot(); string full = FilePath.GetFullPath(i_path);
			return full.StartsWith(root + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full.Substring(root.Length + 1).Replace('\\', '/') : full;
		}
	}

	public static class DragonBonesAnimationBridge
	{
		private static readonly string[] s_referenceFamilies = { "zombie", "death-hound", "fly", "maggot", "musca", "gremlin" };
		public static string LastArmatureName { get; private set; }
		public static int LastAnimationCount { get; private set; }
		public static int LastImageCount { get; private set; }

		public static DragonBonesSafeImportResult SafeImport(string i_skeletonPath, string i_sidecarPath, string i_modRoot, bool i_commit)
		{
			DragonBonesSafeImportResult result = new DragonBonesSafeImportResult();
			string temporaryRoot = null;
			try
			{
				string skeletonPath = FilePath.GetFullPath(i_skeletonPath ?? string.Empty), sidecarPath = FilePath.GetFullPath(i_sidecarPath ?? string.Empty);
				string modRoot = FilePath.GetFullPath(i_modRoot ?? string.Empty);
				if (!File.Exists(skeletonPath)) throw new FileNotFoundException("DragonBones skeleton JSON was not found.", skeletonPath);
				if (!File.Exists(sidecarPath)) throw new FileNotFoundException("Round-trip sidecar was not found.", sidecarPath);
				if (!File.Exists(FilePath.Combine(modRoot, "manifest.json"))) throw new InvalidDataException("Choose the loose mod folder containing manifest.json.");
				JObject skeleton = JObject.Parse(File.ReadAllText(skeletonPath)), sidecar = JObject.Parse(File.ReadAllText(sidecarPath));
				AnalyzeRoundTrip(skeleton, sidecar, result.Report, skeletonPath);

				temporaryRoot = FilePath.Combine(FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..")), "Temp", "Modding", "DragonBonesImport", Guid.NewGuid().ToString("N"));
				string convertedRoot = FilePath.Combine(temporaryRoot, "converted");
				Directory.CreateDirectory(convertedRoot);
				result.ConvertedAnimationCount = Import(skeletonPath, sidecarPath, convertedRoot, false);
				if (result.ConvertedAnimationCount == 0) result.Report.Add(ValidationSeverity.Error, "dragonbones.import.empty", "No matching animations were converted from the edited armature.", skeletonPath);

				Dictionary<string, string> targetsById = DiscoverAnimationTargets(modRoot, result.Report);
				Dictionary<string, string> importedByTarget = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (string importedPath in Directory.GetFiles(convertedRoot, "*.json", SearchOption.AllDirectories))
				{
					JObject document = JObject.Parse(File.ReadAllText(importedPath)); string id = (string)document["id"], type = (string)document["type"];
					if ((type != "enemyAnimation" && type != "playerAnimation") || string.IsNullOrWhiteSpace(id))
					{
						result.Report.Add(ValidationSeverity.Error, "dragonbones.import.document", "Converted output is not a named enemyAnimation or playerAnimation.", importedPath); continue;
					}
					if (!targetsById.TryGetValue(id, out string target))
					{
						result.Report.Add(ValidationSeverity.Error, "dragonbones.import.target", "No existing mod animation has ID " + id + ". Create or reconnect the draft before importing.", importedPath); continue;
					}
					if (importedByTarget.ContainsKey(target)) result.Report.Add(ValidationSeverity.Error, "dragonbones.import.duplicate", "More than one converted animation targets " + id + ".", importedPath);
					else importedByTarget.Add(target, importedPath);
				}
				foreach (string target in importedByTarget.Keys.OrderBy(i_path => i_path, StringComparer.OrdinalIgnoreCase)) result.TargetFiles.Add(target);

				if (result.Report.IsValid)
				{
					string stagedMod = FilePath.Combine(temporaryRoot, "staged-mod"); CopyDirectory(modRoot, stagedMod);
					foreach (KeyValuePair<string, string> pair in importedByTarget)
					{
						string relative = RelativePath(modRoot, pair.Key), stagedTarget = FilePath.Combine(stagedMod, relative);
						Directory.CreateDirectory(FilePath.GetDirectoryName(stagedTarget)); File.Copy(pair.Value, stagedTarget, true);
					}
					result.Report.Merge(CapmodPackageBuilder.ValidateSource(stagedMod).Report);
				}
				if (!i_commit || !result.Report.IsValid) return result;

				JObject manifest = JObject.Parse(File.ReadAllText(FilePath.Combine(modRoot, "manifest.json")));
				string packId = Slug((string)manifest["id"] ?? "mod"), projectRoot = FilePath.GetFullPath(FilePath.Combine(Application.dataPath, ".."));
				string label = Slug(FilePath.GetFileNameWithoutExtension(skeletonPath).Replace("_ske", string.Empty));
				string backup = FilePath.Combine(projectRoot, "ModAuthoringBackups", packId, label,
					DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6));
				foreach (string target in importedByTarget.Keys)
				{
					string destination = FilePath.Combine(backup, RelativePath(modRoot, target)); Directory.CreateDirectory(FilePath.GetDirectoryName(destination)); File.Copy(target, destination, false);
				}
				result.BackupDirectory = backup;
				try
				{
					foreach (KeyValuePair<string, string> pair in importedByTarget) File.Copy(pair.Value, pair.Key, true);
					CapmodPackageResult liveValidation = CapmodPackageBuilder.ValidateSource(modRoot);
					if (!liveValidation.Report.IsValid) { result.Report.Merge(liveValidation.Report); throw new InvalidDataException("The live mod failed validation after replacement."); }
					result.ReplacedAnimationCount = importedByTarget.Count;
				}
				catch
				{
					foreach (string target in importedByTarget.Keys)
					{
						string source = FilePath.Combine(backup, RelativePath(modRoot, target)); if (File.Exists(source)) File.Copy(source, target, true);
					}
					throw;
				}
				return result;
			}
			catch (Exception exception)
			{
				result.Report.Add(ValidationSeverity.Error, "dragonbones.import", exception.Message, i_skeletonPath); return result;
			}
			finally
			{
				if (!string.IsNullOrEmpty(temporaryRoot) && Directory.Exists(temporaryRoot))
					try { Directory.Delete(temporaryRoot, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
			}
		}

		private static void AnalyzeRoundTrip(JObject i_skeleton, JObject i_sidecar, ValidationReport io_report, string i_source)
		{
			JObject armature = ((JArray)i_skeleton["armature"] ?? new JArray()).OfType<JObject>().FirstOrDefault();
			if (armature == null) { io_report.Add(ValidationSeverity.Error, "dragonbones.armature", "The DragonBones file contains no armature.", i_source); return; }
			int frameRate = (int?)armature["frameRate"] ?? (int?)i_skeleton["frameRate"] ?? 0;
			if (frameRate < 1 || frameRate > 240) io_report.Add(ValidationSeverity.Error, "dragonbones.frame-rate", "Armature frameRate must be from 1 through 240.", i_source);
			string type = (string)i_sidecar["type"];
			if (type == "captivityDragonBonesPairedRoundTrip")
			{
				AnalyzeRoundTripSet(armature, i_sidecar["enemy"] as JObject, (string)i_sidecar["enemyPrefix"] ?? "enemy-", io_report, i_source);
				AnalyzeRoundTripSet(armature, i_sidecar["player"] as JObject, (string)i_sidecar["playerPrefix"] ?? "player-", io_report, i_source);
			}
			else if (type == "captivityDragonBonesRoundTrip" || type == "captivityLoongBonesRoundTrip") AnalyzeRoundTripSet(armature, i_sidecar, string.Empty, io_report, i_source);
			else io_report.Add(ValidationSeverity.Error, "dragonbones.sidecar", "The sidecar type is not supported.", i_source);
		}

		private static void AnalyzeRoundTripSet(JObject i_armature, JObject i_sidecar, string i_prefix, ValidationReport io_report, string i_source)
		{
			if (i_sidecar == null) { io_report.Add(ValidationSeverity.Error, "dragonbones.sidecar-set", "The round-trip sidecar is missing a rig set.", i_source); return; }
			JObject rig = i_sidecar["rig"] as JObject; JObject sources = i_sidecar["sourceDocuments"] as JObject;
			if (rig == null || sources == null) { io_report.Add(ValidationSeverity.Error, "dragonbones.sidecar-data", "The round-trip sidecar is missing its rig or source documents.", i_source); return; }
			List<string> actualBones = ((JArray)i_armature["bone"] ?? new JArray()).OfType<JObject>().Select(i_bone => (string)i_bone["name"]).Where(i_name => i_name != null).ToList();
			foreach (IGrouping<string, string> duplicate in actualBones.GroupBy(i_name => i_name, StringComparer.Ordinal).Where(i_group => i_group.Count() > 1))
				io_report.Add(ValidationSeverity.Error, "dragonbones.bone-duplicate", "Duplicate bone name: " + duplicate.Key, i_source);
			HashSet<string> expectedBones = new HashSet<string>(((JArray)rig["bones"] ?? new JArray()).OfType<JObject>().Select(i_bone => i_prefix + (string)i_bone["name"]), StringComparer.Ordinal);
			HashSet<string> actual = new HashSet<string>(actualBones, StringComparer.Ordinal);
			foreach (string expected in expectedBones.Where(i_bone => !actual.Contains(i_bone))) io_report.Add(ValidationSeverity.Error, "dragonbones.bone-missing", "Required bone was removed or renamed: " + expected, i_source);
			HashSet<string> animationNames = new HashSet<string>(((JArray)i_armature["animation"] ?? new JArray()).OfType<JObject>().Select(i_animation => (string)i_animation["name"]), StringComparer.Ordinal);
			foreach (JProperty source in sources.Properties().Where(i_property => !animationNames.Contains(i_property.Name))) io_report.Add(ValidationSeverity.Error, "dragonbones.clip-missing", "Required animation was removed or renamed: " + source.Name, i_source);
			foreach (string extra in animationNames.Where(i_name => i_name != null && sources[i_name] == null)) io_report.Add(ValidationSeverity.Warning, "dragonbones.clip-unmapped", "Animation is not recorded in the sidecar and will not be imported: " + extra, i_source);
			foreach (JObject animation in ((JArray)i_armature["animation"] ?? new JArray()).OfType<JObject>())
			{
				if (((int?)animation["duration"] ?? 0) < 1) io_report.Add(ValidationSeverity.Error, "dragonbones.clip-duration", "Animation has no positive frame duration: " + (string)animation["name"], i_source);
				foreach (JObject timeline in ((JArray)animation["bone"] ?? new JArray()).OfType<JObject>())
				{
					string name = (string)timeline["name"];
					if (!string.IsNullOrEmpty(i_prefix) && (name == null || !name.StartsWith(i_prefix, StringComparison.Ordinal))) continue;
					if (!expectedBones.Contains(name)) io_report.Add(ValidationSeverity.Error, "dragonbones.timeline-bone", "Animation " + (string)animation["name"] + " targets an unknown bone: " + name, i_source);
				}
			}
			List<JObject> slots = ((JArray)i_armature["slot"] ?? new JArray()).OfType<JObject>().ToList();
			HashSet<string> slottedBones = new HashSet<string>(slots.Select(i_slot => (string)i_slot["parent"]), StringComparer.Ordinal);
			HashSet<string> displayBones = new HashSet<string>(((JArray)rig["bones"] ?? new JArray()).OfType<JObject>()
				.Where(i_bone => ((JArray)i_bone["sprites"] ?? new JArray()).Count > 0).Select(i_bone => i_prefix + (string)i_bone["name"]), StringComparer.Ordinal);
			foreach (string bone in displayBones.Where(i_bone => !slottedBones.Contains(i_bone))) io_report.Add(ValidationSeverity.Warning, "dragonbones.slot-missing", "Bone has no display slot; motion imports but its sprite may be absent in DragonBones: " + bone, i_source);
			if (slots.Any(i_slot => slottedBones.Contains((string)i_slot["parent"]) && i_slot["userData"]?["captivitySortingOrder"] == null))
				io_report.Add(ValidationSeverity.Warning, "dragonbones.layer-metadata", "One or more display slots lost their Captivity sorting metadata. Animation motion can import, but keep layer changes in the enemy JSON or Unity VFX editor.", i_source);
		}

		private static Dictionary<string, string> DiscoverAnimationTargets(string i_modRoot, ValidationReport io_report)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (string path in Directory.GetFiles(i_modRoot, "*.json", SearchOption.AllDirectories))
			{
				try
				{
					JObject document = JObject.Parse(File.ReadAllText(path)); string type = (string)document["type"], id = (string)document["id"];
					if ((type != "enemyAnimation" && type != "playerAnimation") || string.IsNullOrWhiteSpace(id)) continue;
					if (result.ContainsKey(id)) io_report.Add(ValidationSeverity.Error, "dragonbones.target-duplicate", "The loose mod contains duplicate animation ID " + id + ".", path);
					else result.Add(id, path);
				}
				catch (Newtonsoft.Json.JsonException) { }
			}
			return result;
		}

		private static void CopyDirectory(string i_source, string i_destination)
		{
			Directory.CreateDirectory(i_destination);
			foreach (string file in Directory.GetFiles(i_source, "*", SearchOption.TopDirectoryOnly)) File.Copy(file, FilePath.Combine(i_destination, FilePath.GetFileName(file)), false);
			foreach (string directory in Directory.GetDirectories(i_source, "*", SearchOption.TopDirectoryOnly))
			{
				FileAttributes attributes = File.GetAttributes(directory); if ((attributes & FileAttributes.ReparsePoint) != 0) continue;
				CopyDirectory(directory, FilePath.Combine(i_destination, FilePath.GetFileName(directory)));
			}
		}

		private static string RelativePath(string i_root, string i_path)
		{
			string root = FilePath.GetFullPath(i_root).TrimEnd(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar) + FilePath.DirectorySeparatorChar;
			string path = FilePath.GetFullPath(i_path); if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Animation target escapes the selected mod folder: " + path);
			return path.Substring(root.Length);
		}

		/// <summary>
		/// Creates a DragonBones 5.5 authoring project directly from a data-defined
		/// original enemy. Unlike Export, this path reads regions from the pack PNG
		/// and therefore does not require a Unity prefab or imported Sprite assets.
		/// </summary>
		public static void ExportOriginalEnemy(string i_enemyDefinitionPath, string i_animationDirectory,
			string i_outputDirectory, int i_imageScale = 4, bool i_includePairedPlayer = false)
		{
			if (!File.Exists(i_enemyDefinitionPath)) throw new FileNotFoundException("Original enemy JSON was not found.", i_enemyDefinitionPath);
			if (!Directory.Exists(i_animationDirectory)) throw new DirectoryNotFoundException("Animation directory was not found: " + i_animationDirectory);
			if (i_imageScale < 1 || i_imageScale > 8) throw new InvalidDataException("Pixel preview scale must be from 1 through 8.");
			JObject enemy = JObject.Parse(File.ReadAllText(i_enemyDefinitionPath));
			JObject visual = enemy["visual"] as JObject ?? throw new InvalidDataException("The enemy has no visual definition.");
			if ((string)visual["type"] != "originalSkeletonAtlas") throw new InvalidDataException("DragonBones project generation requires an originalSkeletonAtlas enemy.");
			string packRoot = FindPackRoot(FilePath.GetDirectoryName(FilePath.GetFullPath(i_enemyDefinitionPath)));
			string atlasRelative = (string)visual["atlas"];
			if (string.IsNullOrWhiteSpace(atlasRelative)) throw new InvalidDataException("The original enemy has no atlas path.");
			string atlasPath = FilePath.GetFullPath(FilePath.Combine(packRoot, atlasRelative.Replace('/', FilePath.DirectorySeparatorChar)));
			if (!File.Exists(atlasPath)) throw new FileNotFoundException("The original enemy atlas was not found.", atlasPath);
			JObject rig = BuildOriginalRig(enemy, visual, atlasPath);
			List<string> enemyFiles = AnimationFiles(i_animationDirectory, "enemyAnimation", (string)enemy["id"]);
			if (enemyFiles.Count == 0) throw new InvalidDataException("The selected directory contains no enemyAnimation documents for " + (string)enemy["id"] + ".");

			if (!i_includePairedPlayer)
			{
				ExportOriginalSet(rig, enemyFiles, i_outputDirectory, (float?)visual["pixelsPerUnit"] ?? 32f, i_imageScale);
				return;
			}

			string enemyOnly = FilePath.Combine(i_outputDirectory, "enemy-only");
			ExportOriginalSet(rig, enemyFiles, enemyOnly, (float?)visual["pixelsPerUnit"] ?? 32f, i_imageScale);
			string pairedEnemyFile = enemyFiles.FirstOrDefault(i_file =>
				string.Equals(((string)JObject.Parse(File.ReadAllText(i_file))["id"])?.Split('/').LastOrDefault(), "paired-draft", StringComparison.Ordinal));
			if (pairedEnemyFile == null) throw new InvalidDataException("Paired export requires an enemy animation whose ID ends in paired-draft.");
			string playerAnimationDirectory = FilePath.Combine(i_animationDirectory, "player");
			List<string> playerFiles = AnimationFiles(playerAnimationDirectory, "playerAnimation", "core:player-rig/alex");
			string pairedPlayerFile = playerFiles.FirstOrDefault(i_file =>
				string.Equals(((string)JObject.Parse(File.ReadAllText(i_file))["id"])?.Split('/').LastOrDefault(), "paired-draft", StringComparison.Ordinal));
			if (pairedPlayerFile == null) throw new InvalidDataException("Paired export requires player/paired-draft.player-animation.json.");

			string enemySource = FilePath.Combine(i_outputDirectory, "source-enemy");
			string enemySourceAnimations = FilePath.Combine(enemySource, "animations");
			Directory.CreateDirectory(enemySourceAnimations);
			string enemyCopy = FilePath.Combine(enemySourceAnimations, FilePath.GetFileName(pairedEnemyFile));
			File.Copy(pairedEnemyFile, enemyCopy, true);
			ExportOriginalSet(rig, new List<string> { enemyCopy }, enemySource, (float?)visual["pixelsPerUnit"] ?? 32f, i_imageScale);
			string enemyArmature = LastArmatureName;

			string playerSource = FilePath.Combine(i_outputDirectory, "source-player");
			string playerSourceAnimations = FilePath.Combine(playerSource, "animations");
			Directory.CreateDirectory(playerSourceAnimations);
			File.Copy(pairedPlayerFile, FilePath.Combine(playerSourceAnimations, FilePath.GetFileName(pairedPlayerFile)), true);
			string playerRigPath = FilePath.Combine(playerSource, "player-rig.json");
			File.WriteAllText(playerRigPath, NormalizedPlayerAnimationExporter.BuildRigDocument().ToString(Formatting.Indented));
			Export(playerRigPath, playerSourceAnimations, playerSource, 32f, i_imageScale);
			string playerArmature = LastArmatureName;
			string family = Slug(((string)enemy["id"])?.Split('/').LastOrDefault() ?? "original-enemy");
			CombinePairedExports(family, i_outputDirectory, enemySource, enemyArmature, playerSource, playerArmature);
		}

		private static List<string> AnimationFiles(string i_directory, string i_type, string i_target)
		{
			if (!Directory.Exists(i_directory)) return new List<string>();
			return Directory.GetFiles(i_directory, "*.json", SearchOption.TopDirectoryOnly).Where(i_file =>
			{
				JObject document = JObject.Parse(File.ReadAllText(i_file));
				return (string)document["type"] == i_type
					&& (i_type == "enemyAnimation" ? (string)document["enemy"] : (string)document["rig"]) == i_target;
			}).OrderBy(i_file => i_file, StringComparer.Ordinal).ToList();
		}

		private static string FindPackRoot(string i_directory)
		{
			for (DirectoryInfo current = new DirectoryInfo(i_directory); current != null; current = current.Parent)
				if (File.Exists(FilePath.Combine(current.FullName, "manifest.json"))) return current.FullName;
			throw new InvalidDataException("Could not find manifest.json above the enemy definition.");
		}

		private static JObject BuildOriginalRig(JObject i_enemy, JObject i_visual, string i_atlasPath)
		{
			JObject regions = i_visual["regions"] as JObject ?? throw new InvalidDataException("The original enemy has no atlas regions.");
			JArray rigBones = new JArray();
			foreach (JObject bone in ((JArray)i_visual["bones"] ?? new JArray()).OfType<JObject>())
			{
				string id = (string)bone["id"], regionName = (string)bone["region"];
				JObject region = regions[regionName] as JObject ?? throw new InvalidDataException("Bone " + id + " references missing region " + regionName + ".");
				JObject rigBone = new JObject
				{
					["name"] = id, ["defaultTransform"] = new JObject { ["x"] = (float?)bone["x"] ?? 0f, ["y"] = (float?)bone["y"] ?? 0f,
						["rotationZ"] = (float?)bone["rotation"] ?? 0f, ["scaleX"] = 1f, ["scaleY"] = 1f, ["scaleZ"] = 1f },
					["sprites"] = new JArray(new JObject { ["name"] = regionName, ["region"] = region.DeepClone(),
						["pivotX"] = (float?)bone["pivotX"] ?? .5f, ["pivotY"] = (float?)bone["pivotY"] ?? .5f,
						["sortingOrder"] = (int?)bone["sortingOrder"] ?? 0 })
				};
				if (!string.IsNullOrWhiteSpace((string)bone["parent"])) rigBone["parent"] = (string)bone["parent"];
				rigBones.Add(rigBone);
			}
			if (rigBones.Count == 0) throw new InvalidDataException("The original enemy has no bones.");
			return new JObject { ["schemaVersion"] = 1, ["type"] = "enemyRigReference", ["id"] = ((string)i_enemy["id"])?.Replace(":enemy/", ":enemy-rig/"),
				["enemy"] = (string)i_enemy["id"], ["atlas"] = i_atlasPath, ["bones"] = rigBones };
		}

		private static void ExportOriginalSet(JObject i_rig, IReadOnlyList<string> i_animationFiles, string i_outputDirectory,
			float i_pixelsPerUnit, int i_imageScale)
		{
			if (!Finite(i_pixelsPerUnit) || i_pixelsPerUnit <= 0f || i_pixelsPerUnit > 4096f) throw new InvalidDataException("Pixels per unit must be greater than zero and at most 4096.");
			Directory.CreateDirectory(i_outputDirectory);
			List<KeyValuePair<string, JObject>> sources = i_animationFiles.Select(i_file => new KeyValuePair<string, JObject>(i_file, JObject.Parse(File.ReadAllText(i_file))))
				.OrderBy(i_pair => (string)i_pair.Value["id"], StringComparer.Ordinal).ToList();
			List<JObject> documents = sources.Select(i_pair => i_pair.Value).ToList();
			if (documents.Count == 0) throw new InvalidDataException("No original enemy animations were selected.");
			string targetRig = (string)documents[0]["enemy"];
			string armatureName = Slug(targetRig?.Split('/').LastOrDefault() ?? "original-enemy");
			float authoringPpu = i_pixelsPerUnit * i_imageScale;
			JArray bones = (JArray)i_rig["bones"];
			Dictionary<string, JObject> bonesByName = bones.OfType<JObject>().ToDictionary(i_bone => (string)i_bone["name"], StringComparer.Ordinal);
			Texture2D atlas = LoadPng((string)i_rig["atlas"]);
			try
			{
				JArray dragonBones = new JArray(), slots = new JArray(), skinSlots = new JArray();
				string imageDirectory = FilePath.Combine(i_outputDirectory, "images"); Directory.CreateDirectory(imageDirectory);
				Dictionary<string, string> resourceNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (JObject bone in bones.OfType<JObject>()) dragonBones.Add(ConvertBone(bone, authoringPpu));
				int imageCount = 0;
				foreach (JObject bone in bones.OfType<JObject>())
				{
					string boneName = (string)bone["name"];
					foreach (JObject sprite in ((JArray)bone["sprites"] ?? new JArray()).OfType<JObject>())
					{
						JObject region = sprite["region"] as JObject ?? throw new InvalidDataException("Original rig sprite has no atlas region.");
						string resourceName = UniqueResourceName(Slug((string)sprite["name"] ?? boneName), resourceNames);
						resourceNames[boneName + "|" + resourceName] = resourceName;
						ExportAtlasRegionPng(atlas, region, FilePath.Combine(imageDirectory, resourceName + ".png"), i_imageScale); imageCount++;
						string slotName = boneName + "-slot";
						slots.Add(new JObject { ["name"] = slotName, ["parent"] = boneName, ["displayIndex"] = 0,
							["userData"] = new JObject { ["captivityBone"] = boneName, ["captivitySortingOrder"] = (int?)sprite["sortingOrder"] ?? 0 } });
						float pivotX = (float?)sprite["pivotX"] ?? .5f, pivotY = (float?)sprite["pivotY"] ?? .5f;
						int width = (int?)region["width"] ?? 1, height = (int?)region["height"] ?? 1;
						skinSlots.Add(new JObject { ["name"] = slotName, ["display"] = new JArray(new JObject { ["name"] = resourceName,
							["path"] = resourceName, ["type"] = "image", ["pivot"] = new JObject { ["x"] = .5f, ["y"] = .5f },
							["transform"] = new JObject { ["x"] = (.5f - pivotX) * width * i_imageScale, ["y"] = -(.5f - pivotY) * height * i_imageScale },
							["userData"] = new JObject { ["captivityPivotX"] = pivotX, ["captivityPivotY"] = pivotY } }) });
					}
				}
				List<JObject> orderedSlots = slots.OfType<JObject>().Select((i_slot, i_index) => new { Slot = i_slot, Index = i_index })
					.OrderBy(i_item => (int?)i_item.Slot["userData"]?["captivitySortingOrder"] ?? 0).ThenBy(i_item => i_item.Index)
					.Select(i_item => i_item.Slot).ToList();
				Dictionary<string, JObject> skinBySlot = skinSlots.OfType<JObject>().ToDictionary(i_slot => (string)i_slot["name"], StringComparer.Ordinal);
				slots = new JArray(orderedSlots.Select(i_slot => i_slot.DeepClone()));
				skinSlots = new JArray(orderedSlots.Select(i_slot => skinBySlot[(string)i_slot["name"]].DeepClone()));
				int frameRate = Mathf.Clamp(Mathf.RoundToInt(documents.Max(i_document => (float?)i_document["frameRate"] ?? 60f)), 1, 240);
				JArray animations = new JArray(); JObject sourceDocuments = new JObject(), fileNames = new JObject();
				for (int index = 0; index < documents.Count; index++)
				{
					JObject document = documents[index]; string animationName = Slug(((string)document["id"])?.Split('/').LastOrDefault() ?? (string)document["displayName"]);
					animations.Add(ConvertAnimation(document, bonesByName, frameRate, authoringPpu, animationName)); sourceDocuments[animationName] = document.DeepClone(); fileNames[animationName] = FilePath.GetFileName(sources[index].Key);
				}
				JObject skeleton = new JObject { ["name"] = armatureName, ["version"] = "5.5", ["compatibleVersion"] = "5.5", ["frameRate"] = frameRate,
					["armature"] = new JArray(new JObject { ["name"] = armatureName, ["frameRate"] = frameRate, ["type"] = "Armature", ["bone"] = dragonBones,
						["slot"] = slots, ["skin"] = new JArray(new JObject { ["name"] = "default", ["slot"] = skinSlots }), ["animation"] = animations }) };
				File.WriteAllText(FilePath.Combine(i_outputDirectory, armatureName + "_ske.json"), skeleton.ToString(Formatting.Indented));
				JObject sidecar = new JObject { ["schemaVersion"] = 1, ["type"] = "captivityDragonBonesRoundTrip", ["armature"] = armatureName,
					["pixelsPerUnit"] = authoringPpu, ["sourcePixelsPerUnit"] = i_pixelsPerUnit, ["imageScale"] = i_imageScale,
					["sourceDirectory"] = FilePath.GetFullPath(FilePath.GetDirectoryName(sources[0].Key)), ["rig"] = i_rig.DeepClone(),
					["sourceDocuments"] = sourceDocuments, ["fileNames"] = fileNames };
				File.WriteAllText(FilePath.Combine(i_outputDirectory, "captivity-roundtrip.json"), sidecar.ToString(Formatting.Indented));
				File.WriteAllText(FilePath.Combine(i_outputDirectory, "README.txt"), "Import " + armatureName + "_ske.json as Images and choose this folder's images directory. Export edited work as DragonBones JSON 5.5.\r\n");
				LastArmatureName = armatureName; LastAnimationCount = documents.Count; LastImageCount = imageCount;
			}
			finally { UnityEngine.Object.DestroyImmediate(atlas); }
		}

		private static Texture2D LoadPng(string i_path)
		{
			Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
			if (!texture.LoadImage(File.ReadAllBytes(i_path), false)) { UnityEngine.Object.DestroyImmediate(texture); throw new InvalidDataException("Could not decode atlas PNG: " + i_path); }
			return texture;
		}

		private static void ExportAtlasRegionPng(Texture2D i_atlas, JObject i_region, string i_destination, int i_scale)
		{
			int x = (int?)i_region["x"] ?? 0, y = (int?)i_region["y"] ?? 0, width = (int?)i_region["width"] ?? 0, height = (int?)i_region["height"] ?? 0;
			if (width <= 0 || height <= 0 || x < 0 || y < 0 || x + width > i_atlas.width || y + height > i_atlas.height) throw new InvalidDataException("An atlas region falls outside the source PNG.");
			Color32[] source = i_atlas.GetPixels32(), output = new Color32[width * i_scale * height * i_scale]; int outputWidth = width * i_scale;
			for (int outputY = 0; outputY < height * i_scale; outputY++) for (int outputX = 0; outputX < outputWidth; outputX++)
				output[outputY * outputWidth + outputX] = source[(y + outputY / i_scale) * i_atlas.width + x + outputX / i_scale];
			Texture2D cropped = new Texture2D(outputWidth, height * i_scale, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
			try { cropped.SetPixels32(output); cropped.Apply(); File.WriteAllBytes(i_destination, cropped.EncodeToPNG()); }
			finally { UnityEngine.Object.DestroyImmediate(cropped); }
		}

		[MenuItem("Captivity Reloaded/Modding/Export All DragonBones References")]
		public static void ExportReferenceGalleryMenu()
		{
			ExportReferenceGallery(32f, 4);
			AssetDatabase.Refresh();
			Debug.Log("[Modding] Exported all corrected DragonBones reference families.");
		}

		public static void ExportReferenceGallery(float i_pixelsPerUnit = 32f, int i_imageScale = 4)
		{
			string root = FilePath.GetFullPath(FilePath.Combine(Application.dataPath, ".."));
			foreach (string family in s_referenceFamilies)
			{
				Export(FilePath.Combine(root, "ExampleMods", "animation-reference-gallery", "reference", "rigs", family, "rig.json"),
					FilePath.Combine(root, "ExampleMods", "animation-reference-gallery", "content", "animations", family),
					FilePath.Combine(root, "DragonBonesExports", family), i_pixelsPerUnit, i_imageScale);
			}
		}

		[MenuItem("Captivity Reloaded/Modding/Export All Paired DragonBones References")]
		public static void ExportPairedReferenceGalleryMenu()
		{
			ExportPairedReferenceGallery(32f, 4);
			AssetDatabase.Refresh();
			Debug.Log("[Modding] Exported all synchronized enemy + player DragonBones reference families.");
		}

		public static void ExportPairedReferenceGallery(float i_pixelsPerUnit = 32f, int i_imageScale = 4)
		{
			string root = FilePath.GetFullPath(FilePath.Combine(Application.dataPath, ".."));
			JObject catalog = JObject.Parse(File.ReadAllText(FilePath.Combine(root, "ModSDK", "AnimationReference", "core-animation-catalog.json")));
			foreach (string family in s_referenceFamilies) ExportPairedFamily(root, family, catalog, i_pixelsPerUnit, i_imageScale);
		}

		public static void Export(string i_rigPath, string i_animationDirectory, string i_outputDirectory, float i_pixelsPerUnit, int i_imageScale = 4)
		{
			if (!File.Exists(i_rigPath)) throw new FileNotFoundException("Rig reference was not found.", i_rigPath);
			if (!Directory.Exists(i_animationDirectory)) throw new DirectoryNotFoundException("Animation directory was not found: " + i_animationDirectory);
			if (!Finite(i_pixelsPerUnit) || i_pixelsPerUnit <= 0f || i_pixelsPerUnit > 4096f) throw new InvalidDataException("Pixels per unit must be greater than zero and at most 4096.");
			Directory.CreateDirectory(i_outputDirectory);

			JObject rig = JObject.Parse(File.ReadAllText(i_rigPath));
			List<JObject> documents = Directory.GetFiles(i_animationDirectory, "*.json", SearchOption.TopDirectoryOnly)
				.Select(File.ReadAllText).Select(JObject.Parse).Where(i_json => (string)i_json["type"] == "enemyAnimation" || (string)i_json["type"] == "playerAnimation")
				.OrderBy(i_json => (string)i_json["id"], StringComparer.Ordinal).ToList();
			if (documents.Count == 0) throw new InvalidDataException("The selected directory contains no enemyAnimation or playerAnimation documents.");
			string documentType = (string)documents[0]["type"];
			if (documents.Any(i_document => (string)i_document["type"] != documentType)) throw new InvalidDataException("Enemy and player animation documents must be exported separately or through the paired exporter.");
			string targetRig = documentType == "enemyAnimation" ? (string)documents[0]["enemy"] : (string)documents[0]["rig"];
			if (documents.Any(i_document => (documentType == "enemyAnimation" ? (string)i_document["enemy"] : (string)i_document["rig"]) != targetRig)) throw new InvalidDataException("Every exported animation must target the same rig.");
			if (i_imageScale < 1 || i_imageScale > 8) throw new InvalidDataException("Pixel preview scale must be from 1 through 8.");
			float authoringPixelsPerUnit = i_pixelsPerUnit * i_imageScale;

			string armatureName = Slug(targetRig?.Split('/').LastOrDefault() ?? FilePath.GetFileName(i_animationDirectory));
			LastArmatureName = armatureName;
			JArray bones = (JArray)rig["bones"] ?? throw new InvalidDataException("Rig reference has no bones array.");
			Dictionary<string, JObject> bonesByName = bones.OfType<JObject>().Where(i_bone => i_bone["name"] != null)
				.ToDictionary(i_bone => (string)i_bone["name"], StringComparer.Ordinal);

			GameObject prefabRoot = LoadRigPrefab((string)rig["prefab"]);
			try
			{
				Transform rigRoot = ResolveRigRoot(prefabRoot, (string)rig["sampleRoot"], bones);
				if (prefabRoot != null && rigRoot == null) throw new InvalidDataException("Could not resolve the rig sample root or its referenced sprite paths in the prefab.");
				JArray dragonBones = new JArray(); JArray slots = new JArray(); JArray skinSlots = new JArray();
				Dictionary<string, string> resourceNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				string imageDirectory = FilePath.Combine(i_outputDirectory, "images"); Directory.CreateDirectory(imageDirectory);
				foreach (JObject bone in bones.OfType<JObject>()) dragonBones.Add(ConvertBone(bone, authoringPixelsPerUnit));
				int imageCount = 0;
				foreach (JObject bone in bones.OfType<JObject>())
				{
					string boneName = (string)bone["name"]; JArray sprites = (JArray)bone["sprites"]; if (sprites == null) continue;
					for (int index = 0; index < sprites.Count; index++)
					{
						JObject spriteInfo = (JObject)sprites[index]; string assetPath = (string)spriteInfo["asset"];
						Sprite sprite = LoadSprite(assetPath, (string)spriteInfo["name"]); if (sprite == null) continue;
						string resourceName = UniqueResourceName(Slug(sprite.name), resourceNames); resourceNames[assetPath + "|" + sprite.name] = resourceName;
						ExportSpritePng(sprite, FilePath.Combine(imageDirectory, resourceName + ".png"), i_imageScale); imageCount++;
						string slotName = boneName + "-slot-" + (index + 1);
						JObject slot = new JObject { ["name"] = slotName, ["parent"] = boneName, ["displayIndex"] = 0,
							["userData"] = new JObject { ["captivityBone"] = boneName, ["captivitySortingOrder"] = (int?)spriteInfo["sortingOrder"] ?? 0 } };
						slots.Add(slot);
						JObject display = new JObject { ["name"] = resourceName, ["path"] = resourceName, ["type"] = "image",
							["pivot"] = new JObject { ["x"] = 0.5f, ["y"] = 0.5f },
							["transform"] = DisplayTransform(rigRoot, bonesByName[boneName], spriteInfo, sprite, authoringPixelsPerUnit),
							["userData"] = new JObject { ["captivityPivotX"] = (float?)spriteInfo["pivotX"] ?? sprite.pivot.x / sprite.rect.width,
								["captivityPivotY"] = (float?)spriteInfo["pivotY"] ?? sprite.pivot.y / sprite.rect.height } };
						skinSlots.Add(new JObject { ["name"] = slotName, ["display"] = new JArray(display) });
					}
				}

				int frameRate = Mathf.Clamp(Mathf.RoundToInt(documents.Max(i_document => (float?)i_document["frameRate"] ?? 60f)), 1, 240);
				JArray animations = new JArray(); JObject sourceDocuments = new JObject(); JObject fileNames = new JObject();
				foreach (JObject document in documents)
				{
					string animationName = Slug(((string)document["id"])?.Split('/').LastOrDefault() ?? (string)document["displayName"]);
					animations.Add(ConvertAnimation(document, bonesByName, frameRate, authoringPixelsPerUnit, animationName));
					sourceDocuments[animationName] = document.DeepClone();
					fileNames[animationName] = FilePath.GetFileName(Directory.GetFiles(i_animationDirectory, "*.json").First(i_file => JObject.Parse(File.ReadAllText(i_file))["id"]?.Value<string>() == document["id"]?.Value<string>()));
				}

				JObject skeleton = new JObject
				{
					["name"] = armatureName, ["version"] = "5.5", ["compatibleVersion"] = "5.5", ["frameRate"] = frameRate,
					["armature"] = new JArray(new JObject { ["name"] = armatureName, ["frameRate"] = frameRate, ["type"] = "Armature",
						["bone"] = dragonBones, ["slot"] = slots, ["skin"] = new JArray(new JObject { ["name"] = "default", ["slot"] = skinSlots }),
						["animation"] = animations })
				};
				File.WriteAllText(FilePath.Combine(i_outputDirectory, armatureName + "_ske.json"), skeleton.ToString(Formatting.Indented));
				JObject sidecar = new JObject { ["schemaVersion"] = 1, ["type"] = "captivityDragonBonesRoundTrip", ["armature"] = armatureName,
					["pixelsPerUnit"] = authoringPixelsPerUnit, ["sourcePixelsPerUnit"] = i_pixelsPerUnit, ["imageScale"] = i_imageScale,
					["sourceDirectory"] = FilePath.GetFullPath(i_animationDirectory),
					["rig"] = rig.DeepClone(), ["sourceDocuments"] = sourceDocuments, ["fileNames"] = fileNames };
				File.WriteAllText(FilePath.Combine(i_outputDirectory, "captivity-roundtrip.json"), sidecar.ToString(Formatting.Indented));
				LastAnimationCount = documents.Count; LastImageCount = imageCount;
			}
			finally { if (prefabRoot != null) PrefabUtility.UnloadPrefabContents(prefabRoot); }
		}

		private static void ExportPairedFamily(string i_root, string i_family, JObject i_catalog, float i_pixelsPerUnit, int i_imageScale)
		{
			string definitionPath = FilePath.Combine(i_root, "ExampleMods", "animation-reference-gallery", "content", i_family + "-reference.json");
			JObject definition = JObject.Parse(File.ReadAllText(definitionPath));
			string coreEnemy = (string)definition["extends"];
			JObject catalogEnemy = ((JArray)i_catalog["enemies"] ?? new JArray()).OfType<JObject>().FirstOrDefault(i_enemy => (string)i_enemy["id"] == coreEnemy)
				?? throw new InvalidDataException("The animation catalog has no entry for " + coreEnemy + ".");
			JArray pairs = (JArray)catalogEnemy["interactionPairs"] ?? new JArray();
			if (pairs.Count == 0) throw new InvalidDataException(i_family + " has no paired player animations in the Core catalog.");

			string output = FilePath.Combine(i_root, "DragonBonesExports", i_family + "-paired");
			string enemyOutput = FilePath.Combine(output, "source-enemy");
			string playerOutput = FilePath.Combine(output, "source-player");
			string enemyAnimations = FilePath.Combine(enemyOutput, "animations");
			string playerAnimations = FilePath.Combine(playerOutput, "animations");
			Directory.CreateDirectory(enemyAnimations); Directory.CreateDirectory(playerAnimations);
			foreach (string oldFile in Directory.GetFiles(enemyAnimations, "*.json", SearchOption.TopDirectoryOnly)) File.Delete(oldFile);
			foreach (string oldFile in Directory.GetFiles(playerAnimations, "*.json", SearchOption.TopDirectoryOnly)) File.Delete(oldFile);
			JObject playerRig = NormalizedPlayerAnimationExporter.BuildRigDocument();
			string playerRigPath = FilePath.Combine(playerOutput, "player-rig.json");
			File.WriteAllText(playerRigPath, playerRig.ToString(Formatting.Indented));
			foreach (JObject pair in pairs.OfType<JObject>())
			{
				string semantic = (string)pair["semanticName"] ?? "finisher-phase-" + ((int?)pair["phase"] ?? 1);
				string enemySource = Directory.GetFiles(FilePath.Combine(i_root, "ExampleMods", "animation-reference-gallery", "content", "animations", i_family), "*.json", SearchOption.TopDirectoryOnly)
					.FirstOrDefault(i_file => string.Equals(((string)JObject.Parse(File.ReadAllText(i_file))["id"])?.Split('/').LastOrDefault(), semantic, StringComparison.Ordinal));
				if (enemySource == null) throw new InvalidDataException("Could not find " + i_family + " enemy animation " + semantic + ".");
				File.Copy(enemySource, FilePath.Combine(enemyAnimations, FilePath.GetFileName(enemySource)), true);
				string playerAsset = (string)pair["playerAsset"];
				JObject playerDocument = NormalizedPlayerAnimationExporter.BuildClipDocument(playerAsset, "core:player-animation/" + semantic);
				File.WriteAllText(FilePath.Combine(playerAnimations, semantic + ".player-animation.json"), playerDocument.ToString(Formatting.Indented));
			}

			Export(FilePath.Combine(i_root, "ExampleMods", "animation-reference-gallery", "reference", "rigs", i_family, "rig.json"),
				enemyAnimations, enemyOutput, i_pixelsPerUnit, i_imageScale);
			string enemyArmatureName = LastArmatureName;
			Export(playerRigPath, playerAnimations, playerOutput, i_pixelsPerUnit, i_imageScale);
			string playerArmatureName = LastArmatureName;
			CombinePairedExports(i_family, output, enemyOutput, enemyArmatureName, playerOutput, playerArmatureName);
		}

		private static void CombinePairedExports(string i_family, string i_output, string i_enemyOutput, string i_enemyArmatureName, string i_playerOutput, string i_playerArmatureName)
		{
			JObject enemySkeleton = JObject.Parse(File.ReadAllText(FilePath.Combine(i_enemyOutput, i_enemyArmatureName + "_ske.json")));
			JObject playerSkeleton = JObject.Parse(File.ReadAllText(FilePath.Combine(i_playerOutput, i_playerArmatureName + "_ske.json")));
			JObject enemySidecar = JObject.Parse(File.ReadAllText(FilePath.Combine(i_enemyOutput, "captivity-roundtrip.json")));
			JObject playerSidecar = JObject.Parse(File.ReadAllText(FilePath.Combine(i_playerOutput, "captivity-roundtrip.json")));
			JObject enemyArmature = ((JArray)enemySkeleton["armature"])[0] as JObject;
			JObject playerArmature = ((JArray)playerSkeleton["armature"])[0] as JObject;
			string imageDirectory = FilePath.Combine(i_output, "images"); Directory.CreateDirectory(imageDirectory);
			PrefixArmature(enemyArmature, "enemy", FilePath.Combine(i_enemyOutput, "images"), imageDirectory);
			PrefixArmature(playerArmature, "player", FilePath.Combine(i_playerOutput, "images"), imageDirectory);

			int frameRate = Math.Max((int?)enemyArmature["frameRate"] ?? 60, (int?)playerArmature["frameRate"] ?? 60);
			Dictionary<string, JObject> playerAnimations = ((JArray)playerArmature["animation"] ?? new JArray()).OfType<JObject>()
				.ToDictionary(i_animation => (string)i_animation["name"], StringComparer.Ordinal);
			JArray animations = new JArray();
			foreach (JObject enemyAnimation in ((JArray)enemyArmature["animation"] ?? new JArray()).OfType<JObject>())
			{
				string name = (string)enemyAnimation["name"];
				if (!playerAnimations.TryGetValue(name, out JObject playerAnimation)) continue;
				JObject combined = (JObject)enemyAnimation.DeepClone();
				combined["duration"] = Math.Max((int?)enemyAnimation["duration"] ?? 1, (int?)playerAnimation["duration"] ?? 1);
				combined["playTimes"] = ((int?)enemyAnimation["playTimes"] ?? 1) == 0 || ((int?)playerAnimation["playTimes"] ?? 1) == 0 ? 0 : 1;
				JArray timelines = (JArray)combined["bone"] ?? new JArray();
				foreach (JObject timeline in ((JArray)playerAnimation["bone"] ?? new JArray()).OfType<JObject>()) timelines.Add(timeline.DeepClone());
				combined["bone"] = timelines;
				animations.Add(combined);
			}
			if (animations.Count == 0) throw new InvalidDataException("No paired animation names matched for " + i_family + ".");

			JArray bones = new JArray(new JObject { ["name"] = "pair-root" });
			foreach (JObject bone in ((JArray)enemyArmature["bone"] ?? new JArray()).OfType<JObject>()) { if (bone["parent"] == null) bone["parent"] = "pair-root"; bones.Add(bone.DeepClone()); }
			foreach (JObject bone in ((JArray)playerArmature["bone"] ?? new JArray()).OfType<JObject>()) { if (bone["parent"] == null) bone["parent"] = "pair-root"; bones.Add(bone.DeepClone()); }
			List<JObject> orderedSlots = ((JArray)enemyArmature["slot"] ?? new JArray()).OfType<JObject>().Concat(((JArray)playerArmature["slot"] ?? new JArray()).OfType<JObject>())
				.Select((i_slot, i_index) => new { Slot = i_slot, Index = i_index })
				.OrderBy(i_item => (int?)i_item.Slot["userData"]?["captivitySortingOrder"] ?? 0)
				.ThenBy(i_item => ((string)i_item.Slot["name"])?.StartsWith("enemy-", StringComparison.Ordinal) == true ? 1 : 0)
				.ThenBy(i_item => i_item.Index).Select(i_item => i_item.Slot).ToList();
			Dictionary<string, JObject> skinSlots = GetSkinSlots(enemyArmature).Concat(GetSkinSlots(playerArmature)).OfType<JObject>()
				.ToDictionary(i_slot => (string)i_slot["name"], StringComparer.Ordinal);
			foreach (JObject animation in animations.OfType<JObject>())
			{
				string animationName = (string)animation["name"];
				JObject enemyDocument = (JObject)enemySidecar["sourceDocuments"]?[animationName];
				JObject playerDocument = (JObject)playerSidecar["sourceDocuments"]?[animationName];
				JObject zOrder = BuildZOrderTimeline(orderedSlots, enemyDocument, playerDocument, frameRate, (int?)animation["duration"] ?? 1);
				if (zOrder != null) animation["zOrder"] = zOrder;
			}
			string armatureName = i_family + "-paired";
			JObject combinedArmature = new JObject { ["name"] = armatureName, ["frameRate"] = frameRate, ["type"] = "Armature", ["bone"] = bones,
				["slot"] = new JArray(orderedSlots.Select(i_slot => i_slot.DeepClone())),
				["skin"] = new JArray(new JObject { ["name"] = "default", ["slot"] = new JArray(orderedSlots.Where(i_slot => skinSlots.ContainsKey((string)i_slot["name"])).Select(i_slot => skinSlots[(string)i_slot["name"]].DeepClone())) }),
				["animation"] = animations };
			JObject skeleton = new JObject { ["name"] = armatureName, ["version"] = "5.5", ["compatibleVersion"] = "5.5", ["frameRate"] = frameRate, ["armature"] = new JArray(combinedArmature) };
			File.WriteAllText(FilePath.Combine(i_output, armatureName + "_ske.json"), skeleton.ToString(Formatting.Indented));

			JObject sidecar = new JObject { ["schemaVersion"] = 1, ["type"] = "captivityDragonBonesPairedRoundTrip", ["armature"] = armatureName,
				["enemyPrefix"] = "enemy-", ["playerPrefix"] = "player-",
				["enemy"] = enemySidecar, ["player"] = playerSidecar };
			File.WriteAllText(FilePath.Combine(i_output, "captivity-paired-roundtrip.json"), sidecar.ToString(Formatting.Indented));
			File.WriteAllText(FilePath.Combine(i_output, "README.txt"), "Import " + armatureName + "_ske.json as Images and select this folder's images directory. Both rigs share one armature and one playhead. Enemy bones use enemy-; player bones use player-.\r\n");
			LastArmatureName = armatureName; LastAnimationCount = animations.Count;
		}

		private static JObject BuildZOrderTimeline(IReadOnlyList<JObject> i_slots, JObject i_enemyDocument, JObject i_playerDocument, int i_fps, int i_frameCount)
		{
			if (i_slots.Count == 0) return null;
			Dictionary<string, JObject> enemyTracks = SortingTracks(i_enemyDocument);
			Dictionary<string, JObject> playerTracks = SortingTracks(i_playerDocument);
			JArray frames = new JArray(); JArray previousOrder = null; int previousDuration = 0; bool anyChanged = false;
			for (int frame = 0; frame < Math.Max(1, i_frameCount); frame++)
			{
				float time = frame / (float)Math.Max(1, i_fps);
				List<int> ordered = Enumerable.Range(0, i_slots.Count)
					.OrderBy(i_index => SlotSortingOrder(i_slots[i_index], enemyTracks, playerTracks, time))
					.ThenBy(i_index => ((string)i_slots[i_index]["name"])?.StartsWith("enemy-", StringComparison.Ordinal) == true ? 1 : 0)
					.ThenBy(i_index => i_index).ToList();
				JArray rawOrder = new JArray();
				for (int originalIndex = 0; originalIndex < i_slots.Count; originalIndex++)
				{
					int targetIndex = ordered.IndexOf(originalIndex); rawOrder.Add(originalIndex); rawOrder.Add(targetIndex - originalIndex);
					if (targetIndex != originalIndex) anyChanged = true;
				}
				if (previousOrder != null && JToken.DeepEquals(previousOrder, rawOrder)) { previousDuration++; continue; }
				if (previousOrder != null) frames.Add(new JObject { ["duration"] = previousDuration, ["zOrder"] = previousOrder });
				previousOrder = rawOrder; previousDuration = 1;
			}
			if (previousOrder != null) frames.Add(new JObject { ["duration"] = previousDuration, ["zOrder"] = previousOrder });
			return anyChanged ? new JObject { ["frame"] = frames } : null;
		}

		private static Dictionary<string, JObject> SortingTracks(JObject i_document)
		{
			return ((JArray)i_document?["tracks"] ?? new JArray()).OfType<JObject>()
				.Where(i_track => (string)i_track["property"] == "sortingOrder" && ((string)i_track["target"])?.StartsWith("sprite/", StringComparison.Ordinal) == true)
				.GroupBy(i_track => ((string)i_track["target"]).Substring(7), StringComparer.Ordinal)
				.ToDictionary(i_group => i_group.Key, i_group => i_group.Last(), StringComparer.Ordinal);
		}

		private static int SlotSortingOrder(JObject i_slot, IDictionary<string, JObject> i_enemyTracks, IDictionary<string, JObject> i_playerTracks, float i_time)
		{
			string parent = (string)i_slot["parent"] ?? string.Empty; bool enemy = parent.StartsWith("enemy-", StringComparison.Ordinal);
			string prefix = enemy ? "enemy-" : "player-"; string bone = parent.StartsWith(prefix, StringComparison.Ordinal) ? parent.Substring(prefix.Length) : parent;
			IDictionary<string, JObject> tracks = enemy ? i_enemyTracks : i_playerTracks;
			int fallback = (int?)i_slot["userData"]?["captivitySortingOrder"] ?? 0;
			return tracks.TryGetValue(bone, out JObject track) ? Mathf.RoundToInt(EvaluateStep(track, i_time, fallback)) : fallback;
		}

		private static float EvaluateStep(JObject i_track, float i_time, float i_fallback)
		{
			JObject selected = null;
			foreach (JObject key in ((JArray)i_track?["keys"] ?? new JArray()).OfType<JObject>().OrderBy(i_key => Value(i_key, "time")))
			{
				if (Value(key, "time") > i_time + 0.00001f) break;
				selected = key;
			}
			return selected == null ? i_fallback : Value(selected, "value", i_fallback);
		}

		private static IEnumerable<JToken> GetSkinSlots(JObject i_armature)
		{
			JObject skin = ((JArray)i_armature["skin"] ?? new JArray()).OfType<JObject>().FirstOrDefault();
			return skin == null ? Enumerable.Empty<JToken>() : ((JArray)skin["slot"] ?? new JArray()).Children();
		}

		private static void PrefixArmature(JObject io_armature, string i_prefix, string i_sourceImages, string i_destinationImages)
		{
			string prefix = i_prefix + "-";
			foreach (JObject bone in ((JArray)io_armature["bone"] ?? new JArray()).OfType<JObject>()) { bone["name"] = prefix + (string)bone["name"]; if (bone["parent"] != null) bone["parent"] = prefix + (string)bone["parent"]; }
			foreach (JObject slot in ((JArray)io_armature["slot"] ?? new JArray()).OfType<JObject>()) { slot["name"] = prefix + (string)slot["name"]; slot["parent"] = prefix + (string)slot["parent"]; }
			foreach (JObject skinSlot in GetSkinSlots(io_armature).OfType<JObject>())
			{
				skinSlot["name"] = prefix + (string)skinSlot["name"];
				foreach (JObject display in ((JArray)skinSlot["display"] ?? new JArray()).OfType<JObject>())
				{
					string oldPath = (string)display["path"] ?? (string)display["name"];
					string newPath = prefix + oldPath; display["name"] = newPath; display["path"] = newPath;
					string source = FilePath.Combine(i_sourceImages, oldPath + ".png"), destination = FilePath.Combine(i_destinationImages, newPath + ".png");
					if (File.Exists(source)) File.Copy(source, destination, true);
				}
			}
			foreach (JObject animation in ((JArray)io_armature["animation"] ?? new JArray()).OfType<JObject>())
				foreach (JObject timeline in ((JArray)animation["bone"] ?? new JArray()).OfType<JObject>()) timeline["name"] = prefix + (string)timeline["name"];
		}

		public static int Import(string i_skeletonPath, string i_sidecarPath, string i_outputDirectory, bool i_replaceOriginals)
		{
			if (!File.Exists(i_skeletonPath)) throw new FileNotFoundException("DragonBones skeleton JSON was not found.", i_skeletonPath);
			if (!File.Exists(i_sidecarPath)) throw new FileNotFoundException("Round-trip sidecar was not found.", i_sidecarPath);
			JObject skeleton = JObject.Parse(File.ReadAllText(i_skeletonPath)); JObject sidecar = JObject.Parse(File.ReadAllText(i_sidecarPath));
			string sidecarType = (string)sidecar["type"];
			JObject armature = ((JArray)skeleton["armature"])?.OfType<JObject>().FirstOrDefault() ?? throw new InvalidDataException("The DragonBones JSON contains no armature.");
			if (sidecarType == "captivityDragonBonesPairedRoundTrip")
			{
				if (i_replaceOriginals) throw new InvalidDataException("Paired imports are written to separate enemy and player folders so both sides can be reviewed before replacement.");
				JObject enemySidecar = sidecar["enemy"] as JObject ?? throw new InvalidDataException("The paired sidecar has no enemy data.");
				JObject playerSidecar = sidecar["player"] as JObject ?? throw new InvalidDataException("The paired sidecar has no player data.");
				return ImportSet(armature, enemySidecar, FilePath.Combine(i_outputDirectory, "enemy"), false, (string)sidecar["enemyPrefix"] ?? "enemy-")
					+ ImportSet(armature, playerSidecar, FilePath.Combine(i_outputDirectory, "player"), false, (string)sidecar["playerPrefix"] ?? "player-");
			}
			if (sidecarType != "captivityDragonBonesRoundTrip" && sidecarType != "captivityLoongBonesRoundTrip")
				throw new InvalidDataException("The selected sidecar is not a Captivity DragonBones round-trip document.");
			return ImportSet(armature, sidecar, i_outputDirectory, i_replaceOriginals, null);
		}

		private static int ImportSet(JObject i_armature, JObject i_sidecar, string i_outputDirectory, bool i_replaceOriginals, string i_bonePrefix)
		{
			JObject sourceDocuments = (JObject)i_sidecar["sourceDocuments"] ?? throw new InvalidDataException("The sidecar contains no source documents.");
			JObject fileNames = (JObject)i_sidecar["fileNames"] ?? new JObject(); JObject rig = (JObject)i_sidecar["rig"] ?? throw new InvalidDataException("The sidecar contains no rig.");
			float ppu = (float?)i_sidecar["pixelsPerUnit"] ?? 32f; int fps = (int?)i_armature["frameRate"] ?? 60;
			Dictionary<string, JObject> bones = ((JArray)rig["bones"] ?? new JArray()).OfType<JObject>().Where(i_bone => i_bone["name"] != null).ToDictionary(i_bone => (string)i_bone["name"], StringComparer.Ordinal);
			Directory.CreateDirectory(i_outputDirectory); int count = 0;
			foreach (JObject sourceAnimation in ((JArray)i_armature["animation"] ?? new JArray()).OfType<JObject>())
			{
				JObject animation = (JObject)sourceAnimation.DeepClone();
				if (!string.IsNullOrEmpty(i_bonePrefix))
				{
					JArray selected = new JArray();
					foreach (JObject timeline in ((JArray)animation["bone"] ?? new JArray()).OfType<JObject>())
					{
						string timelineName = (string)timeline["name"];
						if (timelineName == null || !timelineName.StartsWith(i_bonePrefix, StringComparison.Ordinal)) continue;
						JObject clone = (JObject)timeline.DeepClone(); clone["name"] = timelineName.Substring(i_bonePrefix.Length); selected.Add(clone);
					}
					animation["bone"] = selected;
				}
				string name = (string)animation["name"]; JObject source = sourceDocuments[name] as JObject; if (source == null) continue;
				JObject output = (JObject)source.DeepClone(); output["frameRate"] = fps; output["durationSeconds"] = Math.Max(1, (int?)animation["duration"] ?? 1) / (float)Math.Max(1, fps);
				output["loop"] = ((int?)animation["playTimes"] ?? 1) == 0;
				JArray importedTracks = ImportBoneTracks(animation, bones, fps, ppu, (bool)output["loop"]);
				foreach (JObject preservedTrack in ((JArray)source["tracks"] ?? new JArray()).OfType<JObject>().Where(i_track => !(((string)i_track["target"])?.StartsWith("bone/", StringComparison.Ordinal) == true))) importedTracks.Add(preservedTrack.DeepClone());
				output["tracks"] = importedTracks;
				string fileName = (string)fileNames[name] ?? name + ".enemy-animation.json";
				string destination;
				if (i_replaceOriginals)
				{
					string originalDirectory = (string)i_sidecar["sourceDirectory"];
					if (string.IsNullOrEmpty(originalDirectory)) throw new InvalidDataException("Could not locate the original animation directory. Use a separate output directory instead.");
					originalDirectory = FilePath.GetFullPath(originalDirectory);
					if (!Directory.Exists(originalDirectory)) throw new DirectoryNotFoundException("The original animation directory no longer exists: " + originalDirectory);
					destination = FilePath.Combine(originalDirectory, fileName);
				}
				else destination = FilePath.Combine(i_outputDirectory, fileName);
				File.WriteAllText(destination, output.ToString(Formatting.Indented)); count++;
			}
			return count;
		}

		private static JObject ConvertBone(JObject i_bone, float i_ppu)
		{
			JObject setup = (JObject)i_bone["defaultTransform"] ?? new JObject(); JObject result = new JObject { ["name"] = (string)i_bone["name"] };
			if (!string.IsNullOrEmpty((string)i_bone["parent"])) result["parent"] = (string)i_bone["parent"];
			result["transform"] = new JObject { ["x"] = Value(setup, "x") * i_ppu, ["y"] = -Value(setup, "y") * i_ppu,
				["skX"] = -RotationValue(setup), ["skY"] = -RotationValue(setup),
				["scX"] = Value(setup, "scaleX", 1f), ["scY"] = Value(setup, "scaleY", 1f) };
			result["userData"] = new JObject { ["captivityUnityPath"] = (string)i_bone["unityPath"] ?? string.Empty };
			return result;
		}

		private static JObject ConvertAnimation(JObject i_document, Dictionary<string, JObject> i_bones, int i_fps, float i_ppu, string i_name)
		{
			float duration = (float?)i_document["durationSeconds"] ?? 1f; bool loop = (bool?)i_document["loop"] ?? false; int frameCount = Mathf.Max(1, Mathf.RoundToInt(duration * i_fps));
			JObject result = new JObject { ["name"] = i_name, ["duration"] = frameCount, ["playTimes"] = loop ? 0 : 1 };
			List<JObject> tracks = ((JArray)i_document["tracks"] ?? new JArray()).OfType<JObject>().ToList(); JArray timelines = new JArray();
			foreach (IGrouping<string, JObject> group in tracks.Where(i_track => ((string)i_track["target"])?.StartsWith("bone/", StringComparison.Ordinal) == true)
				.GroupBy(i_track => ((string)i_track["target"]).Substring(5), StringComparer.Ordinal))
			{
				if (!i_bones.TryGetValue(group.Key, out JObject bone)) continue; JObject timeline = new JObject { ["name"] = group.Key };
				JObject x = group.FirstOrDefault(i_track => (string)i_track["property"] == "position.x"), y = group.FirstOrDefault(i_track => (string)i_track["property"] == "position.y");
				JObject rotation = group.FirstOrDefault(i_track => (string)i_track["property"] == "rotation.z"); JObject scaleX = group.FirstOrDefault(i_track => (string)i_track["property"] == "scale.x"), scaleY = group.FirstOrDefault(i_track => (string)i_track["property"] == "scale.y");
				JObject setup = (JObject)bone["defaultTransform"] ?? new JObject();
				if (x != null || y != null) timeline["translateFrame"] = SampleFrames(frameCount, i_fps, duration, loop, i_frame => new JObject { ["duration"] = 1,
					["x"] = (Evaluate(x, SampleTime(i_frame, frameCount, i_fps, duration, loop), Value(setup, "x")) - Value(setup, "x")) * i_ppu,
					["y"] = -(Evaluate(y, SampleTime(i_frame, frameCount, i_fps, duration, loop), Value(setup, "y")) - Value(setup, "y")) * i_ppu });
				if (rotation != null) timeline["rotateFrame"] = SampleFrames(frameCount, i_fps, duration, loop, i_frame => new JObject { ["duration"] = 1, ["clockwise"] = 0,
					["rotate"] = -(Evaluate(rotation, SampleTime(i_frame, frameCount, i_fps, duration, loop), RotationValue(setup)) - RotationValue(setup)), ["skew"] = 0f });
				if (scaleX != null || scaleY != null) timeline["scaleFrame"] = SampleFrames(frameCount, i_fps, duration, loop, i_frame => new JObject { ["duration"] = 1,
					["x"] = SafeRatio(Evaluate(scaleX, SampleTime(i_frame, frameCount, i_fps, duration, loop), Value(setup, "scaleX", 1f)), Value(setup, "scaleX", 1f)),
					["y"] = SafeRatio(Evaluate(scaleY, SampleTime(i_frame, frameCount, i_fps, duration, loop), Value(setup, "scaleY", 1f)), Value(setup, "scaleY", 1f)) });
				if (timeline.Count > 1) timelines.Add(timeline);
			}
			result["bone"] = timelines; JArray markers = ExportMarkers(i_document, frameCount, i_fps); if (markers.Count > 0) result["frame"] = markers;
			return result;
		}

		private static JArray ImportBoneTracks(JObject i_animation, Dictionary<string, JObject> i_bones, int i_fps, float i_ppu, bool i_loop)
		{
			JArray tracks = new JArray(); int totalFrames = Math.Max(1, (int?)i_animation["duration"] ?? 1);
			foreach (JObject timeline in ((JArray)i_animation["bone"] ?? new JArray()).OfType<JObject>())
			{
				string boneName = (string)timeline["name"]; if (!i_bones.TryGetValue(boneName, out JObject bone)) continue; JObject setup = (JObject)bone["defaultTransform"] ?? new JObject();
				JArray translate = timeline["translateFrame"] as JArray;
				if (translate != null) { tracks.Add(ImportTrack(boneName, "position.x", translate, i_fps, totalFrames, i_loop, i_frame => Value(setup, "x") + Value(i_frame, "x") / i_ppu)); tracks.Add(ImportTrack(boneName, "position.y", translate, i_fps, totalFrames, i_loop, i_frame => Value(setup, "y") - Value(i_frame, "y") / i_ppu)); }
				JArray rotate = timeline["rotateFrame"] as JArray;
				if (rotate != null) tracks.Add(ImportTrack(boneName, "rotation.z", rotate, i_fps, totalFrames, i_loop, i_frame => RotationValue(setup) - Value(i_frame, "rotate")));
				JArray scale = timeline["scaleFrame"] as JArray;
				if (scale != null) { tracks.Add(ImportTrack(boneName, "scale.x", scale, i_fps, totalFrames, i_loop, i_frame => Value(setup, "scaleX", 1f) * Value(i_frame, "x", 1f))); tracks.Add(ImportTrack(boneName, "scale.y", scale, i_fps, totalFrames, i_loop, i_frame => Value(setup, "scaleY", 1f) * Value(i_frame, "y", 1f))); }
			}
			return tracks;
		}

		private static JObject ImportTrack(string i_bone, string i_property, JArray i_frames, int i_fps, int i_totalFrames, bool i_loop, Func<JObject, float> i_value)
		{
			JArray keys = new JArray(); int cursor = 0; JObject first = null, last = null;
			foreach (JObject frame in i_frames.OfType<JObject>()) { if (first == null) first = frame; last = frame; keys.Add(new JObject { ["time"] = cursor / (float)i_fps, ["value"] = i_value(frame) }); cursor += Math.Max(1, (int?)frame["duration"] ?? 1); }
			JObject endpoint = i_loop ? first : last; if (endpoint != null) keys.Add(new JObject { ["time"] = i_totalFrames / (float)i_fps, ["value"] = i_value(endpoint) });
			return new JObject { ["target"] = "bone/" + i_bone, ["property"] = i_property, ["keys"] = keys };
		}

		private static JArray ExportMarkers(JObject i_document, int i_frameCount, int i_fps)
		{
			Dictionary<int, List<JObject>> actions = new Dictionary<int, List<JObject>>();
			foreach (JObject animationEvent in ((JArray)i_document["events"] ?? new JArray()).OfType<JObject>()) AddAction(actions, Mathf.Clamp(Mathf.RoundToInt(((float?)animationEvent["time"] ?? 0f) * i_fps), 0, i_frameCount - 1), "cr.event." + ((string)animationEvent["type"] ?? "cue"));
			foreach (JObject trigger in ((JArray)i_document["effectTriggers"] ?? new JArray()).OfType<JObject>()) foreach (string id in ((JArray)trigger["effects"] ?? new JArray()).Values<string>()) AddAction(actions, Mathf.Clamp(Mathf.RoundToInt(((float?)trigger["time"] ?? 0f) * i_fps), 0, i_frameCount - 1), "cr.vfx." + id);
			if (actions.Count == 0) return new JArray(); JArray frames = new JArray(); int cursor = 0;
			foreach (int frameIndex in actions.Keys.OrderBy(i_value => i_value)) { if (frameIndex > cursor) frames.Add(new JObject { ["duration"] = frameIndex - cursor }); frames.Add(new JObject { ["duration"] = 1, ["actions"] = new JArray(actions[frameIndex]) }); cursor = frameIndex + 1; }
			if (cursor < i_frameCount) frames.Add(new JObject { ["duration"] = i_frameCount - cursor }); return frames;
		}

		private static void AddAction(Dictionary<int, List<JObject>> io_actions, int i_frame, string i_name) { if (!io_actions.TryGetValue(i_frame, out List<JObject> list)) io_actions.Add(i_frame, list = new List<JObject>()); list.Add(new JObject { ["type"] = 10, ["name"] = i_name }); }
		private static JArray SampleFrames(int i_count, int i_fps, float i_duration, bool i_loop, Func<int, JObject> i_create) { JArray result = new JArray(); for (int index = 0; index < i_count; index++) result.Add(i_create(index)); return result; }
		private static float SampleTime(int i_frame, int i_frameCount, int i_fps, float i_duration, bool i_loop) { return !i_loop && i_frame == i_frameCount - 1 ? i_duration : Math.Min(i_duration, i_frame / (float)i_fps); }

		private static float Evaluate(JObject i_track, float i_time, float i_fallback)
		{
			JArray keys = i_track?["keys"] as JArray; if (keys == null || keys.Count == 0) return i_fallback; List<JObject> values = keys.OfType<JObject>().ToList();
			if (i_time <= Value(values[0], "time")) return Value(values[0], "value", i_fallback); if (i_time >= Value(values[values.Count - 1], "time")) return Value(values[values.Count - 1], "value", i_fallback);
			int upper = 1; while (upper < values.Count && Value(values[upper], "time") < i_time) upper++; JObject left = values[upper - 1], right = values[upper];
			float leftTime = Value(left, "time"), rightTime = Value(right, "time"), duration = rightTime - leftTime, t = duration <= 0f ? 1f : (i_time - leftTime) / duration;
			float leftValue = Value(left, "value"), rightValue = Value(right, "value"); float? outTangent = (float?)left["outTangent"], inTangent = (float?)right["inTangent"];
			if (!outTangent.HasValue || !inTangent.HasValue) return Mathf.LerpUnclamped(leftValue, rightValue, t); float t2 = t * t, t3 = t2 * t;
			return (2f * t3 - 3f * t2 + 1f) * leftValue + (t3 - 2f * t2 + t) * duration * outTangent.Value + (-2f * t3 + 3f * t2) * rightValue + (t3 - t2) * duration * inTangent.Value;
		}

		private static JObject DisplayTransform(Transform i_rigRoot, JObject i_bone, JObject i_sprite, Sprite i_spriteAsset, float i_ppu)
		{
			if (i_rigRoot == null) return new JObject();
			Transform bone = Find(i_rigRoot, (string)i_bone["unityPath"]), display = Find(i_rigRoot, (string)i_sprite["unityPath"]);
			if (bone == null || display == null) throw new InvalidDataException("Could not resolve sprite '" + (string)i_sprite["unityPath"] + "' relative to the rig sample root.");
			float sourcePpu = i_spriteAsset.pixelsPerUnit > 0f ? i_spriteAsset.pixelsPerUnit : 32f;
			float pivotX = (float?)i_sprite["pivotX"] ?? i_spriteAsset.pivot.x / i_spriteAsset.rect.width;
			float pivotY = (float?)i_sprite["pivotY"] ?? i_spriteAsset.pivot.y / i_spriteAsset.rect.height;
			Vector3 centerFromPivot = new Vector3((0.5f - pivotX) * i_spriteAsset.rect.width / sourcePpu,
				(0.5f - pivotY) * i_spriteAsset.rect.height / sourcePpu, 0f);
			Vector3 centerWorld = display.position + display.TransformVector(centerFromPivot);
			Vector3 local = bone.InverseTransformPoint(centerWorld); float rotation = Mathf.DeltaAngle(bone.eulerAngles.z, display.eulerAngles.z); Vector3 boneScale = bone.lossyScale, displayScale = display.lossyScale;
			return new JObject { ["x"] = local.x * i_ppu, ["y"] = -local.y * i_ppu, ["skX"] = -rotation, ["skY"] = -rotation,
				["scX"] = SafeRatio(displayScale.x, boneScale.x), ["scY"] = SafeRatio(displayScale.y, boneScale.y) };
		}

		private static Transform ResolveRigRoot(GameObject i_prefabRoot, string i_sampleRoot, JArray i_bones)
		{
			if (i_prefabRoot == null) return null;
			Transform direct = Find(i_prefabRoot.transform, i_sampleRoot);
			List<Transform> candidates = i_prefabRoot.GetComponentsInChildren<Transform>(true)
				.Where(i_transform => string.IsNullOrEmpty(i_sampleRoot) || i_transform.name == FilePath.GetFileName(i_sampleRoot.Replace('\\', '/'))).ToList();
			if (direct != null && !candidates.Contains(direct)) candidates.Add(direct);
			if (candidates.Count == 0) candidates.Add(i_prefabRoot.transform);
			return candidates.OrderByDescending(i_candidate => i_bones.OfType<JObject>().Sum(i_bone =>
			{
				int score = Find(i_candidate, (string)i_bone["unityPath"]) != null ? 1 : 0;
				JArray sprites = i_bone["sprites"] as JArray;
				if (sprites != null) score += sprites.OfType<JObject>().Count(i_sprite => Find(i_candidate, (string)i_sprite["unityPath"]) != null) * 4;
				return score;
			})).FirstOrDefault();
		}

		private static GameObject LoadRigPrefab(string i_path) { if (string.IsNullOrEmpty(i_path)) return null; GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(i_path); return prefab == null ? null : PrefabUtility.LoadPrefabContents(i_path); }
		private static Transform Find(Transform i_root, string i_path) { return string.IsNullOrEmpty(i_path) ? i_root : i_root.Find(i_path); }
		private static Sprite LoadSprite(string i_assetPath, string i_name) { if (string.IsNullOrEmpty(i_assetPath)) return null; Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(i_assetPath); if (direct != null) return direct; return AssetDatabase.LoadAllAssetsAtPath(i_assetPath).OfType<Sprite>().FirstOrDefault(i_sprite => string.IsNullOrEmpty(i_name) || i_sprite.name == i_name); }

		private static void ExportSpritePng(Sprite i_sprite, string i_destination, int i_scale)
		{
			Rect rect = i_sprite.textureRect; int width = Mathf.Max(1, Mathf.RoundToInt(rect.width)), height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
			RenderTexture temporary = RenderTexture.GetTemporary(i_sprite.texture.width, i_sprite.texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB); RenderTexture previous = RenderTexture.active; FilterMode previousFilter = i_sprite.texture.filterMode;
			Texture2D cropped = null, scaled = null;
			try
			{
				i_sprite.texture.filterMode = FilterMode.Point; temporary.filterMode = FilterMode.Point; Graphics.Blit(i_sprite.texture, temporary); RenderTexture.active = temporary;
				cropped = new Texture2D(width, height, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
				cropped.ReadPixels(new Rect(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height), 0, 0); cropped.Apply();
				if (i_scale == 1) File.WriteAllBytes(i_destination, cropped.EncodeToPNG());
				else
				{
					Color32[] source = cropped.GetPixels32(), destination = new Color32[width * i_scale * height * i_scale]; int scaledWidth = width * i_scale;
					for (int y = 0; y < height * i_scale; y++) for (int x = 0; x < scaledWidth; x++) destination[y * scaledWidth + x] = source[(y / i_scale) * width + x / i_scale];
					scaled = new Texture2D(scaledWidth, height * i_scale, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
					scaled.SetPixels32(destination); scaled.Apply(); File.WriteAllBytes(i_destination, scaled.EncodeToPNG());
				}
			}
			finally { i_sprite.texture.filterMode = previousFilter; RenderTexture.active = previous; RenderTexture.ReleaseTemporary(temporary); if (cropped != null) UnityEngine.Object.DestroyImmediate(cropped); if (scaled != null) UnityEngine.Object.DestroyImmediate(scaled); }
		}

		private static string UniqueResourceName(string i_root, IDictionary<string, string> i_existing) { string result = string.IsNullOrEmpty(i_root) ? "sprite" : i_root; int suffix = 2; HashSet<string> values = new HashSet<string>(i_existing.Values, StringComparer.OrdinalIgnoreCase); while (values.Contains(result)) result = i_root + "-" + suffix++; return result; }
		private static string Slug(string i_value) { if (string.IsNullOrWhiteSpace(i_value)) return "armature"; return new string(i_value.Trim().ToLowerInvariant().Select(i_char => char.IsLetterOrDigit(i_char) || i_char == '-' || i_char == '_' ? i_char : '-').ToArray()).Trim('-'); }
		private static float Value(JObject i_object, string i_name, float i_fallback = 0f) { return (float?)i_object?[i_name] ?? i_fallback; }
		private static float RotationValue(JObject i_object) { return i_object?["rotationZ"] != null ? Value(i_object, "rotationZ") : Value(i_object, "rotation"); }
		private static float SafeRatio(float i_value, float i_divisor) { return Mathf.Abs(i_divisor) < 0.00001f ? 1f : i_value / i_divisor; }
		private static bool Finite(float i_value) { return !float.IsNaN(i_value) && !float.IsInfinity(i_value); }
	}
}
