using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed partial class PlayerAnimationPreviewWindow : EditorWindow
	{
		private const string CatalogPath = "ModSDK/AnimationReference/core-animation-catalog.json";
		private const string PlayerPrefabPath = "Assets/Actors/Players/Alex.prefab";
		private const string PlayerControllerPath = "Assets/AnimatorController/DefaultPlayer.controller";
		private const string PreviewMaterialPath = "Assets/Material/Sprite-Unlit-Default.mat";
		private const string CoreClothingCatalogPath = "Assets/Resources/Modding/Core/Content/core-clothing.json";
		private const string NormalizedEnemyRoot = "ModSDK/AnimationReference/NormalizedEnemies";

		private PreviewRenderUtility m_preview;
		private GameObject m_playerInstance;
		private GameObject m_enemyInstance;
		private Animator m_playerAnimator;
		private Animator m_enemyAnimator;
		private GameObject m_sampleTarget;
		private GameObject m_enemySampleTarget;
		private AnimationClip m_clip;
		private AnimationClip m_enemyClip;
		private NormalizedEnemyAnimationDocument m_normalizedEnemyClip;
		private readonly Dictionary<string, Transform> m_normalizedEnemyBones = new Dictionary<string, Transform>(StringComparer.Ordinal);
		private readonly Dictionary<string, List<SpriteRenderer>> m_normalizedEnemyRenderers = new Dictionary<string, List<SpriteRenderer>>(StringComparer.Ordinal);
		private string m_normalizedEnemyPath;
		private string m_normalizedEnemyStatus;
		private int m_enemyPreviewSource;
		[SerializeField] private string m_authoringJson;
		private string m_authoringValidation;
		private ValidationReport m_authoringReferenceValidation = new ValidationReport();
		private string m_savePackRoot = "Mods";
		[SerializeField] private string m_modEnemyPath;
		[SerializeField] private string m_modAnimationPath;
		[SerializeField] private string m_modPackRoot;
		private readonly List<ModEnemyChoice> m_modEnemies = new List<ModEnemyChoice>();
		private readonly List<ModAnimationChoice> m_modAnimations = new List<ModAnimationChoice>();
		private int m_modEnemyIndex;
		private int m_modAnimationIndex;
		private string m_newClipName = "New Animation";
		private float m_newClipDuration = 1f;
		private readonly Dictionary<string, Sprite> m_modSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		private readonly List<UnityEngine.Object> m_modPreviewAssets = new List<UnityEngine.Object>();
		private bool m_showAuthoring;
		private int m_authoringTrackIndex;
		private int m_authoringKeyIndex;
		private int m_authoringEventIndex;
		private int m_authoringObjectTrackIndex;
		private int m_authoringObjectKeyIndex;
		private Vector2 m_authoringScroll;
		[SerializeField] private bool m_animationUtilityTools;
		[SerializeField] private bool m_modSourceBrowser;
		[SerializeField] private Vector2 m_modEnemyBrowserScroll;
		[SerializeField] private Vector2 m_modAnimationBrowserScroll;
		private RaperAnimation m_pairAnimation;
		private SkeletonPlayer m_skeletonPlayer;
		private Material m_previewMaterial;
		private CatalogView m_catalog;
		private readonly List<TransformSnapshot> m_defaultTransforms = new List<TransformSnapshot>();
		private readonly List<RendererSnapshot> m_defaultRenderers = new List<RendererSnapshot>();
		private readonly List<SortingSnapshot> m_defaultSortingGroups = new List<SortingSnapshot>();
		private readonly List<TransformSnapshot> m_enemyDefaultTransforms = new List<TransformSnapshot>();
		private readonly List<RendererSnapshot> m_enemyDefaultRenderers = new List<RendererSnapshot>();
		private readonly List<SortingSnapshot> m_enemyDefaultSortingGroups = new List<SortingSnapshot>();
		private readonly List<GameObject> m_previewClothingPieces = new List<GameObject>();
		private readonly List<GameObject> m_effectPreviewInstances = new List<GameObject>();
		private readonly List<ClothingPiecePreview> m_clothingPiecePreviews = new List<ClothingPiecePreview>();
		private readonly List<SpriteRenderer> m_hiddenBodyRenderers = new List<SpriteRenderer>();
		private readonly List<string> m_diagnostics = new List<string>();
		private readonly List<ClothingOption> m_clothingOptions = new List<ClothingOption>();
		private readonly List<ClothingOption> m_equippedClothing = new List<ClothingOption>();
		private string m_error;
		private int m_mode;
		private int m_standardClipIndex;
		private int m_enemyIndex;
		private int m_phaseIndex;
		private int m_enemyClipIndex;
		private int m_previewEnemyIndex = -1;
		private float m_time;
		private bool m_playing;
		private bool m_loop = true;
		private bool m_facingLeft;
		private bool m_lockCameraFraming = true;
		private bool m_hasLockedBounds;
		private Bounds m_lockedBounds;
		private int m_playbackSpeedIndex = 2;
		private int m_clothingCategoryIndex;
		private int m_clothingIndex;
		private bool m_showClothing = true;
		private bool m_showBoneOverlay;
		private bool m_showAttachmentOverlay;
		private bool m_showSpriteBoundsOverlay;
		private bool m_showCanvasBoundsOverlay;
		private bool m_showSlotLabels;
		private bool m_showDiagnostics = true;
		private bool m_showFinisherEffects = true;
		private bool m_enemyInverseFacing;
		private bool m_enemyPositionToPlayer;
		private bool m_centerEnemyY;
		private bool m_centerPlayerY;
		private bool m_samplingCameraBounds;
		private float m_playerFeetOffset;
		private float m_enemyFeetOffset;
		private string m_compatibilityNotice;
		private double m_lastEditorTime;
		private static readonly float[] PlaybackSpeeds = { 0.25f, 0.5f, 1f, 1.5f, 2f };
		private static readonly string[] PlaybackSpeedLabels = { "0.25x", "0.5x", "1x", "1.5x", "2x" };

		[MenuItem("Captivity Reloaded/Modding/Player Animation Preview")]
		public static void Open()
		{
			PlayerAnimationPreviewWindow window = GetWindow<PlayerAnimationPreviewWindow>();
			window.titleContent = new GUIContent("Animation Preview");
			window.minSize = new Vector2(540f, 520f);
			window.Show();
		}

		private void OnEnable()
		{
			LoadCatalog();
			LoadCoreClothing();
			CreatePreview();
			if (!string.IsNullOrEmpty(m_modPackRoot)) RefreshModPackContents();
			SelectCurrentClip();
			m_lastEditorTime = EditorApplication.timeSinceStartup;
			EditorApplication.update += OnEditorUpdate;
			Undo.undoRedoPerformed += OnAuthoringUndoRedo;
		}

		private void OnDisable()
		{
			EditorApplication.update -= OnEditorUpdate;
			Undo.undoRedoPerformed -= OnAuthoringUndoRedo;
			DestroyPreview();
		}

		private void LoadCatalog()
		{
			m_error = string.Empty;
			if (!File.Exists(CatalogPath))
			{
				m_error = "No animation catalog was found. Run Captivity Reloaded > Modding > Export Core Animation Catalog first.";
				return;
			}
			try
			{
				m_catalog = JsonConvert.DeserializeObject<CatalogView>(File.ReadAllText(CatalogPath));
				if (m_catalog == null || m_catalog.SchemaVersion < 2)
					m_error = "The animation catalog is outdated. Export it again to create schema version 2.";
				else if (m_catalog.Enemies == null || m_catalog.Enemies.Count == 0)
					m_error = "The animation catalog contains no enemies.";
			}
			catch (Exception exception)
			{
				m_error = "Could not read the animation catalog: " + exception.Message;
			}
		}

		private void LoadCoreClothing()
		{
			m_clothingOptions.Clear();
			TextAsset catalogAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CoreClothingCatalogPath);
			if (catalogAsset == null) return;
			CoreClothingCatalogView catalog;
			try { catalog = JsonConvert.DeserializeObject<CoreClothingCatalogView>(catalogAsset.text); }
			catch (JsonException) { return; }
			if (catalog?.Entries == null) return;

			Dictionary<int, CoreClothingEntryView> entries = catalog.Entries
				.GroupBy(i_entry => i_entry.LegacyId)
				.ToDictionary(i_group => i_group.Key, i_group => i_group.First());
			foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Clothes" }))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				Clothing clothing = prefab == null ? null : prefab.GetComponent<Clothing>();
				if (clothing == null || !entries.TryGetValue(clothing.GetId(), out CoreClothingEntryView entry)) continue;
				if (!Enum.TryParse(entry.Category, true, out ClothingCategory category)) continue;
				m_clothingOptions.Add(new ClothingOption
				{
					Id = entry.Id,
					DisplayName = MakeDisplayName(entry.Id),
					Category = category,
					Prefab = prefab,
					Clothing = clothing
				});
			}
			m_clothingOptions.Sort((i_left, i_right) =>
			{
				int category = i_left.Category.CompareTo(i_right.Category);
				return category != 0 ? category : string.Compare(i_left.DisplayName, i_right.DisplayName, StringComparison.OrdinalIgnoreCase);
			});
		}

		private static string MakeDisplayName(string i_id)
		{
			string path = (i_id ?? string.Empty).Split('/').LastOrDefault() ?? string.Empty;
			return string.Join(" ", path.Split('-').Where(i_part => i_part.Length > 0)
				.Select(i_part => char.ToUpperInvariant(i_part[0]) + i_part.Substring(1)));
		}

		private void CreatePreview()
		{
			DestroyPreview();
			m_preview = new PreviewRenderUtility();
			m_preview.camera.orthographic = true;
			m_preview.camera.clearFlags = CameraClearFlags.Color;
			m_preview.camera.backgroundColor = new Color(0.09f, 0.075f, 0.07f, 1f);
			m_preview.camera.nearClipPlane = 0.01f;
			m_preview.camera.farClipPlane = 100f;

			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
			if (prefab == null)
			{
				m_error = "The Core player prefab could not be found at " + PlayerPrefabPath + ".";
				return;
			}

			m_playerInstance = Instantiate(prefab);
			m_playerInstance.name = "PlayerAnimationPreview";
			m_playerInstance.hideFlags = HideFlags.HideAndDontSave;
			m_playerInstance.transform.position = Vector3.zero;
			m_preview.AddSingleGO(m_playerInstance);

			RuntimeAnimatorController expectedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerPath);
			m_playerAnimator = m_playerInstance.GetComponentsInChildren<Animator>(true)
				.FirstOrDefault(i_animator => i_animator.runtimeAnimatorController == expectedController);
			if (m_playerAnimator == null)
				m_playerAnimator = m_playerInstance.GetComponentsInChildren<Animator>(true)
					.FirstOrDefault(i_animator => i_animator.gameObject.name == "SkeletonAlex");
			m_skeletonPlayer = m_playerInstance.GetComponentInChildren<SkeletonPlayer>(true);
			Player previewPlayer = m_playerInstance.GetComponentInChildren<Player>(true);
			m_playerFeetOffset = previewPlayer == null ? 0f : GetFeetOffset(previewPlayer);

			// The preview samples clips directly. Leaving an Animator enabled lets its
			// default state evaluate during editor rendering and overwrite that sample.
			foreach (Animator animator in m_playerInstance.GetComponentsInChildren<Animator>(true))
				animator.enabled = false;
			foreach (Rigidbody2D body in m_playerInstance.GetComponentsInChildren<Rigidbody2D>(true))
				body.simulated = false;

			m_previewMaterial = AssetDatabase.LoadAssetAtPath<Material>(PreviewMaterialPath);
			if (m_previewMaterial != null)
			{
				foreach (SpriteRenderer renderer in m_playerInstance.GetComponentsInChildren<SpriteRenderer>(true))
					renderer.sharedMaterial = m_previewMaterial;
			}

			if (m_playerAnimator == null)
				m_error = "The SkeletonPlayer Animator was not found in the Core player prefab.";
			else if (m_skeletonPlayer == null)
				m_error = "The SkeletonPlayer component was not found in the Core player prefab.";
			CaptureDefaultPose();
			RebuildPreviewClothing();
		}

		private void DestroyPreview()
		{
			ClearPreviewEffects();
			if (m_preview != null) m_preview.Cleanup();
			m_preview = null;
			foreach (UnityEngine.Object asset in m_modPreviewAssets) if (asset != null) DestroyImmediate(asset);
			m_modPreviewAssets.Clear();
			m_modSprites.Clear();
			m_playerInstance = null;
			m_enemyInstance = null;
			m_playerAnimator = null;
			m_enemyAnimator = null;
			m_sampleTarget = null;
			m_enemySampleTarget = null;
			m_enemyClip = null;
			ClearNormalizedEnemyPreview();
			m_pairAnimation = null;
			m_previewEnemyIndex = -1;
			m_playerFeetOffset = 0f;
			m_enemyFeetOffset = 0f;
			m_skeletonPlayer = null;
			m_previewMaterial = null;
			m_defaultTransforms.Clear();
			m_defaultRenderers.Clear();
			m_defaultSortingGroups.Clear();
			m_enemyDefaultTransforms.Clear();
			m_enemyDefaultRenderers.Clear();
			m_enemyDefaultSortingGroups.Clear();
			m_previewClothingPieces.Clear();
			m_clothingPiecePreviews.Clear();
			m_hiddenBodyRenderers.Clear();
			m_diagnostics.Clear();
		}

		private void OnEditorUpdate()
		{
			double now = EditorApplication.timeSinceStartup;
			float delta = (float)Math.Max(0.0, Math.Min(0.1, now - m_lastEditorTime));
			m_lastEditorTime = now;
			float duration = GetCurrentDuration();
			if (!m_playing || duration <= 0f) return;

			m_time += delta * PlaybackSpeeds[Mathf.Clamp(m_playbackSpeedIndex, 0, PlaybackSpeeds.Length - 1)];
			if (m_time > duration)
			{
				if (m_loop) m_time %= duration;
				else
				{
					m_time = duration;
					m_playing = false;
				}
			}
			SampleCurrentClip();
			Repaint();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(6f);
			EditorGUILayout.LabelField("Animation and VFX Preview", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Preview existing player/enemy motion and author portable effects", EditorStyles.miniLabel);
			m_animationUtilityTools = EditorGUILayout.Foldout(m_animationUtilityTools, "Catalog and export tools", true);
			Action refreshCatalog = () =>
			{
				CoreAnimationCatalogExporter.Export(); LoadCatalog(); SelectCurrentClip();
			};
			if (m_animationUtilityTools && position.width < 650f)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button("Refresh catalog")) refreshCatalog();
					if (GUILayout.Button("Export player refs")) NormalizedPlayerAnimationExporter.ExportReference();
				}
				if (GUILayout.Button("Export representative enemy set")) NormalizedCoreEnemyAnimationExporter.ExportRepresentativeSet();
			}
			else if (m_animationUtilityTools) using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Refresh Core animation catalog")) refreshCatalog();
				if (GUILayout.Button("Export normalized player references")) NormalizedPlayerAnimationExporter.ExportReference();
				if (GUILayout.Button("Export representative enemy set")) NormalizedCoreEnemyAnimationExporter.ExportRepresentativeSet();
			}
			EditorGUILayout.Space(4f);

			if (!string.IsNullOrEmpty(m_error))
			{
				EditorGUILayout.HelpBox(m_error, MessageType.Error);
				if (GUILayout.Button("Reload catalog and preview"))
				{
					LoadCatalog();
					CreatePreview();
					SelectCurrentClip();
				}
				return;
			}

			int nextMode = GUILayout.Toolbar(m_mode, position.width < 650f ? new[] { "Player", "Pairs", "Core", "Mods" } : new[] { "Standard player", "Paired finishers", "Core enemies", "Modded enemies" });
			if (nextMode != m_mode)
			{
				m_mode = nextMode;
				m_time = 0f;
				SelectCurrentClip();
			}

			DrawClipSelection();
			if (m_mode == 3)
			{
				DrawModdedEnemySelection();
				if (m_normalizedEnemyClip != null)
				{
					int workspace = GUILayout.Toolbar(m_designStudio ? 1 : 0, new[] { "Animation Preview", "VFX Timeline" });
					bool nextStudio = workspace == 1;
					if (nextStudio != m_designStudio) { m_designStudio = nextStudio; minSize = m_designStudio ? new Vector2(820f, 620f) : new Vector2(540f, 520f); }
					if (m_designStudio)
					{
						DrawAnimationVfxStudio();
						return;
					}
				}
				DrawNormalizedEnemyAuthoring();
			}
			if (m_mode == 2 && m_enemyClip != null)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("Authoring", EditorStyles.boldLabel, GUILayout.Width(70f));
					if (GUILayout.Button("Export selected normalized enemy clip"))
					{
						NormalizedCoreEnemyAnimationExporter.ExportSelected(GetSelectedEnemyId(), GetSelectedEnemySemanticName(), m_enemyClip);
						LoadNormalizedEnemyPreview();
					}
				}
				int nextSource = GUILayout.Toolbar(m_enemyPreviewSource, new[] { "Original Unity clip", "Exported normalized JSON" });
				if (nextSource != m_enemyPreviewSource)
				{
					m_enemyPreviewSource = nextSource;
					if (m_enemyPreviewSource == 1) LoadNormalizedEnemyPreview();
					m_time = Mathf.Clamp(m_time, 0f, GetCurrentDuration());
					SampleCurrentClip();
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField(string.IsNullOrEmpty(m_normalizedEnemyStatus) ? "Normalized export not loaded" : m_normalizedEnemyStatus, EditorStyles.miniLabel);
					if (GUILayout.Button("Reload JSON", GUILayout.Width(90f))) { LoadNormalizedEnemyPreview(); SampleCurrentClip(); }
				}
				DrawNormalizedEnemyAuthoring();
			}
			if (m_mode < 2) DrawClothingControls();
			DrawDiagnosticControls();
			DrawPlaybackControls();

			Rect previewRect = GUILayoutUtility.GetRect(100f, 10000f, 280f, 10000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			DrawPreview(previewRect);

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				string clipPath = m_mode == 3 ? (m_normalizedEnemyClip == null ? "No mod animation" : "Mod: " + m_normalizedEnemyPath) : m_mode == 2
					? (m_enemyClip == null ? "No enemy clip" : "Enemy: " + AssetDatabase.GetAssetPath(m_enemyClip))
					: (m_clip == null ? "No player clip" : "Player: " + AssetDatabase.GetAssetPath(m_clip));
				if (m_mode == 1) clipPath += m_enemyClip == null ? " | Enemy: missing" : " | Enemy: " + AssetDatabase.GetAssetPath(m_enemyClip);
				EditorGUILayout.LabelField(clipPath, EditorStyles.miniLabel, GUILayout.MaxWidth(position.width - 20f));
			}
		}

		private void DrawDiagnosticControls()
		{
			m_showDiagnostics = EditorGUILayout.Foldout(m_showDiagnostics, "Diagnostic overlays", true);
			if (!m_showDiagnostics) return;
			using (new EditorGUI.IndentLevelScope())
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					m_showBoneOverlay = GUILayout.Toggle(m_showBoneOverlay, "Bones", "Button");
					m_showAttachmentOverlay = GUILayout.Toggle(m_showAttachmentOverlay, "Attachments", "Button");
					m_showSpriteBoundsOverlay = GUILayout.Toggle(m_showSpriteBoundsOverlay, "Sprites", "Button");
					m_showCanvasBoundsOverlay = GUILayout.Toggle(m_showCanvasBoundsOverlay, "Canvas", "Button");
					m_showSlotLabels = GUILayout.Toggle(m_showSlotLabels, "Labels", "Button");
				}
				EditorGUILayout.LabelField("Cyan: player bones  Purple: enemy bones/sprites  Yellow: attachment  Magenta: pivot  Green: player sprite", EditorStyles.miniLabel);
				if (m_mode == 1 && m_enemyInstance != null)
				{
					string placement = m_pairAnimation != null && m_pairAnimation.IsAttachToPlayerBone()
						? "Enemy attachment: " + m_pairAnimation.GetBonePlayerToAttachTo() + "  offset " + m_pairAnimation.GetOffsetLocalPositionAttachment()
						: "Enemy placement: " + (m_enemyPositionToPlayer ? "enemy to player" : "player to enemy")
							+ "  center Y: " + (m_enemyPositionToPlayer ? m_centerEnemyY : m_centerPlayerY) + "  inverse-start: " + m_enemyInverseFacing;
					EditorGUILayout.LabelField(placement, EditorStyles.miniLabel);
				}
				string bodyVariant = PlayerPrefs.GetString("ModBodyVariant", string.Empty).Trim();
				EditorGUILayout.LabelField("Body variant: " + (string.IsNullOrEmpty(bodyVariant) ? "default" : bodyVariant), EditorStyles.miniLabel);
				if (!string.IsNullOrEmpty(m_compatibilityNotice)) EditorGUILayout.HelpBox(m_compatibilityNotice, MessageType.Warning);
				foreach (string diagnostic in m_diagnostics) EditorGUILayout.HelpBox(diagnostic, MessageType.Warning);
				if (m_diagnostics.Count == 0 && string.IsNullOrEmpty(m_compatibilityNotice))
					EditorGUILayout.LabelField("No clothing diagnostics", EditorStyles.miniLabel);
			}
		}

		private void DrawClothingControls()
		{
			m_showClothing = EditorGUILayout.Foldout(m_showClothing, "Core clothing", true);
			if (!m_showClothing) return;
			using (new EditorGUI.IndentLevelScope())
			{
				string[] categories = Enum.GetNames(typeof(ClothingCategory));
				int nextCategory = EditorGUILayout.Popup("Category", Mathf.Clamp(m_clothingCategoryIndex, 0, categories.Length - 1), categories);
				if (nextCategory != m_clothingCategoryIndex)
				{
					m_clothingCategoryIndex = nextCategory;
					m_clothingIndex = 0;
				}

				ClothingCategory category = (ClothingCategory)m_clothingCategoryIndex;
				List<ClothingOption> available = m_clothingOptions.Where(i_option => i_option.Category == category).ToList();
				using (new EditorGUILayout.HorizontalScope())
				{
					string[] labels = available.Select(i_option => i_option.DisplayName).ToArray();
					m_clothingIndex = labels.Length == 0 ? 0 : EditorGUILayout.Popup("Item", Mathf.Clamp(m_clothingIndex, 0, labels.Length - 1), labels);
					using (new EditorGUI.DisabledScope(available.Count == 0))
					{
						if (GUILayout.Button("Equip", GUILayout.Width(64f))) EquipPreviewClothing(available[m_clothingIndex]);
					}
				}

				if (m_equippedClothing.Count == 0) EditorGUILayout.LabelField("No clothing equipped", EditorStyles.miniLabel);
				for (int index = 0; index < m_equippedClothing.Count; index++)
				{
					ClothingOption option = m_equippedClothing[index];
					using (new EditorGUILayout.HorizontalScope())
					{
						EditorGUILayout.LabelField(option.Category + " - " + option.DisplayName);
						if (GUILayout.Button("Remove", GUILayout.Width(64f)))
						{
							m_equippedClothing.RemoveAt(index);
							m_compatibilityNotice = string.Empty;
							RebuildPreviewClothing();
							break;
						}
					}
				}
				if (m_equippedClothing.Count > 0 && GUILayout.Button("Clear outfit"))
				{
					m_equippedClothing.Clear();
					m_compatibilityNotice = string.Empty;
					RebuildPreviewClothing();
				}
			}
		}

		private void DrawClipSelection()
		{
			if (m_mode == 3) return;
			if (m_mode == 0)
			{
				List<AnimationClip> clips = GetStandardClips();
				string[] labels = clips.Select(i_clip => i_clip.name).ToArray();
				int next = labels.Length == 0 ? 0 : EditorGUILayout.Popup("Animation", Mathf.Clamp(m_standardClipIndex, 0, labels.Length - 1), labels);
				if (next != m_standardClipIndex)
				{
					m_standardClipIndex = next;
					m_time = 0f;
					SelectCurrentClip();
				}
				return;
			}

			string[] enemies = m_catalog.Enemies.Select(i_enemy => i_enemy.DisplayName).ToArray();
			int nextEnemy = EditorGUILayout.Popup("Enemy", Mathf.Clamp(m_enemyIndex, 0, enemies.Length - 1), enemies);
			if (nextEnemy != m_enemyIndex)
			{
				m_enemyIndex = nextEnemy;
				m_phaseIndex = 0;
				m_enemyClipIndex = 0;
				m_time = 0f;
				SelectCurrentClip();
			}

			EnemyView enemy = m_catalog.Enemies[Mathf.Clamp(m_enemyIndex, 0, m_catalog.Enemies.Count - 1)];
			if (m_mode == 2)
			{
				List<AnimationClipView> clips = enemy.EnemyClips ?? new List<AnimationClipView>();
				string[] clipLabels = clips.Select(i_clip =>
				{
					string semantic = i_clip.SemanticNames == null || i_clip.SemanticNames.Count == 0 ? string.Empty : " [" + string.Join(", ", i_clip.SemanticNames.ToArray()) + "]";
					return i_clip.Name + semantic;
				}).ToArray();
				int nextClip = clipLabels.Length == 0 ? 0 : EditorGUILayout.Popup("Enemy animation", Mathf.Clamp(m_enemyClipIndex, 0, clipLabels.Length - 1), clipLabels);
				if (nextClip != m_enemyClipIndex)
				{
					m_enemyClipIndex = nextClip;
					m_time = 0f;
					SelectCurrentClip();
				}
				return;
			}
			List<InteractionPairView> pairs = enemy.InteractionPairs ?? new List<InteractionPairView>();
			string[] phases = pairs.Select(i_pair => "Phase " + i_pair.Phase + " - " + i_pair.EnemyMotion + " / " + i_pair.PlayerClip).ToArray();
			int nextPhase = phases.Length == 0 ? 0 : EditorGUILayout.Popup("Interaction", Mathf.Clamp(m_phaseIndex, 0, phases.Length - 1), phases);
			if (nextPhase != m_phaseIndex)
			{
				m_phaseIndex = nextPhase;
				m_time = 0f;
				SelectCurrentClip();
			}
		}

		private void DrawPlaybackControls()
		{
			using (new EditorGUI.DisabledScope(m_clip == null && m_enemyClip == null && m_normalizedEnemyClip == null))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					if (GUILayout.Button(m_playing ? "Pause" : "Play", GUILayout.Width(70f)))
					{
					if (!m_playing && !m_loop && m_time >= GetCurrentDuration()) m_time = 0f;
						m_playing = !m_playing;
					}
					if (GUILayout.Button("Restart", GUILayout.Width(70f)))
					{
						m_time = 0f;
						SampleCurrentClip();
					}
					if (GUILayout.Button("< Frame", GUILayout.Width(70f))) StepFrame(-1);
					if (GUILayout.Button("Frame >", GUILayout.Width(70f))) StepFrame(1);
					m_playbackSpeedIndex = EditorGUILayout.Popup(m_playbackSpeedIndex, PlaybackSpeedLabels, GUILayout.Width(62f));
					m_loop = GUILayout.Toggle(m_loop, "Loop", GUILayout.Width(52f));
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					float length = GetCurrentDuration();
					float nextTime = EditorGUILayout.Slider(m_time, 0f, length);
					if (!Mathf.Approximately(nextTime, m_time))
					{
						m_time = nextTime;
						SampleCurrentClip();
					}
					GUILayout.Label(m_time.ToString("0.00") + " / " + length.ToString("0.00") + "s", GUILayout.Width(95f));
				}
				using (new EditorGUILayout.HorizontalScope())
				{
					bool nextFacing = GUILayout.Toggle(m_facingLeft, "Face left", "Button", GUILayout.Width(85f));
					if (nextFacing != m_facingLeft)
					{
						m_facingLeft = nextFacing;
						InvalidateCameraFrame();
						SampleCurrentClip();
					}
					bool nextLock = GUILayout.Toggle(m_lockCameraFraming, "Lock framing", "Button", GUILayout.Width(105f));
					if (nextLock != m_lockCameraFraming)
					{
						m_lockCameraFraming = nextLock;
						InvalidateCameraFrame();
					}
					using (new EditorGUI.DisabledScope(!m_lockCameraFraming))
						if (GUILayout.Button("Reframe", GUILayout.Width(75f))) InvalidateCameraFrame();
					if (m_mode == 1)
					{
						bool nextEffects = GUILayout.Toggle(m_showFinisherEffects, "VFX", "Button", GUILayout.Width(55f));
						if (nextEffects != m_showFinisherEffects)
						{
							m_showFinisherEffects = nextEffects;
							SampleCurrentClip();
						}
					}
					GUILayout.FlexibleSpace();
				}
				if (m_mode == 1 && m_enemyClip != null)
				{
					string eventsAtTime = GetFinisherEffectSummary(m_time);
					EditorGUILayout.LabelField(string.IsNullOrEmpty(eventsAtTime) ? "VFX: no active trigger" : "VFX: " + eventsAtTime, EditorStyles.miniLabel);
				}
			}
		}

		private void StepFrame(int i_direction)
		{
			float duration = GetCurrentDuration();
			if (duration <= 0f) return;
			m_playing = false;
			float normalizedRate = (m_mode == 3 || m_mode == 2 && m_enemyPreviewSource == 1) && m_normalizedEnemyClip != null ? m_normalizedEnemyClip.FrameRate : 0f;
			float frameRate = Mathf.Max(Mathf.Max(m_clip == null ? 0f : m_clip.frameRate, m_enemyClip == null ? 0f : m_enemyClip.frameRate), normalizedRate);
			if (frameRate <= 0f) frameRate = 60f;
			float next = m_time + i_direction / frameRate;
			if (m_loop)
			{
				if (next < 0f) next = Mathf.Max(0f, duration - 1f / frameRate);
				else if (next > duration) next = 0f;
			}
			m_time = Mathf.Clamp(next, 0f, duration);
			SampleCurrentClip();
			Repaint();
		}

		private void SelectCurrentClip()
		{
			m_clip = null;
			m_enemyClip = null;
			m_pairAnimation = null;
			if (m_catalog == null && m_mode != 3) return;
			if (m_playerInstance != null) m_playerInstance.SetActive(m_mode < 2);
			if (m_mode == 3)
			{
				LoadModdedEnemyPreview();
				InvalidateCameraFrame();
				SampleCurrentClip();
				return;
			}
			if (m_mode == 0)
			{
				DestroyEnemyPreview();
				List<AnimationClip> clips = GetStandardClips();
				if (clips.Count > 0) m_clip = clips[Mathf.Clamp(m_standardClipIndex, 0, clips.Count - 1)];
			}
			else if (m_catalog.Enemies.Count > 0)
			{
				int enemyIndex = Mathf.Clamp(m_enemyIndex, 0, m_catalog.Enemies.Count - 1);
				EnemyView enemy = m_catalog.Enemies[enemyIndex];
				EnsureEnemyPreview(enemy, enemyIndex);
				if (m_mode == 2)
				{
					List<AnimationClipView> clips = enemy.EnemyClips ?? new List<AnimationClipView>();
					if (clips.Count > 0)
					{
						AnimationClipView selected = clips[Mathf.Clamp(m_enemyClipIndex, 0, clips.Count - 1)];
						m_enemyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(selected.Asset);
					}
				}
				else
				{
					List<InteractionPairView> pairs = enemy.InteractionPairs ?? new List<InteractionPairView>();
					if (pairs.Count == 0) goto Selected;
					InteractionPairView pair = pairs[Mathf.Clamp(m_phaseIndex, 0, pairs.Count - 1)];
					m_clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(pair.PlayerAsset);
					AnimationClipView enemyClip = (enemy.EnemyClips ?? new List<AnimationClipView>())
						.FirstOrDefault(i_clip => string.Equals(i_clip.Name, pair.EnemyMotion, StringComparison.Ordinal));
					if (enemyClip != null) m_enemyClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(enemyClip.Asset);
					if (m_enemyInstance != null)
						m_pairAnimation = m_enemyInstance.GetComponentsInChildren<RaperAnimation>(true)
							.FirstOrDefault(i_animation => string.Equals(i_animation.GetNameStateAnimationRaper(), pair.EnemyMotion, StringComparison.Ordinal)
								&& string.Equals(i_animation.GetNameStateAnimationPlayer(), pair.PlayerClip, StringComparison.Ordinal));
				}
			}
		Selected:
			m_sampleTarget = ResolveSampleTarget(m_clip);
			m_enemySampleTarget = ResolveSampleTarget(m_enemyInstance, m_enemyAnimator, m_enemyClip);
			if (m_mode == 2) LoadNormalizedEnemyPreview(); else ClearNormalizedEnemyPreview();
			InvalidateCameraFrame();
			SampleCurrentClip();
		}

		private string GetSelectedEnemyId()
		{
			if (m_catalog?.Enemies == null || m_catalog.Enemies.Count == 0) return string.Empty;
			return m_catalog.Enemies[Mathf.Clamp(m_enemyIndex, 0, m_catalog.Enemies.Count - 1)].Id;
		}

		private string GetSelectedEnemySemanticName()
		{
			if (m_catalog?.Enemies == null || m_catalog.Enemies.Count == 0) return string.Empty;
			EnemyView enemy = m_catalog.Enemies[Mathf.Clamp(m_enemyIndex, 0, m_catalog.Enemies.Count - 1)];
			List<AnimationClipView> clips = enemy.EnemyClips ?? new List<AnimationClipView>();
			if (clips.Count == 0) return string.Empty;
			AnimationClipView clip = clips[Mathf.Clamp(m_enemyClipIndex, 0, clips.Count - 1)];
			return clip.SemanticNames != null && clip.SemanticNames.Count > 0 ? clip.SemanticNames[0] : clip.Name;
		}

		private void ClearNormalizedEnemyPreview()
		{
			m_vfxTextureFailures.Clear(); m_vfxTextureMaterials.Clear(); m_vfxRegionSprites.Clear(); m_vfxPreviewMaterial = null;
			m_normalizedEnemyClip = null;
			m_normalizedEnemyPath = string.Empty;
			m_normalizedEnemyStatus = string.Empty;
			m_authoringJson = string.Empty;
			m_authoringValidation = string.Empty;
			m_normalizedEnemyBones.Clear();
			m_normalizedEnemyRenderers.Clear();
		}

		private void DrawModdedEnemySelection()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					EditorGUILayout.LabelField("ANIMATION SOURCE", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(m_modSourceBrowser ? "Close browser" : "Change", GUILayout.Width(92f))) m_modSourceBrowser = !m_modSourceBrowser;
					if (GUILayout.Button("Folder", GUILayout.Width(58f)))
					{
						string path = EditorUtility.OpenFolderPanel("Select mod folder", string.IsNullOrEmpty(m_modPackRoot) ? ProjectRoot() : m_modPackRoot, string.Empty);
						if (!string.IsNullOrEmpty(path)) { m_modPackRoot = path; RefreshModPackContents(); SelectCurrentClip(); m_modSourceBrowser = true; }
					}
					if (GUILayout.Button("Reload", GUILayout.Width(58f))) { RefreshModPackContents(); SelectCurrentClip(); }
				}
				string enemyName = m_modEnemies.Count == 0 ? "No enemy selected" : m_modEnemies[Mathf.Clamp(m_modEnemyIndex, 0, m_modEnemies.Count - 1)].DisplayName;
				List<ModAnimationChoice> matching = MatchingModAnimations();
				string animationName = m_modAnimationIndex == 0 || matching.Count == 0 ? "New animation" : matching[Mathf.Clamp(m_modAnimationIndex - 1, 0, matching.Count - 1)].DisplayName;
				EditorGUILayout.LabelField(enemyName + "  /  " + animationName, EditorStyles.wordWrappedMiniLabel);
				if (!string.IsNullOrEmpty(m_modPackRoot)) EditorGUILayout.LabelField(m_modPackRoot, EditorStyles.miniLabel);
				if (m_modSourceBrowser)
				{
					if (position.width >= 650f)
					{
						using (new EditorGUILayout.HorizontalScope()) { DrawModEnemyBrowser(); DrawModAnimationBrowser(); }
					}
					else { DrawModEnemyBrowser(); DrawModAnimationBrowser(); }
				}
				if (m_modAnimationIndex == 0 && m_modEnemies.Count > 0)
				{
					EditorGUILayout.LabelField("CREATE ANIMATION", EditorStyles.miniBoldLabel);
					m_newClipName = EditorGUILayout.TextField("Name", m_newClipName);
					m_newClipDuration = EditorGUILayout.FloatField("Duration", m_newClipDuration);
					if (GUILayout.Button("Create and open animation")) CreateModdedAnimation();
				}
				if (m_modEnemies.Count == 0 && !string.IsNullOrEmpty(m_modPackRoot)) EditorGUILayout.HelpBox("No enemy JSON files were found in this mod.", MessageType.Warning);
				if (!string.IsNullOrEmpty(m_normalizedEnemyStatus)) EditorGUILayout.LabelField(m_normalizedEnemyStatus, EditorStyles.wordWrappedMiniLabel);
			}
		}

		private void DrawModEnemyBrowser()
		{
			using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(150f)))
			{
				EditorGUILayout.LabelField("1. ENEMY", EditorStyles.miniBoldLabel);
				m_modEnemyBrowserScroll = EditorGUILayout.BeginScrollView(m_modEnemyBrowserScroll, EditorStyles.helpBox, GUILayout.Height(position.width < 650f ? 78f : 112f));
				for (int index = 0; index < m_modEnemies.Count; index++)
				{
					bool selected = index == m_modEnemyIndex;
					if (GUILayout.Toggle(selected, m_modEnemies[index].DisplayName, selected ? EditorStyles.miniButton : EditorStyles.miniButtonLeft) && !selected)
					{
						m_modEnemyIndex = index; m_modAnimationIndex = 0; ApplyModDiscoverySelection(); SelectCurrentClip(); GUI.FocusControl(null);
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}

		private void DrawModAnimationBrowser()
		{
			using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(150f)))
			{
				EditorGUILayout.LabelField("2. ANIMATION", EditorStyles.miniBoldLabel);
				List<ModAnimationChoice> matching = MatchingModAnimations();
				m_modAnimationBrowserScroll = EditorGUILayout.BeginScrollView(m_modAnimationBrowserScroll, EditorStyles.helpBox, GUILayout.Height(position.width < 650f ? 78f : 112f));
				if (GUILayout.Toggle(m_modAnimationIndex == 0, "+ New animation", m_modAnimationIndex == 0 ? EditorStyles.miniButton : EditorStyles.miniButtonLeft) && m_modAnimationIndex != 0)
				{
					m_modAnimationIndex = 0; ApplyModDiscoverySelection(); SelectCurrentClip(); GUI.FocusControl(null);
				}
				for (int index = 0; index < matching.Count; index++)
				{
					bool selected = index + 1 == m_modAnimationIndex;
					if (GUILayout.Toggle(selected, matching[index].DisplayName, selected ? EditorStyles.miniButton : EditorStyles.miniButtonLeft) && !selected)
					{
						m_modAnimationIndex = index + 1; ApplyModDiscoverySelection(); SelectCurrentClip(); GUI.FocusControl(null);
					}
				}
				EditorGUILayout.EndScrollView();
			}
		}

		private void RefreshModPackContents()
		{
			m_modEnemies.Clear(); m_modAnimations.Clear();
			if (string.IsNullOrEmpty(m_modPackRoot)) return;
			string root = FilePath.GetFullPath(m_modPackRoot);
			string manifestPath = FilePath.Combine(root, "manifest.json");
			if (!File.Exists(manifestPath)) { m_normalizedEnemyStatus = "The selected folder has no manifest.json."; return; }
			try
			{
				ModManifest manifest = JsonConvert.DeserializeObject<ModManifest>(File.ReadAllText(manifestPath));
				foreach (string contentRoot in manifest.ContentRoots ?? new List<string> { "content" })
				{
					string directory = FilePath.GetFullPath(FilePath.Combine(root, contentRoot));
					if (!directory.StartsWith(root + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !Directory.Exists(directory)) continue;
					foreach (string file in Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories))
					{
						try
						{
							JObject json = JObject.Parse(File.ReadAllText(file));
							string type = json["type"]?.Value<string>();
							if (type == "enemy") m_modEnemies.Add(new ModEnemyChoice { Id = json["id"]?.Value<string>(), DisplayName = json["displayName"]?.Value<string>() ?? FilePath.GetFileNameWithoutExtension(file), Path = file });
							else if (type == "enemyAnimation") m_modAnimations.Add(new ModAnimationChoice { Id = json["id"]?.Value<string>(), Enemy = json["enemy"]?.Value<string>(), DisplayName = json["displayName"]?.Value<string>() ?? FilePath.GetFileNameWithoutExtension(file), Path = file });
						}
						catch (JsonException) { }
					}
				}
				m_modEnemies.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
				m_modAnimations.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
				m_modEnemyIndex = Mathf.Max(0, m_modEnemies.FindIndex(i_item => string.Equals(i_item.Path, m_modEnemyPath, StringComparison.OrdinalIgnoreCase)));
				List<ModAnimationChoice> matching = MatchingModAnimations();
				int existingAnimation = matching.FindIndex(i_item => string.Equals(i_item.Path, m_modAnimationPath, StringComparison.OrdinalIgnoreCase));
				m_modAnimationIndex = existingAnimation < 0 ? 0 : existingAnimation + 1;
				ApplyModDiscoverySelection();
				m_normalizedEnemyStatus = "Found " + m_modEnemies.Count + " enemies and " + m_modAnimations.Count + " enemy animations.";
			}
			catch (Exception exception) { m_normalizedEnemyStatus = "Could not scan mod: " + exception.Message; }
		}

		private List<ModAnimationChoice> MatchingModAnimations()
		{
			if (m_modEnemies.Count == 0) return new List<ModAnimationChoice>();
			string enemy = m_modEnemies[Mathf.Clamp(m_modEnemyIndex, 0, m_modEnemies.Count - 1)].Id;
			return m_modAnimations.Where(i_item => string.Equals(i_item.Enemy, enemy, StringComparison.Ordinal)).ToList();
		}

		private void ApplyModDiscoverySelection()
		{
			if (m_modEnemies.Count == 0) { m_modEnemyPath = string.Empty; m_modAnimationPath = string.Empty; return; }
			m_modEnemyIndex = Mathf.Clamp(m_modEnemyIndex, 0, m_modEnemies.Count - 1);
			m_modEnemyPath = m_modEnemies[m_modEnemyIndex].Path;
			List<ModAnimationChoice> matching = MatchingModAnimations();
			m_modAnimationIndex = Mathf.Clamp(m_modAnimationIndex, 0, matching.Count);
			m_modAnimationPath = m_modAnimationIndex == 0 ? string.Empty : matching[m_modAnimationIndex - 1].Path;
		}

		private void CreateModdedAnimation()
		{
			try
			{
				if (m_modEnemies.Count == 0) throw new InvalidOperationException("Select a mod and enemy first.");
				string root = FilePath.GetFullPath(m_modPackRoot);
				string manifestPath = FilePath.Combine(root, "manifest.json");
				string packId = JObject.Parse(File.ReadAllText(manifestPath))["id"]?.Value<string>();
				if (!ContentId.IsValidNamespace(packId)) throw new InvalidDataException("The manifest pack ID is invalid.");
				ModEnemyChoice enemy = m_modEnemies[Mathf.Clamp(m_modEnemyIndex, 0, m_modEnemies.Count - 1)];
				string slug = SlugForFile(m_newClipName);
				string destination = FilePath.Combine(FilePath.GetDirectoryName(enemy.Path), slug + ".enemy-animation.json");
				if (File.Exists(destination)) throw new IOException("An animation file named " + FilePath.GetFileName(destination) + " already exists.");
				EnemyDefinitionDocument enemyDocument = JsonConvert.DeserializeObject<EnemyDefinitionDocument>(File.ReadAllText(enemy.Path));
				EnemyBoneDefinition firstBoneDefinition = enemyDocument?.Visual?.Bones?.FirstOrDefault();
				string firstBone = firstBoneDefinition?.Id;
				if (string.IsNullOrEmpty(firstBone)) throw new InvalidDataException("The selected enemy has no authorable bones.");
				float duration = Mathf.Clamp(m_newClipDuration, 0.05f, 60f);
				NormalizedEnemyAnimationDocument animation = new NormalizedEnemyAnimationDocument
				{
					SchemaVersion = 1, Type = "enemyAnimation", Id = packId + ":enemy-animation/" + slug, Enemy = enemy.Id,
					DisplayName = string.IsNullOrWhiteSpace(m_newClipName) ? "New Animation" : m_newClipName.Trim(), DurationSeconds = duration, FrameRate = 60f, Loop = true,
					Tracks = new List<NormalizedNumericTrack> { new NormalizedNumericTrack { Target = "bone/" + firstBone, Property = "position.x", Keys = new List<NormalizedNumericKey> { new NormalizedNumericKey { Time = 0f, Value = firstBoneDefinition.X }, new NormalizedNumericKey { Time = duration, Value = firstBoneDefinition.X } } } }
				};
				string json = JsonConvert.SerializeObject(animation, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				NormalizedEnemyAnimationLoadResult validation = NormalizedEnemyAnimationParser.Parse(json, packId, destination, root);
				if (!validation.Report.IsValid) throw new InvalidDataException(string.Join("\n", validation.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				File.WriteAllText(destination, json);
				JObject enemyJson = JObject.Parse(File.ReadAllText(enemy.Path));
				JObject references = enemyJson["animationRefs"] as JObject ?? new JObject();
				references[slug] = animation.Id;
				enemyJson["animationRefs"] = references;
				File.WriteAllText(enemy.Path, enemyJson.ToString(Formatting.Indented));
				AssetDatabase.Refresh();
				m_modAnimationPath = destination;
				RefreshModPackContents();
				m_modAnimationIndex = MatchingModAnimations().FindIndex(i_item => string.Equals(i_item.Path, destination, StringComparison.OrdinalIgnoreCase)) + 1;
				ApplyModDiscoverySelection();
				SelectCurrentClip();
				m_normalizedEnemyStatus = "Created " + destination + " and linked it as animationRefs.";
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Create enemy animation", exception.Message, "OK"); }
		}

		private void LoadModdedEnemyPreview()
		{
			DestroyEnemyPreview();
			ClearNormalizedEnemyPreview();
			if (string.IsNullOrEmpty(m_modEnemyPath) || !File.Exists(m_modEnemyPath) || m_preview == null) return;
			try
			{
				string directory = FilePath.GetDirectoryName(FilePath.GetFullPath(m_modEnemyPath));
				while (directory != null && !File.Exists(FilePath.Combine(directory, "manifest.json"))) directory = FilePath.GetDirectoryName(directory);
				if (directory == null) throw new InvalidDataException("Enemy file is not inside a mod pack with manifest.json.");
				m_modPackRoot = directory;
				string packId = JObject.Parse(File.ReadAllText(FilePath.Combine(directory, "manifest.json")))["id"]?.Value<string>();
				if (!ContentId.IsValidNamespace(packId)) throw new InvalidDataException("The mod manifest has an invalid ID.");
				string enemyJson = File.ReadAllText(m_modEnemyPath);
				EnemyDefinitionLoadResult parsedEnemy = EnemyDefinitionParser.Parse(enemyJson, packId, m_modEnemyPath);
				if (!parsedEnemy.Report.IsValid) throw new InvalidDataException(string.Join("\n", parsedEnemy.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				EnemyDefinitionDocument enemy = JsonConvert.DeserializeObject<EnemyDefinitionDocument>(enemyJson);
				if (enemy?.Visual == null || enemy.Visual.Type != "originalSkeletonAtlas") throw new InvalidDataException("This preview currently supports originalSkeletonAtlas enemies. Inherited Core enemy visuals can still be edited through Core enemies.");
				string atlasPath = FilePath.GetFullPath(FilePath.Combine(directory, enemy.Visual.Atlas.Replace('/', FilePath.DirectorySeparatorChar)));
				if (!atlasPath.StartsWith(directory + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(atlasPath)) throw new InvalidDataException("The enemy atlas is missing or outside the mod pack: " + enemy.Visual.Atlas);
				Texture2D atlas = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
				if (!atlas.LoadImage(File.ReadAllBytes(atlasPath))) throw new InvalidDataException("Could not decode the enemy atlas PNG.");
				m_modPreviewAssets.Add(atlas);
				m_enemyInstance = new GameObject("ModdedEnemyPreview_" + enemy.DisplayName) { hideFlags = HideFlags.HideAndDontSave };
				m_preview.AddSingleGO(m_enemyInstance);
				Transform skeleton = new GameObject("Skeleton").transform;
				skeleton.SetParent(m_enemyInstance.transform, false);
				List<EnemyBoneDefinition> pending = new List<EnemyBoneDefinition>(enemy.Visual.Bones);
				while (pending.Count > 0)
				{
					int before = pending.Count;
					for (int index = pending.Count - 1; index >= 0; index--)
					{
						EnemyBoneDefinition bone = pending[index];
						if (!string.IsNullOrEmpty(bone.Parent) && !m_normalizedEnemyBones.ContainsKey(bone.Parent)) continue;
						if (!enemy.Visual.Regions.TryGetValue(bone.Region, out AtlasRegionDefinition region)) throw new InvalidDataException("Missing atlas region: " + bone.Region);
						Transform parent = string.IsNullOrEmpty(bone.Parent) ? skeleton : m_normalizedEnemyBones[bone.Parent];
						GameObject part = new GameObject(bone.Id);
						part.transform.SetParent(parent, false);
						part.transform.localPosition = new Vector3(bone.X, bone.Y, 0f);
						part.transform.localRotation = Quaternion.Euler(0f, 0f, bone.Rotation);
						Sprite sprite = Sprite.Create(atlas, new Rect(region.X, region.Y, region.Width, region.Height), new Vector2(bone.PivotX, bone.PivotY), enemy.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
						sprite.name = bone.Region;
						m_modPreviewAssets.Add(sprite);
						m_modSprites[bone.Region] = sprite;
						SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
						renderer.sprite = sprite;
						renderer.sortingOrder = bone.SortingOrder;
						if (m_previewMaterial != null) renderer.sharedMaterial = m_previewMaterial;
						m_normalizedEnemyBones.Add(bone.Id, part.transform);
						m_normalizedEnemyRenderers[bone.Id] = new List<SpriteRenderer> { renderer };
						pending.RemoveAt(index);
					}
						if (pending.Count == before) throw new InvalidDataException("The enemy bone hierarchy has missing parents or a cycle.");
				}
				foreach (KeyValuePair<string, AtlasRegionDefinition> pair in enemy.Visual.Regions)
				{
					if (m_modSprites.ContainsKey(pair.Key)) continue;
					AtlasRegionDefinition area = pair.Value;
					Sprite sprite = Sprite.Create(atlas, new Rect(area.X, area.Y, area.Width, area.Height), new Vector2(0.5f, 0.5f), enemy.Visual.PixelsPerUnit, 0, SpriteMeshType.FullRect);
					sprite.name = pair.Key;
					m_modPreviewAssets.Add(sprite);
					m_modSprites.Add(pair.Key, sprite);
				}
				CaptureEnemyDefaultPose();
				if (string.IsNullOrEmpty(m_modAnimationPath))
				{
					m_normalizedEnemyStatus = "Enemy rig loaded. Choose an animation JSON to preview and edit.";
					return;
				}
				string animationPath = FilePath.GetFullPath(m_modAnimationPath);
				if (!animationPath.StartsWith(directory + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Animation must belong to the selected mod pack.");
				string animationJson = File.ReadAllText(animationPath);
				// Keep broken pack-local assets editable; the authoring reference validator reports them in the window.
				NormalizedEnemyAnimationLoadResult parsedAnimation = NormalizedEnemyAnimationParser.Parse(animationJson, packId, animationPath);
				if (!parsedAnimation.Report.IsValid) throw new InvalidDataException(string.Join("\n", parsedAnimation.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				m_normalizedEnemyClip = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(animationJson);
				if (m_normalizedEnemyClip.Enemy != enemy.Id) throw new InvalidDataException("Animation targets " + m_normalizedEnemyClip.Enemy + ", not " + enemy.Id + ".");
				m_normalizedEnemyPath = animationPath;
				m_authoringJson = animationJson;
				m_savePackRoot = directory;
				ValidateAuthoringDocument();
				m_normalizedEnemyStatus = "Loaded " + enemy.DisplayName + " / " + m_normalizedEnemyClip.DisplayName;
			}
			catch (Exception exception) { m_normalizedEnemyClip = null; m_normalizedEnemyStatus = exception.Message; }
		}

		private void LoadNormalizedEnemyPreview()
		{
			ClearNormalizedEnemyPreview();
			if (m_mode != 2 || m_enemyInstance == null) return;
			string enemyPath = GetSelectedEnemyId();
			enemyPath = enemyPath.StartsWith("core:enemy/", StringComparison.Ordinal) ? enemyPath.Substring("core:enemy/".Length) : SlugForFile(enemyPath);
			string directory = System.IO.Path.Combine(NormalizedEnemyRoot, enemyPath);
			string animationPath = System.IO.Path.Combine(directory, SlugForFile(GetSelectedEnemySemanticName()) + ".json");
			string rigPath = System.IO.Path.Combine(directory, "rig.json");
			m_normalizedEnemyPath = animationPath.Replace('\\', '/');
			if (!File.Exists(animationPath) || !File.Exists(rigPath))
			{
				m_normalizedEnemyStatus = "Missing export - use Export selected normalized enemy clip";
				return;
			}
			try
			{
				NormalizedEnemyAnimationLoadResult parsed = NormalizedEnemyAnimationParser.Parse(File.ReadAllText(animationPath), "core", animationPath);
				if (!parsed.Report.IsValid)
				{
					m_normalizedEnemyStatus = string.Join(" | ", parsed.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
					return;
				}
				m_normalizedEnemyClip = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(File.ReadAllText(animationPath));
				m_authoringJson = JsonConvert.SerializeObject(m_normalizedEnemyClip, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				EnemyRigView rig = JsonConvert.DeserializeObject<EnemyRigView>(File.ReadAllText(rigPath));
				Transform root = string.IsNullOrEmpty(rig.SampleRoot) ? m_enemyInstance.transform : m_enemyInstance.transform.Find(rig.SampleRoot);
				if (root == null) throw new InvalidDataException("Rig sampleRoot was not found on the preview prefab: " + rig.SampleRoot);
				foreach (EnemyRigBoneView bone in rig.Bones ?? new List<EnemyRigBoneView>())
				{
					Transform target = string.IsNullOrEmpty(bone.UnityPath) ? root : root.Find(bone.UnityPath);
					if (target != null && !m_normalizedEnemyBones.ContainsKey(bone.Name)) m_normalizedEnemyBones.Add(bone.Name, target);
					List<SpriteRenderer> renderers = new List<SpriteRenderer>();
					foreach (EnemyRigSpriteView sprite in bone.Sprites ?? new List<EnemyRigSpriteView>())
					{
						Transform spriteTarget = string.IsNullOrEmpty(sprite.UnityPath) ? root : root.Find(sprite.UnityPath);
						SpriteRenderer renderer = spriteTarget == null ? null : spriteTarget.GetComponent<SpriteRenderer>();
						if (renderer != null && !renderers.Contains(renderer)) renderers.Add(renderer);
					}
					if (renderers.Count > 0) m_normalizedEnemyRenderers[bone.Name] = renderers;
				}
				int warningCount = (m_normalizedEnemyClip.Warnings ?? new List<string>()).Count + (rig.Warnings ?? new List<string>()).Count;
				m_normalizedEnemyStatus = "Loaded " + m_normalizedEnemyPath + " - " + m_normalizedEnemyClip.Tracks.Count + " tracks, " + warningCount + " warnings";
				ValidateAuthoringDocument();
			}
			catch (Exception exception)
			{
				ClearNormalizedEnemyPreview();
				m_normalizedEnemyStatus = "Could not load normalized export: " + exception.Message;
			}
		}

		private void DrawNormalizedEnemyAuthoring()
		{
			m_showAuthoring = EditorGUILayout.Foldout(m_showAuthoring, "Normalized timeline authoring", true);
			if (!m_showAuthoring || m_normalizedEnemyClip == null) return;
			m_authoringScroll = EditorGUILayout.BeginScrollView(m_authoringScroll, GUILayout.Height(Mathf.Min(300f, position.height * 0.42f)));
			using (new EditorGUI.IndentLevelScope())
			{
				EditorGUI.BeginChangeCheck();
				string id = EditorGUILayout.TextField("Document ID", m_normalizedEnemyClip.Id);
				string enemy = EditorGUILayout.TextField("Enemy ID", m_normalizedEnemyClip.Enemy);
				string displayName = EditorGUILayout.TextField("Display name", m_normalizedEnemyClip.DisplayName);
				float duration = EditorGUILayout.FloatField("Duration", m_normalizedEnemyClip.DurationSeconds);
				float frameRate = EditorGUILayout.FloatField("Frame rate", m_normalizedEnemyClip.FrameRate);
				bool loop = EditorGUILayout.Toggle("Loop", m_normalizedEnemyClip.Loop);
				if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation metadata", () =>
				{
					m_normalizedEnemyClip.Id = id; m_normalizedEnemyClip.Enemy = enemy; m_normalizedEnemyClip.DisplayName = displayName;
					m_normalizedEnemyClip.DurationSeconds = Mathf.Clamp(duration, 0.0001f, 60f);
					m_normalizedEnemyClip.FrameRate = Mathf.Clamp(frameRate, 1f, 240f); m_normalizedEnemyClip.Loop = loop;
				});

				DrawTimelineMarkers();
				DrawNumericTrackEditor();
				DrawSpriteTrackEditor();
				DrawEventEditor();
				DrawAuthoringValidation();

				using (new EditorGUILayout.HorizontalScope())
				{
					m_savePackRoot = EditorGUILayout.TextField("Mod pack", m_savePackRoot);
					if (m_mode != 3 && GUILayout.Button("Browse", GUILayout.Width(70f))) BrowseForModPack();
					using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(m_authoringValidation)))
						if (GUILayout.Button(m_mode == 3 ? "Save JSON" : "Save to mod", GUILayout.Width(90f))) SaveAnimationToMod();
				}
				EditorGUILayout.LabelField("Undo/redo uses the normal Unity Edit menu and shortcuts.", EditorStyles.miniLabel);
			}
			EditorGUILayout.EndScrollView();
		}

		private void DrawNumericTrackEditor()
		{
			List<NormalizedNumericTrack> tracks = m_normalizedEnemyClip.Tracks ?? (m_normalizedEnemyClip.Tracks = new List<NormalizedNumericTrack>());
			EditorGUILayout.Space(2f); EditorGUILayout.LabelField("Transform / sprite timeline", EditorStyles.boldLabel);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add track")) AddNumericTrack();
				using (new EditorGUI.DisabledScope(tracks.Count <= 1))
					if (GUILayout.Button("Remove selected track")) RecordAuthoringChange("Remove animation track", () => { tracks.RemoveAt(Mathf.Clamp(m_authoringTrackIndex, 0, tracks.Count - 1)); m_authoringTrackIndex = Mathf.Clamp(m_authoringTrackIndex, 0, tracks.Count - 1); });
			}
			if (tracks.Count == 0) { EditorGUILayout.HelpBox("Add at least one numeric track to make this animation valid.", MessageType.Warning); return; }
			string[] labels = tracks.Select(i_track => i_track.Target + " : " + i_track.Property).ToArray();
			m_authoringTrackIndex = EditorGUILayout.Popup("Track", Mathf.Clamp(m_authoringTrackIndex, 0, tracks.Count - 1), labels);
			NormalizedNumericTrack track = tracks[m_authoringTrackIndex];
			string[] bones = AvailableBoneNames();
			string currentBone = track.Target != null && track.Target.Contains("/") ? track.Target.Substring(track.Target.IndexOf('/') + 1) : string.Empty;
			int boneIndex = Mathf.Max(0, Array.IndexOf(bones, currentBone));
			string[] properties = { "position.x", "position.y", "rotation.z", "scale.x", "scale.y", "color.r", "color.g", "color.b", "color.a", "sortingOrder" };
			int propertyIndex = Mathf.Max(0, Array.IndexOf(properties, track.Property));
			EditorGUI.BeginChangeCheck();
			string selectedBone = bones.Length == 0 ? currentBone : bones[EditorGUILayout.Popup("Bone", boneIndex, bones)];
			string property = properties[EditorGUILayout.Popup("Property", propertyIndex, properties)];
			if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation track", () => { track.Target = (property.StartsWith("color.", StringComparison.Ordinal) || property == "sortingOrder" ? "sprite/" : "bone/") + selectedBone; track.Property = property; });
			List<NormalizedNumericKey> keys = track.Keys ?? (track.Keys = new List<NormalizedNumericKey>());
			if (keys.Count > 0)
			{
				m_authoringKeyIndex = EditorGUILayout.IntSlider("Key", Mathf.Clamp(m_authoringKeyIndex, 0, keys.Count - 1), 0, keys.Count - 1);
				NormalizedNumericKey key = keys[m_authoringKeyIndex];
				EditorGUI.BeginChangeCheck();
				float time = EditorGUILayout.FloatField("Key time", key.Time);
				float value = EditorGUILayout.FloatField("Value", key.Value);
				if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation key", () =>
				{
					key.Time = Mathf.Clamp(time, 0f, m_normalizedEnemyClip.DurationSeconds); key.Value = value;
					track.Keys = keys.OrderBy(i_key => i_key.Time).ToList(); m_authoringKeyIndex = track.Keys.IndexOf(key);
				});
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add key at playhead")) RecordAuthoringChange("Add animation key", () =>
				{
					float value = keys.Count == 0 ? 0f : EvaluateNormalizedKeys(keys, Mathf.Clamp(m_time, 0f, m_normalizedEnemyClip.DurationSeconds));
					NormalizedNumericKey key = new NormalizedNumericKey { Time = Mathf.Clamp(m_time, 0f, m_normalizedEnemyClip.DurationSeconds), Value = value };
					keys.Add(key); track.Keys = keys.OrderBy(i_key => i_key.Time).ToList(); m_authoringKeyIndex = track.Keys.IndexOf(key);
				});
				using (new EditorGUI.DisabledScope(keys.Count <= 1))
					if (GUILayout.Button("Remove selected key")) RecordAuthoringChange("Remove animation key", () => { keys.RemoveAt(Mathf.Clamp(m_authoringKeyIndex, 0, keys.Count - 1)); m_authoringKeyIndex = Mathf.Clamp(m_authoringKeyIndex, 0, keys.Count - 1); });
			}
		}

		private void AddNumericTrack()
		{
			List<NormalizedNumericTrack> tracks = m_normalizedEnemyClip.Tracks ?? (m_normalizedEnemyClip.Tracks = new List<NormalizedNumericTrack>());
			string[] bones = AvailableBoneNames();
			if (bones.Length == 0) return;
			string[] properties = { "position.x", "position.y", "rotation.z", "scale.x", "scale.y", "color.r", "color.g", "color.b", "color.a", "sortingOrder" };
			foreach (string bone in bones)
				foreach (string property in properties)
				{
					string target = (property.StartsWith("color.", StringComparison.Ordinal) || property == "sortingOrder" ? "sprite/" : "bone/") + bone;
					if (tracks.Any(i_track => i_track.Target == target && i_track.Property == property)) continue;
					float value = CurrentTrackValue(bone, property);
					RecordAuthoringChange("Add animation track", () =>
					{
						NormalizedNumericTrack added = new NormalizedNumericTrack { Target = target, Property = property, Keys = new List<NormalizedNumericKey> { new NormalizedNumericKey { Time = 0f, Value = value }, new NormalizedNumericKey { Time = m_normalizedEnemyClip.DurationSeconds, Value = value } } };
						tracks.Add(added); m_authoringTrackIndex = tracks.Count - 1;
					});
					return;
				}
		}

		private float CurrentTrackValue(string i_bone, string i_property)
		{
			if (m_normalizedEnemyBones.TryGetValue(i_bone, out Transform bone))
			{
				if (i_property == "position.x") return bone.localPosition.x;
				if (i_property == "position.y") return bone.localPosition.y;
				if (i_property == "rotation.z") return Mathf.DeltaAngle(0f, bone.localEulerAngles.z);
				if (i_property == "scale.x") return bone.localScale.x;
				if (i_property == "scale.y") return bone.localScale.y;
			}
			if (m_normalizedEnemyRenderers.TryGetValue(i_bone, out List<SpriteRenderer> renderers) && renderers.Count > 0 && renderers[0] != null)
			{
				Color color = renderers[0].color;
				if (i_property == "color.r") return color.r;
				if (i_property == "color.g") return color.g;
				if (i_property == "color.b") return color.b;
				if (i_property == "color.a") return color.a;
				if (i_property == "sortingOrder") return renderers[0].sortingOrder;
			}
			return i_property.StartsWith("scale.", StringComparison.Ordinal) || i_property.StartsWith("color.", StringComparison.Ordinal) ? 1f : 0f;
		}

		private void DrawSpriteTrackEditor()
		{
			List<NormalizedObjectTrack> tracks = m_normalizedEnemyClip.ObjectTracks ?? (m_normalizedEnemyClip.ObjectTracks = new List<NormalizedObjectTrack>());
			EditorGUILayout.Space(2f); EditorGUILayout.LabelField("Sprite region timeline", EditorStyles.boldLabel);
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add sprite track"))
				{
					string bone = AvailableBoneNames().FirstOrDefault(); string region = AvailableRegionNames().FirstOrDefault();
					if (!string.IsNullOrEmpty(bone) && !string.IsNullOrEmpty(region)) RecordAuthoringChange("Add sprite track", () => { tracks.Add(new NormalizedObjectTrack { Target = "sprite/" + bone, Property = "sprite", Keys = new List<NormalizedObjectKey> { new NormalizedObjectKey { Time = 0f, Name = region } } }); m_authoringObjectTrackIndex = tracks.Count - 1; });
				}
				using (new EditorGUI.DisabledScope(tracks.Count == 0)) if (GUILayout.Button("Remove sprite track")) RecordAuthoringChange("Remove sprite track", () => { tracks.RemoveAt(Mathf.Clamp(m_authoringObjectTrackIndex, 0, tracks.Count - 1)); m_authoringObjectTrackIndex = Mathf.Clamp(m_authoringObjectTrackIndex, 0, tracks.Count - 1); });
			}
			if (tracks.Count == 0) return;
			m_authoringObjectTrackIndex = EditorGUILayout.Popup("Sprite track", Mathf.Clamp(m_authoringObjectTrackIndex, 0, tracks.Count - 1), tracks.Select(i_track => i_track.Target).ToArray());
			NormalizedObjectTrack track = tracks[m_authoringObjectTrackIndex];
			string[] bones = AvailableBoneNames(); string currentBone = track.Target.StartsWith("sprite/", StringComparison.Ordinal) ? track.Target.Substring(7) : string.Empty;
			int boneIndex = Mathf.Max(0, Array.IndexOf(bones, currentBone));
			EditorGUI.BeginChangeCheck(); string selectedBone = bones.Length == 0 ? currentBone : bones[EditorGUILayout.Popup("Sprite bone", boneIndex, bones)];
			if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Change sprite track bone", () => track.Target = "sprite/" + selectedBone);
			List<NormalizedObjectKey> keys = track.Keys ?? (track.Keys = new List<NormalizedObjectKey>());
			if (keys.Count > 0)
			{
				m_authoringObjectKeyIndex = EditorGUILayout.IntSlider("Sprite key", Mathf.Clamp(m_authoringObjectKeyIndex, 0, keys.Count - 1), 0, keys.Count - 1);
				NormalizedObjectKey key = keys[m_authoringObjectKeyIndex]; string[] regions = AvailableRegionNames(); int regionIndex = Mathf.Max(0, Array.IndexOf(regions, key.Name));
				EditorGUI.BeginChangeCheck(); float time = EditorGUILayout.FloatField("Sprite key time", key.Time); string region = regions.Length == 0 ? key.Name : regions[EditorGUILayout.Popup("Atlas region", regionIndex, regions)];
				if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit sprite key", () => { key.Time = Mathf.Clamp(time, 0f, m_normalizedEnemyClip.DurationSeconds); key.Name = region; key.Asset = null; track.Keys = keys.OrderBy(i_key => i_key.Time).ToList(); m_authoringObjectKeyIndex = track.Keys.IndexOf(key); });
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add sprite key at playhead") && AvailableRegionNames().Length > 0) RecordAuthoringChange("Add sprite key", () => { NormalizedObjectKey key = new NormalizedObjectKey { Time = Mathf.Clamp(m_time, 0f, m_normalizedEnemyClip.DurationSeconds), Name = AvailableRegionNames()[0] }; keys.Add(key); track.Keys = keys.OrderBy(i_key => i_key.Time).ToList(); m_authoringObjectKeyIndex = track.Keys.IndexOf(key); });
				using (new EditorGUI.DisabledScope(keys.Count <= 1)) if (GUILayout.Button("Remove sprite key")) RecordAuthoringChange("Remove sprite key", () => { keys.RemoveAt(Mathf.Clamp(m_authoringObjectKeyIndex, 0, keys.Count - 1)); m_authoringObjectKeyIndex = Mathf.Clamp(m_authoringObjectKeyIndex, 0, keys.Count - 1); });
			}
		}

		private void DrawTimelineMarkers()
		{
			float duration = Mathf.Max(0.0001f, m_normalizedEnemyClip.DurationSeconds);
			Rect rect = GUILayoutUtility.GetRect(10f, 42f, GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect(rect, new Color(0.10f, 0.10f, 0.10f, 1f));
			Rect track = new Rect(rect.x + 8f, rect.y + 19f, Mathf.Max(1f, rect.width - 16f), 8f);
			EditorGUI.DrawRect(track, new Color(0.28f, 0.28f, 0.28f, 1f));
			foreach (EnemyAnimationEventDefinition item in m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>())
			{
				float x = track.x + Mathf.Clamp01(item.Time / duration) * track.width;
				Color old = GUI.color; GUI.color = new Color(1f, 0.68f, 0.2f, 1f);
				GUI.Label(new Rect(x - 5f, track.y - 8f, 11f, 22f), new GUIContent("◆", item.Type + " at " + item.Time.ToString("0.###") + "s"), EditorStyles.miniLabel);
				GUI.color = old;
			}
			foreach (NormalizedEffectTrigger trigger in m_normalizedEnemyClip.EffectTriggers ?? new List<NormalizedEffectTrigger>())
			{
				float x = track.x + Mathf.Clamp01(trigger.Time / duration) * track.width;
				Color old = GUI.color; GUI.color = new Color(0.25f, 0.9f, 1f, 1f);
				GUI.Label(new Rect(x - 5f, track.y + 3f, 11f, 20f), new GUIContent("▲", "Effects: " + string.Join(", ", trigger.Effects.ToArray()) + " at " + trigger.Time.ToString("0.###") + "s"), EditorStyles.miniLabel);
				GUI.color = old;
			}
			float playheadX = track.x + Mathf.Clamp01(m_time / duration) * track.width;
			EditorGUI.DrawRect(new Rect(playheadX, rect.y + 3f, 1f, rect.height - 6f), new Color(1f, 1f, 1f, 0.8f));
			GUI.Label(new Rect(rect.x + 4f, rect.y, rect.width - 8f, 18f), "Events ◆     Effects ▲", EditorStyles.miniLabel);
			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				m_time = Mathf.Clamp01((Event.current.mousePosition.x - track.x) / track.width) * duration;
				m_playing = false; SampleCurrentClip(); Repaint(); Event.current.Use();
			}
		}

		private string[] AvailableBoneNames()
		{
			return m_normalizedEnemyBones.Keys.OrderBy(i_name => i_name, StringComparer.OrdinalIgnoreCase).ToArray();
		}

		private string[] AvailableRegionNames()
		{
			IEnumerable<string> names = m_mode == 3
				? m_modSprites.Keys
				: m_normalizedEnemyRenderers.Values.SelectMany(i_renderers => i_renderers).Where(i_renderer => i_renderer != null && i_renderer.sprite != null).Select(i_renderer => i_renderer.sprite.name);
			return names.Where(i_name => !string.IsNullOrEmpty(i_name)).Distinct(StringComparer.Ordinal).OrderBy(i_name => i_name, StringComparer.OrdinalIgnoreCase).ToArray();
		}

		private void DrawEventEditor()
		{
			List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? (m_normalizedEnemyClip.Events = new List<EnemyAnimationEventDefinition>());
			EditorGUILayout.Space(2f); EditorGUILayout.LabelField("Safe events and cues", EditorStyles.boldLabel);
			if (events.Count > 0)
			{
				string[] labels = events.Select(i_event => i_event.Time.ToString("0.###") + "s  " + i_event.Type + (string.IsNullOrEmpty(i_event.Cue) ? string.Empty : " / " + i_event.Cue)).ToArray();
				m_authoringEventIndex = EditorGUILayout.Popup("Event", Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1), labels);
				EnemyAnimationEventDefinition item = events[m_authoringEventIndex];
				string[] types = { "cue", "attackHit", "impulse", "sound", "cameraShake", "spriteEffect" };
				int typeIndex = Mathf.Max(0, Array.IndexOf(types, item.Type));
				EditorGUI.BeginChangeCheck();
				float time = EditorGUILayout.FloatField("Event time", item.Time);
				string type = types[EditorGUILayout.Popup("Event type", typeIndex, types)];
				string cue = item.Cue; int index = item.Index ?? 0; float amount = item.Amount ?? 0f;
				float x = item.X ?? 0f, y = item.Y ?? 0f, volume = item.Volume ?? 1f, effectDuration = item.DurationSeconds ?? 0.15f, scale = item.Scale ?? 1f;
				bool relative = item.RelativeToFacing ?? true; string file = item.File ?? string.Empty, region = item.Region ?? string.Empty, bone = item.Bone ?? string.Empty; int sorting = item.SortingOrder ?? 100;
				if (type == "cue") { cue = EditorGUILayout.TextField("Cue", cue ?? string.Empty); index = EditorGUILayout.IntField("Index", index); amount = EditorGUILayout.FloatField("Amount", amount); }
				else if (type == "impulse") { x = EditorGUILayout.FloatField("X impulse", x); y = EditorGUILayout.FloatField("Y impulse", y); relative = EditorGUILayout.Toggle("Relative to facing", relative); }
				else if (type == "sound") { file = EditorGUILayout.TextField("Pack-relative WAV/OGG", file); volume = EditorGUILayout.Slider("Volume", volume, 0f, 1f); }
				else if (type == "cameraShake") amount = EditorGUILayout.Slider("Strength", amount, 0f, 1f);
				else if (type == "spriteEffect")
				{
					string[] regions = AvailableRegionNames(); int regionIndex = Mathf.Max(0, Array.IndexOf(regions, region));
					region = regions.Length == 0 ? EditorGUILayout.TextField("Atlas region", region) : regions[EditorGUILayout.Popup("Atlas region", regionIndex, regions)];
					string[] bones = AvailableBoneNames(); int boneIndex = Mathf.Max(0, Array.IndexOf(bones, bone));
					bone = bones.Length == 0 ? EditorGUILayout.TextField("Bone", bone) : bones[EditorGUILayout.Popup("Bone", boneIndex, bones)];
					x = EditorGUILayout.FloatField("Offset X", x); y = EditorGUILayout.FloatField("Offset Y", y);
					effectDuration = EditorGUILayout.FloatField("Duration", effectDuration); scale = EditorGUILayout.FloatField("Scale", scale); sorting = EditorGUILayout.IntField("Sorting order", sorting);
				}
				if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation event", () =>
				{
					item.Time = Mathf.Clamp(time, 0f, m_normalizedEnemyClip.DurationSeconds); item.Type = type;
					item.Cue = type == "cue" ? cue : null; item.Index = type == "cue" ? (int?)Mathf.Clamp(index, 0, 255) : null;
					item.Amount = (type == "cue" && !Mathf.Approximately(amount, 0f)) || type == "cameraShake" ? (float?)amount : null;
					item.X = type == "impulse" || type == "spriteEffect" ? (float?)x : null; item.Y = type == "impulse" || type == "spriteEffect" ? (float?)y : null;
					item.RelativeToFacing = type == "impulse" ? (bool?)relative : null; item.File = type == "sound" ? file : null; item.Volume = type == "sound" ? (float?)volume : null;
					item.Region = type == "spriteEffect" ? region : null; item.Bone = type == "spriteEffect" && !string.IsNullOrEmpty(bone) ? bone : null;
					item.DurationSeconds = type == "spriteEffect" ? (float?)effectDuration : null; item.Scale = type == "spriteEffect" ? (float?)scale : null; item.SortingOrder = type == "spriteEffect" ? (int?)sorting : null;
					m_normalizedEnemyClip.Events = events.OrderBy(i_event => i_event.Time).ToList(); m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(item);
				});
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Add cue at playhead")) RecordAuthoringChange("Add animation cue", () =>
				{
					EnemyAnimationEventDefinition item = new EnemyAnimationEventDefinition { Time = Mathf.Clamp(m_time, 0f, m_normalizedEnemyClip.DurationSeconds), Type = "cue", Cue = "author.event" };
					events.Add(item); m_normalizedEnemyClip.Events = events.OrderBy(i_event => i_event.Time).ToList(); m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(item);
				});
				using (new EditorGUI.DisabledScope(events.Count == 0))
					if (GUILayout.Button("Remove selected event")) RecordAuthoringChange("Remove animation event", () => { events.RemoveAt(Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1)); m_authoringEventIndex = Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1); });
			}
		}

		private void RecordAuthoringChange(string i_label, Action i_change)
		{
			Undo.RecordObject(this, i_label); i_change(); SyncAuthoringJson(); ValidateAuthoringDocument(); EditorUtility.SetDirty(this); SampleCurrentClip(); Repaint();
		}

		private void SyncAuthoringJson()
		{
			m_authoringJson = JsonConvert.SerializeObject(m_normalizedEnemyClip, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
		}

		private void OnAuthoringUndoRedo()
		{
			if (m_designStudio && m_studioEditPlayer && !string.IsNullOrEmpty(m_studioPlayerJson))
			{
				try { m_studioPlayerAnimation = JsonConvert.DeserializeObject<NormalizedPlayerAnimationDocument>(m_studioPlayerJson); ValidateStudioPlayerDocument(); SampleCurrentClip(); Repaint(); }
				catch (JsonException exception) { m_studioPlayerValidation = exception.Message; }
				return;
			}
			if (string.IsNullOrEmpty(m_authoringJson)) return;
			try { m_normalizedEnemyClip = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(m_authoringJson); ValidateAuthoringDocument(); SampleCurrentClip(); Repaint(); }
			catch (JsonException exception) { m_authoringValidation = exception.Message; }
		}

		private void ValidateAuthoringDocument()
		{
			if (m_normalizedEnemyClip == null) { m_authoringReferenceValidation = new ValidationReport(); m_authoringValidation = "No normalized document is loaded."; return; }
			SyncAuthoringJson();
			string pack = ContentId.TryParse(m_normalizedEnemyClip.Id, out ContentId id) ? id.Namespace : "invalid";
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(m_authoringJson, pack, m_normalizedEnemyPath);
			string packRoot = AuthoringPackRoot();
			m_authoringReferenceValidation = EnemyAnimationReferenceValidator.Validate(m_normalizedEnemyClip,
				m_normalizedEnemyBones.Count == 0 ? null : m_normalizedEnemyBones.Keys,
				m_mode == 3 ? m_modSprites.Keys : AvailableRegionNames(), packRoot, m_normalizedEnemyPath);
			IEnumerable<ValidationIssue> errors = result.Report.Issues.Concat(m_authoringReferenceValidation.Issues)
				.Where(i_issue => i_issue.Severity == ValidationSeverity.Error)
				.GroupBy(i_issue => i_issue.Message, StringComparer.Ordinal).Select(i_group => i_group.First());
			m_authoringValidation = string.Join("\n", errors.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray());
		}

		private string AuthoringPackRoot()
		{
			string candidate = m_mode == 3 ? m_modPackRoot : m_savePackRoot;
			if (string.IsNullOrWhiteSpace(candidate)) return null;
			try
			{
				string fullPath = FilePath.GetFullPath(candidate);
				return Directory.Exists(fullPath) && File.Exists(FilePath.Combine(fullPath, "manifest.json")) ? fullPath : null;
			}
			catch (Exception) { return null; }
		}

		private void DrawAuthoringValidation()
		{
			if (string.IsNullOrEmpty(m_authoringValidation))
			{
				EditorGUILayout.HelpBox("Document, rig references, and pack assets are valid.", MessageType.Info);
				return;
			}
			EditorGUILayout.HelpBox(m_authoringValidation, MessageType.Error);
			if (m_authoringReferenceValidation == null || m_authoringReferenceValidation.Issues.Count == 0) return;
			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.Label("Reference fixes retarget missing bones/regions and remove dangling trigger IDs.", EditorStyles.miniLabel);
				using (new EditorGUI.DisabledScope(m_normalizedEnemyBones.Count == 0))
					if (GUILayout.Button("Fix safe references", GUILayout.Width(125f)))
					{
						int changes = 0;
						RecordAuthoringChange("Fix animation references", () => changes = EnemyAnimationReferenceValidator.RepairSafeReferences(
							m_normalizedEnemyClip, m_normalizedEnemyBones.Keys, m_mode == 3 ? m_modSprites.Keys : AvailableRegionNames()));
						if (changes == 0) EditorUtility.DisplayDialog("No automatic fix", "Missing files cannot be guessed. Use Choose pack audio or Choose texture to replace them.", "OK");
					}
			}
		}

		private void BrowseForModPack()
		{
			string selected = EditorUtility.OpenFolderPanel("Choose a mod pack containing manifest.json", ProjectRoot(), string.Empty);
			if (!string.IsNullOrEmpty(selected)) m_savePackRoot = selected;
		}

		private void SaveAnimationToMod()
		{
			try
			{
				string root = System.IO.Path.GetFullPath(m_mode == 3 ? m_modPackRoot : m_savePackRoot);
				string project = ProjectRoot();
				if (!root.StartsWith(project + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("The mod pack must be inside this project folder.");
				string manifestPath = System.IO.Path.Combine(root, "manifest.json");
				if (!File.Exists(manifestPath)) throw new FileNotFoundException("The selected directory has no manifest.json.");
				string pack = JObject.Parse(File.ReadAllText(manifestPath))["id"]?.Value<string>();
				if (!ContentId.IsValidNamespace(pack)) throw new InvalidDataException("The manifest has an invalid pack ID.");
				NormalizedEnemyAnimationDocument copy = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(m_authoringJson);
				if (ContentId.TryParse(copy.Id, out ContentId id) && id.Namespace == "core") copy.Id = pack + ":" + id.Path;
				if (ContentId.TryParse(copy.Enemy, out ContentId enemy) && enemy.Namespace == "core") copy.Enemy = pack + ":" + enemy.Path;
				string json = JsonConvert.SerializeObject(copy, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
				NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(json, pack, manifestPath);
				if (!result.Report.IsValid) throw new InvalidDataException(string.Join("\n", result.Report.Issues.Select(i_issue => i_issue.Message).ToArray()));
				string content = System.IO.Path.Combine(root, "content"); Directory.CreateDirectory(content);
				string destination = m_mode == 3 ? m_normalizedEnemyPath : System.IO.Path.Combine(content, SlugForFile(copy.DisplayName) + ".enemy-animation.json");
				if (m_mode == 3 && !FilePath.GetFullPath(destination).StartsWith(root + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("The animation file is outside the mod pack.");
				if (File.Exists(destination) && !EditorUtility.DisplayDialog("Replace normalized animation?", "A file already exists:\n" + destination, "Replace", "Cancel")) return;
				File.WriteAllText(destination, json); AssetDatabase.Refresh(); m_normalizedEnemyStatus = "Saved animation: " + destination;
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Save normalized animation", exception.Message, "OK"); }
		}

		private static string ProjectRoot() { return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "..")); }

		private static string SlugForFile(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value)) return "unnamed";
			System.Text.StringBuilder result = new System.Text.StringBuilder(); bool separator = false; char previous = '\0';
			foreach (char character in i_value)
			{
				if (char.IsLetterOrDigit(character)) { if ((separator || (char.IsUpper(character) && char.IsLower(previous))) && result.Length > 0) result.Append('-'); result.Append(char.ToLowerInvariant(character)); separator = false; }
				else separator = true;
				previous = character;
			}
			return result.Length == 0 ? "unnamed" : result.ToString();
		}

		private void EnsureEnemyPreview(EnemyView i_enemy, int i_index)
		{
			if (m_enemyInstance != null && m_previewEnemyIndex == i_index) return;
			DestroyEnemyPreview();
			if (m_preview == null || i_enemy == null || string.IsNullOrEmpty(i_enemy.Prefab)) return;
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(i_enemy.Prefab);
			if (prefab == null) return;

			m_enemyInstance = Instantiate(prefab);
			m_enemyInstance.name = "EnemyAnimationPreview_" + i_enemy.DisplayName;
			m_enemyInstance.hideFlags = HideFlags.HideAndDontSave;
			m_enemyInstance.transform.position = Vector3.zero;
			m_preview.AddSingleGO(m_enemyInstance);
			RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(i_enemy.Controller);
			m_enemyAnimator = m_enemyInstance.GetComponentsInChildren<Animator>(true)
				.FirstOrDefault(i_animator => i_animator.runtimeAnimatorController == controller)
				?? m_enemyInstance.GetComponentsInChildren<Animator>(true).FirstOrDefault();
			NPC previewEnemy = m_enemyInstance.GetComponentInChildren<NPC>(true);
			m_enemyFeetOffset = previewEnemy == null ? 0f : GetFeetOffset(previewEnemy);
			foreach (Animator animator in m_enemyInstance.GetComponentsInChildren<Animator>(true)) animator.enabled = false;
			foreach (MonoBehaviour behavior in m_enemyInstance.GetComponentsInChildren<MonoBehaviour>(true)) behavior.enabled = false;
			foreach (Collider2D colliderValue in m_enemyInstance.GetComponentsInChildren<Collider2D>(true)) colliderValue.enabled = false;
			foreach (Rigidbody2D body in m_enemyInstance.GetComponentsInChildren<Rigidbody2D>(true)) body.simulated = false;
			foreach (SpriteRenderer renderer in m_enemyInstance.GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (m_previewMaterial != null) renderer.sharedMaterial = m_previewMaterial;
				// Raper.BeginRape calls SkeletonActor.SetToRape(), promoting the enemy
				// artwork from Actor to Player so numeric orders can interleave with Alex.
				// Applying it to the whole isolated enemy also covers auxiliary finisher
				// props such as the Goblin's stick and syringe.
				renderer.sortingLayerName = "Player";
			}
			Raper raper = m_enemyInstance.GetComponentInChildren<Raper>(true);
			if (raper != null)
			{
				SerializedObject serializedRaper = new SerializedObject(raper);
				m_enemyPositionToPlayer = ReadBool(serializedRaper, "m_isPositionToPlayer");
				m_centerEnemyY = ReadBool(serializedRaper, "m_isCenterYRaper");
				m_centerPlayerY = ReadBool(serializedRaper, "m_isCenterYPlayer");
				m_enemyInverseFacing = ReadBool(serializedRaper, "m_isInverseFlipX");
			}
			CaptureEnemyDefaultPose();
			m_previewEnemyIndex = i_index;
		}

		private void DestroyEnemyPreview()
		{
			ClearPreviewEffects();
			if (m_enemyInstance != null) DestroyImmediate(m_enemyInstance);
			foreach (UnityEngine.Object asset in m_modPreviewAssets) if (asset != null) DestroyImmediate(asset);
			m_modPreviewAssets.Clear();
			m_modSprites.Clear();
			m_enemyInstance = null;
			m_enemyAnimator = null;
			m_enemySampleTarget = null;
			m_enemyClip = null;
			m_pairAnimation = null;
			m_previewEnemyIndex = -1;
			m_enemyPositionToPlayer = false;
			m_centerEnemyY = false;
			m_centerPlayerY = false;
			m_enemyInverseFacing = false;
			m_enemyFeetOffset = 0f;
			m_enemyDefaultTransforms.Clear();
			m_enemyDefaultRenderers.Clear();
			m_enemyDefaultSortingGroups.Clear();
		}

		private static bool ReadBool(SerializedObject i_object, string i_name)
		{
			SerializedProperty property = i_object.FindProperty(i_name);
			return property != null && property.boolValue;
		}

		private static List<AnimationClip> GetStandardClips()
		{
			RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerPath);
			return controller == null
				? new List<AnimationClip>()
				: controller.animationClips.Where(i_clip => i_clip != null).Distinct().OrderBy(i_clip => i_clip.name, StringComparer.Ordinal).ToList();
		}

		private void CaptureDefaultPose()
		{
			m_defaultTransforms.Clear();
			m_defaultRenderers.Clear();
			m_defaultSortingGroups.Clear();
			if (m_playerInstance == null) return;
			foreach (Transform transformValue in m_playerInstance.GetComponentsInChildren<Transform>(true))
				m_defaultTransforms.Add(new TransformSnapshot(transformValue));
			foreach (SpriteRenderer renderer in m_playerInstance.GetComponentsInChildren<SpriteRenderer>(true))
				m_defaultRenderers.Add(new RendererSnapshot(renderer));
			foreach (SortingGroup group in m_playerInstance.GetComponentsInChildren<SortingGroup>(true))
				m_defaultSortingGroups.Add(new SortingSnapshot(group));
		}

		private void RestoreDefaultPose()
		{
			foreach (TransformSnapshot snapshot in m_defaultTransforms) snapshot.Restore();
			foreach (RendererSnapshot snapshot in m_defaultRenderers) snapshot.Restore();
			foreach (SortingSnapshot snapshot in m_defaultSortingGroups) snapshot.Restore();
			foreach (TransformSnapshot snapshot in m_enemyDefaultTransforms) snapshot.Restore();
			foreach (RendererSnapshot snapshot in m_enemyDefaultRenderers) snapshot.Restore();
			foreach (SortingSnapshot snapshot in m_enemyDefaultSortingGroups) snapshot.Restore();
		}

		private void CaptureEnemyDefaultPose()
		{
			m_enemyDefaultTransforms.Clear();
			m_enemyDefaultRenderers.Clear();
			m_enemyDefaultSortingGroups.Clear();
			if (m_enemyInstance == null) return;
			foreach (Transform transformValue in m_enemyInstance.GetComponentsInChildren<Transform>(true))
				m_enemyDefaultTransforms.Add(new TransformSnapshot(transformValue));
			foreach (SpriteRenderer renderer in m_enemyInstance.GetComponentsInChildren<SpriteRenderer>(true))
				m_enemyDefaultRenderers.Add(new RendererSnapshot(renderer));
			foreach (SortingGroup group in m_enemyInstance.GetComponentsInChildren<SortingGroup>(true))
				m_enemyDefaultSortingGroups.Add(new SortingSnapshot(group));
		}

		private void EquipPreviewClothing(ClothingOption i_option)
		{
			if (i_option == null || m_equippedClothing.Contains(i_option)) return;
			List<ClothingOption> removed = m_equippedClothing.Where(i_existing =>
				i_existing.Clothing == null || i_option.Clothing == null || !i_option.Clothing.IsCompatibleWithClothing(i_existing.Clothing)).ToList();
			foreach (ClothingOption existing in removed) m_equippedClothing.Remove(existing);
			m_compatibilityNotice = removed.Count == 0 ? string.Empty : i_option.DisplayName + " replaced incompatible: "
				+ string.Join(", ", removed.Select(i_removed => i_removed.DisplayName).ToArray());
			m_equippedClothing.Add(i_option);
			RebuildPreviewClothing();
		}

		private void RebuildPreviewClothing()
		{
			foreach (GameObject piece in m_previewClothingPieces)
				if (piece != null) DestroyImmediate(piece);
			m_previewClothingPieces.Clear();
			m_clothingPiecePreviews.Clear();
			m_hiddenBodyRenderers.Clear();
			m_diagnostics.Clear();
			RestoreDefaultPose();
			if (m_playerInstance == null || m_skeletonPlayer == null) return;

			foreach (ClothingOption option in m_equippedClothing)
			{
				if (option?.Prefab == null) continue;
				Dictionary<string, Sprite> expectedSprites = option.Prefab.GetComponentsInChildren<ClothingPiece>(true)
					.Select(i_piece => new { Slot = GetClothingSlot(i_piece), Renderer = i_piece.GetComponent<SpriteRenderer>() })
					.Where(i_item => i_item.Renderer != null && i_item.Renderer.sprite != null)
					.GroupBy(i_item => i_item.Slot, StringComparer.Ordinal)
					.ToDictionary(i_group => i_group.Key, i_group => i_group.First().Renderer.sprite, StringComparer.Ordinal);
				int expectedPieceCount = option.Prefab.GetComponentsInChildren<ClothingPiece>(true).Length;
				int attachedPieceCount = 0;
				GameObject clothingRoot = Instantiate(option.Prefab, m_playerInstance.transform);
				clothingRoot.name = "PreviewClothing_" + option.Prefab.name;
				clothingRoot.hideFlags = HideFlags.HideAndDontSave;
				ClothingPiece[] pieces = clothingRoot.GetComponentsInChildren<ClothingPiece>(true);
				foreach (ClothingPiece piece in pieces)
				{
					piece.gameObject.SetActive(false);
					BonePlayer bone = FindPlayerBone(piece.GetBoneToAttachTo());
					if (bone == null || bone.GetBodyPart() == null)
					{
						m_diagnostics.Add(option.DisplayName + ": missing player bone for " + GetClothingSlot(piece) + " (" + piece.GetBoneToAttachTo() + ").");
						DestroyImmediate(piece.gameObject);
						continue;
					}

					Transform parent = piece.IsAttachToBoneInsteadOfBodyPart() ? bone.transform : bone.GetBodyPart().transform;
					piece.transform.SetParent(parent, false);
					piece.transform.localPosition = piece.GetLocalPosition();
					piece.transform.localRotation = Quaternion.Euler(0f, 0f, piece.GetLocalEulerAngleZ());
					piece.transform.localScale = Vector3.one;
					piece.enabled = false;
					foreach (MonoBehaviour behavior in piece.GetComponents<MonoBehaviour>()) behavior.enabled = false;
					foreach (Collider2D colliderValue in piece.GetComponents<Collider2D>()) colliderValue.enabled = false;
					foreach (Rigidbody2D body in piece.GetComponents<Rigidbody2D>()) body.simulated = false;

					SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
					string slot = GetClothingSlot(piece);
					Sprite expectedSprite = expectedSprites.TryGetValue(slot, out Sprite expected) ? expected : null;
					if (renderer != null)
					{
						if (m_previewMaterial != null) renderer.sharedMaterial = m_previewMaterial;
						int sortingOrder = 1 + piece.GetSortingNumberClothingPiece();
						if (piece.IsAttachToBoneInsteadOfBodyPart())
						{
							SortingGroup bodySorting = bone.GetBodyPart().GetComponent<SortingGroup>();
							if (bodySorting != null) sortingOrder += bodySorting.sortingOrder;
						}
						renderer.sortingOrder = sortingOrder;
					}
					else m_diagnostics.Add(option.DisplayName + ": " + slot + " has no SpriteRenderer.");
					if (renderer != null && renderer.sprite == null) m_diagnostics.Add(option.DisplayName + ": " + slot + " has no sprite.");
					if (piece.IsHideBodyPartAttachedTo())
					{
						SpriteRenderer bodyRenderer = bone.GetBodyPart().GetComponent<SpriteRenderer>();
						if (bodyRenderer != null && !m_hiddenBodyRenderers.Contains(bodyRenderer)) m_hiddenBodyRenderers.Add(bodyRenderer);
					}
					piece.gameObject.SetActive(true);
					m_previewClothingPieces.Add(piece.gameObject);
					ClothingPiecePreview previewPiece = new ClothingPiecePreview(piece, bone, slot, renderer, expectedSprite);
					m_clothingPiecePreviews.Add(previewPiece);
					if (previewPiece.ExtendsExpectedCanvas)
						m_diagnostics.Add(option.DisplayName + ": " + slot + " artwork exceeds its "
							+ expectedSprite.rect.width.ToString("0") + "x" + expectedSprite.rect.height.ToString("0") + " template canvas.");
					attachedPieceCount++;
				}
				DestroyImmediate(clothingRoot);
				if (attachedPieceCount != expectedPieceCount)
					m_diagnostics.Add(option.DisplayName + ": attached " + attachedPieceCount + " of " + expectedPieceCount + " clothing pieces.");
				ValidateBodyVariant(option);
			}
			InvalidateCameraFrame();
			SampleCurrentClip();
			Repaint();
		}

		private static string GetClothingSlot(ClothingPiece i_piece)
		{
			ModClothingSlotIdentity identity = i_piece.GetComponent<ModClothingSlotIdentity>();
			return identity == null ? ClothingSlotCatalog.FromCorePieceName(i_piece.name) : identity.Slot;
		}

		private BonePlayer[] GetPlayerBones()
		{
			return m_playerInstance == null
				? new BonePlayer[0]
				: m_playerInstance.GetComponentsInChildren<BonePlayer>(true);
		}

		private BonePlayer FindPlayerBone(BoneTypePlayer i_type)
		{
			return GetPlayerBones().FirstOrDefault(i_bone => i_bone.GetBoneType() == i_type);
		}

		private void ValidateBodyVariant(ClothingOption i_option)
		{
			string selected = PlayerPrefs.GetString("ModBodyVariant", string.Empty).Trim().ToLowerInvariant();
			if (string.IsNullOrEmpty(selected) || selected == "default") return;
			ModClothingVariantSprite[] variants = i_option.Prefab.GetComponentsInChildren<ModClothingVariantSprite>(true);
			bool supported = variants.Any(i_variant => HasBodyVariant(i_variant, selected));
			if (!supported) m_diagnostics.Add(i_option.DisplayName + ": no explicit '" + selected + "' body variant; using its baseline artwork.");
		}

		private static bool HasBodyVariant(ModClothingVariantSprite i_variants, string i_name)
		{
			SerializedProperty names = new SerializedObject(i_variants).FindProperty("m_variantNames");
			if (names == null) return false;
			for (int index = 0; index < names.arraySize; index++)
				if (string.Equals(names.GetArrayElementAtIndex(index).stringValue, i_name, StringComparison.OrdinalIgnoreCase)) return true;
			return false;
		}

		private void SampleCurrentClip()
		{
			SampleClipAt(m_time);
		}

		private void SampleClipAt(float i_time)
		{
			RestoreDefaultPose();
			if (m_clip != null && m_sampleTarget != null)
				m_clip.SampleAnimation(m_sampleTarget, Mathf.Clamp(i_time, 0f, m_clip.length));
			if ((m_mode == 3 || m_mode == 2 && m_enemyPreviewSource == 1) && m_normalizedEnemyClip != null)
				ApplyNormalizedEnemyAnimation(i_time);
			else if (m_enemyClip != null && m_enemySampleTarget != null)
				m_enemyClip.SampleAnimation(m_enemySampleTarget, Mathf.Clamp(i_time, 0f, m_enemyClip.length));
			if (m_mode == 3) SampleStudioPairedPlayer(i_time);
			Player previewPlayer = m_playerInstance == null ? null : m_playerInstance.GetComponentInChildren<Player>(true);
			if (previewPlayer != null)
			{
				Vector3 scale = previewPlayer.transform.localScale;
				bool facingLeft = m_mode == 3 && m_designStudio ? m_studioPlayerFacingLeft : m_facingLeft;
				scale.x = Mathf.Abs(scale.x) * (facingLeft ? -1f : 1f);
				previewPlayer.transform.localScale = scale;
			}
			NPC previewEnemy = m_enemyInstance == null ? null : m_enemyInstance.GetComponentInChildren<NPC>(true);
			if (previewEnemy != null)
			{
				Vector3 scale = previewEnemy.transform.localScale;
				bool facingLeft = m_mode == 3 && m_designStudio ? m_studioEnemyFacingLeft : m_facingLeft;
				scale.x = Mathf.Abs(scale.x) * (facingLeft ? -1f : 1f);
				previewEnemy.transform.localScale = scale;
			}
			ApplyPairPlacement();
			ApplyAnimatedSorting(m_clip, m_sampleTarget);
			if (m_mode < 2 || m_mode == 2 && m_enemyPreviewSource == 0) ApplyAnimatedSorting(m_enemyClip, m_enemySampleTarget);
			SampleFinisherEffects(i_time);
			if (m_mode == 3) SampleAuthoredVfx(i_time);
			ApplyClothingSorting();
			foreach (SpriteRenderer renderer in m_hiddenBodyRenderers)
				if (renderer != null) renderer.enabled = false;
		}

		private void ApplyNormalizedEnemyAnimation(float i_time)
		{
			float time = Mathf.Clamp(i_time, 0f, m_normalizedEnemyClip.DurationSeconds);
			foreach (NormalizedNumericTrack track in m_normalizedEnemyClip.Tracks ?? new List<NormalizedNumericTrack>())
			{
				if (track == null || string.IsNullOrEmpty(track.Target) || track.Keys == null || track.Keys.Count == 0) continue;
				int slash = track.Target.IndexOf('/');
				if (slash < 0) continue;
				string kind = track.Target.Substring(0, slash), name = track.Target.Substring(slash + 1);
				float value = EvaluateNormalizedKeys(track.Keys, time);
				if (kind == "bone" && m_normalizedEnemyBones.TryGetValue(name, out Transform bone))
				{
					Vector3 position = bone.localPosition, scale = bone.localScale, rotation = bone.localEulerAngles;
					if (track.Property == "position.x") position.x = value;
					else if (track.Property == "position.y") position.y = value;
					else if (track.Property == "rotation.z") rotation.z = value;
					else if (track.Property == "scale.x") scale.x = value;
					else if (track.Property == "scale.y") scale.y = value;
					bone.localPosition = position; bone.localEulerAngles = rotation; bone.localScale = scale;
				}
				else if (kind == "sprite" && m_normalizedEnemyRenderers.TryGetValue(name, out List<SpriteRenderer> renderers))
					foreach (SpriteRenderer renderer in renderers)
					{
						Color color = renderer.color;
						if (track.Property == "color.r") color.r = value;
						else if (track.Property == "color.g") color.g = value;
						else if (track.Property == "color.b") color.b = value;
						else if (track.Property == "color.a") color.a = value;
						else if (track.Property == "sortingOrder") renderer.sortingOrder = Mathf.RoundToInt(value);
						renderer.color = color;
					}
			}
			foreach (NormalizedObjectTrack track in m_normalizedEnemyClip.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				if (track == null || !track.Target.StartsWith("sprite/", StringComparison.Ordinal) || track.Keys == null) continue;
				NormalizedObjectKey selected = track.Keys.LastOrDefault(i_key => i_key.Time <= time + 0.0001f);
				if (selected == null || !m_normalizedEnemyRenderers.TryGetValue(track.Target.Substring(7), out List<SpriteRenderer> renderers)) continue;
				Sprite sprite = LoadReferenceSprite(selected);
				if (sprite != null) foreach (SpriteRenderer renderer in renderers) renderer.sprite = sprite;
			}
		}

		private static float EvaluateNormalizedKeys(IReadOnlyList<NormalizedNumericKey> i_keys, float i_time)
		{
			if (i_keys.Count == 1 || i_time <= i_keys[0].Time) return i_keys[0].Value;
			if (i_time >= i_keys[i_keys.Count - 1].Time) return i_keys[i_keys.Count - 1].Value;
			int upper = 1; while (upper < i_keys.Count && i_keys[upper].Time < i_time) upper++;
			NormalizedNumericKey left = i_keys[upper - 1], right = i_keys[upper];
			float duration = right.Time - left.Time, t = duration <= 0f ? 1f : (i_time - left.Time) / duration;
			if (!left.OutTangent.HasValue || !right.InTangent.HasValue) return Mathf.LerpUnclamped(left.Value, right.Value, t);
			float t2 = t * t, t3 = t2 * t;
			return (2f * t3 - 3f * t2 + 1f) * left.Value + (t3 - 2f * t2 + t) * duration * left.OutTangent.Value
				+ (-2f * t3 + 3f * t2) * right.Value + (t3 - t2) * duration * right.InTangent.Value;
		}

		private Sprite LoadReferenceSprite(NormalizedObjectKey i_key)
		{
			if (m_mode == 3 && !string.IsNullOrEmpty(i_key.Name) && m_modSprites.TryGetValue(i_key.Name, out Sprite modSprite)) return modSprite;
			if (!string.IsNullOrEmpty(i_key.Asset))
			{
				Sprite direct = AssetDatabase.LoadAssetAtPath<Sprite>(i_key.Asset);
				if (direct != null && (string.IsNullOrEmpty(i_key.Name) || direct.name == i_key.Name)) return direct;
				Sprite nested = AssetDatabase.LoadAllAssetsAtPath(i_key.Asset).OfType<Sprite>().FirstOrDefault(i_sprite => i_sprite.name == i_key.Name);
				if (nested != null) return nested;
			}
			return null;
		}

		private void SampleFinisherEffects(float i_time)
		{
			if (m_enemyInstance == null) return;
			ClearPreviewEffects();
			foreach (ParticleSystem particles in m_enemyInstance.GetComponentsInChildren<ParticleSystem>(true))
				particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			if (!m_showFinisherEffects || m_samplingCameraBounds || m_pairAnimation == null || m_enemyClip == null) return;

			AnimationEvent[] events = AnimationUtility.GetAnimationEvents(m_enemyClip);
			CreateEffectPreviews(m_pairAnimation.GetParticlesThrust(), events
				.Where(i_event => i_event.functionName == "Thrust").Select(i_event => i_event.time), i_time);
			CreateEffectPreviews(m_pairAnimation.GetParticlesCumThrust(), events
				.Where(i_event => i_event.functionName == "CumThrust").Select(i_event => i_event.time), i_time);

			IList<RapeParticleSystem> unique = m_pairAnimation.GetParticlesUnique();
			for (int index = 0; index < unique.Count; index++)
			{
				int expected = index;
				CreateEffectPreviews(new[] { unique[index] }, events
					.Where(i_event => i_event.functionName == "PlayParticleUnique"
						&& Mathf.Max(0, i_event.intParameter - 1) == expected)
					.Select(i_event => i_event.time), i_time);
			}
		}

		private void CreateEffectPreviews(IEnumerable<RapeParticleSystem> i_markers,
			IEnumerable<float> i_triggerTimes, float i_time)
		{
			float[] triggers = i_triggerTimes.Where(i_trigger => i_trigger <= i_time + 0.0001f).OrderBy(i_trigger => i_trigger).ToArray();
			if (triggers.Length == 0) return;
			foreach (RapeParticleSystem marker in i_markers)
			{
				ParticleSystem source = marker == null ? null : marker.GetParticleSystem();
				Transform parent = ResolveEffectParent(marker);
				if (source == null || parent == null) continue;
				foreach (float trigger in triggers)
				{
					float elapsed = Mathf.Max(0f, i_time - trigger);
					if (elapsed > source.main.duration + 5f) continue;
					ParticleSystem particles = Instantiate(source, parent);
					particles.name = "PreviewVFX_" + source.name;
					particles.gameObject.hideFlags = HideFlags.HideAndDontSave;
					particles.transform.localPosition = Vector3.zero;
					particles.transform.localEulerAngles = Vector3.zero;
					particles.transform.localScale = source.transform.localScale;
					particles.gameObject.SetActive(true);
					// Runtime duplicates receive independent automatic random seeds. Cloning
					// several historical triggers in an editor preview otherwise gives every
					// system the same seed, stacking identical particles on top of each other
					// and making the effect look far denser than it does in game.
					uint seed = (uint)Mathf.Max(1, Mathf.Abs(source.GetInstanceID()) + Mathf.RoundToInt(trigger * 1000f));
					foreach (ParticleSystem system in particles.GetComponentsInChildren<ParticleSystem>(true))
					{
						system.useAutoRandomSeed = false;
						system.randomSeed = seed++;
					}
					particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
					particles.Play(true);
					if (elapsed > 0f) particles.Simulate(elapsed, true, false, false);
					particles.Pause(true);
					m_effectPreviewInstances.Add(particles.gameObject);
				}
			}
		}

		private Transform ResolveEffectParent(RapeParticleSystem i_marker)
		{
			if (i_marker == null) return null;
			SerializedObject serialized = new SerializedObject(i_marker);
			Bone enemyBone = serialized.FindProperty("m_boneRaperToParentTo")?.objectReferenceValue as Bone;
			if (enemyBone != null) return enemyBone.transform;
			SerializedProperty playerBone = serialized.FindProperty("m_bonePlayerToParentTo");
			return playerBone == null ? null : FindPlayerBone((BoneTypePlayer)playerBone.intValue)?.transform;
		}

		private void ClearPreviewEffects()
		{
			foreach (GameObject effect in m_effectPreviewInstances)
				if (effect != null) DestroyImmediate(effect);
			m_effectPreviewInstances.Clear();
		}

		private string GetFinisherEffectSummary(float i_time)
		{
			if (!m_showFinisherEffects || m_enemyClip == null) return m_showFinisherEffects ? string.Empty : "hidden";
			const float displayWindow = 0.2f;
			return string.Join(", ", AnimationUtility.GetAnimationEvents(m_enemyClip)
				.Where(i_event => IsFinisherEffectEvent(i_event.functionName)
					&& i_event.time <= i_time + 0.0001f && i_time - i_event.time <= displayWindow)
				.Select(i_event => i_event.functionName == "PlayParticleUnique"
					? i_event.functionName + " " + Mathf.Max(1, i_event.intParameter)
					: i_event.functionName)
				.ToArray());
		}

		private static bool IsFinisherEffectEvent(string i_name)
		{
			return i_name == "Thrust" || i_name == "CumThrust" || i_name == "PlayParticleUnique";
		}

		private static void ApplyAnimatedSorting(AnimationClip i_clip, GameObject i_targetRoot)
		{
			if (i_clip == null || i_targetRoot == null) return;
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(i_clip))
			{
				bool bodyPartOrder = binding.type == typeof(BodyPartPlayer) && binding.propertyName == "m_sortingOrder";
				bool rendererOrder = binding.type == typeof(SpriteRenderer) && binding.propertyName == "m_SortingOrder";
				if (!bodyPartOrder && !rendererOrder) continue;
				Transform target = string.IsNullOrEmpty(binding.path) ? i_targetRoot.transform : i_targetRoot.transform.Find(binding.path);
				if (target == null) continue;
				SortingGroup sorting = target.GetComponent<SortingGroup>();
				if (sorting == null) continue;
				if (bodyPartOrder)
				{
					BodyPartPlayer bodyPart = target.GetComponent<BodyPartPlayer>();
					SerializedProperty order = bodyPart == null ? null : new SerializedObject(bodyPart).FindProperty("m_sortingOrder");
					if (order != null) sorting.sortingOrder = order.intValue;
				}
				else
				{
					SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
					if (renderer != null) sorting.sortingOrder = renderer.sortingOrder;
				}
			}
		}

		private void ApplyClothingSorting()
		{
			foreach (ClothingPiecePreview piece in m_clothingPiecePreviews)
			{
				if (piece.Piece == null || piece.Renderer == null || !piece.Piece.IsAttachToBoneInsteadOfBodyPart()) continue;
				SortingGroup bodySorting = piece.Bone.GetBodyPart() == null ? null : piece.Bone.GetBodyPart().GetComponent<SortingGroup>();
				piece.Renderer.sortingOrder = 1 + piece.Piece.GetSortingNumberClothingPiece() + (bodySorting == null ? 0 : bodySorting.sortingOrder);
			}
		}

		private void ApplyPairPlacement()
		{
			if (m_mode != 1 || m_playerInstance == null || m_enemyInstance == null) return;
			Player player = m_playerInstance.GetComponentInChildren<Player>(true);
			NPC enemy = m_enemyInstance.GetComponentInChildren<NPC>(true);
			if (player == null || enemy == null) return;
			if (m_pairAnimation != null && m_pairAnimation.IsAttachToPlayerBone())
			{
				BonePlayer attachmentBone = FindPlayerBone(m_pairAnimation.GetBonePlayerToAttachTo());
				if (attachmentBone != null)
				{
					enemy.transform.position = attachmentBone.transform.TransformPoint(m_pairAnimation.GetOffsetLocalPositionAttachment());
					enemy.transform.rotation = attachmentBone.transform.rotation * Quaternion.Euler(m_pairAnimation.GetOffsetLocalEulerAnglesAttachment());
					return;
				}
			}
			Vector3 playerPosition = player.transform.position;
			Vector3 enemyPosition = enemy.transform.position;
			if (m_enemyPositionToPlayer)
			{
				enemyPosition.x = playerPosition.x;
				enemyPosition.y = m_centerEnemyY ? playerPosition.y : playerPosition.y - m_playerFeetOffset + m_enemyFeetOffset;
			}
			else
			{
				playerPosition.x = enemyPosition.x;
				playerPosition.y = m_centerPlayerY ? enemyPosition.y : enemyPosition.y - m_enemyFeetOffset + m_playerFeetOffset;
				player.transform.position = playerPosition;
			}
			enemyPosition.z = player.transform.position.z - 0.1f;
			enemy.transform.position = enemyPosition;
		}

		private static float GetFeetOffset(Actor i_actor)
		{
			Collider2D colliderValue = i_actor.GetComponent<Collider2D>();
			if (colliderValue != null) return Mathf.Max(0f, i_actor.transform.position.y - colliderValue.bounds.min.y);
			SpriteRenderer renderer = i_actor.GetComponent<SpriteRenderer>();
			return renderer == null ? 0f : Mathf.Max(0f, i_actor.transform.position.y - renderer.bounds.min.y);
		}

		private void InvalidateCameraFrame()
		{
			m_hasLockedBounds = false;
		}

		private GameObject ResolveSampleTarget(AnimationClip i_clip)
		{
			return ResolveSampleTarget(m_playerInstance, m_playerAnimator, i_clip);
		}

		private static GameObject ResolveSampleTarget(GameObject i_root, Animator i_animator, AnimationClip i_clip)
		{
			if (i_clip == null || i_animator == null) return null;
			List<GameObject> candidates = new List<GameObject> { i_animator.gameObject };
			Transform skeletonPlayer = i_animator.transform.Find("SkeletonPlayer");
			if (skeletonPlayer != null) candidates.Add(skeletonPlayer.gameObject);
			Transform parent = i_animator.transform.parent;
			if (parent != null && !candidates.Contains(parent.gameObject)) candidates.Add(parent.gameObject);
			if (i_root != null && !candidates.Contains(i_root)) candidates.Add(i_root);

			string[] paths = AnimationUtility.GetCurveBindings(i_clip).Select(i_binding => i_binding.path)
				.Concat(AnimationUtility.GetObjectReferenceCurveBindings(i_clip).Select(i_binding => i_binding.path))
				.Distinct(StringComparer.Ordinal)
				.ToArray();
			return candidates
				.OrderByDescending(i_candidate => paths.Count(i_path => string.IsNullOrEmpty(i_path) || i_candidate.transform.Find(i_path) != null))
				.First();
		}

		private void DrawPreview(Rect i_rect)
		{
			if (m_preview == null || m_playerInstance == null || Event.current.type != EventType.Repaint) return;
			Bounds bounds = GetCameraBounds();
			float aspect = Mathf.Max(0.1f, i_rect.width / Mathf.Max(1f, i_rect.height));
			float verticalSize = Mathf.Max(1.5f, bounds.extents.y * 1.35f + 0.25f);
			verticalSize = Mathf.Max(verticalSize, bounds.extents.x / aspect * 1.35f + 0.25f);
			if (m_mode == 3 && m_designStudio) verticalSize /= Mathf.Max(0.4f, m_studioViewportZoom);
			m_preview.camera.orthographicSize = verticalSize;
			Vector2 studioPan = m_mode == 3 && m_designStudio ? m_studioViewportPan : Vector2.zero;
			m_preview.camera.transform.position = new Vector3(bounds.center.x + studioPan.x, bounds.center.y + studioPan.y, -10f);
			m_preview.camera.transform.rotation = Quaternion.identity;

			m_preview.BeginPreview(i_rect, GUIStyle.none);
			m_preview.camera.Render();
			Texture texture = m_preview.EndPreview();
			GUI.DrawTexture(i_rect, texture, ScaleMode.StretchToFill, false);
			DrawDiagnosticOverlays(i_rect);
		}

		private void DrawDiagnosticOverlays(Rect i_rect)
		{
			if (!m_showBoneOverlay && !m_showAttachmentOverlay && !m_showSpriteBoundsOverlay
				&& !m_showCanvasBoundsOverlay && !m_showSlotLabels) return;
			Handles.BeginGUI();
			if (m_showSpriteBoundsOverlay)
			{
				foreach (SpriteRenderer renderer in m_playerInstance.GetComponentsInChildren<SpriteRenderer>(true))
					if (renderer.enabled && renderer.sprite != null) DrawSpriteOutline(i_rect, renderer.transform, renderer.sprite, new Color(0.2f, 1f, 0.35f, 0.72f));
				if (m_enemyInstance != null)
					foreach (SpriteRenderer renderer in m_enemyInstance.GetComponentsInChildren<SpriteRenderer>(true))
						if (renderer.enabled && renderer.sprite != null) DrawSpriteOutline(i_rect, renderer.transform, renderer.sprite, new Color(0.8f, 0.25f, 1f, 0.82f));
			}

			if (m_showCanvasBoundsOverlay)
			{
				foreach (ClothingPiecePreview piece in m_clothingPiecePreviews)
				{
					if (piece.Piece == null || piece.ExpectedSprite == null) continue;
					Color color = piece.ExtendsExpectedCanvas ? new Color(1f, 0.15f, 0.15f, 1f) : new Color(1f, 0.6f, 0.1f, 0.9f);
					DrawSpriteOutline(i_rect, piece.Piece.transform, piece.ExpectedSprite, color);
				}
			}

			if (m_showBoneOverlay && m_skeletonPlayer != null)
			{
				foreach (BonePlayer bone in GetPlayerBones())
				{
					if (bone == null) continue;
					Vector2 point = WorldToGui(i_rect, bone.transform.position);
					BonePlayer parent = bone.transform.parent == null ? null : bone.transform.parent.GetComponentInParent<BonePlayer>();
					Handles.color = new Color(0.15f, 0.9f, 1f, 0.9f);
					if (parent != null && parent != bone) Handles.DrawAAPolyLine(2f, point, WorldToGui(i_rect, parent.transform.position));
					DrawCross(point, 3f);
					GUI.Label(new Rect(point.x + 4f, point.y - 8f, 110f, 16f), bone.GetBoneType().ToString(), EditorStyles.miniLabel);
				}
			}
			if (m_showBoneOverlay && m_enemyInstance != null)
			{
				foreach (Bone bone in m_enemyInstance.GetComponentsInChildren<Bone>(true))
				{
					if (bone == null) continue;
					Vector2 point = WorldToGui(i_rect, bone.transform.position);
					Bone parent = bone.transform.parent == null ? null : bone.transform.parent.GetComponentInParent<Bone>();
					Handles.color = new Color(0.8f, 0.25f, 1f, 0.95f);
					if (parent != null && parent != bone) Handles.DrawAAPolyLine(2f, point, WorldToGui(i_rect, parent.transform.position));
					DrawCross(point, 3f);
					GUI.Label(new Rect(point.x + 4f, point.y - 8f, 150f, 16f), "enemy:" + bone.name, EditorStyles.miniLabel);
				}
			}

			if (m_showAttachmentOverlay && m_pairAnimation != null && m_pairAnimation.IsAttachToPlayerBone() && m_enemyInstance != null)
			{
				BonePlayer attachmentBone = FindPlayerBone(m_pairAnimation.GetBonePlayerToAttachTo());
				NPC enemy = m_enemyInstance.GetComponentInChildren<NPC>(true);
				if (attachmentBone != null && enemy != null)
				{
					Vector2 anchor = WorldToGui(i_rect, attachmentBone.transform.position);
					Vector2 pivot = WorldToGui(i_rect, enemy.transform.position);
					Handles.color = new Color(1f, 0.85f, 0.15f, 1f);
					Handles.DrawAAPolyLine(2f, anchor, pivot);
					DrawCross(anchor, 4f);
					Handles.color = new Color(1f, 0.25f, 0.9f, 1f);
					DrawCross(pivot, 4f);
				}
			}

			foreach (ClothingPiecePreview piece in m_clothingPiecePreviews)
			{
				if (piece.Piece == null) continue;
				Vector2 pivot = WorldToGui(i_rect, piece.Piece.transform.position);
				if (m_showAttachmentOverlay)
				{
					Vector3 anchorWorld = piece.Piece.transform.parent == null ? piece.Piece.transform.position : piece.Piece.transform.parent.position;
					Vector2 anchor = WorldToGui(i_rect, anchorWorld);
					Handles.color = new Color(1f, 0.85f, 0.15f, 1f);
					Handles.DrawAAPolyLine(2f, anchor, pivot);
					DrawCross(anchor, 4f);
					Handles.color = new Color(1f, 0.25f, 0.9f, 1f);
					DrawCross(pivot, 4f);
				}
				if (m_showSlotLabels)
				{
					int sorting = piece.Renderer == null ? 0 : piece.Renderer.sortingOrder;
					GUI.Label(new Rect(pivot.x + 5f, pivot.y + 3f, 260f, 18f), piece.Slot + " | " + piece.Bone.GetBoneType() + " | sort " + sorting, EditorStyles.miniLabel);
				}
			}
			if (m_showSlotLabels && m_enemyInstance != null)
			{
				foreach (SpriteRenderer renderer in m_enemyInstance.GetComponentsInChildren<SpriteRenderer>(true))
				{
					if (!renderer.enabled || renderer.sprite == null) continue;
					Vector2 point = WorldToGui(i_rect, renderer.bounds.max);
					SortingGroup group = renderer.GetComponent<SortingGroup>();
					int sorting = group == null ? renderer.sortingOrder : group.sortingOrder;
					GUI.Label(new Rect(point.x + 3f, point.y - 9f, 280f, 18f), "enemy:" + renderer.name + " | " + renderer.sortingLayerName + ":" + sorting, EditorStyles.miniLabel);
				}
			}
			Handles.EndGUI();
		}

		private void DrawSpriteOutline(Rect i_rect, Transform i_transform, Sprite i_sprite, Color i_color)
		{
			Bounds bounds = i_sprite.bounds;
			Vector2 bottomLeft = WorldToGui(i_rect, i_transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, 0f)));
			Vector2 topLeft = WorldToGui(i_rect, i_transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, 0f)));
			Vector2 topRight = WorldToGui(i_rect, i_transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, 0f)));
			Vector2 bottomRight = WorldToGui(i_rect, i_transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, 0f)));
			Handles.color = i_color;
			Handles.DrawAAPolyLine(2f, bottomLeft, topLeft, topRight, bottomRight, bottomLeft);
		}

		private Vector2 WorldToGui(Rect i_rect, Vector3 i_world)
		{
			Vector3 viewport = m_preview.camera.WorldToViewportPoint(i_world);
			return new Vector2(i_rect.x + viewport.x * i_rect.width, i_rect.y + (1f - viewport.y) * i_rect.height);
		}

		private static void DrawCross(Vector2 i_point, float i_size)
		{
			Handles.DrawAAPolyLine(2f, new Vector2(i_point.x - i_size, i_point.y), new Vector2(i_point.x + i_size, i_point.y));
			Handles.DrawAAPolyLine(2f, new Vector2(i_point.x, i_point.y - i_size), new Vector2(i_point.x, i_point.y + i_size));
		}

		private Bounds GetCameraBounds()
		{
			float duration = GetCurrentDuration();
			if (!m_lockCameraFraming || duration <= 0f) return CalculateSpriteBounds();
			if (m_hasLockedBounds) return m_lockedBounds;

			float frameRate = Mathf.Max(m_clip == null ? 0f : m_clip.frameRate, m_enemyClip == null ? 0f : m_enemyClip.frameRate);
			int samples = Mathf.Clamp(Mathf.CeilToInt(duration * Mathf.Min(30f, Mathf.Max(1f, frameRate))), 2, 90);
			m_samplingCameraBounds = true;
			try
			{
				for (int index = 0; index <= samples; index++)
				{
					SampleClipAt(duration * index / samples);
					Bounds sample = CalculateSpriteBounds();
					if (index == 0) m_lockedBounds = sample;
					else m_lockedBounds.Encapsulate(sample);
				}
			}
			finally { m_samplingCameraBounds = false; }
			m_hasLockedBounds = true;
			SampleCurrentClip();
			return m_lockedBounds;
		}

		private Bounds CalculateSpriteBounds()
		{
			IEnumerable<SpriteRenderer> source = m_playerInstance.GetComponentsInChildren<SpriteRenderer>(true);
			if (m_enemyInstance != null) source = source.Concat(m_enemyInstance.GetComponentsInChildren<SpriteRenderer>(true));
			SpriteRenderer[] renderers = source
				.Where(i_renderer => i_renderer.enabled && i_renderer.sprite != null)
				.ToArray();
			if (renderers.Length == 0) return new Bounds(Vector3.zero, new Vector3(2f, 3f, 0f));
			Bounds result = renderers[0].bounds;
			for (int index = 1; index < renderers.Length; index++) result.Encapsulate(renderers[index].bounds);
			return result;
		}

		private float GetCurrentDuration()
		{
			if ((m_mode == 3 || m_mode == 2 && m_enemyPreviewSource == 1) && m_normalizedEnemyClip != null) return m_normalizedEnemyClip.DurationSeconds;
			return Mathf.Max(m_clip == null ? 0f : m_clip.length, m_enemyClip == null ? 0f : m_enemyClip.length);
		}

		private sealed class ModEnemyChoice
		{
			public string Id;
			public string DisplayName;
			public string Path;
		}

		private sealed class ModAnimationChoice
		{
			public string Id;
			public string Enemy;
			public string DisplayName;
			public string Path;
		}

		private sealed class TransformSnapshot
		{
			private readonly Transform m_target;
			private readonly Vector3 m_position;
			private readonly Quaternion m_rotation;
			private readonly Vector3 m_scale;

			public TransformSnapshot(Transform i_target)
			{
				m_target = i_target;
				m_position = i_target.localPosition;
				m_rotation = i_target.localRotation;
				m_scale = i_target.localScale;
			}

			public void Restore()
			{
				if (m_target == null) return;
				m_target.localPosition = m_position;
				m_target.localRotation = m_rotation;
				m_target.localScale = m_scale;
			}
		}

		private sealed class RendererSnapshot
		{
			private readonly SpriteRenderer m_target;
			private readonly Sprite m_sprite;
			private readonly Material m_material;
			private readonly Color m_color;
			private readonly bool m_enabled;
			private readonly bool m_flipX;
			private readonly bool m_flipY;
			private readonly int m_sortingOrder;

			public RendererSnapshot(SpriteRenderer i_target)
			{
				m_target = i_target;
				m_sprite = i_target.sprite;
				m_material = i_target.sharedMaterial;
				m_color = i_target.color;
				m_enabled = i_target.enabled;
				m_flipX = i_target.flipX;
				m_flipY = i_target.flipY;
				m_sortingOrder = i_target.sortingOrder;
			}

			public void Restore()
			{
				if (m_target == null) return;
				m_target.sprite = m_sprite;
				m_target.sharedMaterial = m_material;
				m_target.color = m_color;
				m_target.enabled = m_enabled;
				m_target.flipX = m_flipX;
				m_target.flipY = m_flipY;
				m_target.sortingOrder = m_sortingOrder;
			}
		}

		private sealed class SortingSnapshot
		{
			private readonly SortingGroup m_target;
			private readonly int m_sortingOrder;

			public SortingSnapshot(SortingGroup i_target)
			{
				m_target = i_target;
				m_sortingOrder = i_target.sortingOrder;
			}

			public void Restore()
			{
				if (m_target != null) m_target.sortingOrder = m_sortingOrder;
			}
		}

		private sealed class ClothingOption
		{
			public string Id;
			public string DisplayName;
			public ClothingCategory Category;
			public GameObject Prefab;
			public Clothing Clothing;
		}

		private sealed class ClothingPiecePreview
		{
			public readonly ClothingPiece Piece;
			public readonly BonePlayer Bone;
			public readonly string Slot;
			public readonly SpriteRenderer Renderer;
			public readonly Sprite ExpectedSprite;
			public bool ExtendsExpectedCanvas => Renderer != null && Renderer.sprite != null && ExpectedSprite != null
				&& (Renderer.sprite.rect.width > ExpectedSprite.rect.width + 0.01f
					|| Renderer.sprite.rect.height > ExpectedSprite.rect.height + 0.01f);

			public ClothingPiecePreview(ClothingPiece i_piece, BonePlayer i_bone, string i_slot,
				SpriteRenderer i_renderer, Sprite i_expectedSprite)
			{
				Piece = i_piece;
				Bone = i_bone;
				Slot = i_slot;
				Renderer = i_renderer;
				ExpectedSprite = i_expectedSprite;
			}
		}

		[Serializable]
		private sealed class CatalogView
		{
			[JsonProperty("schemaVersion")] public int SchemaVersion;
			[JsonProperty("enemies")] public List<EnemyView> Enemies = new List<EnemyView>();
		}

		[Serializable]
		private sealed class EnemyView
		{
			[JsonProperty("id")] public string Id;
			[JsonProperty("displayName")] public string DisplayName;
			[JsonProperty("prefab")] public string Prefab;
			[JsonProperty("controller")] public string Controller;
			[JsonProperty("enemyClips")] public List<AnimationClipView> EnemyClips = new List<AnimationClipView>();
			[JsonProperty("interactionPairs")] public List<InteractionPairView> InteractionPairs = new List<InteractionPairView>();
		}

		[Serializable]
		private sealed class AnimationClipView
		{
			[JsonProperty("name")] public string Name;
			[JsonProperty("asset")] public string Asset;
			[JsonProperty("semanticNames")] public List<string> SemanticNames = new List<string>();
		}

		[Serializable]
		private sealed class EnemyRigView
		{
			[JsonProperty("sampleRoot")] public string SampleRoot;
			[JsonProperty("bones")] public List<EnemyRigBoneView> Bones = new List<EnemyRigBoneView>();
			[JsonProperty("warnings")] public List<string> Warnings = new List<string>();
		}

		[Serializable]
		private sealed class EnemyRigBoneView
		{
			[JsonProperty("name")] public string Name;
			[JsonProperty("unityPath")] public string UnityPath;
			[JsonProperty("sprites")] public List<EnemyRigSpriteView> Sprites = new List<EnemyRigSpriteView>();
		}

		[Serializable]
		private sealed class EnemyRigSpriteView
		{
			[JsonProperty("unityPath")] public string UnityPath;
		}

		[Serializable]
		private sealed class InteractionPairView
		{
			[JsonProperty("phase")] public int Phase;
			[JsonProperty("enemyMotion")] public string EnemyMotion;
			[JsonProperty("playerClip")] public string PlayerClip;
			[JsonProperty("playerAsset")] public string PlayerAsset;
		}

		[Serializable]
		private sealed class CoreClothingCatalogView
		{
			[JsonProperty("entries")] public List<CoreClothingEntryView> Entries = new List<CoreClothingEntryView>();
		}

		[Serializable]
		private sealed class CoreClothingEntryView
		{
			[JsonProperty("id")] public string Id;
			[JsonProperty("legacyId")] public int LegacyId;
			[JsonProperty("category")] public string Category;
		}
	}
}
