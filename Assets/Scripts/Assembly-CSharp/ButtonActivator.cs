using System.Collections.Generic;
using UnityEngine;

public class ButtonActivator : Button
{
	[SerializeField]
	private List<Interactable> m_interactablesToActivate = new List<Interactable>();

	protected override void HandlePressButton()
	{
		foreach (Interactable item in m_interactablesToActivate)
		{
			item.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
		}
	}
}
