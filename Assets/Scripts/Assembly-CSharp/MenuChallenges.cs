using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuChallenges : Menu
{
	[SerializeField]
	private Dropdown m_dropDownStages;

	[SerializeField]
	private ChallengeItemHud m_challengeItemDefault;

	private List<ChallengeItemHud> m_challengeItems = new List<ChallengeItemHud>();

	private int m_lastDropdownValue = -1;

	private float m_nextDropdownChangeTime;

	private void Start()
	{
		m_challengeItemDefault.gameObject.SetActive(value: false);
		m_dropDownStages.options.Clear();
		List<string> list = new List<string>();
		list.Add("Select a location...");
		foreach (Stage allStage in CommonReferences.Instance.GetManagerStages().GetAllStages())
		{
			if (!(allStage is StageHub) && !allStage.GetIsRuntimeTemplate())
			{
				list.Add(allStage.GetName());
			}
		}
		list.Add("General");
		m_dropDownStages.AddOptions(list);
	}

	public override void Open()
	{
		base.Open();
		m_lastDropdownValue = -1;
		m_nextDropdownChangeTime = 0f;
		m_dropDownStages.SetValueWithoutNotify(0);
		m_dropDownStages.RefreshShownValue();
	}

	public void OnDropdownValueChanged()
	{
		int value = m_dropDownStages.value;
		if (m_lastDropdownValue == value && Time.unscaledTime < m_nextDropdownChangeTime)
		{
			return;
		}
		m_lastDropdownValue = value;
		m_nextDropdownChangeTime = Time.unscaledTime + 0.25f;
		Stage stage = null;
		foreach (Stage allStage in CommonReferences.Instance.GetManagerStages().GetAllStages())
		{
			if (allStage.GetName() == m_dropDownStages.options[m_dropDownStages.value].text)
			{
				stage = allStage;
				break;
			}
		}
		ClearChallengeItems();
		if (!(stage == null) || !(m_dropDownStages.options[m_dropDownStages.value].text != "General"))
		{
			BuildChallengeItems(stage);
		}
	}

	private void BuildChallengeItems(Stage i_stage)
	{
		if (i_stage == null)
		{
			foreach (Challenge allChallenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
			{
				if (allChallenge.GetStageAssociated() == null)
				{
					CreateChallengeItem(allChallenge);
				}
			}
			return;
		}
		foreach (Challenge allChallenge2 in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
		{
			if (!(allChallenge2.GetStageAssociated() == null) && allChallenge2.GetStageAssociated().GetId() == i_stage.GetId())
			{
				CreateChallengeItem(allChallenge2);
			}
		}
	}

	private void CreateChallengeItem(Challenge i_challenge)
	{
		ChallengeItemHud challengeItemHud = Object.Instantiate(m_challengeItemDefault, m_challengeItemDefault.transform.parent);
		challengeItemHud.gameObject.SetActive(value: true);
		challengeItemHud.Initialize(i_challenge);
		m_challengeItems.Add(challengeItemHud);
		if (i_challenge.GetState() == 1)
		{
			ManagerDB.SetChallengeToSeen(i_challenge);
			i_challenge.SetState(2);
		}
	}

	private void ClearChallengeItems()
	{
		foreach (ChallengeItemHud challengeItem in m_challengeItems)
		{
			challengeItem.gameObject.SetActive(value: false);
			Object.Destroy(challengeItem.gameObject);
		}
		m_challengeItems.Clear();
	}
}
