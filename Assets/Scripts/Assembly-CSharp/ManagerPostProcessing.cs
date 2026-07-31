using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ManagerPostProcessing : MonoBehaviour
{
	[SerializeField]
	private Volume m_effectDeath;

	[SerializeField]
	private Volume m_effectThrust;

	[SerializeField]
	private Volume m_effectCum;

	[SerializeField]
	private Volume m_effectOrgasm;

	[SerializeField]
	private Volume m_effectTakeHit;

	[SerializeField]
	private Volume m_effectPsycho;

	[SerializeField]
	private Volume m_effectBuffout;

	[SerializeField]
	private Volume m_effectDoubleTrigger;

	[SerializeField]
	private Volume m_effectBPrec;

	[SerializeField]
	private Volume m_effectBirth;

	[SerializeField]
	private Volume m_effectRagdoll;

	[SerializeField]
	private Volume m_effectPoisonDartHit;

	[SerializeField]
	private Volume m_effectPoisonDartRagdoll;

	[SerializeField]
	private Volume m_effectStrangeLiquid;

	[SerializeField]
	private Volume m_effectElectrocute;

	[SerializeField]
	private Volume m_effectHypnotize;

	[SerializeField]
	private Volume m_effectFear;

	private Coroutine m_coroutineEffectTakeHit;

	private Coroutine m_coroutineEffectCum;

	private Coroutine m_coroutineEffectDeathAway;

	private Coroutine m_coroutineEffectPsycho;

	private Coroutine m_coroutineEffectBirth;

	public void PlayEffectTakeHit(float i_damage)
	{
		m_coroutineEffectTakeHit = StartCoroutine(CoroutinePlayEffectTakeHit(i_damage));
	}

	private IEnumerator CoroutinePlayEffectTakeHit(float i_damage)
	{
		float l_weightFrom = ((!(i_damage <= 15f)) ? (i_damage / 50f) : 0.35f);
		float l_weightTo = 0f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_weightFrom, l_weightTo, i_time);
			m_effectTakeHit.weight = weight;
			yield return new WaitForFixedUpdate();
		}
	}

	public void PlayEffectThrust(float i_strength0to100)
	{
		StartCoroutine(CoroutinePlayEffectThrust(i_strength0to100));
	}

	private IEnumerator CoroutinePlayEffectThrust(float i_strength0to100)
	{
		float l_weightFrom = i_strength0to100 / 100f;
		float l_weightTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_effectThrust.weight = weight;
			yield return new WaitForFixedUpdate();
		}
	}

	public void PlayEffectCum()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectCum, 1f, 0f, 0.5f));
	}

	public void PlayEffectDeathAway()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectDeath, 1f, 0f, 1.5f));
	}

	public void PlayEffectPsycho()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectPsycho, 1f, 0f, 3f));
	}

	public void PlayEffectBuffout()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectBuffout, 1f, 0f, 2f));
	}

	public void PlayEffectDoubleTrigger()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectDoubleTrigger, 1f, 0f, 3f));
	}

	public void PlayEffectBPrec()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectBPrec, 1f, 0f, 3f));
	}

	public void PlayEffectBirth()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectBirth, 1f, 0f, 1f));
	}

	public Volume GetEffectDeath()
	{
		return m_effectDeath;
	}

	public void PlayEffectOrgasm()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectOrgasm, 1f, 0f, 5f));
	}

	public void PlayEffectRagdoll()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectRagdoll, 1f, 0f, 2f));
	}

	public void PlayEffectPoisonDartHit()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectPoisonDartHit, 1f, 0f, 1f));
	}

	public void PlayEffectStrangeLiquid()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectStrangeLiquid, 1f, 0f, 12f));
	}

	public void PlayEffectPoisonDartRagdoll()
	{
		StartCoroutine(CoroutinePlayEffectPoisonDartRagdoll());
	}

	private IEnumerator CoroutinePlayEffectPoisonDartRagdoll()
	{
		m_effectPoisonDartRagdoll.weight = 1f;
		yield return new WaitForSeconds(3f);
		float l_weightFrom = 1f;
		float l_weightTo = 0f;
		float l_timeToMove = 8f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
			m_effectPoisonDartRagdoll.weight = weight;
			yield return new WaitForFixedUpdate();
		}
	}

	public void PlayEffectElectrocute()
	{
		StartCoroutine(CoroutinePlayEffect(m_effectElectrocute, 1f, 0f, 0.5f));
	}

	public void PlayEffectFear(float i_duration)
	{
		StartCoroutine(CoroutinePlayEffect(m_effectFear, 1f, 0f, i_duration));
	}

	private IEnumerator CoroutinePlayEffect(Volume i_effect, float i_weightFrom, float i_weightTo, float i_timeToMove)
	{
		float l_timeCurrent = 0f;
		while (l_timeCurrent < i_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / i_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, i_weightFrom, i_weightTo, i_time);
			i_effect.weight = weight;
			yield return new WaitForFixedUpdate();
		}
	}

	public Volume GetEffectHypnotize()
	{
		return m_effectHypnotize;
	}

	public void ResetAllEffects()
	{
		Volume[] componentsInChildren = GetComponentsInChildren<Volume>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].weight = 0f;
		}
	}
}
