using UnityEngine;
using UnityEngine.UI;

public class MoneyHud : MonoBehaviour
{
	[SerializeField]
	private GameObject m_parent;

	[SerializeField]
	private Text m_txtMoney;

	[SerializeField]
	private Text m_txtMoneyBg;

	[SerializeField]
	private MoneyHudGainItem m_moneyHudGainItemDefault;

	private void Awake()
	{
		ConfigureMoneyText(m_txtMoney);
		ConfigureMoneyText(m_txtMoneyBg);
		m_moneyHudGainItemDefault.gameObject.SetActive(value: false);
	}

	public static void ConfigureMoneyText(Text i_text)
	{
		if (i_text == null) return;
		// The original fixed 36 px label wraps once the balance reaches six
		// digits. Preserve the full value and let only this label scale down.
		i_text.resizeTextForBestFit = true;
		i_text.resizeTextMinSize = Mathf.Min(18, i_text.fontSize);
		i_text.resizeTextMaxSize = i_text.fontSize;
		i_text.horizontalOverflow = HorizontalWrapMode.Wrap;
		i_text.verticalOverflow = VerticalWrapMode.Truncate;
	}

	private void Update()
	{
		m_txtMoney.text = "$" + CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney();
		m_txtMoneyBg.text = m_txtMoney.text;
	}

	public void GainMoney(int i_amount)
	{
		MoneyHudGainItem moneyHudGainItem = Object.Instantiate(m_moneyHudGainItemDefault, m_moneyHudGainItemDefault.transform.parent);
		moneyHudGainItem.Initialize(i_amount, i_isGain: true);
		moneyHudGainItem.gameObject.SetActive(value: true);
	}

	public void LoseMoney(int i_amount)
	{
		MoneyHudGainItem moneyHudGainItem = Object.Instantiate(m_moneyHudGainItemDefault, m_moneyHudGainItemDefault.transform.parent);
		moneyHudGainItem.Initialize(i_amount, i_isGain: false);
		moneyHudGainItem.gameObject.SetActive(value: true);
	}

	public void Show()
	{
		m_parent.SetActive(value: true);
	}

	public void Hide()
	{
		m_parent.SetActive(value: false);
	}
}
