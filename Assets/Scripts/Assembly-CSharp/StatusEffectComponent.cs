using System.Collections;
using UnityEngine;

public class StatusEffectComponent : MonoBehaviour
{
	private StatusEffect m_statusEffect;

	protected bool m_isActive;

	protected float m_durationLeft;

	private Coroutine m_coroutineWaitForDuration;

	private Coroutine m_coroutineTick;

	public void Initialize(StatusEffect i_statusEffect)
	{
		m_statusEffect = i_statusEffect;
	}

	public virtual void ActivateStatusEffect()
	{
		m_isActive = true;
		m_statusEffect.Activate();
		m_coroutineWaitForDuration = StartCoroutine(CoroutineWaitForDuration());
		if (m_statusEffect is StatusEffectTicker)
		{
			m_coroutineTick = StartCoroutine(CoroutineTick());
		}
	}

	private IEnumerator CoroutineWaitForDuration()
	{
		m_durationLeft = m_statusEffect.GetDuration();
		while (m_durationLeft > 0f)
		{
			yield return new WaitForEndOfFrame();
			m_durationLeft -= Time.deltaTime;
			if (!m_statusEffect.IsActive())
			{
				EndStatusEffectEmpty();
			}
		}
		EndStatusEffect();
	}

	private IEnumerator CoroutineTick()
	{
		StatusEffectTicker statusEffectTicker = (StatusEffectTicker)m_statusEffect;
		Timer l_timer = new Timer(0f);
		float l_durationTick = 1f / statusEffectTicker.GetTicksPerSec();
		while (m_isActive)
		{
			Tick();
			l_timer.SetDurationAndResetTimer(l_durationTick);
			yield return l_timer.CoroutinePlayAndWaitForEnd();
		}
	}

	private void Tick()
	{
		((StatusEffectTicker)m_statusEffect).Tick();
	}

	public virtual void EndStatusEffect()
	{
		m_isActive = false;
		m_statusEffect.End();
		if (m_coroutineWaitForDuration != null)
		{
			StopCoroutine(m_coroutineWaitForDuration);
		}
		if (m_coroutineTick != null)
		{
			StopCoroutine(m_coroutineTick);
		}
		Object.Destroy(this);
	}

	private void EndStatusEffectEmpty()
	{
		m_isActive = false;
		if (m_coroutineWaitForDuration != null)
		{
			StopCoroutine(m_coroutineWaitForDuration);
		}
		if (m_coroutineTick != null)
		{
			StopCoroutine(m_coroutineTick);
		}
		Object.Destroy(this);
	}

	public void ResetDurationLeft()
	{
		m_durationLeft = m_statusEffect.GetDuration();
	}

	public bool IsActive()
	{
		return m_isActive;
	}

	public StatusEffect GetStatusEffect()
	{
		return m_statusEffect;
	}
}
