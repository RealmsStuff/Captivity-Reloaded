using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuLocations : Menu
{
	[SerializeField]
	private StageMenuItem m_stageMenuItemDefault;

	[SerializeField]
	private Text m_txtTitleLvl;

	[SerializeField]
	private Text m_txtDescriptionLvl;

	[SerializeField]
	private Text m_txtHighscore;

	[SerializeField]
	private UnityEngine.UI.Button m_btnGo;

	private Stage m_stageSelected;
	private ManagerStages m_managerStages;

	private void Start()
	{
		m_btnGo.interactable = false;
		m_stageMenuItemDefault.gameObject.SetActive(value: false);
		m_managerStages = CommonReferences.Instance.GetManagerStages();
		m_managerStages.OnRuntimeStageAdded += AddStageMenuItem;
		BuildStageMenuItems();
	}

	private void OnDestroy()
	{
		if (m_managerStages != null) m_managerStages.OnRuntimeStageAdded -= AddStageMenuItem;
	}

	private void BuildStageMenuItems()
	{
		foreach (Stage allStage in CommonReferences.Instance.GetManagerStages().GetAllStages())
		{
			AddStageMenuItem(allStage);
		}
	}

	private void AddStageMenuItem(Stage i_stage)
	{
		if (i_stage == null || i_stage is StageHub) return;
		StageMenuItem stageMenuItem = Object.Instantiate(m_stageMenuItemDefault, m_stageMenuItemDefault.transform.parent);
		stageMenuItem.SetStage(i_stage);
		stageMenuItem.gameObject.SetActive(value: true);
	}

	public void Go()
	{
		CommonReferences.Instance.GetManagerScreens().GetScreenGame().OpenStage(m_stageSelected);
		CommonReferences.Instance.GetManagerHud().CloseHubMainMenu();
	}

	public void SetLevelSelected(Stage i_stage)
	{
		m_stageSelected = i_stage;
		m_txtTitleLvl.text = m_stageSelected.GetName();
		m_txtDescriptionLvl.text = m_stageSelected.GetDescription();
		int highscore = m_stageSelected.GetHighscore();
		if (highscore != 0)
		{
			m_txtHighscore.text = "Highscore: Wave " + highscore;
		}
		else
		{
			m_txtHighscore.text = "";
		}
		StopAllCoroutines();
		StartCoroutine(CoroutineTypeWriterEffect(m_stageSelected.GetDescription()));
		m_btnGo.interactable = true;
	}

	private IEnumerator CoroutineTypeWriterEffect(string i_text)
	{
		m_txtDescriptionLvl.text = "";
		for (int l_index = 0; l_index < i_text.Length; l_index++)
		{
			m_txtDescriptionLvl.text += i_text[l_index];
			yield return new WaitForEndOfFrame();
		}
	}
}
