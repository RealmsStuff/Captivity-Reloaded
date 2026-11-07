using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

// Experimental. Runs an inherited enemy's additionalAttacks on its Core template's rig.
//
// The Core AI keeps choosing and running its own attacks. Before it acts each physics step (this runs earlier),
// while the enemy is ready to attack, this picks at most once a second between the Core attack and the additional
// attacks whose cooldown is over and whose initiateRange reaches the player (hitscan and projectile attacks also
// need a line of sight), weighted by chance: the Core attack counts with its own chance when the player is within
// its range, and "keep closing in" counts as 1 when not. When an additional attack wins, it takes the enemy over:
// thinking off (unless movesDuringAttack) and no Core attacks, face the player, play the attack's clip through
// CoreAnimationOverrideDriver, and land each hit through ConfiguredAttackDelivery, the same path original enemies'
// attacks use (hitTimeSeconds, or one hit per attackHit event; multiStage stages in order; missAnimation on a miss).
// When durationSeconds is up it hands control back. cooldownSeconds counts from the first hit (or the end of an
// attack that never got to hit) and holds every attack, Core or additional, as an original enemy's shared cooldown
// does. Being knocked down, killed or starting a grab ends the attack at once, and the Core logic takes it from there.
//
// Per-class exceptions (Core moves that run outside the Core attack list), see IsBusyWithOwnMove:
// - DeathHound: no additional attack starts while it prepares or runs its charge.
// - Marksman: no additional attack starts while it is shooting.
[DefaultExecutionOrder(-100)]
public sealed class CoreExtraAttackController : MonoBehaviour
{
	public const float PickIntervalSeconds = 1f;

	private NPC m_npc;
	private CoreAnimationOverrideDriver m_driver;
	private readonly List<EnemyAttackDefinition> m_attacks = new List<EnemyAttackDefinition>();
	private readonly Dictionary<string, float> m_readyAt = new Dictionary<string, float>();
	private float m_nextPickAt;
	private float m_canAttackAgainAt = -1f;
	private Player m_target;
	private int m_stageIndex;
	private bool m_cooldownStarted;
	private float m_endsAt;
	private Coroutine m_running;

	public EnemyAttackDefinition Current { get; private set; }
	public IReadOnlyList<EnemyAttackDefinition> Attacks => m_attacks;
	public int Started { get; private set; }
	public int HitsLanded { get; private set; }
	public string LastAttackId { get; private set; }

	public static void Attach(NPC i_npc)
	{
		if (i_npc != null && i_npc.GetComponent<CoreExtraAttackController>() == null) i_npc.gameObject.AddComponent<CoreExtraAttackController>();
	}

	private void Start()
	{
		m_npc = GetComponent<NPC>();
		m_driver = GetComponent<CoreAnimationOverrideDriver>();
		if (!Configure()) enabled = false;
	}

	public bool Configure()
	{
		m_attacks.Clear();
		if (m_npc == null || m_driver == null || !RuntimeContentIdentity.TryResolve(this, out ContentId id, out _) || id.Namespace == "core") return false;
		string templateId = null;
		foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
			if (definition.Id == id && definition.Extends.HasValue) { templateId = definition.Extends.Value.ToString(); break; }
		CoreAnimationOverrideSet set = templateId == null ? null : CoreAnimationOverrides.For(id.ToString(), templateId);
		if (set == null) return false;
		m_attacks.AddRange(set.ExtraAttacks);
		return m_attacks.Count > 0;
	}

	// Lets a test or a check run one attack now.
	public bool TryStart(string i_attackId, Player i_player)
	{
		if (Current != null || i_player == null) return false;
		foreach (EnemyAttackDefinition attack in m_attacks)
			if (attack.Id == i_attackId) { m_running = StartCoroutine(Perform(attack, i_player)); return true; }
		return false;
	}

	private void FixedUpdate()
	{
		if (m_npc == null || Current != null) return;
		if (m_canAttackAgainAt >= 0f && Time.time >= m_canAttackAgainAt)
		{
			m_canAttackAgainAt = -1f;
			if (!m_npc.IsDead()) m_npc.SetIsCanAttack(true);
		}
		if (Time.time < m_nextPickAt || !CanStart(out Player player)) return;
		float distance = Mathf.Min(Vector2.Distance(m_npc.GetPos(), player.GetPosHips()), Vector2.Distance(m_npc.GetPosFeet(), player.GetPosHips()));
		List<EnemyAttackDefinition> candidates = new List<EnemyAttackDefinition>();
		float total = 0f;
		bool lineOfSight = false, checkedLineOfSight = false;
		foreach (EnemyAttackDefinition attack in m_attacks)
		{
			if (m_readyAt.TryGetValue(attack.Id, out float readyAt) && Time.time < readyAt) continue;
			if (distance >= (attack.InitiateRange ?? 0f)) continue;
			if (NeedsLineOfSight(attack))
			{
				if (!checkedLineOfSight) { lineOfSight = m_npc.IsHasLineOfSightToPlayerRaycast(); checkedLineOfSight = true; }
				if (!lineOfSight) continue;
			}
			if ((attack.Chance ?? 0f) <= 0f) continue;
			candidates.Add(attack);
			total += attack.Chance.Value;
		}
		if (candidates.Count == 0) return;
		m_nextPickAt = Time.time + PickIntervalSeconds;
		AttackNPC coreAttack = m_npc.GetAttackCurrent();
		bool coreInRange = coreAttack != null && m_npc.GetIsCloseEnoughToAttackPlayer();
		float stay = coreInRange ? (coreAttack.GetChance01() > 0f ? coreAttack.GetChance01() : 1f) : 1f;
		float roll = Random.Range(0f, total + stay);
		foreach (EnemyAttackDefinition attack in candidates)
		{
			if (roll < attack.Chance.Value) { m_running = StartCoroutine(Perform(attack, player)); return; }
			roll -= attack.Chance.Value;
		}
	}

	private static bool NeedsLineOfSight(EnemyAttackDefinition i_attack)
	{
		if (i_attack.Type == "hitscan" || i_attack.Type == "projectile") return true;
		foreach (EnemyAttackStageDefinition stage in i_attack.Stages ?? new List<EnemyAttackStageDefinition>())
			if (stage != null && (stage.Type == "hitscan" || stage.Type == "projectile")) return true;
		return false;
	}

	private bool CanStart(out Player o_player)
	{
		o_player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (o_player == null || m_npc.IsDead() || !m_npc.GetIsThinking() || !m_npc.GetIsCanAttack() || m_npc.GetIsAttacking()) return false;
		StateActor state = m_npc.GetStateActorCurrent();
		if (state == StateActor.Ragdoll || state == StateActor.Climbing || state == StateActor.Jumping) return false;
		Raper raper = m_npc.GetRaper();
		if (raper != null && raper.GetIsRaping()) return false;
		if (o_player.IsDead() || o_player.GetIsBeingRaped() || o_player.GetStatePlayerCurrent() == StatePlayer.Labor) return false;
		return !IsBusyWithOwnMove(m_npc);
	}

	// Core moves that run outside the Core attack list; an additional attack waits for them to finish.
	public static bool IsBusyWithOwnMove(NPC i_npc)
	{
		if (i_npc is DeathHound hound) return hound.GetIsCharging() || hound.GetIsPreparingCharge();
		if (i_npc is Marksman marksman) return marksman.IsShooting();
		return false;
	}

	private bool Interrupted()
	{
		if (m_npc == null || m_npc.IsDead() || m_npc.GetStateActorCurrent() == StateActor.Ragdoll) return true;
		Raper raper = m_npc.GetRaper();
		return raper != null && raper.GetIsRaping();
	}

	private IEnumerator Perform(EnemyAttackDefinition i_attack, Player i_player)
	{
		Current = i_attack;
		LastAttackId = i_attack.Id;
		Started++;
		m_target = i_player;
		m_stageIndex = 0;
		m_cooldownStarted = false;
		bool moves = i_attack.MovesDuringAttack ?? false;
		bool tookThinking = !moves && m_npc.GetIsThinking();
		if (tookThinking) m_npc.SetIsThinking(false);
		m_npc.SetIsCanAttack(false);
		if (!moves)
		{
			m_npc.StopMovingHorizontally();
			Rigidbody2D body = m_npc.GetRigidbody2D();
			if (body != null && Mathf.Approximately(body.gravityScale, 0f)) body.velocity = Vector2.zero;
		}
		m_npc.SetIsFacingLeft(i_player.GetPosHips().x < m_npc.GetPos().x);
		m_driver.PlayExtra(i_attack.Animation, OnEvent);
		int hits = 0;
		CoreAnimationOverrideSet set = null;
		if (RuntimeContentIdentity.TryResolve(this, out ContentId id, out _))
			foreach (EnemyDefinition definition in ModLoaderRuntime.EnemyDefinitions)
				if (definition.Id == id && definition.Extends.HasValue) { set = CoreAnimationOverrides.For(id.ToString(), definition.Extends.Value.ToString()); break; }
		if (set != null && set.Enemy.TryGetValue(i_attack.Animation, out CoreAnimationOverrideClip clip))
			foreach (EnemyAnimationEventDefinition animationEvent in clip.Events) if (animationEvent?.Type == "attackHit") hits++;
		float started = Time.time;
		m_endsAt = started + (i_attack.DurationSeconds ?? 0f);
		bool interrupted = false;
		if (hits == 0)
		{
			float hitTime = i_attack.HitTimeSeconds ?? 0f;
			while (Time.time < started + hitTime) { if (Interrupted()) { interrupted = true; break; } yield return null; }
			m_endsAt = Mathf.Max(m_endsAt, started + hitTime);
			if (!interrupted && !Deliver(null)) PlayMiss(i_attack, hitTime);
		}
		while (!interrupted && Time.time < m_endsAt)
		{
			if (Interrupted()) { interrupted = true; break; }
			yield return null;
		}
		m_driver.StopExtra();
		if (!m_cooldownStarted) StartCooldown(i_attack);
		// Knocked down or killed: the Core logic turns thinking back on when the enemy gets up.
		if (tookThinking && !interrupted) m_npc.SetIsThinking(true);
		Current = null;
		m_target = null;
		m_running = null;
	}

	private void OnEvent(EnemyAnimationEventDefinition i_event)
	{
		EnemyAttackDefinition attack = Current;
		if (attack == null || i_event == null || Interrupted()) return;
		if (i_event.Type == "attackHit")
		{
			EnemyAttackStageDefinition stage = null;
			if (attack.Type == "multiStage" && attack.Stages != null && m_stageIndex < attack.Stages.Count) stage = attack.Stages[m_stageIndex++];
			if (!Deliver(stage) && stage == null) PlayMiss(attack, i_event.Time);
		}
		else if (i_event.Type == "impulse")
		{
			Rigidbody2D body = m_npc.GetRigidbody2D();
			if (body == null || body.bodyType != RigidbodyType2D.Dynamic) return;
			float x = i_event.X ?? 0f;
			if ((i_event.RelativeToFacing ?? true) && m_npc.GetIsFacingLeft()) x = -x;
			body.AddForce(new Vector2(x, i_event.Y ?? 0f), ForceMode2D.Impulse);
		}
	}

	private bool Deliver(EnemyAttackStageDefinition i_stage)
	{
		EnemyAttackDefinition attack = Current;
		Player player = m_target;
		if (attack == null || player == null || player.IsDead() || player.GetIsBeingRaped() || player.GetStatePlayerCurrent() == StatePlayer.Labor) return false;
		if (!m_cooldownStarted) StartCooldown(attack);
		bool landed = ConfiguredAttackDelivery.Perform(m_npc, player, attack, i_stage, m_driver.GetBoneSprite, false);
		if (landed) HitsLanded++;
		return landed;
	}

	private void StartCooldown(EnemyAttackDefinition i_attack)
	{
		m_cooldownStarted = true;
		float readyAt = Time.time + (i_attack.CooldownSeconds ?? 0f);
		m_readyAt[i_attack.Id] = readyAt;
		m_canAttackAgainAt = readyAt;
	}

	// The hit missed: the missAnimation takes over from the same moment and the attack runs until it ends.
	private void PlayMiss(EnemyAttackDefinition i_attack, float i_clipSeconds)
	{
		if (string.IsNullOrEmpty(i_attack.MissAnimation) || !m_driver.HasClip(i_attack.MissAnimation)) return;
		m_driver.PlayExtra(i_attack.MissAnimation, OnEvent, i_clipSeconds);
		m_endsAt = Time.time + Mathf.Max(0f, m_driver.GetClipSeconds(i_attack.MissAnimation) - i_clipSeconds);
	}

	private void OnDisable()
	{
		if (Current == null) return;
		if (m_running != null) StopCoroutine(m_running);
		if (m_driver != null) m_driver.StopExtra();
		Current = null;
		m_running = null;
	}
}
