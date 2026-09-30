using System.Collections.Generic;
using UnityEngine;


public class Stage : MonoBehaviour
{
	[SerializeField]
	private int m_id;

	[SerializeField]
	private bool m_isRuntimeTemplate;

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

	private float m_fallRecoveryY = float.NaN;
	private bool m_hasModCameraBounds;
	private Rect m_modCameraBounds;

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
		CameraXGame camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent();
		if (m_hasModCameraBounds)
		{
			Vector2 origin = transform.position;
			camera.SetWorldBounds(new Rect(origin.x + m_modCameraBounds.x, origin.y + m_modCameraBounds.y,
				m_modCameraBounds.width, m_modCameraBounds.height));
		}
		else camera.ClearWorldBounds();
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
		// Disabled inherited platforms in a Tiled stage still retain their old
		// ledges. Excluding inactive objects prevents AI from climbing toward
		// invisible geometry instead of the active mod platforms.
		Ledge[] componentsInChildren2 = GetComponentsInChildren<Ledge>();
		foreach (Ledge item in componentsInChildren2)
		{
			m_ledges.Add(item);
		}
		if (m_navMap != null) m_navMap.FinalizeModPlatformConnections();
		m_fallRecoveryY = CalculateFallRecoveryY();
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

	public void ConfigureModGlobalLight(UnityEngine.Rendering.Universal.Light2D i_light)
	{
		if (i_light != null) m_lightGlobal = i_light;
	}

	public int GetId()
	{
		return m_id;
	}

	public bool GetIsRuntimeTemplate()
	{
		return m_isRuntimeTemplate;
	}

	public void ConfigureRuntimeTemplate(int i_templateId)
	{
		m_id = i_templateId;
		m_isRuntimeTemplate = true;
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

	public float GetFallRecoveryY()
	{
		if (float.IsNaN(m_fallRecoveryY)) m_fallRecoveryY = CalculateFallRecoveryY();
		return m_fallRecoveryY;
	}

	private float CalculateFallRecoveryY()
	{
		float lowestGeometryY = float.PositiveInfinity;
		int platformLayer = LayerMask.NameToLayer("Platform");
		int invisibleWallLayer = LayerMask.NameToLayer("InvisibleWall");
		foreach (Collider2D collider in GetComponentsInChildren<Collider2D>(includeInactive: false))
		{
			if (collider == null || !collider.enabled || collider.isTrigger) continue;
			int layer = collider.gameObject.layer;
			if (layer != platformLayer && layer != invisibleWallLayer) continue;
			lowestGeometryY = Mathf.Min(lowestGeometryY, collider.bounds.min.y);
		}
		float spawnY = m_waypointStart != null ? m_waypointStart.GetPos().y : m_posStart.y;
		return float.IsPositiveInfinity(lowestGeometryY)
			? spawnY - 25f
			: Mathf.Min(spawnY - 10f, lowestGeometryY - 10f);
	}

	public Vector2 GetPlayerSpawnPosition()
	{
		return m_waypointStart != null ? m_waypointStart.GetPos() : (Vector2)m_posStart;
	}

	public void ConfigureModStage(int i_runtimeId, string i_name, string i_description, Vector2 i_playerSpawn)
	{
		m_id = i_runtimeId;
		m_isRuntimeTemplate = false;
		m_nameStage = i_name;
		m_description = i_description ?? string.Empty;
		if (m_waypointStart != null)
		{
			m_waypointStart.transform.position = transform.TransformPoint(i_playerSpawn);
			m_posStart = m_waypointStart.transform.position;
		}
		m_fallRecoveryY = float.NaN;
	}

	public void ConfigureModCameraBounds(Rect i_bounds)
	{
		m_modCameraBounds = i_bounds;
		m_hasModCameraBounds = i_bounds.width > 0f && i_bounds.height > 0f;
	}

	public void CopyModRuntimeSettingsFrom(Stage i_source)
	{
		if (i_source == null) return;
		m_modCameraBounds = i_source.m_modCameraBounds;
		m_hasModCameraBounds = i_source.m_hasModCameraBounds;
	}

	public void ConfigureModAudioSlot(string i_slot, AudioClip i_clip)
	{
		if (i_clip == null) return;
		if (i_slot == "ambience") m_audioAmbience = i_clip;
		else if (i_slot == "entryMusic") m_audioEnterStage = i_clip;
		else if (i_slot == "waveMusic") m_audioStartWave = i_clip;
		else if (i_slot == "waveComplete") m_audioWinWave = i_clip;
	}

	public void ConfigureCoreStage(string i_name, string i_description)
	{
		m_nameStage = i_name;
		m_description = i_description ?? string.Empty;
	}
}
