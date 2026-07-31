using System;
using UnityEngine;

public class InputButtonXGame
{
	private string m_nameButton;

	private InputButton m_inputButton;

	private KeyCode m_keyCode;

	public InputButtonXGame(InputButton i_inputButton, KeyCode i_keyCode)
	{
		m_inputButton = i_inputButton;
		m_nameButton = m_inputButton.ToString();
		m_keyCode = i_keyCode;
	}

	public InputButtonXGame(string i_inputButton, string i_keyCode)
	{
		m_inputButton = (InputButton)Enum.Parse(typeof(InputButton), i_inputButton);
		m_nameButton = i_inputButton;
		m_keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), i_keyCode);
	}

	public string GetName()
	{
		return m_nameButton;
	}

	public InputButton GetInputButton()
	{
		return m_inputButton;
	}

	public KeyCode GetKeyCode()
	{
		return m_keyCode;
	}
}
