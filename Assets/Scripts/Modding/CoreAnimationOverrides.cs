using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptivityReloaded.Modding
{
	// A normalized clip that plays on a Core enemy (or on the player during its grab) in place of an Animator state.
	public sealed class CoreAnimationOverrideClip
	{
		public ContentId Id { get; }
		public string PackId { get; }
		public string PackRoot { get; }
		public string Source { get; }
		public float DurationSeconds { get; }
		public bool Loop { get; }
		public AnimationCurveSampler Sampler { get; }
		// Cosmetic events only (sound, cameraShake); the original state's Unity events keep firing for gameplay. An
		// additional attack's own clip also carries attackHit and impulse.
		public IReadOnlyList<EnemyAnimationEventDefinition> Events { get; }
		public NormalizedAnimationPresentation Presentation { get; }

		public CoreAnimationOverrideClip(ContentId i_id, string i_packId, string i_packRoot, string i_source, float i_durationSeconds, bool i_loop,
			AnimationCurveSampler i_sampler, IReadOnlyList<EnemyAnimationEventDefinition> i_events, NormalizedAnimationPresentation i_presentation)
		{
			Id = i_id; PackId = i_packId; PackRoot = i_packRoot; Source = i_source; DurationSeconds = i_durationSeconds; Loop = i_loop;
			Sampler = i_sampler; Events = i_events ?? new List<EnemyAnimationEventDefinition>(); Presentation = i_presentation;
		}
	}

	// The clips that replace a Core enemy's Animator states, by semantic name, and the player's side of its grab
	// phases, by finisher-phase-N.
	public sealed class CoreAnimationOverrideSet
	{
		public Dictionary<string, CoreAnimationOverrideClip> Enemy { get; } = new Dictionary<string, CoreAnimationOverrideClip>(StringComparer.Ordinal);
		public Dictionary<string, CoreAnimationOverrideClip> Player { get; } = new Dictionary<string, CoreAnimationOverrideClip>(StringComparer.Ordinal);
		// Experimental: an inherited enemy's additionalAttacks whose clips resolved; Enemy holds their clips by name.
		public List<EnemyAttackDefinition> ExtraAttacks { get; } = new List<EnemyAttackDefinition>();
		public bool IsEmpty => Enemy.Count == 0 && Player.Count == 0 && ExtraAttacks.Count == 0;
	}

	// Resolves the two ways a pack changes a Core enemy's animation:
	// - REPLACE: an assetPatch on core:enemy/<slug> with "animations" (semantic -> enemyAnimation) and
	//   "playerAnimations" (finisher-phase-N -> playerAnimation). Every instance of that Core enemy, and its inherited
	//   variants, plays them. Two patches on the same clip follow the asset patch rules: the later pack wins only when
	//   its manifest lists the slot <target>/animation/<semantic> (or <target>/player-animation/<phase>) in overrides;
	//   otherwise both are dropped and the Core clip stays.
	// - ADD: an inherited enemy's animationRefs and playerAnimationRefs give it its own clips for any of its
	//   template's semantic names. They win over a replacement of the template.
	public static class CoreAnimationOverrides
	{
		private static readonly Dictionary<string, CoreAnimationOverrideSet> s_sets = new Dictionary<string, CoreAnimationOverrideSet>(StringComparer.Ordinal);
		private static readonly HashSet<string> CosmeticEvents = new HashSet<string>(new[] { "sound", "cameraShake", "cue" }, StringComparer.Ordinal);
		private static readonly HashSet<string> AttackEvents = new HashSet<string>(new[] { "sound", "cameraShake", "cue", "attackHit", "impulse" }, StringComparer.Ordinal);
		// How far an override's length may differ from the original state's before a warning.
		public const float LengthTolerance = 0.05f;

		public static int Count => s_sets.Count;

		public static void Clear() { s_sets.Clear(); }

		public static bool TryGet(string i_id, out CoreAnimationOverrideSet o_set)
		{
			return s_sets.TryGetValue(i_id ?? string.Empty, out o_set) && !o_set.IsEmpty;
		}

		// The clips an enemy plays: its own (an inherited enemy's refs) over its template's replacements.
		public static CoreAnimationOverrideSet For(string i_id, string i_templateId)
		{
			s_sets.TryGetValue(i_id ?? string.Empty, out CoreAnimationOverrideSet own);
			s_sets.TryGetValue(i_templateId ?? string.Empty, out CoreAnimationOverrideSet template);
			if (template == null || i_templateId == i_id) return own != null && !own.IsEmpty ? own : null;
			if (own == null) return template.IsEmpty ? null : template;
			CoreAnimationOverrideSet merged = new CoreAnimationOverrideSet();
			foreach (KeyValuePair<string, CoreAnimationOverrideClip> pair in template.Enemy) merged.Enemy[pair.Key] = pair.Value;
			foreach (KeyValuePair<string, CoreAnimationOverrideClip> pair in template.Player) merged.Player[pair.Key] = pair.Value;
			foreach (KeyValuePair<string, CoreAnimationOverrideClip> pair in own.Enemy) merged.Enemy[pair.Key] = pair.Value;
			foreach (KeyValuePair<string, CoreAnimationOverrideClip> pair in own.Player) merged.Player[pair.Key] = pair.Value;
			merged.ExtraAttacks.AddRange(own.ExtraAttacks);
			return merged.IsEmpty ? null : merged;
		}

		private sealed class Candidate
		{
			public AssetPatchDefinition Patch;
			public string Key;
			public bool Player;
			public CoreAnimationOverrideClip Clip;
			public bool Authorized;
		}

		public static void Resolve(IEnumerable<AssetPatchDefinition> i_patches, IEnumerable<EnemyDefinition> i_enemies, IEnumerable<ModPack> i_packs,
			CoreAnimationRuntimeMap i_map, ValidationReport io_report)
		{
			s_sets.Clear();
			Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
			foreach (ModPack pack in i_packs ?? new ModPack[0]) if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;
			i_map = i_map ?? new CoreAnimationRuntimeMap();

			// REPLACE: asset patches, slot by slot in pack load order.
			Dictionary<string, List<Candidate>> slots = new Dictionary<string, List<Candidate>>(StringComparer.Ordinal);
			List<string> slotOrder = new List<string>();
			foreach (AssetPatchDefinition patch in i_patches ?? new AssetPatchDefinition[0])
			{
				if (patch == null || (patch.Animations.Count == 0 && patch.PlayerAnimations.Count == 0)) continue;
				string target = patch.Target.ToString();
				if (!i_map.TryGet(target, out CoreAnimationRuntimeEnemy enemy))
				{
					io_report.Add(ValidationSeverity.Error, "core-animation.enemy", "Animation patch targets an enemy the game has no Core rig for: " + target, patch.Source);
					continue;
				}
				packs.TryGetValue(patch.PackId, out ModPack pack);
				foreach (KeyValuePair<string, ContentId> pair in patch.Animations)
				{
					CoreAnimationOverrideClip clip = ResolveEnemyClip(enemy, pair.Key, pair.Value, patch.Target, null, pack, patch.Source, io_report);
					if (clip != null) AddCandidate(slots, slotOrder, target + "/animation/" + pair.Key, patch, pair.Key, false, clip, pack);
				}
				foreach (KeyValuePair<string, ContentId> pair in patch.PlayerAnimations)
				{
					CoreAnimationOverrideClip clip = ResolvePlayerClip(enemy, pair.Key, pair.Value, pack, patch.Source, io_report);
					if (clip != null) AddCandidate(slots, slotOrder, target + "/player-animation/" + pair.Key, patch, pair.Key, true, clip, pack);
				}
			}
			foreach (string slot in slotOrder)
			{
				List<Candidate> candidates = slots[slot];
				Candidate winner = candidates[0];
				bool unresolved = false;
				for (int index = 1; index < candidates.Count; index++)
				{
					if (candidates[index].Authorized)
					{
						io_report.Add(ValidationSeverity.Warning, "asset-patch.override", "Authorized animation override: " + candidates[index].Patch.Id + " replaces " + winner.Patch.Id + " for " + slot, candidates[index].Patch.Source);
						winner = candidates[index];
						unresolved = false;
					}
					else unresolved = true;
				}
				if (unresolved)
				{
					io_report.Add(ValidationSeverity.Error, "asset-patch.conflict", "Conflicting animation patches without an explicit override; Core animation retained: " + slot + " ("
						+ string.Join(", ", candidates.Select(i_candidate => i_candidate.Patch.Id.ToString()).ToArray()) + ")");
					continue;
				}
				CoreAnimationOverrideSet set = GetOrAdd(winner.Patch.Target.ToString());
				(winner.Player ? set.Player : set.Enemy)[winner.Key] = winner.Clip;
			}

			// ADD: inherited enemies' own clips.
			foreach (EnemyDefinition definition in i_enemies ?? new EnemyDefinition[0])
			{
				if (definition == null || !definition.Extends.HasValue) continue;
				if (definition.AnimationReferences.Count == 0 && definition.PlayerAnimationReferences.Count == 0 && definition.AdditionalAttacks.Count == 0) continue;
				ContentId template = definition.Extends.Value;
				if (!i_map.TryGet(template.ToString(), out CoreAnimationRuntimeEnemy enemy))
				{
					io_report.Add(ValidationSeverity.Error, "core-animation.enemy", "The game has no Core rig for this enemy's template, so it cannot animate it: " + template, definition.Source);
					continue;
				}
				packs.TryGetValue(definition.PackId, out ModPack pack);
				CoreAnimationOverrideSet set = null;
				// Clip names only additional attacks play: not Core states, so they may carry attackHit and impulse.
				HashSet<string> attackClips = new HashSet<string>(StringComparer.Ordinal);
				foreach (EnemyAttackDefinition attack in definition.AdditionalAttacks)
				{
					if (attack?.Animation != null && !enemy.HasSemantic(attack.Animation)) attackClips.Add(attack.Animation);
					if (attack?.MissAnimation != null && !enemy.HasSemantic(attack.MissAnimation)) attackClips.Add(attack.MissAnimation);
				}
				foreach (KeyValuePair<string, string> pair in definition.AnimationReferences)
				{
					if (!ContentId.TryParse(pair.Value, out ContentId id))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.animation-ref-missing", "Unknown normalized enemy animation: " + pair.Value, definition.Source);
						continue;
					}
					CoreAnimationOverrideClip clip = ResolveEnemyClip(enemy, pair.Key, id, template, definition.Id, pack, definition.Source, io_report, attackClips.Contains(pair.Key));
					if (clip != null) (set = set ?? GetOrAdd(definition.Id.ToString())).Enemy[pair.Key] = clip;
				}
				foreach (KeyValuePair<string, string> pair in definition.PlayerAnimationReferences)
				{
					if (!ContentId.TryParse(pair.Value, out ContentId id))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.player-animation-ref-missing", "Unknown normalized player animation: " + pair.Value, definition.Source);
						continue;
					}
					CoreAnimationOverrideClip clip = ResolvePlayerClip(enemy, pair.Key, id, pack, definition.Source, io_report);
					if (clip != null) (set = set ?? GetOrAdd(definition.Id.ToString())).Player[pair.Key] = clip;
				}
				foreach (EnemyAttackDefinition attack in definition.AdditionalAttacks)
				{
					if (attack == null) continue;
					CoreAnimationOverrideClip clip = null, miss = null;
					bool hasClip = set != null && set.Enemy.TryGetValue(attack.Animation ?? string.Empty, out clip);
					if (attack.MissAnimation != null && (set == null || !set.Enemy.TryGetValue(attack.MissAnimation, out miss))) hasClip = false;
					if (hasClip && CheckExtraAttack(enemy, attack, clip, definition.Source, io_report)) set.ExtraAttacks.Add(attack);
				}
			}
		}

		// An additional attack's timing and projectiles against its clip and the Core rig, as for an original enemy's
		// attacks: a hit time or attackHit events, one attackHit per multiStage stage, nothing after the attack ends.
		private static bool CheckExtraAttack(CoreAnimationRuntimeEnemy i_enemy, EnemyAttackDefinition i_attack, CoreAnimationOverrideClip i_clip, string i_source, ValidationReport io_report)
		{
			int errors = io_report.Issues.Count(i_issue => i_issue.Severity == ValidationSeverity.Error);
			int hits = i_clip.Events.Count(i_event => i_event != null && i_event.Type == "attackHit");
			if (!i_attack.HitTimeSeconds.HasValue && hits == 0)
				io_report.Add(ValidationSeverity.Error, "core-extra-attack.hit-timing", "Additional attack " + i_attack.Id + " needs hitTimeSeconds or an attackHit event in its clip.", i_source);
			if (i_attack.Type == "multiStage" && (i_attack.Stages == null || hits != i_attack.Stages.Count))
				io_report.Add(ValidationSeverity.Error, "core-extra-attack.stage-events", "multiStage attack " + i_attack.Id + " needs exactly one attackHit event per stage in its clip.", i_source);
			foreach (EnemyAnimationEventDefinition animationEvent in i_clip.Events)
				if (animationEvent != null && i_attack.DurationSeconds.HasValue && animationEvent.Time > i_attack.DurationSeconds.Value)
					io_report.Add(ValidationSeverity.Error, "core-extra-attack.event-after-attack", "Animation event at " + animationEvent.Time + " seconds occurs after additional attack " + i_attack.Id + " has ended.", i_source);
			List<string> regions = new List<string>();
			if (i_attack.Type == "projectile") regions.Add(i_attack.ProjectileRegion);
			foreach (EnemyAttackStageDefinition stage in i_attack.Stages ?? new List<EnemyAttackStageDefinition>())
				if (stage?.Type == "projectile") regions.Add(stage.ProjectileRegion ?? i_attack.ProjectileRegion);
			foreach (string region in regions)
				if (!i_enemy.TryGetBone(region ?? string.Empty, out _))
					io_report.Add(ValidationSeverity.Error, "core-extra-attack.projectile-region", "On a Core enemy, projectileRegion names a bone of its rig (the projectile shows that bone's sprite): " + (region ?? "<null>") + " is not a bone of " + i_enemy.Id + ".", i_source);
			return io_report.Issues.Count(i_issue => i_issue.Severity == ValidationSeverity.Error) == errors;
		}

		private static void AddCandidate(Dictionary<string, List<Candidate>> io_slots, List<string> io_order, string i_slot, AssetPatchDefinition i_patch,
			string i_key, bool i_player, CoreAnimationOverrideClip i_clip, ModPack i_pack)
		{
			if (!io_slots.TryGetValue(i_slot, out List<Candidate> list)) { io_slots.Add(i_slot, list = new List<Candidate>()); io_order.Add(i_slot); }
			bool authorized = i_pack?.Manifest?.Overrides != null && i_pack.Manifest.Overrides.Contains(i_slot);
			list.Add(new Candidate { Patch = i_patch, Key = i_key, Player = i_player, Clip = i_clip, Authorized = authorized });
		}

		private static CoreAnimationOverrideSet GetOrAdd(string i_id)
		{
			if (!s_sets.TryGetValue(i_id, out CoreAnimationOverrideSet set)) s_sets.Add(i_id, set = new CoreAnimationOverrideSet());
			return set;
		}

		// i_owner: the inherited enemy whose ref this is (its clips may name it or its template), or null for a patch
		// (the clip must name the patched Core enemy).
		// i_attackClip: a clip only an additional attack plays (its name is not a Core state).
		private static CoreAnimationOverrideClip ResolveEnemyClip(CoreAnimationRuntimeEnemy i_enemy, string i_semantic, ContentId i_id, ContentId i_template,
			ContentId? i_owner, ModPack i_pack, string i_source, ValidationReport io_report, bool i_attackClip = false)
		{
			if (!i_attackClip && !i_enemy.HasSemantic(i_semantic))
			{
				io_report.Add(ValidationSeverity.Error, "core-animation.semantic", "Not one of the Core enemy's clips: " + i_semantic + " on " + i_template + ".", i_source);
				return null;
			}
			if (!NormalizedEnemyAnimationRegistry.TryGet(i_id, out NormalizedEnemyAnimationDefinition animation))
			{
				io_report.Add(ValidationSeverity.Error, "enemy.animation-ref-missing", "Unknown normalized enemy animation: " + i_id, i_source);
				return null;
			}
			if (animation.Enemy != i_template && (!i_owner.HasValue || animation.Enemy != i_owner.Value))
			{
				io_report.Add(ValidationSeverity.Error, "core-animation.owner", "Animation " + animation.Id + " targets " + animation.Enemy + ", not " + (i_owner.HasValue ? i_owner.Value + " or " : string.Empty) + i_template + ".", i_source);
				return null;
			}
			int issues = io_report.Issues.Count(i_issue => i_issue.Severity == ValidationSeverity.Error);
			foreach (NormalizedNumericTrack track in animation.Tracks) CheckBone(i_enemy, track?.Target, animation, io_report);
			foreach (NormalizedObjectTrack track in animation.ObjectTracks) CheckBone(i_enemy, track?.Target, animation, io_report);
			foreach (NormalizedEffectDefinition effect in animation.Effects)
				CheckBone(i_enemy, effect?.Target != null && effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal) ? "bone/" + effect.Target.Substring(11) : effect?.Target, animation, io_report);
			foreach (EnemyAnimationEventDefinition animationEvent in animation.Events)
				if (animationEvent != null && !(i_attackClip ? AttackEvents : CosmeticEvents).Contains(animationEvent.Type))
					io_report.Add(ValidationSeverity.Error, "core-animation.event", "A Core enemy's clip keeps its own gameplay; an override may only carry sound, cameraShake and cue events (not " + animationEvent.Type + "): " + animation.Id, animation.Source);
			if (io_report.Issues.Count(i_issue => i_issue.Severity == ValidationSeverity.Error) != issues) return null;
			if (!i_attackClip) WarnLength(i_enemy.OriginalSeconds(i_semantic), animation.DurationSeconds, animation.Id + " (" + i_semantic + ")", animation.Source, io_report);
			return new CoreAnimationOverrideClip(animation.Id, animation.PackId, i_pack?.RootPath, animation.Source, animation.DurationSeconds, animation.Loop,
				animation.CreateSampler(), animation.Events.Where(i_event => i_event != null && i_event.Type != "cue").ToList(), animation.CreatePresentation());
		}

		private static CoreAnimationOverrideClip ResolvePlayerClip(CoreAnimationRuntimeEnemy i_enemy, string i_phase, ContentId i_id, ModPack i_pack, string i_source, ValidationReport io_report)
		{
			CoreAnimationRuntimePhase phase = i_enemy.GetPhase(i_phase);
			if (phase == null)
			{
				io_report.Add(ValidationSeverity.Error, "core-animation.phase", "Not one of the Core enemy's grab phases: " + i_phase + " on " + i_enemy.Id + ".", i_source);
				return null;
			}
			if (!NormalizedPlayerAnimationRegistry.TryGet(i_id, out NormalizedPlayerAnimationDefinition animation))
			{
				io_report.Add(ValidationSeverity.Error, "enemy.player-animation-ref-missing", "Unknown normalized player animation: " + i_id, i_source);
				return null;
			}
			if ((animation.Events?.Count ?? 0) > 0)
				io_report.Add(ValidationSeverity.Warning, "core-animation.player-events", "A Core grab keeps its own pleasure and damage; this player clip's events are ignored: " + animation.Id, animation.Source);
			WarnLength(phase.PlayerSeconds, animation.DurationSeconds, animation.Id + " (" + i_phase + ")", animation.Source, io_report);
			return new CoreAnimationOverrideClip(animation.Id, animation.PackId, i_pack?.RootPath, animation.Source, animation.DurationSeconds, animation.Loop,
				animation.CreateSampler(), new List<EnemyAnimationEventDefinition>(), animation.CreatePresentation());
		}

		private static void CheckBone(CoreAnimationRuntimeEnemy i_enemy, string i_target, NormalizedEnemyAnimationDefinition i_animation, ValidationReport io_report)
		{
			int slash = i_target?.IndexOf('/') ?? -1;
			string bone = slash < 0 ? string.Empty : i_target.Substring(slash + 1);
			if (!i_enemy.TryGetBone(bone, out _))
				io_report.Add(ValidationSeverity.Error, "core-animation.bone", "Not a bone of the Core rig " + i_enemy.Id + ": " + bone + " (" + i_animation.Id + ")", i_animation.Source);
		}

		public static void WarnLength(float? i_originalSeconds, float i_seconds, string i_what, string i_source, ValidationReport io_report)
		{
			if (!i_originalSeconds.HasValue || i_originalSeconds.Value <= 0f) return;
			if (Math.Abs(i_seconds - i_originalSeconds.Value) <= i_originalSeconds.Value * LengthTolerance) return;
			io_report.Add(ValidationSeverity.Warning, "core-animation.length", "The override lasts " + i_seconds.ToString("0.###") + " s but the Core clip it replaces lasts "
				+ i_originalSeconds.Value.ToString("0.###") + " s; it is stretched to the Core state's timing: " + i_what, i_source);
		}
	}
}
