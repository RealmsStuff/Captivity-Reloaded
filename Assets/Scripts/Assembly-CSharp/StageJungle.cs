using System.Collections.Generic;

public class StageJungle : Stage
{
	private List<PickUpable> m_fetishesDropped = new List<PickUpable>();

	public void SetFetishDropped(PickUpable i_fetish)
	{
		m_fetishesDropped.Add(i_fetish);
	}

	public bool IsFetishDropped(PickUpable i_fetish)
	{
		if (m_fetishesDropped.Contains(i_fetish))
		{
			return true;
		}
		return false;
	}
}
