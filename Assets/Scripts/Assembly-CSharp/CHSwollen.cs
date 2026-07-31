using UnityEngine;

public class CHSwollen : Challenge
{
	[SerializeField]
	private double m_litreSpermToReceive;

	private RaperMusca m_raperMuscaCurrent;

	private double m_litreSpermReceived;

	protected override void HandleActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped += OnBeingRaped;
		CommonReferences.Instance.GetPlayer().OnRapeEnd += OnRapeEnd;
		m_raperMuscaCurrent = null;
		m_litreSpermReceived = 0.0;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnBeingRaped -= OnBeingRaped;
		CommonReferences.Instance.GetPlayer().OnRapeEnd -= OnRapeEnd;
		if (m_raperMuscaCurrent != null)
		{
			m_raperMuscaCurrent.OnThrust -= OnThrust;
			m_raperMuscaCurrent.OnCumThrust -= OnCumThrust;
			m_raperMuscaCurrent = null;
		}
	}

	protected override void TrackCompletion()
	{
	}

	private void OnBeingRaped()
	{
		if (CommonReferences.Instance.GetPlayer().GetRaperCurrent() is RaperMusca)
		{
			m_raperMuscaCurrent = (RaperMusca)CommonReferences.Instance.GetPlayer().GetRaperCurrent();
			m_raperMuscaCurrent.OnThrust += OnThrust;
			m_raperMuscaCurrent.OnCumThrust += OnCumThrust;
		}
	}

	private void OnThrust()
	{
		m_litreSpermReceived += m_raperMuscaCurrent.GetRaperAnimationCurrent().GetLitreCumPerThrust();
		CheckForCompletion();
	}

	private void OnCumThrust()
	{
		m_litreSpermReceived += m_raperMuscaCurrent.GetRaperAnimationCurrent().GetLitreCumPerCumThrust();
		CheckForCompletion();
	}

	private void CheckForCompletion()
	{
		if (m_litreSpermReceived >= m_litreSpermToReceive)
		{
			Complete();
		}
	}

	private void OnRapeEnd()
	{
		if (m_raperMuscaCurrent != null)
		{
			m_raperMuscaCurrent.OnThrust -= OnThrust;
			m_raperMuscaCurrent.OnCumThrust -= OnCumThrust;
			m_raperMuscaCurrent = null;
		}
	}
}
