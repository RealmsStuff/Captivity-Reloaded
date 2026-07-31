using UnityEngine;
using UnityEngine.UI;

public class ClothingHudItem : MonoBehaviour
{
	[SerializeField]
	private Image m_imgBorder;

	private Clothing m_clothing;

	public void SetClothing(Clothing i_clothing)
	{
		m_clothing = i_clothing;
		GetComponentInChildren<UnityEngine.UI.Button>().GetComponent<Image>().sprite = i_clothing.GetIcon();
	}

	public void SetIsSelected(bool i_isSelected)
	{
		if (i_isSelected)
		{
			m_imgBorder.color = Color.blue;
		}
		else
		{
			m_imgBorder.color = Color.white;
		}
	}

	public void SelectClothing()
	{
		GetComponentInParent<WardrobeHud>().ClickClothingItem(this);
	}

	public Clothing GetClothing()
	{
		return m_clothing;
	}
}
