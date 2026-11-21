using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

// playerReactionRef: a short player animation layered on the player when a data-defined attack lands (the head
// snapping back from a jab, folding round a kick). Unlike a finisher's player animation the player is free: the
// Animator keeps running, physics keeps knocking them back and nothing is locked. So this adds, after the Animator
// has posed the frame (LateUpdate), each bone's turn away from the clip's first frame, faded in and out, on top of
// whatever the player is doing; it restores nothing because it never replaces anything.
public sealed class ModularPlayerHitReaction : MonoBehaviour
{
	// The clip is posed for an attacker in front of the player. Its fade in is short so the hit reads on the frame it
	// lands; the fade out hands the body back to the Animator without a pop.
	private const float BlendInSeconds = 0.04f;
	private const float BlendOutSeconds = 0.12f;

	private readonly Dictionary<string, Transform> m_bones = new Dictionary<string, Transform>(StringComparer.Ordinal);
	// What this wrote to each bone last frame and what the bone held before it: a bone the Animator did not pose again
	// still holds last frame's offset, and adding to that would run away.
	private readonly Dictionary<string, Quaternion> m_writtenRotations = new Dictionary<string, Quaternion>(StringComparer.Ordinal);
	private readonly Dictionary<string, Quaternion> m_baseRotations = new Dictionary<string, Quaternion>(StringComparer.Ordinal);
	private Player m_player;
	private EnemyAnimationClipDefinition m_clip;
	private float m_startedAt;
	private float m_side = 1f;

	public bool IsPlaying => m_clip != null;

	// Starts the reaction, replacing one that is still playing. i_attackerX is where the hit came from: a hit from
	// behind turns the reaction round, so the body is always thrown away from the attacker.
	public static void Play(Player i_player, EnemyAnimationClipDefinition i_clip, float i_attackerX)
	{
		if (i_player == null || !CanPlay(i_clip) || !CanReact(i_player)) return;
		ModularPlayerHitReaction reaction = i_player.GetComponent<ModularPlayerHitReaction>();
		if (reaction == null) reaction = i_player.gameObject.AddComponent<ModularPlayerHitReaction>();
		reaction.Begin(i_player, i_clip, i_attackerX);
	}

	public static bool CanPlay(EnemyAnimationClipDefinition i_clip)
	{
		return i_clip != null && i_clip.DurationSeconds > 0f && i_clip.Frames != null && i_clip.Frames.Count > 0;
	}

	// A ragdolled, held, labouring or dead player is posed by something else entirely.
	private static bool CanReact(Player i_player)
	{
		return !i_player.IsDead() && !i_player.GetIsBeingRaped() && i_player.GetStateActorCurrent() != StateActor.Ragdoll
			&& i_player.GetStatePlayerCurrent() != StatePlayer.Labor;
	}

	// How much of the clip's turns show at a time into it: in over BlendInSeconds, out over the last BlendOutSeconds.
	public static float Weight(float i_elapsed, float i_duration)
	{
		if (i_duration <= 0f || i_elapsed < 0f || i_elapsed >= i_duration) return 0f;
		float fadeIn = Mathf.Min(BlendInSeconds, i_duration * 0.25f), fadeOut = Mathf.Min(BlendOutSeconds, i_duration * 0.5f);
		float rise = fadeIn <= 0f ? 1f : Mathf.Clamp01(i_elapsed / fadeIn);
		float fall = fadeOut <= 0f ? 1f : Mathf.Clamp01((i_duration - i_elapsed) / fadeOut);
		return Mathf.SmoothStep(0f, 1f, Mathf.Min(rise, fall));
	}

	private void Begin(Player i_player, EnemyAnimationClipDefinition i_clip, float i_attackerX)
	{
		m_player = i_player;
		m_clip = i_clip;
		m_startedAt = Time.time;
		m_writtenRotations.Clear();
		m_baseRotations.Clear();
		bool attackerIsLeft = i_attackerX < i_player.transform.position.x;
		m_side = attackerIsLeft == i_player.GetIsFacingLeft() ? 1f : -1f;
		if (m_bones.Count > 0) return;
		SkeletonPlayer skeleton = i_player.GetSkeletonPlayer();
		if (skeleton == null) return;
		foreach (string name in ModularEnemyFinisher.GetPlayerBoneNames())
		{
			if (!ModularEnemyFinisher.TryGetPlayerBoneType(name, out BoneTypePlayer type)) continue;
			BonePlayer bone = skeleton.GetBone(type);
			if (bone != null) m_bones[name] = bone.transform;
		}
	}

	private void LateUpdate()
	{
		if (m_clip == null) return;
		float elapsed = Time.time - m_startedAt;
		if (m_player == null || !CanReact(m_player) || elapsed >= m_clip.DurationSeconds)
		{
			m_clip = null;
			return;
		}
		float weight = Weight(elapsed, m_clip.DurationSeconds);
		if (weight <= 0f) return;
		// The baked frames around this time, as ModularEnemyAnimationController samples an enemy clip.
		List<EnemyAnimationFrameDefinition> frames = m_clip.Frames;
		EnemyAnimationFrameDefinition first = frames[0], from = frames[0], to = frames[frames.Count - 1];
		for (int index = 0; index < frames.Count; index++)
		{
			if (frames[index].Time <= elapsed) from = frames[index];
			if (frames[index].Time >= elapsed) { to = frames[index]; break; }
		}
		float blend = Mathf.Approximately(from.Time, to.Time) ? 0f : Mathf.InverseLerp(from.Time, to.Time, elapsed);
		foreach (KeyValuePair<string, Transform> pair in m_bones)
		{
			Transform target = pair.Value;
			if (target == null) continue;
			from.Bones.TryGetValue(pair.Key, out EnemyBonePoseDefinition a);
			to.Bones.TryGetValue(pair.Key, out EnemyBonePoseDefinition b);
			if (a == null) a = b;
			if (b == null) b = a;
			if (a == null || !a.Rotation.HasValue || !b.Rotation.HasValue) continue;
			first.Bones.TryGetValue(pair.Key, out EnemyBonePoseDefinition reference);
			float turn = Mathf.DeltaAngle(reference?.Rotation ?? a.Rotation.Value, Mathf.LerpAngle(a.Rotation.Value, b.Rotation.Value, blend));
			if (Mathf.Abs(turn) < 0.001f) continue;
			// The Animator's pose for this frame, or last frame's if it left this bone alone.
			Quaternion current = target.localRotation;
			if (m_writtenRotations.TryGetValue(pair.Key, out Quaternion written) && Quaternion.Angle(written, current) < 0.01f)
				current = m_baseRotations[pair.Key];
			m_baseRotations[pair.Key] = current;
			Quaternion posed = current * Quaternion.Euler(0f, 0f, turn * weight * m_side);
			target.localRotation = posed;
			m_writtenRotations[pair.Key] = posed;
		}
	}

	private void OnDisable()
	{
		m_clip = null;
	}
}
