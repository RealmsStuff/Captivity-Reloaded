using UnityEngine;
using UnityEngine.UI;

public class InputBox : MonoBehaviour
{
	[SerializeField]
	private Text m_txtInputName;

	[SerializeField]
	private Text m_txtKey;

	private InputButtonXGame m_button;

	public void Initialize(InputButtonXGame i_button)
	{
		m_button = i_button;
		m_txtInputName.text = m_button.GetName();
		m_txtKey.text = m_button.GetKeyCode().ToString();
	}

	public void InitializeDisplay(string i_name, string i_binding)
	{
		m_txtInputName.text = i_name;
		m_txtKey.text = i_binding;
		Button component = GetComponent<Button>();
		if (component != null)
		{
			component.enabled = false;
		}
	}

	public void SetToListenInput()
	{
		m_txtKey.text = "Press a button to set...";
	}

	public string GetNameButton()
	{
		return m_txtInputName.text;
	}
}
