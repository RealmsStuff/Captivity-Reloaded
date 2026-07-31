using System.Collections;
using UnityEngine;

public class PleasureBar : HudBar
{
	protected override void UpdateValues()
	{
		m_valueMax = CommonReferences.Instance.GetPlayer().GetPleasureMax();
		m_valueCurrent = CommonReferences.Instance.GetPlayer().GetPleasureCurrent();
	}

	public override void Increase()
	{
		base.Increase();
		FlashGainPleasure();
	}

	private void FlashGainPleasure()
	{
		StartCoroutine(CoroutineFlashGainPleasure());
	}

	private IEnumerator CoroutineFlashGainPleasure()
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
}
