using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerFetusHud : MonoBehaviour
{
	[SerializeField]
	private List<FetusHudItem> m_fetusHudItems = new List<FetusHudItem>();

	[SerializeField]
	private GameObject m_fetusHud;

	[SerializeField]
	private Text m_txtCountFetus;

	private Vector2 m_pointHide;

	private List<Fetus> m_fetuses = new List<Fetus>();

	private float m_heightFetusHud;

	private float m_heightFetusItem;

	private float m_spaceBetweenFetusItems;

	private void Awake()
	{
		m_heightFetusHud = m_fetusHud.GetComponent<RectTransform>().sizeDelta.y;
		m_heightFetusItem = m_fetusHudItems[0].GetComponent<RectTransform>().sizeDelta.y;
		m_spaceBetweenFetusItems = 8f;
		m_pointHide = new Vector2(m_fetusHud.GetComponent<RectTransform>().anchoredPosition.x, m_heightFetusHud / 2f);
	}

	public void AddFetus(Fetus i_fetus)
	{
		m_fetuses.Add(i_fetus);
		if (m_fetuses.Count > 0)
		{
			Show();
		}
		FillFetusItems();
		if (base.isActiveAndEnabled && m_fetuses.Count <= 3)
		{
			StartCoroutine(CoroutineAdjustFetusHudPos());
		}
	}

	private void FillFetusItems()
	{
		int num = 0;
		foreach (Fetus item in GetFetusesSortedFromLowestToHighestPregnancyTimeLeft())
		{
			m_fetusHudItems[num].SetFetus(item);
			if (num == m_fetusHudItems.Count - 1)
			{
				break;
			}
			num++;
		}
		m_txtCountFetus.text = m_fetuses.Count.ToString();
	}

	private List<Fetus> GetFetusesSortedFromLowestToHighestPregnancyTimeLeft()
	{
		List<Fetus> list = new List<Fetus>();
		foreach (Fetus fetuse in m_fetuses)
		{
			list.Add(fetuse);
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (i > j && list[j].GetTimePregnantLeft() > list[i].GetTimePregnantLeft())
					{
						Fetus value = list[j];
						list[j] = list[i];
						list[i] = value;
						flag = false;
					}
				}
			}
		}
		return list;
	}

	private IEnumerator CoroutineAdjustFetusHudPos()
	{
		float l_posYFrom = m_fetusHud.GetComponent<RectTransform>().anchoredPosition.y;
		float l_posYTo = m_pointHide.y - (float)m_fetuses.Count * (m_heightFetusItem + m_spaceBetweenFetusItems);
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		_ = m_fetusHud.GetComponent<RectTransform>().anchoredPosition.y;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_posYFrom, l_posYTo, i_time);
			m_fetusHud.GetComponent<RectTransform>().anchoredPosition = new Vector2(m_fetusHud.GetComponent<RectTransform>().anchoredPosition.x, y);
			yield return new WaitForFixedUpdate();
		}
	}

	public void Show()
	{
		m_fetusHud.SetActive(value: true);
	}

	public void Hide()
	{
		m_fetusHud.SetActive(value: false);
	}

	public void AnimateHide()
	{
		StartCoroutine(CoroutineHideHud());
	}

	private IEnumerator CoroutineHideHud()
	{
		float l_posYFrom = m_fetusHud.GetComponent<RectTransform>().anchoredPosition.y;
		float l_posYTo = m_pointHide.y;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		_ = m_fetusHud.GetComponent<RectTransform>().anchoredPosition.y;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_posYFrom, l_posYTo, i_time);
			m_fetusHud.GetComponent<RectTransform>().anchoredPosition = new Vector2(m_fetusHud.GetComponent<RectTransform>().anchoredPosition.x, y);
			yield return new WaitForFixedUpdate();
		}
		Hide();
	}

	public void DestroyFetusItem(Fetus i_fetus)
	{
		m_fetuses.Remove(i_fetus);
		foreach (FetusHudItem fetusHudItem in m_fetusHudItems)
		{
			if (fetusHudItem.GetFetus() == i_fetus)
			{
				fetusHudItem.Clear();
				break;
			}
		}
		if (m_fetuses.Count < 1)
		{
			AnimateHide();
			return;
		}
		FillFetusItems();
		if (m_fetuses.Count <= 3)
		{
			StartCoroutine(CoroutineAdjustFetusHudPos());
		}
	}
}
