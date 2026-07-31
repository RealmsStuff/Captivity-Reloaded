using UnityEngine;
using UnityEngine.UI;

public class InfoNPCArchive : MonoBehaviour
{
	[SerializeField]
	private Text m_txtName;

	[SerializeField]
	private Text m_txtDescription;

	[SerializeField]
	private Text m_txtStats;

	[SerializeField]
	private UnityEngine.UI.Button m_btnDrawing;

	[SerializeField]
	private GameObject m_boxDrawingZoom;

	public void SetAndShowInfoNPC(NPC i_npc)
	{
		m_txtName.text = i_npc.GetName();
		m_txtDescription.text = i_npc.GetDescription();
		m_txtStats.text = "Health: " + i_npc.GetStat("HealthMax").GetValueTotal() + "\nDamage: " + i_npc.GetStat("Damage").GetValueTotal() + "\nAcceleration: " + i_npc.GetStat("SpeedAccel").GetValueTotal() + "\nMax Speed: " + i_npc.GetStat("SpeedMax").GetValueTotal();
		if (i_npc.GetIsRaper())
		{
			string text = "";
			if (i_npc.GetRaper() is RaperSmasher)
			{
				text = "Smasher";
			}
			Text txtStats = m_txtStats;
			txtStats.text = txtStats.text + "\n---\nType Raper: " + text;
			if (i_npc.GetRaper().GetFetusesToInsert().Count > 0)
			{
				Text txtStats2 = m_txtStats;
				txtStats2.text = txtStats2.text + "\nBreeds: " + i_npc.GetRaper().GetFetusesToInsert()[0].GetName() + "(s)";
			}
			else
			{
				m_txtStats.text += "\nBreeds: Nothing";
			}
			if (i_npc.GetSprIcon() != null)
			{
				m_btnDrawing.gameObject.SetActive(value: true);
				m_btnDrawing.GetComponent<Image>().sprite = i_npc.GetSprIcon();
				m_boxDrawingZoom.GetComponentInChildren<UnityEngine.UI.Button>().GetComponent<Image>().sprite = i_npc.GetSprIcon();
			}
			else
			{
				m_btnDrawing.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_btnDrawing.gameObject.SetActive(value: false);
		}
	}

	public void ShowBoxDrawingZoomIn()
	{
		m_boxDrawingZoom.SetActive(value: true);
	}

	public void HideBoxDrawingZoomIn()
	{
		m_boxDrawingZoom.SetActive(value: false);
	}
}
