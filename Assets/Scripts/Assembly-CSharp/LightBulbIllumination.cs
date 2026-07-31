using System.Collections;
using UnityEngine;

public class LightBulbIllumination : LightBulb
{
	private float m_intensity;

	[SerializeField]
	private float m_distanceSeeActor;

	private bool m_isAlight;

	private Coroutine m_coroutineLight;

	protected override void Start()
	{
		base.Start();
		m_intensity = m_light.intensity;
		m_light.intensity = 0f;
		StartCoroutine(CoroutineCheckIfCanLight());
	}

	private IEnumerator CoroutineCheckIfCanLight()
	{
		while (true)
		{
			bool i_lightUp = false;
			foreach (Actor allActor in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllActors())
			{
				if (Vector2.Distance(allActor.GetPosHips(), base.transform.position) <= m_distanceSeeActor)
				{
					i_lightUp = true;
					break;
				}
			}
			Lighten(i_lightUp);
			yield return new WaitForSeconds(0.25f);
		}
	}

	private void Lighten(bool i_lightUp)
	{
		if (m_isAlight != i_lightUp)
		{
			m_isAlight = i_lightUp;
			if (m_coroutineLight != null)
			{
				StopCoroutine(m_coroutineLight);
			}
			m_coroutineLight = StartCoroutine(CoroutineLight(i_lightUp));
		}
	}

	private IEnumerator CoroutineLight(bool i_isOn)
	{
		float l_intensityFrom = m_light.intensity;
		float l_intensityTo = (i_isOn ? m_intensity : 0f);
		float l_timeToMove = 1.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Smooth, AnimationTools.Transition.Steep, l_intensityFrom, l_intensityTo, i_time);
			m_light.intensity = intensity;
			yield return new WaitForFixedUpdate();
		}
		m_coroutineLight = null;
	}
}
