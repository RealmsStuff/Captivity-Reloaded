using System.Collections;
using UnityEngine;


public class Sqoid : Flier
{
	[SerializeField]
	private EnergyBlast m_energyBlast;

	[SerializeField]
	private AudioClip m_audioZap;

	[SerializeField]
	private AudioClip m_audioHypnotize;

	private float m_chanceToAttack = 0.5f;

	private bool m_isCanAttackOverride = true;

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(CoroutineWaitBeforeCanAttack());
	}

	private IEnumerator CoroutineWaitBeforeCanAttack()
	{
		SetIsCanAttack(i_isCanAttack: false);
		yield return new WaitForSeconds(2f);
		SetIsCanAttack(i_isCanAttack: true);
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAISqoid>();
		m_xAI.Initialize(this);
	}

	public override void StartAttack()
	{
		if (m_isCanAttackOverride)
		{
			if (!TryAttack())
			{
				StartCoroutine(CoroutineWaitBeforeCanAttackAgain());
			}
			else
			{
				base.StartAttack();
			}
		}
	}

	private bool TryAttack()
	{
		if (Random.Range(0f, 1f) >= m_chanceToAttack)
		{
			return true;
		}
		return false;
	}

	private IEnumerator CoroutineWaitBeforeCanAttackAgain()
	{
		m_isCanAttackOverride = false;
		yield return new WaitForSeconds(3f);
		m_isCanAttackOverride = true;
	}

	public override void Die()
	{
		base.Die();
		UnityEngine.Rendering.Universal.Light2D[] componentsInChildren = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	public EnergyBlast GetEnergyBlast()
	{
		return m_energyBlast;
	}

	public AudioClip GetAudioZap()
	{
		return m_audioZap;
	}

	public AudioClip GetAudioHypnotize()
	{
		return m_audioHypnotize;
	}
}
