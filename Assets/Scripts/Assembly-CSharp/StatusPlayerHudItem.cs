using UnityEngine;
using UnityEngine.UI;

public class StatusPlayerHudItem : MonoBehaviour
{
	[SerializeField]
	private Text m_txtNameStatus;

	[SerializeField]
	private Text m_txtDescriptionStatus;
	private InputButton? m_promptInput;
	private Image m_promptGlyph;

	public void Initialise(string i_name, string i_description, Color i_color)
	{
		GetComponent<Image>().color = i_color;
		m_txtNameStatus.color = i_color;
		m_txtDescriptionStatus.color = new Color(m_txtDescriptionStatus.color.r, m_txtDescriptionStatus.color.g, m_txtDescriptionStatus.color.b, i_color.a);
		m_txtNameStatus.text = i_name;
		m_txtDescriptionStatus.text = i_description;
	}

	public string GetName()
	{
		return m_txtNameStatus.text;
	}

	public void SetText(string i_name, string i_description)
	{
		m_txtNameStatus.text = i_name;
		m_txtDescriptionStatus.text = i_description;
	}

	public void SetPromptInput(InputButton i_input)
	{
		m_promptInput = i_input;
		RefreshPromptGlyph();
	}

	private void Update()
	{
		if (m_promptInput.HasValue) RefreshPromptGlyph();
	}

	private void RefreshPromptGlyph()
	{
		Sprite sprite = InputGlyphLibrary.GetPromptSprite(m_promptInput.Value);
		if (sprite == null && m_promptGlyph == null) return;
		if (m_promptGlyph == null)
		{
			m_promptGlyph = InputGlyphLibrary.GetOrCreateImage(transform, "InputPromptGlyph");
			RectTransform rect = m_promptGlyph.rectTransform;
			rect.anchorMin = new Vector2(1f, 0.5f);
			rect.anchorMax = new Vector2(1f, 0.5f);
			rect.pivot = new Vector2(1f, 0.5f);
			rect.anchoredPosition = new Vector2(-8f, 0f);
			rect.sizeDelta = new Vector2(30f, 30f);
		}
		m_promptGlyph.sprite = sprite;
		m_promptGlyph.enabled = sprite != null;
	}

	public Color GetColor()
	{
		return m_txtNameStatus.color;
	}

	public void SetColor(Color i_color)
	{
		GetComponent<Image>().color = i_color;
		m_txtNameStatus.color = i_color;
		m_txtDescriptionStatus.color = new Color(m_txtDescriptionStatus.color.r, m_txtDescriptionStatus.color.g, m_txtDescriptionStatus.color.b, i_color.a);
	}
}
