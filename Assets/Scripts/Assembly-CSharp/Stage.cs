using System.Collections.Generic;
using UnityEngine;


public class Stage : MonoBehaviour
{
	[SerializeField]
	private int m_id;

	[SerializeField]
	private string m_nameStage;

	[TextArea(5, 10)]
	[SerializeField]
	private string m_description;

	private NavMap m_navMap;

	[SerializeField]
	private AudioClip m_audioAmbience;

	[SerializeField]
	private AudioClip m_audioEnterStage;

	[SerializeField]
	private AudioClip m_audioStartWave;

	[SerializeField]
	private AudioClip m_audioWinWave;

	[SerializeField]
	private Waypoint m_waypointStart;

	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightGlobal;

	[SerializeField]
	private Transform m_actorsParent;

	[SerializeField]
	private Transform m_itemsParent;

	private Vector3 m_posStart;

	private bool m_isDuplicate;

	private List<Ledge> m_ledges = new List<Ledge>();

	private void Awake()
	{
		if (m_waypointStart != null)
		{
			m_posStart = m_waypointStart.GetPos();
		}
		m_navMap = GetComponentInChildren<NavMap>();
	}

	public Waypoint GetWaypointStart()
	{
		return m_waypointStart;
	}

	public virtual void OpenStage()
	{
		base.gameObject.SetActive(value: true);
		CommonReferences.Instance.GetPlayer().transform.SetParent(GetActorsParent().transform);
		if ((bool)GetComponent<ManagerWave>())
		{
			GetComponent<ManagerWave>().Initialize(this);
		}
		Platform[] componentsInChildren = GetComponentsInChildren<Platform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].CreateLedgesAndEdges();
		}
		m_ledges.Clear();
		Ledge[] componentsInChildren2 = GetComponentsInChildren<Ledge>(includeInactive: true);
		foreach (Ledge item in componentsInChildren2)
		{
			m_ledges.Add(item);
		}
	}

	public void CloseStage()
	{
		CommonReferences.Instance.GetManagerAudio().StopMusic();
		CommonReferences.Instance.GetManagerAudio().StopAudioAmbience();
		base.gameObject.SetActive(value: false);
	}

	public List<Actor> GetAllActors()
	{
		List<Actor> list = new List<Actor>();
		Actor[] componentsInChildren = GetComponentsInChildren<Actor>();
		foreach (Actor actor in componentsInChildren)
		{
			if (!(actor is Egg))
			{
				list.Add(actor);
			}
		}
		return list;
	}

	public List<NPC> GetAllNPCs()
	{
		List<NPC> list = new List<NPC>();
		NPC[] componentsInChildren = GetComponentsInChildren<NPC>(includeInactive: true);
		foreach (NPC item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<Item> GetAllItems()
	{
		List<Item> list = new List<Item>();
		Item[] componentsInChildren = GetComponentsInChildren<Item>();
		foreach (Item item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<Weapon> GetAllWeapons()
	{
		List<Weapon> list = new List<Weapon>();
		Weapon[] componentsInChildren = GetComponentsInChildren<Weapon>();
		foreach (Weapon item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<Interactable> GetAllInteractables()
	{
		List<Interactable> list = new List<Interactable>();
		Interactable[] componentsInChildren = GetComponentsInChildren<Interactable>();
		foreach (Interactable item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<Vendor> GetAllVendors()
	{
		List<Vendor> list = new List<Vendor>();
		Vendor[] componentsInChildren = GetComponentsInChildren<Vendor>();
		foreach (Vendor item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public string GetName()
	{
		return m_nameStage;
	}

	public Transform GetActorsParent()
	{
		return m_actorsParent;
	}

	public Transform GetItemsParent()
	{
		return m_itemsParent;
	}

	public ManagerWave GetManagerWave()
	{
		return GetComponent<ManagerWave>();
	}

	public void SetIsDuplicate(bool i_isDuplicate)
	{
		m_isDuplicate = i_isDuplicate;
	}

	public bool GetIsDuplicate()
	{
		return m_isDuplicate;
	}

	public AudioClip GetAudioEnterStage()
	{
		return m_audioEnterStage;
	}

	public AudioClip GetAudioStartWave()
	{
		return m_audioStartWave;
	}

	public AudioClip GetAudioWinWave()
	{
		return m_audioWinWave;
	}

	public AudioClip GetAudioAmbience()
	{
		return m_audioAmbience;
	}

	public UnityEngine.Rendering.Universal.Light2D GetLightGlobal()
	{
		return m_lightGlobal;
	}

	public int GetId()
	{
		return m_id;
	}

	public NavMap GetNavMap()
	{
		return m_navMap;
	}

	public List<Ledge> GetAllLedges()
	{
		return m_ledges;
	}

	public Ledge GetLedgeClosestToPos(Vector2 i_pos)
	{
		Ledge ledge = null;
		for (int i = 0; i < m_ledges.Count; i++)
		{
			if (ledge == null)
			{
				ledge = m_ledges[i];
			}
			else if (Vector2.Distance(i_pos, m_ledges[i].GetPos()) < Vector2.Distance(i_pos, ledge.GetPos()))
			{
				ledge = m_ledges[i];
			}
		}
		return ledge;
	}

	public int GetHighscore()
	{
		return ManagerDB.GetHighscore(this);
	}

	public string GetDescription()
	{
		return m_description;
	}

	public void ConfigureModStage(int i_runtimeId, string i_name, string i_description, Vector2 i_playerSpawn)
	{
		m_id = i_runtimeId;
		m_nameStage = i_name;
		m_description = i_description ?? string.Empty;
		if (m_waypointStart != null)
		{
			m_waypointStart.transform.localPosition = i_playerSpawn;
			m_posStart = m_waypointStart.transform.position;
		}
	}
}
