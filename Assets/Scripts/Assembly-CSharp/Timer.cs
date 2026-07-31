using System.Collections;
using UnityEngine;

public class Timer
{
	private float m_timeMax;

	private float m_timeLeft;

	private bool m_isPlay;

	public Timer(float i_secsDuration)
	{
		m_timeMax = i_secsDuration;
		m_timeLeft = m_timeMax;
	}

	public IEnumerator CoroutinePlayAndWaitForEnd()
	{
		Play();
		while (m_timeLeft > 0f)
		{
			yield return new WaitForEndOfFrame();
			UpdateTimer();
		}
	}

	private void UpdateTimer()
	{
		if (m_isPlay)
		{
			m_timeLeft -= Time.deltaTime;
			if (m_timeLeft <= 0f)
			{
				EndTimer();
			}
		}
	}

	public void Play()
	{
		m_isPlay = true;
	}

	public void Pause()
	{
		m_isPlay = false;
	}

	public void Reset()
	{
		m_isPlay = false;
		m_timeLeft = m_timeMax;
	}

	private void EndTimer()
	{
		m_isPlay = false;
		m_timeLeft = 0f;
	}

	public float GetTimeLeft()
	{
		return m_timeLeft;
	}

	public float GetTimePassed()
	{
		return m_timeMax - m_timeLeft;
	}

	public void SetDuration(float i_secsDuration)
	{
		m_timeMax = i_secsDuration;
	}

	public void SetDurationAndResetTimer(float i_secsDurationNew)
	{
		SetDuration(i_secsDurationNew);
		Reset();
	}
}
