using UnityEngine;
using UnityEngine.UI;

public class HealthDisplayItem : MonoBehaviour
{
	[SerializeField]
	private GameObject m_healthDisplayItemParent;

	[SerializeField]
	private Image m_imgMeterBg;

	[SerializeField]
	private Image m_imgMeterCurrent;

	[SerializeField]
	private Image m_imgMeterBar;

	private NPC m_npc;

	private bool m_isShowing;

	private bool m_isShowingInternal;

	public void Initialize(NPC i_npc)
	{
		m_npc = i_npc;
		UpdateMeter();
		UpdatePos();
		m_isShowing = true;
		HideInternal();
	}

	private void Update()
	{
		if (m_npc.IsDead() || m_npc == null)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			if (!m_isShowing)
			{
				return;
			}
			if (m_isShowingInternal)
			{
				if (m_npc.GetHealthCurrent() == m_npc.GetStat("HealthMax").GetValueTotal())
				{
					HideInternal();
				}
			}
			else if (m_npc.GetHealthCurrent() < m_npc.GetStat("HealthMax").GetValueTotal())
			{
				ShowInternal();
			}
			UpdateMeter();
			UpdatePos();
		}
	}

	private void UpdateMeter()
	{
		Vector3 vector = m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imgMeterBar.GetComponent<RectTransform>().rect.width / m_npc.GetStat("HealthMax").GetValueTotal() * m_npc.GetHealthCurrent() - m_imgMeterBar.GetComponent<RectTransform>().rect.width;
		m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition = vector;
	}

	private void UpdatePos()
	{
		Vector3 position = m_npc.GetSkeletonActor().GetBoneHead().transform.position;
		position.y += m_npc.GetSkeletonActor().GetBoneHead().GetBodyPart()
			.GetComponent<SpriteRenderer>()
			.bounds.size.y;
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(position);
	}

	private void ShowInternal()
	{
		m_healthDisplayItemParent.SetActive(value: true);
		m_isShowingInternal = true;
	}

	private void HideInternal()
	{
		m_healthDisplayItemParent.SetActive(value: false);
		m_isShowingInternal = false;
	}

	public void Hide()
	{
		HideInternal();
		m_isShowing = false;
	}

	public void Show()
	{
		m_isShowing = true;
	}

	public NPC GetNpc()
	{
		return m_npc;
	}
}
