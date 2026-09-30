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

	public void InitializeControllerDisplay(string i_name, string i_binding, InputButton i_input)
	{
		InitializeDisplay(i_name, i_binding);
		Image glyph = InputGlyphLibrary.GetOrCreateImage(m_txtKey.transform.parent, "ControllerBindingGlyph");
		glyph.sprite = InputGlyphLibrary.GetControllerSprite(i_input);
		RectTransform rect = glyph.rectTransform;
		rect.anchorMin = new Vector2(1f, 0.5f);
		rect.anchorMax = new Vector2(1f, 0.5f);
		rect.pivot = new Vector2(1f, 0.5f);
		rect.anchoredPosition = new Vector2(-8f, 0f);
		rect.sizeDelta = new Vector2(34f, 34f);
		RectTransform keyRect = m_txtKey.rectTransform;
		Vector2 offsetMax = keyRect.offsetMax;
		offsetMax.x -= 42f;
		keyRect.offsetMax = offsetMax;
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
