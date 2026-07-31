using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransmogrifierHud : MonoBehaviour
{
	[SerializeField]
	private GameObject m_playerShowcase;

	[SerializeField]
	private TransmogrifierHudItem m_transmogrifierHudItemDefault;

	[SerializeField]
	private Text m_txtId;

	[SerializeField]
	private Text m_txtName;

	[SerializeField]
	private Text m_txtSex;

	[SerializeField]
	private Text m_txtAge;

	[SerializeField]
	private Text m_txtDescription;

	private List<TransmogrifierHudItem> m_transmogrifierHudItems = new List<TransmogrifierHudItem>();

	private GameObject m_playerCurrent;

	public void ShowcasePlayer(TransmogrifierHudItem i_transmogrifierHudItem)
	{
		if (m_playerCurrent != null)
		{
			Object.Destroy(m_playerCurrent);
		}
		m_playerCurrent = Object.Instantiate(i_transmogrifierHudItem.GetPlayer().gameObject, m_playerShowcase.transform);
		m_playerCurrent.GetComponent<Player>().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_playerCurrent.SetActive(value: true);
		m_playerCurrent.GetComponent<Player>().GetAnimator().Play("Idle");
		SetText();
	}

	private void SetText()
	{
		Player component = m_playerCurrent.GetComponent<Player>();
		int num = 0;
		foreach (Player allPlayer in Library.Instance.Actors.GetAllPlayers())
		{
			if (allPlayer.GetName() == component.GetName())
			{
				num = Library.Instance.Actors.GetAllPlayers().IndexOf(allPlayer);
			}
		}
		m_txtId.text = "_id: " + num;
		m_txtName.text = "_name: \"" + component.GetName() + "\"";
		m_txtSex.text = "_sex: " + component.GetSex();
		m_txtAge.text = "_age: " + component.GetAge();
		m_txtDescription.text = component.GetDescriptionPlayer();
	}

	private void DefaultText()
	{
		m_txtId.text = "_id: null";
		m_txtName.text = "_name: null";
		m_txtSex.text = "_sex: null";
		m_txtAge.text = "_age: null";
		m_txtDescription.text = "null";
	}

	public void Transmogrify()
	{
		CommonReferences.Instance.GetManagerActor().SelectPlayerTransmogrifier(m_playerCurrent.GetComponent<Player>());
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		Hide();
	}

	private void BuildItems()
	{
		float x = m_transmogrifierHudItemDefault.GetComponent<RectTransform>().sizeDelta.x;
		float y = m_transmogrifierHudItemDefault.GetComponent<RectTransform>().sizeDelta.y;
		float num = m_transmogrifierHudItemDefault.GetComponent<RectTransform>().anchoredPosition.x - x / 2f;
		_ = m_transmogrifierHudItemDefault.GetComponent<RectTransform>().anchoredPosition;
		_ = y / 2f;
		foreach (Player allPlayer in Library.Instance.Actors.GetAllPlayers())
		{
			TransmogrifierHudItem transmogrifierHudItem = Object.Instantiate(m_transmogrifierHudItemDefault, m_transmogrifierHudItemDefault.transform.parent);
			transmogrifierHudItem.SetPlayer(allPlayer);
			Vector3 vector = transmogrifierHudItem.GetComponent<RectTransform>().anchoredPosition;
			int num2 = m_transmogrifierHudItems.Count % 3;
			vector.x += x * (float)num2;
			vector.x += num * (float)num2;
			transmogrifierHudItem.GetComponent<RectTransform>().anchoredPosition = vector;
			transmogrifierHudItem.gameObject.SetActive(value: true);
			m_transmogrifierHudItems.Add(transmogrifierHudItem);
		}
	}

	private void Clear()
	{
		foreach (TransmogrifierHudItem transmogrifierHudItem in m_transmogrifierHudItems)
		{
			Object.Destroy(transmogrifierHudItem.gameObject);
		}
		m_transmogrifierHudItems.Clear();
		if (m_playerCurrent != null)
		{
			Object.Destroy(m_playerCurrent);
			m_playerCurrent = null;
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
		Clear();
		BuildItems();
		DefaultText();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
