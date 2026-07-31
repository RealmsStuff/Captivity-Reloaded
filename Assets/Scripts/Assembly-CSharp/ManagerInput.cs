using System;
using System.Collections.Generic;
using UnityEngine;

public class ManagerInput : MonoBehaviour
{
	[SerializeField]
	private KeyCode m_keyDefaultLeft;

	[SerializeField]
	private KeyCode m_keyDefaultRight;

	[SerializeField]
	private KeyCode m_keyDefaultJump;

	[SerializeField]
	private KeyCode m_keyDefaultWalk;

	[SerializeField]
	private KeyCode m_keyDefaultCrouch;

	[SerializeField]
	private KeyCode m_keyDefaultDash;

	[SerializeField]
	private KeyCode m_keyDefaultFire;

	[SerializeField]
	private KeyCode m_keyDefaultReload;

	[SerializeField]
	private KeyCode m_keyDefaultUse;

	[SerializeField]
	private KeyCode m_keyDefaultPickUp;

	[SerializeField]
	private KeyCode m_keyDefaultEquipPrevious;

	[SerializeField]
	private KeyCode m_keyDefaultDropWeapon;

	[SerializeField]
	private KeyCode m_keyDefaultDrugSelection;

	[SerializeField]
	private KeyCode m_keyDefaultExpose;

	private List<InputButtonXGame> m_buttonsDefault = new List<InputButtonXGame>();

	private List<InputButtonXGame> m_buttons = new List<InputButtonXGame>();

	private void Awake()
	{
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.MoveLeft, m_keyDefaultLeft));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.MoveRight, m_keyDefaultRight));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Jump, m_keyDefaultJump));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Walk, m_keyDefaultWalk));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Crouch, m_keyDefaultCrouch));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Dash, m_keyDefaultDash));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Fire, m_keyDefaultFire));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Reload, m_keyDefaultReload));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Use, m_keyDefaultUse));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.PickUp, m_keyDefaultPickUp));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.EquipPrevious, m_keyDefaultEquipPrevious));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.DropWeapon, m_keyDefaultDropWeapon));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.DrugSelection, m_keyDefaultDrugSelection));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Expose, m_keyDefaultExpose));
	}

	public bool IsButton(InputButton i_inputButton)
	{
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKey(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool IsButtonDown(InputButton i_inputButton)
	{
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKeyDown(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool IsButtonUp(InputButton i_inputButton)
	{
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKeyUp(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public List<KeyCode> GetAnyKey()
	{
		List<KeyCode> list = new List<KeyCode>();
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKey(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public KeyCode GetKeyAssignedToButton(InputButton i_inputButton)
	{
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				return m_buttons[i].GetKeyCode();
			}
		}
		return KeyCode.Alpha0;
	}

	public List<InputButtonXGame> GetButtonsDefault()
	{
		return m_buttonsDefault;
	}

	public void SetButtonsToDefault()
	{
		m_buttons.Clear();
		for (int i = 0; i < m_buttonsDefault.Count; i++)
		{
			m_buttons.Add(m_buttonsDefault[i]);
		}
	}

	public void SetButtonsToSavedButtons()
	{
		m_buttons.Clear();
		List<InputButtonXGame> inputButtons = ManagerDB.GetInputButtons();
		for (int i = 0; i < inputButtons.Count; i++)
		{
			m_buttons.Add(inputButtons[i]);
		}
	}

	public List<InputButtonXGame> GetButtons()
	{
		return m_buttons;
	}
}
