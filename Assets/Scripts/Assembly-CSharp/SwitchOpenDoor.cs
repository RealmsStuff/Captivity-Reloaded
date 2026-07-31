using System.Collections.Generic;
using UnityEngine;

public class SwitchOpenDoor : Switch
{
	[SerializeField]
	private List<Door> m_doorsToOpen = new List<Door>();

	public override void HandleSwitchOn()
	{
		foreach (Door item in m_doorsToOpen)
		{
			if (!(item == null))
			{
				item.OpenOrClose();
			}
		}
	}

	public override void HandleSwitchOff()
	{
		foreach (Door item in m_doorsToOpen)
		{
			if (!(item == null))
			{
				item.OpenOrClose();
			}
		}
	}
}
