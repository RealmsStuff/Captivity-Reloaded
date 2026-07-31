using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Challenge : MonoBehaviour
{
	[SerializeField]
	private int m_id;

	[SerializeField]
	private string m_name;

	[TextArea(1, 5)]
	[SerializeField]
	private string m_description;

	[SerializeField]
	private Stage m_stageAssociated;

	[SerializeField]
	private bool m_isCanBeCompletedWithBrokenMind;

	[SerializeField]
	private bool m_isHiddenDescription;

	[SerializeField]
	private List<Clothing> m_rewardsClothing = new List<Clothing>();

	protected float m_trackTickDelay = 0.5f;

	private int m_state;

	private bool m_isActive;

	protected Coroutine m_coroutineTrackCompletion;

	public void Activate()
	{
		m_isActive = true;
		HandleActivation();
		m_coroutineTrackCompletion = StartCoroutine(CoroutineTrackCompletion());
	}

	public void DeActivate()
	{
		m_isActive = false;
		if (m_coroutineTrackCompletion != null)
		{
			StopCoroutine(m_coroutineTrackCompletion);
		}
		HandleDeActivation();
	}

	protected IEnumerator CoroutineTrackCompletion()
	{
		while (true)
		{
			yield return new WaitForSeconds(m_trackTickDelay);
			TrackCompletion();
		}
	}

	protected abstract void HandleActivation();

	protected abstract void HandleDeActivation();

	protected abstract void TrackCompletion();

	protected virtual void Complete()
	{
		if (m_state == 0 && m_isActive)
		{
			DeActivate();
			if (m_isCanBeCompletedWithBrokenMind || (!CommonReferences.Instance.GetPlayer().IsDead() && CommonReferences.Instance.GetPlayer().GetNumOfHeartsCurrent() != 0))
			{
				m_state = 1;
				CommonReferences.Instance.GetManagerChallenge().CompleteChallenge(this);
			}
		}
	}

	public int GetId()
	{
		return m_id;
	}

	public void SetId(int i_id)
	{
		m_id = i_id;
	}

	public int GetState()
	{
		return m_state;
	}

	public void SetState(int i_state)
	{
		m_state = i_state;
	}

	public List<Clothing> GetRewardsClothing()
	{
		return m_rewardsClothing;
	}

	public string GetName()
	{
		return m_name;
	}

	public string GetDescription()
	{
		return m_description;
	}

	public bool IsHiddenDescription()
	{
		return m_isHiddenDescription;
	}

	public Stage GetStageAssociated()
	{
		return m_stageAssociated;
	}

	public bool IsActive()
	{
		return m_isActive;
	}
}
