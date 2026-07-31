using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : HudBar
{
	[SerializeField]
	private Image m_imageHealthEmpty;

	[SerializeField]
	private Image m_imageHealthRapeAble;

	[SerializeField]
	private Color m_colorHealthNormal;

	[SerializeField]
	private Color m_colorHealthLow;

	private void Start()
	{
	}

	private IEnumerator CoroutineWaitInitialization()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		m_valueMax = CommonReferences.Instance.GetPlayer().GetStat("HealthMax").GetValueTotal();
		m_colorHealthNormal = m_imageValue.color;
	}

	protected override void UpdateValues()
	{
		m_valueMax = CommonReferences.Instance.GetPlayer().GetStat("HealthMax").GetValueTotal();
		m_valueCurrent = CommonReferences.Instance.GetPlayer().GetHealthCurrent();
	}

	protected override void Update()
	{
		base.Update();
		if (m_valueCurrent < 1f)
		{
			m_imageHealthRapeAble.gameObject.SetActive(value: true);
		}
		else
		{
			m_imageHealthRapeAble.gameObject.SetActive(value: false);
		}
		if (m_valueCurrent < m_valueMax / 4f && m_imageValue.color == m_colorHealthNormal)
		{
			m_imageValue.color = GetCorrectColorHealthBar();
		}
		if (m_valueCurrent >= m_valueMax / 4f && m_imageValue.color == m_colorHealthLow)
		{
			m_imageValue.color = GetCorrectColorHealthBar();
		}
	}

	public override void Increase()
	{
		base.Increase();
		FlashTakeDamage(3);
	}

	private void FlashTakeDamage(int i_amountOfTimes)
	{
		StartCoroutine(CoroutineFlashTakeDamage(i_amountOfTimes));
	}

	private IEnumerator CoroutineFlashTakeDamage(int i_amountOfTimes)
	{
		for (int l_index = 0; l_index < i_amountOfTimes; l_index++)
		{
			m_imageValue.color = new Color(1f, 0f, 0f, GetCorrectColorHealthBar().a);
			yield return new WaitForSeconds(0.1f);
			m_imageValue.color = GetCorrectColorHealthBar();
			yield return new WaitForSeconds(0.1f);
		}
		m_imageValue.color = GetCorrectColorHealthBar();
	}

	private Color GetCorrectColorHealthBar()
	{
		if (CommonReferences.Instance.GetPlayer().GetHealthCurrent() < m_valueMax / 4f)
		{
			return m_colorHealthLow;
		}
		return m_colorHealthNormal;
	}

	public override void ResetBar()
	{
		base.ResetBar();
		m_imageValue.color = m_colorHealthNormal;
	}
}
