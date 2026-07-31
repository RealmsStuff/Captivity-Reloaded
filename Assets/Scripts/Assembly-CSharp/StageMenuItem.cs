using UnityEngine;
using UnityEngine.UI;

public class StageMenuItem : MonoBehaviour
{
	private Stage m_stage;

	public void SetStage(Stage i_stage)
	{
		m_stage = i_stage;
		GetComponentInChildren<Text>().text = m_stage.GetName();
	}

	public void SelectStage()
	{
		GetComponentInParent<MenuLocations>().SetLevelSelected(m_stage);
	}
}
