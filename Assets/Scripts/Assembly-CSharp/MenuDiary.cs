using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuDiary : Menu
{
	[Header("Player Data:")]
	[SerializeField]
	private Text m_txtChallengesCompleted;

	[SerializeField]
	private Text m_txtEnemiesKilledTotal;

	[SerializeField]
	private Text m_txtDamageTakenTotal;

	[SerializeField]
	private Text m_txtTimesKnockedOutTotal;

	[SerializeField]
	private Text m_txtTimesRapedTotal;

	[SerializeField]
	private Text m_txtTimesOrgasmedTotal;

	[SerializeField]
	private Text m_txtLitreCumTakenTotal;

	[SerializeField]
	private Text m_txtLostVirginityTo;

	[SerializeField]
	private Text m_txtRapedTheMostBy;

	[SerializeField]
	private Text m_txtTimesMindBrokenTotal;

	[SerializeField]
	private Text m_txtTimesImpregnatedTotal;

	[SerializeField]
	private Text m_txtNumOfOffspringTotal;

	[SerializeField]
	private Text m_txtNumOfLayedEggsTotal;

	[SerializeField]
	private Text m_txtImpregnatedTheMostBy;

	[SerializeField]
	private Text m_txtGivenBirthTheMostTo;

	[Header("Npc Data:")]
	[SerializeField]
	private Dropdown m_dropDownNpcs;

	[SerializeField]
	private Image m_imgIconNpc;

	[SerializeField]
	private Text m_txtNameNpc;

	[SerializeField]
	private Text m_txtSexNpc;

	[SerializeField]
	private Text m_txtAppearanceNpc;

	[SerializeField]
	private Text m_txtDescriptionNpc;

	[SerializeField]
	private Text m_txtTimesKilledNpc;

	[SerializeField]
	private Text m_txtDamageTakenNpc;

	[SerializeField]
	private Text m_txtTimesKONpc;

	[SerializeField]
	private Text m_txtTimesRapedNpc;

	[SerializeField]
	private Text m_txtTimesOrgasmedNpc;

	[SerializeField]
	private Text m_txtLitreCumTakenNpc;

	[SerializeField]
	private Text m_txtTimesMindBrokenNpc;

	[SerializeField]
	private Text m_txtTimesImpregnatedNpc;

	[SerializeField]
	private GameObject m_parentInteraction;

	[SerializeField]
	private InteractionMenuItem m_interactionItemDefault;

	private List<InteractionMenuItem> m_interactionItems = new List<InteractionMenuItem>();

	public override void Open()
	{
		base.Open();
		LoadPlayerData();
		BuildDropDownMenu();
		m_interactionItemDefault.gameObject.SetActive(value: false);
		ClearNpcData();
		m_dropDownNpcs.value = 0;
		m_imgIconNpc.enabled = false;
	}

	private void BuildDropDownMenu()
	{
		m_dropDownNpcs.options.Clear();
		List<string> list = new List<string>();
		list.Add("Select an enemy to view...");
		m_dropDownNpcs.AddOptions(list);
		List<string> list2 = new List<string>();
		foreach (NPC allNpc in Library.Instance.Actors.GetAllNpcs())
		{
			if (!(allNpc is Egg) && allNpc.GetId() != -1)
			{
				list2.Add(allNpc.GetName());
			}
		}
		m_dropDownNpcs.AddOptions(list2);
		m_dropDownNpcs.SetValueWithoutNotify(0);
		m_dropDownNpcs.RefreshShownValue();
	}

	public void OnDropdownValueChanged()
	{
		ClearNpcData();
		if (m_dropDownNpcs.value == 0)
		{
			m_imgIconNpc.enabled = false;
			return;
		}
		m_imgIconNpc.enabled = true;
		NPC i_npc = null;
		foreach (NPC allNpc in Library.Instance.Actors.GetAllNpcs())
		{
			if (!(allNpc is Egg) && allNpc.GetId() != -1 && allNpc.GetName() == m_dropDownNpcs.options[m_dropDownNpcs.value].text)
			{
				i_npc = allNpc;
				break;
			}
		}
		LoadNpcData(i_npc);
	}

	private void LoadNpcData(NPC i_npc)
	{
		m_imgIconNpc.sprite = i_npc.GetSprIcon();
		m_txtNameNpc.text = i_npc.GetName();
		m_txtSexNpc.text = i_npc.GetSex();
		m_txtAppearanceNpc.text = "Appears in " + i_npc.GetStageAppearance().GetName();
		m_txtDescriptionNpc.text = i_npc.GetDescription();
		m_txtTimesKilledNpc.text = GetTimesKilledNpc(i_npc.GetId()).ToString();
		m_txtDamageTakenNpc.text = GetDamageTakenNpc(i_npc.GetId()).ToString();
		m_txtTimesKONpc.text = GetTimesKONpc(i_npc.GetId()).ToString();
		m_txtTimesRapedNpc.text = GetTimesRapedNpc(i_npc.GetId()).ToString();
		m_txtTimesOrgasmedNpc.text = GetTimesOrgasmedNpc(i_npc.GetId()).ToString();
		m_txtLitreCumTakenNpc.text = GetLitreCumTakenNpc(i_npc.GetId()) + "L";
		m_txtTimesMindBrokenNpc.text = GetTimesMindBrokenNpc(i_npc.GetId()).ToString();
		m_txtTimesImpregnatedNpc.text = GetTimesImpregnatedNpc(i_npc.GetId()).ToString();
		LoadInteractions(i_npc);
	}

	private void LoadInteractions(NPC i_npc)
	{
		if (i_npc.GetAllInteractions().Count <= 0)
		{
			return;
		}
		m_parentInteraction.SetActive(value: true);
		foreach (Interaction allInteraction in i_npc.GetAllInteractions())
		{
			InteractionMenuItem interactionMenuItem = UnityEngine.Object.Instantiate(m_interactionItemDefault, m_interactionItemDefault.transform.parent);
			interactionMenuItem.Initialize(allInteraction, i_npc);
			interactionMenuItem.gameObject.SetActive(value: true);
			m_interactionItems.Add(interactionMenuItem);
		}
	}

	private int GetTimesKilledNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT timesKilled FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetDamageTakenNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT damageTaken FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetTimesKONpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT timesKO FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetTimesRapedNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT timesRaped FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetTimesOrgasmedNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT timesOrgasmed FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private float GetLitreCumTakenNpc(int i_idNpc)
	{
		return Convert.ToSingle(ManagerDB.GetExecuteReader("SELECT litreCumTaken FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetTimesMindBrokenNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT timesMindBroken FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private int GetTimesImpregnatedNpc(int i_idNpc)
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT numOfFetusInserted FROM tbl_relationship WHERE idNpc = " + i_idNpc, 0));
	}

	private void ClearNpcData()
	{
		foreach (InteractionMenuItem interactionItem in m_interactionItems)
		{
			UnityEngine.Object.Destroy(interactionItem.gameObject);
		}
		m_interactionItems.Clear();
		m_parentInteraction.SetActive(value: false);
		m_imgIconNpc.sprite = null;
		m_txtNameNpc.text = "";
		m_txtSexNpc.text = "";
		m_txtAppearanceNpc.text = "";
		m_txtDescriptionNpc.text = "";
		m_txtTimesKilledNpc.text = "-";
		m_txtDamageTakenNpc.text = "-";
		m_txtTimesKONpc.text = "-";
		m_txtTimesRapedNpc.text = "-";
		m_txtTimesOrgasmedNpc.text = "-";
		m_txtLitreCumTakenNpc.text = "-";
		m_txtTimesMindBrokenNpc.text = "-";
		m_txtTimesImpregnatedNpc.text = "-";
	}

	private void LoadPlayerData()
	{
		m_txtChallengesCompleted.text = GetCountCompletedChallenges() + "/" + CommonReferences.Instance.GetManagerChallenge().GetAllChallenges().Count;
		int enemiesKilledTotal = GetEnemiesKilledTotal();
		m_txtEnemiesKilledTotal.text = enemiesKilledTotal.ToString();
		m_txtDamageTakenTotal.text = GetDamageTakenTotal().ToString();
		m_txtTimesKnockedOutTotal.text = GetTimesKnockedOutTotal().ToString();
		m_txtTimesRapedTotal.text = GetTimesRapedTotal().ToString();
		m_txtTimesOrgasmedTotal.text = GetTimesOrgasmedTotal().ToString();
		m_txtLitreCumTakenTotal.text = GetLitreCumTakenTotal() + "L";
		SetNpcName(m_txtLostVirginityTo, GetIdNpcDeflower());
		int idNpcTimesRapedTheMost = GetIdNpcTimesRapedTheMost();
		SetNpcName(m_txtRapedTheMostBy, idNpcTimesRapedTheMost);
		m_txtTimesMindBrokenTotal.text = GetTimesMindBrokenTotal().ToString();
		m_txtTimesImpregnatedTotal.text = GetTimesImpregnatedTotal().ToString();
		m_txtNumOfOffspringTotal.text = GetNumOfOffspringTotal().ToString();
		m_txtNumOfLayedEggsTotal.text = GetNumOfEggsLayedTotal().ToString();
		SetNpcName(m_txtImpregnatedTheMostBy, GetIdNpcImpregnatedMostBy());
		int idNpcGivenBirthMostTo = GetIdNpcGivenBirthMostTo();
		SetNpcName(m_txtGivenBirthTheMostTo, idNpcGivenBirthMostTo);
	}

	private static void SetNpcName(Text i_text, int i_npcId)
	{
		if (i_text == null)
		{
			return;
		}
		NPC npc = i_npcId < 0 || Library.Instance == null || Library.Instance.Actors == null
			? null
			: Library.Instance.Actors.GetNpc(i_npcId);
		i_text.text = npc == null ? "-" : npc.GetName();
	}

	private int GetCountCompletedChallenges()
	{
		int num = 0;
		foreach (Challenge allChallenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
		{
			if (allChallenge.GetState() == 1 || allChallenge.GetState() == 2)
			{
				num++;
			}
		}
		return num;
	}

	private int GetEnemiesKilledTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(timesKilled) FROM tbl_relationship", 0));
	}

	private int GetDamageTakenTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(damageTaken) FROM tbl_relationship", 0));
	}

	private int GetTimesKnockedOutTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(timesKO) FROM tbl_relationship", 0));
	}

	private int GetTimesRapedTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(timesRaped) FROM tbl_relationship", 0));
	}

	private int GetTimesOrgasmedTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(timesOrgasmed) FROM tbl_relationship", 0));
	}

	private float GetLitreCumTakenTotal()
	{
		return Convert.ToSingle(ManagerDB.GetExecuteReader("SELECT SUM(litreCumTaken) FROM tbl_relationship", 0));
	}

	private int GetIdNpcDeflower()
	{
		object executeReader = ManagerDB.GetExecuteReader("SELECT idNpcDeflower FROM tbl_player WHERE idPlayer = 1", 0);
		if (executeReader == DBNull.Value)
		{
			return -1;
		}
		return Convert.ToInt32(executeReader);
	}

	private int GetIdNpcTimesRapedTheMost()
	{
		if (Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT idNpc, MAX(timesRaped) FROM tbl_relationship", 1)) == 0)
		{
			return -1;
		}
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT idNpc FROM tbl_relationship ORDER BY timesRaped DESC, idNpc ASC LIMIT 1", 0));
	}

	private int GetTimesMindBrokenTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(timesMindBroken) FROM tbl_relationship", 0));
	}

	private int GetTimesImpregnatedTotal()
	{
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT SUM(numOfFetusInserted) FROM tbl_relationship", 0));
	}

	private int GetNumOfOffspringTotal()
	{
		object executeReader = ManagerDB.GetExecuteReader("SELECT SUM(timesBirth) FROM tbl_birth WHERE isEgg = 0", 0);
		if (executeReader == DBNull.Value)
		{
			return 0;
		}
		return Convert.ToInt32(executeReader);
	}

	private int GetNumOfEggsLayedTotal()
	{
		object executeReader = ManagerDB.GetExecuteReader("SELECT SUM(timesBirth) FROM tbl_birth WHERE isEgg = 1", 0);
		if (executeReader == DBNull.Value)
		{
			return 0;
		}
		return Convert.ToInt32(executeReader);
	}

	private int GetIdNpcImpregnatedMostBy()
	{
		if (Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT idNpc, MAX(numOfFetusInserted) FROM tbl_relationship", 1)) == 0)
		{
			return -1;
		}
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT idNpc FROM tbl_relationship ORDER BY numOfFetusInserted DESC, idNpc ASC LIMIT 1", 0));
	}

	private int GetIdNpcGivenBirthMostTo()
	{
		object executeReader = ManagerDB.GetExecuteReader("SELECT MAX(timesBirth) FROM tbl_birth", 0);
		if (executeReader == DBNull.Value || Convert.ToInt32(executeReader) == 0)
		{
			return -1;
		}
		return Convert.ToInt32(ManagerDB.GetExecuteReader("SELECT idNpcOffspring FROM tbl_birth ORDER BY timesBirth DESC, idNpcOffspring ASC LIMIT 1", 0));
	}
}
