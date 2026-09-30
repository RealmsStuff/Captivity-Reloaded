using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
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
	private List<ChallengeGrantDefinition> m_modRewardItems = new List<ChallengeGrantDefinition>();
	private List<ChallengeGrantDefinition> m_modRewardWeapons = new List<ChallengeGrantDefinition>();
	private List<ContentId> m_modRewardContent = new List<ContentId>();
	private int m_modRewardCurrency;

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

	public IReadOnlyList<ChallengeGrantDefinition> GetModRewardItems() { return m_modRewardItems; }
	public IReadOnlyList<ChallengeGrantDefinition> GetModRewardWeapons() { return m_modRewardWeapons; }
	public IReadOnlyList<ContentId> GetModRewardContent() { return m_modRewardContent; }
	public int GetModRewardCurrency() { return m_modRewardCurrency; }

	public void ConfigureModChallenge(int i_id, string i_name, string i_description, IEnumerable<Clothing> i_rewards, Stage i_stage = null)
	{
		ConfigureModChallenge(i_id, i_name, i_description, i_rewards, null, null, null, 0, i_stage);
	}

	public void ConfigureModChallenge(int i_id, string i_name, string i_description, IEnumerable<Clothing> i_rewards,
		IEnumerable<ChallengeGrantDefinition> i_rewardItems, IEnumerable<ChallengeGrantDefinition> i_rewardWeapons,
		IEnumerable<ContentId> i_rewardContent, int i_rewardCurrency, Stage i_stage = null)
	{
		m_id = i_id;
		m_name = i_name;
		m_description = i_description;
		m_stageAssociated = i_stage;
		m_isCanBeCompletedWithBrokenMind = true;
		m_isHiddenDescription = false;
		m_rewardsClothing = new List<Clothing>();
		if (i_rewards != null) foreach (Clothing reward in i_rewards) if (reward != null) m_rewardsClothing.Add(reward);
		m_modRewardItems = i_rewardItems == null ? new List<ChallengeGrantDefinition>() : new List<ChallengeGrantDefinition>(i_rewardItems);
		m_modRewardWeapons = i_rewardWeapons == null ? new List<ChallengeGrantDefinition>() : new List<ChallengeGrantDefinition>(i_rewardWeapons);
		m_modRewardContent = i_rewardContent == null ? new List<ContentId>() : new List<ContentId>(i_rewardContent);
		m_modRewardCurrency = Mathf.Max(0, i_rewardCurrency);
	}
}
