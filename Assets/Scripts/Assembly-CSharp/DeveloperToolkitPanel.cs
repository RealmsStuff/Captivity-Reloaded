using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

// Runtime panel for the faithful Captivity Multi-Tool conversion.
public sealed class DeveloperToolkitPanel : MonoBehaviour
{
	private const string ProfileId = "legacy.captivity-multi-tool:rule/multi-tool";
	private const int WindowId = 731905;
	private static bool s_infiniteAmmo;
	private static int? s_intermissionSeconds;
	private bool m_wasAvailable;
	private string m_lastProfileId = string.Empty;
	private bool m_visible;
	private bool m_noReload;
	private bool m_infiniteStrength;
	private bool m_infiniteStamina;
	private int m_tab;
	private int m_weaponIndex;
	private int m_actorIndex;
	private Vector2 m_scroll;
	private string m_confirmation = string.Empty;
	private string m_status = "Ready";
	private Rect m_window = new Rect(24f, 50f, 620f, 650f);
	private readonly List<ContentRegistration> m_weapons = new List<ContentRegistration>();
	private readonly List<ContentRegistration> m_actors = new List<ContentRegistration>();
	private readonly List<Stage> m_stages = new List<Stage>();

	public static bool InfiniteAmmoEnabled => IsPanelAvailable() && s_infiniteAmmo;

	public static bool TryGetIntermissionSeconds(out int o_seconds)
	{
		if (IsPanelAvailable() && s_intermissionSeconds.HasValue)
		{
			o_seconds = s_intermissionSeconds.Value;
			return true;
		}
		o_seconds = 0;
		return false;
	}

	private static bool IsProfileActive()
	{
		return RuleProfileRegistry.CurrentId == ProfileId;
	}

	private static bool IsPanelAvailable()
	{
		foreach (RuleProfileDefinition profile in RuleProfileRegistry.Definitions)
			if (profile != null && profile.Id.ToString() == ProfileId) return true;
		return false;
	}

	private void Awake()
	{
		Debug.Log("[Captivity Multi-Tool] Runtime panel attached. Installed=" + IsPanelAvailable()
			+ ", selectedProfile='" + RuleProfileRegistry.CurrentId + "'.");
	}

	private void Update()
	{
		bool available = IsPanelAvailable();
		if (available && Input.GetKeyDown(KeyCode.F1)) m_visible = !m_visible;
		if (m_wasAvailable && !available) ResetRuntimeOptions();
		m_wasAvailable = available;
		if (!available) return;
		if (m_lastProfileId != RuleProfileRegistry.CurrentId)
		{
			m_lastProfileId = RuleProfileRegistry.CurrentId;
			Debug.Log("[Captivity Multi-Tool] Selected profile changed to '" + m_lastProfileId
				+ "'. C4C baseline active=" + IsProfileActive() + ".");
		}
		Player player = GetPlayer();
		if (player == null) return;
		if (m_infiniteStrength) player.RestoreStrength(10000f);
		if (m_infiniteStamina) player.RestoreStaminaFully();
		if (m_noReload && player.GetEquippableEquipped() is Gun gun) gun.FillEntireGun();
	}

	private void OnGUI()
	{
		if (!IsPanelAvailable()) return;
		if (GUI.Button(new Rect(12f, 12f, 92f, 34f), m_visible ? "Hide tools" : "Tools (F1)")) m_visible = !m_visible;
		if (!m_visible) return;
		float width = Mathf.Clamp(UnityEngine.Screen.width - 32f, 360f, 720f);
		float height = Mathf.Clamp(UnityEngine.Screen.height - 76f, 360f, 760f);
		m_window.width = width;
		m_window.height = height;
		m_window.x = Mathf.Clamp(m_window.x, 0f, Mathf.Max(0f, UnityEngine.Screen.width - width));
		m_window.y = Mathf.Clamp(m_window.y, 0f, Mathf.Max(0f, UnityEngine.Screen.height - height));
		m_window = GUILayout.Window(WindowId, m_window, DrawWindow, "Captivity Multi-Tool");
	}

	private void DrawWindow(int i_id)
	{
		GUILayout.Space(4f);
		m_tab = GUILayout.Toolbar(m_tab, new[] { "Player", "Weapons", "Actors", "Stages" });
		m_scroll = GUILayout.BeginScrollView(m_scroll);
		switch (m_tab)
		{
			case 0: DrawPlayerTab(); break;
			case 1: DrawWeaponsTab(); break;
			case 2: DrawActorsTab(); break;
			case 3: DrawStagesTab(); break;
		}
		GUILayout.EndScrollView();
		GUILayout.FlexibleSpace();
		GUILayout.Label(m_status);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Close", GUILayout.Width(100f))) m_visible = false;
		GUILayout.EndHorizontal();
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 28f));
	}

	private void DrawPlayerTab()
	{
		if (!IsProfileActive())
			GUILayout.Label("Tool controls are active. Select Captivity Multi-Tool in Options to also enable its C4C gameplay rules.", GUI.skin.box);
		GUILayout.Label("Runtime actions");
		Row("Add $10,000", () => { PlayerController controller = GetController(); if (controller != null) controller.GainMoney(10000); SetStatus("Added $10,000."); },
			"Add one heart", () => { Player player = GetPlayer(); if (player != null) player.RestoreAHeart(); SetStatus("Restored one heart (maximum five in CMT)."); });
		Row("Heal 10", () => WithPlayer(player => player.RestoreHealth(10f), "Restored 10 health."),
			"Damage 10", () => WithPlayer(player => player.TakeDamage(10f), "Dealt 10 damage."));
		Row("+100 libido", () => WithPlayer(player => player.GainLibido(100f), "Added 100 libido."),
			"-100 libido", () => WithPlayer(player => player.LoseLibido(100f), "Removed 100 libido."));
		Row("+100 pleasure", () => WithPlayer(player => player.GainPleasureFlat(100f), "Added 100 pleasure."),
			"-100 pleasure", () => WithPlayer(player => player.LosePleasure(100f), "Removed 100 pleasure."));
		Row("Damage strength", () => WithPlayer(player => player.DamageStrength(100f), "Damaged strength by 100."),
			"Orgasm", () => WithPlayer(player => player.Orgasm(), "Triggered orgasm."));
		Row("Mind break (5 orgasms)", () => WithPlayer(player => { for (int index = 0; index < 5; index++) player.Orgasm(); }, "Triggered five orgasms."),
			"Fly pregnancy", AddFlyPregnancy);
		Row("Hypnotize", () => SetHypnosis(true), "Remove hypnosis", () => SetHypnosis(false));

		GUILayout.Space(8f);
		GUILayout.Label("Continuous options");
		s_infiniteAmmo = GUILayout.Toggle(s_infiniteAmmo, "Infinite ammunition");
		m_noReload = GUILayout.Toggle(m_noReload, "No reload (refill equipped gun)");
		m_infiniteStrength = GUILayout.Toggle(m_infiniteStrength, "Infinite strength");
		m_infiniteStamina = GUILayout.Toggle(m_infiniteStamina, "Infinite stamina");

		GUILayout.Space(8f);
		GUILayout.Label("Wave intermission");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("-30 seconds")) AdjustIntermission(-30);
		GUILayout.Label((s_intermissionSeconds ?? 30) + " seconds", GUI.skin.box, GUILayout.Width(130f));
		if (GUILayout.Button("+30 seconds")) AdjustIntermission(30);
		GUILayout.EndHorizontal();

		GUILayout.Space(8f);
		GUILayout.Label("Permanent save actions");
		if (m_confirmation == "challenges") DrawConfirmation("Complete every challenge?", CompleteAllChallenges);
		else if (GUILayout.Button("Complete all challenges...")) m_confirmation = "challenges";
		if (m_confirmation == "clothing") DrawConfirmation("Unlock every clothing item?", UnlockAllClothing);
		else if (GUILayout.Button("Unlock all clothing...")) m_confirmation = "clothing";
	}

	private void DrawWeaponsTab()
	{
		RefreshRegisteredContent();
		GUILayout.Label("Registered Core and mod weapons");
		if (m_weapons.Count == 0) { GUILayout.Label("No weapon templates are currently registered."); return; }
		m_weaponIndex = Mathf.Clamp(m_weaponIndex, 0, m_weapons.Count - 1);
		for (int index = 0; index < m_weapons.Count; index++)
		{
			ContentRegistration entry = m_weapons[index];
			if (GUILayout.Toggle(m_weaponIndex == index, EntryLabel(entry), "Button")) m_weaponIndex = index;
		}
		if (GUILayout.Button("Grant selected weapon")) GrantWeapon(m_weapons[m_weaponIndex]);
	}

	private void DrawActorsTab()
	{
		RefreshRegisteredContent();
		GUILayout.Label("Registered Core and mod enemies");
		if (m_actors.Count == 0) { GUILayout.Label("No enemy templates are currently registered."); return; }
		m_actorIndex = Mathf.Clamp(m_actorIndex, 0, m_actors.Count - 1);
		for (int index = 0; index < m_actors.Count; index++)
		{
			ContentRegistration entry = m_actors[index];
			if (GUILayout.Toggle(m_actorIndex == index, EntryLabel(entry), "Button")) m_actorIndex = index;
		}
		ContentRegistration selected = m_actors[m_actorIndex];
		Row("Spawn", () => SpawnActor(selected, ActorAction.Normal), "Add pregnancy", () => SpawnActor(selected, ActorAction.Pregnancy));
		Row("Spawn ragdolled", () => SpawnActor(selected, ActorAction.Ragdoll), "Spawn without AI", () => SpawnActor(selected, ActorAction.NoThinking));
	}

	private void DrawStagesTab()
	{
		RefreshStages();
		GUILayout.Label("Available Core and mod stages");
		if (m_stages.Count == 0) { GUILayout.Label("No stages are currently available."); return; }
		foreach (Stage stage in m_stages)
			if (stage != null && GUILayout.Button(stage.GetName())) OpenStage(stage);
	}

	private void RefreshRegisteredContent()
	{
		m_weapons.Clear();
		m_actors.Clear();
		ContentRegistry registry = ModLoaderRuntime.Registry;
		if (registry == null) return;
		foreach (ContentRegistration entry in registry.GetByCategory(ContentCategory.Item))
			if (entry.RuntimeAsset is Gun) m_weapons.Add(entry);
		foreach (ContentRegistration entry in registry.GetByCategory(ContentCategory.Enemy))
			if (entry.RuntimeAsset is NPC) m_actors.Add(entry);
	}

	private void RefreshStages()
	{
		m_stages.Clear();
		ManagerStages manager = GetStages();
		if (manager == null) return;
		foreach (Stage stage in manager.GetAllStages())
			if (stage != null && !m_stages.Contains(stage)) m_stages.Add(stage);
	}

	private void GrantWeapon(ContentRegistration i_entry)
	{
		Player player = GetPlayer();
		Gun template = i_entry == null ? null : i_entry.RuntimeAsset as Gun;
		if (player == null || template == null) { SetStatus("That weapon is not available in the current session."); return; }
		Gun granted = Instantiate(template, player.transform.parent);
		player.PickUp(granted, false);
		SetStatus("Granted " + template.GetName() + ".");
	}

	private enum ActorAction { Normal, Pregnancy, Ragdoll, NoThinking }

	private void SpawnActor(ContentRegistration i_entry, ActorAction i_action)
	{
		Player player = GetPlayer();
		NPC template = i_entry == null ? null : i_entry.RuntimeAsset as NPC;
		ManagerStages manager = GetStages();
		Stage stage = manager == null ? null : manager.GetStageCurrent();
		if (player == null || template == null || stage == null) { SetStatus("That enemy cannot be used in the current stage."); return; }
		if (i_action == ActorAction.Pregnancy)
		{
			player.CreateAndAddFetus(null, template, 3f, template.GetName());
			SetStatus("Added " + template.GetName() + " pregnancy.");
			return;
		}
		NPC spawned = Instantiate(template, stage.GetActorsParent());
		spawned.transform.position = player.transform.position + new Vector3(3f, 1f, 0f);
		spawned.gameObject.SetActive(true);
		spawned.SetIsIgnoreWave(true);
		spawned.Spawn(false);
		if (i_action == ActorAction.Ragdoll) spawned.Ragdoll(99999f);
		if (i_action == ActorAction.NoThinking) spawned.SetIsThinking(false);
		SetStatus("Spawned " + template.GetName() + ".");
	}

	private void AddFlyPregnancy()
	{
		Player player = GetPlayer();
		Actor fly = Library.Instance == null ? null : Library.Instance.Actors.GetActor("Fly");
		if (player == null || fly == null) { SetStatus("Fly template is unavailable."); return; }
		player.CreateAndAddFetus(null, fly, 3f, "Fly");
		SetStatus("Added Fly pregnancy.");
	}

	private void SetHypnosis(bool i_enabled)
	{
		Player player = GetPlayer();
		if (player == null) return;
		if (i_enabled)
		{
			player.EnableHypnotized();
			player.EnableStrengthDrain();
			player.GainLibido(500f);
			player.SetIsExposing(true);
			player.SetIsForceIgnoreInput(true);
			SetStatus("Hypnosis applied. Use this panel to remove it.");
		}
		else
		{
			player.DisableHypnotized();
			player.DisableStrengthDrain();
			player.SetIsExposing(false);
			player.SetIsForceIgnoreInput(false);
			SetStatus("Hypnosis removed.");
		}
	}

	private void CompleteAllChallenges()
	{
		ManagerChallenge manager = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerChallenge();
		if (manager == null) return;
		foreach (Challenge challenge in manager.GetAllChallenges()) manager.CompleteChallenge(challenge);
		m_confirmation = string.Empty;
		SetStatus("All challenges completed and saved.");
	}

	private void UnlockAllClothing()
	{
		ManagerDB.UnlockAllClothes();
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Clothing)) ModContentUnlockState.Unlock(entry.Id);
		m_confirmation = string.Empty;
		SetStatus("All Core and registered mod clothing unlocked.");
	}

	private void OpenStage(Stage i_stage)
	{
		ManagerStages manager = GetStages();
		if (manager == null || i_stage == null) return;
		m_visible = false;
		manager.OpenStage(i_stage);
		SetStatus("Opened " + i_stage.GetName() + ".");
	}

	private void AdjustIntermission(int i_delta)
	{
		s_intermissionSeconds = Mathf.Clamp((s_intermissionSeconds ?? 30) + i_delta, 0, 3600);
		SetStatus("Wave intermission set to " + s_intermissionSeconds.Value + " seconds.");
	}

	private void DrawConfirmation(string i_text, Action i_confirm)
	{
		GUILayout.BeginVertical(GUI.skin.box);
		GUILayout.Label(i_text + " This permanently changes the active save.");
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Confirm")) i_confirm();
		if (GUILayout.Button("Cancel")) m_confirmation = string.Empty;
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	private static void Row(string i_leftLabel, Action i_left, string i_rightLabel, Action i_right)
	{
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(i_leftLabel)) i_left();
		if (GUILayout.Button(i_rightLabel)) i_right();
		GUILayout.EndHorizontal();
	}

	private void WithPlayer(Action<Player> i_action, string i_status)
	{
		Player player = GetPlayer();
		if (player == null) { SetStatus("Player is unavailable."); return; }
		i_action(player);
		SetStatus(i_status);
	}

	private static string EntryLabel(ContentRegistration i_entry)
	{
		if (i_entry == null) return "Unavailable";
		Actor actor = i_entry.RuntimeAsset as Actor;
		if (actor != null) return actor.GetName() + "  [" + i_entry.Id + "]";
		PickUpable item = i_entry.RuntimeAsset as PickUpable;
		return item == null ? i_entry.Id.ToString() : item.GetName() + "  [" + i_entry.Id + "]";
	}

	private void SetStatus(string i_status)
	{
		m_status = i_status;
		Debug.Log("[Captivity Multi-Tool] " + i_status);
	}

	private void ResetRuntimeOptions()
	{
		s_infiniteAmmo = false;
		s_intermissionSeconds = null;
		m_noReload = false;
		m_infiniteStrength = false;
		m_infiniteStamina = false;
		m_visible = false;
		m_confirmation = string.Empty;
	}

	private static Player GetPlayer() { return CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer(); }
	private static PlayerController GetController() { return CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayerController(); }
	private static ManagerStages GetStages() { return CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerStages(); }
}
