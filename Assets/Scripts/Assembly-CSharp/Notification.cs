using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
	public delegate void DelDestroy(Notification i_notification);

	[SerializeField]
	private Text m_txt;

	private int m_numPosY;
	private InputButton? m_promptInput;
	private Image m_promptGlyph;
	private Sprite m_promptSprite;
	private string m_promptRawText;

	public event DelDestroy OnDestroy;

	public void Initialize(string i_text, Color i_colorText, int i_numPosY, bool i_isContinues)
	{
		m_promptRawText = i_text;
		m_txt.text = i_text;
		m_txt.color = i_colorText;
		m_numPosY = i_numPosY;
		if (!i_isContinues)
		{
			StartCoroutine(CoroutineAnimateFloatUp(2f));
			StartCoroutine(CoroutineAnimateFadeOut(2f));
		}
	}

	public int GetNumPosY()
	{
		return m_numPosY;
	}

	public void SetText(string i_text)
	{
		m_promptRawText = i_text;
		if (m_promptInput.HasValue) RefreshPromptGlyph();
		else m_txt.text = i_text;
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
			m_promptGlyph = InputGlyphLibrary.GetOrCreateImage(m_txt.transform, "InputPromptGlyph");
			RectTransform iconRect = m_promptGlyph.rectTransform;
			iconRect.anchorMin = new Vector2(0f, 0.5f);
			iconRect.anchorMax = new Vector2(0f, 0.5f);
			iconRect.pivot = new Vector2(0.5f, 0.5f);
			iconRect.sizeDelta = new Vector2(30f, 30f);
		}
		string raw = string.IsNullOrEmpty(m_promptRawText) ? m_txt.text : m_promptRawText;
		if (sprite == null)
		{
			m_txt.text = raw;
		}
		else
		{
			string binding = CommonReferences.Instance.GetManagerInput().GetPromptBindingName(m_promptInput.Value);
			string token = "Press " + binding;
			m_txt.text = raw.StartsWith(token) ? "Press     " + raw.Substring(token.Length) : raw;
			PositionGlyphAfterPress();
		}
		if (sprite == m_promptSprite && m_promptGlyph.enabled == (sprite != null)) return;
		m_promptSprite = sprite;
		m_promptGlyph.sprite = sprite;
		m_promptGlyph.enabled = sprite != null;
	}

	private void PositionGlyphAfterPress()
	{
		TextGenerator generator = m_txt.cachedTextGeneratorForLayout;
		TextGenerationSettings settings = m_txt.GetGenerationSettings(m_txt.rectTransform.rect.size);
		float prefixWidth = generator.GetPreferredWidth("Press ", settings) / m_txt.pixelsPerUnit;
		float fullWidth = generator.GetPreferredWidth(m_txt.text, settings) / m_txt.pixelsPerUnit;
		float startX = 0f;
		if (m_txt.alignment == TextAnchor.UpperCenter || m_txt.alignment == TextAnchor.MiddleCenter || m_txt.alignment == TextAnchor.LowerCenter)
			startX = (m_txt.rectTransform.rect.width - fullWidth) * 0.5f;
		else if (m_txt.alignment == TextAnchor.UpperRight || m_txt.alignment == TextAnchor.MiddleRight || m_txt.alignment == TextAnchor.LowerRight)
			startX = m_txt.rectTransform.rect.width - fullWidth;
		m_promptGlyph.rectTransform.anchoredPosition = new Vector2(startX + prefixWidth + 15f, 0f);
	}

	private IEnumerator CoroutineAnimateFadeOut(float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_txt.color = new Color(m_txt.color.r, m_txt.color.g, m_txt.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyNotification(this);
	}

	private IEnumerator CoroutineAnimateFloatUp(float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		float l_amountToMoveUp = 0f;
		float l_durationToMove = 0.5f;
		float l_durationPassed = 0f;
		while (l_durationPassed < l_durationToMove)
		{
			Vector2 anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
			anchoredPosition.y += l_amountToMoveUp;
			GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			l_amountToMoveUp += 0.05f;
			yield return new WaitForFixedUpdate();
		}
	}
}
