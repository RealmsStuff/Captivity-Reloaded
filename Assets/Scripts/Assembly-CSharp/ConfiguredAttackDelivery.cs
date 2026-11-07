using System;
using CaptivityReloaded.Modding;
using UnityEngine;

// Lands a data-defined attack (the original-enemy attack format: melee, hitscan, projectile, area, and each stage
// of a multiStage attack) for any enemy: ModularEnemy's own attacks and a Core enemy's additionalAttacks
// (CoreExtraAttackController) both come through here. Grab is the caller's, since only original enemies have a
// data-defined finisher to start.
public static class ConfiguredAttackDelivery
{
	// Whether the hit landed on the player (a projectile counts once it is fired). i_projectileSprite finds the
	// sprite a projectileRegion names.
	public static bool Perform(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage,
		Func<string, Sprite> i_projectileSprite, bool i_isCoreAttack = true)
	{
		string type = i_stage == null ? i_definition.Type : i_stage.Type;
		if (type == "hitscan") return PerformHitscan(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
		if (type == "projectile") { SpawnProjectile(i_owner, i_player, i_definition, i_stage, i_projectileSprite, i_isCoreAttack); return true; }
		if (type == "area") return PerformArea(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
		return PerformMelee(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
	}

	public static bool PerformMelee(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage, bool i_isCoreAttack = true)
	{
		if (i_player.GetStateActorCurrent() == StateActor.Ragdoll) return false;
		float range = Resolve(i_stage?.HitRange, i_definition.HitRange, 1f);
		if (Vector2.Distance(i_owner.GetPosHips(), i_player.GetPosHips()) > range) return false;
		return ApplyConfiguredHit(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
	}

	public static bool PerformHitscan(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage, bool i_isCoreAttack = true)
	{
		if (i_player == null || i_player.GetStateActorCurrent() == StateActor.Ragdoll || !i_owner.IsHasLineOfSightToPlayerRaycast()) return false;
		Vector2 origin = Origin(i_owner);
		Vector2 target = i_player.GetPosHips();
		if (ResourceContainer.Resources != null && ResourceContainer.Resources.m_lineShoot != null)
		{
			SpriteRenderer line = UnityEngine.Object.Instantiate(ResourceContainer.Resources.m_lineShoot,
				CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
			Vector2 direction = target - origin;
			line.transform.position = origin;
			line.transform.right = direction;
			line.transform.localScale = new Vector3(direction.magnitude * 32f, 1f, 1f);
			line.color = new Color(1f, 0.35f, 0.2f, 1f);
			line.gameObject.SetActive(true);
			line.enabled = true;
			UnityEngine.Object.Destroy(line.gameObject, 0.15f);
		}
		if (Vector2.Distance(origin, target) > Resolve(i_stage?.HitRange, i_definition.HitRange, 1f)) return false;
		return ApplyConfiguredHit(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
	}

	public static bool PerformArea(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage, bool i_isCoreAttack = true)
	{
		if (i_player.GetStateActorCurrent() == StateActor.Ragdoll) return false;
		float x = Resolve(i_stage?.OffsetX, i_definition.OffsetX, 0f);
		if (i_owner.GetIsFacingLeft()) x = -x;
		Vector2 center = i_owner.GetPos() + new Vector2(x, Resolve(i_stage?.OffsetY, i_definition.OffsetY, 0f));
		float radius = Resolve(i_stage?.AreaRadius, i_definition.AreaRadius, 1f);
		if (Vector2.Distance(center, i_player.GetPosHips()) > radius) return false;
		return ApplyConfiguredHit(i_owner, i_player, i_definition, i_stage, i_isCoreAttack);
	}

	public static void SpawnProjectile(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage,
		Func<string, Sprite> i_projectileSprite, bool i_isCoreAttack = true)
	{
		Vector2 origin = Origin(i_owner);
		Vector2 displacement = i_player.GetPosHips() - origin;
		Vector2 direction = displacement.normalized;
		if (direction.sqrMagnitude <= 0.001f) direction = i_owner.GetIsFacingLeft() ? Vector2.left : Vector2.right;
		string region = i_stage?.ProjectileRegion ?? i_definition.ProjectileRegion;
		Sprite sprite = i_projectileSprite == null ? null : i_projectileSprite(region);
		if (sprite == null) return;
		GameObject projectileObject = new GameObject("mod-enemy-projectile_" + i_definition.Id);
		projectileObject.layer = LayerMask.NameToLayer("Projectile");
		Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		projectileObject.transform.SetParent(stage == null ? null : stage.transform, true);
		projectileObject.transform.position = origin;
		projectileObject.transform.right = direction;
		SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
		renderer.sprite = sprite;
		renderer.sortingLayerName = "Actor";
		renderer.sortingOrder = 100;
		Rigidbody2D body = projectileObject.AddComponent<Rigidbody2D>();
		float gravityScale = Resolve(i_stage?.ProjectileGravityScale, i_definition.ProjectileGravityScale, 0f);
		float projectileSpeed = Resolve(i_stage?.ProjectileSpeed, i_definition.ProjectileSpeed, 8f);
		body.gravityScale = gravityScale;
		body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
		if (Mathf.Abs(gravityScale) > 0.001f)
		{
			// Compensate the launch velocity so gravity creates a visible arc that
			// still passes through a stationary target's firing-time position.
			float travelSeconds = Mathf.Clamp(displacement.magnitude / projectileSpeed, 0.1f,
				Resolve(i_stage?.ProjectileLifetimeSeconds, i_definition.ProjectileLifetimeSeconds, 4f));
			Vector2 gravity = Physics2D.gravity * gravityScale;
			body.velocity = displacement / travelSeconds - gravity * (0.5f * travelSeconds);
			projectileObject.transform.right = body.velocity.normalized;
		}
		else body.velocity = direction * projectileSpeed;
		CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
		collider.isTrigger = true;
		collider.radius = Resolve(i_stage?.ProjectileRadius, i_definition.ProjectileRadius, 0.15f);
		float damage = Resolve(i_stage?.Damage, i_definition.Damage, 0f) * Multiplier(i_owner, "DamageMultiplier");
		float knockbackX = Resolve(i_stage?.KnockbackX, i_definition.KnockbackX, 0f) * Multiplier(i_owner, "KnockbackXMultiplier");
		float knockbackY = Resolve(i_stage?.KnockbackY, i_definition.KnockbackY, 0f) * Multiplier(i_owner, "KnockbackYMultiplier");
		projectileObject.AddComponent<ModularEnemyProjectile>().Configure(i_owner, damage, knockbackX, knockbackY,
			Resolve(i_stage?.RagdollSeconds, i_definition.RagdollSeconds, 0f),
			Resolve(i_stage?.ProjectileLifetimeSeconds, i_definition.ProjectileLifetimeSeconds, 4f), i_isCoreAttack);
	}

	// Whether the hit landed. A player who can't be attacked right now takes no damage, so the attack missed.
	public static bool ApplyConfiguredHit(NPC i_owner, Player i_player, EnemyAttackDefinition i_definition, EnemyAttackStageDefinition i_stage, bool i_isCoreAttack = true)
	{
		if (i_player == null || !i_player.GetIsCanBeAttacked()) return false;
		float damage = Resolve(i_stage?.Damage, i_definition.Damage, 0f) * Multiplier(i_owner, "DamageMultiplier");
		float knockbackX = Resolve(i_stage?.KnockbackX, i_definition.KnockbackX, 0f) * Multiplier(i_owner, "KnockbackXMultiplier");
		float knockbackY = Resolve(i_stage?.KnockbackY, i_definition.KnockbackY, 0f) * Multiplier(i_owner, "KnockbackYMultiplier");
		return ApplyHit(i_owner, i_player, damage, knockbackX, knockbackY, Resolve(i_stage?.RagdollSeconds, i_definition.RagdollSeconds, 0f), i_isCoreAttack);
	}

	// Whether the player took the hit. i_isCoreAttack: the hit also counts as the enemy's current Core attack
	// landing (its on-hit interaction); false for a Core enemy's additional attack.
	public static bool ApplyHit(NPC i_owner, Player i_player, float i_damage, float i_knockbackX, float i_knockbackY, float i_ragdollSeconds, bool i_isCoreAttack = true)
	{
		if (i_player == null || i_owner == null || i_owner.IsDead() || i_player.IsDead() || !i_player.GetIsCanBeAttacked() || i_player.GetIsBeingRaped()) return false;
		bool wasDown = i_player.GetStateActorCurrent() == StateActor.Ragdoll;
		i_player.TakeConfiguredHit(i_owner, i_damage, i_knockbackX, i_knockbackY);
		// Per-attack knockdown. A hit that already ragdolled the player counted its KO in TakeConfiguredHit.
		if (i_ragdollSeconds > 0f && !wasDown && !i_player.IsDead() && !i_player.GetIsBeingRaped()
			&& i_player.GetStateActorCurrent() != StateActor.Ragdoll && i_player.GetStatePlayerCurrent() != StatePlayer.Labor)
		{
			i_player.Ragdoll(i_ragdollSeconds);
			if (i_player.GetStateActorCurrent() == StateActor.Ragdoll)
			{
				CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().AddStatusKO(i_owner);
				ManagerDB.KO(i_owner);
			}
		}
		i_owner.NotifyDirectAttackHit(i_player, i_isCoreAttack);
		return true;
	}

	private static Vector2 Origin(NPC i_owner)
	{
		return i_owner.GetSkeletonActor() == null || i_owner.GetSkeletonActor().GetBoneHead() == null
			? i_owner.GetPos() : (Vector2)i_owner.GetSkeletonActor().GetBoneHead().transform.position;
	}

	// Core enemies may not have every multiplier stat an original enemy has.
	private static float Multiplier(NPC i_owner, string i_stat)
	{
		Stat stat = i_owner.GetStat(i_stat);
		return stat == null ? 1f : stat.GetValueTotal();
	}

	public static float Resolve(float? i_stageValue, float? i_attackValue, float i_fallback)
	{
		return i_stageValue ?? i_attackValue ?? i_fallback;
	}
}
