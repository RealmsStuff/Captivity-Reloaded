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
	public sealed class NewEnemyWizardWindow : EditorWindow
	{
		private enum Topology { Humanoid, Quadruped, Flying, Crawler, Blank }

		[SerializeField] private int m_mode;
		[SerializeField] private string m_modFolder = "ExampleMods/yourname.my-mod";
		[SerializeField] private string m_slug = "new-enemy";
		[SerializeField] private string m_displayName = "New Enemy";
		[SerializeField] private string m_description = "A fully original enemy.";
		[SerializeField] private Topology m_topology = Topology.Humanoid;
		[SerializeField] private int m_aiPresetIndex;
		[SerializeField] private string m_aiType = "groundChase";
		[SerializeField] private float m_aiPreferredRange = .8f;
		[SerializeField] private float m_aiRetreatRange;
		[SerializeField] private float m_aiReactionSeconds = .08f;
		[SerializeField] private float m_aiVisionRange = 22f;
		[SerializeField] private float m_aiSpeedAcceleration = 6f;
		[SerializeField] private float m_aiSpeedMax = 3.2f;
		[SerializeField] private float m_aiTraction = .15f;
		[SerializeField] private int m_attackPresetIndex;
		[SerializeField] private List<AttackDraft> m_attackDrafts = new List<AttackDraft> { AttackDraft.Melee("primary") };
		[SerializeField] private float m_healthMax = 50f;
		[SerializeField] private int m_bounty = 25;
		[SerializeField] private float m_healthIncreasePerWave = 1f;
		[SerializeField] private float m_spawnSelectionWeight = 1f;
		[SerializeField] private List<BehaviorDraft> m_behaviorDrafts = new List<BehaviorDraft>();
		[SerializeField] private int m_newBehaviorType;
		[SerializeField] private float m_dropChance;
		[SerializeField] private string m_dropItemIds = string.Empty;
		[SerializeField] private bool m_generateStarterFinisher;
		[SerializeField] private bool m_createTestStage = true;
		[SerializeField] private bool m_createDragonBonesProject = true;
		[SerializeField] private bool m_includePairedPlayer;
		[SerializeField] private int m_dragonBonesImageScale = 4;
		[SerializeField] private string m_importSkeletonPath = string.Empty;
		[SerializeField] private string m_importSidecarPath = string.Empty;
		[SerializeField] private bool m_finisherUsePhases;
		[SerializeField] private int m_finisherEnemyClipIndex;
		[SerializeField] private int m_finisherPlayerClipIndex;
		[SerializeField] private int m_finisherEnemyClipIndex2;
		[SerializeField] private int m_finisherPlayerClipIndex2;
		[SerializeField] private string m_finisherPhase1 = "restraint";
		[SerializeField] private string m_finisherPhase2 = "intense";
		[SerializeField] private float m_finisherDuration = 4f;
		[SerializeField] private float m_finisherDuration2 = 4f;
		[SerializeField] private float m_finisherTriggerRange = 1.5f;
		[SerializeField] private float m_finisherStartDelay = .25f;
		[SerializeField] private float m_finisherMeterMax = 100f;
		[SerializeField] private float m_finisherInputPower = 14f;
		[SerializeField] private float m_finisherDecay = 2f;
		[SerializeField] private float m_finisherCooldown = 4f;
		[SerializeField] private float m_finisherSuccessRecovery = 8f;
		[SerializeField] private float m_finisherSuccessStun = 2.5f;
		[SerializeField] private float m_finisherFailureHealth = 20f;
		[SerializeField] private float m_finisherFailureStrength = 4f;
		[SerializeField] private float m_finisherFailurePleasure = 5f;
		[SerializeField] private float m_finisherFailureLibido = 2f;
		[SerializeField] private float m_finisherFailureRagdoll = 1f;
		[SerializeField] private bool m_preserveExistingAdditionalPhases = true;
		private readonly List<FinisherEnemyClip> m_finisherEnemyClips = new List<FinisherEnemyClip>();
		private readonly List<FinisherPlayerClip> m_finisherPlayerClips = new List<FinisherPlayerClip>();
		private string m_finisherEnemyPath;
		private Vector2 m_scroll;
		private string m_status;
		private MessageType m_statusType;

		[MenuItem("Captivity Reloaded/Modding/Create Original Enemy...")]
		public static void Open()
		{
			NewEnemyWizardWindow window = GetWindow<NewEnemyWizardWindow>();
			window.titleContent = new GUIContent("New Enemy");
			window.minSize = new Vector2(570f, 480f);
			window.Show();
		}

		private void OnGUI()
		{
			m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
			EditorGUILayout.Space(8f);
			m_mode = GUILayout.Toolbar(m_mode, new[] { "Create enemy", "Import DragonBones", "Paired interaction" });
			EditorGUILayout.Space(8f);
			if (m_mode == 0) DrawCreate(); else if (m_mode == 1) DrawSafeImport(); else DrawFinisherWizard();
			if (!string.IsNullOrEmpty(m_status)) EditorGUILayout.HelpBox(m_status, m_statusType);
			EditorGUILayout.EndScrollView();
		}

		private void DrawCreate()
		{
			EditorGUILayout.LabelField("Create a fully original enemy", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Creates a schema-valid original enemy, atlas guide, idle/move/attack animation documents, and an optional isolated test stage inside an existing loose mod.", MessageType.Info);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_modFolder = EditorGUILayout.TextField("Existing mod folder", m_modFolder);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFolderPanel("Choose the folder containing manifest.json", ProjectPath(m_modFolder), string.Empty);
					if (!string.IsNullOrEmpty(selected)) m_modFolder = RelativeProjectPath(selected);
				}
			}
			m_slug = EditorGUILayout.TextField("Enemy ID", m_slug);
			m_displayName = EditorGUILayout.TextField("Display name", m_displayName);
			m_description = EditorGUILayout.TextField("Description", m_description);
			EditorGUI.BeginChangeCheck();
			m_topology = (Topology)EditorGUILayout.EnumPopup("Starter topology", m_topology);
			if (EditorGUI.EndChangeCheck()) ApplySuggestedAiPreset();
			DrawAiAuthoring();
			DrawAttackAuthoring();
			DrawStatsAndBehaviors();
			m_createTestStage = EditorGUILayout.ToggleLeft("Create a Field-Day test stage that only spawns this enemy", m_createTestStage);
			m_createDragonBonesProject = EditorGUILayout.ToggleLeft("Create an editable DragonBones project", m_createDragonBonesProject);
			using (new EditorGUI.DisabledScope(!m_createDragonBonesProject))
			{
				m_dragonBonesImageScale = EditorGUILayout.IntSlider("DragonBones pixel scale", m_dragonBonesImageScale, 1, 8);
				m_includePairedPlayer = EditorGUILayout.ToggleLeft("Add a paired enemy + player draft project", m_includePairedPlayer);
				using (new EditorGUI.DisabledScope(!m_includePairedPlayer))
					m_generateStarterFinisher = EditorGUILayout.ToggleLeft("Connect the paired draft as a playable starter grab/finisher", m_generateStarterFinisher);
			}
			if (m_includePairedPlayer && !m_generateStarterFinisher) EditorGUILayout.HelpBox("The paired draft is not connected to gameplay. Author both rigs together, import the edited JSON, then attach the clips through the Paired interaction tab.", MessageType.None);
			else if (m_generateStarterFinisher) EditorGUILayout.HelpBox("The generated grab uses the paired draft immediately. Replace and re-import both animations before shipping the mod.", MessageType.Warning);
			EditorGUILayout.Space(6f);
			DrawPresetSummary();
			EditorGUILayout.Space(8f);
			if (GUILayout.Button("Create Original Enemy Starter", GUILayout.Height(34f))) CreateEnemy();
		}

		private void DrawSafeImport()
		{
			EditorGUILayout.LabelField("Safely import edited DragonBones animations", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("The importer converts into a temporary directory, checks the armature and clip contract, validates a staged copy of the complete mod, and only then replaces matching animation IDs. Originals are backed up outside the mod and restored automatically if live validation fails.", MessageType.Info);
			DrawModFolder();
			m_slug = EditorGUILayout.TextField("Enemy ID", m_slug);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_importSkeletonPath = EditorGUILayout.TextField("Edited skeleton JSON", m_importSkeletonPath);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFilePanel("Choose edited DragonBones skeleton JSON", ExistingParent(m_importSkeletonPath), "json");
					if (!string.IsNullOrEmpty(selected)) m_importSkeletonPath = RelativeProjectPath(selected);
				}
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				m_importSidecarPath = EditorGUILayout.TextField("Round-trip sidecar", m_importSidecarPath);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFilePanel("Choose Captivity round-trip sidecar", ExistingParent(m_importSidecarPath), "json");
					if (!string.IsNullOrEmpty(selected)) m_importSidecarPath = RelativeProjectPath(selected);
				}
			}
			if (GUILayout.Button("Use Generated Project Paths")) SetGeneratedImportPaths();
			EditorGUILayout.Space(8f);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Analyze Only", GUILayout.Height(32f))) RunSafeImport(false);
				if (GUILayout.Button("Validate, Back Up, and Import", GUILayout.Height(32f))) RunSafeImport(true);
			}
			EditorGUILayout.HelpBox("Only documents whose content IDs already exist inside the selected mod can be replaced. New or renamed clips must be created and connected first.", MessageType.None);
		}

		private void DrawModFolder()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				m_modFolder = EditorGUILayout.TextField("Existing mod folder", m_modFolder);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFolderPanel("Choose the folder containing manifest.json", ProjectPath(m_modFolder), string.Empty);
					if (!string.IsNullOrEmpty(selected)) m_modFolder = RelativeProjectPath(selected);
				}
			}
		}

		private void SetGeneratedImportPaths()
		{
			try
			{
				string root = ProjectPath(m_modFolder), manifestPath = FilePath.Combine(root, "manifest.json");
				if (!File.Exists(manifestPath)) throw new InvalidDataException("Choose the loose mod folder first.");
				string packId = (string)JObject.Parse(File.ReadAllText(manifestPath))["id"], slug = Slug(m_slug);
				string directory = FilePath.Combine(ProjectRoot(), "DragonBonesProjects", packId, slug);
				string pairedSkeleton = FilePath.Combine(directory, slug + "-paired_ske.json"), regularSkeleton = FilePath.Combine(directory, slug + "_ske.json");
				bool paired = File.Exists(pairedSkeleton);
				m_importSkeletonPath = RelativeProjectPath(paired ? pairedSkeleton : regularSkeleton);
				m_importSidecarPath = RelativeProjectPath(FilePath.Combine(directory, paired ? "captivity-paired-roundtrip.json" : "captivity-roundtrip.json"));
				m_statusType = MessageType.Info; m_status = "Selected the " + (paired ? "paired" : "enemy-only") + " generated project paths. Choose the edited exported skeleton instead if DragonBones wrote it elsewhere.";
			}
			catch (Exception exception) { m_statusType = MessageType.Error; m_status = exception.Message; }
		}

		private void RunSafeImport(bool i_commit)
		{
			DragonBonesSafeImportResult result = DragonBonesAnimationBridge.SafeImport(ProjectPath(m_importSkeletonPath), ProjectPath(m_importSidecarPath), ProjectPath(m_modFolder), i_commit);
			List<ValidationIssue> errors = result.Report.Issues.Where(i_issue => i_issue.Severity == ValidationSeverity.Error).ToList();
			List<ValidationIssue> warnings = result.Report.Issues.Where(i_issue => i_issue.Severity == ValidationSeverity.Warning).ToList();
			m_statusType = errors.Count > 0 ? MessageType.Error : warnings.Count > 0 ? MessageType.Warning : MessageType.Info;
			string heading = i_commit && result.ReplacedAnimationCount > 0 ? "Imported " + result.ReplacedAnimationCount + " animation(s)."
				: "Analysis converted " + result.ConvertedAnimationCount + " animation(s); no mod files were changed.";
			string backup = string.IsNullOrEmpty(result.BackupDirectory) ? string.Empty : "\nBackup: " + result.BackupDirectory;
			string issues = string.Join("\n", result.Report.Issues.Take(8).Select(i_issue => i_issue.Severity + " " + i_issue.Code + ": " + i_issue.Message).ToArray());
			m_status = heading + backup + (issues.Length == 0 ? "\nAll armature, clip, and staged mod checks passed." : "\n" + issues);
			if (result.ReplacedAnimationCount > 0) AssetDatabase.Refresh();
		}

		private static string ExistingParent(string i_path)
		{
			string full;
			try { full = ProjectPath(i_path); } catch { return ProjectRoot(); }
			if (File.Exists(full)) return FilePath.GetDirectoryName(full); if (Directory.Exists(full)) return full;
			string parent = FilePath.GetDirectoryName(full); return !string.IsNullOrEmpty(parent) && Directory.Exists(parent) ? parent : ProjectRoot();
		}

		private void DrawFinisherWizard()
		{
			EditorGUILayout.LabelField("Configure a paired gameplay interaction", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Connects existing enemy and player animation documents to a runtime downedFinisher. The complete mod is staged and validated before the enemy definition is replaced, and the previous definition is backed up outside the mod.", MessageType.Info);
			DrawModFolder();
			m_slug = EditorGUILayout.TextField("Enemy ID", m_slug);
			if (GUILayout.Button("Discover Connected Animation Clips")) RefreshFinisherChoices();
			if (m_finisherEnemyClips.Count == 0 || m_finisherPlayerClips.Count == 0)
			{
				EditorGUILayout.HelpBox("Discover an original enemy with at least one animationRef and one playerAnimation document.", MessageType.Warning); return;
			}

			m_finisherUsePhases = EditorGUILayout.ToggleLeft("Use two independently configured phases", m_finisherUsePhases);
			m_preserveExistingAdditionalPhases = EditorGUILayout.ToggleLeft("Preserve existing phases after the two shown here", m_preserveExistingAdditionalPhases);
			EditorGUILayout.HelpBox("Existing phase timing is loaded when clips are discovered. Keeping this enabled prevents a two-phase edit from deleting phase 3 and later. Disable it explicitly to replace the complete phase list.", MessageType.None);
			EditorGUILayout.Space(4f);
			if (m_finisherUsePhases)
			{
				DrawFinisherPhase("Phase 1", ref m_finisherPhase1, ref m_finisherEnemyClipIndex, ref m_finisherPlayerClipIndex, ref m_finisherDuration);
				DrawFinisherPhase("Phase 2", ref m_finisherPhase2, ref m_finisherEnemyClipIndex2, ref m_finisherPlayerClipIndex2, ref m_finisherDuration2);
			}
			else
			{
				m_finisherEnemyClipIndex = EditorGUILayout.Popup("Enemy animation", ClampEnemyIndex(m_finisherEnemyClipIndex), EnemyClipLabels());
				m_finisherPlayerClipIndex = EditorGUILayout.Popup("Player animation", ClampPlayerIndex(m_finisherPlayerClipIndex), PlayerClipLabels());
				m_finisherDuration = EditorGUILayout.FloatField("Interaction duration", m_finisherDuration);
			}

			EditorGUILayout.Space(5f); EditorGUILayout.LabelField("Struggle", EditorStyles.boldLabel);
			m_finisherTriggerRange = EditorGUILayout.FloatField("Trigger range", m_finisherTriggerRange);
			m_finisherStartDelay = EditorGUILayout.FloatField("Start delay", m_finisherStartDelay);
			m_finisherMeterMax = EditorGUILayout.FloatField("Escape meter maximum", m_finisherMeterMax);
			m_finisherInputPower = EditorGUILayout.FloatField("Input power", m_finisherInputPower);
			m_finisherDecay = EditorGUILayout.FloatField("Decay per second", m_finisherDecay);
			m_finisherCooldown = EditorGUILayout.FloatField("Cooldown", m_finisherCooldown);

			EditorGUILayout.Space(5f); EditorGUILayout.LabelField("Outcomes", EditorStyles.boldLabel);
			using (new EditorGUILayout.HorizontalScope()) { m_finisherSuccessRecovery = EditorGUILayout.FloatField("Success health", m_finisherSuccessRecovery); m_finisherSuccessStun = EditorGUILayout.FloatField("Enemy stun", m_finisherSuccessStun); }
			using (new EditorGUILayout.HorizontalScope()) { m_finisherFailureHealth = EditorGUILayout.FloatField("Failure health", m_finisherFailureHealth); m_finisherFailureStrength = EditorGUILayout.FloatField("Strength", m_finisherFailureStrength); }
			using (new EditorGUILayout.HorizontalScope()) { m_finisherFailurePleasure = EditorGUILayout.FloatField("Pleasure", m_finisherFailurePleasure); m_finisherFailureLibido = EditorGUILayout.FloatField("Libido", m_finisherFailureLibido); }
			m_finisherFailureRagdoll = EditorGUILayout.FloatField("Failure ragdoll", m_finisherFailureRagdoll);
			EditorGUILayout.Space(8f);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Validate and Configure", GUILayout.Height(34f))) ConfigureFinisher();
				if (GUILayout.Button("Open Pair in Preview", GUILayout.Height(34f))) OpenFinisherPreview();
			}
		}

		private void DrawFinisherPhase(string i_label, ref string io_id, ref int io_enemy, ref int io_player, ref float io_duration)
		{
			EditorGUILayout.LabelField(i_label, EditorStyles.boldLabel); io_id = EditorGUILayout.TextField("Phase ID", io_id);
			io_enemy = EditorGUILayout.Popup("Enemy animation", ClampEnemyIndex(io_enemy), EnemyClipLabels());
			io_player = EditorGUILayout.Popup("Player animation", ClampPlayerIndex(io_player), PlayerClipLabels());
			io_duration = EditorGUILayout.FloatField("Duration", io_duration);
		}

		private string[] EnemyClipLabels() { return m_finisherEnemyClips.Select(i_clip => i_clip.Semantic + " — " + i_clip.DisplayName).ToArray(); }
		private string[] PlayerClipLabels() { return m_finisherPlayerClips.Select(i_clip => i_clip.DisplayName + " — " + i_clip.Id).ToArray(); }
		private int ClampEnemyIndex(int i_index) { return Mathf.Clamp(i_index, 0, Mathf.Max(0, m_finisherEnemyClips.Count - 1)); }
		private int ClampPlayerIndex(int i_index) { return Mathf.Clamp(i_index, 0, Mathf.Max(0, m_finisherPlayerClips.Count - 1)); }

		private void RefreshFinisherChoices()
		{
			try
			{
				m_finisherEnemyClips.Clear(); m_finisherPlayerClips.Clear(); m_finisherEnemyPath = null;
				string root = ProjectPath(m_modFolder), manifestPath = FilePath.Combine(root, "manifest.json");
				if (!File.Exists(manifestPath)) throw new InvalidDataException("Choose a loose mod folder containing manifest.json.");
				JObject manifest = JObject.Parse(File.ReadAllText(manifestPath)); string packId = (string)manifest["id"];
				if (!ContentId.IsValidNamespace(packId)) throw new InvalidDataException("The mod manifest has an invalid ID.");
				string requested = Slug(m_slug); List<KeyValuePair<string, JObject>> documents = new List<KeyValuePair<string, JObject>>();
				foreach (string contentRoot in ((JArray)manifest["contentRoots"] ?? new JArray("content")).Values<string>())
				{
					string directory = FilePath.GetFullPath(FilePath.Combine(root, contentRoot));
					if (!directory.StartsWith(root.TrimEnd(FilePath.DirectorySeparatorChar) + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(directory)) continue;
					foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
						try { documents.Add(new KeyValuePair<string, JObject>(file, JObject.Parse(File.ReadAllText(file)))); } catch (JsonException) { }
				}
				KeyValuePair<string, JObject> enemyEntry = documents.FirstOrDefault(i_entry => (string)i_entry.Value["type"] == "enemy"
					&& ((string)i_entry.Value["id"])?.EndsWith("/" + requested, StringComparison.Ordinal) == true);
				if (enemyEntry.Value == null) throw new InvalidDataException("Could not find an enemy whose content ID ends in /" + requested + ".");
				if ((string)enemyEntry.Value["visual"]?["type"] != "originalSkeletonAtlas") throw new InvalidDataException("Paired interactions currently require an originalSkeletonAtlas enemy.");
				m_finisherEnemyPath = enemyEntry.Key; string enemyId = (string)enemyEntry.Value["id"];
				JObject references = enemyEntry.Value["animationRefs"] as JObject ?? new JObject();
				Dictionary<string, KeyValuePair<string, JObject>> enemyAnimations = documents.Where(i_entry => (string)i_entry.Value["type"] == "enemyAnimation" && (string)i_entry.Value["enemy"] == enemyId)
					.Where(i_entry => i_entry.Value["id"] != null).GroupBy(i_entry => (string)i_entry.Value["id"], StringComparer.Ordinal).ToDictionary(i_group => i_group.Key, i_group => i_group.First(), StringComparer.Ordinal);
				foreach (JProperty reference in references.Properties())
					if (enemyAnimations.TryGetValue((string)reference.Value, out KeyValuePair<string, JObject> animation)) m_finisherEnemyClips.Add(new FinisherEnemyClip
						{ Semantic = reference.Name, Id = (string)reference.Value, DisplayName = (string)animation.Value["displayName"] ?? reference.Name, Path = animation.Key,
							Duration = (float?)animation.Value["durationSeconds"] ?? 1f });
				foreach (KeyValuePair<string, JObject> entry in documents.Where(i_entry => (string)i_entry.Value["type"] == "playerAnimation"))
					m_finisherPlayerClips.Add(new FinisherPlayerClip { Id = (string)entry.Value["id"], DisplayName = (string)entry.Value["displayName"] ?? FilePath.GetFileNameWithoutExtension(entry.Key),
						Path = entry.Key, Duration = (float?)entry.Value["durationSeconds"] ?? 1f });
				m_finisherEnemyClips.Sort((i_left, i_right) => string.Compare(i_left.Semantic, i_right.Semantic, StringComparison.OrdinalIgnoreCase));
				m_finisherPlayerClips.Sort((i_left, i_right) => string.Compare(i_left.DisplayName, i_right.DisplayName, StringComparison.OrdinalIgnoreCase));
				if (m_finisherEnemyClips.Count == 0) throw new InvalidDataException("The enemy has no resolvable animationRefs. Import or connect an enemy clip first.");
				if (m_finisherPlayerClips.Count == 0) throw new InvalidDataException("The mod has no playerAnimation document. Import or create the paired player clip first.");
				int draftEnemy = m_finisherEnemyClips.FindIndex(i_clip => i_clip.Semantic == "paired-draft"); if (draftEnemy >= 0) m_finisherEnemyClipIndex = m_finisherEnemyClipIndex2 = draftEnemy;
				int draftPlayer = m_finisherPlayerClips.FindIndex(i_clip => i_clip.Id.EndsWith("/paired-draft", StringComparison.Ordinal)); if (draftPlayer >= 0) m_finisherPlayerClipIndex = m_finisherPlayerClipIndex2 = draftPlayer;
				m_finisherDuration = m_finisherEnemyClips[ClampEnemyIndex(m_finisherEnemyClipIndex)].Duration;
				m_finisherDuration2 = m_finisherEnemyClips[ClampEnemyIndex(m_finisherEnemyClipIndex2)].Duration;
				int loadedPhases = LoadExistingFinisher(enemyEntry.Value);
				m_statusType = MessageType.Info; m_status = "Found " + m_finisherEnemyClips.Count + " connected enemy clips and " + m_finisherPlayerClips.Count + " player clips."
					+ (loadedPhases > 0 ? " Loaded " + loadedPhases + " existing finisher phase" + (loadedPhases == 1 ? "." : "s.") : string.Empty);
			}
			catch (Exception exception) { m_statusType = MessageType.Error; m_status = exception.Message; }
		}

		private int LoadExistingFinisher(JObject i_enemy)
		{
			JObject finisher = (i_enemy["behavior"]?["modules"] as JArray)?.OfType<JObject>()
				.FirstOrDefault(i_module => (string)i_module["type"] == "downedFinisher");
			if (finisher == null) return 0;
			m_finisherTriggerRange = (float?)finisher["triggerRange"] ?? m_finisherTriggerRange;
			m_finisherStartDelay = (float?)finisher["startDelaySeconds"] ?? m_finisherStartDelay;
			m_finisherMeterMax = (float?)finisher["meterMax"] ?? m_finisherMeterMax;
			m_finisherInputPower = (float?)finisher["inputPower"] ?? m_finisherInputPower;
			m_finisherDecay = (float?)finisher["decayPerSecond"] ?? m_finisherDecay;
			m_finisherCooldown = (float?)finisher["cooldownSeconds"] ?? m_finisherCooldown;
			JArray phases = finisher["phases"] as JArray;
			if (phases == null || phases.Count == 0)
			{
				m_finisherUsePhases = false;
				LoadFinisherPhase(finisher, false);
				return 0;
			}
			m_finisherUsePhases = true;
			LoadFinisherPhase(phases.OfType<JObject>().ElementAtOrDefault(0), false);
			LoadFinisherPhase(phases.OfType<JObject>().ElementAtOrDefault(1), true);
			return phases.Count;
		}

		private void LoadFinisherPhase(JObject i_phase, bool i_second)
		{
			if (i_phase == null) return;
			int enemy = m_finisherEnemyClips.FindIndex(i_clip => i_clip.Semantic == (string)i_phase["animation"]);
			int player = m_finisherPlayerClips.FindIndex(i_clip => i_clip.Id == (string)i_phase["playerAnimationRef"]);
			if (i_second)
			{
				m_finisherPhase2 = (string)i_phase["id"] ?? m_finisherPhase2;
				m_finisherDuration2 = (float?)i_phase["durationSeconds"] ?? m_finisherDuration2;
				if (enemy >= 0) m_finisherEnemyClipIndex2 = enemy;
				if (player >= 0) m_finisherPlayerClipIndex2 = player;
			}
			else
			{
				m_finisherPhase1 = (string)i_phase["id"] ?? m_finisherPhase1;
				m_finisherDuration = (float?)i_phase["durationSeconds"] ?? m_finisherDuration;
				if (enemy >= 0) m_finisherEnemyClipIndex = enemy;
				if (player >= 0) m_finisherPlayerClipIndex = player;
			}
		}

		private void ConfigureFinisher()
		{
			try
			{
				if (m_finisherEnemyClips.Count == 0 || m_finisherPlayerClips.Count == 0 || string.IsNullOrEmpty(m_finisherEnemyPath)) throw new InvalidOperationException("Discover the animation clips first.");
				string root = ProjectPath(m_modFolder), packId = (string)JObject.Parse(File.ReadAllText(FilePath.Combine(root, "manifest.json")))["id"];
				JObject enemy = JObject.Parse(File.ReadAllText(m_finisherEnemyPath)); JObject behavior = enemy["behavior"] as JObject ?? new JObject(); enemy["behavior"] = behavior;
				JArray modules = behavior["modules"] as JArray ?? new JArray(); behavior["modules"] = modules;
				List<JObject> existingFinishers = modules.OfType<JObject>().Where(i_module => (string)i_module["type"] == "downedFinisher").ToList();
				JArray existingPhases = existingFinishers.FirstOrDefault()?["phases"] as JArray;
				if (!m_finisherUsePhases && m_preserveExistingAdditionalPhases && (existingPhases?.Count ?? 0) > 0)
					throw new InvalidOperationException("This enemy already has a phased finisher. Keep phased editing enabled, or explicitly disable phase preservation before replacing the complete phase list.");
				foreach (JObject existing in existingFinishers) existing.Remove();
				JObject finisher = new JObject { ["type"] = "downedFinisher", ["triggerRange"] = m_finisherTriggerRange, ["startDelaySeconds"] = m_finisherStartDelay,
					["meterMax"] = m_finisherMeterMax, ["inputPower"] = m_finisherInputPower, ["decayPerSecond"] = m_finisherDecay, ["cooldownSeconds"] = m_finisherCooldown,
					["successOutcome"] = new JObject { ["healthRecovery"] = m_finisherSuccessRecovery, ["enemyStunSeconds"] = m_finisherSuccessStun },
					["failureOutcome"] = new JObject { ["healthDamage"] = m_finisherFailureHealth, ["strengthDamage"] = m_finisherFailureStrength,
						["pleasure"] = m_finisherFailurePleasure, ["libido"] = m_finisherFailureLibido, ["playerRagdollSeconds"] = m_finisherFailureRagdoll } };
				if (m_finisherUsePhases)
				{
					string phase1 = SemanticSlug(m_finisherPhase1), phase2 = SemanticSlug(m_finisherPhase2);
					if (string.IsNullOrEmpty(phase1) || string.IsNullOrEmpty(phase2) || phase1 == phase2) throw new InvalidDataException("Two-phase finishers need two different semantic phase IDs.");
					JArray phases = new JArray(BuildFinisherPhase(phase1, m_finisherEnemyClipIndex, m_finisherPlayerClipIndex, m_finisherDuration),
						BuildFinisherPhase(phase2, m_finisherEnemyClipIndex2, m_finisherPlayerClipIndex2, m_finisherDuration2));
					if (m_preserveExistingAdditionalPhases && existingPhases != null)
						foreach (JToken phase in existingPhases.Skip(2)) phases.Add(phase.DeepClone());
					finisher["phases"] = phases;
				}
				else
				{
					FinisherEnemyClip enemyClip = m_finisherEnemyClips[ClampEnemyIndex(m_finisherEnemyClipIndex)]; FinisherPlayerClip playerClip = m_finisherPlayerClips[ClampPlayerIndex(m_finisherPlayerClipIndex)];
					finisher["durationSeconds"] = m_finisherDuration; finisher["animation"] = enemyClip.Semantic; finisher["playerAnimationRef"] = playerClip.Id;
				}
				modules.Add(finisher); string candidate = enemy.ToString(Formatting.Indented);
				EnemyDefinitionLoadResult definitionValidation = EnemyDefinitionParser.Parse(candidate, packId, m_finisherEnemyPath);
				if (!definitionValidation.Report.IsValid) throw new InvalidDataException(string.Join("\n", definitionValidation.Report.Issues.Where(i_issue => i_issue.Severity == ValidationSeverity.Error).Take(8).Select(i_issue => i_issue.Message).ToArray()));

				string temporary = FilePath.Combine(ProjectRoot(), "Temp", "Modding", "Finisher", Guid.NewGuid().ToString("N"));
				try
				{
					CopyModDirectory(root, temporary); string stagedEnemy = FilePath.Combine(temporary, RelativeModPath(root, m_finisherEnemyPath)); Directory.CreateDirectory(FilePath.GetDirectoryName(stagedEnemy)); File.WriteAllText(stagedEnemy, candidate, new System.Text.UTF8Encoding(false));
					CapmodPackageResult staged = CapmodPackageBuilder.ValidateSource(temporary); if (!staged.Report.IsValid) throw new InvalidDataException("The staged mod did not validate:\n" + string.Join("\n", staged.Report.Issues.Where(i_issue => i_issue.Severity == ValidationSeverity.Error).Take(8).Select(i_issue => i_issue.Message).ToArray()));
				}
				finally { if (Directory.Exists(temporary)) try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { } }

				string backup = FilePath.Combine(ProjectRoot(), "ModAuthoringBackups", Slug(packId), Slug(m_slug), "finisher-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6), RelativeModPath(root, m_finisherEnemyPath));
				Directory.CreateDirectory(FilePath.GetDirectoryName(backup)); File.Copy(m_finisherEnemyPath, backup, false);
				try
				{
					File.WriteAllText(m_finisherEnemyPath, candidate, new System.Text.UTF8Encoding(false)); CapmodPackageResult live = CapmodPackageBuilder.ValidateSource(root);
					if (!live.Report.IsValid) throw new InvalidDataException("Live validation failed after configuration.");
				}
				catch { File.Copy(backup, m_finisherEnemyPath, true); throw; }
				AssetDatabase.Refresh(); m_statusType = MessageType.Info; m_status = "Configured the paired interaction and validated the mod.\nBackup: " + backup;
			}
			catch (Exception exception) { m_statusType = MessageType.Error; m_status = exception.Message; }
		}

		private JObject BuildFinisherPhase(string i_id, int i_enemyIndex, int i_playerIndex, float i_duration)
		{
			FinisherEnemyClip enemy = m_finisherEnemyClips[ClampEnemyIndex(i_enemyIndex)]; FinisherPlayerClip player = m_finisherPlayerClips[ClampPlayerIndex(i_playerIndex)];
			return new JObject { ["id"] = i_id, ["durationSeconds"] = i_duration, ["animation"] = enemy.Semantic, ["playerAnimationRef"] = player.Id,
				["inputPower"] = m_finisherInputPower, ["decayPerSecond"] = m_finisherDecay };
		}

		private void OpenFinisherPreview()
		{
			if (m_finisherEnemyClips.Count == 0 || m_finisherPlayerClips.Count == 0 || string.IsNullOrEmpty(m_finisherEnemyPath)) { m_statusType = MessageType.Warning; m_status = "Discover the animation clips first."; return; }
			FinisherEnemyClip enemy = m_finisherEnemyClips[ClampEnemyIndex(m_finisherEnemyClipIndex)]; FinisherPlayerClip player = m_finisherPlayerClips[ClampPlayerIndex(m_finisherPlayerClipIndex)];
			PlayerAnimationPreviewWindow.OpenModPair(ProjectPath(m_modFolder), m_finisherEnemyPath, enemy.Path, player.Path);
		}

		private static string SemanticSlug(string i_value) { string slug = Slug(i_value); return slug.Trim('-','_'); }
		private static string RelativeModPath(string i_root, string i_path)
		{
			string root = FilePath.GetFullPath(i_root).TrimEnd(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar) + FilePath.DirectorySeparatorChar, path = FilePath.GetFullPath(i_path);
			if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("A selected file is outside the mod folder: " + path); return path.Substring(root.Length);
		}
		private static void CopyModDirectory(string i_source, string i_destination)
		{
			Directory.CreateDirectory(i_destination); foreach (string file in Directory.GetFiles(i_source, "*", SearchOption.TopDirectoryOnly)) File.Copy(file, FilePath.Combine(i_destination, FilePath.GetFileName(file)), false);
			foreach (string directory in Directory.GetDirectories(i_source, "*", SearchOption.TopDirectoryOnly)) { if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0) continue; CopyModDirectory(directory, FilePath.Combine(i_destination, FilePath.GetFileName(directory))); }
		}

		private void DrawPresetSummary()
		{
			Preset preset = PresetFor(m_topology);
			EditorGUILayout.LabelField("Generated starter", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(preset.Bones.Count + " bones, " + preset.HitZones.Count + " hit zones, " + m_aiType + " AI", EditorStyles.miniLabel);
			EditorGUILayout.LabelField("Bones: " + string.Join(", ", preset.Bones.Select(i_bone => i_bone.Id).ToArray()), EditorStyles.wordWrappedMiniLabel);
		}

		private void DrawAiAuthoring()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("Movement AI", EditorStyles.boldLabel);
			IReadOnlyList<EnemyAiAuthoringPreset> presets = EnemyAiAuthoring.Presets;
			string[] names = presets.Select(i_preset => i_preset.DisplayName).ToArray();
			m_aiPresetIndex = Mathf.Clamp(m_aiPresetIndex, 0, presets.Count - 1);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_aiPresetIndex = EditorGUILayout.Popup("Behavior preset", m_aiPresetIndex, names);
				if (GUILayout.Button("Apply", GUILayout.Width(70f))) ApplyAiPreset(presets[m_aiPresetIndex]);
			}
			EditorGUILayout.HelpBox(presets[m_aiPresetIndex].Description, MessageType.None);

			string[] labels = { "Ground chase", "Flying chase", "Hold position" };
			string[] values = { "groundChase", "flyingChase", "holdPosition" };
			int typeIndex = Mathf.Max(0, Array.IndexOf(values, m_aiType));
			m_aiType = values[EditorGUILayout.Popup("Movement brain", typeIndex, labels)];
			m_aiPreferredRange = EditorGUILayout.FloatField(new GUIContent("Preferred range", "Distance at which pursuit stops."), m_aiPreferredRange);
			m_aiRetreatRange = EditorGUILayout.FloatField(new GUIContent("Retreat range", "Back away while the player is closer than this distance. Use zero to disable retreating."), m_aiRetreatRange);
			m_aiReactionSeconds = EditorGUILayout.FloatField(new GUIContent("Reaction time", "Seconds between AI movement decisions. Lower values react faster."), m_aiReactionSeconds);
			m_aiVisionRange = EditorGUILayout.FloatField(new GUIContent("Vision range", "Maximum distance at which this enemy notices the player."), m_aiVisionRange);
			m_aiSpeedAcceleration = EditorGUILayout.FloatField("Acceleration", m_aiSpeedAcceleration);
			m_aiSpeedMax = EditorGUILayout.FloatField("Maximum speed", m_aiSpeedMax);
			m_aiTraction = EditorGUILayout.FloatField(new GUIContent("Traction", "Ground movement smoothing and braking."), m_aiTraction);

			if (m_aiRetreatRange > m_aiPreferredRange)
				EditorGUILayout.HelpBox("Retreat range cannot exceed preferred range.", MessageType.Error);
			if (m_topology == Topology.Flying && m_aiType != "flyingChase")
				EditorGUILayout.HelpBox("This flying rig is using a non-flying movement brain and may fall or fail to pursue through the air.", MessageType.Warning);
			else if (m_topology != Topology.Flying && m_aiType == "flyingChase")
				EditorGUILayout.HelpBox("Flying chase ignores ground navigation. Use it intentionally for a hovering or airborne enemy.", MessageType.Warning);
			EditorGUILayout.HelpBox("Movement AI controls pursuit only. Attack reach, cooldowns, projectiles, and charge behavior are authored separately in the attack strategy.", MessageType.Info);
		}

		private void ApplySuggestedAiPreset()
		{
			string id = m_topology == Topology.Flying ? "flying-assault" : m_topology == Topology.Blank ? "stationary-guard" : "ground-assault";
			IReadOnlyList<EnemyAiAuthoringPreset> presets = EnemyAiAuthoring.Presets;
			m_aiPresetIndex = Mathf.Max(0, presets.ToList().FindIndex(i_preset => i_preset.Id == id));
			ApplyAiPreset(presets[m_aiPresetIndex]);
		}

		private void ApplyAiPreset(EnemyAiAuthoringPreset i_preset)
		{
			m_aiType = i_preset.Type;
			m_aiPreferredRange = i_preset.PreferredRange;
			m_aiRetreatRange = i_preset.RetreatRange;
			m_aiReactionSeconds = i_preset.ReactionSeconds;
			m_aiVisionRange = i_preset.VisionRange;
			m_aiSpeedAcceleration = i_preset.SpeedAcceleration;
			m_aiSpeedMax = i_preset.SpeedMax;
			m_aiTraction = i_preset.Traction;
		}

		private EnemyAiDefinition CurrentAiDefinition()
		{
			return new EnemyAiDefinition { Type = m_aiType, PreferredRange = m_aiPreferredRange, RetreatRange = m_aiRetreatRange, ReactionSeconds = m_aiReactionSeconds };
		}

		private void ValidateAiAuthoring()
		{
			ValidationReport report = new ValidationReport();
			EnemyAiAuthoring.Validate(CurrentAiDefinition(), report, "New Enemy Wizard");
			if (!IsFiniteInRange(m_aiVisionRange, .1f, 10000f)) report.Add(ValidationSeverity.Error, "enemy.behavior.vision-range", "Vision range must be between 0.1 and 10000.");
			if (!IsFiniteInRange(m_aiSpeedAcceleration, .01f, 1000f)) report.Add(ValidationSeverity.Error, "enemy.stats.speed-acceleration", "Acceleration must be between 0.01 and 1000.");
			if (!IsFiniteInRange(m_aiSpeedMax, .01f, 1000f)) report.Add(ValidationSeverity.Error, "enemy.stats.speed-max", "Maximum speed must be between 0.01 and 1000.");
			if (!IsFiniteInRange(m_aiTraction, 0f, 1f)) report.Add(ValidationSeverity.Error, "enemy.stats.traction", "Traction must be between 0 and 1.");
			if (!report.IsValid) throw new InvalidDataException("Fix the movement AI settings before creating the enemy:\n" + string.Join("\n", report.Issues.Select(i_issue => "- " + i_issue.Message).ToArray()));
		}

		private static bool IsFiniteInRange(float i_value, float i_min, float i_max)
		{
			return !float.IsNaN(i_value) && !float.IsInfinity(i_value) && i_value >= i_min && i_value <= i_max;
		}

		private void DrawAttackAuthoring()
		{
			EnsureAttackDrafts();
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("Attack strategy", EditorStyles.boldLabel);
			string[] presetNames = { "Close melee", "Hitscan marksman", "Physical projectile", "Area burst", "Charging melee", "Melee + projectile mix", "Two-hit combo", "Grab + paired finisher" };
			string[] presetDescriptions = {
				"One dependable close-range attack.", "A long-range line-of-sight attack.", "Fires an atlas-backed physical projectile.",
				"Damages the player inside a radius around an offset point.", "Keeps moving and receives a facing-relative impulse before impact.",
				"Chooses between close melee and a projectile using relative selection weights.", "One animation delivers two ordered melee hits.",
				"Creates a grab attack and connects the generated paired draft to a playable struggle interaction." };
			m_attackPresetIndex = Mathf.Clamp(m_attackPresetIndex, 0, presetNames.Length - 1);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_attackPresetIndex = EditorGUILayout.Popup("Strategy preset", m_attackPresetIndex, presetNames);
				if (GUILayout.Button("Apply", GUILayout.Width(70f))) ApplyAttackPreset(m_attackPresetIndex);
			}
			EditorGUILayout.HelpBox(presetDescriptions[m_attackPresetIndex], MessageType.None);

			for (int index = 0; index < m_attackDrafts.Count; index++)
			{
				AttackDraft draft = m_attackDrafts[index];
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					draft.Expanded = EditorGUILayout.Foldout(draft.Expanded, (index + 1) + ". " + (string.IsNullOrWhiteSpace(draft.Id) ? "Unnamed attack" : draft.Id), true);
					if (draft.Expanded) DrawAttackDraft(draft);
					using (new EditorGUILayout.HorizontalScope())
					{
						if (GUILayout.Button("Duplicate")) m_attackDrafts.Insert(index + 1, draft.Clone("attack-" + (m_attackDrafts.Count + 1)));
						using (new EditorGUI.DisabledScope(m_attackDrafts.Count == 1))
							if (GUILayout.Button("Remove")) { m_attackDrafts.RemoveAt(index); index--; }
					}
				}
			}
			if (GUILayout.Button("+ Add melee attack")) m_attackDrafts.Add(AttackDraft.Melee("attack-" + (m_attackDrafts.Count + 1)));
			EditorGUILayout.HelpBox("Selection weight is relative: weights 3 and 1 choose the first attack about 75% of the time. Attack IDs also become generated animation names.", MessageType.Info);
			float maximumRange = m_attackDrafts.Max(i_attack => i_attack.InitiateRange);
			if (m_aiType != "holdPosition" && m_aiPreferredRange > maximumRange)
				EditorGUILayout.HelpBox("Preferred movement range is beyond every attack's initiate range. The enemy could stop without attacking.", MessageType.Error);
		}

		private void DrawAttackDraft(AttackDraft io_draft)
		{
			io_draft.Id = EditorGUILayout.TextField("Attack ID", io_draft.Id);
			string[] labels = { "Melee", "Hitscan", "Projectile", "Area", "Grab", "Multi-stage" };
			string[] values = { "melee", "hitscan", "projectile", "area", "grab", "multiStage" };
			int current = Mathf.Max(0, Array.IndexOf(values, io_draft.Type));
			io_draft.Type = values[EditorGUILayout.Popup("Delivery", current, labels)];
			io_draft.Weight = EditorGUILayout.FloatField("Selection weight", io_draft.Weight);
			io_draft.Damage = EditorGUILayout.FloatField("Damage", io_draft.Damage);
			io_draft.KnockbackX = EditorGUILayout.FloatField("Horizontal knockback", io_draft.KnockbackX);
			io_draft.KnockbackY = EditorGUILayout.FloatField("Vertical knockback", io_draft.KnockbackY);
			io_draft.Cooldown = EditorGUILayout.FloatField("Cooldown", io_draft.Cooldown);
			io_draft.InitiateRange = EditorGUILayout.FloatField(new GUIContent("Initiate range", "Distance at which the enemy may start this attack."), io_draft.InitiateRange);
			io_draft.HitRange = EditorGUILayout.FloatField(new GUIContent("Hit range", "Maximum distance checked when the hit event occurs."), io_draft.HitRange);
			io_draft.Duration = EditorGUILayout.FloatField("Duration", io_draft.Duration);
			io_draft.HitTime = EditorGUILayout.FloatField(new GUIContent("Hit time", "Generated attackHit marker time. Multi-stage attacks distribute markers over the duration."), io_draft.HitTime);
			io_draft.MovesDuringAttack = EditorGUILayout.Toggle("Moves during attack", io_draft.MovesDuringAttack);
			io_draft.AddImpulse = EditorGUILayout.Toggle(new GUIContent("Charge impulse", "Adds a facing-relative impulse event before the hit."), io_draft.AddImpulse);
			if (io_draft.AddImpulse)
			{
				io_draft.ImpulseTime = EditorGUILayout.FloatField("Impulse time", io_draft.ImpulseTime);
				io_draft.ImpulseX = EditorGUILayout.FloatField("Forward impulse", io_draft.ImpulseX);
				io_draft.ImpulseY = EditorGUILayout.FloatField("Vertical impulse", io_draft.ImpulseY);
			}
			if (io_draft.Type == "area")
			{
				io_draft.AreaRadius = EditorGUILayout.FloatField("Area radius", io_draft.AreaRadius);
				io_draft.OffsetX = EditorGUILayout.FloatField("Area offset X", io_draft.OffsetX);
				io_draft.OffsetY = EditorGUILayout.FloatField("Area offset Y", io_draft.OffsetY);
			}
			if (io_draft.Type == "projectile" || io_draft.Type == "multiStage" && io_draft.Stages.Any(i_stage => i_stage.Type == "projectile"))
			{
				io_draft.ProjectileSpeed = EditorGUILayout.FloatField("Projectile speed", io_draft.ProjectileSpeed);
				io_draft.ProjectileLifetime = EditorGUILayout.FloatField("Projectile lifetime", io_draft.ProjectileLifetime);
				io_draft.ProjectileRadius = EditorGUILayout.FloatField("Projectile radius", io_draft.ProjectileRadius);
				io_draft.ProjectileGravity = EditorGUILayout.FloatField("Projectile gravity", io_draft.ProjectileGravity);
				EditorGUILayout.HelpBox("A projectile atlas cell named 'projectile' will be generated automatically.", MessageType.None);
			}
			if (io_draft.Type == "multiStage") DrawAttackStages(io_draft);
		}

		private void DrawAttackStages(AttackDraft io_draft)
		{
			if (io_draft.Stages == null) io_draft.Stages = new List<AttackStageDraft>();
			EditorGUILayout.Space(4f);
			EditorGUILayout.LabelField("Ordered hit stages", EditorStyles.boldLabel);
			string[] labels = { "Melee", "Hitscan", "Projectile", "Area" };
			string[] values = { "melee", "hitscan", "projectile", "area" };
			for (int index = 0; index < io_draft.Stages.Count; index++)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					int selected = Mathf.Max(0, Array.IndexOf(values, io_draft.Stages[index].Type));
					io_draft.Stages[index].Type = values[EditorGUILayout.Popup("Hit " + (index + 1), selected, labels)];
					using (new EditorGUI.DisabledScope(io_draft.Stages.Count <= 2))
						if (GUILayout.Button("-", GUILayout.Width(25f))) { io_draft.Stages.RemoveAt(index); index--; }
				}
			}
			if (io_draft.Stages.Count < 16 && GUILayout.Button("+ Add hit stage")) io_draft.Stages.Add(new AttackStageDraft { Type = "melee" });
		}

		private void DrawStatsAndBehaviors()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("Balance and spawning", EditorStyles.boldLabel);
			m_healthMax = EditorGUILayout.FloatField("Maximum health", m_healthMax);
			m_bounty = EditorGUILayout.IntField("Bounty", m_bounty);
			m_healthIncreasePerWave = EditorGUILayout.FloatField("Health per wave", m_healthIncreasePerWave);
			m_spawnSelectionWeight = EditorGUILayout.FloatField(new GUIContent("Spawn weight", "Relative weight when a stage spawner contains this enemy."), m_spawnSelectionWeight);

			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Behavior modules", EditorStyles.boldLabel);
			string[] labels = { "Regeneration", "Berserk", "Lifesteal", "Thorns", "Ragdoll on hit", "Spawn on death", "Speed pulse" };
			string[] values = { "regeneration", "berserk", "lifesteal", "thorns", "onHitRagdoll", "spawnOnDeath", "speedPulse" };
			if (m_behaviorDrafts == null) m_behaviorDrafts = new List<BehaviorDraft>();
			m_behaviorDrafts.RemoveAll(i_module => i_module == null);
			for (int index = 0; index < m_behaviorDrafts.Count; index++)
			{
				BehaviorDraft module = m_behaviorDrafts[index];
				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					EditorGUILayout.LabelField(labels[Mathf.Max(0, Array.IndexOf(values, module.Type))], EditorStyles.boldLabel);
					DrawBehaviorFields(module);
					if (GUILayout.Button("Remove module")) { m_behaviorDrafts.RemoveAt(index); index--; }
				}
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				m_newBehaviorType = EditorGUILayout.Popup("Add module", Mathf.Clamp(m_newBehaviorType, 0, values.Length - 1), labels);
				using (new EditorGUI.DisabledScope(m_behaviorDrafts.Any(i_module => i_module.Type == values[m_newBehaviorType]) || m_behaviorDrafts.Count >= 7))
					if (GUILayout.Button("Add", GUILayout.Width(70f))) m_behaviorDrafts.Add(BehaviorDraft.Create(values[m_newBehaviorType]));
			}

			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Drops", EditorStyles.boldLabel);
			m_dropChance = EditorGUILayout.Slider("Drop chance", m_dropChance, 0f, 1f);
			m_dropItemIds = EditorGUILayout.TextField(new GUIContent("Item IDs", "Comma-separated full item IDs, such as core:item/consumable/ammo-box."), m_dropItemIds);
		}

		private static void DrawBehaviorFields(BehaviorDraft io_module)
		{
			switch (io_module.Type)
			{
				case "regeneration":
					io_module.Amount = EditorGUILayout.FloatField("Health restored", io_module.Amount);
					io_module.Interval = EditorGUILayout.FloatField("Interval", io_module.Interval);
					break;
				case "berserk":
					io_module.Threshold = EditorGUILayout.Slider("Health threshold", io_module.Threshold, .01f, 1f);
					io_module.SpeedBonus = EditorGUILayout.FloatField("Speed bonus", io_module.SpeedBonus);
					io_module.DamageBonus = EditorGUILayout.FloatField("Damage multiplier bonus", io_module.DamageBonus);
					break;
				case "lifesteal": case "thorns":
					io_module.Amount = EditorGUILayout.FloatField(io_module.Type == "lifesteal" ? "Health per hit" : "Reflected damage", io_module.Amount);
					break;
				case "onHitRagdoll":
					io_module.Duration = EditorGUILayout.FloatField("Ragdoll duration", io_module.Duration);
					break;
				case "spawnOnDeath":
					io_module.Enemy = EditorGUILayout.TextField("Enemy content ID", io_module.Enemy);
					io_module.Count = EditorGUILayout.IntSlider("Count", io_module.Count, 1, 32);
					io_module.Radius = EditorGUILayout.FloatField("Spawn radius", io_module.Radius);
					break;
				case "speedPulse":
					io_module.SpeedBonus = EditorGUILayout.FloatField("Speed bonus", io_module.SpeedBonus);
					io_module.Interval = EditorGUILayout.FloatField("Pulse interval", io_module.Interval);
					io_module.Duration = EditorGUILayout.FloatField("Pulse duration", io_module.Duration);
					break;
			}
		}

		private void EnsureAttackDrafts()
		{
			if (m_attackDrafts == null) m_attackDrafts = new List<AttackDraft>();
			m_attackDrafts.RemoveAll(i_attack => i_attack == null);
			if (m_attackDrafts.Count == 0) m_attackDrafts.Add(AttackDraft.Melee("primary"));
			foreach (AttackDraft draft in m_attackDrafts)
				if (draft.Stages == null) draft.Stages = new List<AttackStageDraft>();
		}

		private void ApplyAttackPreset(int i_index)
		{
			switch (i_index)
			{
				case 1: m_attackDrafts = new List<AttackDraft> { AttackDraft.Hitscan("shot") }; m_aiPreferredRange = 7f; m_aiRetreatRange = 3f; break;
				case 2: m_attackDrafts = new List<AttackDraft> { AttackDraft.Projectile("shot") }; m_aiPreferredRange = 5f; m_aiRetreatRange = 2f; break;
				case 3: m_attackDrafts = new List<AttackDraft> { AttackDraft.Area("burst") }; m_aiPreferredRange = 1.5f; m_aiRetreatRange = 0f; break;
				case 4: m_attackDrafts = new List<AttackDraft> { AttackDraft.Charge("charge") }; m_aiPreferredRange = 3.5f; m_aiRetreatRange = 0f; break;
				case 5:
					AttackDraft melee = AttackDraft.Melee("claw"); melee.Weight = 3f;
					AttackDraft projectile = AttackDraft.Projectile("shot"); projectile.Weight = 1f;
					m_attackDrafts = new List<AttackDraft> { melee, projectile }; m_aiPreferredRange = 1f; m_aiRetreatRange = 0f; break;
				case 6: m_attackDrafts = new List<AttackDraft> { AttackDraft.Combo("combo") }; m_aiPreferredRange = .8f; m_aiRetreatRange = 0f; break;
				case 7:
					m_attackDrafts = new List<AttackDraft> { AttackDraft.Grab("grab") }; m_aiPreferredRange = .8f; m_aiRetreatRange = 0f;
					m_createDragonBonesProject = true; m_includePairedPlayer = true; m_generateStarterFinisher = true; break;
				default: m_attackDrafts = new List<AttackDraft> { AttackDraft.Melee("primary") }; m_aiPreferredRange = .8f; m_aiRetreatRange = 0f; break;
			}
		}

		private void ValidateAttackAuthoring()
		{
			EnsureAttackDrafts();
			ValidationReport report = new ValidationReport();
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (AttackDraft draft in m_attackDrafts)
			{
				if (string.IsNullOrWhiteSpace(draft.Id) || SemanticSlug(draft.Id) != draft.Id || !ids.Add(draft.Id))
					report.Add(ValidationSeverity.Error, "enemy.attacks.id", "Attack IDs must be unique lowercase semantic names.");
				if (!EnemyAttackAuthoring.DeliveryTypes.Contains(draft.Type))
					report.Add(ValidationSeverity.Error, "enemy.attacks.type", "The starter wizard supports melee, hitscan, projectile, area, grab, and multi-stage attacks.");
				if (draft.Type == "grab" && !m_generateStarterFinisher)
					report.Add(ValidationSeverity.Error, "enemy.attacks.grab-finisher", "Grab attacks require the playable starter grab/finisher option.");
				ValidateDraftRange(draft.Weight, 0f, 1000f, "Selection weight", report);
				ValidateDraftRange(draft.Damage, 0f, 100000f, "Damage", report);
				ValidateDraftRange(draft.KnockbackX, 0f, 1000f, "Horizontal knockback", report);
				ValidateDraftRange(draft.KnockbackY, 0f, 1000f, "Vertical knockback", report);
				ValidateDraftRange(draft.Cooldown, 0f, 600f, "Cooldown", report);
				ValidateDraftRange(draft.InitiateRange, 0f, 1000f, "Initiate range", report);
				ValidateDraftRange(draft.HitRange, 0f, 1000f, "Hit range", report);
				ValidateDraftRange(draft.Duration, .01f, 600f, "Duration", report);
				if (draft.Type != "multiStage") ValidateDraftRange(draft.HitTime, 0f, draft.Duration, "Hit time", report);
				if (draft.AddImpulse) ValidateDraftRange(draft.ImpulseTime, 0f, draft.Duration, "Impulse time", report);
				if (draft.Type == "area") ValidateDraftRange(draft.AreaRadius, .05f, 100f, "Area radius", report);
				bool projectile = draft.Type == "projectile" || draft.Type == "multiStage" && draft.Stages.Any(i_stage => i_stage.Type == "projectile");
				if (projectile)
				{
					ValidateDraftRange(draft.ProjectileSpeed, .05f, 1000f, "Projectile speed", report);
					ValidateDraftRange(draft.ProjectileLifetime, .05f, 60f, "Projectile lifetime", report);
					ValidateDraftRange(draft.ProjectileRadius, .01f, 10f, "Projectile radius", report);
					ValidateDraftRange(draft.ProjectileGravity, -10f, 10f, "Projectile gravity", report);
				}
				if (draft.Type == "multiStage" && (draft.Stages == null || draft.Stages.Count < 2 || draft.Stages.Count > 16))
					report.Add(ValidationSeverity.Error, "enemy.attacks.stages", "Multi-stage attacks require 2 through 16 hit stages.");
			}
			EnemyAttackAuthoring.ValidateStrategy(m_attackDrafts.Select(ToAttackDefinition).ToList(), CurrentAiDefinition(), report, "New Enemy Wizard");
			if (!report.IsValid) throw new InvalidDataException("Fix the attack strategy before creating the enemy:\n" + string.Join("\n", report.Issues.Select(i_issue => "- " + i_issue.Message).Distinct().ToArray()));
		}

		private static void ValidateDraftRange(float i_value, float i_min, float i_max, string i_name, ValidationReport io_report)
		{
			if (!IsFiniteInRange(i_value, i_min, i_max)) io_report.Add(ValidationSeverity.Error, "enemy.attacks.authoring-range", i_name + " must be between " + i_min + " and " + i_max + ".");
		}

		private static EnemyAttackDefinition ToAttackDefinition(AttackDraft i_draft)
		{
			return new EnemyAttackDefinition { Id = i_draft.Id, Type = i_draft.Type, Chance = i_draft.Weight, Damage = i_draft.Damage,
				CooldownSeconds = i_draft.Cooldown, InitiateRange = i_draft.InitiateRange, HitRange = i_draft.HitRange,
				DurationSeconds = i_draft.Duration };
		}

		private void ValidateStatsAndBehaviors()
		{
			ValidationReport report = new ValidationReport();
			ValidateDraftRange(m_healthMax, .01f, 1000000f, "Maximum health", report);
			if (m_bounty < 0) report.Add(ValidationSeverity.Error, "enemy.stats.bounty", "Bounty cannot be negative.");
			ValidateDraftRange(m_healthIncreasePerWave, 0f, 1000000f, "Health per wave", report);
			ValidateDraftRange(m_spawnSelectionWeight, .001f, 1000f, "Spawn weight", report);
			ValidateDraftRange(m_dropChance, 0f, 1f, "Drop chance", report);
			if (m_behaviorDrafts == null) m_behaviorDrafts = new List<BehaviorDraft>();
			HashSet<string> moduleTypes = new HashSet<string>(StringComparer.Ordinal);
			foreach (BehaviorDraft module in m_behaviorDrafts)
			{
				if (module == null || !moduleTypes.Add(module.Type ?? string.Empty)) { report.Add(ValidationSeverity.Error, "enemy.behavior.module", "Behavior modules must have unique types."); continue; }
				switch (module.Type)
				{
					case "regeneration": ValidateDraftRange(module.Amount, .01f, 100000f, "Regeneration amount", report); ValidateDraftRange(module.Interval, .05f, 600f, "Regeneration interval", report); break;
					case "berserk": ValidateDraftRange(module.Threshold, .01f, 1f, "Berserk threshold", report); ValidateDraftRange(module.SpeedBonus, -1000f, 1000f, "Berserk speed bonus", report); ValidateDraftRange(module.DamageBonus, -100f, 100f, "Berserk damage bonus", report); break;
					case "lifesteal": case "thorns": ValidateDraftRange(module.Amount, .01f, 100000f, module.Type == "lifesteal" ? "Lifesteal amount" : "Thorns amount", report); break;
					case "onHitRagdoll": ValidateDraftRange(module.Duration, .05f, 30f, "Ragdoll duration", report); break;
					case "spawnOnDeath":
						if (!ContentId.TryParse(module.Enemy, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal)) report.Add(ValidationSeverity.Error, "enemy.behavior.spawn-on-death-enemy", "Spawn-on-death requires a full enemy content ID.");
						if (module.Count < 1 || module.Count > 32) report.Add(ValidationSeverity.Error, "enemy.behavior.spawn-on-death-count", "Spawn-on-death count must be 1 through 32.");
						ValidateDraftRange(module.Radius, 0f, 20f, "Spawn radius", report); break;
					case "speedPulse":
						ValidateDraftRange(module.SpeedBonus, -1000f, 1000f, "Pulse speed bonus", report); ValidateDraftRange(module.Interval, .1f, 600f, "Pulse interval", report); ValidateDraftRange(module.Duration, .05f, 600f, "Pulse duration", report);
						if (module.Duration >= module.Interval) report.Add(ValidationSeverity.Error, "enemy.behavior.speed-pulse-overlap", "Pulse duration must be shorter than its interval."); break;
					default: report.Add(ValidationSeverity.Error, "enemy.behavior.module-type", "Unsupported behavior module: " + module.Type); break;
				}
			}
			List<string> dropIds = ParseDropItemIds();
			if (dropIds.Count != dropIds.Distinct(StringComparer.Ordinal).Count()) report.Add(ValidationSeverity.Error, "enemy.drops.duplicate", "Drop item IDs must be unique.");
			foreach (string drop in dropIds)
				if (!ContentId.TryParse(drop, out ContentId id) || !id.Path.StartsWith("item/", StringComparison.Ordinal)) report.Add(ValidationSeverity.Error, "enemy.drops.item", "Invalid drop item ID: " + drop);
			if (m_generateStarterFinisher && (!m_createDragonBonesProject || !m_includePairedPlayer))
				report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-draft", "The starter finisher requires the DragonBones project and paired-player draft options.");
			if (m_generateStarterFinisher)
			{
				ValidateDraftRange(m_finisherTriggerRange, .1f, 20f, "Finisher trigger range", report);
				ValidateDraftRange(m_finisherStartDelay, 0f, 10f, "Finisher start delay", report);
				ValidateDraftRange(m_finisherDuration, 1f, 60f, "Finisher duration", report);
				ValidateDraftRange(m_finisherMeterMax, 1f, 10000f, "Finisher meter", report);
				ValidateDraftRange(m_finisherInputPower, .01f, 10000f, "Finisher input power", report);
				ValidateDraftRange(m_finisherDecay, 0f, 10000f, "Finisher decay", report);
				ValidateDraftRange(m_finisherCooldown, 0f, 600f, "Finisher cooldown", report);
				ValidateDraftRange(m_finisherSuccessRecovery, 0f, 100000f, "Finisher success recovery", report);
				ValidateDraftRange(m_finisherSuccessStun, 0f, 30f, "Finisher success stun", report);
				ValidateDraftRange(m_finisherFailureHealth, 0f, 100000f, "Finisher failure health damage", report);
				ValidateDraftRange(m_finisherFailureStrength, 0f, 100000f, "Finisher failure strength damage", report);
				ValidateDraftRange(m_finisherFailurePleasure, 0f, 100f, "Finisher failure pleasure", report);
				ValidateDraftRange(m_finisherFailureLibido, 0f, 10000f, "Finisher failure libido", report);
				ValidateDraftRange(m_finisherFailureRagdoll, 0f, 30f, "Finisher failure ragdoll", report);
			}
			if (!report.IsValid) throw new InvalidDataException("Fix the balance, behavior, or drop settings before creating the enemy:\n" + string.Join("\n", report.Issues.Select(i_issue => "- " + i_issue.Message).Distinct().ToArray()));
		}

		private List<string> ParseDropItemIds()
		{
			return (m_dropItemIds ?? string.Empty).Split(new[] { ',', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(i_value => i_value.Trim()).Where(i_value => i_value.Length > 0).ToList();
		}

		private void CreateEnemy()
		{
			try
			{
				ValidateAiAuthoring();
				ValidateAttackAuthoring();
				ValidateStatsAndBehaviors();
				string root = ProjectPath(m_modFolder);
				string manifestPath = FilePath.Combine(root, "manifest.json");
				if (!File.Exists(manifestPath)) throw new InvalidDataException("Choose an existing mod folder containing manifest.json. Use Create Mod first if necessary.");
				JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));
				string packId = (string)manifest["id"];
				if (!ContentId.IsValidNamespace(packId) || packId == "core") throw new InvalidDataException("The selected manifest does not contain a valid external pack ID.");
				string slug = Slug(m_slug);
				if (string.IsNullOrEmpty(slug) || slug != m_slug.Trim().ToLowerInvariant()) throw new InvalidDataException("Enemy ID must use lowercase letters, numbers, dashes, or underscores without spaces.");
				if (string.IsNullOrWhiteSpace(m_displayName)) throw new InvalidDataException("Display name is required.");

				string content = FilePath.Combine(root, "content");
				string animationDirectory = FilePath.Combine(content, "animations", slug);
				string assetDirectory = FilePath.Combine(root, "assets", "enemies", slug);
				string enemyPath = FilePath.Combine(content, slug + ".enemy.json");
				string atlasPath = FilePath.Combine(assetDirectory, slug + "-atlas.png");
				string guidePath = FilePath.Combine(assetDirectory, "atlas-regions.json");
				string readmePath = FilePath.Combine(assetDirectory, "README.txt");
				string stagePath = FilePath.Combine(content, slug + "-test-stage.json");
				string pairedEnemyPath = FilePath.Combine(animationDirectory, "paired-draft.enemy-animation.json");
				string pairedPlayerDirectory = FilePath.Combine(animationDirectory, "player");
				string pairedPlayerPath = FilePath.Combine(pairedPlayerDirectory, "paired-draft.player-animation.json");
				string dragonBonesDirectory = FilePath.Combine(ProjectRoot(), "DragonBonesProjects", packId, slug);
				List<string> targets = new List<string> { enemyPath, guidePath,
					readmePath, FilePath.Combine(animationDirectory, "idle.enemy-animation.json"), FilePath.Combine(animationDirectory, "move.enemy-animation.json") };
				foreach (AttackDraft attack in m_attackDrafts) targets.Add(FilePath.Combine(animationDirectory, "attack-" + attack.Id + ".enemy-animation.json"));
				targets.Add(atlasPath);
				if (m_createTestStage) targets.Add(stagePath);
				if (m_createDragonBonesProject && m_includePairedPlayer) { targets.Add(pairedEnemyPath); targets.Add(pairedPlayerPath); }
				string existing = targets.FirstOrDefault(File.Exists);
				if (existing != null) throw new IOException("Nothing was replaced because this generated file already exists: " + existing);
				if (m_createDragonBonesProject && Directory.Exists(dragonBonesDirectory) && Directory.EnumerateFileSystemEntries(dragonBonesDirectory).Any())
					throw new IOException("Nothing was replaced because the DragonBones project directory is not empty: " + dragonBonesDirectory);

				Directory.CreateDirectory(content); Directory.CreateDirectory(animationDirectory); Directory.CreateDirectory(assetDirectory);
				Preset preset = PresetFor(m_topology);
				bool needsProjectile = m_attackDrafts.Any(UsesProjectile);
				Dictionary<string, JObject> regions = BuildRegions(preset.Bones, needsProjectile);
				string enemyId = packId + ":enemy/" + slug;
				WriteJson(enemyPath, BuildEnemy(packId, slug, enemyId, preset, regions, m_createDragonBonesProject && m_includePairedPlayer));
				WriteJson(FilePath.Combine(animationDirectory, "idle.enemy-animation.json"), BuildAnimation(packId, slug, enemyId, "idle", "Idle", 0.8f, true, false));
				WriteJson(FilePath.Combine(animationDirectory, "move.enemy-animation.json"), BuildAnimation(packId, slug, enemyId, "move", "Move", 0.6f, true, false));
				foreach (AttackDraft attack in m_attackDrafts)
					WriteJson(FilePath.Combine(animationDirectory, "attack-" + attack.Id + ".enemy-animation.json"), BuildAttackAnimation(packId, slug, enemyId, attack));
				if (m_createDragonBonesProject && m_includePairedPlayer)
				{
					Directory.CreateDirectory(pairedPlayerDirectory);
					WriteJson(pairedEnemyPath, BuildAnimation(packId, slug, enemyId, "paired-draft", "Paired Interaction Draft", 1f, true, false));
					WriteJson(pairedPlayerPath, BuildPlayerPairedDraft(packId, slug));
				}
				WriteJson(guidePath, BuildAtlasGuide(preset.Bones, regions));
				WriteAtlas(atlasPath, regions.Count);
				if (m_createTestStage) WriteJson(stagePath, BuildTestStage(packId, slug, enemyId));
				WriteReadme(readmePath, slug, preset, m_createDragonBonesProject && m_includePairedPlayer);

				AssetDatabase.Refresh();
				if (m_createDragonBonesProject)
					DragonBonesAnimationBridge.ExportOriginalEnemy(enemyPath, animationDirectory, dragonBonesDirectory, m_dragonBonesImageScale, m_includePairedPlayer);
				CapmodPackageResult validation = CapmodPackageBuilder.ValidateSource(root);
				m_statusType = validation.Report.IsValid ? MessageType.Info : MessageType.Warning;
				m_status = validation.Report.IsValid
					? "Created and validated " + enemyId + "." + (m_createDragonBonesProject ? " DragonBones project: " + dragonBonesDirectory : string.Empty)
					: "Created " + enemyId + ", but validation reported: " + string.Join("\n", validation.Report.Issues.Take(6).Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
				EditorUtility.RevealInFinder(enemyPath);
			}
			catch (Exception exception) { m_statusType = MessageType.Error; m_status = exception.Message; }
		}

		private JObject BuildEnemy(string i_packId, string i_slug, string i_enemyId, Preset i_preset, Dictionary<string, JObject> i_regions, bool i_includePairedDraft)
		{
			JArray bones = new JArray();
			foreach (BonePreset bone in i_preset.Bones)
			{
				JObject item = new JObject { ["id"] = bone.Id, ["region"] = bone.Id, ["x"] = bone.X, ["y"] = bone.Y,
					["rotation"] = 0f, ["pivotX"] = bone.PivotX, ["pivotY"] = bone.PivotY, ["sortingOrder"] = bone.SortingOrder };
				if (!string.IsNullOrEmpty(bone.Parent)) item["parent"] = bone.Parent;
				bones.Add(item);
			}
			JArray hitZones = new JArray(i_preset.HitZones.Select(i_zone => new JObject { ["bone"] = i_zone.Bone, ["shape"] = i_zone.Circle ? "circle" : "box",
				["width"] = i_zone.Circle ? null : (JToken)i_zone.Width, ["height"] = i_zone.Circle ? null : (JToken)i_zone.Height,
				["radius"] = i_zone.Circle ? (JToken)i_zone.Radius : null, ["damageMultiplier"] = i_zone.Damage }).Select(RemoveNulls));
			JObject animationReferences = new JObject { ["idle"] = i_packId + ":enemy-animation/" + i_slug + "/idle", ["move"] = i_packId + ":enemy-animation/" + i_slug + "/move" };
			foreach (AttackDraft attack in m_attackDrafts) animationReferences["attack-" + attack.Id] = i_packId + ":enemy-animation/" + i_slug + "/attack-" + attack.Id;
			if (i_includePairedDraft) animationReferences["paired-draft"] = i_packId + ":enemy-animation/" + i_slug + "/paired-draft";
			JArray attacks = new JArray(m_attackDrafts.Select(BuildAttackJson));
			return new JObject
			{
				["schemaVersion"] = 1, ["type"] = "enemy", ["id"] = i_enemyId, ["displayName"] = m_displayName.Trim(), ["description"] = m_description.Trim(),
				["stats"] = new JObject { ["healthMax"] = m_healthMax, ["speedAcceleration"] = m_aiSpeedAcceleration, ["speedMax"] = m_aiSpeedMax,
					["traction"] = m_aiTraction, ["bounty"] = m_bounty, ["healthIncreasePerWave"] = m_healthIncreasePerWave },
				["spawn"] = new JObject { ["inheritTemplateSpawners"] = false, ["selectionWeight"] = m_spawnSelectionWeight },
				["ai"] = new JObject { ["type"] = m_aiType, ["preferredRange"] = m_aiPreferredRange, ["retreatRange"] = m_aiRetreatRange, ["reactionSeconds"] = m_aiReactionSeconds },
				["attacks"] = attacks,
				["behavior"] = new JObject { ["visionRange"] = m_aiVisionRange, ["ignoreWave"] = false, ["modules"] = BuildBehaviorModules(i_packId, i_slug) },
				["animationRefs"] = animationReferences,
				["drops"] = new JObject { ["chance"] = m_dropChance, ["items"] = new JArray(ParseDropItemIds()) },
				["visual"] = new JObject { ["type"] = "originalSkeletonAtlas", ["atlas"] = "assets/enemies/" + i_slug + "/" + i_slug + "-atlas.png", ["pixelsPerUnit"] = 32,
					["bodyWidth"] = i_preset.BodyWidth, ["bodyHeight"] = i_preset.BodyHeight, ["bodyOffsetX"] = 0, ["bodyOffsetY"] = i_preset.BodyOffsetY,
					["regions"] = JObject.FromObject(i_regions), ["bones"] = bones, ["hitZones"] = hitZones }
			};
		}

		private static JObject BuildAnimation(string i_packId, string i_slug, string i_enemyId, string i_name, string i_displayName, float i_duration, bool i_loop, bool i_attack)
		{
			float middle = i_duration * 0.5f; string property = i_attack ? "rotation.z" : "position.y";
			float middleValue = i_attack ? 24f : i_name == "move" ? 0.08f : 0.04f;
			JArray keys = new JArray(Key(0f, 0f), Key(middle, middleValue), Key(i_duration, 0f));
			JObject result = new JObject { ["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = i_packId + ":enemy-animation/" + i_slug + "/" + i_name,
				["enemy"] = i_enemyId, ["displayName"] = i_displayName, ["durationSeconds"] = i_duration, ["frameRate"] = 60, ["loop"] = i_loop,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = property, ["keys"] = keys }) };
			if (i_attack) result["events"] = new JArray(new JObject { ["time"] = middle, ["type"] = "attackHit" });
			return result;
		}

		private static JObject BuildAttackJson(AttackDraft i_attack)
		{
			JObject result = new JObject { ["id"] = i_attack.Id, ["type"] = i_attack.Type, ["animation"] = "attack-" + i_attack.Id,
				["chance"] = i_attack.Weight, ["damage"] = i_attack.Damage, ["knockbackX"] = i_attack.KnockbackX, ["knockbackY"] = i_attack.KnockbackY,
				["cooldownSeconds"] = i_attack.Cooldown, ["initiateRange"] = i_attack.InitiateRange, ["hitRange"] = i_attack.HitRange,
				["movesDuringAttack"] = i_attack.MovesDuringAttack, ["durationSeconds"] = i_attack.Duration };
			bool usesArea = i_attack.Type == "area" || i_attack.Type == "multiStage" && i_attack.Stages.Any(i_stage => i_stage.Type == "area");
			if (usesArea) { result["areaRadius"] = i_attack.AreaRadius; result["offsetX"] = i_attack.OffsetX; result["offsetY"] = i_attack.OffsetY; }
			if (UsesProjectile(i_attack))
			{
				result["projectileRegion"] = "projectile"; result["projectileSpeed"] = i_attack.ProjectileSpeed;
				result["projectileLifetimeSeconds"] = i_attack.ProjectileLifetime; result["projectileRadius"] = i_attack.ProjectileRadius;
				result["projectileGravityScale"] = i_attack.ProjectileGravity;
			}
			if (i_attack.Type == "multiStage") result["stages"] = new JArray(i_attack.Stages.Select(i_stage => new JObject { ["type"] = i_stage.Type }));
			return result;
		}

		private JArray BuildBehaviorModules(string i_packId, string i_slug)
		{
			JArray modules = new JArray();
			foreach (BehaviorDraft module in m_behaviorDrafts ?? new List<BehaviorDraft>()) modules.Add(BuildBehaviorJson(module));
			if (m_generateStarterFinisher)
			{
				modules.Add(new JObject { ["type"] = "downedFinisher", ["triggerRange"] = m_finisherTriggerRange, ["startDelaySeconds"] = m_finisherStartDelay,
					["durationSeconds"] = m_finisherDuration, ["animation"] = "paired-draft", ["playerAnimationRef"] = i_packId + ":player-animation/" + i_slug + "/paired-draft",
					["meterMax"] = m_finisherMeterMax, ["inputPower"] = m_finisherInputPower, ["decayPerSecond"] = m_finisherDecay, ["cooldownSeconds"] = m_finisherCooldown,
					["successOutcome"] = new JObject { ["healthRecovery"] = m_finisherSuccessRecovery, ["enemyStunSeconds"] = m_finisherSuccessStun },
					["failureOutcome"] = new JObject { ["healthDamage"] = m_finisherFailureHealth, ["strengthDamage"] = m_finisherFailureStrength,
						["pleasure"] = m_finisherFailurePleasure, ["libido"] = m_finisherFailureLibido, ["playerRagdollSeconds"] = m_finisherFailureRagdoll } });
			}
			return modules;
		}

		private static JObject BuildBehaviorJson(BehaviorDraft i_module)
		{
			JObject result = new JObject { ["type"] = i_module.Type };
			switch (i_module.Type)
			{
				case "regeneration": result["amount"] = i_module.Amount; result["intervalSeconds"] = i_module.Interval; break;
				case "berserk": result["healthThreshold"] = i_module.Threshold; result["speedBonus"] = i_module.SpeedBonus; result["damageMultiplierBonus"] = i_module.DamageBonus; break;
				case "lifesteal": case "thorns": result["amount"] = i_module.Amount; break;
				case "onHitRagdoll": result["durationSeconds"] = i_module.Duration; break;
				case "spawnOnDeath": result["enemy"] = i_module.Enemy; result["count"] = i_module.Count; result["radius"] = i_module.Radius; break;
				case "speedPulse": result["speedBonus"] = i_module.SpeedBonus; result["intervalSeconds"] = i_module.Interval; result["durationSeconds"] = i_module.Duration; break;
			}
			return result;
		}

		private static JObject BuildAttackAnimation(string i_packId, string i_slug, string i_enemyId, AttackDraft i_attack)
		{
			float middle = i_attack.Duration * .5f;
			JArray keys = new JArray(Key(0f, 0f), Key(middle, 24f), Key(i_attack.Duration, 0f));
			List<JObject> events = new List<JObject>();
			if (i_attack.AddImpulse) events.Add(new JObject { ["time"] = i_attack.ImpulseTime, ["type"] = "impulse", ["x"] = i_attack.ImpulseX, ["y"] = i_attack.ImpulseY, ["relativeToFacing"] = true });
			if (i_attack.Type == "multiStage")
			{
				int count = i_attack.Stages.Count;
				for (int index = 0; index < count; index++) events.Add(new JObject { ["time"] = i_attack.Duration * (index + 1f) / (count + 1f), ["type"] = "attackHit" });
			}
			else events.Add(new JObject { ["time"] = i_attack.HitTime, ["type"] = "attackHit" });
			events.Sort((i_left, i_right) => ((float)i_left["time"]).CompareTo((float)i_right["time"]));
			return new JObject { ["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = i_packId + ":enemy-animation/" + i_slug + "/attack-" + i_attack.Id,
				["enemy"] = i_enemyId, ["displayName"] = "Attack: " + i_attack.Id, ["durationSeconds"] = i_attack.Duration, ["frameRate"] = 60, ["loop"] = false,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "rotation.z", ["keys"] = keys }), ["events"] = new JArray(events) };
		}

		private static bool UsesProjectile(AttackDraft i_attack)
		{
			return i_attack != null && (i_attack.Type == "projectile" || i_attack.Type == "multiStage" && i_attack.Stages != null && i_attack.Stages.Any(i_stage => i_stage.Type == "projectile"));
		}

		private static JObject Key(float i_time, float i_value) { return new JObject { ["time"] = i_time, ["value"] = i_value, ["inTangent"] = JValue.CreateNull(), ["outTangent"] = JValue.CreateNull() }; }

		private static JObject BuildPlayerPairedDraft(string i_packId, string i_slug)
		{
			return new JObject { ["schemaVersion"] = 1, ["type"] = "playerAnimation", ["id"] = i_packId + ":player-animation/" + i_slug + "/paired-draft",
				["rig"] = "core:player-rig/alex", ["displayName"] = "Paired Interaction Draft (Player)", ["durationSeconds"] = 1f, ["frameRate"] = 60,
				["loop"] = true, ["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "position.y",
					["keys"] = new JArray(Key(0f, 0f), Key(.5f, 0f), Key(1f, 0f)) }) };
		}

		private static JObject BuildTestStage(string i_packId, string i_slug, string i_enemyId)
		{
			return new JObject { ["schemaVersion"] = 1, ["type"] = "stage", ["id"] = i_packId + ":stage/" + i_slug + "-test", ["displayName"] = "Test: " + i_slug,
				["extends"] = "core:stage/field-day", ["description"] = "Generated isolated test arena for " + i_enemyId + ".",
				["layout"] = new JObject { ["type"] = "coreStageLayout", ["playerSpawn"] = new JObject { ["x"] = 0, ["y"] = 2 } },
				["waves"] = new JObject { ["firstWaveEnemyCount"] = 1 },
				["testTools"] = new JObject { ["enabled"] = true, ["enemy"] = i_enemyId, ["spawnPosition"] = new JObject { ["x"] = 5, ["y"] = -5.6 } },
				["spawners"] = new JArray(new JObject { ["id"] = "enemy-test", ["position"] = new JObject { ["x"] = 8, ["y"] = -5.6 }, ["enemies"] = new JArray(i_enemyId),
					["selectionWeight"] = 1, ["minimumWave"] = 0, ["delaySeconds"] = 1, ["delayJitterSeconds"] = 0, ["initialDelaySeconds"] = 0.5,
					["initialDelayJitterSeconds"] = 0, ["spawnOutOfSight"] = false, ["enabled"] = true }) };
		}

		private static Dictionary<string, JObject> BuildRegions(IReadOnlyList<BonePreset> i_bones, bool i_includeProjectile)
		{
			Dictionary<string, JObject> result = new Dictionary<string, JObject>(StringComparer.Ordinal); const int columns = 4, cell = 32;
			for (int index = 0; index < i_bones.Count; index++) result.Add(i_bones[index].Id,
				new JObject { ["x"] = index % columns * cell, ["y"] = index / columns * cell, ["width"] = cell, ["height"] = cell });
			if (i_includeProjectile)
			{
				int index = i_bones.Count;
				result.Add("projectile", new JObject { ["x"] = index % columns * cell, ["y"] = index / columns * cell, ["width"] = cell, ["height"] = cell });
			}
			return result;
		}

		private static JObject BuildAtlasGuide(IReadOnlyList<BonePreset> i_bones, Dictionary<string, JObject> i_regions)
		{
			return new JObject { ["cellSize"] = 32, ["columns"] = 4, ["coordinateOrigin"] = "bottom-left", ["instructions"] = "Replace each colored cell with transparent artwork without moving its rectangle. Update both this guide and the enemy visual.regions if cell sizes change.",
				["regions"] = JObject.FromObject(i_regions), ["boneOrder"] = new JArray(i_bones.Select(i_bone => i_bone.Id)) };
		}

		private static void WriteAtlas(string i_path, int i_count)
		{
			const int columns = 4, cell = 32; int rows = Mathf.Max(1, Mathf.CeilToInt(i_count / (float)columns));
			Texture2D texture = new Texture2D(columns * cell, rows * cell, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
			try
			{
				Color32[] clear = Enumerable.Repeat(new Color32(0, 0, 0, 0), texture.width * texture.height).ToArray(); texture.SetPixels32(clear);
				for (int index = 0; index < i_count; index++)
				{
					Color color = Color.HSVToRGB((index * 0.618034f) % 1f, 0.58f, 0.9f); int originX = index % columns * cell, originY = index / columns * cell;
					for (int y = 2; y < cell - 2; y++) for (int x = 2; x < cell - 2; x++)
					{
						bool guide = x == cell / 2 || y == cell / 2; Color pixel = guide ? Color.Lerp(color, Color.white, 0.5f) : color; pixel.a = guide ? 0.9f : 0.72f;
						texture.SetPixel(originX + x, originY + y, pixel);
					}
				}
				texture.Apply(); File.WriteAllBytes(i_path, texture.EncodeToPNG());
			}
			finally { UnityEngine.Object.DestroyImmediate(texture); }
		}

		private static void WriteReadme(string i_path, string i_slug, Preset i_preset, bool i_paired)
		{
			File.WriteAllText(i_path, "ORIGINAL ENEMY STARTER\r\n\r\nTopology: " + i_preset.Name + "\r\n"
				+ "1. Replace the colored cells in " + i_slug + "-atlas.png while keeping their rectangles.\r\n"
				+ "2. Adjust pivots and bone positions in ../../../content/" + i_slug + ".enemy.json.\r\n"
				+ "3. Edit the normalized animation files under ../../../content/animations/" + i_slug + ".\r\n"
				+ "4. Preview through Captivity Reloaded > Modding > Player Animation Preview.\r\n"
				+ "5. Validate and package through Captivity Mod Packager.\r\n"
				+ (i_paired ? "\r\nPAIRED DRAFT\r\nThe paired-draft enemy and player clips are authoring templates only. Import both, then reference them from a finisher phase or scripted interaction before release.\r\n" : string.Empty), new System.Text.UTF8Encoding(false));
		}

		private static JObject RemoveNulls(JObject i_object) { foreach (JProperty property in i_object.Properties().Where(i_property => i_property.Value.Type == JTokenType.Null).ToList()) property.Remove(); return i_object; }
		private static void WriteJson(string i_path, JObject i_document) { File.WriteAllText(i_path, i_document.ToString(Formatting.Indented), new System.Text.UTF8Encoding(false)); }
		private static string Slug(string i_value) { return new string((i_value ?? string.Empty).Trim().ToLowerInvariant().Where(i_char => char.IsLetterOrDigit(i_char) || i_char == '-' || i_char == '_').ToArray()); }
		private static string ProjectRoot() { return FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..")); }
		private static string ProjectPath(string i_path) { return FilePath.GetFullPath(FilePath.IsPathRooted(i_path ?? string.Empty) ? i_path : FilePath.Combine(ProjectRoot(), i_path ?? string.Empty)); }
		private static string RelativeProjectPath(string i_path) { string root = ProjectRoot(), full = FilePath.GetFullPath(i_path); return full.StartsWith(root + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ? full.Substring(root.Length + 1).Replace('\\', '/') : full; }

		private static Preset PresetFor(Topology i_topology)
		{
			switch (i_topology)
			{
				case Topology.Quadruped: return Quadruped();
				case Topology.Flying: return Flying();
				case Topology.Crawler: return Crawler();
				case Topology.Blank: return Blank();
				default: return Humanoid();
			}
		}

		private static Preset Humanoid()
		{
			return new Preset("Humanoid", "groundChase", 0.75f, 3.4f, 0.1f, new[] {
				B("hips", null, 0, 0, 10), B("butt", "hips", 0, -0.08f, 9), B("spine", "hips", 0, 0.18f, 11), B("chest", "spine", 0, 0.38f, 12), B("neck", "chest", 0, 0.38f, 11), B("head", "neck", 0, 0.18f, 12),
				B("arm-left-upper", "chest", -0.23f, 0.25f, -15), B("arm-left-lower", "arm-left-upper", 0, -0.48f, -14), B("hand-left", "arm-left-lower", 0, -0.38f, -13),
				B("arm-right-upper", "chest", 0.23f, 0.25f, 13), B("arm-right-lower", "arm-right-upper", 0, -0.48f, 14), B("hand-right", "arm-right-lower", 0, -0.38f, 15),
				B("leg-left-upper", "hips", -0.12f, -0.2f, -11), B("leg-left-lower", "leg-left-upper", 0, -0.62f, -12), B("foot-left", "leg-left-lower", 0, -0.55f, -13),
				B("leg-right-upper", "hips", 0.12f, -0.2f, 13), B("leg-right-lower", "leg-right-upper", 0, -0.62f, 12), B("foot-right", "leg-right-lower", 0, -0.55f, 11)
			}, new[] { Z("hips", .65f, .55f), Z("chest", .72f, .72f), C("head", .36f, "critical"), Z("leg-left-upper", .34f, .7f), Z("leg-right-upper", .34f, .7f) });
		}

		private static Preset Quadruped()
		{
			return new Preset("Quadruped", "groundChase", 1.8f, 1.25f, 0.15f, new[] {
				B("hips", null, 0, 0, 10), B("body", "hips", .45f, .08f, 11), B("neck", "body", .48f, .12f, 12), B("head", "neck", .3f, .05f, 13), B("tail", "hips", -.45f, .12f, 8),
				B("leg-back-left", "hips", -.2f, -.22f, 8), B("foot-back-left", "leg-back-left", 0, -.5f, 8), B("leg-back-right", "hips", -.05f, -.22f, 14), B("foot-back-right", "leg-back-right", 0, -.5f, 14),
				B("leg-front-left", "body", .32f, -.2f, 9), B("foot-front-left", "leg-front-left", 0, -.5f, 9), B("leg-front-right", "body", .48f, -.2f, 15), B("foot-front-right", "leg-front-right", 0, -.5f, 15)
			}, new[] { Z("hips", .8f, .7f), Z("body", 1.1f, .75f), C("head", .35f, "critical") });
		}

		private static Preset Flying()
		{
			return new Preset("Flying", "flyingChase", 1.1f, 1.1f, 0, new[] {
				B("hips", null, 0, 0, 10), B("body", "hips", 0, .12f, 11), B("head", "body", .38f, .18f, 13), B("wing-left", "body", -.4f, .1f, 7), B("wing-right", "body", .4f, .1f, 15), B("leg-left", "hips", -.18f, -.3f, 8), B("leg-right", "hips", .18f, -.3f, 14)
			}, new[] { C("hips", .48f, "normal"), C("body", .5f, "normal"), C("head", .28f, "critical") });
		}

		private static Preset Crawler()
		{
			return new Preset("Crawler", "groundChase", 1.5f, .8f, .05f, new[] {
				B("hips", null, 0, 0, 10), B("body", "hips", .28f, .02f, 11), B("head", "body", .38f, .05f, 13), B("leg-left-front", "body", .2f, -.2f, 8), B("leg-right-front", "body", .3f, -.2f, 14), B("leg-left-back", "hips", -.22f, -.2f, 7), B("leg-right-back", "hips", -.12f, -.2f, 15)
			}, new[] { Z("hips", .65f, .5f), Z("body", .75f, .5f), C("head", .28f, "critical") });
		}

		private static Preset Blank() { return new Preset("Blank custom", "holdPosition", .75f, 1f, 0, new[] { B("hips", null, 0, 0, 10) }, new[] { Z("hips", .65f, .8f) }); }
		private static BonePreset B(string i_id, string i_parent, float i_x, float i_y, int i_sorting) { return new BonePreset { Id = i_id, Parent = i_parent, X = i_x, Y = i_y, PivotX = .5f, PivotY = .5f, SortingOrder = i_sorting }; }
		private static HitZonePreset Z(string i_bone, float i_width, float i_height) { return new HitZonePreset { Bone = i_bone, Width = i_width, Height = i_height, Damage = "normal" }; }
		private static HitZonePreset C(string i_bone, float i_radius, string i_damage) { return new HitZonePreset { Bone = i_bone, Circle = true, Radius = i_radius, Damage = i_damage }; }

		private sealed class Preset
		{
			public readonly string Name; public readonly string Ai; public readonly float BodyWidth; public readonly float BodyHeight; public readonly float BodyOffsetY; public readonly List<BonePreset> Bones; public readonly List<HitZonePreset> HitZones;
			public Preset(string i_name, string i_ai, float i_width, float i_height, float i_offsetY, IEnumerable<BonePreset> i_bones, IEnumerable<HitZonePreset> i_zones) { Name = i_name; Ai = i_ai; BodyWidth = i_width; BodyHeight = i_height; BodyOffsetY = i_offsetY; Bones = i_bones.ToList(); HitZones = i_zones.ToList(); }
		}
		private sealed class BonePreset { public string Id; public string Parent; public float X; public float Y; public float PivotX; public float PivotY; public int SortingOrder; }
		private sealed class HitZonePreset { public string Bone; public bool Circle; public float Width; public float Height; public float Radius; public string Damage; }
		[Serializable]
		private sealed class AttackStageDraft
		{
			public string Type = "melee";
			public AttackStageDraft Clone() { return new AttackStageDraft { Type = Type }; }
		}
		[Serializable]
		private sealed class BehaviorDraft
		{
			public string Type;
			public float Amount = 2f;
			public float Interval = 3f;
			public float Threshold = .3f;
			public float SpeedBonus = 1f;
			public float DamageBonus = .5f;
			public float Duration = 1f;
			public string Enemy = "core:enemy/zombie-1";
			public int Count = 1;
			public float Radius = 1f;
			public static BehaviorDraft Create(string i_type)
			{
				BehaviorDraft result = new BehaviorDraft { Type = i_type };
				if (i_type == "regeneration") { result.Amount = 2f; result.Interval = 2f; }
				else if (i_type == "lifesteal" || i_type == "thorns") result.Amount = 2f;
				else if (i_type == "onHitRagdoll") result.Duration = .5f;
				else if (i_type == "speedPulse") { result.SpeedBonus = 2f; result.Interval = 4f; result.Duration = 1f; }
				return result;
			}
		}
		[Serializable]
		private sealed class AttackDraft
		{
			public bool Expanded = true;
			public string Id = "primary";
			public string Type = "melee";
			public float Weight = 1f;
			public float Damage = 8f;
			public float KnockbackX = 3f;
			public float KnockbackY = 1f;
			public float Cooldown = 1.1f;
			public float InitiateRange = 1.5f;
			public float HitRange = 1.25f;
			public float Duration = .6f;
			public float HitTime = .3f;
			public bool MovesDuringAttack;
			public bool AddImpulse;
			public float ImpulseTime = .08f;
			public float ImpulseX = 8f;
			public float ImpulseY;
			public float AreaRadius = 2f;
			public float OffsetX = 1f;
			public float OffsetY;
			public float ProjectileSpeed = 8f;
			public float ProjectileLifetime = 4f;
			public float ProjectileRadius = .15f;
			public float ProjectileGravity;
			public List<AttackStageDraft> Stages = new List<AttackStageDraft>();

			public AttackDraft Clone(string i_id)
			{
				return new AttackDraft { Id = i_id, Type = Type, Weight = Weight, Damage = Damage, KnockbackX = KnockbackX, KnockbackY = KnockbackY,
					Cooldown = Cooldown, InitiateRange = InitiateRange, HitRange = HitRange, Duration = Duration, HitTime = HitTime,
					MovesDuringAttack = MovesDuringAttack, AddImpulse = AddImpulse, ImpulseTime = ImpulseTime, ImpulseX = ImpulseX, ImpulseY = ImpulseY,
					AreaRadius = AreaRadius, OffsetX = OffsetX, OffsetY = OffsetY, ProjectileSpeed = ProjectileSpeed,
					ProjectileLifetime = ProjectileLifetime, ProjectileRadius = ProjectileRadius, ProjectileGravity = ProjectileGravity,
					Stages = Stages == null ? new List<AttackStageDraft>() : Stages.Select(i_stage => i_stage.Clone()).ToList() };
			}

			public static AttackDraft Melee(string i_id) { return new AttackDraft { Id = i_id }; }
			public static AttackDraft Hitscan(string i_id) { return new AttackDraft { Id = i_id, Type = "hitscan", Damage = 7f, Cooldown = 1.4f, InitiateRange = 9f, HitRange = 10f, Duration = .55f, HitTime = .28f }; }
			public static AttackDraft Projectile(string i_id) { return new AttackDraft { Id = i_id, Type = "projectile", Damage = 6f, Cooldown = 1.6f, InitiateRange = 8f, HitRange = 10f, Duration = .65f, HitTime = .32f }; }
			public static AttackDraft Area(string i_id) { return new AttackDraft { Id = i_id, Type = "area", Damage = 10f, Cooldown = 2.2f, InitiateRange = 2.5f, HitRange = 2.5f, Duration = .9f, HitTime = .55f, AreaRadius = 2f, OffsetX = .8f }; }
			public static AttackDraft Charge(string i_id) { return new AttackDraft { Id = i_id, Type = "melee", Damage = 12f, Cooldown = 2f, InitiateRange = 5f, HitRange = 1.5f, Duration = .75f, HitTime = .42f, MovesDuringAttack = true, AddImpulse = true, ImpulseTime = .08f, ImpulseX = 8f }; }
			public static AttackDraft Grab(string i_id) { return new AttackDraft { Id = i_id, Type = "grab", Damage = 0f, KnockbackX = 0f, KnockbackY = 0f, Cooldown = 4f, InitiateRange = 1.5f, HitRange = 1.2f, Duration = .7f, HitTime = .35f }; }
			public static AttackDraft Combo(string i_id) { return new AttackDraft { Id = i_id, Type = "multiStage", Damage = 5f, Cooldown = 1.8f, InitiateRange = 1.6f, HitRange = 1.3f, Duration = .9f, HitTime = .3f,
				Stages = new List<AttackStageDraft> { new AttackStageDraft { Type = "melee" }, new AttackStageDraft { Type = "melee" } } }; }
		}
		private sealed class FinisherEnemyClip { public string Semantic; public string Id; public string DisplayName; public string Path; public float Duration; }
		private sealed class FinisherPlayerClip { public string Id; public string DisplayName; public string Path; public float Duration; }
	}
}
