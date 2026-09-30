using System;
using System.Collections.Generic;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed partial class PlayerAnimationPreviewWindow
	{
		public static void OpenModPair(string i_modRoot, string i_enemyPath, string i_enemyAnimationPath, string i_playerAnimationPath)
		{
			PlayerAnimationPreviewWindow window = GetWindow<PlayerAnimationPreviewWindow>();
			window.titleContent = new GUIContent("Animation Preview"); window.minSize = new Vector2(820f, 620f); window.Show();
			window.m_mode = 3; window.m_modPackRoot = i_modRoot; window.m_modEnemyPath = i_enemyPath; window.m_modAnimationPath = i_enemyAnimationPath;
			window.RefreshModPackContents();
			window.m_modEnemyIndex = Mathf.Max(0, window.m_modEnemies.FindIndex(i_item => string.Equals(i_item.Path, i_enemyPath, StringComparison.OrdinalIgnoreCase)));
			window.ApplyModDiscoverySelection();
			List<ModAnimationChoice> matching = window.MatchingModAnimations();
			int animationIndex = matching.FindIndex(i_item => string.Equals(i_item.Path, i_enemyAnimationPath, StringComparison.OrdinalIgnoreCase));
			window.m_modAnimationIndex = animationIndex < 0 ? 0 : animationIndex + 1; window.ApplyModDiscoverySelection(); window.SelectCurrentClip();
			window.LoadStudioPlayerAnimation(i_playerAnimationPath); window.m_designStudio = true; window.m_studioPairSetup = true; window.m_studioPairPreview = true;
			window.m_studioPairStatus = "Previewing the paired interaction selected by the wizard."; window.SampleCurrentClip(); window.Repaint();
		}

		[SerializeField] private bool m_designStudio;
		[SerializeField] private string m_studioBone;
		[SerializeField] private Vector2 m_studioOutlinerScroll;
		[SerializeField] private Vector2 m_studioInspectorScroll;
		[SerializeField] private Vector2 m_studioDopeScroll;
		[SerializeField] private string m_studioBoneSearch = string.Empty;
		[SerializeField] private bool m_studioAutoKey = true;
		[SerializeField] private bool m_studioSnapFrames = true;
		[SerializeField] private bool m_studioShowFrames = true;
		[SerializeField] private bool m_studioIsolateSelection;
		[SerializeField] private float m_studioMainSplit = 0.64f;
		[SerializeField] private float m_studioOutlinerWidth = 190f;
		[SerializeField] private float m_studioInspectorWidth = 255f;
		[SerializeField] private float m_studioViewportZoom = 1f;
		[SerializeField] private Vector2 m_studioViewportPan;
		[SerializeField] private int m_studioCompactPanel = 1;
		[SerializeField] private bool m_studioWholeRig;
		[SerializeField] private List<string> m_studioSelectedBones = new List<string>();
		[SerializeField] private bool m_studioPlayerFacingLeft;
		[SerializeField] private bool m_studioEnemyFacingLeft;
		[SerializeField] private bool m_studioPairSetup;
		[SerializeField] private bool m_studioPairPreview;
		[SerializeField] private string m_studioPlayerAnimationPath;
		[SerializeField] private string m_studioPairName = "Paired Player Animation";
		[SerializeField] private string m_studioPairPhase = "paired";
		[SerializeField] private bool m_studioEditPlayer;
		[SerializeField] private string m_studioPlayerJson;
		private NormalizedPlayerAnimationDocument m_studioPlayerAnimation;
		private string m_studioPairStatus;
		private string m_studioPlayerValidation;
		private int m_studioSplitter;
		private Vector2 m_studioSplitterMouse;
		private float m_studioSplitterValue;
		private readonly List<string> m_studioPickCycle = new List<string>();
		private Vector2 m_studioLastPickPosition = new Vector2(-1000f, -1000f);
		private int m_studioPickCycleIndex;
		private NormalizedNumericTrack m_studioDraggedTrack;
		private NormalizedNumericKey m_studioDraggedKey;
		private NormalizedNumericTrack m_studioSelectedNumericTrack;
		private NormalizedNumericKey m_studioSelectedNumericKey;
		private readonly Dictionary<string, StudioPoseValue> m_studioPoseClipboard = new Dictionary<string, StudioPoseValue>(StringComparer.Ordinal);
		private int m_studioSelectedTrack = -1;
		private int m_studioSelectedKey = -1;
		[SerializeField] private int m_studioTransformTool;
		private bool m_studioViewportDragging;
		private bool m_studioViewportMoved;
		private Vector2 m_studioDragMouse;
		private Vector3 m_studioDragPosition;
		private Vector3 m_studioDragScale;
		private float m_studioDragRotation;
		private readonly Dictionary<string, StudioTransformStart> m_studioDragStarts = new Dictionary<string, StudioTransformStart>(StringComparer.Ordinal);
		private string m_studioPivotBone;
		private float m_studioPivotX = 0.5f;
		private float m_studioPivotY = 0.5f;

		private void DrawAnimationDesignStudio()
		{
			HandleStudioShortcuts();
			DrawStudioHeader();
			DrawStudioQolBar();
			if (m_studioPairSetup) DrawStudioPairSetup();
			bool compact = position.width < 720f;
			if (compact) m_studioCompactPanel = GUILayout.Toolbar(Mathf.Clamp(m_studioCompactPanel, 0, 2), new[] { "Outliner", "Viewport", "Properties" });
			float available = Mathf.Max(330f, position.height - GUILayoutUtility.GetLastRect().yMax - 24f);
			Rect workspace = GUILayoutUtility.GetRect(100f, 10000f, available, available, GUILayout.ExpandWidth(true));
			float mainHeight = Mathf.Clamp(workspace.height * m_studioMainSplit, 180f, Mathf.Max(180f, workspace.height - 125f));
			Rect main = new Rect(workspace.x, workspace.y, workspace.width, mainHeight);
			Rect horizontalSplitter = new Rect(workspace.x, main.yMax + 2f, workspace.width, 6f);
			Rect dopeSheet = new Rect(workspace.x, horizontalSplitter.yMax + 2f, workspace.width, Mathf.Max(112f, workspace.yMax - horizontalSplitter.yMax - 2f));
			Rect leftSplitter = default, rightSplitter = default;
			if (compact)
			{
				if (m_studioCompactPanel == 0) DrawStudioOutliner(main);
				else if (m_studioCompactPanel == 1) DrawStudioViewport(main);
				else DrawStudioInspector(main);
			}
			else
			{
				m_studioOutlinerWidth = Mathf.Clamp(m_studioOutlinerWidth, 125f, Mathf.Max(125f, main.width - 330f));
				m_studioInspectorWidth = Mathf.Clamp(m_studioInspectorWidth, 185f, Mathf.Max(185f, main.width - m_studioOutlinerWidth - 110f));
				Rect outliner = new Rect(main.x, main.y, m_studioOutlinerWidth, main.height);
				leftSplitter = new Rect(outliner.xMax + 1f, main.y, 5f, main.height);
				Rect inspector = new Rect(main.xMax - m_studioInspectorWidth, main.y, m_studioInspectorWidth, main.height);
				rightSplitter = new Rect(inspector.x - 6f, main.y, 5f, main.height);
				Rect viewport = new Rect(outliner.xMax + 3f, main.y, Mathf.Max(80f, inspector.x - outliner.xMax - 6f), main.height);
				DrawStudioOutliner(outliner); DrawStudioViewport(viewport); DrawStudioInspector(inspector);
			}
			DrawStudioDopeSheet(dopeSheet);
			DrawStudioSplitter(horizontalSplitter, 1);
			if (!compact) { DrawStudioSplitter(leftSplitter, 2); DrawStudioSplitter(rightSplitter, 3); }
			string validation = m_studioEditPlayer ? m_studioPlayerValidation : m_authoringValidation;
			EditorGUILayout.LabelField(string.IsNullOrEmpty(validation) ? "Valid animation document" : validation,
				string.IsNullOrEmpty(validation) ? EditorStyles.miniLabel : EditorStyles.helpBox);
		}

		private void DrawStudioSplitter(Rect i_rect, int i_id)
		{
			EditorGUIUtility.AddCursorRect(i_rect, i_id == 1 ? MouseCursor.ResizeVertical : MouseCursor.ResizeHorizontal);
			if (Event.current.type == EventType.Repaint) EditorGUI.DrawRect(i_rect, m_studioSplitter == i_id ? new Color(0.35f, 0.65f, 1f, 0.8f) : new Color(0.24f, 0.24f, 0.24f, 1f));
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && i_rect.Contains(Event.current.mousePosition))
			{
				m_studioSplitter = i_id; m_studioSplitterMouse = Event.current.mousePosition;
				m_studioSplitterValue = i_id == 1 ? m_studioMainSplit : i_id == 2 ? m_studioOutlinerWidth : m_studioInspectorWidth;
				Event.current.Use();
			}
			if (m_studioSplitter == i_id && Event.current.type == EventType.MouseDrag)
			{
				Vector2 delta = Event.current.mousePosition - m_studioSplitterMouse;
				if (i_id == 1) m_studioMainSplit = Mathf.Clamp(m_studioSplitterValue + delta.y / Mathf.Max(1f, position.height), 0.35f, 0.82f);
				else if (i_id == 2) m_studioOutlinerWidth = Mathf.Max(125f, m_studioSplitterValue + delta.x);
				else m_studioInspectorWidth = Mathf.Max(185f, m_studioSplitterValue - delta.x);
				Repaint(); Event.current.Use();
			}
			if (m_studioSplitter == i_id && Event.current.rawType == EventType.MouseUp) { m_studioSplitter = 0; Repaint(); }
		}

		private void DrawStudioQolBar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				m_studioAutoKey = GUILayout.Toggle(m_studioAutoKey, "Auto Key", EditorStyles.toolbarButton, GUILayout.Width(58f));
				m_studioSnapFrames = GUILayout.Toggle(m_studioSnapFrames, "Snap", EditorStyles.toolbarButton, GUILayout.Width(42f));
				m_studioShowFrames = GUILayout.Toggle(m_studioShowFrames, "Frames", EditorStyles.toolbarButton, GUILayout.Width(48f));
				bool isolate = GUILayout.Toggle(m_studioIsolateSelection, "Isolate", EditorStyles.toolbarButton, GUILayout.Width(46f));
				if (isolate != m_studioIsolateSelection) { m_studioIsolateSelection = isolate; SampleCurrentClip(); Repaint(); }
				bool wholeRig = GUILayout.Toggle(m_studioWholeRig, "Whole Rig", EditorStyles.toolbarButton, GUILayout.Width(58f));
				if (wholeRig != m_studioWholeRig) { m_studioWholeRig = wholeRig; if (wholeRig) SelectStudioWholeRig(); Repaint(); }
				m_studioPairSetup = GUILayout.Toggle(m_studioPairSetup, "Pair", EditorStyles.toolbarButton, GUILayout.Width(50f));
			}
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("|<", EditorStyles.toolbarButton, GUILayout.Width(28f))) SetStudioTime(0f);
				if (GUILayout.Button("Prev Key", EditorStyles.toolbarButton, GUILayout.Width(58f))) JumpStudioKey(-1);
				if (GUILayout.Button("Next Key", EditorStyles.toolbarButton, GUILayout.Width(58f))) JumpStudioKey(1);
				if (GUILayout.Button(">|", EditorStyles.toolbarButton, GUILayout.Width(28f))) SetStudioTime(StudioDuration());
			}
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				if (GUILayout.Button("Copy Pose", EditorStyles.toolbarButton, GUILayout.Width(67f))) CopyStudioPose();
				using (new EditorGUI.DisabledScope(m_studioPoseClipboard.Count == 0)) if (GUILayout.Button("Paste + Key", EditorStyles.toolbarButton, GUILayout.Width(76f))) PasteStudioPose();
				GUILayout.FlexibleSpace();
				using (new EditorGUI.DisabledScope(m_studioSelectedNumericKey == null))
				{
					if (GUILayout.Button("Duplicate Key", EditorStyles.toolbarButton, GUILayout.Width(83f))) DuplicateStudioSelectedKey();
					if (GUILayout.Button("Delete Key", EditorStyles.toolbarButton, GUILayout.Width(70f))) DeleteStudioSelectedKey();
				}
			}
		}

		private void DrawStudioPairSetup()
		{
			if (m_studioPlayerAnimation == null && !string.IsNullOrEmpty(m_studioPlayerAnimationPath) && System.IO.File.Exists(m_studioPlayerAnimationPath))
				LoadStudioPlayerAnimation(m_studioPlayerAnimationPath);
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				EditorGUILayout.LabelField("PAIRED PLAYER + ENEMY ANIMATION", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Animate the enemy clip here, create/select its player partner, then link both to a downed-finisher phase. They share this playhead and duration.", EditorStyles.wordWrappedMiniLabel);
				List<string> files = FindStudioPlayerAnimations();
				int selected = Mathf.Max(0, files.FindIndex(i_path => string.Equals(i_path, m_studioPlayerAnimationPath, StringComparison.OrdinalIgnoreCase)) + 1);
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.PrefixLabel("Player partner");
					if (GUILayout.Button("<", GUILayout.Width(24f))) { int next = Mathf.Max(0, selected - 1); LoadStudioPlayerAnimation(next == 0 ? null : files[next - 1]); SampleCurrentClip(); }
					EditorGUILayout.LabelField(selected == 0 ? "None selected" : System.IO.Path.GetFileNameWithoutExtension(files[selected - 1]), EditorStyles.textField);
					if (GUILayout.Button(">", GUILayout.Width(24f))) { int next = Mathf.Min(files.Count, selected + 1); LoadStudioPlayerAnimation(next == 0 ? null : files[next - 1]); SampleCurrentClip(); }
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					m_studioPairName = EditorGUILayout.TextField("New partner", m_studioPairName);
					if (GUILayout.Button("Create", GUILayout.Width(65f))) CreateStudioPlayerPartner();
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Edit actor", GUILayout.Width(68f));
					if (GUILayout.Toggle(!m_studioEditPlayer, "Enemy", EditorStyles.miniButtonLeft) && m_studioEditPlayer) SwitchStudioActor(false);
					using (new EditorGUI.DisabledScope(m_studioPlayerAnimation == null))
						if (GUILayout.Toggle(m_studioEditPlayer, "Player partner", EditorStyles.miniButtonRight) && !m_studioEditPlayer) SwitchStudioActor(true);
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Facing", GUILayout.Width(68f));
					bool enemyLeft = GUILayout.Toggle(m_studioEnemyFacingLeft, m_studioEnemyFacingLeft ? "Enemy: Left" : "Enemy: Right", EditorStyles.miniButtonLeft);
					bool playerLeft = GUILayout.Toggle(m_studioPlayerFacingLeft, m_studioPlayerFacingLeft ? "Player: Left" : "Player: Right", EditorStyles.miniButtonRight);
					if (enemyLeft != m_studioEnemyFacingLeft || playerLeft != m_studioPlayerFacingLeft)
					{
						m_studioEnemyFacingLeft = enemyLeft; m_studioPlayerFacingLeft = playerLeft; SampleCurrentClip(); Repaint();
					}
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					m_studioPairPhase = EditorGUILayout.TextField("Finisher phase", m_studioPairPhase);
					using (new EditorGUI.DisabledScope(m_studioPlayerAnimation == null))
						if (GUILayout.Button("Link Pair", GUILayout.Width(75f))) LinkStudioPair();
					bool preview = GUILayout.Toggle(m_studioPairPreview, "Preview together", GUILayout.Width(112f));
					if (preview != m_studioPairPreview) { m_studioPairPreview = preview; InvalidateCameraFrame(); SampleCurrentClip(); Repaint(); }
				}
				if (!string.IsNullOrEmpty(m_studioPairStatus)) EditorGUILayout.LabelField(m_studioPairStatus, EditorStyles.wordWrappedMiniLabel);
			}
		}

		private List<string> FindStudioPlayerAnimations()
		{
			if (string.IsNullOrEmpty(m_modPackRoot) || !System.IO.Directory.Exists(m_modPackRoot)) return new List<string>();
			return System.IO.Directory.GetFiles(m_modPackRoot, "*.json", System.IO.SearchOption.AllDirectories).Where(i_path =>
			{
				try { return string.Equals(JObject.Parse(System.IO.File.ReadAllText(i_path))["type"]?.Value<string>(), "playerAnimation", StringComparison.Ordinal); }
				catch (Exception) { return false; }
			}).OrderBy(i_path => i_path, StringComparer.OrdinalIgnoreCase).ToList();
		}

		private void LoadStudioPlayerAnimation(string i_path)
		{
			m_studioPlayerAnimationPath = i_path ?? string.Empty; m_studioPlayerAnimation = null;
			if (string.IsNullOrEmpty(i_path)) { m_studioPairStatus = string.Empty; return; }
			try
			{
				m_studioPlayerJson = System.IO.File.ReadAllText(i_path);
				m_studioPlayerAnimation = Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedPlayerAnimationDocument>(m_studioPlayerJson);
				ValidateStudioPlayerDocument();
				m_studioPairStatus = "Loaded player partner: " + m_studioPlayerAnimation.DisplayName;
			}
			catch (Exception exception) { m_studioPairStatus = "Could not load player partner: " + exception.Message; }
		}

		private void SwitchStudioActor(bool i_player)
		{
			m_studioEditPlayer = i_player; m_studioBone = string.Empty; if (m_studioSelectedBones == null) m_studioSelectedBones = new List<string>(); else m_studioSelectedBones.Clear(); m_studioWholeRig = false; m_studioSelectedNumericTrack = null; m_studioSelectedNumericKey = null;
			if (i_player) { m_studioPairPreview = true; SampleCurrentClip(); }
			Repaint();
		}

		private bool StudioBoneSelected(string i_bone)
		{
			if (m_studioSelectedBones == null) m_studioSelectedBones = new List<string>();
			return m_studioWholeRig || string.Equals(m_studioBone, i_bone, StringComparison.Ordinal) || m_studioSelectedBones.Contains(i_bone);
		}

		private void SelectStudioBone(string i_bone, bool i_additive)
		{
			if (string.IsNullOrEmpty(i_bone)) return;
			if (m_studioSelectedBones == null) m_studioSelectedBones = new List<string>();
			m_studioWholeRig = false;
			if (!i_additive) m_studioSelectedBones.Clear();
			if (i_additive && m_studioSelectedBones.Contains(i_bone)) m_studioSelectedBones.Remove(i_bone);
			else if (!m_studioSelectedBones.Contains(i_bone)) m_studioSelectedBones.Add(i_bone);
			m_studioBone = m_studioSelectedBones.Count == 0 ? string.Empty : i_bone;
		}

		private void SelectStudioWholeRig()
		{
			m_studioSelectedBones = StudioActiveBones().Keys.OrderBy(i_name => i_name, StringComparer.Ordinal).ToList();
			m_studioBone = m_studioSelectedBones.FirstOrDefault() ?? string.Empty;
		}

		private List<string> StudioTransformSelection()
		{
			if (m_studioSelectedBones == null) m_studioSelectedBones = new List<string>();
			Dictionary<string, Transform> bones = StudioActiveBones();
			if (m_studioWholeRig) return bones.Keys.ToList();
			List<string> selected = m_studioSelectedBones.Where(bones.ContainsKey).Distinct(StringComparer.Ordinal).ToList();
			if (selected.Count == 0 && !string.IsNullOrEmpty(m_studioBone) && bones.ContainsKey(m_studioBone)) selected.Add(m_studioBone);
			return selected;
		}

		private List<string> StudioManipulationRoots()
		{
			Dictionary<string, Transform> bones = StudioActiveBones(); List<string> selected = StudioTransformSelection(); HashSet<Transform> selectedTransforms = new HashSet<Transform>(selected.Where(bones.ContainsKey).Select(i_name => bones[i_name]));
			return selected.Where(i_name => bones.TryGetValue(i_name, out Transform bone) && !HasStudioSelectedAncestor(bone, selectedTransforms)).ToList();
		}

		private static bool HasStudioSelectedAncestor(Transform i_bone, HashSet<Transform> i_selected)
		{
			Transform parent = i_bone.parent; while (parent != null) { if (i_selected.Contains(parent)) return true; parent = parent.parent; } return false;
		}

		private void CreateStudioPlayerPartner()
		{
			try
			{
				string manifest = System.IO.Path.Combine(m_modPackRoot, "manifest.json");
				string pack = JObject.Parse(System.IO.File.ReadAllText(manifest))["id"]?.Value<string>();
				if (!ContentId.IsValidNamespace(pack)) throw new InvalidOperationException("The mod manifest has an invalid ID.");
				string slug = SlugForFile(m_studioPairName); float duration = Mathf.Max(0.05f, m_normalizedEnemyClip.DurationSeconds);
				NormalizedPlayerAnimationDocument document = new NormalizedPlayerAnimationDocument
				{
					SchemaVersion = 1, Type = "playerAnimation", Id = pack + ":player-animation/" + slug, Rig = "core:player-rig/alex",
					DisplayName = string.IsNullOrWhiteSpace(m_studioPairName) ? "Paired Player Animation" : m_studioPairName.Trim(),
					DurationSeconds = duration, FrameRate = Mathf.Max(1f, m_normalizedEnemyClip.FrameRate), Loop = m_normalizedEnemyClip.Loop,
					Tracks = new List<NormalizedNumericTrack> { new NormalizedNumericTrack { Target = "bone/hips", Property = "position.x", Keys = new List<NormalizedNumericKey> { new NormalizedNumericKey { Time = 0f, Value = 0f }, new NormalizedNumericKey { Time = duration, Value = 0f } } } }
				};
				string json = Newtonsoft.Json.JsonConvert.SerializeObject(document, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
				string destination = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(m_normalizedEnemyPath), slug + ".player-animation.json");
				if (System.IO.File.Exists(destination)) throw new System.IO.IOException("That player animation file already exists.");
				NormalizedPlayerAnimationLoadResult result = NormalizedPlayerAnimationParser.Parse(json, pack, destination, m_modPackRoot);
				if (!result.Report.IsValid) throw new InvalidOperationException(string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				System.IO.File.WriteAllText(destination, json); AssetDatabase.Refresh(); LoadStudioPlayerAnimation(destination); m_studioPairPreview = true; SampleCurrentClip();
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Create paired animation", exception.Message, "OK"); }
		}

		private void LinkStudioPair()
		{
			try
			{
				if (m_studioPlayerAnimation == null) throw new InvalidOperationException("Select or create a player partner first.");
				JObject enemy = JObject.Parse(System.IO.File.ReadAllText(m_modEnemyPath));
				JObject refs = enemy["animationRefs"] as JObject;
				string enemySemantic = refs?.Properties().FirstOrDefault(i_property => string.Equals(i_property.Value.Value<string>(), m_normalizedEnemyClip.Id, StringComparison.Ordinal))?.Name;
				if (string.IsNullOrEmpty(enemySemantic)) throw new InvalidOperationException("The selected enemy animation is not linked in animationRefs.");
				JObject behavior = enemy["behavior"] as JObject ?? new JObject(); enemy["behavior"] = behavior;
				JArray modules = behavior["modules"] as JArray ?? new JArray(); behavior["modules"] = modules;
				JObject finisher = modules.OfType<JObject>().FirstOrDefault(i_module => i_module["type"]?.Value<string>() == "downedFinisher");
				if (finisher == null)
				{
					finisher = new JObject { ["type"] = "downedFinisher", ["triggerRange"] = 1.5f, ["durationSeconds"] = m_studioPlayerAnimation.DurationSeconds, ["meterMax"] = 100f, ["inputPower"] = 14f, ["animation"] = enemySemantic, ["playerAnimationRef"] = m_studioPlayerAnimation.Id };
					modules.Add(finisher);
				}
				else if (finisher["phases"] is JArray phases && phases.Count > 0)
				{
					string phaseId = string.IsNullOrWhiteSpace(m_studioPairPhase) ? "paired" : SlugForFile(m_studioPairPhase);
					JObject phase = phases.OfType<JObject>().FirstOrDefault(i_item => i_item["id"]?.Value<string>() == phaseId);
					if (phase == null) { if (phases.Count >= 8) throw new InvalidOperationException("This finisher already has the maximum eight phases."); phase = new JObject { ["id"] = phaseId }; phases.Add(phase); }
					phase["durationSeconds"] = m_studioPlayerAnimation.DurationSeconds; phase["animation"] = enemySemantic; phase["playerAnimationRef"] = m_studioPlayerAnimation.Id;
				}
				else { finisher["durationSeconds"] = m_studioPlayerAnimation.DurationSeconds; finisher["animation"] = enemySemantic; finisher["playerAnimationRef"] = m_studioPlayerAnimation.Id; }
				string json = enemy.ToString(Newtonsoft.Json.Formatting.Indented);
				string pack = JObject.Parse(System.IO.File.ReadAllText(System.IO.Path.Combine(m_modPackRoot, "manifest.json")))["id"]?.Value<string>();
				EnemyDefinitionLoadResult validation = EnemyDefinitionParser.Parse(json, pack, m_modEnemyPath);
				if (!validation.Report.IsValid) throw new InvalidOperationException(string.Join("\n", validation.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				System.IO.File.WriteAllText(m_modEnemyPath, json); AssetDatabase.Refresh();
				m_studioPairStatus = "Linked " + enemySemantic + " + " + m_studioPlayerAnimation.Id + " to the finisher.";
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Link paired animation", exception.Message, "OK"); }
		}

		private void SampleStudioPairedPlayer(float i_time)
		{
			if (m_playerInstance == null) return;
			bool visible = m_mode == 3 && m_designStudio && m_studioPairPreview && m_studioPlayerAnimation != null;
			m_playerInstance.SetActive(visible);
			if (!visible) return;
			float time = Mathf.Clamp(i_time, 0f, m_studioPlayerAnimation.DurationSeconds);
			foreach (NormalizedNumericTrack track in m_studioPlayerAnimation.Tracks ?? new List<NormalizedNumericTrack>())
			{
				if (track == null || track.Keys == null || track.Keys.Count == 0) continue;
				bool spriteTrack = track.Target.StartsWith("sprite/", StringComparison.Ordinal); bool boneTrack = track.Target.StartsWith("bone/", StringComparison.Ordinal);
				if (!spriteTrack && !boneTrack) continue;
				Transform bone = FindStudioPlayerBone(track.Target.Substring(track.Target.IndexOf('/') + 1));
				if (bone == null) continue;
				float value = EvaluateNormalizedKeys(track.Keys, time);
				if (spriteTrack)
				{
					BonePlayer playerBone = bone.GetComponent<BonePlayer>(); SpriteRenderer renderer = playerBone?.GetBodyPart() == null ? null : playerBone.GetBodyPart().GetComponent<SpriteRenderer>();
					if (renderer == null) continue; Color color = renderer.color;
					if (track.Property == "sortingOrder") renderer.sortingOrder = Mathf.RoundToInt(value); else if (track.Property == "color.r") color.r = value; else if (track.Property == "color.g") color.g = value; else if (track.Property == "color.b") color.b = value; else if (track.Property == "color.a") color.a = value;
					renderer.color = color; continue;
				}
				Vector3 local = bone.localPosition, scale = bone.localScale, rotation = bone.localEulerAngles;
				if (track.Property == "position.x") local.x = value; else if (track.Property == "position.y") local.y = value; else if (track.Property == "position.z") local.z = value;
				else if (track.Property == "rotation.x") rotation.x = value; else if (track.Property == "rotation.y") rotation.y = value; else if (track.Property == "rotation.z") rotation.z = value;
				else if (track.Property == "scale.x") scale.x = value; else if (track.Property == "scale.y") scale.y = value; else if (track.Property == "scale.z") scale.z = value;
				bone.localPosition = local; bone.localEulerAngles = rotation; bone.localScale = scale;
			}
		}

		private Transform FindStudioPlayerBone(string i_name)
		{
			BoneTypePlayer type;
			switch (i_name)
			{
				case "hips": type = BoneTypePlayer.Hips; break; case "butt": type = BoneTypePlayer.Butt; break; case "spine": type = BoneTypePlayer.Spine; break;
				case "chest": type = BoneTypePlayer.Chest; break; case "neck": type = BoneTypePlayer.Neck; break; case "head": type = BoneTypePlayer.Head; break;
				case "arm-right-upper": type = BoneTypePlayer.rArmUpper; break; case "arm-right-lower": type = BoneTypePlayer.rArmLower; break; case "hand-right": type = BoneTypePlayer.rHand; break;
				case "arm-left-upper": type = BoneTypePlayer.lArmUpper; break; case "arm-left-lower": type = BoneTypePlayer.lArmLower; break; case "hand-left": type = BoneTypePlayer.lHand; break;
				case "leg-right-upper": type = BoneTypePlayer.rLegUpper; break; case "leg-right-lower": type = BoneTypePlayer.rLegLower; break; case "foot-right": type = BoneTypePlayer.rFoot; break;
				case "leg-left-upper": type = BoneTypePlayer.lLegUpper; break; case "leg-left-lower": type = BoneTypePlayer.lLegLower; break; case "foot-left": type = BoneTypePlayer.lFoot; break;
				case "ear": type = BoneTypePlayer.Ear; break; case "face": type = BoneTypePlayer.Face; break; default: return null;
			}
			return FindPlayerBone(type)?.transform;
		}

		private void HandleStudioShortcuts()
		{
			if (Event.current.type != EventType.KeyDown || EditorGUIUtility.editingTextField) return;
			if (Event.current.keyCode == KeyCode.G) m_studioTransformTool = 0;
			else if (Event.current.keyCode == KeyCode.R) m_studioTransformTool = 1;
			else if (Event.current.keyCode == KeyCode.S) m_studioTransformTool = 2;
			else if (Event.current.keyCode == KeyCode.Space) m_playing = !m_playing;
			else if (Event.current.keyCode == KeyCode.LeftArrow) StepFrame(-1);
			else if (Event.current.keyCode == KeyCode.RightArrow) StepFrame(1);
			else if (Event.current.keyCode == KeyCode.UpArrow) JumpStudioKey(-1);
			else if (Event.current.keyCode == KeyCode.DownArrow) JumpStudioKey(1);
			else if (Event.current.keyCode == KeyCode.Delete && m_studioSelectedNumericKey != null) DeleteStudioSelectedKey();
			else if (Event.current.keyCode == KeyCode.D && Event.current.control && m_studioSelectedNumericKey != null) DuplicateStudioSelectedKey();
			else if (Event.current.keyCode == KeyCode.I && StudioTransformSelection().Count > 0) InsertStudioTransformKeysForSelection();
			else return;
			Event.current.Use(); Repaint();
		}

		private void DrawStudioHeader()
		{
			if (position.width < 650f)
			{
				using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
				{
					GUILayout.Label("ANIMATION DESIGN STUDIO", EditorStyles.miniBoldLabel);
					if (GUILayout.Button(m_playing ? "Pause" : "Play", EditorStyles.toolbarButton, GUILayout.Width(45f))) m_playing = !m_playing;
					if (GUILayout.Button("Restart", EditorStyles.toolbarButton, GUILayout.Width(48f))) { m_playing = false; m_time = 0f; SampleCurrentClip(); }
					m_loop = GUILayout.Toggle(m_loop, "Loop", EditorStyles.toolbarButton, GUILayout.Width(40f));
				}
				using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
				{
					if (GUILayout.Toggle(m_studioTransformTool == 0, "G Move", EditorStyles.toolbarButton)) m_studioTransformTool = 0;
					if (GUILayout.Toggle(m_studioTransformTool == 1, "R Rotate", EditorStyles.toolbarButton)) m_studioTransformTool = 1;
					if (GUILayout.Toggle(m_studioTransformTool == 2, "S Scale", EditorStyles.toolbarButton)) m_studioTransformTool = 2;
					GUILayout.Label(StudioTimeLabel(m_time), EditorStyles.miniLabel, GUILayout.Width(48f));
					string compactValidation = m_studioEditPlayer ? m_studioPlayerValidation : m_authoringValidation;
					using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(compactValidation))) if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(42f))) SaveStudioActiveDocument();
				}
				return;
			}
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.Label("ANIMATION DESIGN STUDIO", EditorStyles.miniBoldLabel, GUILayout.Width(155f));
				if (GUILayout.Button(m_playing ? "Pause" : "Play", EditorStyles.toolbarButton, GUILayout.Width(48f))) m_playing = !m_playing;
				if (GUILayout.Button("Restart", EditorStyles.toolbarButton, GUILayout.Width(50f))) { m_playing = false; m_time = 0f; SampleCurrentClip(); }
				if (GUILayout.Button("<", EditorStyles.toolbarButton, GUILayout.Width(24f))) StepFrame(-1);
				if (GUILayout.Button(">", EditorStyles.toolbarButton, GUILayout.Width(24f))) StepFrame(1);
				m_loop = GUILayout.Toggle(m_loop, "Loop", EditorStyles.toolbarButton, GUILayout.Width(45f));
				GUILayout.Space(8f);
				if (GUILayout.Toggle(m_studioTransformTool == 0, "G Move", EditorStyles.toolbarButton, GUILayout.Width(50f))) m_studioTransformTool = 0;
				if (GUILayout.Toggle(m_studioTransformTool == 1, "R Rotate", EditorStyles.toolbarButton, GUILayout.Width(55f))) m_studioTransformTool = 1;
				if (GUILayout.Toggle(m_studioTransformTool == 2, "S Scale", EditorStyles.toolbarButton, GUILayout.Width(52f))) m_studioTransformTool = 2;
				GUILayout.FlexibleSpace();
				if (position.width > 930f) GUILayout.Label("G/R/S tools  I keyframe", EditorStyles.miniLabel, GUILayout.Width(125f));
				GUILayout.Label(StudioTimeLabel(m_time), EditorStyles.miniLabel, GUILayout.Width(58f));
				string validation = m_studioEditPlayer ? m_studioPlayerValidation : m_authoringValidation;
				using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(validation)))
					if (GUILayout.Button("Save JSON", EditorStyles.toolbarButton, GUILayout.Width(68f))) SaveStudioActiveDocument();
			}
		}

		private Dictionary<string, Transform> StudioActiveBones()
		{
			if (!m_studioEditPlayer) return m_normalizedEnemyBones;
			Dictionary<string, Transform> result = new Dictionary<string, Transform>(StringComparer.Ordinal);
			foreach (string name in new[] { "hips", "butt", "spine", "chest", "neck", "head", "arm-right-upper", "arm-right-lower", "hand-right", "arm-left-upper", "arm-left-lower", "hand-left", "leg-right-upper", "leg-right-lower", "foot-right", "leg-left-upper", "leg-left-lower", "foot-left", "ear", "face" })
			{
				Transform bone = FindStudioPlayerBone(name); if (bone != null) result[name] = bone;
			}
			return result;
		}

		private List<NormalizedNumericTrack> StudioNumericTracks()
		{
			if (m_studioEditPlayer) return m_studioPlayerAnimation?.Tracks ?? new List<NormalizedNumericTrack>();
			return m_normalizedEnemyClip.Tracks ?? (m_normalizedEnemyClip.Tracks = new List<NormalizedNumericTrack>());
		}

		private List<NormalizedObjectTrack> StudioObjectTracks()
		{
			if (m_studioEditPlayer) return m_studioPlayerAnimation?.ObjectTracks ?? new List<NormalizedObjectTrack>();
			return m_normalizedEnemyClip.ObjectTracks ?? (m_normalizedEnemyClip.ObjectTracks = new List<NormalizedObjectTrack>());
		}

		private float StudioFrameRate() { return Mathf.Max(1f, m_studioEditPlayer ? m_studioPlayerAnimation?.FrameRate ?? 60f : m_normalizedEnemyClip.FrameRate); }
		private float StudioDuration() { return Mathf.Max(0.0001f, m_studioEditPlayer ? m_studioPlayerAnimation?.DurationSeconds ?? 1f : m_normalizedEnemyClip.DurationSeconds); }

		private void RecordStudioChange(string i_label, Action i_change)
		{
			if (!m_studioEditPlayer) { RecordAuthoringChange(i_label, i_change); return; }
			Undo.RecordObject(this, i_label); i_change(); SyncStudioPlayerJson(); ValidateStudioPlayerDocument(); EditorUtility.SetDirty(this); SampleCurrentClip(); Repaint();
		}

		private void SyncStudioPlayerJson()
		{
			if (m_studioPlayerAnimation != null) m_studioPlayerJson = Newtonsoft.Json.JsonConvert.SerializeObject(m_studioPlayerAnimation, Newtonsoft.Json.Formatting.Indented, new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
		}

		private void ValidateStudioPlayerDocument()
		{
			if (m_studioPlayerAnimation == null) { m_studioPlayerValidation = "No player partner is loaded."; return; }
			SyncStudioPlayerJson();
			string pack = ContentId.TryParse(m_studioPlayerAnimation.Id, out ContentId id) ? id.Namespace : "invalid";
			NormalizedPlayerAnimationLoadResult result = NormalizedPlayerAnimationParser.Parse(m_studioPlayerJson, pack, m_studioPlayerAnimationPath, m_modPackRoot);
			m_studioPlayerValidation = result.Report.IsValid ? string.Empty : string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		private void SaveStudioActiveDocument()
		{
			if (!m_studioEditPlayer) { SaveAnimationToMod(); return; }
			try
			{
				ValidateStudioPlayerDocument(); if (!string.IsNullOrEmpty(m_studioPlayerValidation)) throw new InvalidOperationException(m_studioPlayerValidation);
				System.IO.File.WriteAllText(m_studioPlayerAnimationPath, m_studioPlayerJson); AssetDatabase.Refresh(); m_studioPairStatus = "Saved player partner: " + m_studioPlayerAnimationPath;
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Save player animation", exception.Message, "OK"); }
		}

		private static void DrawStudioPanel(Rect i_rect, string i_title)
		{
			EditorGUI.DrawRect(i_rect, new Color(0.115f, 0.115f, 0.115f, 1f));
			EditorGUI.DrawRect(new Rect(i_rect.x, i_rect.y, i_rect.width, 21f), new Color(0.16f, 0.16f, 0.16f, 1f));
			GUI.Label(new Rect(i_rect.x + 6f, i_rect.y + 2f, i_rect.width - 12f, 18f), i_title, EditorStyles.miniBoldLabel);
		}

		private void DrawStudioOutliner(Rect i_rect)
		{
			DrawStudioPanel(i_rect, "OUTLINER");
			Rect content = new Rect(i_rect.x + 3f, i_rect.y + 24f, i_rect.width - 6f, i_rect.height - 27f);
			GUILayout.BeginArea(content);
			m_studioBoneSearch = EditorGUILayout.TextField(m_studioBoneSearch, EditorStyles.toolbarSearchField);
			EditorGUILayout.LabelField("Ctrl/Shift-click for multi-select", EditorStyles.miniLabel);
			m_studioOutlinerScroll = EditorGUILayout.BeginScrollView(m_studioOutlinerScroll);
			Dictionary<string, Transform> activeBones = StudioActiveBones();
			if (activeBones.Count > 0)
			{
				foreach (KeyValuePair<string, Transform> bonePair in activeBones.OrderBy(i_pair => StudioHierarchyDepth(i_pair.Value)).ThenBy(i_pair => i_pair.Key, StringComparer.Ordinal))
				{
					string bone = bonePair.Key; Transform transformValue = bonePair.Value;
					if (!string.IsNullOrWhiteSpace(m_studioBoneSearch) && bone.IndexOf(m_studioBoneSearch, StringComparison.OrdinalIgnoreCase) < 0) continue;
					int depth = 0; Transform parent = transformValue.parent;
					while (parent != null && (m_enemyInstance == null || parent != m_enemyInstance.transform)) { depth++; parent = parent.parent; }
					using (new EditorGUILayout.HorizontalScope())
					{
						GUILayout.Space(Mathf.Min(36f, depth * 10f));
						bool selected = StudioBoneSelected(bone);
						bool next = GUILayout.Toggle(selected, bone + "  [" + StudioSortingOrder(bone) + "]", "Button");
						if (next != selected)
						{
							SelectStudioBone(bone, Event.current.control || Event.current.shift); Repaint();
						}
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox("No rig bones are loaded for this actor.", MessageType.Warning);
				if (GUILayout.Button("Reload actor preview")) SelectCurrentClip();
			}
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private static int StudioHierarchyDepth(Transform i_transform)
		{
			int depth = 0; while (i_transform != null && i_transform.parent != null) { depth++; i_transform = i_transform.parent; } return depth;
		}

		private void DrawStudioViewport(Rect i_rect)
		{
			DrawStudioPanel(i_rect, "VIEWPORT");
			Rect preview = new Rect(i_rect.x + 2f, i_rect.y + 23f, i_rect.width - 4f, i_rect.height - 25f);
			ApplyStudioIsolation();
			DrawPreview(preview);
			GUI.Label(new Rect(preview.x + 6f, preview.y + 5f, 180f, 18f), Mathf.RoundToInt(m_studioViewportZoom * 100f) + "%  Wheel: zoom  MMB: pan", EditorStyles.miniLabel);
			if (GUI.Button(new Rect(preview.xMax - 48f, preview.y + 4f, 42f, 18f), "Frame", EditorStyles.miniButton))
			{
				m_studioViewportZoom = 1f; m_studioViewportPan = Vector2.zero; Repaint();
			}
			if (Event.current.type == EventType.Repaint)
			{
				Handles.BeginGUI();
				Handles.color = new Color(1f, 1f, 1f, 0.045f);
				for (float x = preview.x; x < preview.xMax; x += 32f) Handles.DrawLine(new Vector3(x, preview.y), new Vector3(x, preview.yMax));
				for (float y = preview.y; y < preview.yMax; y += 32f) Handles.DrawLine(new Vector3(preview.x, y), new Vector3(preview.xMax, y));
				foreach (KeyValuePair<string, Transform> pair in StudioActiveBones().OrderBy(i_pair => StudioSortingOrder(i_pair.Key)))
				{
					Vector2 point = WorldToGui(preview, pair.Value.position);
					bool selectedBone = StudioBoneSelected(pair.Key);
					Handles.color = selectedBone ? new Color(1f, 0.55f, 0.1f, 1f) : new Color(0.3f, 0.85f, 1f, 0.8f);
					Handles.DrawSolidDisc(point, Vector3.forward, selectedBone ? 8f : 5f);
				}
				if (!string.IsNullOrEmpty(m_studioBone) && StudioActiveBones().TryGetValue(m_studioBone, out Transform selected))
				{
					Vector2 point = WorldToGui(preview, selected.position);
					Handles.color = new Color(1f, 0.25f, 0.2f, 0.95f); Handles.DrawAAPolyLine(2f, point, point + Vector2.right * 30f);
					Handles.color = new Color(0.3f, 1f, 0.35f, 0.95f); Handles.DrawAAPolyLine(2f, point, point + Vector2.up * -30f);
				}
				Handles.EndGUI();
			}
			if (preview.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
			{
				m_studioViewportZoom = Mathf.Clamp(m_studioViewportZoom * Mathf.Pow(1.12f, -Event.current.delta.y), 0.4f, 8f);
				Repaint(); Event.current.Use();
			}
			if (preview.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDrag && Event.current.button == 2)
			{
				float worldPerPixel = 2f * m_preview.camera.orthographicSize / Mathf.Max(1f, preview.height);
				m_studioViewportPan += new Vector2(-Event.current.delta.x * worldPerPixel, Event.current.delta.y * worldPerPixel);
				Repaint(); Event.current.Use();
			}
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && preview.Contains(Event.current.mousePosition))
			{
				Dictionary<string, Transform> activeBones = StudioActiveBones();
				List<string> candidates = activeBones
					.Where(i_pair => Vector2.Distance(Event.current.mousePosition, WorldToGui(preview, i_pair.Value.position)) <= 18f)
					.OrderByDescending(i_pair => StudioSortingOrder(i_pair.Key)).ThenBy(i_pair => i_pair.Key, StringComparer.Ordinal)
					.Select(i_pair => i_pair.Key).ToList();
				if (Vector2.Distance(m_studioLastPickPosition, Event.current.mousePosition) <= 5f && candidates.SequenceEqual(m_studioPickCycle))
					m_studioPickCycleIndex = candidates.Count == 0 ? 0 : (m_studioPickCycleIndex + 1) % candidates.Count;
				else { m_studioPickCycle.Clear(); m_studioPickCycle.AddRange(candidates); m_studioPickCycleIndex = 0; }
				m_studioLastPickPosition = Event.current.mousePosition;
				string closest = candidates.Count == 0 ? null : candidates[m_studioPickCycleIndex];
				if (!string.IsNullOrEmpty(closest))
				{
					SelectStudioBone(closest, Event.current.control || Event.current.shift);
					Transform selected = activeBones[closest];
					m_studioViewportDragging = true; m_studioViewportMoved = false; m_studioDragMouse = Event.current.mousePosition; m_studioDragPosition = selected.localPosition;
					m_studioDragScale = selected.localScale; m_studioDragRotation = Mathf.DeltaAngle(0f, selected.localEulerAngles.z);
					m_studioDragStarts.Clear();
					foreach (string boneName in StudioManipulationRoots()) if (activeBones.TryGetValue(boneName, out Transform selectedBone))
						m_studioDragStarts[boneName] = new StudioTransformStart { Position = selectedBone.localPosition, Rotation = Mathf.DeltaAngle(0f, selectedBone.localEulerAngles.z), Scale = selectedBone.localScale };
					Repaint(); Event.current.Use();
				}
			}
			if (m_studioViewportDragging && !string.IsNullOrEmpty(m_studioBone) && StudioActiveBones().TryGetValue(m_studioBone, out Transform dragged))
			{
				if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
				{
					Vector2 delta = Event.current.mousePosition - m_studioDragMouse;
					if (Event.current.shift) delta *= 0.1f;
					m_studioViewportMoved |= delta.sqrMagnitude > 1f;
					foreach (KeyValuePair<string, StudioTransformStart> start in m_studioDragStarts)
					{
						if (!StudioActiveBones().TryGetValue(start.Key, out Transform selectedBone)) continue;
						if (m_studioTransformTool == 0)
						{
							float worldPerPixel = 2f * m_preview.camera.orthographicSize / Mathf.Max(1f, preview.height);
							selectedBone.localPosition = start.Value.Position + new Vector3(delta.x * worldPerPixel, -delta.y * worldPerPixel, 0f);
						}
						else if (m_studioTransformTool == 1) selectedBone.localRotation = Quaternion.Euler(0f, 0f, start.Value.Rotation - delta.x * 0.5f);
						else selectedBone.localScale = new Vector3(Mathf.Max(0.01f, start.Value.Scale.x + delta.x * 0.01f), Mathf.Max(0.01f, start.Value.Scale.y - delta.y * 0.01f), start.Value.Scale.z);
					}
					Repaint(); Event.current.Use();
				}
				else if (Event.current.rawType == EventType.MouseUp)
				{
					m_studioViewportDragging = false;
					if (m_studioViewportMoved && m_studioAutoKey) InsertStudioTransformKeysForSelection();
					m_studioDragStarts.Clear();
				}
			}
		}

		private int StudioSortingOrder(string i_bone)
		{
			if (m_studioEditPlayer)
			{
				Transform bone = FindStudioPlayerBone(i_bone); BonePlayer playerBone = bone == null ? null : bone.GetComponent<BonePlayer>();
				SpriteRenderer renderer = playerBone?.GetBodyPart() == null ? null : playerBone.GetBodyPart().GetComponent<SpriteRenderer>();
				return renderer == null ? 0 : renderer.sortingOrder;
			}
			return m_normalizedEnemyRenderers.TryGetValue(i_bone, out List<SpriteRenderer> renderers) && renderers.Count > 0
				? renderers.Max(i_renderer => i_renderer.sortingOrder) : int.MinValue;
		}

		private void ApplyStudioIsolation()
		{
			if (!m_studioIsolateSelection || string.IsNullOrEmpty(m_studioBone)) return;
			if (m_studioEditPlayer)
			{
				foreach (KeyValuePair<string, Transform> pair in StudioActiveBones())
				{
					BonePlayer bone = pair.Value.GetComponent<BonePlayer>(); SpriteRenderer renderer = bone?.GetBodyPart() == null ? null : bone.GetBodyPart().GetComponent<SpriteRenderer>();
					if (renderer != null) renderer.enabled = StudioBoneSelected(pair.Key);
				}
				return;
			}
			foreach (KeyValuePair<string, List<SpriteRenderer>> pair in m_normalizedEnemyRenderers)
				foreach (SpriteRenderer renderer in pair.Value) renderer.enabled = StudioBoneSelected(pair.Key);
		}

		private void DrawStudioInspector(Rect i_rect)
		{
			DrawStudioPanel(i_rect, "PROPERTIES");
			Rect content = new Rect(i_rect.x + 5f, i_rect.y + 25f, i_rect.width - 10f, i_rect.height - 30f);
			GUILayout.BeginArea(content);
			m_studioInspectorScroll = EditorGUILayout.BeginScrollView(m_studioInspectorScroll);
			if (string.IsNullOrEmpty(m_studioBone) || !StudioActiveBones().TryGetValue(m_studioBone, out Transform bone))
			{
				EditorGUILayout.HelpBox("Select a bone in the Outliner or viewport.", MessageType.Info);
			}
			else
			{
				EditorGUILayout.LabelField(m_studioBone, EditorStyles.boldLabel);
				EditorGUILayout.LabelField(StudioTransformSelection().Count + " selected", EditorStyles.miniLabel);
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Select Whole Rig")) { m_studioWholeRig = true; SelectStudioWholeRig(); }
					if (GUILayout.Button("Clear")) { m_studioWholeRig = false; if (m_studioSelectedBones != null) m_studioSelectedBones.Clear(); m_studioBone = string.Empty; }
				}
				int sortingOrder = StudioSortingOrder(m_studioBone);
				EditorGUI.BeginChangeCheck(); int nextSortingOrder = EditorGUILayout.IntField(m_studioEditPlayer ? "Player sprite layer" : "Sprite layer", sortingOrder);
				if (EditorGUI.EndChangeCheck())
				{
					if (m_studioAutoKey) InsertStudioSortingKey(nextSortingOrder);
					else SetStudioSortingOrder(m_studioBone, nextSortingOrder);
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Layer -1")) { int value = sortingOrder - 1; if (m_studioAutoKey) InsertStudioSortingKey(value); else SetStudioSortingOrder(m_studioBone, value); }
					if (GUILayout.Button("Layer +1")) { int value = sortingOrder + 1; if (m_studioAutoKey) InsertStudioSortingKey(value); else SetStudioSortingOrder(m_studioBone, value); }
				}
				Vector3 positionValue = bone.localPosition;
				Vector3 scaleValue = bone.localScale;
				float rotation = Mathf.DeltaAngle(0f, bone.localEulerAngles.z);
				EditorGUI.BeginChangeCheck();
				float x = EditorGUILayout.FloatField("Location X", positionValue.x);
				float y = EditorGUILayout.FloatField("Location Y", positionValue.y);
				float angle = EditorGUILayout.FloatField("Rotation Z", rotation);
				float scaleX = EditorGUILayout.FloatField("Scale X", scaleValue.x);
				float scaleY = EditorGUILayout.FloatField("Scale Y", scaleValue.y);
				if (EditorGUI.EndChangeCheck())
				{
					if (m_studioAutoKey) InsertStudioTransformKeys(x, y, angle, scaleX, scaleY);
					else { bone.localPosition = new Vector3(x, y, bone.localPosition.z); bone.localRotation = Quaternion.Euler(0f, 0f, angle); bone.localScale = new Vector3(scaleX, scaleY, bone.localScale.z); Repaint(); }
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Insert LocRotScale")) InsertStudioTransformKeys(positionValue.x, positionValue.y, rotation, scaleValue.x, scaleValue.y);
					if (GUILayout.Button("Delete Keys")) DeleteStudioKeysAtPlayhead();
				}
				EditorGUILayout.Space(5f);
				List<SpriteRenderer> renderers = !m_studioEditPlayer && m_normalizedEnemyRenderers.TryGetValue(m_studioBone, out List<SpriteRenderer> found) ? found : null;
				string[] regions = AvailableRegionNames();
				if (renderers != null && renderers.Count > 0 && regions.Length > 0)
				{
					string current = renderers[0].sprite == null ? string.Empty : renderers[0].sprite.name;
					int index = Mathf.Max(0, Array.IndexOf(regions, current));
					EditorGUI.BeginChangeCheck(); int next = EditorGUILayout.Popup("Sprite region", index, regions);
					if (EditorGUI.EndChangeCheck())
					{
						if (m_studioAutoKey) InsertStudioSpriteKey(regions[next]);
						else if (m_modSprites.TryGetValue(regions[next], out Sprite sprite)) foreach (SpriteRenderer renderer in renderers) renderer.sprite = sprite;
					}
					if (GUILayout.Button("Insert Sprite Key")) InsertStudioSpriteKey(regions[index]);
				}
				if (!m_studioEditPlayer) DrawStudioRigPivotEditor();
				EditorGUILayout.Space(5f);
				EditorGUILayout.LabelField("Keying", EditorStyles.boldLabel);
				EditorGUILayout.LabelField(m_studioAutoKey ? "Editing a transform field inserts or updates keys at the playhead." : "Auto Key is off. Changes remain a preview until Insert LocRotScale is pressed.", EditorStyles.wordWrappedMiniLabel);
			}
			DrawStudioSelectedKeyInspector();
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void DrawStudioRigPivotEditor()
		{
			EnsureStudioPivotLoaded();
			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Rig sprite pivot", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("This moves the image around its bone for every clip. Animation keys move the bone itself.", EditorStyles.wordWrappedMiniLabel);
			m_studioPivotX = EditorGUILayout.Slider("Pivot X", m_studioPivotX, 0f, 1f);
			m_studioPivotY = EditorGUILayout.Slider("Pivot Y", m_studioPivotY, 0f, 1f);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Center")) { m_studioPivotX = 0.5f; m_studioPivotY = 0.5f; }
				if (GUILayout.Button("Apply + Reload Rig")) SaveStudioRigPivot();
			}
		}

		private void EnsureStudioPivotLoaded()
		{
			if (string.Equals(m_studioPivotBone, m_studioBone, StringComparison.Ordinal)) return;
			m_studioPivotBone = m_studioBone; m_studioPivotX = 0.5f; m_studioPivotY = 0.5f;
			try
			{
				JArray bones = JObject.Parse(System.IO.File.ReadAllText(m_modEnemyPath))["visual"]?["bones"] as JArray;
				JObject bone = bones?.OfType<JObject>().FirstOrDefault(i_item => string.Equals(i_item["id"]?.Value<string>(), m_studioBone, StringComparison.Ordinal));
				if (bone == null) return;
				m_studioPivotX = bone["pivotX"]?.Value<float>() ?? 0.5f; m_studioPivotY = bone["pivotY"]?.Value<float>() ?? 0.5f;
			}
			catch (Exception) { }
		}

		private void SaveStudioRigPivot()
		{
			try
			{
				if (string.IsNullOrEmpty(m_modEnemyPath) || !System.IO.File.Exists(m_modEnemyPath)) throw new InvalidOperationException("The selected enemy JSON cannot be found.");
				JObject root = JObject.Parse(System.IO.File.ReadAllText(m_modEnemyPath));
				JArray bones = root["visual"]?["bones"] as JArray;
				JObject bone = bones?.OfType<JObject>().FirstOrDefault(i_item => string.Equals(i_item["id"]?.Value<string>(), m_studioBone, StringComparison.Ordinal));
				if (bone == null) throw new InvalidOperationException("The selected bone is not defined in visual.bones.");
				bone["pivotX"] = Mathf.Clamp01(m_studioPivotX); bone["pivotY"] = Mathf.Clamp01(m_studioPivotY);
				System.IO.File.WriteAllText(m_modEnemyPath, root.ToString(Newtonsoft.Json.Formatting.Indented));
				m_studioPivotBone = null; SelectCurrentClip(); m_normalizedEnemyStatus = "Saved rig pivot for " + m_studioBone + ".";
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Edit rig pivot", exception.Message, "OK"); }
		}

		private void DrawStudioSelectedKeyInspector()
		{
			if (m_studioSelectedNumericTrack == null || m_studioSelectedNumericKey == null) return;
			EditorGUILayout.Space(8f); EditorGUILayout.LabelField("Selected key", EditorStyles.boldLabel);
			EditorGUILayout.LabelField(m_studioSelectedNumericTrack.Target + " / " + m_studioSelectedNumericTrack.Property, EditorStyles.wordWrappedMiniLabel);
			EditorGUI.BeginChangeCheck();
			float time = EditorGUILayout.FloatField(m_studioShowFrames ? "Frame" : "Time", m_studioShowFrames ? m_studioSelectedNumericKey.Time * StudioFrameRate() : m_studioSelectedNumericKey.Time);
			float value = EditorGUILayout.FloatField("Value", m_studioSelectedNumericKey.Value);
			if (EditorGUI.EndChangeCheck())
			{
				NormalizedNumericTrack track = m_studioSelectedNumericTrack; NormalizedNumericKey key = m_studioSelectedNumericKey;
				RecordStudioChange("Edit selected animation key", () =>
				{
					key.Time = SnapStudioTime(m_studioShowFrames ? time / StudioFrameRate() : time);
					key.Value = value; track.Keys = track.Keys.OrderBy(i_item => i_item.Time).ToList(); m_studioSelectedKey = track.Keys.IndexOf(key); m_time = key.Time;
				});
			}
		}

		private void InsertStudioTransformKeys(float i_x, float i_y, float i_rotation, float i_scaleX, float i_scaleY)
		{
			if (string.IsNullOrEmpty(m_studioBone)) return;
			RecordStudioChange("Insert bone transform keys", () =>
			{
				UpsertStudioNumericKey("bone/" + m_studioBone, "position.x", i_x);
				UpsertStudioNumericKey("bone/" + m_studioBone, "position.y", i_y);
				UpsertStudioNumericKey("bone/" + m_studioBone, "rotation.z", i_rotation);
				UpsertStudioNumericKey("bone/" + m_studioBone, "scale.x", i_scaleX);
				UpsertStudioNumericKey("bone/" + m_studioBone, "scale.y", i_scaleY);
			});
		}

		private void InsertStudioTransformKeysForSelection()
		{
			Dictionary<string, Transform> bones = StudioActiveBones(); List<string> selection = StudioTransformSelection();
			if (selection.Count == 0) return;
			RecordStudioChange("Insert selected bone transform keys", () =>
			{
				foreach (string name in selection)
				{
					if (!bones.TryGetValue(name, out Transform bone)) continue; Vector3 local = bone.localPosition; Vector3 scale = bone.localScale;
					UpsertStudioNumericKey("bone/" + name, "position.x", local.x); UpsertStudioNumericKey("bone/" + name, "position.y", local.y);
					UpsertStudioNumericKey("bone/" + name, "rotation.z", Mathf.DeltaAngle(0f, bone.localEulerAngles.z));
					UpsertStudioNumericKey("bone/" + name, "scale.x", scale.x); UpsertStudioNumericKey("bone/" + name, "scale.y", scale.y);
				}
			});
		}

		private void InsertStudioSortingKey(int i_sortingOrder)
		{
			if (string.IsNullOrEmpty(m_studioBone)) return;
			RecordStudioChange("Insert sprite layer key", () => UpsertStudioNumericKey("sprite/" + m_studioBone, "sortingOrder", i_sortingOrder));
		}

		private void SetStudioSortingOrder(string i_bone, int i_sortingOrder)
		{
			if (m_studioEditPlayer)
			{
				Transform transformValue = FindStudioPlayerBone(i_bone); BonePlayer bone = transformValue == null ? null : transformValue.GetComponent<BonePlayer>();
				SpriteRenderer renderer = bone?.GetBodyPart() == null ? null : bone.GetBodyPart().GetComponent<SpriteRenderer>(); if (renderer != null) renderer.sortingOrder = i_sortingOrder;
			}
			else if (m_normalizedEnemyRenderers.TryGetValue(i_bone, out List<SpriteRenderer> renderers)) foreach (SpriteRenderer renderer in renderers) renderer.sortingOrder = i_sortingOrder;
			Repaint();
		}

		private void UpsertStudioNumericKey(string i_target, string i_property, float i_value)
		{
			List<NormalizedNumericTrack> tracks = StudioNumericTracks();
			NormalizedNumericTrack track = tracks.FirstOrDefault(i_item => i_item.Target == i_target && i_item.Property == i_property);
			if (track == null) { track = new NormalizedNumericTrack { Target = i_target, Property = i_property, Keys = new List<NormalizedNumericKey>() }; tracks.Add(track); }
			float tolerance = 0.5f / StudioFrameRate();
				float keyTime = SnapStudioTime(m_time);
				NormalizedNumericKey key = track.Keys.FirstOrDefault(i_item => Mathf.Abs(i_item.Time - keyTime) <= tolerance);
				if (key == null) { key = new NormalizedNumericKey { Time = keyTime }; track.Keys.Add(key); }
			key.Value = i_value; track.Keys = track.Keys.OrderBy(i_item => i_item.Time).ToList();
		}

		private void InsertStudioSpriteKey(string i_region)
		{
			if (string.IsNullOrEmpty(m_studioBone) || string.IsNullOrEmpty(i_region)) return;
			RecordStudioChange("Insert sprite region key", () =>
			{
				List<NormalizedObjectTrack> tracks = StudioObjectTracks();
				string target = "sprite/" + m_studioBone;
				NormalizedObjectTrack track = tracks.FirstOrDefault(i_item => i_item.Target == target && i_item.Property == "sprite");
				if (track == null) { track = new NormalizedObjectTrack { Target = target, Property = "sprite", Keys = new List<NormalizedObjectKey>() }; tracks.Add(track); }
				float tolerance = 0.5f / StudioFrameRate();
				float keyTime = SnapStudioTime(m_time);
				NormalizedObjectKey key = track.Keys.FirstOrDefault(i_item => Mathf.Abs(i_item.Time - keyTime) <= tolerance);
				if (key == null) { key = new NormalizedObjectKey { Time = keyTime }; track.Keys.Add(key); }
				key.Name = i_region; key.Asset = null; track.Keys = track.Keys.OrderBy(i_item => i_item.Time).ToList();
			});
		}

		private void DeleteStudioKeysAtPlayhead()
		{
			if (string.IsNullOrEmpty(m_studioBone)) return;
			RecordStudioChange("Delete bone keys at playhead", () =>
			{
				float tolerance = 0.5f / StudioFrameRate();
				foreach (NormalizedNumericTrack track in StudioNumericTracks().Where(i_item => i_item.Target == "bone/" + m_studioBone || i_item.Target == "sprite/" + m_studioBone))
					track.Keys.RemoveAll(i_key => Mathf.Abs(i_key.Time - m_time) <= tolerance && track.Keys.Count > 1);
				foreach (NormalizedObjectTrack track in StudioObjectTracks().Where(i_item => i_item.Target == "sprite/" + m_studioBone))
					track.Keys.RemoveAll(i_key => Mathf.Abs(i_key.Time - m_time) <= tolerance && track.Keys.Count > 1);
			});
		}

		private void DrawStudioDopeSheet(Rect i_rect)
		{
			DrawStudioPanel(i_rect, "DOPE SHEET");
			List<StudioLane> lanes = BuildStudioLanes();
			Rect scrollRect = new Rect(i_rect.x + 2f, i_rect.y + 23f, i_rect.width - 4f, i_rect.height - 25f);
			float contentHeight = Mathf.Max(scrollRect.height - 2f, 25f + lanes.Count * 21f);
			m_studioDopeScroll = GUI.BeginScrollView(scrollRect, m_studioDopeScroll, new Rect(0f, 0f, scrollRect.width - 17f, contentHeight));
			float labels = Mathf.Clamp(scrollRect.width * 0.25f, 145f, 235f);
			Rect timeline = new Rect(labels, 0f, Mathf.Max(30f, scrollRect.width - labels - 20f), contentHeight);
			EditorGUI.DrawRect(new Rect(0f, 0f, labels, contentHeight), new Color(0.135f, 0.135f, 0.135f, 1f));
			EditorGUI.DrawRect(new Rect(labels, 0f, timeline.width, contentHeight), new Color(0.085f, 0.085f, 0.085f, 1f));
			float duration = StudioDuration();
			for (int tick = 0; tick <= 10; tick++)
			{
				float x = timeline.x + timeline.width * tick / 10f;
				EditorGUI.DrawRect(new Rect(x, 0f, 1f, contentHeight), new Color(1f, 1f, 1f, tick % 5 == 0 ? 0.12f : 0.05f));
				GUI.Label(new Rect(x + 2f, 1f, 55f, 18f), StudioTimeLabel(duration * tick / 10f), EditorStyles.miniLabel);
			}
			for (int laneIndex = 0; laneIndex < lanes.Count; laneIndex++)
			{
				StudioLane lane = lanes[laneIndex]; float y = 25f + laneIndex * 21f;
				if (laneIndex % 2 == 1) EditorGUI.DrawRect(new Rect(0f, y, labels + timeline.width, 20f), new Color(1f, 1f, 1f, 0.025f));
				GUI.Label(new Rect(5f, y + 1f, labels - 8f, 18f), lane.Label, laneIndex == m_studioSelectedTrack ? EditorStyles.miniBoldLabel : EditorStyles.miniLabel);
				DrawStudioLaneKeys(lane, laneIndex, new Rect(timeline.x, y, timeline.width, 20f), duration);
			}
			float playhead = timeline.x + Mathf.Clamp01(m_time / duration) * timeline.width;
			EditorGUI.DrawRect(new Rect(playhead, 0f, 1f, contentHeight), new Color(1f, 0.35f, 0.2f, 0.95f));
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && new Rect(timeline.x, 0f, timeline.width, 22f).Contains(Event.current.mousePosition))
			{
				SetStudioTime(Mathf.Clamp01((Event.current.mousePosition.x - timeline.x) / timeline.width) * duration); Event.current.Use();
			}
			HandleStudioKeyDrag(timeline, duration);
			GUI.EndScrollView();
		}

		private List<StudioLane> BuildStudioLanes()
		{
			List<StudioLane> lanes = new List<StudioLane>();
			foreach (NormalizedNumericTrack track in StudioNumericTracks()) lanes.Add(new StudioLane { Label = track.Target + "  " + track.Property, Numeric = track });
			foreach (NormalizedObjectTrack track in StudioObjectTracks()) lanes.Add(new StudioLane { Label = track.Target + "  sprite", Object = track });
			if (!m_studioEditPlayer)
			{
				lanes.Add(new StudioLane { Label = "Events", Events = m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>() });
				lanes.Add(new StudioLane { Label = "Effects", Effects = m_normalizedEnemyClip.EffectTriggers ?? new List<NormalizedEffectTrigger>() });
			}
			return lanes;
		}

		private void DrawStudioLaneKeys(StudioLane i_lane, int i_laneIndex, Rect i_rect, float i_duration)
		{
			if (i_lane.Numeric != null)
			{
				for (int index = 0; index < i_lane.Numeric.Keys.Count; index++) DrawStudioKey(i_rect, i_duration, i_lane.Numeric.Keys[index].Time, i_laneIndex, index, new Color(1f, 0.72f, 0.2f, 1f), i_lane.Numeric, i_lane.Numeric.Keys[index]);
			}
			else if (i_lane.Object != null)
			{
				for (int index = 0; index < i_lane.Object.Keys.Count; index++) DrawStudioKey(i_rect, i_duration, i_lane.Object.Keys[index].Time, i_laneIndex, index, new Color(0.55f, 0.85f, 1f, 1f), null, null);
			}
			else if (i_lane.Events != null)
			{
				for (int index = 0; index < i_lane.Events.Count; index++) DrawStudioKey(i_rect, i_duration, i_lane.Events[index].Time, i_laneIndex, index, new Color(1f, 0.4f, 0.15f, 1f), null, null);
			}
			else if (i_lane.Effects != null)
			{
				for (int index = 0; index < i_lane.Effects.Count; index++) DrawStudioKey(i_rect, i_duration, i_lane.Effects[index].Time, i_laneIndex, index, new Color(0.25f, 1f, 0.9f, 1f), null, null);
			}
		}

		private void DrawStudioKey(Rect i_lane, float i_duration, float i_time, int i_laneIndex, int i_keyIndex, Color i_color, NormalizedNumericTrack i_track, NormalizedNumericKey i_key)
		{
			float x = i_lane.x + Mathf.Clamp01(i_time / i_duration) * i_lane.width;
			Rect keyRect = new Rect(x - 4f, i_lane.y + 6f, 8f, 8f);
			EditorGUI.DrawRect(keyRect, i_laneIndex == m_studioSelectedTrack && i_keyIndex == m_studioSelectedKey ? Color.white : i_color);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && keyRect.Contains(Event.current.mousePosition))
			{
				m_studioSelectedTrack = i_laneIndex; m_studioSelectedKey = i_keyIndex; m_time = i_time; m_playing = false;
				m_studioSelectedNumericTrack = i_track; m_studioSelectedNumericKey = i_key;
				m_studioDraggedTrack = i_track; m_studioDraggedKey = i_key;
				if (i_key != null) Undo.RecordObject(this, "Move animation key");
				SampleCurrentClip(); Repaint(); Event.current.Use();
			}
		}

		private void HandleStudioKeyDrag(Rect i_timeline, float i_duration)
		{
			if (m_studioDraggedKey == null) return;
			if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
			{
				m_studioDraggedKey.Time = SnapStudioTime(Mathf.Clamp01((Event.current.mousePosition.x - i_timeline.x) / i_timeline.width) * i_duration);
				m_time = m_studioDraggedKey.Time; if (m_studioEditPlayer) SyncStudioPlayerJson(); else SyncAuthoringJson(); SampleCurrentClip(); Repaint(); Event.current.Use();
			}
			else if (Event.current.rawType == EventType.MouseUp)
			{
				m_studioDraggedTrack.Keys = m_studioDraggedTrack.Keys.OrderBy(i_item => i_item.Time).ToList();
				m_studioSelectedKey = m_studioDraggedTrack.Keys.IndexOf(m_studioDraggedKey);
				if (m_studioEditPlayer) { SyncStudioPlayerJson(); ValidateStudioPlayerDocument(); } else { SyncAuthoringJson(); ValidateAuthoringDocument(); } EditorUtility.SetDirty(this);
				m_studioDraggedTrack = null; m_studioDraggedKey = null; Repaint();
			}
		}

		private float SnapStudioTime(float i_time)
		{
			float clamped = Mathf.Clamp(i_time, 0f, StudioDuration());
			if (!m_studioSnapFrames) return clamped;
			float rate = StudioFrameRate();
			return Mathf.Clamp(Mathf.Round(clamped * rate) / rate, 0f, StudioDuration());
		}

		private string StudioTimeLabel(float i_time)
		{
			return m_studioShowFrames ? Mathf.RoundToInt(i_time * StudioFrameRate()) + "f" : i_time.ToString("0.000") + "s";
		}

		private void SetStudioTime(float i_time)
		{
			m_time = SnapStudioTime(i_time); m_playing = false; SampleCurrentClip(); Repaint();
		}

		private void JumpStudioKey(int i_direction)
		{
			List<float> times = new List<float>();
			foreach (NormalizedNumericTrack track in StudioNumericTracks()) times.AddRange((track.Keys ?? new List<NormalizedNumericKey>()).Select(i_key => i_key.Time));
			foreach (NormalizedObjectTrack track in StudioObjectTracks()) times.AddRange((track.Keys ?? new List<NormalizedObjectKey>()).Select(i_key => i_key.Time));
			if (!m_studioEditPlayer) { times.AddRange((m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>()).Select(i_event => i_event.Time)); times.AddRange((m_normalizedEnemyClip.EffectTriggers ?? new List<NormalizedEffectTrigger>()).Select(i_trigger => i_trigger.Time)); }
			List<float> ordered = times.Distinct().OrderBy(i_time => i_time).ToList();
			if (ordered.Count == 0) return;
			float tolerance = 0.25f / StudioFrameRate();
			float target = i_direction < 0 ? ordered.LastOrDefault(i_time => i_time < m_time - tolerance) : ordered.FirstOrDefault(i_time => i_time > m_time + tolerance);
			if (i_direction < 0 && !ordered.Any(i_time => i_time < m_time - tolerance)) target = ordered[ordered.Count - 1];
			if (i_direction > 0 && !ordered.Any(i_time => i_time > m_time + tolerance)) target = ordered[0];
			SetStudioTime(target);
		}

		private void CopyStudioPose()
		{
			m_studioPoseClipboard.Clear();
			foreach (KeyValuePair<string, Transform> pair in StudioActiveBones())
				m_studioPoseClipboard[pair.Key] = new StudioPoseValue { Position = pair.Value.localPosition, Rotation = Mathf.DeltaAngle(0f, pair.Value.localEulerAngles.z), Scale = pair.Value.localScale };
			m_normalizedEnemyStatus = "Copied " + m_studioPoseClipboard.Count + " bone transforms.";
			Repaint();
		}

		private void PasteStudioPose()
		{
			if (m_studioPoseClipboard.Count == 0) return;
			RecordStudioChange("Paste keyed pose", () =>
			{
				foreach (KeyValuePair<string, StudioPoseValue> pair in m_studioPoseClipboard)
				{
					UpsertStudioNumericKey("bone/" + pair.Key, "position.x", pair.Value.Position.x);
					UpsertStudioNumericKey("bone/" + pair.Key, "position.y", pair.Value.Position.y);
					UpsertStudioNumericKey("bone/" + pair.Key, "rotation.z", pair.Value.Rotation);
					UpsertStudioNumericKey("bone/" + pair.Key, "scale.x", pair.Value.Scale.x);
					UpsertStudioNumericKey("bone/" + pair.Key, "scale.y", pair.Value.Scale.y);
				}
			});
		}

		private void DuplicateStudioSelectedKey()
		{
			if (m_studioSelectedNumericTrack == null || m_studioSelectedNumericKey == null) return;
			NormalizedNumericTrack track = m_studioSelectedNumericTrack; NormalizedNumericKey source = m_studioSelectedNumericKey;
			RecordStudioChange("Duplicate animation key", () =>
			{
				float frame = 1f / StudioFrameRate();
				float time = SnapStudioTime(source.Time + frame);
				if (Mathf.Approximately(time, source.Time)) time = SnapStudioTime(source.Time - frame);
				NormalizedNumericKey copy = new NormalizedNumericKey { Time = time, Value = source.Value, InTangent = source.InTangent, OutTangent = source.OutTangent };
				track.Keys.Add(copy); track.Keys = track.Keys.OrderBy(i_key => i_key.Time).ToList(); m_studioSelectedNumericKey = copy; m_studioSelectedKey = track.Keys.IndexOf(copy); m_time = copy.Time;
			});
		}

		private void DeleteStudioSelectedKey()
		{
			if (m_studioSelectedNumericTrack == null || m_studioSelectedNumericKey == null) return;
			if (m_studioSelectedNumericTrack.Keys.Count <= 1) { m_normalizedEnemyStatus = "A numeric track must retain at least one key."; return; }
			NormalizedNumericTrack track = m_studioSelectedNumericTrack; NormalizedNumericKey key = m_studioSelectedNumericKey;
			RecordStudioChange("Delete animation key", () => track.Keys.Remove(key));
			m_studioSelectedNumericKey = null; m_studioSelectedNumericTrack = null; m_studioSelectedKey = -1;
		}

		private sealed class StudioLane
		{
			public string Label;
			public NormalizedNumericTrack Numeric;
			public NormalizedObjectTrack Object;
			public List<EnemyAnimationEventDefinition> Events;
			public List<NormalizedEffectTrigger> Effects;
		}

		private sealed class StudioPoseValue
		{
			public Vector3 Position;
			public float Rotation;
			public Vector3 Scale;
		}

		private sealed class StudioTransformStart
		{
			public Vector3 Position;
			public float Rotation;
			public Vector3 Scale;
		}
	}
}
