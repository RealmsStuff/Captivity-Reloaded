using UnityEngine;

public class BodyPartActor : BodyPart
{
	[SerializeField]
	private DamageMultplier m_damageMultiplierBodyPart;

	[SerializeField]
	private bool m_isBodyPartDestroyedOnDeath;

	private float m_health;

	private bool m_isBodyPartDestroyed;
	private bool m_hasModHitZoneConfiguration;

	public void ConfigureModHitZone(string i_damageMultiplier, bool i_destroyedOnDeath)
	{
		if (string.Equals(i_damageMultiplier, "low", System.StringComparison.OrdinalIgnoreCase)) m_damageMultiplierBodyPart = DamageMultplier.Low;
		else if (string.Equals(i_damageMultiplier, "critical", System.StringComparison.OrdinalIgnoreCase)) m_damageMultiplierBodyPart = DamageMultplier.Crit;
		else if (string.Equals(i_damageMultiplier, "block", System.StringComparison.OrdinalIgnoreCase)) m_damageMultiplierBodyPart = DamageMultplier.Block;
		else m_damageMultiplierBodyPart = DamageMultplier.Normal;
		m_isBodyPartDestroyedOnDeath = i_destroyedOnDeath;
		m_hasModHitZoneConfiguration = true;
	}

	private void Start()
	{
		m_health = 15f;
		if (!m_hasModHitZoneConfiguration) m_isBodyPartDestroyedOnDeath = true;
		m_isBodyPartDestroyed = false;
	}

	public void TakeHit(float i_damage)
	{
		if (!m_isBodyPartDestroyed)
		{
			m_health -= i_damage;
			if (m_health < 0f)
			{
				m_health = 0f;
			}
			if (m_health == 0f && GetOwner().IsDead())
			{
				TryDestroyBodyPart();
			}
		}
	}

	public bool TakeHitProjectile(Bullet i_projectile)
	{
		if (!m_owner)
		{
			return false;
		}
		bool num = ((NPC)GetOwner()).TakeHitBullet(i_projectile.GetOwner(), i_projectile, this);
		if (num)
		{
			TakeHit(i_projectile.GetDamage() * GetDamageMultiplierAmountBodyPart());
		}
		return num;
	}

	public float GetDamageMultiplierAmountBodyPart()
	{
		switch (m_damageMultiplierBodyPart)
		{
		case DamageMultplier.Low:
			return 0.75f;
		case DamageMultplier.Normal:
			return 1f;
		case DamageMultplier.Crit:
			return 2f;
		case DamageMultplier.Block:
			return 0f;
		default:
			return 1f;
		}
	}

	public void TryDestroyBodyPart()
	{
		if (Random.Range(0, 101) > 60)
		{
			Explode();
			m_isBodyPartDestroyed = true;
		}
	}

	public bool GetIsBodyPartDestroyedOnHitDeath()
	{
		return m_isBodyPartDestroyedOnDeath;
	}

	public bool GetIsBodyPartDestroyed()
	{
		return m_isBodyPartDestroyed;
	}
}
