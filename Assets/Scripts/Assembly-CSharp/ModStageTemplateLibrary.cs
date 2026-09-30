using UnityEngine;

/// <summary>
/// Stable, stage-independent source objects used by JSON/Tiled stages. The library prefab lives in
/// Resources and is never registered as a playable stage.
/// </summary>
public sealed class ModStageTemplateLibrary : MonoBehaviour
{
	public const string ResourcePath = "Modding/StageTemplates/stage_mod_objects";

	[SerializeField] private Stage m_stageShell;
	[SerializeField] private WeaponCase m_weaponCase;
	[SerializeField] private Vendor m_weaponVendor;
	[SerializeField] private Vendor m_usableVendor;
	[SerializeField] private Door m_standardDoor;
	[SerializeField] private DoorRoller m_rollerDoor;
	[SerializeField] private JackyDoor m_jackyDoor;
	[SerializeField] private Switch m_switch;
	[SerializeField] private FuseBox m_fuseBox;
	[SerializeField] private LightBulb m_lightBulb;
	[SerializeField] private Note m_note;
	[SerializeField] private Keypad m_keypad;
	[SerializeField] private Altar m_altar;
	[SerializeField] private ButtonActivator m_activator;
	[SerializeField] private PipeFire m_fireHazard;

	private static ModStageTemplateLibrary s_cached;
	private static bool s_loadAttempted;

	public Stage StageShell => m_stageShell;
	public WeaponCase WeaponCase => m_weaponCase;
	public Vendor WeaponVendor => m_weaponVendor;
	public Vendor UsableVendor => m_usableVendor;
	public Door StandardDoor => m_standardDoor;
	public DoorRoller RollerDoor => m_rollerDoor;
	public JackyDoor JackyDoor => m_jackyDoor;
	public Switch Switch => m_switch;
	public FuseBox FuseBox => m_fuseBox;
	public LightBulb LightBulb => m_lightBulb;
	public Note Note => m_note;
	public Keypad Keypad => m_keypad;
	public Altar Altar => m_altar;
	public ButtonActivator Activator => m_activator;
	public PipeFire FireHazard => m_fireHazard;

	public bool TryGetMissingTemplate(out string o_name)
	{
		if (m_stageShell == null || !m_stageShell.GetIsRuntimeTemplate()
			|| m_stageShell.GetComponent<ManagerWave>() == null
			|| m_stageShell.GetComponentInChildren<NavMap>(true) == null
			|| m_stageShell.GetWaypointStart() == null
			|| m_stageShell.GetLightGlobal() == null)
		{ o_name = nameof(StageShell); return true; }
		if (m_weaponCase == null || !m_weaponCase.IsUsableModTemplate()) { o_name = nameof(WeaponCase); return true; }
		if (m_weaponVendor == null || m_weaponVendor.GetVendorType() != VendorType.Weapons
			|| !m_weaponVendor.IsUsableModTemplate()) { o_name = nameof(WeaponVendor); return true; }
		if (m_usableVendor == null || m_usableVendor.GetVendorType() != VendorType.Usables
			|| !m_usableVendor.IsUsableModTemplate()) { o_name = nameof(UsableVendor); return true; }
		if (m_standardDoor == null || m_standardDoor.GetType() != typeof(Door)
			|| !m_standardDoor.IsUsableModTemplate()) { o_name = nameof(StandardDoor); return true; }
		if (m_rollerDoor == null || !m_rollerDoor.IsUsableModTemplate()) { o_name = nameof(RollerDoor); return true; }
		if (m_jackyDoor == null || !m_jackyDoor.IsUsableModTemplate()) { o_name = nameof(JackyDoor); return true; }
		if (m_switch == null || !m_switch.IsUsableModTemplate()) { o_name = nameof(Switch); return true; }
		if (m_fuseBox == null || !m_fuseBox.IsUsableModTemplate()) { o_name = nameof(FuseBox); return true; }
		if (m_lightBulb == null || !m_lightBulb.IsUsableModTemplate()) { o_name = nameof(LightBulb); return true; }
		if (m_note == null || !m_note.IsUsableModTemplate()) { o_name = nameof(Note); return true; }
		if (m_keypad == null || !m_keypad.IsUsableModTemplate()) { o_name = nameof(Keypad); return true; }
		if (m_altar == null || !m_altar.IsUsableModTemplate()) { o_name = nameof(Altar); return true; }
		if (m_activator == null) { o_name = nameof(Activator); return true; }
		if (m_fireHazard == null) { o_name = nameof(FireHazard); return true; }
		o_name = null;
		return false;
	}

	public static ModStageTemplateLibrary Load()
	{
		if (s_loadAttempted) return s_cached;
		s_loadAttempted = true;
		GameObject asset = Resources.Load<GameObject>(ResourcePath);
		if (asset != null) s_cached = asset.GetComponent<ModStageTemplateLibrary>();
		return s_cached;
	}

	public static void ClearCache()
	{
		s_cached = null;
		s_loadAttempted = false;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void ResetRuntimeCache()
	{
		ClearCache();
	}
}
