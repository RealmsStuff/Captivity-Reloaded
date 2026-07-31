using UnityEngine;
using UnityEngine.UI;

public class VendorItem : MonoBehaviour
{
	private PickUpable m_pickUpable;

	[SerializeField]
	private Text m_txtNameItem;

	[SerializeField]
	private Text m_txtPriceItem;

	private bool m_isOwnerVendor;

	public void Initialize(PickUpable i_pickUpable, bool i_isOwnerVendor)
	{
		m_pickUpable = i_pickUpable;
		m_txtNameItem.text = m_pickUpable.GetName();
		m_txtPriceItem.text = m_pickUpable.GetValue() + "$";
		m_isOwnerVendor = i_isOwnerVendor;
	}

	public PickUpable GetPickUpable()
	{
		return m_pickUpable;
	}

	public bool GetIsOwnerVendor()
	{
		return m_isOwnerVendor;
	}
}
