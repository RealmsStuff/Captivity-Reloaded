using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
	[SerializeField]
	private GameObject m_staminaBar;

	[SerializeField]
	private Image m_imageBar;

	[SerializeField]
	private Image m_imageStamina;

	[SerializeField]
	private Image m_imageStaminaEmpty;

	private float m_staminaCurrent;

	private float m_staminaMax;

	private Color m_colorStaminaNormal;

	[SerializeField]
	private Color m_colorStaminaTired;

	private void Start()
	{
		StartCoroutine(CoroutineWaitInitialization());
		m_staminaBar.SetActive(value: false);
	}

	private IEnumerator CoroutineWaitInitialization()
	{
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		yield return new WaitForFixedUpdate();
		m_staminaMax = CommonReferences.Instance.GetPlayer().GetStat("HealthMax").GetValueTotal();
		m_colorStaminaNormal = m_imageStamina.color;
	}

	private void LateUpdate()
	{
		UpdateMeter();
		UpdatePos();
		if (m_staminaCurrent < m_staminaMax)
		{
			EnableBar();
		}
		else
		{
			DisableBar();
		}
	}

	private void UpdateMeter()
	{
		m_staminaCurrent = CommonReferences.Instance.GetPlayer().GetStaminaCurrent();
		Vector3 vector = m_imageStamina.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imageBar.GetComponent<RectTransform>().rect.width / m_staminaMax * m_staminaCurrent - m_imageBar.GetComponent<RectTransform>().rect.width;
		m_imageStamina.GetComponent<RectTransform>().anchoredPosition = vector;
		if (m_staminaCurrent < m_staminaMax / 4f && m_imageStamina.color == m_colorStaminaNormal)
		{
			m_imageStamina.color = GetCorrectColorStaminaBar();
		}
		if (m_staminaCurrent >= m_staminaMax / 4f && m_imageStamina.color == m_colorStaminaTired)
		{
			m_imageStamina.color = GetCorrectColorStaminaBar();
		}
		vector = m_imageStamina.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imageBar.GetComponent<RectTransform>().rect.width / m_staminaMax * m_staminaCurrent - m_imageBar.GetComponent<RectTransform>().rect.width;
		m_imageStamina.GetComponent<RectTransform>().anchoredPosition = vector;
	}

	private void UpdatePos()
	{
		Vector3 vector = CommonReferences.Instance.GetPlayer().GetPosFeet();
		vector.y -= 0.5f;
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(vector);
	}

	public void Show()
	{
		m_staminaBar.SetActive(value: true);
	}

	public void Hide()
	{
		m_staminaBar.SetActive(value: false);
	}

	private void EnableBar()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
	}

	private void DisableBar()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
	}

	public void FlashNoStamina(int i_amountOfTimes)
	{
		StartCoroutine(CoroutineFlashNoStamina(i_amountOfTimes));
	}

	private IEnumerator CoroutineFlashNoStamina(int i_amountOfTimes)
	{
		for (int l_index = 0; l_index < i_amountOfTimes; l_index++)
		{
			m_imageStaminaEmpty.color = new Color(1f, 0f, 0f, 1f);
			yield return new WaitForSeconds(0.1f);
			m_imageStaminaEmpty.color = new Color(1f, 0f, 0f, 0f);
			yield return new WaitForSeconds(0.1f);
		}
		m_imageStamina.color = GetCorrectColorStaminaBar();
	}

	private Color GetCorrectColorStaminaBar()
	{
		if (CommonReferences.Instance.GetPlayer().GetStaminaCurrent() < m_staminaMax / 4f)
		{
			return m_colorStaminaTired;
		}
		return m_colorStaminaNormal;
	}
}
