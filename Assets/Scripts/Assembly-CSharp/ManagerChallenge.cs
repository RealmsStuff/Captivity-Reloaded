using System.Collections.Generic;
using CaptivityReloaded.Modding;
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
		GrantModRewards(i_challenge);
		this.OnChallengeComplete?.Invoke(i_challenge);
	}

	private static void GrantModRewards(Challenge i_challenge)
	{
		if (i_challenge.GetModRewardCurrency() > 0)
			CommonReferences.Instance.GetPlayerController().GainMoney(i_challenge.GetModRewardCurrency());
		foreach (ContentId id in i_challenge.GetModRewardContent())
			ModContentUnlockState.Unlock(id);
		foreach (ChallengeGrantDefinition reward in i_challenge.GetModRewardItems()) GrantPickup(reward, false);
		foreach (ChallengeGrantDefinition reward in i_challenge.GetModRewardWeapons()) GrantPickup(reward, true);
	}

	private static void GrantPickup(ChallengeGrantDefinition i_reward, bool i_requireWeapon)
	{
		if (!ModLoaderRuntime.Registry.TryGet(i_reward.Id, out ContentRegistration entry) || !(entry.RuntimeAsset is PickUpable template)
			|| (i_requireWeapon && !(template is Weapon)))
		{
			ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, "challenge.reward-not-bound",
				"Challenge reward is not a bound " + (i_requireWeapon ? "weapon" : "item") + ": " + i_reward.Id);
			return;
		}
		Player player = CommonReferences.Instance.GetPlayer();
		if (player == null) return;
		if (template.GetIsStackable())
		{
			PickUpable granted = Object.Instantiate(template, player.transform.parent);
			granted.ConfigureModRewardAmount(i_reward.Amount);
			player.PickUp(granted, i_isDuplicate: false);
			return;
		}
		for (int index = 0; index < i_reward.Amount; index++)
			player.PickUp(Object.Instantiate(template, player.transform.parent), i_isDuplicate: false);
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

	public void AddRuntimeChallenge(Challenge i_challenge)
	{
		GetAllChallenges();
		if (i_challenge != null && !m_challenges.Contains(i_challenge)) m_challenges.Add(i_challenge);
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
