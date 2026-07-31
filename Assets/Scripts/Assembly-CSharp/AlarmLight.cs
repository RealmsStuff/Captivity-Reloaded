using System.Collections;
using UnityEngine;


public class AlarmLight : MonoBehaviour
{
	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightAlarm;

	[SerializeField]
	private float m_intensityFrom;

	[SerializeField]
	private float m_intensityTo;

	[SerializeField]
	private float m_timeToMove;

	private void Start()
	{
		StartCoroutine(CoroutineAlarmLight());
	}

	private IEnumerator CoroutineAlarmLight()
	{
		while (true)
		{
			float l_weightFrom = m_intensityFrom;
			float l_weightTo = m_intensityTo;
			float l_timeToMove = m_timeToMove;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time);
				m_lightAlarm.intensity = intensity;
				yield return new WaitForFixedUpdate();
			}
			l_weightFrom = m_lightAlarm.intensity;
			l_weightTo = m_intensityFrom;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float intensity2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_weightFrom, l_weightTo, i_time2);
				m_lightAlarm.intensity = intensity2;
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
