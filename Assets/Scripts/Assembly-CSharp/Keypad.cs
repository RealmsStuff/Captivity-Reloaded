using System.Collections.Generic;
using UnityEngine;

public class Keypad : Interactable
{
	[SerializeField]
	private string m_code;

	[SerializeField]
	private Interactable m_interactableToActivateOnPass;

	private bool m_isCompleted;

	private readonly List<Door> m_modDoorTargets = new List<Door>();

	private readonly List<DoorRoller> m_modRollerTargets = new List<DoorRoller>();

	public void ConfigureModKeypad(string i_code, IEnumerable<Door> i_doors, IEnumerable<DoorRoller> i_rollerDoors)
	{
		ResetModActivationLinks();
		m_code = i_code;
		m_interactableToActivateOnPass = null;
		m_isCompleted = false;
		m_priceToActivate = 0;
		m_isSingleUse = false;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = true;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = false;
		m_isUnInteractable = false;
		m_modDoorTargets.Clear();
		m_modRollerTargets.Clear();
		if (i_doors != null) m_modDoorTargets.AddRange(i_doors);
		if (i_rollerDoors != null) m_modRollerTargets.AddRange(i_rollerDoors);
	}

	public bool IsUsableModTemplate()
	{
		return GetComponent<SpriteRenderer>() != null;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerHud().OpenKeypad(this);
	}

	public void SetCode(string i_code)
	{
		m_code = i_code;
	}

	public string GetCode()
	{
		return m_code;
	}

	public void Complete()
	{
		m_isCompleted = true;
		if (m_modDoorTargets.Count > 0 || m_modRollerTargets.Count > 0)
		{
			foreach (Door door in m_modDoorTargets) if (door != null) door.Open();
			foreach (DoorRoller door in m_modRollerTargets) if (door != null) door.Open();
		}
		else if (m_interactableToActivateOnPass != null)
			m_interactableToActivateOnPass.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
	}

	public bool GetIsCompleted()
	{
		return m_isCompleted;
	}
}
