using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : Walker
{
	[SerializeField]
	private List<AudioClip> m_audiosChase = new List<AudioClip>();

	[SerializeField]
	private List<AudioClip> m_audiosFall = new List<AudioClip>();

	private bool m_isChasing;

	private Coroutine m_coroutineChase;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIZombie>();
		m_xAI.Initialize(this);
	}

	public override void UpdateAnim()
	{
		base.UpdateAnim();
		_ = (XAIZombie)m_xAI;
		if (m_isChasing)
		{
			m_animator.SetBool("IsChasing", value: true);
		}
		else
		{
			m_animator.SetBool("IsChasing", value: false);
		}
	}

	public void Chase()
	{
		PlayAudio(m_audiosChase);
		m_coroutineChase = StartCoroutine(CoroutineChase());
	}

	private IEnumerator CoroutineChase()
	{
		m_isChasing = true;
		float seconds = Random.Range(ExternalRuleProfileFactory.ZombieChaseDurationMin(4f), ExternalRuleProfileFactory.ZombieChaseDurationMax(12f));
		List<StatModifier> l_modifiers = new List<StatModifier>
		{
			AddStatModifier("SpeedAccel", 3f),
			AddStatModifier("SpeedMax", ExternalRuleProfileFactory.ZombieChaseSpeedMax(3f))
		};
		yield return new WaitForSeconds(seconds);
		m_isChasing = false;
		RemoveStatModifier(l_modifiers);
	}

	public void Fall()
	{
		Ragdoll(3f);
		PlayAudio(m_audiosFall);
	}

	public bool IsChasing()
	{
		return m_isChasing;
	}
}
