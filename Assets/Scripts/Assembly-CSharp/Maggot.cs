using System.Collections.Generic;
using UnityEngine;

public class Maggot : Walker
{
	private ParticleSystem m_particleSteppedOn;

	private List<AudioClip> m_audiosSteppedOn = new List<AudioClip>();

	public override void Awake()
	{
		base.Awake();
		m_audiosSteppedOn.Add(Resources.Load<AudioClip>("Audio/MaggotSteppedOn1"));
		m_audiosSteppedOn.Add(Resources.Load<AudioClip>("Audio/MaggotSteppedOn2"));
		m_particleSteppedOn = ResourceContainer.Resources.m_particleMaggotSteppedOn;
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIMaggot>();
		m_xAI.Initialize(this);
	}

	public override void Start()
	{
		base.Start();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!m_isDead && !GetRaper().GetIsRaping())
		{
			CheckForSteppedOn();
		}
	}

	private void CheckForSteppedOn()
	{
		if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), GetPos()) < 1.5f && (CommonReferences.Instance.GetPlayer().GetStateActorCurrent() == StateActor.Moving || CommonReferences.Instance.GetPlayer().GetStatePlayerCurrent() == StatePlayer.Dashing) && (m_stateActorCurrent == StateActor.Idle || m_stateActorCurrent == StateActor.Moving))
		{
			GetSteppedOn();
		}
	}

	private void GetSteppedOn()
	{
		Die();
	}

	public override void Die()
	{
		base.Die();
		ParticleSystem particleSystem = Object.Instantiate(m_particleSteppedOn, base.transform.parent);
		particleSystem.gameObject.SetActive(value: true);
		particleSystem.transform.position = GetPos();
		particleSystem.Play();
		Object.Destroy(particleSystem.gameObject, 5f);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFXRandom(m_audiosSteppedOn, 100f);
		Object.Destroy(base.gameObject);
	}
}
