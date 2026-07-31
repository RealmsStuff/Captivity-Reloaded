using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StrengthBar : HudBar
{
	[SerializeField]
	private Image m_imgNoStrengthDanger;

	private Coroutine m_coroutineNoStrengthDanger;

	protected override void UpdateValues()
	{
		m_valueMax = CommonReferences.Instance.GetPlayer().GetStrengthMax();
		m_valueCurrent = CommonReferences.Instance.GetPlayer().GetStrengthCurrent();
	}

	public override void Decrease()
	{
		base.Decrease();
		FlashTakeDamage();
		if (m_valueCurrent <= 0f && m_coroutineNoStrengthDanger == null)
		{
			m_coroutineNoStrengthDanger = StartCoroutine(CoroutineNoStrengthDanger());
		}
	}

	private IEnumerator CoroutineNoStrengthDanger()
	{
		m_imgNoStrengthDanger.gameObject.SetActive(value: true);
		while (m_valueCurrent <= 0f)
		{
			float l_tranparencyFrom = 1f;
			float l_tranparencyTo = 0f;
			float l_timeToMove = 0.1f;
			float l_timeCurrent = 0f;
			m_imgNoStrengthDanger.color = new Color(m_imgNoStrengthDanger.color.r, m_imgNoStrengthDanger.color.g, m_imgNoStrengthDanger.color.b, l_tranparencyFrom);
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
				m_imgNoStrengthDanger.color = new Color(m_imgNoStrengthDanger.color.r, m_imgNoStrengthDanger.color.g, m_imgNoStrengthDanger.color.b, a);
				yield return new WaitForFixedUpdate();
			}
			l_tranparencyFrom = 0f;
			l_tranparencyTo = 1f;
			l_timeToMove = 0.1f;
			l_timeCurrent = 0f;
			m_imgNoStrengthDanger.color = new Color(m_imgNoStrengthDanger.color.r, m_imgNoStrengthDanger.color.g, m_imgNoStrengthDanger.color.b, l_tranparencyFrom);
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float a2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time2);
				m_imgNoStrengthDanger.color = new Color(m_imgNoStrengthDanger.color.r, m_imgNoStrengthDanger.color.g, m_imgNoStrengthDanger.color.b, a2);
				yield return new WaitForFixedUpdate();
			}
		}
		m_imgNoStrengthDanger.gameObject.SetActive(value: false);
		m_coroutineNoStrengthDanger = null;
	}

	private void FlashTakeDamage()
	{
		StartCoroutine(CoroutineFlashTakeDamage());
	}

	private IEnumerator CoroutineFlashTakeDamage()
	{
		m_imageValueOverlay.gameObject.SetActive(value: true);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.25f;
		float l_timeCurrent = 0f;
		m_imageValueOverlay.color = new Color(m_imageValueOverlay.color.r, m_imageValueOverlay.color.g, m_imageValueOverlay.color.b, l_tranparencyFrom);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imageValueOverlay.color = new Color(m_imageValueOverlay.color.r, m_imageValueOverlay.color.g, m_imageValueOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
	}

	public override void ResetBar()
	{
		base.ResetBar();
		if (m_coroutineNoStrengthDanger != null)
		{
			StopCoroutine(m_coroutineNoStrengthDanger);
			m_coroutineNoStrengthDanger = null;
		}
		m_imgNoStrengthDanger.gameObject.SetActive(value: false);
	}
}
