using UnityEngine;

public class PoisonDart : Projectile
{
	[SerializeField]
	private AudioClip m_audioHit;

	private bool m_isHitAlready;

	private void Start()
	{
		SetOwner(GetComponentInParent<Actor>());
	}

	protected override void HandleHit(Collider2D i_collider)
	{
		if (m_isHitAlready)
		{
			return;
		}
		if ((bool)i_collider.gameObject.GetComponent<Platform>())
		{
			StopFlying();
			GetComponent<Rigidbody2D>().isKinematic = true;
			GetComponent<Collider2D>().enabled = false;
			GetComponent<TrailRenderer>().emitting = false;
			m_isHitAlready = true;
			Object.Destroy(base.gameObject, 15f);
		}
		else
		{
			if (!i_collider.gameObject.GetComponent<BodyPartPlayer>())
			{
				return;
			}
			Player player = CommonReferences.Instance.GetPlayer();
			if ((player.GetIsInvulnerable() && !m_isHitsInvulnerable) || player.GetStatePlayerCurrent() == StatePlayer.Dashing)
			{
				return;
			}
			StopFlying();
			GetComponent<Rigidbody2D>().isKinematic = true;
			GetComponent<Collider2D>().enabled = false;
			GetComponent<TrailRenderer>().emitting = false;
			base.transform.parent = i_collider.gameObject.GetComponent<BodyPartPlayer>().transform;
			GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Player");
			GetComponent<SpriteRenderer>().sortingOrder = i_collider.gameObject.GetComponent<SpriteRenderer>().sortingOrder + 1;
			int num = 4;
			int num2 = 0;
			foreach (StatusEffect statusEffect in player.GetStatusEffects())
			{
				if (statusEffect.GetName() == "Poison Dart")
				{
					num2++;
				}
			}
			if (num2 < num)
			{
				AddPoisonStatusEffect();
			}
			player.TakeDamage(1f);
			player.PlayAudioSFX(m_audioHit);
			CommonReferences.Instance.GetManagerPostProcessing().PlayEffectPoisonDartHit();
			Marksman marksman = (Marksman)Library.Instance.Actors.GetNpc("Goblin Marksman");
			marksman.GetAllInteractions()[0].Trigger(marksman);
			if (player.GetStateActorCurrent() != StateActor.Ragdoll && !player.GetIsBeingRaped() && num2 >= num)
			{
				player.Ragdoll(5f);
				CommonReferences.Instance.GetManagerPostProcessing().PlayEffectRagdoll();
				CommonReferences.Instance.GetManagerPostProcessing().PlayEffectPoisonDartRagdoll();
				StatusEffectStatModifier statusEffectStatModifier = new StatusEffectStatModifier("Poison Overdose", "PoisonDart" + marksman.GetName() + marksman.gameObject.GetInstanceID(), TypeStatusEffect.Negative, 8f, i_isStackable: true);
				statusEffectStatModifier.AddPlayerStatusHudItem("Poison Overdose", "Too much poison! You have fainted...", StatusPlayerHudItemColor.Combat);
				marksman.GetAllInteractions()[1].Trigger(marksman);
				RemoveAllPoisonStatusEffects();
				player.ApplyStatusEffect(statusEffectStatModifier);
			}
			m_isHitAlready = true;
			Object.Destroy(base.gameObject, 15f);
		}
	}

	private void AddPoisonStatusEffect()
	{
		SetOwner(GetComponentInParent<Actor>());
		StatusEffectStatModifier statusEffectStatModifier = new StatusEffectStatModifier("Poison Dart", "PoisonDart" + m_owner.GetName() + m_owner.gameObject.GetInstanceID(), TypeStatusEffect.Negative, 15f, i_isStackable: true);
		statusEffectStatModifier.AddStatModification("SpeedMax", -1f);
		statusEffectStatModifier.AddPlayerStatusHudItem("Poison", "Slowed movement speed", StatusPlayerHudItemColor.Combat);
		CommonReferences.Instance.GetPlayer().ApplyStatusEffect(statusEffectStatModifier);
	}

	private void RemoveAllPoisonStatusEffects()
	{
		Player player = CommonReferences.Instance.GetPlayer();
		foreach (StatusEffect statusEffect in player.GetStatusEffects())
		{
			if (statusEffect.GetName() == "Poison Dart")
			{
				player.EndStatusEffect(statusEffect);
			}
		}
	}
}
