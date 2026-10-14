using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed class SimpleRetextureWizardWindow : EditorWindow
	{
		private const string PackPreference = "CaptivityReloaded.SimpleRetexture.Pack";
		private const string ArtPreference = "CaptivityReloaded.SimpleRetexture.Art";
		[SerializeField] private string m_packRoot = string.Empty;
		[SerializeField] private string m_artFolder = string.Empty;
		[SerializeField] private string m_patchName = "enemy-retexture";
		[SerializeField] private int m_enemyIndex;
		[SerializeField] private int m_sourceScale = 1;
		[SerializeField] private bool m_customTarget;
		[SerializeField] private string m_customTargetId = "core:weapon/pistol";
		[SerializeField] private string m_manualSlot = "body";
		[SerializeField] private string m_manualPng = string.Empty;
		private readonly List<ReplacementRow> m_rows = new List<ReplacementRow>();
		private List<CoreEnemyAssetSlotEnemy> m_enemies = new List<CoreEnemyAssetSlotEnemy>();
		private string[] m_enemyNames = new string[0];
		private Vector2 m_scroll;
		private string m_message;
		private MessageType m_messageType;

		private sealed class ReplacementRow
		{
			public bool Included = true;
			public string Slot;
			public string Source;
		}

		[MenuItem("Captivity Reloaded/Modding/Create Simple Retexture...")]
		public static void Open()
		{
			SimpleRetextureWizardWindow window = GetWindow<SimpleRetextureWizardWindow>();
			window.titleContent = new GUIContent("Simple Retexture");
			window.minSize = new Vector2(650f, 520f);
			window.Show();
		}

		private void OnEnable()
		{
			m_packRoot = EditorPrefs.GetString(PackPreference, DefaultPackRoot());
			m_artFolder = EditorPrefs.GetString(ArtPreference, DefaultArtFolder());
			try
			{
				m_enemies = CoreEnemyAssetSlotCatalog.Document.Enemies.OrderBy(i_enemy => i_enemy.Id, StringComparer.Ordinal).ToList();
				m_enemyNames = m_enemies.Select(i_enemy => i_enemy.Id.Substring("core:enemy/".Length)).ToArray();
			}
			catch (Exception exception)
			{
				m_message = exception.Message;
				m_messageType = MessageType.Error;
			}
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("Simple retexture", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Turns edited PNGs into a valid asset patch. The wizard copies artwork into the mod, writes the patch JSON, adds every required manifest override, and can undo the 4x scale used by Core DragonBones exports.", MessageType.Info);

			DrawFolder("Existing mod folder", ref m_packRoot, "Choose a mod folder", PackPreference);
			m_patchName = EditorGUILayout.TextField("Patch name", m_patchName);
			bool previousCustomTarget = m_customTarget;
			m_customTarget = EditorGUILayout.ToggleLeft("Use a custom public target (weapons, player, clothing, stages)", m_customTarget);
			if (m_customTarget != previousCustomTarget)
			{
				m_rows.Clear();
				SetMessage("Target mode changed; add replacements for the new target.", MessageType.Info);
			}
			if (m_customTarget) DrawCustomTarget();
			else DrawEnemyTarget();

			EditorGUILayout.Space(6f);
			DrawMatches();
			if (!string.IsNullOrEmpty(m_message)) EditorGUILayout.HelpBox(m_message, m_messageType);

			using (new EditorGUI.DisabledScope(m_rows.All(i_row => !i_row.Included)))
			{
				if (GUILayout.Button("Create or update retexture", GUILayout.Height(34f))) Build();
			}
		}

		private void DrawEnemyTarget()
		{
			using (new EditorGUI.DisabledScope(m_enemyNames.Length == 0))
			{
				int oldIndex = m_enemyIndex;
				m_enemyIndex = EditorGUILayout.Popup("Core enemy", Mathf.Clamp(m_enemyIndex, 0, Math.Max(0, m_enemyNames.Length - 1)), m_enemyNames);
				if (oldIndex != m_enemyIndex) { m_rows.Clear(); m_patchName = m_enemyNames[m_enemyIndex] + "-retexture"; }
			}
			DrawFolder("Edited PNG folder", ref m_artFolder, "Choose edited enemy PNGs", ArtPreference);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_sourceScale = EditorGUILayout.IntPopup("PNG scale", m_sourceScale,
					new[] { "1x (native)", "2x", "3x", "4x (DragonBones export)" }, new[] { 1, 2, 3, 4 });
				if (GUILayout.Button("Auto-detect", GUILayout.Width(90f))) DetectScale();
			}
			EditorGUILayout.HelpBox("Choose the enemy's images folder from the Core DragonBones export, or any folder containing original sprite-name PNGs. Matching uses the generated public slot catalog; numbered -2 files are assigned to mirrored limbs. Keep 4x selected for edited DragonBones exports so the packaged art returns to native game size.", MessageType.None);
			if (GUILayout.Button("Scan folder and match slots", GUILayout.Height(28f))) ScanEnemyFolder();
		}

		private void DrawCustomTarget()
		{
			m_customTargetId = EditorGUILayout.TextField("Target content ID", m_customTargetId);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_manualSlot = EditorGUILayout.TextField("Relative slot", m_manualSlot);
				if (GUILayout.Button("Add PNG...", GUILayout.Width(90f)))
				{
					string selected = EditorUtility.OpenFilePanel("Choose replacement PNG", Directory.Exists(m_artFolder) ? m_artFolder : ProjectRoot(), "png");
					if (!string.IsNullOrEmpty(selected))
					{
						m_manualPng = selected;
						AddManual();
					}
				}
			}
			m_sourceScale = EditorGUILayout.IntPopup("PNG scale", m_sourceScale,
				new[] { "1x (native)", "2x", "3x", "4x" }, new[] { 1, 2, 3, 4 });
			EditorGUILayout.HelpBox("Enter the target owner from the public asset-slot catalog, then the portion of the slot after that owner. Example: target core:weapon/pistol and relative slot body.", MessageType.None);
		}

		private void DrawMatches()
		{
			EditorGUILayout.LabelField("Replacements (" + m_rows.Count(i_row => i_row.Included) + ")", EditorStyles.boldLabel);
			if (m_rows.Count == 0)
			{
				EditorGUILayout.HelpBox("No PNGs added yet.", MessageType.None);
				return;
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Select all", GUILayout.Width(90f))) m_rows.ForEach(i_row => i_row.Included = true);
				if (GUILayout.Button("Clear all", GUILayout.Width(90f))) m_rows.ForEach(i_row => i_row.Included = false);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Remove unchecked", GUILayout.Width(130f))) m_rows.RemoveAll(i_row => !i_row.Included);
			}
			m_scroll = EditorGUILayout.BeginScrollView(m_scroll, GUILayout.MinHeight(120f), GUILayout.MaxHeight(270f));
			foreach (ReplacementRow row in m_rows)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					row.Included = EditorGUILayout.Toggle(row.Included, GUILayout.Width(20f));
					EditorGUILayout.SelectableLabel(row.Slot, EditorStyles.textField, GUILayout.Height(18f), GUILayout.Width(260f));
					EditorGUILayout.SelectableLabel(FilePath.GetFileName(row.Source), EditorStyles.textField, GUILayout.Height(18f));
				}
			}
			EditorGUILayout.EndScrollView();
		}

		private void ScanEnemyFolder()
		{
			if (m_enemies.Count == 0 || !Directory.Exists(m_artFolder))
			{
				SetMessage("Choose an existing PNG folder first.", MessageType.Error);
				return;
			}
			DetectScale();
			string[] files = Directory.GetFiles(m_artFolder, "*.png", SearchOption.TopDirectoryOnly);
			SimpleRetextureMatchResult matched = SimpleRetexturePackBuilder.MatchEnemyPngs(m_enemies[m_enemyIndex], files);
			m_rows.Clear();
			m_rows.AddRange(matched.Matches.Select(i_match => new ReplacementRow { Slot = i_match.Slot, Source = i_match.SourcePng }));
			string unmatched = matched.UnmatchedPngs.Count == 0 ? string.Empty : " " + matched.UnmatchedPngs.Count + " PNG(s) were not recognized and were left out.";
			SetMessage("Matched " + m_rows.Count + " public slots from " + files.Length + " PNG files." + unmatched, m_rows.Count > 0 ? MessageType.Info : MessageType.Warning);
		}

		private void AddManual()
		{
			string slot = (m_manualSlot ?? string.Empty).Trim().Trim('/');
			if (!ContentId.TryParse("slot:" + slot, out _)) { SetMessage("Enter a valid relative slot before choosing a PNG.", MessageType.Error); return; }
			m_rows.RemoveAll(i_row => i_row.Slot == slot);
			m_rows.Add(new ReplacementRow { Slot = slot, Source = m_manualPng });
			m_rows.Sort((i_left, i_right) => string.CompareOrdinal(i_left.Slot, i_right.Slot));
			SetMessage("Added " + slot + ".", MessageType.Info);
		}

		private void DetectScale()
		{
			string directory = m_artFolder;
			if (string.IsNullOrEmpty(directory)) return;
			string metadata = FilePath.Combine(FilePath.GetDirectoryName(directory) ?? string.Empty, "captivity-roundtrip.json");
			if (!File.Exists(metadata)) return;
			try
			{
				int detected = (int?)JObject.Parse(File.ReadAllText(metadata))["imageScale"] ?? 1;
				if (detected >= 1 && detected <= 4) m_sourceScale = detected;
			}
			catch (Exception exception) { SetMessage("Could not read DragonBones scale metadata: " + exception.Message, MessageType.Warning); }
		}

		private void Build()
		{
			try
			{
				string target = m_customTarget ? m_customTargetId.Trim() : m_enemies[m_enemyIndex].Id;
				List<SimpleRetextureInput> inputs = m_rows.Where(i_row => i_row.Included)
					.Select(i_row => new SimpleRetextureInput { Slot = i_row.Slot, SourcePng = i_row.Source }).ToList();
				string patchPath = FilePath.Combine(m_packRoot, "content", "retexture-" + m_patchName + ".json");
				bool overwrite = !File.Exists(patchPath) || EditorUtility.DisplayDialog("Update retexture?", "This patch already exists. Replace its generated PNGs and update the manifest?", "Update", "Cancel");
				if (!overwrite) return;
				SimpleRetextureBuildResult built = SimpleRetexturePackBuilder.Build(m_packRoot, m_patchName, target, inputs, m_sourceScale, true);
				if (!built.Report.IsValid)
				{
					SetMessage(string.Join("\n", built.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray()), MessageType.Error);
					return;
				}
				CapmodPackageResult validation = CapmodPackageBuilder.ValidateSource(m_packRoot);
				if (!validation.Report.IsValid)
				{
					SetMessage("The retexture was written, but pack validation found:\n" + string.Join("\n", validation.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray()), MessageType.Warning);
					return;
				}
				EditorPrefs.SetString("CaptivityReloaded.ModPackager.Source", m_packRoot);
				AssetDatabase.Refresh();
				SetMessage("Created " + built.ReplacementCount + " replacements and validated the mod. Open Captivity Mod Packager to build the .capmod.", MessageType.Info);
			}
			catch (Exception exception) { SetMessage(exception.Message, MessageType.Error); }
		}

		private void DrawFolder(string i_label, ref string io_value, string i_title, string i_preference)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				io_value = EditorGUILayout.TextField(i_label, io_value);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFolderPanel(i_title, Directory.Exists(io_value) ? io_value : ProjectRoot(), string.Empty);
					if (!string.IsNullOrEmpty(selected)) { io_value = selected; EditorPrefs.SetString(i_preference, selected); }
				}
			}
		}

		private void SetMessage(string i_message, MessageType i_type) { m_message = i_message; m_messageType = i_type; Repaint(); }
		private static string ProjectRoot() { return FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..")); }
		private static string DefaultPackRoot() { return FilePath.Combine(ProjectRoot(), "ExampleMods"); }
		private static string DefaultArtFolder() { return FilePath.Combine(ProjectRoot(), "DragonBonesExports", "core-enemies"); }
	}
}
