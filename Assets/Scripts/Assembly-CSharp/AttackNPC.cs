using System.Collections.Generic;
using UnityEngine;

public class AttackNPC : MonoBehaviour
{
	[SerializeField]
	protected string m_name;

	[SerializeField]
	protected string m_nameStateAndTrigger;

	[SerializeField]
	protected AnimationClip m_animClip;

	[Header("-")]
	[SerializeField]
	protected bool m_isUseDurationAttackCustom;

	[SerializeField]
	protected float m_durationAttackCustom;

	[Header("-")]
	[SerializeField]
	protected float m_chance01;

	[SerializeField]
	protected float m_damage;

	[SerializeField]
	protected float m_knockbackX;

	[SerializeField]
	protected float m_knockbackY;

	[SerializeField]
	protected float m_durationCooldown;

	[SerializeField]
	protected float m_rangeInitiate;

	[SerializeField]
	protected float m_rangeHit;

	[SerializeField]
	protected bool m_isMovesDuringAttack;

	[SerializeField]
	protected bool m_isInstantROnHit;

	[SerializeField]
	protected List<AudioClip> m_audiosAttackStart = new List<AudioClip>();

	[SerializeField]
	protected List<AudioClip> m_audiosAttackPerform = new List<AudioClip>();

	[SerializeField]
	protected AudioClip m_audioAttackHit;

	[SerializeField]
	protected List<TrailRenderer> m_trailsAttack = new List<TrailRenderer>();

	[SerializeField]
	protected Interaction m_interactionToTriggerOnAttackHit;

	protected float m_durationAttack;

	protected NPC m_npc;

	protected Player m_player;

	protected virtual void Start()
	{
		m_npc = GetComponentInParent<NPC>();
		m_player = CommonReferences.Instance.GetPlayer();
		if (m_isUseDurationAttackCustom)
		{
			m_durationAttack = m_durationAttackCustom;
		}
		else
		{
			m_durationAttack = m_animClip.length;
		}
		foreach (TrailRenderer item in m_trailsAttack)
		{
			item.emitting = false;
		}
	}

	public virtual void HandleAttackStart()
	{
		foreach (TrailRenderer item in m_trailsAttack)
		{
			item.emitting = true;
		}
		if (m_isInstantROnHit)
		{
			m_npc.GetSkeletonActor().StartFlashingLeapAttack();
		}
	}

	public virtual void HandleAttackPerform()
	{
		if (m_isInstantROnHit)
		{
			m_npc.GetSkeletonActor().StopFlashingLeapAttack();
		}
	}

	public virtual void HandleAttackHit()
	{
		if (m_isInstantROnHit && m_player.GetIsCanBeRaped())
		{
			m_npc.StartRape();
		}
		if (m_interactionToTriggerOnAttackHit != null)
		{
			m_interactionToTriggerOnAttackHit.Trigger(m_npc);
		}
	}

	public virtual void HandleAttackEnd()
	{
		foreach (TrailRenderer item in m_trailsAttack)
		{
			item.emitting = false;
		}
	}

	public string GetNameStateAndTrigger()
	{
		return m_nameStateAndTrigger;
	}

	public float GetChance01()
	{
		return m_chance01;
	}

	public float GetKnockbackX()
	{
		return m_knockbackX;
	}

	public float GetKnockbackY()
	{
		return m_knockbackY;
	}

	public float GetDurationCooldown()
	{
		return m_durationCooldown;
	}

	public float GetRangeInitiate()
	{
		return m_rangeInitiate;
	}

	public virtual float GetRangeHit()
	{
		return m_rangeHit;
	}

	public bool IsMovesDuringAttack()
	{
		return m_isMovesDuringAttack;
	}

	public List<AudioClip> GetAudiosAttackStart()
	{
		return m_audiosAttackStart;
	}

	public List<AudioClip> GetAudiosAttackPerform()
	{
		return m_audiosAttackPerform;
	}

	public AudioClip GetAudioAttackHit()
	{
		return m_audioAttackHit;
	}

	public float GetDurationAttack()
	{
		return m_durationAttack;
	}

	public NPC GetNPC()
	{
		return m_npc;
	}

	public float GetDamage()
	{
		return m_damage;
	}

	public void ConfigureModAttack(CaptivityReloaded.Modding.EnemyAttackDefinition i_attack)
	{
		if (i_attack == null) return;
		if (i_attack.Chance.HasValue) m_chance01 = i_attack.Chance.Value;
		if (i_attack.Damage.HasValue) m_damage = i_attack.Damage.Value;
		if (i_attack.KnockbackX.HasValue) m_knockbackX = i_attack.KnockbackX.Value;
		if (i_attack.KnockbackY.HasValue) m_knockbackY = i_attack.KnockbackY.Value;
		if (i_attack.CooldownSeconds.HasValue) m_durationCooldown = i_attack.CooldownSeconds.Value;
		if (i_attack.InitiateRange.HasValue) m_rangeInitiate = i_attack.InitiateRange.Value;
		if (i_attack.HitRange.HasValue) m_rangeHit = i_attack.HitRange.Value;
		if (i_attack.MovesDuringAttack.HasValue) m_isMovesDuringAttack = i_attack.MovesDuringAttack.Value;
		if (i_attack.DurationSeconds.HasValue)
		{
			m_isUseDurationAttackCustom = true;
			m_durationAttackCustom = i_attack.DurationSeconds.Value;
			m_durationAttack = m_durationAttackCustom;
		}
	}
}
