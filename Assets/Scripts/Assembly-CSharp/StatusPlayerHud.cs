using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.UI;

public class StatusPlayerHud : MonoBehaviour
{
	public delegate void DelOnAddStatus();

	[SerializeField]
	private StatusPlayerHudItem m_statusPlayerItemDefault;

	[SerializeField]
	private AudioClip m_audioAddStatus;

	[SerializeField]
	private Color m_colorRape;

	[SerializeField]
	private Color m_colorLewd;

	[SerializeField]
	private Color m_colorPregnancy;

	[SerializeField]
	private Color m_colorCombat;

	[SerializeField]
	private Color m_colorBuff;

	[SerializeField]
	private Color m_colorSpecial;

	private List<StatusPlayerHudItem> m_statusPlayerItems = new List<StatusPlayerHudItem>();

	private bool m_isShowingPregnantStatus;

	private float m_heightStatusPlayerItem = 71f;

	private float m_heightSpaceBetweenStatusPlayerItems = 4f;

	private float m_spaceBetweenStatusPlayerItems;

	private Player m_player;

	private Raper m_raperCurrent;

	private bool m_isCanPlayAudioAddState = true;

	private bool m_isHidden;
	private Dictionary<string, EnemyFinisherStatusTextDefinition> m_modFinisherStatuses;
	private string m_modFinisherEnemyName;
	private Dictionary<string, EnemyFinisherStatusTextDefinition> m_modPregnancyStatuses;
	private string m_modPregnancyEnemyName;
	private StatusPlayerHudItem m_statusPregnant;
	private StatusPlayerHudItem m_statusLabor;

	public event DelOnAddStatus OnAddStatus;

	private void Start()
	{
		m_statusPlayerItemDefault.gameObject.SetActive(value: false);
	}

	private void AddListeners()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		OnAddStatus += PlayAudioAddStatus;
		m_player.OnBeingRaped += AddStatusRape;
		m_player.OnOrgasm += AddStatusOrgasm;
		m_player.OnFetusInsert += AddStatusFetusInsert;
		m_player.OnLabor += AddStatusLabor;
		m_player.OnBirth += AddStatusBirth;
		m_player.OnDie += AddStatusDie;
	}

	private void ClearListeners()
	{
		m_player = CommonReferences.Instance.GetPlayer();
		OnAddStatus -= PlayAudioAddStatus;
		m_player.OnBeingRaped -= AddStatusRape;
		m_player.OnOrgasm -= AddStatusOrgasm;
		m_player.OnFetusInsert -= AddStatusFetusInsert;
		m_player.OnLabor -= AddStatusLabor;
		m_player.OnBirth -= AddStatusBirth;
		m_player.OnDie -= AddStatusDie;
	}

	private void PlayAudioAddStatus()
	{
		if (m_isCanPlayAudioAddState)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioAddStatus);
			StartCoroutine(CoroutineWaitBeforeCanPlayAudioAddStatusAgain());
		}
	}

	private IEnumerator CoroutineWaitBeforeCanPlayAudioAddStatusAgain()
	{
		m_isCanPlayAudioAddState = false;
		yield return new WaitForSeconds(0.2f);
		m_isCanPlayAudioAddState = true;
	}

	public StatusPlayerHudItem CreateStatus(string i_nameStatus, string i_descriptionStatus, StatusPlayerHudItemColor i_color)
	{
		StatusPlayerHudItem statusPlayerHudItem = Object.Instantiate(m_statusPlayerItemDefault, m_statusPlayerItemDefault.transform.parent);
		statusPlayerHudItem.transform.position = m_statusPlayerItemDefault.transform.position;
		statusPlayerHudItem.Initialise(i_nameStatus, i_descriptionStatus, GetColorPlayerStatusHudItem(i_color));
		statusPlayerHudItem.gameObject.SetActive(value: false);
		if (m_isHidden)
		{
			Hide();
		}
		return statusPlayerHudItem;
	}

	public void AddStatus(StatusPlayerHudItem i_status)
	{
		i_status.gameObject.SetActive(!m_isHidden);
		m_statusPlayerItems.Add(i_status);
		SortItems();
		if (this.OnAddStatus != null)
		{
			this.OnAddStatus();
		}
	}

	public StatusPlayerHudItem CreateAndAddStatus(string i_nameStatus, string i_descriptionStatus, StatusPlayerHudItemColor i_color)
	{
		StatusPlayerHudItem statusPlayerHudItem = CreateStatus(i_nameStatus, i_descriptionStatus, i_color);
		AddStatus(statusPlayerHudItem);
		return statusPlayerHudItem;
	}

	public StatusPlayerHudItem CreateAndAddStatus(string i_nameStatus, string i_descriptionStatus, StatusPlayerHudItemColor i_color, float i_duration)
	{
		StatusPlayerHudItem statusPlayerHudItem = CreateAndAddStatus(i_nameStatus, i_descriptionStatus, i_color);
		StartCoroutine(CoroutineWaitToDestroyStatusItem(statusPlayerHudItem, i_duration));
		return statusPlayerHudItem;
	}

	public void BeginModularFinisherStatusContext(Dictionary<string, EnemyFinisherStatusTextDefinition> i_statuses, string i_enemyName)
	{
		m_modFinisherStatuses = i_statuses ?? new Dictionary<string, EnemyFinisherStatusTextDefinition>();
		m_modFinisherEnemyName = i_enemyName ?? string.Empty;
	}

	public void EndModularFinisherStatusContext()
	{
		m_modFinisherStatuses = null;
		m_modFinisherEnemyName = null;
	}

	public StatusPlayerHudItem CreateModularFinisherStatus(string i_key, float i_duration = -1f,
		string i_fetusName = null, string i_childName = null)
	{
		if (!TryResolveModularFinisherStatus(i_key, i_fetusName, i_childName,
			out string title, out string description, out StatusPlayerHudItemColor color)) return null;
		return i_duration > 0f ? CreateAndAddStatus(title, description, color, i_duration)
			: CreateAndAddStatus(title, description, color);
	}

	private bool TryResolveModularFinisherStatus(string i_key, string i_fetusName, string i_childName,
		out string o_title, out string o_description, out StatusPlayerHudItemColor o_color)
	{
		Dictionary<string, EnemyFinisherStatusTextDefinition> statuses = m_modFinisherStatuses;
		string enemyName = m_modFinisherEnemyName;
		bool pregnancyStatus = i_key == "fertilized" || i_key == "pregnant" || i_key == "implanting" || i_key == "labor" || i_key == "birth";
		if (pregnancyStatus && statuses == null)
		{
			statuses = m_modPregnancyStatuses;
			enemyName = m_modPregnancyEnemyName;
		}
		o_title = null;
		o_description = null;
		o_color = StatusPlayerHudItemColor.Rape;
		if (statuses == null) return false;
		switch (i_key)
		{
		case "active": o_title = "Rape"; o_description = "{enemy} is raping you!"; break;
		case "mating": o_title = "Mating"; o_description = "{enemy} is breeding with you..."; o_color = StatusPlayerHudItemColor.Lewd; break;
		case "infusion": o_title = "Infusion"; o_description = "{enemy} is cumming inside you!"; break;
		case "succumbed": o_title = "Succumbed"; o_description = "{enemy} has raped you into submission..."; break;
		case "orgasm": o_title = "Orgasm"; o_description = "{enemy} has made you cum!"; o_color = StatusPlayerHudItemColor.Lewd; break;
		case "fertilized": o_title = "Fertilized"; o_description = "{enemy} has impregnated you with a {fetus}!"; o_color = StatusPlayerHudItemColor.Pregnancy; break;
		case "pregnant": o_title = "Pregnant"; o_description = "You are pregnant..."; o_color = StatusPlayerHudItemColor.Pregnancy; break;
		case "implanting": o_title = "Implanting {fetus}"; o_description = null; o_color = StatusPlayerHudItemColor.Pregnancy; break;
		case "labor": o_title = "Labor"; o_description = "Giving birth..."; o_color = StatusPlayerHudItemColor.Pregnancy; break;
		case "birth": o_title = "Birth"; o_description = "You just gave birth to {child}!"; o_color = StatusPlayerHudItemColor.Pregnancy; break;
		case "mindBroken": o_title = "Mind Broken"; o_description = "{enemy} has broken your mind. It's over."; o_color = StatusPlayerHudItemColor.Lewd; break;
		default: return false;
		}
		if (statuses.TryGetValue(i_key, out EnemyFinisherStatusTextDefinition custom) && custom != null)
		{
			if (custom.Title != null) o_title = custom.Title;
			if (custom.Description != null) o_description = custom.Description;
		}
		o_title = FormatModularStatusText(o_title, enemyName, i_fetusName, i_childName);
		o_description = FormatModularStatusText(o_description, enemyName, i_fetusName, i_childName);
		return true;
	}

	private static string FormatModularStatusText(string i_text, string i_enemyName, string i_fetusName, string i_childName)
	{
		if (i_text == null) return null;
		return i_text.Replace("{enemy}", i_enemyName ?? string.Empty)
			.Replace("{fetus}", i_fetusName ?? string.Empty).Replace("{child}", i_childName ?? string.Empty);
	}

	private void SortItems()
	{
		if (m_statusPlayerItems.Count <= 1)
		{
			return;
		}
		foreach (StatusPlayerHudItem item in GetStatusPlayerItemsSorted())
		{
			StartCoroutine(CoroutineAnimateStatusItemToDestination(item));
		}
	}

	private List<StatusPlayerHudItem> GetStatusPlayerItemsSorted()
	{
		List<StatusPlayerHudItem> list = new List<StatusPlayerHudItem>();
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			list.Add(statusPlayerItem);
		}
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (i > j && list.IndexOf(list[i]) < list.IndexOf(list[j]))
					{
						StatusPlayerHudItem value = list[j];
						list[j] = list[i];
						list[i] = value;
						flag = false;
					}
				}
			}
		}
		return list;
	}

	private IEnumerator CoroutineAnimateStatusItemIn(StatusPlayerHudItem i_statusPlayerItem)
	{
		float l_sizeFromX = 0f;
		float l_sizeFromY = 0f;
		float l_sizeToX = m_statusPlayerItemDefault.GetComponent<RectTransform>().sizeDelta.x;
		float l_sizeToY = m_statusPlayerItemDefault.GetComponent<RectTransform>().sizeDelta.y;
		float l_timeToMove = 0.25f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float x = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_sizeFromX, l_sizeToX, i_time);
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_sizeFromY, l_sizeToY, i_time);
			if (i_statusPlayerItem != null)
			{
				i_statusPlayerItem.GetComponent<RectTransform>().sizeDelta = new Vector2(x, y);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineAnimateStatusItemOut(StatusPlayerHudItem i_statusPlayerItem)
	{
		float l_transparencyFrom = i_statusPlayerItem.GetColor().a;
		float l_transparencyTo = 0f;
		float l_timeToMove = 0.25f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_transparencyFrom, l_transparencyTo, i_time);
			if (i_statusPlayerItem != null)
			{
				Color color = i_statusPlayerItem.GetColor();
				color.a = a;
				i_statusPlayerItem.SetColor(color);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineAnimateStatusItemToDestination(StatusPlayerHudItem i_statusPlayerItem)
	{
		Vector2 anchoredPosition = m_statusPlayerItemDefault.GetComponent<RectTransform>().anchoredPosition;
		anchoredPosition.y += (float)m_statusPlayerItems.IndexOf(i_statusPlayerItem) * (m_heightStatusPlayerItem + m_heightSpaceBetweenStatusPlayerItems);
		float l_positionFromY = i_statusPlayerItem.GetComponent<RectTransform>().anchoredPosition.y;
		float l_positionToY = anchoredPosition.y;
		float l_timeToMove = 0.25f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_positionFromY, l_positionToY, i_time);
			if (i_statusPlayerItem != null)
			{
				i_statusPlayerItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(i_statusPlayerItem.GetComponent<RectTransform>().anchoredPosition.x, y);
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineWaitToDestroyStatusItem(StatusPlayerHudItem i_statusItemToDestroy, float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		DestroyStatusItem(i_statusItemToDestroy);
	}

	public void DestroyStatusItem(StatusPlayerHudItem i_statusPlayerItem)
	{
		StartCoroutine(CoroutineDestroyStatusItem(i_statusPlayerItem));
	}

	private IEnumerator CoroutineDestroyStatusItem(StatusPlayerHudItem i_statusPlayerItem)
	{
		yield return CoroutineAnimateStatusItemOut(i_statusPlayerItem);
		m_statusPlayerItems.Remove(i_statusPlayerItem);
		if (i_statusPlayerItem != null)
		{
			Object.Destroy(i_statusPlayerItem.gameObject);
		}
		SortItems();
	}

	public void AddStatusKO(NPC i_npc)
	{
		CreateAndAddStatus("K.O.", "You got knocked out by " + i_npc.GetName() + "!", StatusPlayerHudItemColor.Combat);
		m_player.OnEndExhaustion += DestroyStatusTired;
		m_player.OnRagdollEnd += DestroyStatusTired;
	}

	private void DestroyStatusTired()
	{
		m_player.OnEndExhaustion -= DestroyStatusTired;
		m_player.OnRagdollEnd -= DestroyStatusTired;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "K.O.")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void AddStatusRape()
	{
		m_raperCurrent = m_player.GetRaperCurrent();
		if (m_player.IsDead())
		{
			CreateAndAddStatus("Mating", m_player.GetRaperCurrent().GetComponentInParent<Actor>().GetName() + " is breeding with you...", StatusPlayerHudItemColor.Lewd);
			m_player.OnRapeEnd += DestroyStatusSex;
		}
		else
		{
			CreateAndAddStatus("Rape", m_player.GetRaperCurrent().GetComponentInParent<Actor>().GetName() + " is raping you!", StatusPlayerHudItemColor.Rape);
			m_player.OnRapeEnd += DestroyStatusRape;
		}
		m_player.GetRaperCurrent().OnCumThrust += AddStatusCum;
		if (m_raperCurrent is RaperGame)
		{
			((RaperGame)m_player.GetRaperCurrent()).OnWin += AddStatusDefeat;
		}
		m_raperCurrent.OnEndRape += DestroyRapeEventLinks;
	}

	private void DestroyRapeEventLinks()
	{
		m_raperCurrent.OnEndRape -= DestroyRapeEventLinks;
		m_player.GetRaperCurrent().OnCumThrust -= AddStatusCum;
		if (m_raperCurrent is RaperGame)
		{
			((RaperGame)m_player.GetRaperCurrent()).OnWin -= AddStatusDefeat;
		}
	}

	private void DestroyStatusRape()
	{
		m_player.OnRapeEnd -= DestroyStatusRape;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Rape")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void DestroyStatusSex()
	{
		m_player.OnRapeEnd -= DestroyStatusSex;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Mating")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void AddStatusCum()
	{
		m_player.GetRaperCurrent().OnCumThrust -= AddStatusCum;
		CreateAndAddStatus("Infusion", m_player.GetRaperCurrent().GetComponentInParent<Actor>().GetName() + " is cumming inside you!", StatusPlayerHudItemColor.Rape);
		ExternalRuleProfileFactory.NotifyEventReward("playerInfusion");
		m_player.OnRapeEnd += DestroyStatusCum;
	}

	private void DestroyStatusCum()
	{
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Infusion")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void AddStatusDefeat()
	{
		((RaperGame)m_player.GetRaperCurrent()).OnWin -= AddStatusDefeat;
		CreateAndAddStatus("Succumbed", m_player.GetRaperCurrent().GetComponentInParent<Actor>().GetName() + " has raped you into submission...", StatusPlayerHudItemColor.Rape);
		m_player.OnRapeEnd += DestroyStatusDefeat;
	}

	private void DestroyStatusDefeat()
	{
		m_player.OnRapeEnd -= DestroyStatusDefeat;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Succumbed")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void AddStatusFetusInsert(Fetus i_fetus)
	{
		if (m_modFinisherStatuses != null)
		{
			m_modPregnancyStatuses = m_modFinisherStatuses;
			m_modPregnancyEnemyName = m_modFinisherEnemyName;
		}
		if (!m_isShowingPregnantStatus)
		{
			m_isShowingPregnantStatus = true;
			m_statusPregnant = CreateModularFinisherStatus("pregnant")
				?? CreateAndAddStatus("Pregnant", "You are pregnant...", StatusPlayerHudItemColor.Pregnancy);
			m_player.OnBirthEnd += DestroyStatusPregnant;
		}
		if ((bool)i_fetus.GetNpcParent())
		{
			if (CreateModularFinisherStatus("fertilized", 5f, i_fetus.GetNameFetus()) == null)
				CreateAndAddStatus("Fertilized", i_fetus.GetNpcParent().GetName() + " has impregnated you with a " + i_fetus.GetNameFetus() + "!", StatusPlayerHudItemColor.Pregnancy, 5f);
		}
		else
		{
			ExternalRuleProfileFactory.NotifyEventReward("playerImplantation");
			if (CreateModularFinisherStatus("implanting", 3f, i_fetus.GetNameFetus()) == null)
				CreateAndAddStatus("Implanting " + i_fetus.GetNameFetus(), null, StatusPlayerHudItemColor.Pregnancy, 3f);
		}
	}

	private void DestroyStatusFetusInsert()
	{
		m_player.OnRapeEnd -= DestroyStatusFetusInsert;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Fertilized")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void DestroyStatusPregnant()
	{
		if (m_player.GetNumOfFetuses() >= 1)
		{
			return;
		}
		m_player.OnBirthEnd -= DestroyStatusPregnant;
		if (m_statusPregnant != null)
		{
			DestroyStatusItem(m_statusPregnant);
			m_statusPregnant = null;
		}
		m_isShowingPregnantStatus = false;
		m_modPregnancyStatuses = null;
		m_modPregnancyEnemyName = null;
	}

	private void AddStatusOrgasm()
	{
		if (CreateModularFinisherStatus("orgasm", 5f) != null) return;
		if ((bool)m_player.GetRaperCurrent())
		{
			CreateAndAddStatus("Orgasm", m_player.GetRaperCurrent().GetComponentInParent<Actor>().GetName() + " has made you cum!", StatusPlayerHudItemColor.Lewd, 5f);
		}
		else
		{
			CreateAndAddStatus("Orgasm", "You just came!", StatusPlayerHudItemColor.Lewd, 5f);
		}
	}

	private void DestroyStatusOrgasm()
	{
		m_player.OnRapeEnd -= DestroyStatusOrgasm;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Orgasm")
			{
				DestroyStatusItem(statusPlayerItem);
			}
		}
	}

	private void AddStatusLabor()
	{
		m_statusLabor = CreateModularFinisherStatus("labor")
			?? CreateAndAddStatus("Labor", "Giving birth...", StatusPlayerHudItemColor.Pregnancy);
		m_player.OnLaborEnd += DestroyStatusLabor;
	}

	private void DestroyStatusLabor()
	{
		m_player.OnLaborEnd -= DestroyStatusLabor;
		if (m_statusLabor != null)
		{
			DestroyStatusItem(m_statusLabor);
			m_statusLabor = null;
		}
	}

	private void AddStatusBirth(Actor i_actorChild)
	{
		if (CreateModularFinisherStatus("birth", 5f, null, i_actorChild.GetName()) == null)
			CreateAndAddStatus("Birth", "You just gave birth to " + i_actorChild.GetName() + "!", StatusPlayerHudItemColor.Pregnancy, 5f);
	}

	private void DestroyStatusBirth()
	{
		m_player.OnBirthEnd -= DestroyStatusBirth;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == "Birth")
			{
				DestroyStatusItem(statusPlayerItem);
				break;
			}
		}
	}

	private void AddStatusDie()
	{
		if (m_player.GetHealthCurrent() <= 0f)
		{
			m_player.OnRestoreHealth -= DestroyStatusTired;
			foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
			{
				if (statusPlayerItem.GetName() == "K.O.")
				{
					DestroyStatusItem(statusPlayerItem);
					break;
				}
			}
		}
		if (CreateModularFinisherStatus("mindBroken") == null)
			CreateAndAddStatus("Mind Broken", "All the raping has broken your mind. It's over.", StatusPlayerHudItemColor.Lewd);
	}

	public void Show()
	{
		m_isHidden = false;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			Image[] componentsInChildren = statusPlayerItem.GetComponentsInChildren<Image>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
			Text[] componentsInChildren2 = statusPlayerItem.GetComponentsInChildren<Text>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = true;
			}
		}
	}

	public void Hide()
	{
		m_isHidden = true;
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			Image[] componentsInChildren = statusPlayerItem.GetComponentsInChildren<Image>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
			Text[] componentsInChildren2 = statusPlayerItem.GetComponentsInChildren<Text>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].enabled = false;
			}
		}
	}

	public void RestartGameAfterGameOver()
	{
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			Object.Destroy(statusPlayerItem.gameObject);
		}
		m_statusPlayerItems.Clear();
		ClearListeners();
		AddListeners();
	}

	private Color GetColorPlayerStatusHudItem(StatusPlayerHudItemColor i_color)
	{
		switch (i_color)
		{
		case StatusPlayerHudItemColor.Buff:
			return m_colorBuff;
		case StatusPlayerHudItemColor.Combat:
			return m_colorCombat;
		case StatusPlayerHudItemColor.Lewd:
			return m_colorLewd;
		case StatusPlayerHudItemColor.Pregnancy:
			return m_colorPregnancy;
		case StatusPlayerHudItemColor.Rape:
			return m_colorRape;
		case StatusPlayerHudItemColor.Special:
			return m_colorSpecial;
		default:
			return Color.white;
		}
	}

	public bool IsStatusPlayerHudItemExists(string i_nameStatus)
	{
		foreach (StatusPlayerHudItem statusPlayerItem in m_statusPlayerItems)
		{
			if (statusPlayerItem.GetName() == i_nameStatus)
			{
				return true;
			}
		}
		return false;
	}
}
