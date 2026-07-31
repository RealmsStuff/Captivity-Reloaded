public class Slot
{
	private PickUpable m_pickUpable;

	private int m_numSlot;

	public Slot(int i_numSlot)
	{
		m_numSlot = i_numSlot;
	}

	public void Fill(PickUpable i_pickUpable)
	{
		if (m_pickUpable != null)
		{
			if (i_pickUpable.GetIsStackable() && m_pickUpable.GetIsStackable())
			{
				m_pickUpable.IncreaseAmount(i_pickUpable.GetAmount());
			}
		}
		else
		{
			m_pickUpable = i_pickUpable;
		}
	}

	public void SwapBetweenSlots(Slot i_slotToSwapWith)
	{
		PickUpable pickUpable = i_slotToSwapWith.GetPickUpable();
		i_slotToSwapWith.Fill(m_pickUpable);
		Fill(pickUpable);
	}

	public void RemoveItem()
	{
		m_pickUpable = null;
	}

	public PickUpable GetPickUpable()
	{
		return m_pickUpable;
	}

	public bool GetIsFilled()
	{
		if (m_pickUpable == null)
		{
			return false;
		}
		return true;
	}

	public int GetNumSlot()
	{
		return m_numSlot;
	}
}
