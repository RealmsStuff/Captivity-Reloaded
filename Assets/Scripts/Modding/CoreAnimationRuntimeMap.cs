using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	// What the game needs at runtime to play a normalized clip on a Core enemy in place of one of its Animator
	// states: each rig bone's transform and renderer path under the Animator, and which Animator state plays which
	// semantic clip (with the original clip's length). Generated from ModSDK/AnimationReference (the Core rigs and
	// the animation catalog) by Captivity Reloaded > Modding > Refresh Core Animation Runtime Map, and shipped as
	// Resources/Modding/Core/core-animation-map.json.
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreAnimationRuntimeMap
	{
		public const string ResourcePath = "Modding/Core/core-animation-map";

		[JsonProperty("schemaVersion")] public int SchemaVersion { get; set; } = 1;
		[JsonProperty("type")] public string Type { get; set; } = "coreAnimationRuntimeMap";
		[JsonProperty("enemies")] public List<CoreAnimationRuntimeEnemy> Enemies { get; set; } = new List<CoreAnimationRuntimeEnemy>();

		private Dictionary<string, CoreAnimationRuntimeEnemy> m_byId;
		private static CoreAnimationRuntimeMap s_loaded;

		public bool TryGet(string i_enemyId, out CoreAnimationRuntimeEnemy o_enemy)
		{
			if (m_byId == null)
			{
				m_byId = new Dictionary<string, CoreAnimationRuntimeEnemy>(StringComparer.Ordinal);
				foreach (CoreAnimationRuntimeEnemy enemy in Enemies ?? new List<CoreAnimationRuntimeEnemy>())
					if (enemy?.Id != null) m_byId[enemy.Id] = enemy;
			}
			return m_byId.TryGetValue(i_enemyId ?? string.Empty, out o_enemy);
		}

		public static CoreAnimationRuntimeMap Parse(string i_json)
		{
			return JsonConvert.DeserializeObject<CoreAnimationRuntimeMap>(i_json) ?? new CoreAnimationRuntimeMap();
		}

		// The shipped map (cached). Empty when the resource is missing.
		public static CoreAnimationRuntimeMap Load()
		{
			if (s_loaded != null) return s_loaded;
			TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
			s_loaded = asset == null ? new CoreAnimationRuntimeMap() : Parse(asset.text);
			return s_loaded;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreAnimationRuntimeEnemy
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("bones")] public List<CoreAnimationRuntimeBone> Bones { get; set; } = new List<CoreAnimationRuntimeBone>();
		[JsonProperty("states")] public List<CoreAnimationRuntimeState> States { get; set; } = new List<CoreAnimationRuntimeState>();
		[JsonProperty("phases")] public List<CoreAnimationRuntimePhase> Phases { get; set; } = new List<CoreAnimationRuntimePhase>();

		private Dictionary<string, CoreAnimationRuntimeBone> m_bones;

		public bool TryGetBone(string i_name, out CoreAnimationRuntimeBone o_bone)
		{
			if (m_bones == null)
			{
				m_bones = new Dictionary<string, CoreAnimationRuntimeBone>(StringComparer.Ordinal);
				foreach (CoreAnimationRuntimeBone bone in Bones ?? new List<CoreAnimationRuntimeBone>())
					if (bone?.Name != null) m_bones[bone.Name] = bone;
			}
			return m_bones.TryGetValue(i_name ?? string.Empty, out o_bone);
		}

		// The semantic names this enemy's Animator states play.
		public bool HasSemantic(string i_semantic)
		{
			foreach (CoreAnimationRuntimeState state in States ?? new List<CoreAnimationRuntimeState>())
				if (state != null && state.Semantic == i_semantic) return true;
			return false;
		}

		// The longest original clip the semantic plays (seconds), or null.
		public float? OriginalSeconds(string i_semantic)
		{
			float? longest = null;
			foreach (CoreAnimationRuntimeState state in States ?? new List<CoreAnimationRuntimeState>())
				if (state != null && state.Semantic == i_semantic && state.Seconds > 0f) longest = Math.Max(longest ?? 0f, state.Seconds);
			return longest;
		}

		public CoreAnimationRuntimePhase GetPhase(string i_semantic)
		{
			foreach (CoreAnimationRuntimePhase phase in Phases ?? new List<CoreAnimationRuntimePhase>())
				if (phase != null && phase.Semantic == i_semantic) return phase;
			return null;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreAnimationRuntimeBone
	{
		[JsonProperty("name")] public string Name { get; set; }
		// The bone's transform, relative to the Animator's transform ("" is the Animator's own object).
		[JsonProperty("path")] public string Path { get; set; }
		// Its sprite renderer (usually a bp_ child), when it has one.
		[JsonProperty("renderer", NullValueHandling = NullValueHandling.Ignore)] public string Renderer { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreAnimationRuntimeState
	{
		[JsonProperty("layer")] public string Layer { get; set; }
		[JsonProperty("path")] public string Path { get; set; }
		[JsonProperty("semantic")] public string Semantic { get; set; }
		[JsonProperty("seconds")] public float Seconds { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreAnimationRuntimePhase
	{
		[JsonProperty("semantic")] public string Semantic { get; set; }
		[JsonProperty("playerClip")] public string PlayerClip { get; set; }
		[JsonProperty("playerSeconds")] public float PlayerSeconds { get; set; }
	}
}
