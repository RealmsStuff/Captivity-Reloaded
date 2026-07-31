using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHud : MonoBehaviour
{
	[SerializeField]
	private List<Text> m_txtsAmmoMagazine = new List<Text>();

	[SerializeField]
	private List<Text> m_txtsAmmoLeft = new List<Text>();

	[SerializeField]
	private Text m_txtSlash;

	private Player m_player;

	private Gun m_gunCurrent;

	private bool m_isFacingLeft = true;

	private float[] m_posXTxtsAmmoMagazine = new float[2];

	private float[] m_posXTxtsAmmoLeft = new float[2];

	private float m_posXTxtSlash;

	private void Start()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		StoreData();
	}

	private void StoreData()
	{
		m_posXTxtsAmmoMagazine[0] = m_txtsAmmoMagazine[0].GetComponent<RectTransform>().anchoredPosition.x;
		m_posXTxtsAmmoMagazine[1] = m_txtsAmmoMagazine[1].GetComponent<RectTransform>().anchoredPosition.x;
		m_posXTxtsAmmoLeft[0] = m_txtsAmmoLeft[0].GetComponent<RectTransform>().anchoredPosition.x;
		m_posXTxtsAmmoLeft[1] = m_txtsAmmoLeft[1].GetComponent<RectTransform>().anchoredPosition.x;
		m_posXTxtSlash = m_txtSlash.GetComponent<RectTransform>().anchoredPosition.x;
	}

	public void ShowAmmoGun(Gun i_gun)
	{
		m_gunCurrent = i_gun;
	}

	private void LateUpdate()
	{
		if (m_gunCurrent != null)
		{
			m_player = CommonReferences.Instance.GetPlayer();
			UpdateAmmo();
			UpdatePos();
			UpdateSide();
		}
	}

	private void UpdateAmmo()
	{
		if (m_gunCurrent.GetIsAmmoInfinite())
		{
			m_txtsAmmoLeft[0].enabled = false;
			m_txtsAmmoLeft[1].enabled = false;
			m_txtSlash.enabled = false;
		}
		else
		{
			if (!m_txtsAmmoLeft[0].enabled)
			{
				m_txtsAmmoLeft[0].enabled = true;
				m_txtsAmmoLeft[1].enabled = true;
			}
			if (!m_txtSlash.enabled)
			{
				m_txtSlash.enabled = true;
			}
		}
		UpdateAmmoCount();
		UpdateTextColor();
	}

	private void UpdateAmmoCount()
	{
		m_txtsAmmoMagazine[0].text = m_gunCurrent.GetAmmoMagazineLeft().ToString();
		m_txtsAmmoMagazine[1].text = m_gunCurrent.GetAmmoMagazineLeft().ToString();
		m_txtsAmmoLeft[0].text = m_gunCurrent.GetAmmoLeft().ToString();
		m_txtsAmmoLeft[1].text = m_gunCurrent.GetAmmoLeft().ToString();
	}

	private void UpdateTextColor()
	{
		if ((float)m_gunCurrent.GetAmmoMagazineLeft() < (float)m_gunCurrent.GetAmmoMagazineMax() / 3f)
		{
			m_txtsAmmoMagazine[0].color = Color.red;
		}
		else
		{
			m_txtsAmmoMagazine[0].color = Color.white;
		}
		if ((float)m_gunCurrent.GetAmmoLeft() < (float)m_gunCurrent.GetAmmoMagazineMax() * 1.5f)
		{
			m_txtsAmmoLeft[0].color = Color.red;
		}
		else
		{
			m_txtsAmmoLeft[0].color = Color.white;
		}
	}

	private void UpdatePos()
	{
		Vector2 i_posWorld = m_gunCurrent.transform.position;
		i_posWorld -= (Vector2)m_gunCurrent.transform.up * 0.5f;
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(i_posWorld);
		GetComponent<RectTransform>().rotation = m_gunCurrent.transform.rotation;
	}

	private void UpdateSide()
	{
		if (m_isFacingLeft != m_player.GetIsFacingLeft())
		{
			if (m_player.GetIsFacingLeft() && !m_isFacingLeft)
			{
				SideLeft();
				m_isFacingLeft = true;
			}
			if (!m_player.GetIsFacingLeft() && m_isFacingLeft)
			{
				SideRight();
				m_isFacingLeft = false;
			}
		}
	}

	private void SideLeft()
	{
		Vector2 anchoredPosition = m_txtsAmmoMagazine[0].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition2 = m_txtsAmmoMagazine[1].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition3 = m_txtsAmmoLeft[0].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition4 = m_txtsAmmoLeft[1].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition5 = m_txtSlash.GetComponent<RectTransform>().anchoredPosition;
		anchoredPosition.x = m_posXTxtsAmmoMagazine[0];
		anchoredPosition2.x = m_posXTxtsAmmoMagazine[1];
		anchoredPosition3.x = m_posXTxtsAmmoLeft[0];
		anchoredPosition4.x = m_posXTxtsAmmoLeft[1];
		anchoredPosition5.x = m_posXTxtSlash;
		m_txtsAmmoMagazine[0].GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		m_txtsAmmoMagazine[1].GetComponent<RectTransform>().anchoredPosition = anchoredPosition2;
		m_txtsAmmoLeft[0].GetComponent<RectTransform>().anchoredPosition = anchoredPosition3;
		m_txtsAmmoLeft[1].GetComponent<RectTransform>().anchoredPosition = anchoredPosition4;
		m_txtSlash.GetComponent<RectTransform>().anchoredPosition = anchoredPosition5;
		m_txtsAmmoMagazine[0].alignment = TextAnchor.MiddleRight;
		m_txtsAmmoMagazine[1].alignment = TextAnchor.MiddleRight;
		m_txtsAmmoLeft[0].alignment = TextAnchor.MiddleLeft;
		m_txtsAmmoLeft[1].alignment = TextAnchor.MiddleLeft;
		m_txtSlash.alignment = TextAnchor.MiddleLeft;
		m_txtSlash.text = "/";
	}

	private void SideRight()
	{
		Vector2 anchoredPosition = m_txtsAmmoMagazine[0].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition2 = m_txtsAmmoMagazine[1].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition3 = m_txtsAmmoLeft[0].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition4 = m_txtsAmmoLeft[1].GetComponent<RectTransform>().anchoredPosition;
		Vector2 anchoredPosition5 = m_txtSlash.GetComponent<RectTransform>().anchoredPosition;
		anchoredPosition.x = m_posXTxtsAmmoMagazine[0] * -1f;
		anchoredPosition2.x = m_posXTxtsAmmoMagazine[1] * -1f;
		anchoredPosition3.x = m_posXTxtsAmmoLeft[0] * -1f;
		anchoredPosition4.x = m_posXTxtsAmmoLeft[1] * -1f;
		anchoredPosition5.x = m_posXTxtSlash * -1f;
		m_txtsAmmoMagazine[0].GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		m_txtsAmmoMagazine[1].GetComponent<RectTransform>().anchoredPosition = anchoredPosition2;
		m_txtsAmmoLeft[0].GetComponent<RectTransform>().anchoredPosition = anchoredPosition3;
		m_txtsAmmoLeft[1].GetComponent<RectTransform>().anchoredPosition = anchoredPosition4;
		m_txtSlash.GetComponent<RectTransform>().anchoredPosition = anchoredPosition5;
		m_txtsAmmoMagazine[0].alignment = TextAnchor.MiddleLeft;
		m_txtsAmmoMagazine[1].alignment = TextAnchor.MiddleLeft;
		m_txtsAmmoLeft[0].alignment = TextAnchor.MiddleRight;
		m_txtsAmmoLeft[1].alignment = TextAnchor.MiddleRight;
		m_txtSlash.alignment = TextAnchor.MiddleRight;
		m_txtSlash.text = "\\";
	}
}
