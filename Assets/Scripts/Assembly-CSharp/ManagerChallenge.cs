using System.Collections.Generic;
using UnityEngine;

public class ManagerChallenge : MonoBehaviour
{
	public delegate void DelOnChallengeComplete(Challenge i_challenge);

	private List<Challenge> m_challenges = new List<Challenge>();

	public event DelOnChallengeComplete OnChallengeComplete;

	public void UpdateChallenges()
	{
		List<int> idsOpenChallenges = ManagerDB.GetIdsOpenChallenges();
		List<int> idsCompletedPendingChallenges = ManagerDB.GetIdsCompletedPendingChallenges();
		List<int> idsCompletedSeenChallenges = ManagerDB.GetIdsCompletedSeenChallenges();
		List<Challenge> list = new List<Challenge>();
		List<Challenge> list2 = new List<Challenge>();
		List<Challenge> list3 = new List<Challenge>();
		foreach (Challenge challenge in m_challenges)
		{
			if (idsOpenChallenges.Contains(challenge.GetId()))
			{
				list.Add(challenge);
			}
			else if (idsCompletedPendingChallenges.Contains(challenge.GetId()))
			{
				list2.Add(challenge);
			}
			else if (idsCompletedSeenChallenges.Contains(challenge.GetId()))
			{
				list3.Add(challenge);
			}
		}
		foreach (Challenge item in list)
		{
			item.SetState(0);
		}
		foreach (Challenge item2 in list2)
		{
			item2.SetState(1);
		}
		foreach (Challenge item3 in list3)
		{
			item3.SetState(2);
		}
	}

	public void ActivateChallenges(int i_idStage)
	{
		foreach (Challenge challenge in m_challenges)
		{
			if ((!(challenge.GetStageAssociated() != null) || challenge.GetStageAssociated().GetId() == i_idStage) && challenge.GetState() == 0)
			{
				challenge.Activate();
			}
		}
	}

	public void DeActivateAllChallenges()
	{
		foreach (Challenge challenge in m_challenges)
		{
			if (challenge.IsActive())
			{
				challenge.DeActivate();
			}
		}
	}

	public void CompleteChallenge(Challenge i_challenge)
	{
		if (i_challenge == null || i_challenge.GetState() != 0)
		{
			return;
		}
		i_challenge.SetState(1);
		CommonReferences.Instance.GetManagerHud().CompleteChallenge(i_challenge);
		ManagerDB.CompleteChallenge(i_challenge);
		foreach (Clothing item in i_challenge.GetRewardsClothing())
		{
			ManagerDB.UnlockClothing(item);
		}
		this.OnChallengeComplete?.Invoke(i_challenge);
	}

	public List<Challenge> GetAllChallenges()
	{
		if (m_challenges.Count == 0)
		{
			Challenge[] componentsInChildren = GetComponentsInChildren<Challenge>(includeInactive: true);
			foreach (Challenge item in componentsInChildren)
			{
				m_challenges.Add(item);
			}
		}
		return m_challenges;
	}

	public Challenge GetChallenge(string i_nameChallenge)
	{
		foreach (Challenge allChallenge in GetAllChallenges())
		{
			if (allChallenge.GetName() == i_nameChallenge)
			{
				return allChallenge;
			}
		}
		return null;
	}

	public bool IsAllChallengesComplete()
	{
		bool result = true;
		foreach (Challenge allChallenge in GetAllChallenges())
		{
			if (allChallenge.GetState() == 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public void ResetAndReassignIds()
	{
		Challenge[] componentsInChildren = GetComponentsInChildren<Challenge>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetId(i + 1);
		}
	}

	public void UpdateUnsetIds()
	{
		int num = 0;
		Challenge[] componentsInChildren = GetComponentsInChildren<Challenge>(includeInactive: true);
		foreach (Challenge challenge in componentsInChildren)
		{
			if (challenge.GetId() != 0 && challenge.GetId() != -1 && challenge.GetId() > num)
			{
				num = challenge.GetId();
			}
		}
		componentsInChildren = GetComponentsInChildren<Challenge>(includeInactive: true);
		foreach (Challenge challenge2 in componentsInChildren)
		{
			if (challenge2.GetId() == 0)
			{
				challenge2.SetId(num + 1);
				num++;
			}
		}
	}
}
