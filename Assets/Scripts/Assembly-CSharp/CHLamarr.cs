using UnityEngine;

public class CHLamarr : Challenge
{
	[SerializeField]
	private int m_wavesToSurvive;

	private HeadHumper m_headHumperHumping;

	private int m_wavesSurvived;

	protected override void HandleActivation()
	{
		m_headHumperHumping = null;
		m_wavesSurvived = 0;
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart += OnWaveStart;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.OnWaveStart -= OnWaveStart;
		if (m_headHumperHumping != null)
		{
			m_headHumperHumping.OnLetGo -= OnHeadHumperLetGo;
		}
	}

	protected override void TrackCompletion()
	{
	}

	private void OnWaveStart()
	{
		if (!(m_headHumperHumping == null))
		{
			m_wavesSurvived++;
			if (m_wavesSurvived >= m_wavesToSurvive)
			{
				Complete();
			}
		}
	}

	public void SetHeadHumperHumping(HeadHumper i_headHumper)
	{
		m_headHumperHumping = i_headHumper;
		m_wavesSurvived = 0;
		if (m_headHumperHumping != null)
		{
			m_headHumperHumping.OnLetGo -= OnHeadHumperLetGo;
		}
		m_headHumperHumping.OnLetGo += OnHeadHumperLetGo;
	}

	private void OnHeadHumperLetGo()
	{
		m_headHumperHumping.OnLetGo -= OnHeadHumperLetGo;
		m_headHumperHumping = null;
		m_wavesSurvived = 0;
	}
}
