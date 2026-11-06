using System;
using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Networking;

// Plays mod clips in place of a Core enemy's Animator states (CoreAnimationOverrides: an asset patch's
// "animations" / "playerAnimations", or an inherited enemy's animationRefs / playerAnimationRefs).
//
// The Animator keeps running the enemy: its states, transitions and Unity animation events (attack hits, grab
// steps) are untouched. After it has posed the rig each frame, LateUpdate looks up the semantic name of the state
// each layer is in; when that name has an override, it samples the override at the state's normalizedTime scaled
// to the override's own length (wrapped when the override loops, clamped when it does not) and writes the bones'
// local position, rotation and scale and their renderers' sprite, sorting order and colour. During this enemy's
// grab it does the same for the player's side of the current finisher-phase-N, timed by the player's Animator.
// Properties an override wrote are put back to their rest values in Update, before the Animator runs, so nothing
// the Animator does not drive stays bent once the override stops.
[DefaultExecutionOrder(-50)]
public sealed class CoreAnimationOverrideDriver : MonoBehaviour
{
	private sealed class Bone
	{
		public Transform Transform;
		public SpriteRenderer Renderer;
		public BodyPartPlayer BodyPart;
		public Vector3 Position, Scale, Euler;
		public Color Color;
		public int Sorting;
		public Sprite Sprite;
		public Touched Written;
	}

	[Flags]
	private enum Touched { None = 0, Position = 1, Rotation = 2, Scale = 4, Color = 8, Sorting = 16, Sprite = 32 }

	private sealed class Playback
	{
		public CoreAnimationOverrideClip Clip;
		public AnimationCurveSampler.Cursor Cursor;
		public int Cycle;
		public float Time = -1f;
		public int NextEvent, NextTrigger;
	}

	private NPC m_npc;
	private Animator m_animator;
	private CoreAnimationRuntimeEnemy m_rig;
	private CoreAnimationOverrideSet m_set;
	private readonly Dictionary<string, Bone> m_bones = new Dictionary<string, Bone>(StringComparer.Ordinal);
	private Dictionary<int, string>[] m_layerSemantics;
	private Playback[] m_layers;
	private readonly Playback m_playerPlayback = new Playback();
	private Player m_player;
	private readonly Dictionary<string, Bone> m_playerBones = new Dictionary<string, Bone>(StringComparer.Ordinal);
	private AudioSource m_audio;
	private static readonly Dictionary<string, Sprite> s_spritesByName = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private static readonly Dictionary<string, AudioClip> s_audio = new Dictionary<string, AudioClip>(StringComparer.OrdinalIgnoreCase);
	private static readonly HashSet<string> s_audioLoading = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public string CurrentSemantic { get; private set; }
	public bool IsOverriding { get; private set; }
	public bool IsOverridingPlayer { get; private set; }

	// Puts the driver on a Core enemy template (spawned copies and inherited clones carry it). It turns itself off
	// on enemies without overrides.
	public static void Attach(NPC i_npc)
	{
		if (i_npc != null && i_npc.GetComponent<CoreAnimationOverrideDriver>() == null) i_npc.gameObject.AddComponent<CoreAnimationOverrideDriver>();
	}

	private void Start()
	{
		if (!Configure()) enabled = false;
	}

	// Finds this enemy's overrides and its rig. False when it has none.
	public bool Configure()
	{
		m_npc = GetComponent<NPC>();
		m_animator = m_npc == null ? null : m_npc.GetAnimator();
		if (m_npc == null || m_animator == null || !RuntimeContentIdentity.TryResolve(this, out ContentId id, out _)) return false;
		string templateId = id.Namespace == "core" ? id.ToString() : null;
		if (templateId == null)
			foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
				if (definition.Id == id && definition.Extends.HasValue) { templateId = definition.Extends.Value.ToString(); break; }
		if (templateId == null || !CoreAnimationRuntimeMap.Load().TryGet(templateId, out m_rig)) return false;
		return Configure(CoreAnimationOverrides.For(id.ToString(), templateId), m_rig);
	}

	public bool Configure(CoreAnimationOverrideSet i_set, CoreAnimationRuntimeEnemy i_rig)
	{
		if (m_npc == null) m_npc = GetComponent<NPC>();
		if (m_animator == null && m_npc != null) m_animator = m_npc.GetAnimator();
		if (m_animator == null) m_animator = GetComponentInChildren<Animator>(true);
		m_set = i_set;
		m_rig = i_rig;
		if (m_set == null || m_set.IsEmpty || m_rig == null || m_animator == null) return false;
		m_bones.Clear();
		Transform root = m_animator.transform;
		foreach (CoreAnimationRuntimeBone rigBone in m_rig.Bones)
		{
			Transform target = string.IsNullOrEmpty(rigBone.Path) ? root : root.Find(rigBone.Path);
			if (target == null) continue;
			SpriteRenderer renderer = !string.IsNullOrEmpty(rigBone.Renderer) ? root.Find(rigBone.Renderer)?.GetComponent<SpriteRenderer>() : null;
			if (renderer == null) renderer = target.GetComponent<SpriteRenderer>();
			m_bones[rigBone.Name] = Capture(target, renderer, null);
		}
		int layers = m_animator.runtimeAnimatorController == null ? 0 : m_animator.layerCount;
		m_layerSemantics = new Dictionary<int, string>[layers];
		m_layers = new Playback[layers];
		for (int layer = 0; layer < layers; layer++) { m_layerSemantics[layer] = new Dictionary<int, string>(); m_layers[layer] = new Playback(); }
		foreach (CoreAnimationRuntimeState state in m_rig.States)
		{
			int layer = m_animator.GetLayerIndex(state.Layer);
			if (layer < 0 || layer >= layers) continue;
			m_layerSemantics[layer][Animator.StringToHash(state.Layer + "." + state.Path)] = state.Semantic;
		}
		foreach (CoreAnimationOverrideClip clip in m_set.Enemy.Values) LoadAudio(clip);
		return true;
	}

	private static Bone Capture(Transform i_transform, SpriteRenderer i_renderer, BodyPartPlayer i_bodyPart)
	{
		return new Bone
		{
			Transform = i_transform, Renderer = i_renderer, BodyPart = i_bodyPart,
			Position = i_transform.localPosition, Scale = i_transform.localScale, Euler = i_transform.localEulerAngles,
			Color = i_renderer == null ? Color.white : i_renderer.color,
			Sorting = i_bodyPart != null ? i_bodyPart.GetSortingOrder() : i_renderer == null ? 0 : i_renderer.sortingOrder,
			Sprite = i_renderer == null ? null : i_renderer.sprite
		};
	}

	private void Update()
	{
		Rest(m_bones);
		Rest(m_playerBones);
	}

	private void LateUpdate()
	{
		if (m_animator == null || m_layers == null || !m_animator.isActiveAndEnabled) return;
		IsOverriding = false;
		CurrentSemantic = null;
		for (int layer = 0; layer < m_layers.Length; layer++)
		{
			AnimatorStateInfo info = m_animator.GetCurrentAnimatorStateInfo(layer);
			m_layerSemantics[layer].TryGetValue(info.fullPathHash, out string semantic);
			if (layer == 0) CurrentSemantic = semantic;
			Playback playback = m_layers[layer];
			if (semantic == null || !m_set.Enemy.TryGetValue(semantic, out CoreAnimationOverrideClip clip)) { playback.Clip = null; continue; }
			Advance(playback, clip, info.normalizedTime, m_bones, false);
			IsOverriding = true;
		}
		UpdatePlayer();
	}

	private void UpdatePlayer()
	{
		IsOverridingPlayer = false;
		if (m_set.Player.Count == 0) return;
		Raper raper = m_npc == null ? null : m_npc.GetRaper();
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (raper == null || player == null || !raper.GetIsRaping() || player.GetRaperCurrent() != raper
			|| CurrentSemantic == null || !m_set.Player.TryGetValue(CurrentSemantic, out CoreAnimationOverrideClip clip))
		{
			m_playerPlayback.Clip = null;
			return;
		}
		if (m_player != player || m_playerBones.Count == 0) CapturePlayer(player);
		Animator animator = player.GetAnimator();
		if (animator == null) return;
		Advance(m_playerPlayback, clip, animator.GetCurrentAnimatorStateInfo(0).normalizedTime, m_playerBones, true);
		IsOverridingPlayer = true;
	}

	private void CapturePlayer(Player i_player)
	{
		m_player = i_player;
		m_playerBones.Clear();
		SkeletonPlayer skeleton = i_player.GetSkeletonPlayer();
		if (skeleton == null) return;
		foreach (string name in new[] { "hips", "butt", "spine", "chest", "neck", "head", "arm-right-upper", "arm-right-lower", "hand-right", "arm-left-upper",
			"arm-left-lower", "hand-left", "leg-right-upper", "leg-right-lower", "foot-right", "leg-left-upper", "leg-left-lower", "foot-left", "ear", "face" })
		{
			if (!ModularEnemyFinisher.TryGetPlayerBoneType(name, out BoneTypePlayer type)) continue;
			BonePlayer bone = skeleton.GetBone(type);
			if (bone == null) continue;
			SpriteRenderer renderer = bone.transform.GetComponentInChildren<SpriteRenderer>(true);
			m_playerBones[name] = Capture(bone.transform, renderer, renderer == null ? null : renderer.GetComponent<BodyPartPlayer>());
		}
	}

	// Seconds into the override for the state's normalizedTime, events and effects passed since the last frame,
	// then the pose.
	private void Advance(Playback io_playback, CoreAnimationOverrideClip i_clip, float i_normalizedTime, Dictionary<string, Bone> i_bones, bool i_player)
	{
		float duration = Mathf.Max(0.0001f, i_clip.DurationSeconds);
		int cycle;
		float time;
		if (i_clip.Loop) { cycle = Mathf.FloorToInt(Mathf.Max(0f, i_normalizedTime)); time = (Mathf.Max(0f, i_normalizedTime) - cycle) * duration; }
		else { cycle = 0; time = Mathf.Clamp01(i_normalizedTime) * duration; }
		bool restarted = io_playback.Clip != i_clip || cycle < io_playback.Cycle || (cycle == io_playback.Cycle && time + 0.0001f < io_playback.Time);
		if (restarted)
		{
			if (io_playback.Clip != i_clip || io_playback.Cursor == null) io_playback.Cursor = i_clip.Sampler.CreateCursor();
			io_playback.Clip = i_clip;
			io_playback.Cycle = cycle;
			io_playback.Time = -0.0001f;
			io_playback.NextEvent = 0;
			io_playback.NextTrigger = 0;
		}
		while (io_playback.Cycle < cycle)
		{
			FireUpTo(io_playback, duration + 1f, i_player);
			io_playback.Cycle++;
			io_playback.NextEvent = 0;
			io_playback.NextTrigger = 0;
		}
		FireUpTo(io_playback, time, i_player);
		io_playback.Time = time;
		Apply(i_clip.Sampler.Sample(time, io_playback.Cursor), i_bones);
	}

	private void FireUpTo(Playback io_playback, float i_time, bool i_player)
	{
		IReadOnlyList<EnemyAnimationEventDefinition> events = io_playback.Clip.Events;
		while (io_playback.NextEvent < events.Count && events[io_playback.NextEvent].Time <= i_time)
			HandleEvent(io_playback.Clip, events[io_playback.NextEvent++]);
		IReadOnlyList<NormalizedEffectTrigger> triggers = io_playback.Clip.Presentation?.EffectTriggers;
		if (triggers == null) return;
		while (io_playback.NextTrigger < triggers.Count && triggers[io_playback.NextTrigger].Time <= i_time)
		{
			foreach (string effectId in triggers[io_playback.NextTrigger].Effects)
				foreach (NormalizedEffectDefinition effect in io_playback.Clip.Presentation.Effects)
					if (effect != null && effect.Id == effectId) PlayEffect(effect, i_player);
			io_playback.NextTrigger++;
		}
	}

	private void Apply(IReadOnlyDictionary<string, EnemyBonePoseDefinition> i_poses, Dictionary<string, Bone> i_bones)
	{
		foreach (KeyValuePair<string, EnemyBonePoseDefinition> pair in i_poses)
		{
			if (!i_bones.TryGetValue(pair.Key, out Bone bone) || bone.Transform == null) continue;
			EnemyBonePoseDefinition pose = pair.Value;
			Transform target = bone.Transform;
			if (pose.X.HasValue || pose.Y.HasValue)
			{
				bone.Written |= Touched.Position;
				Vector3 position = target.localPosition;
				target.localPosition = new Vector3(pose.X ?? position.x, pose.Y ?? position.y, position.z);
			}
			if (pose.Rotation.HasValue)
			{
				bone.Written |= Touched.Rotation;
				Vector3 euler = target.localEulerAngles;
				target.localEulerAngles = new Vector3(euler.x, euler.y, pose.Rotation.Value);
			}
			if (pose.ScaleX.HasValue || pose.ScaleY.HasValue)
			{
				bone.Written |= Touched.Scale;
				Vector3 scale = target.localScale;
				target.localScale = new Vector3(pose.ScaleX ?? scale.x, pose.ScaleY ?? scale.y, scale.z);
			}
			SpriteRenderer renderer = bone.Renderer;
			if (renderer == null) continue;
			if (pose.ColorR.HasValue || pose.ColorG.HasValue || pose.ColorB.HasValue || pose.ColorA.HasValue)
			{
				bone.Written |= Touched.Color;
				Color color = renderer.color;
				renderer.color = new Color(pose.ColorR ?? color.r, pose.ColorG ?? color.g, pose.ColorB ?? color.b, pose.ColorA ?? color.a);
			}
			if (pose.RuntimeSortingOrder.HasValue)
			{
				bone.Written |= Touched.Sorting;
				if (bone.BodyPart != null) bone.BodyPart.SetSortingOrder(pose.RuntimeSortingOrder.Value);
				else renderer.sortingOrder = pose.RuntimeSortingOrder.Value;
			}
			if (pose.RuntimeSprite != null) { renderer.sprite = pose.RuntimeSprite; bone.Written |= Touched.Sprite; }
			else if (!string.IsNullOrEmpty(pose.RuntimeSpriteName) && TryFindSprite(pose.RuntimeSpriteName, out Sprite sprite)) { renderer.sprite = sprite; bone.Written |= Touched.Sprite; }
		}
	}

	// Back to the captured rest pose, for whatever an override wrote last frame; the Animator then poses what it drives.
	private static void Rest(Dictionary<string, Bone> i_bones)
	{
		foreach (Bone bone in i_bones.Values)
		{
			Touched written = bone.Written;
			if (written == Touched.None || bone.Transform == null) continue;
			bone.Written = Touched.None;
			if ((written & Touched.Position) != 0) bone.Transform.localPosition = bone.Position;
			if ((written & Touched.Scale) != 0) bone.Transform.localScale = bone.Scale;
			if ((written & Touched.Rotation) != 0) bone.Transform.localEulerAngles = bone.Euler;
			if (bone.Renderer == null) continue;
			if ((written & Touched.Color) != 0) bone.Renderer.color = bone.Color;
			if ((written & Touched.Sorting) != 0) { if (bone.BodyPart != null) bone.BodyPart.SetSortingOrder(bone.Sorting); else bone.Renderer.sortingOrder = bone.Sorting; }
			if ((written & Touched.Sprite) != 0 && bone.Sprite != null) bone.Renderer.sprite = bone.Sprite;
		}
	}

	private static bool TryFindSprite(string i_name, out Sprite o_sprite)
	{
		if (s_spritesByName.TryGetValue(i_name, out o_sprite)) return o_sprite != null;
		foreach (Sprite candidate in Resources.FindObjectsOfTypeAll<Sprite>())
			if (candidate != null && candidate.name == i_name) { o_sprite = candidate; break; }
		s_spritesByName[i_name] = o_sprite;
		return o_sprite != null;
	}

	private void PlayEffect(NormalizedEffectDefinition i_effect, bool i_player)
	{
		Transform parent = null;
		string target = i_effect.Target ?? string.Empty;
		if (target.StartsWith("enemy-bone/", StringComparison.Ordinal)) { if (m_bones.TryGetValue(target.Substring(11), out Bone bone)) parent = bone.Transform; }
		else if (target.StartsWith("bone/", StringComparison.Ordinal))
		{
			if ((i_player ? m_playerBones : m_bones).TryGetValue(target.Substring(5), out Bone bone)) parent = bone.Transform;
		}
		NormalizedParticleEffectRuntime.Play(i_effect, parent != null ? parent : transform);
	}

	private void HandleEvent(CoreAnimationOverrideClip i_clip, EnemyAnimationEventDefinition i_event)
	{
		if (i_event.Type == "cameraShake")
		{
			CameraXGame camera = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerCamerasXGame()?.GetCameraXGameCurrent();
			if (camera != null) camera.Shake(i_event.Amount ?? 0f, 0.1f);
		}
		else if (i_event.Type == "sound" && s_audio.TryGetValue(AudioKey(i_clip, i_event.File), out AudioClip clip) && clip != null)
		{
			if (m_audio == null)
			{
				m_audio = gameObject.AddComponent<AudioSource>();
				m_audio.playOnAwake = false;
				m_audio.spatialBlend = 1f;
				m_audio.rolloffMode = AudioRolloffMode.Linear;
				m_audio.minDistance = 2f;
				m_audio.maxDistance = 30f;
				m_audio.dopplerLevel = 0f;
				if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerAudio() != null)
				{
					var groups = CommonReferences.Instance.GetManagerAudio().GetAudioMixer().FindMatchingGroups("SFX");
					if (groups.Length > 0) m_audio.outputAudioMixerGroup = groups[0];
				}
			}
			m_audio.PlayOneShot(clip, Mathf.Clamp01(i_event.Volume ?? 1f));
		}
	}

	private static string AudioKey(CoreAnimationOverrideClip i_clip, string i_file)
	{
		return (i_clip.PackRoot ?? string.Empty) + "|" + (i_file ?? string.Empty);
	}

	private void LoadAudio(CoreAnimationOverrideClip i_clip)
	{
		foreach (EnemyAnimationEventDefinition animationEvent in i_clip.Events)
		{
			if (animationEvent?.Type != "sound" || string.IsNullOrEmpty(i_clip.PackRoot)) continue;
			string key = AudioKey(i_clip, animationEvent.File);
			if (s_audio.ContainsKey(key) || !s_audioLoading.Add(key)) continue;
			StartCoroutine(LoadClip(key, i_clip, animationEvent.File));
		}
	}

	private IEnumerator LoadClip(string i_key, CoreAnimationOverrideClip i_clip, string i_relative)
	{
		string full = null;
		try
		{
			full = System.IO.Path.GetFullPath(System.IO.Path.Combine(i_clip.PackRoot, i_relative));
			if (!ModPath.IsSafeRelativePath(i_relative) || !AssetPatchDiscovery.IsInside(full, i_clip.PackRoot) || !System.IO.File.Exists(full)) full = null;
		}
		catch (Exception) { full = null; }
		if (full == null)
		{
			ModLoaderRuntime.LastReport?.Add(ValidationSeverity.Error, "core-animation.audio-file", "Override sound is missing or outside its pack: " + i_relative, i_clip.Source);
			yield break;
		}
		AudioType type = full.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? AudioType.WAV : AudioType.OGGVORBIS;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(full).AbsoluteUri, type))
		{
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success) s_audio[i_key] = DownloadHandlerAudioClip.GetContent(request);
		}
	}
}
