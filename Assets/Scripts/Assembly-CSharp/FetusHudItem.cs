using UnityEngine;
using UnityEngine.UI;

public class FetusHudItem : MonoBehaviour
{
	[SerializeField]
	private Image m_imgBarFill;

	[SerializeField]
	private Image m_imgBarBorder;

	private Fetus m_fetus;

	private bool m_isUpdate;

	public void SetFetus(Fetus i_fetus)
	{
		m_fetus = i_fetus;
		GetComponentInChildren<Text>().text = m_fetus.GetNameFetus();
		m_isUpdate = true;
	}

	private void Update()
	{
		if (m_isUpdate)
		{
			UpdateBar();
		}
	}

	private void UpdateBar()
	{
		Vector3 vector = m_imgBarFill.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imgBarBorder.GetComponent<RectTransform>().rect.width / m_fetus.GetTimePregnantMax() * (m_fetus.GetTimePregnantMax() - m_fetus.GetTimePregnantLeft()) - m_imgBarBorder.GetComponent<RectTransform>().rect.width;
		m_imgBarFill.GetComponent<RectTransform>().anchoredPosition = vector;
	}

	public Fetus GetFetus()
	{
		return m_fetus;
	}

	public void Clear()
	{
		m_isUpdate = false;
		m_fetus = null;
		GetComponentInChildren<Text>().text = "";
	}

	public bool GetIsFilled()
	{
		if (m_fetus == null)
		{
			return false;
		}
		return true;
	}
}
