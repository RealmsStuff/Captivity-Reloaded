using UnityEngine;


public class Incinerator : Gun
{
	[SerializeField]
	private AudioClip m_audioFiring;

	[SerializeField]
	private ParticleSystem m_psFire;

	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightFire;

	private bool m_isPullingTrigger;

	protected override void Awake()
	{
		base.Awake();
		m_audioSourceSFX.loop = true;
		m_audioSourceSFX.clip = m_audioFiring;
	}

	public override void Equip()
	{
		base.Equip();
		m_audioSourceSFX.Stop();
	}

	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		if (GetAmmoMagazineLeft() <= 0)
		{
			m_isPullingTrigger = false;
			if (m_audioSourceSFX.isPlaying)
			{
				m_audioSourceSFX.Stop();
			}
			return false;
		}
		m_isPullingTrigger = true;
		if (!m_audioSourceSFX.isPlaying)
		{
			m_audioSourceSFX.Play();
		}
		return true;
	}

	public override void HandleActorHit(BodyPartActor i_bodyPart, Bullet i_bullet)
	{
		base.HandleActorHit(i_bodyPart, i_bullet);
		i_bodyPart.GetOwner().Ignite(GetName(), 8f, 2f, 4f);
	}

	private void Update()
	{
		if (!m_isPickedUp)
		{
			if (m_psFire.isPlaying)
			{
				m_psFire.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				m_lightFire.enabled = false;
				m_isPullingTrigger = false;
				m_audioSourceSFX.Stop();
			}
		}
		else if (m_isPullingTrigger)
		{
			if (!m_psFire.isEmitting)
			{
				m_psFire.Play();
				m_lightFire.enabled = true;
			}
			if (CommonReferences.Instance.GetManagerInput().IsButtonUp(InputButton.Fire))
			{
				m_psFire.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
				m_lightFire.enabled = false;
				m_isPullingTrigger = false;
				m_audioSourceSFX.Stop();
			}
		}
	}
}
