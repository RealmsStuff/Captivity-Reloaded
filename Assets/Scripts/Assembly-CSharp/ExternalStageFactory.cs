using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using UnityEngine;
using UnityEngine.Networking;

public static class ExternalStageFactory
{
	private const string TiledSpriteMaterialResource = "materials/SpriteDiffuse";
	private static Material s_tiledSpriteMaterial;

	// ManagerStages instantiates a second, playable copy of each registered stage.
	// Unity copies serialized fields, but not the runtime-only configuration below.
	public static void RestoreRuntimeStageClone(Stage i_source, Stage i_clone)
	{
		if (i_source == null || i_clone == null) return;
		RuntimeContentIdentity identity = i_source.GetComponent<RuntimeContentIdentity>();
		if (identity == null || !identity.TryGetContentId(out ContentId stageId) || stageId.Namespace == "core") return;
		Dictionary<UnityEngine.Object, UnityEngine.Object> counterparts = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
		MapStageCloneTree(i_source.transform, i_clone.transform, counterparts);
		i_clone.CopyModRuntimeSettingsFrom(i_source);
		foreach (KeyValuePair<UnityEngine.Object, UnityEngine.Object> pair in counterparts)
		{
			if (pair.Key is Spawner sourceSpawner && pair.Value is Spawner cloneSpawner)
				cloneSpawner.CopyModRuntimeSettingsFrom(sourceSpawner);
			if (pair.Key is NavMap sourceNav && pair.Value is NavMap cloneNav)
				cloneNav.CopyModRuntimeSettingsFrom(sourceNav, counterparts);
			if (pair.Key is LightBulb sourceLight && pair.Value is LightBulb cloneLight)
				cloneLight.CopyModRuntimePresentationFrom(sourceLight);
			if (!(pair.Key is MonoBehaviour source) || !(pair.Value is MonoBehaviour clone)) continue;
			if (source is ModStageScriptRunner sourceRunner && clone is ModStageScriptRunner cloneRunner)
				cloneRunner.RestoreCloneConfiguration(sourceRunner);
			else if (IsRuntimeConfiguredStageComponent(source))
				CopyRuntimeStageFields(source, clone, counterparts);
		}
		foreach (ModMapInteractionVisual visual in i_clone.GetComponentsInChildren<ModMapInteractionVisual>(true))
			visual.RestoreCloneSubscription();
	}

	private static bool IsRuntimeConfiguredStageComponent(MonoBehaviour i_component)
	{
		return i_component is ModStageMarker || i_component is ModStagePickupSignal
			|| i_component is ModStageScriptTrigger || i_component is ModRetainedCoreStageObject
			|| i_component is ModMapInteraction || i_component is ModMapInteractionVisual
			|| i_component is ModProximityLight || i_component is ModPulsingLight
			|| i_component is ModMovingPlatform || i_component is ModMapAudioSource
			|| i_component is ModAnimatedSprite || i_component is ModParallaxLayer
			|| i_component is ModRoomController || i_component is ModRoomVolume
			|| i_component is ModRoomTransition || i_component is ModFinisherTestHarness;
	}

	private static void MapStageCloneTree(Transform i_source, Transform i_clone,
		Dictionary<UnityEngine.Object, UnityEngine.Object> io_counterparts)
	{
		io_counterparts[i_source] = i_clone;
		io_counterparts[i_source.gameObject] = i_clone.gameObject;
		Component[] sourceComponents = i_source.GetComponents<Component>();
		Component[] cloneComponents = i_clone.GetComponents<Component>();
		if (sourceComponents.Length != cloneComponents.Length || i_source.childCount != i_clone.childCount)
			throw new InvalidOperationException("Stage clone hierarchy changed before runtime configuration could be restored: " + i_source.name);
		for (int index = 0; index < sourceComponents.Length; index++)
		{
			if (sourceComponents[index] == null || cloneComponents[index] == null) continue;
			if (sourceComponents[index].GetType() != cloneComponents[index].GetType())
				throw new InvalidOperationException("Stage clone component order changed: " + i_source.name);
			io_counterparts[sourceComponents[index]] = cloneComponents[index];
		}
		for (int index = 0; index < i_source.childCount; index++)
			MapStageCloneTree(i_source.GetChild(index), i_clone.GetChild(index), io_counterparts);
	}

	private static void CopyRuntimeStageFields(MonoBehaviour i_source, MonoBehaviour i_clone,
		Dictionary<UnityEngine.Object, UnityEngine.Object> i_counterparts)
	{
		const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Instance
			| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
			| System.Reflection.BindingFlags.DeclaredOnly;
		foreach (System.Reflection.FieldInfo field in i_source.GetType().GetFields(flags))
		{
			object value = field.GetValue(i_source);
			if (value is System.Collections.IDictionary sourceDictionary
				&& field.GetValue(i_clone) is System.Collections.IDictionary cloneDictionary)
			{
				cloneDictionary.Clear();
				foreach (System.Collections.DictionaryEntry entry in sourceDictionary)
					cloneDictionary.Add(RemapStageCloneValue(entry.Key, i_counterparts),
						RemapStageCloneValue(entry.Value, i_counterparts));
			}
			else if (value is System.Collections.IList sourceList
				&& field.GetValue(i_clone) is System.Collections.IList cloneList)
			{
				cloneList.Clear();
				foreach (object entry in sourceList) cloneList.Add(RemapStageCloneValue(entry, i_counterparts));
			}
			else if (!field.IsInitOnly)
				field.SetValue(i_clone, RemapStageCloneValue(value, i_counterparts));
		}
	}

	private static object RemapStageCloneValue(object i_value,
		Dictionary<UnityEngine.Object, UnityEngine.Object> i_counterparts)
	{
		if (i_value is UnityEngine.Object unityObject && i_counterparts.TryGetValue(unityObject, out UnityEngine.Object clone))
			return clone;
		return i_value;
	}

	public static void Schedule(ManagerStages i_manager)
	{
		if (i_manager == null || ModLoaderRuntime.StageDefinitions.Count == 0) return;
		ExternalStageFactoryHost host = ExternalFactoryRunner.GetOrAdd<ExternalStageFactoryHost>();
		host.Begin(i_manager);
	}

	internal static void Build(ManagerStages i_manager)
	{
		// -1 and -2 are reserved for the hidden shell and object-library prefabs.
		int runtimeId = -1000;
		foreach (StageDefinition definition in ModLoaderRuntime.StageDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			Stage template = null;
			if (definition.UsesRuntimeTemplate)
			{
				ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
				if (library != null && library.TryGetMissingTemplate(out string missingTemplate))
				{
					Report("stage.factory-runtime-template-incomplete",
						"The shared runtime template library is incomplete: " + missingTemplate + ".", definition.Source);
					continue;
				}
				template = library == null ? null : library.StageShell;
			}
			else if (ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry))
				template = baseEntry.RuntimeAsset as Stage;
			if (template == null)
			{
				Report("stage.factory-template", definition.UsesRuntimeTemplate
					? "The shared runtime stage template is unavailable. Regenerate the shared stage templates in the Unity editor."
					: "Core stage template is not bound: " + definition.Extends, definition.Source);
				continue;
			}

			Stage clone = UnityEngine.Object.Instantiate(template, i_manager.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Stage);
			Vector2 playerSpawn = GetPlayerSpawn(definition);
			if (definition.Layout.Type == "tiledJson")
			{
				if (definition.Layout.PreserveInheritedStageObjects) AlignInheritedStageObjects(clone, playerSpawn);
				else AlignInheritedStageLights(clone, playerSpawn, definition);
			}
			clone.ConfigureModStage(runtimeId--, definition.DisplayName, definition.Description, playerSpawn);
			clone.ConfigureModCameraBounds(GetCameraBounds(definition));
			ExternalFactoryRunner.GetOrAdd<ModStageAudioLoader>().Load(clone, definition);
			if (definition.Layout.Type == "tiledJson") BuildTiledLayout(clone, definition);
			List<StageScriptDefinition> stageScripts = new List<StageScriptDefinition>();
			foreach (StageScriptDefinition script in ModLoaderRuntime.StageScriptDefinitions)
				if (script.Stage == definition.Id) stageScripts.Add(script);
			if (stageScripts.Count > 0) clone.gameObject.AddComponent<ModStageScriptRunner>().Configure(stageScripts);
			if (definition.TestTools?.Enabled == true)
				clone.gameObject.AddComponent<ModFinisherTestHarness>().Configure(clone, definition.TestTools);

			Spawner[] inheritedSpawners = clone.GetComponentsInChildren<Spawner>(true);
			bool reuseInheritedSpawners = definition.Layout.ReuseInheritedSpawners
				&& inheritedSpawners.Length == definition.Spawners.Count;
			if (definition.Layout.ReuseInheritedSpawners && !reuseInheritedSpawners)
				Report("stage.factory-inherited-spawner-count", "reuseInheritedSpawners requires exactly "
					+ inheritedSpawners.Length + " spawner definitions in prefab order.", definition.Source);
			if (!reuseInheritedSpawners)
				foreach (Spawner inherited in inheritedSpawners) inherited.gameObject.SetActive(false);

			bool valid = true;
			Dictionary<Spawner, Spawner> replacedSpawners = new Dictionary<Spawner, Spawner>();
			for (int spawnerIndex = 0; spawnerIndex < definition.Spawners.Count; spawnerIndex++)
			{
				StageSpawnerDefinition spawnerDefinition = definition.Spawners[spawnerIndex];
				List<NPC> enemies = new List<NPC>();
				foreach (ContentId enemyId in spawnerDefinition.Enemies)
				{
					if (!ModLoaderRuntime.Registry.TryGet(enemyId, out ContentRegistration enemyEntry) || !(enemyEntry.RuntimeAsset is NPC enemy))
					{
						Report("stage.factory-enemy", "Spawner enemy is not built: " + enemyId, definition.Source);
						valid = false;
						break;
					}
					enemies.Add(enemy);
				}
				if (!valid) break;
				GameObject spawnerObject = reuseInheritedSpawners
					? inheritedSpawners[spawnerIndex].gameObject : new GameObject();
				spawnerObject.name = "mod-spawner_" + spawnerDefinition.Id;
				if (!reuseInheritedSpawners) spawnerObject.transform.SetParent(clone.transform);
				StagePointDefinition spawnerPosition = GetSpawnerPosition(definition, spawnerDefinition);
				spawnerObject.transform.position = clone.transform.TransformPoint(new Vector2(spawnerPosition.X, spawnerPosition.Y));
				Spawner spawner = reuseInheritedSpawners ? inheritedSpawners[spawnerIndex] : spawnerObject.AddComponent<Spawner>();
				spawner.ConfigureModSpawner(enemies, spawnerDefinition);
				if (!reuseInheritedSpawners && spawnerIndex < inheritedSpawners.Length) replacedSpawners[inheritedSpawners[spawnerIndex]] = spawner;
			}
			if (!reuseInheritedSpawners)
				foreach (Interactable interaction in clone.GetComponentsInChildren<Interactable>(true))
					interaction.RemapModSpawnerActivationLinks(replacedSpawners);

			if (!valid)
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ManagerWave wave = clone.GetManagerWave();
			if (wave == null)
			{
				Report("stage.factory-wave", "Core stage template has no ManagerWave.", definition.Source);
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			wave.ConfigureModWave(definition.Waves.FirstWaveEnemyCount);
			i_manager.AddRuntimeStage(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external stage " + definition.Id + " from "
				+ (definition.UsesRuntimeTemplate ? "the shared runtime template" : definition.Extends.ToString()) + ".");
		}
	}

	private static void AlignInheritedStageObjects(Stage i_stage, Vector2 i_authoredPlayerSpawn)
	{
		Waypoint inheritedSpawn = i_stage.GetWaypointStart();
		if (inheritedSpawn == null) return;
		Vector3 targetWorld = i_stage.transform.TransformPoint(i_authoredPlayerSpawn);
		Vector3 offset = targetWorld - inheritedSpawn.transform.position;
		if (offset.sqrMagnitude < 0.000001f) return;
		// Move the prefab's complete authored hierarchy as one layout. This keeps doors,
		// scripted actors, trigger zones, display skeletons, and their cross-references aligned.
		// Runtime Tiled objects are created afterwards and therefore are not shifted twice.
		for (int index = 0; index < i_stage.transform.childCount; index++)
			i_stage.transform.GetChild(index).position += offset;
	}

	private static void AlignInheritedStageLights(Stage i_stage, Vector2 i_authoredPlayerSpawn,
		StageDefinition i_definition)
	{
		Waypoint inheritedSpawn = i_stage.GetWaypointStart();
		if (inheritedSpawn == null) return;
		Vector3 targetWorld = i_stage.transform.TransformPoint(i_authoredPlayerSpawn);
		Vector3 offset = targetWorld - inheritedSpawn.transform.position;
		if (offset.sqrMagnitude < 0.000001f) return;
		UnityEngine.Rendering.Universal.Light2D[] lights =
			i_stage.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
		foreach (UnityEngine.Rendering.Universal.Light2D light in lights)
		{
			// Tiled vendors are cloned as complete prefabs later. Keep their child lights
			// at the original local offsets so moving the vendor moves each light once.
			if (light.GetComponentInParent<Vendor>(true) != null) continue;
			bool positionedByTiledAdapter = false;
			foreach (TiledCoreStageObjectDefinition adapter in
				i_definition.Layout.TiledLevel.CoreStageObjects)
			{
				string sourcePath = adapter.Kind == "catalog-prop"
					? ResolveCoreStageProp(adapter.SourcePath) : adapter.SourcePath;
				Transform retainedRoot = FindCoreStageObject(i_stage.transform, sourcePath);
				if (retainedRoot != null && (light.transform == retainedRoot
					|| light.transform.IsChildOf(retainedRoot)))
				{
					positionedByTiledAdapter = true;
					break;
				}
			}
			// BuildTiledCoreStageObjects positions the retained root immediately after this.
			// Moving a nested light here as well would apply the authored stage offset twice.
			if (!positionedByTiledAdapter) light.transform.position += offset;
		}
	}

	private static Rect GetCameraBounds(StageDefinition i_definition)
	{
		if (i_definition.Camera?.Unbounded == true) return default;
		if (i_definition.Camera?.Bounds != null)
		{
			StageCameraBoundsDefinition bounds = i_definition.Camera.Bounds;
			return Rect.MinMaxRect(bounds.MinX, bounds.MinY, bounds.MaxX, bounds.MaxY);
		}
		if (i_definition.Layout.Type == "tiledJson" && i_definition.Layout.TiledLevel != null)
		{
			float width = i_definition.Layout.TiledLevel.WidthPixels / (float)i_definition.Layout.PixelsPerUnit;
			float height = i_definition.Layout.TiledLevel.HeightPixels / (float)i_definition.Layout.PixelsPerUnit;
			return new Rect(width * -0.5f, 0f, width, height);
		}
		return default;
	}

	private static Vector2 GetPlayerSpawn(StageDefinition i_definition)
	{
		if (i_definition.Layout.Type == "tiledJson")
		{
			StagePointDefinition point = i_definition.Layout.TiledLevel.ToUnityPoint(
				i_definition.Layout.TiledLevel.PlayerSpawn, i_definition.Layout.PixelsPerUnit);
			return new Vector2(point.X, point.Y);
		}
		return new Vector2(i_definition.Layout.PlayerSpawn.X, i_definition.Layout.PlayerSpawn.Y);
	}

	private static StagePointDefinition GetSpawnerPosition(StageDefinition i_definition, StageSpawnerDefinition i_spawner)
	{
		if (i_definition.Layout.Type == "tiledJson")
		{
			foreach (TiledLevelObject point in i_definition.Layout.TiledLevel.EnemySpawners)
				if (point.Name == i_spawner.Id)
					return i_definition.Layout.TiledLevel.ToUnityPoint(point, i_definition.Layout.PixelsPerUnit);
		}
		return i_spawner.Position;
	}

	private static void BuildTiledLayout(Stage i_stage, StageDefinition i_definition)
	{
		SelectTiledSpriteMaterial(i_stage);
		BuildTiledCoreStageObjects(i_stage, i_definition);
		if (!i_definition.Layout.PreserveInheritedStageObjects)
			foreach (Interactable inherited in i_stage.GetComponentsInChildren<Interactable>(true))
				if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		if (!i_definition.Layout.PreserveInheritedStageObjects)
			foreach (AudioSource inherited in i_stage.GetComponentsInChildren<AudioSource>(true))
				if (!HasRetainedCoreStageObjectAncestor(inherited.transform)
					&& IsUnderNamedAncestor(inherited.transform, "Audio", i_stage.transform)) inherited.enabled = false;
		if (i_definition.Layout.HideInheritedVisuals)
			foreach (SpriteRenderer inherited in i_stage.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
				if (!HasRetainedCoreStageObjectAncestor(inherited.transform) && !HasInteractableAncestor(inherited.transform)
					&& inherited.GetComponentInParent<Actor>(true) == null
					&& (!i_definition.Layout.PreserveInheritedStageObjects
						|| !HasSkeletonHierarchy(inherited.transform)))
					inherited.enabled = false;
		foreach (Collider2D inherited in i_stage.GetComponentsInChildren<Collider2D>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)
				&& inherited.GetComponentInParent<Actor>(true) == null
				&& (!i_definition.Layout.PreserveInheritedStageObjects || (!inherited.isTrigger
				&& !HasSkeletonHierarchy(inherited.transform)))
				&& !HasInteractableAncestor(inherited.transform)
				&& inherited.GetComponentInChildren<Interactable>(true) == null)
				inherited.enabled = false;
		foreach (Platform inherited in i_stage.GetComponentsInChildren<Platform>(includeInactive: true))
			inherited.gameObject.SetActive(false);
		GameObject layoutRoot = new GameObject("mod-tiled-layout");
		layoutRoot.transform.SetParent(i_stage.transform, false);
		BuildTiledGlobalLight(i_stage, layoutRoot.transform, i_definition);
		BuildTiledVisuals(layoutRoot.transform, i_definition);
		BuildTiledParticles(layoutRoot.transform, i_definition);
		BuildTiledAudio(layoutRoot.transform, i_definition);
		BuildTiledRooms(i_stage, layoutRoot.transform, i_definition);
		BuildTiledStageMarkers(layoutRoot.transform, i_definition);
		BuildTiledScriptTriggers(i_stage, layoutRoot.transform, i_definition);
		BuildTiledVendors(i_stage, layoutRoot.transform, i_definition);
		BuildTiledWeaponCases(i_stage, layoutRoot.transform, i_definition);
		BuildTiledPickups(i_stage, i_definition);
		BuildTiledStageItems(i_stage, i_definition);
		BuildTiledScriptedActors(i_stage, i_definition);
		BuildTiledDoors(i_stage, layoutRoot.transform, i_definition);
		BuildTiledProximityLights(layoutRoot.transform, i_definition);
		BuildTiledFreeformLights(i_stage, layoutRoot.transform, i_definition);
		BuildTiledPointLights(i_stage, layoutRoot.transform, i_definition);
		foreach (Interactable interaction in i_stage.GetComponentsInChildren<Interactable>(true))
		{
			if (!interaction.gameObject.name.StartsWith("mod-", StringComparison.Ordinal)) continue;
			ModStageMarker marker = interaction.GetComponent<ModStageMarker>();
			string objectId = marker != null && !string.IsNullOrEmpty(marker.ObjectId)
				? marker.ObjectId : ModStageScriptRunner.InteractionObjectId(interaction.gameObject.name);
			interaction.gameObject.AddComponent<ModStageActivationBridge>().Configure(objectId);
		}
		List<Platform> platforms = new List<Platform>();
		foreach (TiledLevelObject platformDefinition in i_definition.Layout.TiledLevel.Platforms)
		{
			StagePointDefinition center = i_definition.Layout.TiledLevel.ToUnityRectangleCenter(
				platformDefinition, i_definition.Layout.PixelsPerUnit);
			GameObject platformObject = new GameObject("mod-platform_" + platformDefinition.Id);
			platformObject.layer = LayerMask.NameToLayer("Platform");
			platformObject.transform.SetParent(layoutRoot.transform, false);
			platformObject.transform.localPosition = new Vector2(center.X, center.Y);
			BoxCollider2D collider = platformObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector2(platformDefinition.Width / i_definition.Layout.PixelsPerUnit,
				platformDefinition.Height / i_definition.Layout.PixelsPerUnit);
			Platform platform = platformObject.AddComponent<Platform>();
			platform.ConfigureModPlatform(platformDefinition.ClimbableLeft, platformDefinition.ClimbableRight);
			platforms.Add(platform);
		}
		foreach (TiledMovingPlatformDefinition definition in i_definition.Layout.TiledLevel.MovingPlatforms)
		{
			StagePointDefinition center = i_definition.Layout.TiledLevel.ToUnityRectangleCenter(
				definition.Rectangle, i_definition.Layout.PixelsPerUnit);
			GameObject platformObject = new GameObject("mod-moving-platform_" + definition.Rectangle.Id);
			platformObject.layer = LayerMask.NameToLayer("Platform");
			platformObject.transform.SetParent(layoutRoot.transform, false);
			platformObject.transform.localPosition = new Vector2(center.X, center.Y);
			BoxCollider2D collider = platformObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector2(definition.Rectangle.Width / i_definition.Layout.PixelsPerUnit,
				definition.Rectangle.Height / i_definition.Layout.PixelsPerUnit);
			Rigidbody2D body = platformObject.AddComponent<Rigidbody2D>();
			body.bodyType = RigidbodyType2D.Kinematic;
			body.gravityScale = 0f;
			Platform platform = platformObject.AddComponent<Platform>();
			platform.ConfigureModPlatform(definition.Rectangle.ClimbableLeft,
				definition.Rectangle.ClimbableRight);
			Sprite movingSprite = FindCoreMapArtSprite(definition.Art.SpriteName);
			if (movingSprite != null)
			{
				GameObject visualObject = new GameObject("visual");
				visualObject.transform.SetParent(platformObject.transform, false);
				SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
				ApplyTiledSpriteMaterial(renderer);
				renderer.sprite = movingSprite;
				renderer.sortingLayerName = "Platform";
				visualObject.transform.localScale = new Vector3(
					(definition.Rectangle.Width / i_definition.Layout.PixelsPerUnit) / movingSprite.bounds.size.x,
					(definition.Rectangle.Height / i_definition.Layout.PixelsPerUnit) / movingSprite.bounds.size.y, 1f);
			}
			ModMovingPlatform motion = platformObject.AddComponent<ModMovingPlatform>();
			motion.Configure(new Vector2(definition.OffsetX / i_definition.Layout.PixelsPerUnit,
				-definition.OffsetY / i_definition.Layout.PixelsPerUnit), definition.TravelSeconds, definition.PauseSeconds);
		}
		NavMap navMap = i_stage.GetComponentInChildren<NavMap>(includeInactive: true);
		if (navMap != null)
		{
			navMap.ConfigureModPlatforms(platforms);
			BuildTiledNavNodes(i_stage, navMap, platforms, i_definition);
		}
	}

	private static void BuildTiledCoreStageObjects(Stage i_stage, StageDefinition i_definition)
	{
		foreach (TiledCoreStageObjectDefinition definition in i_definition.Layout.TiledLevel.CoreStageObjects)
		{
			string sourcePath = definition.Kind == "catalog-prop"
				? ResolveCoreStageProp(definition.SourcePath) : definition.SourcePath;
			Transform source = FindCoreStageObject(i_stage.transform, sourcePath);
			if (source == null)
			{
				Report("stage.factory-core-stage-object", "Inherited Core stage object was not found: " + definition.SourcePath, i_definition.Source);
				continue;
			}
			ModRetainedCoreStageObject retained = source.GetComponent<ModRetainedCoreStageObject>();
			if (retained == null) retained = source.gameObject.AddComponent<ModRetainedCoreStageObject>();
			retained.Configure(definition.Point.Name);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(definition.Point, i_definition.Layout.PixelsPerUnit);
			Vector3 authoredWorld = i_stage.transform.TransformPoint(new Vector3(position.X, position.Y, 0f));
			source.position = new Vector3(authoredWorld.x, authoredWorld.y, source.position.z);
			source.gameObject.SetActive(definition.InitiallyActive);
		}
	}

	private static string ResolveCoreStageProp(string i_assetId)
	{
		return s_coreFerProps.TryGetValue(i_assetId ?? string.Empty, out string path) ? path : string.Empty;
	}

	private static readonly Dictionary<string, string> s_coreFerProps = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		{ "core:stage-prop/fer/as-jennydining", "AndroidStatuePoses[13]/AS_JennyDining[2]/as_jennyDining[1]" },
		{ "core:stage-prop/fer/as-jennytheatre", "AndroidStatuePoses[13]/AS_JennyTheatre[0]/as_jennyTheatre[1]" },
		{ "core:stage-prop/fer/as-sunnydining", "AndroidStatuePoses[13]/AS_SunnyDining[3]/as_sunnyDining[1]" },
		{ "core:stage-prop/fer/as-sunnytheatre", "AndroidStatuePoses[13]/AS_SunnyTheatre[1]/as_sunnyTheatre[1]" },
		{ "core:stage-prop/fer/particle-system-1", "AreaLights[6]/Particle System (1)[4]" },
		{ "core:stage-prop/fer/particle-system", "AreaLights[6]/Particle System[3]" },
		{ "core:stage-prop/fer/android-1", "Decoration[2]/Attic[3]/Androids[0]/android (1)[0]" },
		{ "core:stage-prop/fer/android-2", "Decoration[2]/Attic[3]/Androids[0]/android (2)[1]" },
		{ "core:stage-prop/fer/android-3", "Decoration[2]/Attic[3]/Androids[0]/android (3)[3]" },
		{ "core:stage-prop/fer/android-4", "Decoration[2]/Attic[3]/Androids[0]/android (4)[4]" },
		{ "core:stage-prop/fer/android-5", "Decoration[2]/Attic[3]/Androids[0]/android (5)[5]" },
		{ "core:stage-prop/fer/android-6", "Decoration[2]/Attic[3]/Androids[0]/android (6)[6]" },
		{ "core:stage-prop/fer/android-7", "Decoration[2]/Attic[3]/Androids[0]/android (7)[2]" },
		{ "core:stage-prop/fer/android-8", "Decoration[2]/Attic[3]/Androids[0]/android (8)[11]" },
		{ "core:stage-prop/fer/dc-brainmachine", "Decoration[2]/Basement[5]/dc_brainMachine[18]" },
		{ "core:stage-prop/fer/dc-deadbodyraped", "Decoration[2]/Basement[5]/dc_deadBodyRaped[2]" },
		{ "core:stage-prop/fer/dc-deadbody", "Decoration[2]/Basement[5]/dc_deadBody[3]" },
		{ "core:stage-prop/fer/dc-deadbody-2", "Decoration[2]/Basement[5]/dc_deadBody[4]" },
		{ "core:stage-prop/fer/dc-hangingbody", "Decoration[2]/Basement[5]/dc_hangingBody[1]" },
		{ "core:stage-prop/fer/dc-jackyhanging", "Decoration[2]/JackyRoom[2]/dc_chain (1)[6]/dc_jackyHanging[0]" },
		{ "core:stage-prop/fer/dc-jackyhanging-2", "Decoration[2]/JackyRoom[2]/dc_chain[5]/dc_jackyHanging[0]" },
		{ "core:stage-prop/fer/android-1-2", "Decoration[2]/Lab[4]/dc_androidCloset[2]/android (1)[1]" },
		{ "core:stage-prop/fer/android-2-2", "Decoration[2]/Lab[4]/dc_androidCloset[2]/android (2)[2]" },
		{ "core:stage-prop/fer/android-3-2", "Decoration[2]/Lab[4]/dc_androidCloset[2]/android (3)[3]" },
		{ "core:stage-prop/fer/android-4-2", "Decoration[2]/Lab[4]/dc_androidCloset[2]/android (4)[4]" },
		{ "core:stage-prop/fer/android", "Decoration[2]/Lab[4]/dc_androidCloset[2]/android[0]" },
		{ "core:stage-prop/fer/android-9", "Decoration[2]/Lab[4]/dc_surgeryBed[0]/android[0]" },
		{ "core:stage-prop/fer/int-lampjackyroom", "Interactables[3]/int_lampJackyRoom[20]" }
	};

	private static Transform FindCoreStageObject(Transform i_root, string i_path)
	{
		Transform current = i_root;
		foreach (string rawSegment in (i_path ?? string.Empty).Split('/'))
		{
			string name = rawSegment;
			int siblingIndex = -1;
			int bracket = rawSegment.LastIndexOf('[');
			if (bracket > 0 && rawSegment.EndsWith("]", StringComparison.Ordinal)
				&& int.TryParse(rawSegment.Substring(bracket + 1, rawSegment.Length - bracket - 2), out int parsed))
			{
				name = rawSegment.Substring(0, bracket);
				siblingIndex = parsed;
			}
			Transform next = null;
			for (int index = 0; current != null && index < current.childCount; index++)
			{
				Transform child = current.GetChild(index);
				if (child.name == name && (siblingIndex < 0 || child.GetSiblingIndex() == siblingIndex)) { next = child; break; }
			}
			if (next == null) return null;
			current = next;
		}
		return current == i_root ? null : current;
	}

	private static bool HasRetainedCoreStageObjectAncestor(Transform i_transform)
	{
		return i_transform != null && i_transform.GetComponentInParent<ModRetainedCoreStageObject>(true) != null;
	}

	private static bool IsUnderNamedAncestor(Transform i_transform, string i_name, Transform i_root)
	{
		for (Transform current = i_transform; current != null && current != i_root; current = current.parent)
			if (current.name == i_name) return true;
		return false;
	}

	private static void BuildTiledNavNodes(Stage i_stage, NavMap i_navMap, List<Platform> i_platforms,
		StageDefinition i_definition)
	{
		Dictionary<string, NavNode> nodes = new Dictionary<string, NavNode>(System.StringComparer.Ordinal);
		foreach (TiledNavNodeDefinition definition in i_definition.Layout.TiledLevel.NavNodes)
		{
			NavNode node = i_navMap.CreateNode();
			node.gameObject.name = "mod-nav-node_" + definition.Point.Name;
			StagePointDefinition local = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			node.transform.position = i_stage.transform.TransformPoint(new Vector3(local.X, local.Y, 0f));
			node.SetIsFlyNode(definition.IsFly);
			if (!definition.IsFly)
			{
				// The stage clone is inactive while it is assembled, so Physics2D cannot
				// discover these platforms yet. Resolve the platform from its authored
				// geometry and connect the explicit node before the stage is enabled.
				Platform platform = FindPlatformBelow(node.transform.position, i_platforms, out float platformTop);
				if (platform != null)
				{
					node.SetPlatform(platform);
					node.transform.position = new Vector2(node.transform.position.x, platformTop + 0.25f);
				}
			}
			nodes.Add(definition.Point.Name, node);
		}
		foreach (NavNode node in nodes.Values)
			i_navMap.ConnectNodeToPlatformNetwork(node);
		foreach (TiledNavNodeDefinition definition in i_definition.Layout.TiledLevel.NavNodes)
			foreach (string target in definition.Links)
			{
				NodeConnectionType connectionType = definition.ConnectionType == "climb"
					? NodeConnectionType.Climb : NodeConnectionType.Move;
				nodes[definition.Point.Name].CreateNodeConnection(nodes[target], connectionType);
				if (definition.Bidirectional)
					nodes[target].CreateNodeConnection(nodes[definition.Point.Name], connectionType);
			}
	}

	private static Platform FindPlatformBelow(Vector2 i_position, IEnumerable<Platform> i_platforms, out float o_top)
	{
		Platform closest = null;
		o_top = float.NegativeInfinity;
		float closestDistance = float.PositiveInfinity;
		foreach (Platform platform in i_platforms)
		{
			if (platform == null) continue;
			BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
			if (collider == null) continue;
			Vector2 center = collider.transform.TransformPoint(collider.offset);
			Vector3 scale = collider.transform.lossyScale;
			float halfWidth = collider.size.x * Mathf.Abs(scale.x) * 0.5f;
			float halfHeight = collider.size.y * Mathf.Abs(scale.y) * 0.5f;
			float top = center.y + halfHeight;
			if (i_position.x < center.x - halfWidth - 0.05f || i_position.x > center.x + halfWidth + 0.05f) continue;
			float distance = i_position.y - top;
			if (distance < -0.1f || distance > 6f || distance >= closestDistance) continue;
			closest = platform;
			o_top = top;
			closestDistance = distance;
		}
		return closest;
	}

	private static void BuildTiledRooms(Stage i_stage, Transform i_parent, StageDefinition i_definition)
	{
		TiledLevelDefinition level = i_definition.Layout.TiledLevel;
		if (level.Rooms.Count == 0) return;
		ModRoomController controller = i_stage.gameObject.AddComponent<ModRoomController>();
		controller.Configure(i_stage);
		foreach (TiledRoomDefinition definition in level.Rooms)
		{
			StagePointDefinition center = level.ToUnityRectangleCenter(definition.Rectangle, i_definition.Layout.PixelsPerUnit);
			Vector2 size = new Vector2(definition.Rectangle.Width / i_definition.Layout.PixelsPerUnit,
				definition.Rectangle.Height / i_definition.Layout.PixelsPerUnit);
			controller.RegisterRoom(definition.Rectangle.Name, new Rect(center.X - size.x * 0.5f,
				center.Y - size.y * 0.5f, size.x, size.y), definition.Initial);
			GameObject roomObject = new GameObject("mod-room_" + definition.Rectangle.Name);
			roomObject.transform.SetParent(i_parent, false);
			roomObject.transform.localPosition = new Vector2(center.X, center.Y);
			BoxCollider2D trigger = roomObject.AddComponent<BoxCollider2D>();
			trigger.isTrigger = true;
			trigger.size = size;
			roomObject.AddComponent<ModRoomVolume>().Configure(controller, definition.Rectangle.Name);
		}
		foreach (TiledRoomEntryDefinition definition in level.RoomEntries)
		{
			StagePointDefinition point = level.ToUnityPoint(definition.Point, i_definition.Layout.PixelsPerUnit);
			controller.RegisterEntry(definition.Point.Name, definition.RoomId, new Vector2(point.X, point.Y));
		}
		foreach (TiledRoomTransitionDefinition definition in level.RoomTransitions)
		{
			StagePointDefinition center = level.ToUnityRectangleCenter(definition.Rectangle, i_definition.Layout.PixelsPerUnit);
			GameObject transitionObject = new GameObject("mod-room-transition_" + definition.Rectangle.Name);
			transitionObject.transform.SetParent(i_parent, false);
			transitionObject.transform.localPosition = new Vector2(center.X, center.Y);
			BoxCollider2D trigger = transitionObject.AddComponent<BoxCollider2D>();
			trigger.isTrigger = true;
			trigger.size = new Vector2(definition.Rectangle.Width / i_definition.Layout.PixelsPerUnit,
				definition.Rectangle.Height / i_definition.Layout.PixelsPerUnit);
			transitionObject.AddComponent<ModRoomTransition>().Configure(controller, definition.DestinationId,
				definition.RequireNoEnemies, definition.OneShot);
		}
	}

	private static void BuildTiledScriptTriggers(Stage i_stage, Transform i_parent, StageDefinition i_definition)
	{
		foreach (TiledLevelObject definition in i_definition.Layout.TiledLevel.ScriptTriggers)
		{
			StagePointDefinition center = i_definition.Layout.TiledLevel.ToUnityRectangleCenter(
				definition, i_definition.Layout.PixelsPerUnit);
			GameObject triggerObject = new GameObject("mod-script-trigger_" + definition.Name);
			triggerObject.transform.SetParent(i_parent, false);
			triggerObject.transform.localPosition = new Vector3(center.X, center.Y, 0f);
			BoxCollider2D collider = triggerObject.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
			collider.size = new Vector2(definition.Width / i_definition.Layout.PixelsPerUnit,
				definition.Height / i_definition.Layout.PixelsPerUnit);
			triggerObject.AddComponent<ModStageScriptTrigger>().Configure(definition.Name);
		}
	}

	private static void BuildTiledStageMarkers(Transform i_parent, StageDefinition i_definition)
	{
		foreach (TiledLevelObject definition in i_definition.Layout.TiledLevel.StageMarkers)
		{
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition, i_definition.Layout.PixelsPerUnit);
			GameObject marker = new GameObject("mod-stage-marker_" + definition.Name);
			marker.transform.SetParent(i_parent, false);
			marker.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			marker.AddComponent<ModStageMarker>().Configure(definition.Name);
		}
	}

	private static bool HasInteractableAncestor(Transform i_transform)
	{
		for (Transform current = i_transform; current != null; current = current.parent)
			if (current.GetComponent<Interactable>() != null) return true;
		return false;
	}

	private static bool HasSkeletonHierarchy(Transform i_transform)
	{
		for (Transform current = i_transform; current != null; current = current.parent)
			if (current.GetComponent<Skeleton>() != null || current.name.StartsWith("Skeleton", StringComparison.OrdinalIgnoreCase))
				return true;
		return false;
	}

	private static void BuildTiledVisuals(Transform i_layoutRoot, StageDefinition i_definition)
	{
		TiledLevelDefinition level = i_definition.Layout.TiledLevel;
		if (level.TileLayers.Count == 0 && level.ImageLayers.Count == 0
			&& level.Decorations.Count == 0 && level.CoreArt.Count == 0) return;
		Dictionary<TiledTilesetDefinition, Texture2D> textures = new Dictionary<TiledTilesetDefinition, Texture2D>();
		Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
		foreach (TiledTilesetDefinition tileset in level.Tilesets)
		{
			if (!RuntimePngAssetLoader.TryLoad(tileset.AssetRoot, tileset.ImagePath,
				i_definition.Id + "/tileset/" + tileset.Name, FilterMode.Point, ModLoaderRuntime.LastReport,
				"stage.factory-tileset-file", "stage.factory-tileset-decode", i_definition.Source, out Texture2D texture))
				continue;
			if (texture.width != tileset.ImageWidth || texture.height != tileset.ImageHeight)
			{
				Report("stage.factory-tileset-size", "Tileset image dimensions do not match its Tiled metadata: " + tileset.ImagePath, i_definition.Source);
				UnityEngine.Object.Destroy(texture);
				continue;
			}
			textures[tileset] = texture;
		}

		const uint gidMask = 0x1FFFFFFF;
		const uint horizontalFlip = 0x80000000;
		const uint verticalFlip = 0x40000000;
		foreach (TiledImageLayerDefinition layer in level.ImageLayers)
		{
			if (!RuntimePngAssetLoader.TryLoad(layer.AssetRoot, layer.ImagePath,
				i_definition.Id + "/image-layer/" + layer.Name, FilterMode.Point, ModLoaderRuntime.LastReport,
				"stage.factory-image-layer-file", "stage.factory-image-layer-decode", i_definition.Source,
				out Texture2D texture)) continue;
			Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f), i_definition.Layout.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			sprite.name = i_definition.Id + "/image-layer/" + layer.Name;
			float widthPixels = layer.RepeatX ? Mathf.Max(texture.width, level.WidthPixels - layer.X) : texture.width;
			float heightPixels = layer.RepeatY ? Mathf.Max(texture.height, level.HeightPixels - layer.Y) : texture.height;
			GameObject imageObject = new GameObject("image-layer_" + layer.Name);
			imageObject.transform.SetParent(i_layoutRoot, false);
			imageObject.transform.localPosition = new Vector3(
				(layer.X + widthPixels * 0.5f - level.WidthPixels * 0.5f) / i_definition.Layout.PixelsPerUnit,
				(level.HeightPixels - layer.Y - heightPixels * 0.5f) / i_definition.Layout.PixelsPerUnit, 0f);
			SpriteRenderer renderer = imageObject.AddComponent<SpriteRenderer>();
			ApplyTiledSpriteMaterial(renderer);
			renderer.sprite = sprite;
			renderer.sortingLayerName = "Background";
			renderer.sortingOrder = layer.DrawOrder * 1000;
			renderer.color = new Color(1f, 1f, 1f, layer.Opacity);
			if (layer.RepeatX || layer.RepeatY)
			{
				renderer.drawMode = SpriteDrawMode.Tiled;
				renderer.size = new Vector2(widthPixels / i_definition.Layout.PixelsPerUnit,
					heightPixels / i_definition.Layout.PixelsPerUnit);
			}
			if (Mathf.Abs(layer.ParallaxX - 1f) > 0.0001f || Mathf.Abs(layer.ParallaxY - 1f) > 0.0001f)
				imageObject.AddComponent<ModParallaxLayer>().Configure(layer.ParallaxX, layer.ParallaxY);
		}
		foreach (TiledTileLayerDefinition layer in level.TileLayers)
		{
			GameObject layerObject = new GameObject("tile-layer_" + layer.Name);
			layerObject.transform.SetParent(i_layoutRoot, false);
			for (int index = 0; index < layer.Data.Count; index++)
			{
				uint encodedGid = layer.Data[index];
				uint gid = encodedGid & gidMask;
				if (gid == 0) continue;
				if (!TryGetTileSprite(gid, level.Tilesets, textures, sprites, i_definition, out Sprite sprite,
					out TiledTilesetDefinition tileset)) continue;

				int columnIndex = index % layer.Width;
				int rowIndex = index / layer.Width;
				float pixelX = layer.X + columnIndex * level.TileWidth + tileset.TileWidth * 0.5f;
				float pixelY = layer.Y + rowIndex * level.TileHeight + level.TileHeight - tileset.TileHeight * 0.5f;
				GameObject tileObject = new GameObject("tile_" + columnIndex + "_" + rowIndex);
				tileObject.transform.SetParent(layerObject.transform, false);
				tileObject.transform.localPosition = new Vector3(
					(pixelX - level.WidthPixels * 0.5f) / i_definition.Layout.PixelsPerUnit,
					(level.HeightPixels - pixelY) / i_definition.Layout.PixelsPerUnit, 0f);
				SpriteRenderer renderer = tileObject.AddComponent<SpriteRenderer>();
				ApplyTiledSpriteMaterial(renderer);
				renderer.sprite = sprite;
				renderer.sortingLayerName = "Platform";
				renderer.flipX = (encodedGid & horizontalFlip) != 0;
				renderer.flipY = (encodedGid & verticalFlip) != 0;
				renderer.color = new Color(1f, 1f, 1f, layer.Opacity);
				renderer.sortingOrder = layer.DrawOrder * 1000;
				AttachTileAnimation(renderer, gid, tileset, level.Tilesets, textures, sprites, i_definition);
			}
		}

		GameObject decorationRoot = new GameObject("tile-decorations");
		decorationRoot.transform.SetParent(i_layoutRoot, false);
		foreach (TiledDecorationDefinition decoration in level.Decorations)
		{
			uint gid = decoration.EncodedGid & gidMask;
			if (!TryGetTileSprite(gid, level.Tilesets, textures, sprites, i_definition,
				out Sprite sprite, out TiledTilesetDefinition tileset)) continue;
			GameObject decorationObject = new GameObject("decoration_" + decoration.Id + "_" + decoration.Name);
			decorationObject.transform.SetParent(decorationRoot.transform, false);
			Vector2 pixelCenter = GetTileObjectCenter(decoration, tileset.ObjectAlignment);
			decorationObject.transform.localPosition = new Vector3(
				(pixelCenter.x - level.WidthPixels * 0.5f) / i_definition.Layout.PixelsPerUnit,
				(level.HeightPixels - pixelCenter.y) / i_definition.Layout.PixelsPerUnit, 0f);
			decorationObject.transform.localRotation = Quaternion.Euler(0f, 0f, -decoration.Rotation);
			SpriteRenderer renderer = decorationObject.AddComponent<SpriteRenderer>();
			ApplyTiledSpriteMaterial(renderer);
			renderer.sprite = sprite;
			renderer.sortingLayerName = decoration.SortingLayer;
			if (decoration.Repeat)
			{
				renderer.drawMode = SpriteDrawMode.Tiled;
				renderer.tileMode = decoration.AdaptiveTiling ? SpriteTileMode.Adaptive : SpriteTileMode.Continuous;
				renderer.adaptiveModeThreshold = decoration.AdaptiveModeThreshold;
				renderer.size = new Vector2(decoration.Width / i_definition.Layout.PixelsPerUnit,
					decoration.Height / i_definition.Layout.PixelsPerUnit);
			}
			else decorationObject.transform.localScale = new Vector3(
				decoration.Width / tileset.TileWidth, decoration.Height / tileset.TileHeight, 1f);
			renderer.flipX = (decoration.EncodedGid & horizontalFlip) != 0;
			renderer.flipY = (decoration.EncodedGid & verticalFlip) != 0;
			Color tint = Color.white;
			if (!string.IsNullOrWhiteSpace(decoration.TintColor)
				&& ColorUtility.TryParseHtmlString(decoration.TintColor, out Color parsedTint)) tint = parsedTint;
			tint.a *= decoration.Opacity;
			renderer.color = tint;
			renderer.sortingOrder = decoration.SortingOrder;
			AttachTileAnimation(renderer, gid, tileset, level.Tilesets, textures, sprites, i_definition);
		}

		GameObject coreArtRoot = new GameObject("core-map-art");
		coreArtRoot.transform.SetParent(i_layoutRoot, false);
		foreach (TiledCoreArtDefinition definition in level.CoreArt)
		{
			Sprite sprite = FindCoreMapArtSprite(definition.Art.SpriteName);
			if (sprite == null)
			{
				Report("stage.factory-core-art", "Core map-art sprite is unavailable: " + definition.Art.Id, i_definition.Source);
				continue;
			}
			GameObject artObject = new GameObject("core-art_" + definition.Rectangle.Id + "_" + definition.Art.Id);
			artObject.transform.SetParent(coreArtRoot.transform, false);
			StagePointDefinition center = level.ToUnityRectangleCenter(definition.Rectangle, i_definition.Layout.PixelsPerUnit);
			artObject.transform.localPosition = new Vector3(center.X, center.Y, 0f);
			artObject.transform.localRotation = Quaternion.Euler(0f, 0f, -definition.Rotation);
			float desiredWidth = definition.Rectangle.Width / i_definition.Layout.PixelsPerUnit;
			float desiredHeight = definition.Rectangle.Height / i_definition.Layout.PixelsPerUnit;
			float scaleX = desiredWidth / sprite.bounds.size.x * (definition.FlipX ? -1f : 1f);
			float scaleY = desiredHeight / sprite.bounds.size.y * (definition.FlipY ? -1f : 1f);
			artObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
			SpriteRenderer renderer = artObject.AddComponent<SpriteRenderer>();
			ApplyTiledSpriteMaterial(renderer);
			renderer.sprite = sprite;
			renderer.sortingLayerName = definition.SortingLayer;
			renderer.sortingOrder = definition.SortingOrder;
			renderer.color = new Color(1f, 1f, 1f, definition.Opacity);
		}
		}

	private static void ApplyTiledSpriteMaterial(SpriteRenderer i_renderer)
	{
		if (i_renderer == null) return;
		if (s_tiledSpriteMaterial == null)
			s_tiledSpriteMaterial = Resources.Load<Material>(TiledSpriteMaterialResource);
		if (s_tiledSpriteMaterial != null) i_renderer.sharedMaterial = s_tiledSpriteMaterial;
		else Debug.LogError("[ModLoader] Tiled sprite material is unavailable: Resources/"
			+ TiledSpriteMaterialResource + ".mat");
	}

	private static void SelectTiledSpriteMaterial(Stage i_stage)
	{
		s_tiledSpriteMaterial = null;
		if (i_stage != null)
			foreach (SpriteRenderer renderer in i_stage.GetComponentsInChildren<SpriteRenderer>(true))
			{
				Material material = renderer == null ? null : renderer.sharedMaterial;
				if (material == null || material.shader == null
					|| material.shader.name != "Universal Render Pipeline/2D/Sprite-Lit-Default") continue;
				s_tiledSpriteMaterial = material;
				break;
			}
		if (s_tiledSpriteMaterial == null)
			s_tiledSpriteMaterial = Resources.Load<Material>(TiledSpriteMaterialResource);
	}

	private static Vector2 GetTileObjectCenter(TiledDecorationDefinition i_decoration, string i_alignment)
	{
		float halfWidth = i_decoration.Width * 0.5f;
		float halfHeight = i_decoration.Height * 0.5f;
		switch (i_alignment)
		{
			case "topleft": return new Vector2(i_decoration.X + halfWidth, i_decoration.Y + halfHeight);
			case "top": return new Vector2(i_decoration.X, i_decoration.Y + halfHeight);
			case "topright": return new Vector2(i_decoration.X - halfWidth, i_decoration.Y + halfHeight);
			case "left": return new Vector2(i_decoration.X + halfWidth, i_decoration.Y);
			case "center": return new Vector2(i_decoration.X, i_decoration.Y);
			case "right": return new Vector2(i_decoration.X - halfWidth, i_decoration.Y);
			case "bottom": return new Vector2(i_decoration.X, i_decoration.Y - halfHeight);
			case "bottomright": return new Vector2(i_decoration.X - halfWidth, i_decoration.Y - halfHeight);
			default: return new Vector2(i_decoration.X + halfWidth, i_decoration.Y - halfHeight);
		}
	}

	private static void AttachTileAnimation(SpriteRenderer i_renderer, uint i_gid, TiledTilesetDefinition i_tileset,
		IReadOnlyList<TiledTilesetDefinition> i_tilesets, Dictionary<TiledTilesetDefinition, Texture2D> i_textures,
		Dictionary<string, Sprite> io_sprites, StageDefinition i_definition)
	{
		int localId = (int)(i_gid - i_tileset.FirstGid);
		if (!i_tileset.Animations.TryGetValue(localId, out IReadOnlyList<TiledAnimationFrameDefinition> animation)) return;
		List<Sprite> frames = new List<Sprite>();
		List<float> durations = new List<float>();
		foreach (TiledAnimationFrameDefinition frame in animation)
		{
			uint frameGid = i_tileset.FirstGid + (uint)frame.TileId;
			if (!TryGetTileSprite(frameGid, i_tilesets, i_textures, io_sprites, i_definition,
				out Sprite sprite, out TiledTilesetDefinition _)) return;
			frames.Add(sprite);
			durations.Add(frame.DurationMilliseconds / 1000f);
		}
		i_renderer.gameObject.AddComponent<ModAnimatedSprite>().Configure(i_renderer, frames, durations);
	}

	private static void BuildTiledParticles(Transform i_layoutRoot, StageDefinition i_definition)
	{
		foreach (TiledParticleDefinition definition in i_definition.Layout.TiledLevel.Particles)
		{
			GameObject particleObject = new GameObject("mod-particle-emitter_" + definition.Point.Id);
			particleObject.transform.SetParent(i_layoutRoot, false);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			particleObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
			ParticleSystem.MainModule main = particles.main;
			main.loop = true;
			main.playOnAwake = true;
			main.startLifetime = definition.Lifetime;
			main.startSpeed = definition.Speed;
			main.startSize = definition.Size;
			if (ColorUtility.TryParseHtmlString(definition.Color, out Color color)) main.startColor = color;
			ParticleSystem.EmissionModule emission = particles.emission;
			emission.rateOverTime = definition.Rate;
			ParticleSystem.ShapeModule shape = particles.shape;
			shape.shapeType = ParticleSystemShapeType.Circle;
			shape.radius = definition.Radius;
			ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
			renderer.sortingLayerName = "Decoration";
			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null) renderer.material = new Material(shader);
		}
	}

	private static void BuildTiledAudio(Transform i_layoutRoot, StageDefinition i_definition)
	{
		foreach (TiledAudioDefinition definition in i_definition.Layout.TiledLevel.AudioSources)
		{
			GameObject audioObject = new GameObject("mod-" + (definition.Ambient ? "ambient-audio_" : "audio-source_") + definition.Point.Name);
			audioObject.transform.SetParent(i_layoutRoot, false);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			audioObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			audioObject.AddComponent<ModMapAudioSource>().Configure(definition, i_definition.Source);
		}
	}

	private static Sprite FindCoreMapArtSprite(string i_spriteName)
	{
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (SpriteRenderer renderer in stage.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
				if (renderer.sprite != null && renderer.sprite.name == i_spriteName) return renderer.sprite;
		}
		foreach (Sprite sprite in Resources.FindObjectsOfTypeAll<Sprite>())
			if (sprite != null && sprite.name == i_spriteName) return sprite;
		return null;
	}

	private static void BuildTiledVendors(Stage i_stage, Transform i_layoutRoot, StageDefinition i_definition)
	{
		TiledLevelDefinition level = i_definition.Layout.TiledLevel;
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		Vendor weaponTemplate = library == null ? null : library.WeaponVendor;
		Vendor usableTemplate = library == null ? null : library.UsableVendor;
		foreach (Vendor inherited in i_stage.GetComponentsInChildren<Vendor>(includeInactive: true))
		{
			if (inherited.GetVendorType() == VendorType.Weapons && weaponTemplate == null) weaponTemplate = inherited;
			else if (inherited.GetVendorType() == VendorType.Usables && usableTemplate == null) usableTemplate = inherited;
			inherited.gameObject.SetActive(false);
		}
		BuildTiledVendorType(weaponTemplate, level.WeaponVendors, "weapon-vendor", VendorType.Weapons, i_layoutRoot, i_definition);
		BuildTiledVendorType(usableTemplate, level.UsableVendors, "usable-vendor", VendorType.Usables, i_layoutRoot, i_definition);
	}

	private static Sprite LoadModObjectSprite(StageDefinition i_stage, TiledLevelObject i_point,
		string i_file, string i_slot, float i_pivotX = 0.5f, float i_pivotY = 0.5f)
	{
		if (string.IsNullOrEmpty(i_file)) return null;
		if (!RuntimePngAssetLoader.TryLoad(i_stage.AssetRoot, i_file,
			i_stage.Id + "/object/" + i_point.Name + "/" + i_slot, FilterMode.Point,
			ModLoaderRuntime.LastReport, "stage.factory-object-visual-file", "stage.factory-object-visual-decode",
			i_stage.Source, out Texture2D texture)) return null;
		return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
			new Vector2(i_pivotX, i_pivotY), i_point.VisualPixelsPerUnit, 0, SpriteMeshType.FullRect);
	}

	private static Sprite ApplyModObjectVisual(GameObject i_object, TiledLevelObject i_point,
		StageDefinition i_stage, string i_slot, float i_pivotX = 0.5f, float i_pivotY = 0.5f)
	{
		Sprite sprite = LoadModObjectSprite(i_stage, i_point, i_point.VisualFile, i_slot, i_pivotX, i_pivotY);
		if (sprite == null) return null;
		SpriteRenderer renderer = i_object.GetComponent<SpriteRenderer>()
			?? i_object.GetComponentInChildren<SpriteRenderer>(true);
		if (renderer != null) renderer.sprite = sprite;
		return sprite;
	}

	private static void BuildTiledVendorType(Vendor i_template, IReadOnlyList<TiledLevelObject> i_points,
		string i_name, VendorType i_type, Transform i_parent, StageDefinition i_definition)
	{
		if (i_points.Count > 0 && i_template == null)
		{
			Report("stage.factory-vendor-template", "The inherited Core stage has no " + i_name + " template.", i_definition.Source);
			return;
		}
		foreach (TiledLevelObject point in i_points)
		{
			GameObject vendorObject = UnityEngine.Object.Instantiate(i_template.gameObject, i_parent);
			vendorObject.name = "mod-" + i_name + "_" + point.Id;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(point, i_definition.Layout.PixelsPerUnit);
			vendorObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			vendorObject.GetComponent<Vendor>().ConfigureModVendor(i_type);
			SpriteRenderer renderer = vendorObject.GetComponent<SpriteRenderer>()
				?? vendorObject.GetComponentInChildren<SpriteRenderer>(true);
			Sprite coreBaseline = renderer == null ? null : renderer.sprite;
			Sprite localVisual = ApplyModObjectVisual(vendorObject, point, i_definition, "vendor", 0.5f, 0f);
			if (i_type == VendorType.Usables)
				CoreAssetSlotBinder.BindUsableVendorRenderer(renderer, coreBaseline, localVisual);
			vendorObject.SetActive(true);
		}
	}

	private static void BuildTiledWeaponCases(Stage i_stage, Transform i_layoutRoot, StageDefinition i_definition)
	{
		if (i_definition.Layout.PreserveInheritedStageObjects
			&& i_definition.Layout.TiledLevel.WeaponCases.Count == 0) return;
		WeaponCase template = FindCoreWeaponCaseTemplate(i_stage);
		foreach (WeaponCase inherited in i_stage.GetComponentsInChildren<WeaponCase>(includeInactive: true))
		{
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		}
		if (i_definition.Layout.TiledLevel.WeaponCases.Count > 0 && template == null)
		{
			Report("stage.factory-weapon-case-template", "The inherited Core stage has no weapon-case template.", i_definition.Source);
			return;
		}
		foreach (TiledWeaponCaseDefinition definition in i_definition.Layout.TiledLevel.WeaponCases)
		{
			if (!ModLoaderRuntime.Registry.TryGet(definition.Weapon, out ContentRegistration entry) || !(entry.RuntimeAsset is Weapon weapon))
			{
				Report("stage.factory-weapon-case-item", "Weapon-case content is not built: " + definition.Weapon, i_definition.Source);
				continue;
			}
			GameObject caseObject = UnityEngine.Object.Instantiate(template.gameObject, i_layoutRoot);
			caseObject.name = "mod-weapon-case_" + definition.Point.Id;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			caseObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			WeaponCase weaponCase = caseObject.GetComponent<WeaponCase>();
			weaponCase.ConfigureModWeaponCase(weapon, definition.CaseSize);
			weaponCase.ConfigureModCaseVisuals(
				LoadModObjectSprite(i_definition, definition.Point, definition.Point.VisualFile, "case-intact"),
				LoadModObjectSprite(i_definition, definition.Point, definition.Point.BrokenVisualFile, "case-broken"));
			caseObject.SetActive(true);
		}
	}

	private static WeaponCase FindCoreWeaponCaseTemplate(Stage i_stage)
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.WeaponCase != null && library.WeaponCase.IsUsableModTemplate()) return library.WeaponCase;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage) || stage == i_stage) continue;
			WeaponCase candidate = stage.GetComponentInChildren<WeaponCase>(true);
			if (candidate != null && candidate.IsUsableModTemplate()) return candidate;
		}
		WeaponCase local = i_stage.GetComponentInChildren<WeaponCase>(true);
		return local != null && local.IsUsableModTemplate() ? local : null;
	}

	private static void BuildTiledPickups(Stage i_stage, StageDefinition i_definition)
	{
		foreach (PickUpable inherited in i_stage.GetComponentsInChildren<PickUpable>(true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)
				&& IsUnderNamedAncestor(inherited.transform, "Items", i_stage.transform)) inherited.gameObject.SetActive(false);
		foreach (TiledPickupDefinition definition in i_definition.Layout.TiledLevel.Pickups)
		{
			if (!ModLoaderRuntime.Registry.TryGet(definition.Item, out ContentRegistration entry)
				|| !(entry.RuntimeAsset is PickUpable template))
			{
				Report("stage.factory-pickup-item", "Pickup content is not built: " + definition.Item, i_definition.Source);
				continue;
			}
			if (!template.GetIsStackable() && definition.Amount != 1)
			{
				Report("stage.factory-pickup-amount", "Non-stackable pickup amount must be 1: " + definition.Item, i_definition.Source);
				continue;
			}
			PickUpable pickup = UnityEngine.Object.Instantiate(template);
			pickup.gameObject.name = "mod-pickup_" + definition.Point.Name;
			pickup.gameObject.SetActive(true);
			pickup.ConfigureModWorldPickup(definition.Amount);
			pickup.transform.SetParent(i_stage.GetItemsParent(), false);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			pickup.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			if (definition.InitiallyKinematic)
			{
				Rigidbody2D body = pickup.GetComponent<Rigidbody2D>();
				if (body != null)
				{
					body.bodyType = RigidbodyType2D.Kinematic;
					body.gravityScale = 0f;
				}
			}
			pickup.gameObject.SetActive(true);
		}
	}

	private static void BuildTiledStageItems(Stage i_stage, StageDefinition i_definition)
	{
		foreach (TiledStageItemDefinition definition in i_definition.Layout.TiledLevel.StageItems)
		{
			if (!RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.FilePath,
				i_definition.Id + "/stage-item/" + definition.Point.Name, FilterMode.Point,
				ModLoaderRuntime.LastReport, "stage.factory-stage-item-file", "stage.factory-stage-item-decode",
				i_definition.Source, out Texture2D texture)) continue;
			Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(definition.PivotX, definition.PivotY), definition.PixelsPerUnit, 0, SpriteMeshType.FullRect);
			sprite.name = i_definition.Id + "/stage-item/" + definition.Point.Name;
			GameObject itemObject = new GameObject("mod-stage-item_" + definition.Point.Name);
			itemObject.SetActive(false);
			itemObject.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
			itemObject.transform.SetParent(i_stage.GetItemsParent(), false);
			int itemLayer = LayerMask.NameToLayer("Item");
			if (itemLayer >= 0) itemObject.layer = itemLayer;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			itemObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			SpriteRenderer renderer = itemObject.AddComponent<SpriteRenderer>();
			ApplyTiledSpriteMaterial(renderer);
			renderer.sprite = sprite;
			renderer.sortingLayerName = "Item";
			Rigidbody2D body = itemObject.AddComponent<Rigidbody2D>();
			body.gravityScale = 1f;
			BoxCollider2D collider = itemObject.AddComponent<BoxCollider2D>();
			collider.size = new Vector2(definition.ColliderWidth, definition.ColliderHeight);
			collider.offset = new Vector2(definition.ColliderOffsetX, definition.ColliderOffsetY);
			PickUpable pickup = itemObject.AddComponent<PickUpable>();
			pickup.ConfigureModItem(definition.DisplayName, definition.Description);
			pickup.SetModItemIcon(sprite);
			pickup.ConfigureModEconomy(definition.Weight, definition.Value);
			pickup.ConfigureModPickup(definition.CanDrop);
			itemObject.AddComponent<ModStagePickupSignal>().Configure(definition.OnPickupSignal);
			if (!string.IsNullOrEmpty(definition.LightColor))
			{
				GameObject glowObject = new GameObject("mod-stage-item-light");
				glowObject.transform.SetParent(itemObject.transform, false);
				glowObject.transform.localPosition = new Vector3(definition.LightOffsetX, definition.LightOffsetY, 0f);
				UnityEngine.Rendering.Universal.Light2D glow = glowObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
				glow.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Parametric;
				ColorUtility.TryParseHtmlString(definition.LightColor, out Color glowColor);
				glow.color = glowColor;
				glow.intensity = definition.LightIntensity;
				glow.falloffIntensity = definition.LightFalloffIntensity;
				glow.shapeLightFalloffSize = definition.LightFalloffSize;
				System.Reflection.BindingFlags privateInstance = System.Reflection.BindingFlags.Instance
					| System.Reflection.BindingFlags.NonPublic;
				typeof(UnityEngine.Rendering.Universal.Light2D).GetField("m_ShapeLightParametricSides", privateInstance)
					?.SetValue(glow, definition.LightSides);
				typeof(UnityEngine.Rendering.Universal.Light2D).GetField("m_ShapeLightParametricRadius", privateInstance)
					?.SetValue(glow, definition.LightRadius);
				System.Reflection.FieldInfo sortingLayers = typeof(UnityEngine.Rendering.Universal.Light2D).GetField(
					"m_ApplyToSortingLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				if (sortingLayers != null) sortingLayers.SetValue(glow, new[] { SortingLayer.NameToID("Default"), SortingLayer.NameToID("Item") });
			}
			itemObject.SetActive(true);
			pickup.ConfigureModWorldPickup(1);
			if (definition.InitiallyKinematic)
			{
				// ConfigureModWorldPickup resets physics to dynamic. Authored shelf items
				// must keep their Tiled position until the player takes them.
				body.bodyType = RigidbodyType2D.Kinematic;
				body.gravityScale = 0f;
			}
		}
	}

	private static void BuildTiledProximityLights(Transform i_parent, StageDefinition i_definition)
	{
		foreach (TiledProximityLightDefinition definition in i_definition.Layout.TiledLevel.ProximityLights)
		{
			if (!RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.FilePath,
				i_definition.Id + "/proximity-light/" + definition.Point.Name, FilterMode.Point,
				ModLoaderRuntime.LastReport, "stage.factory-proximity-light-file", "stage.factory-proximity-light-decode",
				i_definition.Source, out Texture2D texture)) continue;
			GameObject fixture = new GameObject("mod-proximity-light_" + definition.Point.Name);
			fixture.SetActive(false);
			fixture.transform.SetParent(i_parent, false);
			// The cloned Core stage still contains an inactive fixture with the same name.
			// Stage scripts resolve markers first, so they must target the portable one.
			fixture.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(definition.Point, i_definition.Layout.PixelsPerUnit);
			fixture.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			SpriteRenderer renderer = fixture.AddComponent<SpriteRenderer>();
			ApplyTiledSpriteMaterial(renderer);
			renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
				new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
			renderer.sortingLayerName = definition.SortingLayer;
			renderer.sortingOrder = definition.SortingOrder;
			GameObject lightObject = new GameObject("Light");
			lightObject.transform.SetParent(fixture.transform, false);
			lightObject.transform.localPosition = new Vector3(0f, definition.LightOffsetY, 0f);
			lightObject.transform.localRotation = Quaternion.Euler(0f, 0f, definition.LightRotationZ);
			UnityEngine.Rendering.Universal.Light2D light = lightObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
			light.lightType = definition.LightType == "freeform"
				? UnityEngine.Rendering.Universal.Light2D.LightType.Freeform
				: UnityEngine.Rendering.Universal.Light2D.LightType.Point;
			ColorUtility.TryParseHtmlString(definition.Color, out Color color);
			light.color = color;
			light.intensity = 0f;
			light.falloffIntensity = definition.FalloffIntensity;
			light.pointLightInnerAngle = 170f;
			light.pointLightOuterAngle = 185f;
			light.pointLightInnerRadius = definition.InnerRadius;
			light.pointLightOuterRadius = definition.OuterRadius;
			if (definition.LightType == "freeform")
			{
				Vector3[] path = new Vector3[definition.ShapePath.Length / 2];
				for (int index = 0; index < path.Length; index++)
					path[index] = new Vector3(definition.ShapePath[index * 2], definition.ShapePath[index * 2 + 1], 0f);
				light.SetShapePath(path);
				light.shapeLightFalloffSize = definition.ShapeFalloffSize;
			}
			System.Reflection.FieldInfo sortingLayers = typeof(UnityEngine.Rendering.Universal.Light2D).GetField(
				"m_ApplyToSortingLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (sortingLayers != null) sortingLayers.SetValue(light, new[]
			{
				SortingLayer.NameToID("Background"), SortingLayer.NameToID("Decoration"),
				SortingLayer.NameToID("Platform"), SortingLayer.NameToID("Interactable"),
				SortingLayer.NameToID("Actor"), SortingLayer.NameToID("Player"),
				SortingLayer.NameToID("Projectile"), SortingLayer.NameToID("Item")
			});
			fixture.AddComponent<ModProximityLight>().Configure(light, definition.DetectionRadius,
				definition.FadeSeconds, definition.Intensity);
			fixture.SetActive(definition.InitiallyActive);
		}
	}

	private static void BuildTiledFreeformLights(Stage i_stage, Transform i_parent, StageDefinition i_definition)
	{
		IReadOnlyList<TiledFreeformLightDefinition> definitions = i_definition.Layout.TiledLevel.FreeformLights;
		if (definitions.Count == 0) return;
		if (!i_definition.Layout.PreserveInheritedStageObjects)
			foreach (TiledFreeformLightDefinition definition in definitions)
				if (!string.IsNullOrEmpty(definition.SuppressInheritedPath))
				{
					Transform original = FindCoreStageObject(i_stage.transform, definition.SuppressInheritedPath);
					if (original != null && original.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true) != null)
						original.gameObject.SetActive(false);
				}
		if (!i_definition.Layout.PreserveInheritedStageObjects)
			foreach (UnityEngine.Rendering.Universal.Light2D inherited in
				i_stage.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true))
				if (inherited.transform.parent != null && inherited.transform.parent.name == "AreaLights"
					&& definitions.Any(item => item.Point.Name == PortableLightName(inherited.name)))
					inherited.gameObject.SetActive(false);
		foreach (TiledFreeformLightDefinition definition in definitions)
		{
			Texture2D texture = null;
			if (!string.IsNullOrEmpty(definition.VisualFile) && !RuntimePngAssetLoader.TryLoad(
				definition.AssetRoot, definition.VisualFile,
				i_definition.Id + "/freeform-light/" + definition.Point.Name, FilterMode.Point,
				ModLoaderRuntime.LastReport, "stage.factory-freeform-light-file", "stage.factory-freeform-light-decode",
				i_definition.Source, out texture)) continue;
			GameObject fixture = new GameObject("mod-freeform-light_" + definition.Point.Name);
			fixture.SetActive(false);
			fixture.transform.SetParent(i_parent, false);
			fixture.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			fixture.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			if (texture != null)
			{
				SpriteRenderer renderer = fixture.AddComponent<SpriteRenderer>();
				ApplyTiledSpriteMaterial(renderer);
				renderer.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
					new Vector2(0.5f, 0.5f), definition.PixelsPerUnit, 0, SpriteMeshType.FullRect);
				renderer.sortingLayerName = definition.SortingLayer;
				renderer.sortingOrder = definition.SortingOrder;
			}
			GameObject lightObject = new GameObject("Light");
			lightObject.transform.SetParent(fixture.transform, false);
			lightObject.transform.localPosition = new Vector3(0f, definition.LightOffsetY, 0f);
			lightObject.transform.localRotation = Quaternion.Euler(0f, 0f, definition.LightRotationZ);
			UnityEngine.Rendering.Universal.Light2D light = lightObject.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
			light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Freeform;
			ColorUtility.TryParseHtmlString(definition.Color, out Color color);
			light.color = color;
			light.intensity = definition.Intensity;
			light.falloffIntensity = definition.FalloffIntensity;
			light.shapeLightFalloffSize = definition.ShapeFalloffSize;
			Vector3[] path = new Vector3[definition.ShapePath.Length / 2];
			for (int index = 0; index < path.Length; index++)
				path[index] = new Vector3(definition.ShapePath[index * 2], definition.ShapePath[index * 2 + 1], 0f);
			light.SetShapePath(path);
			System.Reflection.FieldInfo sortingLayers = typeof(UnityEngine.Rendering.Universal.Light2D).GetField(
				"m_ApplyToSortingLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (sortingLayers != null) sortingLayers.SetValue(light,
				definition.SortingLayers.Select(SortingLayer.NameToID).ToArray());
			fixture.SetActive(definition.InitiallyActive);
		}
	}

	private static string PortableLightName(string i_name)
	{
		System.Text.StringBuilder result = new System.Text.StringBuilder();
		bool dash = false;
		foreach (char character in (i_name ?? string.Empty).Trim().ToLowerInvariant())
		{
			if (char.IsLetterOrDigit(character)) { result.Append(character); dash = false; }
			else if (!dash && result.Length > 0) { result.Append('-'); dash = true; }
		}
		while (result.Length > 0 && result[result.Length - 1] == '-') result.Length--;
		return result.ToString();
	}

	private static void BuildTiledGlobalLight(Stage i_stage, Transform i_parent, StageDefinition i_definition)
	{
		if (i_definition.Layout.TiledLevel.GlobalLights.Count == 0) return;
		TiledGlobalLightDefinition definition = i_definition.Layout.TiledLevel.GlobalLights[0];
		UnityEngine.Rendering.Universal.Light2D inherited = i_stage.GetLightGlobal();
		GameObject fixture = new GameObject("mod-global-light_" + definition.Point.Name);
		fixture.SetActive(false);
		fixture.transform.SetParent(i_parent, false);
		fixture.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
		StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
			definition.Point, i_definition.Layout.PixelsPerUnit);
		fixture.transform.localPosition = new Vector3(position.X, position.Y, 0f);
		UnityEngine.Rendering.Universal.Light2D light = fixture.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
		light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
		ColorUtility.TryParseHtmlString(definition.Color, out Color color);
		light.color = color;
		light.intensity = definition.Intensity;
		light.falloffIntensity = definition.FalloffIntensity;
		System.Reflection.FieldInfo sortingLayers = typeof(UnityEngine.Rendering.Universal.Light2D).GetField(
			"m_ApplyToSortingLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
		if (sortingLayers != null) sortingLayers.SetValue(light,
			definition.SortingLayers.Select(SortingLayer.NameToID).ToArray());
		i_stage.ConfigureModGlobalLight(light);
		if (inherited != null)
			inherited.gameObject.SetActive(false);
		fixture.SetActive(true);
	}

	private static void BuildTiledPointLights(Stage i_stage, Transform i_parent, StageDefinition i_definition)
	{
		foreach (TiledPointLightDefinition definition in i_definition.Layout.TiledLevel.PointLights)
		{
			if (!i_definition.Layout.PreserveInheritedStageObjects && !string.IsNullOrEmpty(definition.SuppressInheritedPath))
			{
				Transform original = FindCoreStageObject(i_stage.transform, definition.SuppressInheritedPath);
				if (original != null && original.GetComponent<UnityEngine.Rendering.Universal.Light2D>() != null)
					original.gameObject.SetActive(false);
			}
			GameObject fixture = new GameObject("mod-point-light_" + definition.Point.Name);
			fixture.SetActive(false);
			fixture.transform.SetParent(i_parent, false);
			fixture.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			fixture.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			fixture.transform.localRotation = Quaternion.Euler(0f, 0f, definition.RotationZ);
			UnityEngine.Rendering.Universal.Light2D light = fixture.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
			light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
			ColorUtility.TryParseHtmlString(definition.Color, out Color color);
			light.color = color;
			light.intensity = definition.Intensity;
			light.falloffIntensity = definition.FalloffIntensity;
			light.pointLightInnerRadius = definition.InnerRadius;
			light.pointLightOuterRadius = definition.OuterRadius;
			light.pointLightInnerAngle = definition.InnerAngle;
			light.pointLightOuterAngle = definition.OuterAngle;
			light.overlapOperation = definition.OverlapOperation == "alpha-blend"
				? UnityEngine.Rendering.Universal.Light2D.OverlapOperation.AlphaBlend
				: UnityEngine.Rendering.Universal.Light2D.OverlapOperation.Additive;
			System.Reflection.FieldInfo sortingLayers = typeof(UnityEngine.Rendering.Universal.Light2D).GetField(
				"m_ApplyToSortingLayers", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (sortingLayers != null) sortingLayers.SetValue(light,
				definition.SortingLayers.Select(SortingLayer.NameToID).ToArray());
			if (definition.PulseSeconds > 0f)
				fixture.AddComponent<ModPulsingLight>().Configure(light, definition.PulseFrom,
					definition.PulseTo, definition.PulseSeconds);
			fixture.SetActive(definition.InitiallyActive);
		}
	}

	private static void BuildTiledDoors(Stage i_stage, Transform i_layoutRoot, StageDefinition i_definition)
	{
		TiledLevelDefinition level = i_definition.Layout.TiledLevel;
		if (i_definition.Layout.PreserveInheritedStageObjects && level.Doors.Count == 0 && level.DoorSwitches.Count == 0 && level.Notes.Count == 0
			&& level.Keypads.Count == 0 && level.Altars.Count == 0 && level.Lights.Count == 0) return;
		foreach (Door inherited in i_stage.GetComponentsInChildren<Door>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (DoorRoller inherited in i_stage.GetComponentsInChildren<DoorRoller>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (Switch inherited in i_stage.GetComponentsInChildren<Switch>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (Note inherited in i_stage.GetComponentsInChildren<Note>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (Keypad inherited in i_stage.GetComponentsInChildren<Keypad>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (Altar inherited in i_stage.GetComponentsInChildren<Altar>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		foreach (LightBulb inherited in i_stage.GetComponentsInChildren<LightBulb>(includeInactive: true))
			if (!HasRetainedCoreStageObjectAncestor(inherited.transform)) inherited.gameObject.SetActive(false);
		if (level.Lights.Count > 0)
			foreach (FuseBox inherited in i_stage.GetComponentsInChildren<FuseBox>(includeInactive: true))
				if (level.Interactions.Any(item => item.Area.Name == NormalizeTiledObjectId(inherited.name)
					&& item.TargetLightIds.Any(id => level.Lights.Any(light => light.Point.Name == id))))
					inherited.SuppressInheritedIndicatorForMod();

		Door standardTemplate = FindCoreDoorTemplate(i_jacky: false);
		Door jackyTemplate = FindCoreDoorTemplate(i_jacky: true);
		DoorRoller rollerTemplate = FindCoreRollerDoorTemplate();
		Switch switchTemplate = FindCoreSwitchTemplate();
		Dictionary<string, Door> doors = new Dictionary<string, Door>();
		Dictionary<string, DoorRoller> rollerDoors = new Dictionary<string, DoorRoller>();
		HashSet<string> switchControlled = new HashSet<string>();
		foreach (TiledDoorSwitchDefinition definition in i_definition.Layout.TiledLevel.DoorSwitches)
			foreach (string target in definition.TargetDoorIds) switchControlled.Add(target);

		foreach (TiledDoorDefinition definition in i_definition.Layout.TiledLevel.Doors)
		{
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			if (definition.DoorType == "roller")
			{
				if (rollerTemplate == null)
				{
					Report("stage.factory-door-template", "No Core roller-door template is available.", i_definition.Source);
					continue;
				}
				GameObject doorObject = UnityEngine.Object.Instantiate(rollerTemplate.gameObject, i_layoutRoot);
				doorObject.name = "mod-door_" + definition.Point.Name;
				doorObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
				DoorRoller door = doorObject.GetComponent<DoorRoller>();
				door.ConfigureModDoor(definition.ProximityRadius, !switchControlled.Contains(definition.Point.Name), definition.InitiallyOpen);
				if (!door.ConfigureModVisual(LoadModObjectSprite(i_definition, definition.Point,
					definition.Point.VisualFile, "roller-door")))
					Report("stage.factory-roller-door-visual", "Roller-door template has no reskinnable door sprite.", i_definition.Source);
				doorObject.SetActive(true);
				rollerDoors.Add(definition.Point.Name, door);
				continue;
			}

			Door template = definition.DoorType == "jacky" ? jackyTemplate : standardTemplate;
			if (template == null)
			{
				Report("stage.factory-door-template", "No Core " + definition.DoorType + " door template is available.", i_definition.Source);
				continue;
			}
			GameObject regularDoorObject = UnityEngine.Object.Instantiate(template.gameObject, i_layoutRoot);
			regularDoorObject.name = "mod-door_" + definition.Point.Name;
			regularDoorObject.AddComponent<ModStageMarker>().Configure(definition.Point.Name);
			regularDoorObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			Door regularDoor = regularDoorObject.GetComponent<Door>();
			PickUpable key = null;
			if (!string.IsNullOrEmpty(definition.RequiredItemId))
			{
				foreach (ModStageMarker marker in i_stage.GetComponentsInChildren<ModStageMarker>(true))
					if (marker.ObjectId == definition.RequiredItemId) { key = marker.GetComponent<PickUpable>(); break; }
				if (key == null) Report("stage.factory-door-key", "Door required item was not found: " + definition.RequiredItemId, i_definition.Source);
			}
			Sprite openSprite = definition.Point.OpenVisualFile.Length > 0
				? LoadModObjectSprite(i_definition, definition.Point, definition.Point.OpenVisualFile, "door-open")
				: string.IsNullOrEmpty(definition.OpenSprite) ? null : FindCoreMapArtSprite(definition.OpenSprite);
			Sprite closedSprite = definition.Point.ClosedVisualFile.Length > 0
				? LoadModObjectSprite(i_definition, definition.Point, definition.Point.ClosedVisualFile, "door-closed")
				: string.IsNullOrEmpty(definition.ClosedSprite) ? null : FindCoreMapArtSprite(definition.ClosedSprite);
			regularDoor.ConfigureModDoor(definition.InitiallyOpen, definition.Price, definition.SingleUse,
				definition.InitiallyInteractable, key, openSprite, closedSprite);
			regularDoorObject.SetActive(true);
			doors.Add(definition.Point.Name, regularDoor);
		}

		if (i_definition.Layout.TiledLevel.DoorSwitches.Count > 0 && switchTemplate == null)
		{
			Report("stage.factory-door-switch-template", "No Core switch template is available.", i_definition.Source);
			return;
		}
		foreach (TiledDoorSwitchDefinition definition in i_definition.Layout.TiledLevel.DoorSwitches)
		{
			List<Door> targets = new List<Door>();
			List<DoorRoller> rollerTargets = new List<DoorRoller>();
			foreach (string target in definition.TargetDoorIds)
			{
				if (doors.TryGetValue(target, out Door door)) targets.Add(door);
				else if (rollerDoors.TryGetValue(target, out DoorRoller rollerDoor)) rollerTargets.Add(rollerDoor);
			}
			GameObject switchObject = UnityEngine.Object.Instantiate(switchTemplate.gameObject, i_layoutRoot);
			switchObject.name = "mod-door-switch_" + definition.Point.Name;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			switchObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			Switch doorSwitch = switchObject.GetComponent<Switch>();
			doorSwitch.ConfigureModDoorTargets(targets, rollerTargets);
			doorSwitch.ConfigureModVisuals(
				LoadModObjectSprite(i_definition, definition.Point, definition.Point.OnVisualFile, "switch-on"),
				LoadModObjectSprite(i_definition, definition.Point, definition.Point.OffVisualFile, "switch-off"));
			switchObject.SetActive(true);
		}

		Note noteTemplate = FindCoreNoteTemplate();
		if (i_definition.Layout.TiledLevel.Notes.Count > 0 && noteTemplate == null)
			Report("stage.factory-note-template", "No Core note template is available.", i_definition.Source);
		else foreach (TiledNoteDefinition definition in i_definition.Layout.TiledLevel.Notes)
		{
			GameObject noteObject = UnityEngine.Object.Instantiate(noteTemplate.gameObject, i_layoutRoot);
			noteObject.name = "mod-note_" + definition.Point.Name;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			noteObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			noteObject.GetComponent<Note>().ConfigureModNote(definition.Text, definition.FontSize);
			noteObject.SetActive(true);
		}

		Keypad keypadTemplate = FindCoreKeypadTemplate();
		if (i_definition.Layout.TiledLevel.Keypads.Count > 0 && keypadTemplate == null)
			Report("stage.factory-keypad-template", "No Core keypad template is available.", i_definition.Source);
		else foreach (TiledKeypadDefinition definition in i_definition.Layout.TiledLevel.Keypads)
		{
			List<Door> targets = new List<Door>();
			List<DoorRoller> rollerTargets = new List<DoorRoller>();
			foreach (string target in definition.TargetDoorIds)
			{
				if (doors.TryGetValue(target, out Door door)) targets.Add(door);
				else if (rollerDoors.TryGetValue(target, out DoorRoller rollerDoor)) rollerTargets.Add(rollerDoor);
			}
			GameObject keypadObject = UnityEngine.Object.Instantiate(keypadTemplate.gameObject, i_layoutRoot);
			keypadObject.name = "mod-keypad_" + definition.Point.Name;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			keypadObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			keypadObject.GetComponent<Keypad>().ConfigureModKeypad(definition.Code, targets, rollerTargets);
			keypadObject.SetActive(true);
		}

		Altar altarTemplate = FindCoreAltarTemplate();
		if (i_definition.Layout.TiledLevel.Altars.Count > 0 && altarTemplate == null)
			Report("stage.factory-altar-template", "No complete Core altar template is available.", i_definition.Source);
		else foreach (TiledLevelObject definition in i_definition.Layout.TiledLevel.Altars)
		{
			GameObject altarObject = UnityEngine.Object.Instantiate(altarTemplate.gameObject, i_layoutRoot);
			altarObject.name = "mod-altar_" + definition.Name;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition, i_definition.Layout.PixelsPerUnit);
			altarObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			altarObject.GetComponent<Altar>().ConfigureModAltar(i_stage.GetLightGlobal());
			altarObject.SetActive(true);
		}

		LightBulb lightTemplate = FindCoreLightTemplate();
		Dictionary<string, LightBulb> lights = new Dictionary<string, LightBulb>(System.StringComparer.Ordinal);
		if (i_definition.Layout.TiledLevel.Lights.Count > 0 && lightTemplate == null)
			Report("stage.factory-light-template", "No complete Core light-bulb template is available.", i_definition.Source);
		else foreach (TiledLightDefinition definition in i_definition.Layout.TiledLevel.Lights)
		{
			GameObject lightObject = UnityEngine.Object.Instantiate(lightTemplate.gameObject, i_layoutRoot);
			lightObject.name = "mod-light-bulb_" + definition.Point.Name;
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			lightObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			LightBulb light = lightObject.GetComponent<LightBulb>();
			light.ConfigureModLight(definition.InitiallyOn, definition.Flicker, definition.Interactive);
			if (definition.VisualFile.Length > 0)
			{
				bool loadedInitial = RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.VisualFile,
					i_definition.Id + "/light/" + definition.Point.Name + "/initial", FilterMode.Point,
					ModLoaderRuntime.LastReport, "stage.factory-light-visual-file", "stage.factory-light-visual-decode",
					i_definition.Source, out Texture2D initialTexture);
				bool loadedActivated = RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.ActivatedVisualFile,
					i_definition.Id + "/light/" + definition.Point.Name + "/activated", FilterMode.Point,
					ModLoaderRuntime.LastReport, "stage.factory-light-visual-file", "stage.factory-light-visual-decode",
					i_definition.Source, out Texture2D activatedTexture);
				if (loadedInitial && loadedActivated)
				{
					Vector2 pivot = new Vector2(definition.PivotX, definition.PivotY);
					Sprite initial = Sprite.Create(initialTexture, new Rect(0f, 0f, initialTexture.width, initialTexture.height),
						pivot, definition.PixelsPerUnit, 0, SpriteMeshType.FullRect);
					Sprite activated = Sprite.Create(activatedTexture, new Rect(0f, 0f, activatedTexture.width, activatedTexture.height),
						pivot, definition.PixelsPerUnit, 0, SpriteMeshType.FullRect);
					ColorUtility.TryParseHtmlString(definition.InitialColor, out Color initialColor);
					ColorUtility.TryParseHtmlString(definition.ActivatedColor, out Color activatedColor);
					light.ConfigureModPresentation(initial, activated, initialColor, activatedColor,
						definition.InnerRadius, definition.OuterRadius, definition.FalloffIntensity,
						definition.SortingLayer, definition.SortingOrder);
				}
				else
				{
					if (loadedInitial) UnityEngine.Object.Destroy(initialTexture);
					if (loadedActivated) UnityEngine.Object.Destroy(activatedTexture);
				}
			}
			lightObject.SetActive(true);
			lights.Add(definition.Point.Name, light);
		}

		Dictionary<string, ModMapInteraction> interactions = new Dictionary<string, ModMapInteraction>(System.StringComparer.Ordinal);
		Dictionary<string, TiledInteractionDefinition> interactionDefinitions = new Dictionary<string, TiledInteractionDefinition>(System.StringComparer.Ordinal);
		foreach (TiledInteractionDefinition definition in i_definition.Layout.TiledLevel.Interactions)
		{
			GameObject interactionObject = new GameObject("mod-interaction_" + definition.Area.Name);
			interactionObject.transform.SetParent(i_layoutRoot, false);
			if (definition.Trigger == "use")
			{
				StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
					definition.Area, i_definition.Layout.PixelsPerUnit);
				interactionObject.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			}
			else
			{
				StagePointDefinition center = i_definition.Layout.TiledLevel.ToUnityRectangleCenter(
					definition.Area, i_definition.Layout.PixelsPerUnit);
				interactionObject.transform.localPosition = new Vector3(center.X, center.Y, 0f);
				BoxCollider2D collider = interactionObject.AddComponent<BoxCollider2D>();
				collider.size = new Vector2(definition.Area.Width / i_definition.Layout.PixelsPerUnit,
					definition.Area.Height / i_definition.Layout.PixelsPerUnit);
				collider.isTrigger = true;
			}
			ModMapInteraction interaction = interactionObject.AddComponent<ModMapInteraction>();
			if (definition.VisualFile.Length > 0)
			{
				bool loadedNormal = RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.VisualFile,
					i_definition.Id + "/interaction/" + definition.Area.Name + "/normal", FilterMode.Point,
					ModLoaderRuntime.LastReport, "stage.factory-interaction-visual-file", "stage.factory-interaction-visual-decode",
					i_definition.Source, out Texture2D normalTexture);
				bool loadedActivated = RuntimePngAssetLoader.TryLoad(definition.AssetRoot, definition.ActivatedVisualFile,
					i_definition.Id + "/interaction/" + definition.Area.Name + "/activated", FilterMode.Point,
					ModLoaderRuntime.LastReport, "stage.factory-interaction-visual-file", "stage.factory-interaction-visual-decode",
					i_definition.Source, out Texture2D activatedTexture);
				if (loadedNormal && loadedActivated)
				{
					Vector2 pivot = new Vector2(definition.VisualPivotX, definition.VisualPivotY);
					Sprite normal = Sprite.Create(normalTexture, new Rect(0f, 0f, normalTexture.width, normalTexture.height),
						pivot, definition.VisualPixelsPerUnit, 0, SpriteMeshType.FullRect);
					Sprite activated = Sprite.Create(activatedTexture, new Rect(0f, 0f, activatedTexture.width, activatedTexture.height),
						pivot, definition.VisualPixelsPerUnit, 0, SpriteMeshType.FullRect);
					GameObject visual = new GameObject("visual_portable");
					visual.transform.SetParent(interactionObject.transform, false);
					visual.transform.localScale = Vector3.one * definition.VisualScale;
					SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
					ApplyTiledSpriteMaterial(renderer);
					renderer.sprite = normal;
					renderer.sortingLayerName = definition.VisualSortingLayer;
					renderer.sortingOrder = definition.VisualSortingOrder;
					visual.AddComponent<ModMapInteractionVisual>().Configure(interaction, renderer, normal, activated);
				}
				else
				{
					if (loadedNormal) UnityEngine.Object.Destroy(normalTexture);
					if (loadedActivated) UnityEngine.Object.Destroy(activatedTexture);
				}
			}
			else if (definition.VisualArt != null)
			{
				Sprite sprite = FindCoreMapArtSprite(definition.VisualArt.SpriteName);
				if (sprite == null) Report("stage.factory-interaction-art", "Interaction visual is unavailable: " + definition.VisualArt.Id, i_definition.Source);
				else
				{
					GameObject visual = new GameObject("visual_" + definition.VisualArt.Id);
					visual.transform.SetParent(interactionObject.transform, false);
					visual.transform.localScale = Vector3.one * definition.VisualScale;
					SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
					ApplyTiledSpriteMaterial(renderer);
					renderer.sprite = sprite;
					renderer.sortingLayerName = "Decoration";
				}
			}
			interactions.Add(definition.Area.Name, interaction);
			interactionDefinitions.Add(definition.Area.Name, definition);
		}
		foreach (KeyValuePair<string, ModMapInteraction> pair in interactions)
		{
			TiledInteractionDefinition definition = interactionDefinitions[pair.Key];
			List<Door> targets = new List<Door>();
			List<DoorRoller> rollerTargets = new List<DoorRoller>();
			List<LightBulb> lightTargets = new List<LightBulb>();
			List<ModMapInteraction> chainTargets = new List<ModMapInteraction>();
			List<ModMapInteraction> requiredInteractions = new List<ModMapInteraction>();
			foreach (string target in definition.TargetDoorIds)
			{
				if (doors.TryGetValue(target, out Door door)) targets.Add(door);
				else if (rollerDoors.TryGetValue(target, out DoorRoller rollerDoor)) rollerTargets.Add(rollerDoor);
			}
			foreach (string target in definition.TargetLightIds)
				if (lights.TryGetValue(target, out LightBulb light)) lightTargets.Add(light);
			foreach (string target in definition.TargetInteractionIds)
				if (interactions.TryGetValue(target, out ModMapInteraction interaction)) chainTargets.Add(interaction);
			foreach (string target in definition.RequiredInteractionIds)
				if (interactions.TryGetValue(target, out ModMapInteraction interaction)) requiredInteractions.Add(interaction);
			pair.Value.Configure(definition, targets, rollerTargets, lightTargets, chainTargets, requiredInteractions, i_stage);
		}
	}

	private static void BuildTiledScriptedActors(Stage i_stage, StageDefinition i_definition)
	{
		NPC[] inheritedActors = i_stage.GetComponentsInChildren<NPC>(true);
		foreach (TiledScriptedActorDefinition definition in i_definition.Layout.TiledLevel.ScriptedActors)
		{
			if (!ModLoaderRuntime.Registry.TryGet(definition.Enemy, out ContentRegistration entry)
				|| !(entry.RuntimeAsset is NPC template))
			{
				Report("stage.factory-scripted-actor", "Scripted actor enemy is not built: " + definition.Enemy, i_definition.Source);
				continue;
			}
			NPC actor = null;
			foreach (NPC candidate in inheritedActors)
				if (candidate != null && NormalizeTiledObjectId(candidate.gameObject.name) == definition.Point.Name)
				{
					actor = candidate;
					break;
				}
			if (actor == null) actor = UnityEngine.Object.Instantiate(template, i_stage.GetActorsParent());
			// An inherited scripted actor may sit under a presentation group that the
			// portable layout disables. Keep all portable actors under the live actor root.
			actor.transform.SetParent(i_stage.GetActorsParent(), false);
			actor.gameObject.name = "mod-scripted-actor_" + definition.Point.Name;
			ModStageMarker actorMarker = actor.GetComponent<ModStageMarker>();
			if (actorMarker == null) actorMarker = actor.gameObject.AddComponent<ModStageMarker>();
			actorMarker.Configure(definition.Point.Name);
			RuntimeContentIdentity identity = actor.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = actor.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Enemy, ContentCategory.Enemy);
			StagePointDefinition position = i_definition.Layout.TiledLevel.ToUnityPoint(
				definition.Point, i_definition.Layout.PixelsPerUnit);
			actor.transform.localPosition = new Vector3(position.X, position.Y, 0f);
			actor.gameObject.SetActive(definition.InitiallyActive);
		}
	}

	private static string NormalizeTiledObjectId(string i_name)
	{
		string value = (i_name ?? "object").Trim().ToLowerInvariant();
		System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length);
		bool dash = false;
		foreach (char character in value)
		{
			if (char.IsLetterOrDigit(character)) { result.Append(character); dash = false; }
			else if (!dash && result.Length > 0) { result.Append('-'); dash = true; }
		}
		while (result.Length > 0 && result[result.Length - 1] == '-') result.Length--;
		if (result.Length == 0 || !char.IsLetter(result[0])) result.Insert(0, "object-");
		return result.ToString();
	}

	private static Door FindCoreDoorTemplate(bool i_jacky)
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		Door shared = library == null ? null : (i_jacky ? (Door)library.JackyDoor : library.StandardDoor);
		if (shared != null && shared.IsUsableModTemplate()) return shared;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (Door door in stage.GetComponentsInChildren<Door>(includeInactive: true))
				if ((i_jacky ? door is JackyDoor : door.GetType() == typeof(Door)) && door.IsUsableModTemplate()) return door;
		}
		return null;
	}

	private static DoorRoller FindCoreRollerDoorTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.RollerDoor != null && library.RollerDoor.IsUsableModTemplate()) return library.RollerDoor;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (DoorRoller door in stage.GetComponentsInChildren<DoorRoller>(includeInactive: true))
				if (door.IsUsableModTemplate()) return door;
		}
		return null;
	}

	private static Switch FindCoreSwitchTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.Switch != null && library.Switch.IsUsableModTemplate()) return library.Switch;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (Switch doorSwitch in stage.GetComponentsInChildren<Switch>(includeInactive: true))
				if (doorSwitch.GetType() == typeof(Switch) && doorSwitch.IsUsableModTemplate()) return doorSwitch;
		}
		return null;
	}

	private static Note FindCoreNoteTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.Note != null && library.Note.IsUsableModTemplate()) return library.Note;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (Note note in stage.GetComponentsInChildren<Note>(includeInactive: true))
				if (note.IsUsableModTemplate()) return note;
		}
		return null;
	}

	private static Keypad FindCoreKeypadTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.Keypad != null && library.Keypad.IsUsableModTemplate()) return library.Keypad;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (Keypad keypad in stage.GetComponentsInChildren<Keypad>(includeInactive: true))
				if (keypad.IsUsableModTemplate()) return keypad;
		}
		return null;
	}

	private static Altar FindCoreAltarTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.Altar != null && library.Altar.IsUsableModTemplate()) return library.Altar;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (Altar altar in stage.GetComponentsInChildren<Altar>(includeInactive: true))
				if (altar.IsUsableModTemplate()) return altar;
		}
		return null;
	}

	private static LightBulb FindCoreLightTemplate()
	{
		ModStageTemplateLibrary library = ModStageTemplateLibrary.Load();
		if (library != null && library.LightBulb != null && library.LightBulb.IsUsableModTemplate()) return library.LightBulb;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Stage))
		{
			if (entry.PackId != "core" || !(entry.RuntimeAsset is Stage stage)) continue;
			foreach (LightBulb light in stage.GetComponentsInChildren<LightBulb>(includeInactive: true))
				if (light.GetType() == typeof(LightBulb) && light.IsUsableModTemplate()) return light;
		}
		return null;
	}

	private static bool TryGetTileSprite(uint i_gid, IReadOnlyList<TiledTilesetDefinition> i_tilesets,
		Dictionary<TiledTilesetDefinition, Texture2D> i_textures, Dictionary<string, Sprite> io_sprites,
		StageDefinition i_definition, out Sprite o_sprite, out TiledTilesetDefinition o_tileset)
	{
		o_sprite = null;
		o_tileset = FindTileset(i_tilesets, i_gid);
		if (o_tileset == null || !i_textures.TryGetValue(o_tileset, out Texture2D texture)) return false;
		uint localId = i_gid - o_tileset.FirstGid;
		string spriteKey = o_tileset.FirstGid + ":" + localId;
		if (io_sprites.TryGetValue(spriteKey, out o_sprite)) return true;
		int column = (int)localId % o_tileset.Columns;
		int row = (int)localId / o_tileset.Columns;
		int sourceX = o_tileset.Margin + column * (o_tileset.TileWidth + o_tileset.Spacing);
		int sourceY = texture.height - o_tileset.Margin - (row + 1) * o_tileset.TileHeight - row * o_tileset.Spacing;
		if (sourceX < 0 || sourceY < 0 || sourceX + o_tileset.TileWidth > texture.width || sourceY + o_tileset.TileHeight > texture.height)
		{
			Report("stage.factory-tile-rect", "Tile gid " + i_gid + " falls outside tileset image " + o_tileset.ImagePath + ".", i_definition.Source);
			return false;
		}
		o_sprite = Sprite.Create(texture, new Rect(sourceX, sourceY, o_tileset.TileWidth, o_tileset.TileHeight),
			new Vector2(0.5f, 0.5f), i_definition.Layout.PixelsPerUnit, 0, SpriteMeshType.FullRect,
			new Vector4(o_tileset.BorderLeft, o_tileset.BorderBottom, o_tileset.BorderRight, o_tileset.BorderTop));
		o_sprite.name = i_definition.Id + "/tile/" + i_gid;
		io_sprites[spriteKey] = o_sprite;
		return true;
	}

	private static TiledTilesetDefinition FindTileset(IReadOnlyList<TiledTilesetDefinition> i_tilesets, uint i_gid)
	{
		for (int index = i_tilesets.Count - 1; index >= 0; index--)
			if (i_gid >= i_tilesets[index].FirstGid) return i_tilesets[index];
		return null;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}

public sealed class ModStageScriptRunner : MonoBehaviour
{
	private sealed class StageScriptExecutionBudget
	{
		public int Remaining = 4096;
	}

	private readonly List<StageScriptDefinition> m_scripts = new List<StageScriptDefinition>();
	private readonly Dictionary<string, float> m_variables = new Dictionary<string, float>(StringComparer.Ordinal);
	private readonly HashSet<string> m_completedOnce = new HashSet<string>(StringComparer.Ordinal);
	private readonly HashSet<string> m_running = new HashSet<string>(StringComparer.Ordinal);
	private readonly Dictionary<string, Coroutine> m_runningCoroutines = new Dictionary<string, Coroutine>(StringComparer.Ordinal);
	private readonly Dictionary<string, int> m_signalVersions = new Dictionary<string, int>(StringComparer.Ordinal);
	private readonly Dictionary<string, bool> m_worldTriggerStates = new Dictionary<string, bool>(StringComparer.Ordinal);
	private readonly Dictionary<string, float> m_timerTriggerTimes = new Dictionary<string, float>(StringComparer.Ordinal);
	private readonly Dictionary<string, string> m_machineStates = new Dictionary<string, string>(StringComparer.Ordinal);
	private ManagerWave m_wave;
	private bool m_changedPlayerInput;
	private bool m_changedHudVisibility;
	private RuntimeAnimatorController m_originalPlayerAnimatorController;

	public void Configure(IEnumerable<StageScriptDefinition> i_scripts)
	{
		m_scripts.Clear();
		foreach (StageScriptDefinition script in i_scripts ?? new StageScriptDefinition[0])
			m_scripts.Add(script);
		ResetRunState();
	}

	public void RestoreCloneConfiguration(ModStageScriptRunner i_source)
	{
		Configure(i_source.m_scripts);
	}

	private void OnEnable()
	{
		RuntimeContentIdentity identity = GetComponent<RuntimeContentIdentity>();
		ContentId stageId = default;
		bool hasStageId = identity != null && identity.TryGetContentId(out stageId);
		if (m_scripts.Count == 0)
		{
			if (hasStageId)
				Configure(ModLoaderRuntime.StageScriptDefinitions.Where(script => script.Stage == stageId));
		}
		if (hasStageId && m_scripts.Count == 0
			&& ModLoaderRuntime.StageScriptDefinitions.Any(script => script.Stage == stageId))
			Debug.LogWarning("[ModLoader] External stage opened without stage scripts: " + gameObject.name);
		if (hasStageId && stageId.Path == "stage/fer-tiled-port")
			Debug.Log("[ModLoader] FER stage script runner loaded " + m_scripts.Count + " scripts on " + gameObject.name + ".");
		ResetRunState();
		m_wave = GetComponentInChildren<ManagerWave>(true);
		if (m_wave != null) { m_wave.OnWaveStart += HandleWaveStart; m_wave.OnWaveEnd += HandleWaveEnd; }
		Interactable.OnAnyActivated += HandleInteraction;
		StartCoroutine(BeginStage());
		StartCoroutine(PollWorldTriggers());
	}

	private IEnumerator BeginStage()
	{
		// Allow Stage.OpenStage and its HUD references to initialize before the first sequence.
		yield return null;
		SuppressReplacedBuiltInStageBehaviors();
		foreach (StageScriptDefinition script in m_scripts)
			foreach (StageScriptMachineDocument machine in script.Machines)
				yield return SetMachineState(script, machine.Id, machine.InitialState, 0, new StageScriptExecutionBudget());
		Trigger("stage-open", null);
	}

	private void SuppressReplacedBuiltInStageBehaviors()
	{
		foreach (StageScriptDefinition script in m_scripts)
		{
			if (script.Id.Path == "stage-script/fer-laboratory")
			{
				StageFER fer = GetComponent<StageFER>();
				if (fer != null) fer.SetUseBuiltInLabSequence(false);
			}
			if (script.Id.Path == "stage-script/fer-statues")
				foreach (AndroidStatue statue in GetComponentsInChildren<AndroidStatue>(true))
					statue.SetUseBuiltInSequence(false);
			foreach (StageScriptSequenceDocument sequence in script.Sequences)
			{
				if (sequence.Trigger != "door-state" || sequence.State != "open") continue;
				bool replacesJackyCurse = false;
				foreach (StageScriptActionDocument action in sequence.Actions)
					if (action.Type == "apply-player-status" && action.Status == "jacky-curse") { replacesJackyCurse = true; break; }
				if (!replacesJackyCurse) continue;
				Transform target = FindObject(transform, sequence.ObjectId);
				JackyDoor door = target == null ? null : target.GetComponent<JackyDoor>();
				if (door != null) door.SetUseBuiltInCurse(false);
			}
		}
	}

	private void OnDisable()
	{
		if (m_wave != null) { m_wave.OnWaveStart -= HandleWaveStart; m_wave.OnWaveEnd -= HandleWaveEnd; }
		Interactable.OnAnyActivated -= HandleInteraction;
		StopAllCoroutines(); m_running.Clear(); m_runningCoroutines.Clear();
		RestoreTransientPlayerState();
	}

	private void ResetRunState()
	{
		m_variables.Clear(); m_completedOnce.Clear(); m_running.Clear(); m_runningCoroutines.Clear(); m_signalVersions.Clear(); m_worldTriggerStates.Clear(); m_timerTriggerTimes.Clear(); m_machineStates.Clear();
		m_changedPlayerInput = false; m_changedHudVisibility = false; m_originalPlayerAnimatorController = null;
		foreach (StageScriptDefinition script in m_scripts)
			foreach (KeyValuePair<string, float> variable in script.Variables)
				m_variables[VariableKey(script, variable.Key)] = variable.Value;
	}

	private void HandleWaveStart() => Trigger("wave-start", null);
	private void HandleWaveEnd() => Trigger("wave-end", null);
	private void HandleInteraction(Interactable i_interaction, Actor i_initiator, InteractableActivationType i_type)
	{
		if (i_interaction == null || !i_interaction.transform.IsChildOf(transform)) return;
		if (i_interaction.GetComponent<ModStageActivationBridge>()?.isActiveAndEnabled == true) return;
		ModStageMarker marker = i_interaction.GetComponent<ModStageMarker>();
		ModRetainedCoreStageObject retained = i_interaction.GetComponent<ModRetainedCoreStageObject>();
		string id = marker != null && !string.IsNullOrEmpty(marker.ObjectId)
			? marker.ObjectId
			: retained != null && !string.IsNullOrEmpty(retained.ObjectId)
			? retained.ObjectId
			: InteractionObjectId(i_interaction.gameObject.name);
		if (m_scripts.Any(script => script.Stage.Path == "stage/fer-tiled-port"))
			Debug.Log("[ModLoader] FER interaction received: " + ScriptObjectId(id));
		Trigger("interaction", ScriptObjectId(id));
	}

	public void NotifyInteraction(string i_objectId)
	{
		if (m_scripts.Any(script => script.Stage.Path == "stage/fer-tiled-port"))
			Debug.Log("[ModLoader] FER interaction received: " + ScriptObjectId(i_objectId));
		Trigger("interaction", ScriptObjectId(i_objectId));
	}

	internal static string InteractionObjectId(string i_name)
	{
		foreach (string prefix in new[] { "mod-interaction_", "mod-door_", "mod-light-bulb_" })
			if (i_name.StartsWith(prefix, StringComparison.Ordinal)) return i_name.Substring(prefix.Length);
		return i_name;
	}

	private IEnumerator PollWorldTriggers()
	{
		while (true)
		{
			foreach (StageScriptDefinition script in m_scripts)
				foreach (StageScriptSequenceDocument sequence in script.Sequences)
				{
					if (sequence.Trigger == "timer") PollTimerTrigger(script, sequence);
					else if (sequence.Trigger == "door-state" || sequence.Trigger == "object-state" || sequence.Trigger == "enemy-count")
						PollEdgeTrigger(script, sequence);
				}
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void PollTimerTrigger(StageScriptDefinition i_script, StageScriptSequenceDocument i_sequence)
	{
		string key = SequenceKey(i_script, i_sequence.Id);
		if (!m_timerTriggerTimes.TryGetValue(key, out float next)) { m_timerTriggerTimes[key] = Time.time + i_sequence.Seconds; return; }
		if (Time.time < next) return;
		m_timerTriggerTimes[key] = Time.time + i_sequence.Seconds;
		StartSequence(i_script, i_sequence);
	}

	private void PollEdgeTrigger(StageScriptDefinition i_script, StageScriptSequenceDocument i_sequence)
	{
		string key = SequenceKey(i_script, i_sequence.Id);
		// A world predicate is not consumed while its additional conditions are false.
		// This lets, for example, an already-open door trigger once the requested wave begins.
		bool active = TriggerPredicate(i_sequence) && ConditionsPass(i_script, i_sequence.Conditions);
		m_worldTriggerStates.TryGetValue(key, out bool wasActive);
		m_worldTriggerStates[key] = active;
		if (active && !wasActive) StartSequence(i_script, i_sequence);
	}

	private bool TriggerPredicate(StageScriptSequenceDocument i_sequence)
	{
		if (i_sequence.Trigger == "enemy-count")
		{
			int count = CountEnemies(i_sequence.Enemy);
			return i_sequence.State == "at-most" ? count <= i_sequence.Value : count >= i_sequence.Value;
		}
		Transform target = FindObject(transform, i_sequence.ObjectId);
		if (i_sequence.Trigger == "object-state") return target != null && target.gameObject.activeInHierarchy == (i_sequence.State == "active");
		if (target == null) return false;
		Door door = target.GetComponent<Door>();
		DoorRoller roller = target.GetComponent<DoorRoller>();
		bool found = door != null || roller != null;
		bool open = door != null ? door.GetIsOpen() : roller != null && roller.GetIsOpen();
		return found && open == (i_sequence.State == "open");
	}

	public void SendSignal(string i_signal)
	{
		m_signalVersions.TryGetValue(i_signal, out int version);
		m_signalVersions[i_signal] = version + 1;
		Trigger("signal", i_signal);
	}

	public void NotifyPlayerVolume(string i_objectId, bool i_entered)
	{
		Trigger(i_entered ? "player-enter" : "player-exit", ScriptObjectId(i_objectId));
	}

	private void Trigger(string i_trigger, string i_signal)
	{
		foreach (StageScriptDefinition script in m_scripts)
			foreach (StageScriptSequenceDocument sequence in script.Sequences)
			{
				if (sequence.Trigger != i_trigger) continue;
				if (i_trigger == "signal" && sequence.Signal != i_signal) continue;
				if (i_trigger == "interaction" && ScriptObjectId(sequence.Signal) != ScriptObjectId(i_signal)) continue;
				if ((i_trigger == "player-enter" || i_trigger == "player-exit")
					&& ScriptObjectId(sequence.ObjectId) != ScriptObjectId(i_signal)) continue;
				StartSequence(script, sequence);
			}
	}

	private void TriggerMachineState(StageScriptDefinition i_script, string i_machine, string i_state)
	{
		foreach (StageScriptSequenceDocument sequence in i_script.Sequences)
			if (sequence.Trigger == "machine-state" && sequence.ObjectId == i_machine && sequence.State == i_state)
				StartSequence(i_script, sequence);
	}

	private void StartSequence(StageScriptDefinition i_script, StageScriptSequenceDocument i_sequence)
	{
		string sequenceKey = SequenceKey(i_script, i_sequence.Id);
		if ((i_sequence.Once && m_completedOnce.Contains(sequenceKey)) || m_running.Contains(sequenceKey)) return;
		if (!ConditionsPass(i_script, i_sequence.Conditions)) return;
		if (i_script.Stage.Path == "stage/fer-tiled-port")
			Debug.Log("[ModLoader] FER sequence started: " + i_script.Id + "/" + i_sequence.Id);
		if (i_sequence.Once) m_completedOnce.Add(sequenceKey);
		m_running.Add(sequenceKey);
		Coroutine coroutine = StartCoroutine(RunTopLevel(i_script, i_sequence, sequenceKey));
		// A sequence containing no yielding action can finish during StartCoroutine.
		if (m_running.Contains(sequenceKey)) m_runningCoroutines[sequenceKey] = coroutine;
	}

	private IEnumerator RunTopLevel(StageScriptDefinition i_script, StageScriptSequenceDocument i_sequence, string i_sequenceKey)
	{
		yield return RunActions(i_script, i_sequence.Actions, 0, new StageScriptExecutionBudget());
		m_running.Remove(i_sequenceKey);
		m_runningCoroutines.Remove(i_sequenceKey);
	}

	private bool ConditionsPass(StageScriptDefinition i_script, IEnumerable<StageScriptConditionDocument> i_conditions)
	{
		foreach (StageScriptConditionDocument condition in i_conditions)
		{
			float actual = ReadConditionValue(i_script, condition);
			switch (condition.Operator)
			{
				case "==": if (!Mathf.Approximately(actual, condition.Value)) return false; break;
				case "!=": if (Mathf.Approximately(actual, condition.Value)) return false; break;
				case "<": if (!(actual < condition.Value)) return false; break;
				case "<=": if (!(actual <= condition.Value)) return false; break;
				case ">": if (!(actual > condition.Value)) return false; break;
				case ">=": if (!(actual >= condition.Value)) return false; break;
			}
		}
		return true;
	}

	private float ReadConditionValue(StageScriptDefinition i_script, StageScriptConditionDocument i_condition)
	{
		string source = string.IsNullOrEmpty(i_condition.Source) ? "variable" : i_condition.Source;
		if (source == "wave") return m_wave == null ? 0f : m_wave.GetNumWaveCurrent();
		if (source == "enemy-count") return CountEnemies(i_condition.Enemy);
		Transform target = (source == "door-open" || source == "object-active") ? FindObject(transform, i_condition.ObjectId) : null;
		if (source == "object-active") return target != null && target.gameObject.activeInHierarchy ? 1f : 0f;
		if (source == "door-open")
		{
			Door door = target == null ? null : target.GetComponent<Door>();
			DoorRoller roller = target == null ? null : target.GetComponent<DoorRoller>();
			return ((door != null && door.GetIsOpen()) || (roller != null && roller.GetIsOpen())) ? 1f : 0f;
		}
		m_variables.TryGetValue(VariableKey(i_script, i_condition.Variable), out float actual);
		return actual;
	}

	private int CountEnemies(string i_enemyId)
	{
		Stage stage = GetComponent<Stage>();
		if (stage == null) return 0;
		ContentId filter = default;
		bool filtered = !string.IsNullOrEmpty(i_enemyId) && ContentId.TryParse(i_enemyId, out filter);
		int count = 0;
		foreach (NPC enemy in stage.GetAllNPCs())
		{
			if (enemy == null || !enemy.gameObject.activeSelf || enemy.IsDead() || enemy.IsIgnoreWave()) continue;
			if (filtered && (!RuntimeContentIdentity.TryResolve(enemy, out ContentId id, out ContentCategory category) || category != ContentCategory.Enemy || id != filter)) continue;
			count++;
		}
		return count;
	}

	private IEnumerator RunActions(StageScriptDefinition i_script, IEnumerable<StageScriptActionDocument> i_actions,
		int i_depth, StageScriptExecutionBudget io_budget)
	{
		if (i_depth > 16) { Debug.LogWarning("[ModLoader] Stage-script action nesting exceeded the safe limit."); yield break; }
		foreach (StageScriptActionDocument action in i_actions)
		{
			if (--io_budget.Remaining < 0)
			{
				Debug.LogWarning("[ModLoader] Stage-script execution exceeded the 4096-action safe limit.");
				yield break;
			}
			switch (action.Type)
				{
					case "wait": yield return new WaitForSeconds(action.Seconds); break;
					case "wait-for-door": yield return WaitForDoor(action); break;
					case "wait-for-enemy-count": yield return WaitForEnemyCount(action); break;
					case "wait-for-signal": yield return WaitForSignal(action); break;
					case "wait-for-sequence": yield return WaitForSequence(i_script, action); break;
					case "wait-until": yield return WaitUntilConditions(i_script, action); break;
					case "wait-until-player-distant": yield return WaitUntilPlayerDistant(action); break;
				case "repeat":
					for (int repeat = 0; repeat < action.Amount; repeat++)
						yield return RunActions(i_script, action.Actions, i_depth + 1, io_budget);
					break;
				case "notify": Notify(action.Message); break;
				case "set-variable": m_variables[VariableKey(i_script, action.Variable)] = action.Value; break;
				case "add-variable":
					string key = VariableKey(i_script, action.Variable);
					m_variables.TryGetValue(key, out float current); m_variables[key] = current + action.Value;
					break;
				case "send-signal": SendSignal(action.Signal); break;
				case "run-sequence": yield return RunNamedSequence(i_script, action.Sequence, i_depth + 1, io_budget); break;
				case "start-sequence":
					StageScriptSequenceDocument parallel = FindSequence(i_script, action.Sequence);
					if (parallel != null) StartSequence(i_script, parallel);
					break;
				case "cancel-sequence": CancelSequence(i_script, action.Sequence); break;
				case "set-machine-state": yield return SetMachineState(i_script, action.ObjectId, action.State, i_depth + 1, io_budget); break;
				case "set-object-active":
					Transform target = FindObject(transform, action.ObjectId);
					if (target != null) target.gameObject.SetActive(action.Active);
					else Debug.LogWarning("[ModLoader] Stage script could not find object '" + action.ObjectId + "'.");
					break;
				case "spawn-actor": SpawnActor(action); break;
				case "move-object": yield return MoveObject(action); break;
				case "move-object-to": yield return MoveObjectTo(action); break;
				case "rotate-object": yield return RotateObject(action); break;
				case "set-door": ApplyDoor(action); break;
				case "set-light": ApplyLight(action); break;
				case "set-spawner-enabled": SetSpawnerEnabled(action); break;
				case "set-interaction-enabled": SetInteractionEnabled(action); break;
				case "activate-interaction": ActivateInteraction(action); break;
				case "queue-spawns": QueueSpawns(action); break;
				case "teleport-player": TeleportPlayer(action); break;
				case "set-player-input": SetPlayerInput(action); break;
				case "set-hud-visible": SetHudVisible(action); break;
				case "set-player-facing": SetPlayerFacing(action); break;
				case "remove-player-clothing": RemovePlayerClothing(); break;
				case "play-player-animation": PlayPlayerAnimation(action); break;
				case "kill-player": KillPlayer(); break;
				case "apply-player-status": ApplyPlayerStatus(action); break;
				case "camera-shake": ApplyCameraShake(action); break;
				case "camera-zoom": ApplyCameraZoom(action); break;
				case "set-global-light": ApplyGlobalLight(action); break;
				case "play-audio": SetAudioPlaying(action, true); break;
				case "stop-audio": SetAudioPlaying(action, false); break;
				case "play-particles": PlayParticles(action); break;
				case "play-animation": PlayAnimation(action); break;
			}
		}
	}

	private IEnumerator SetMachineState(StageScriptDefinition i_script, string i_machineId, string i_state,
		int i_depth, StageScriptExecutionBudget io_budget)
	{
		if (i_depth > 16) { Debug.LogWarning("[ModLoader] Stage-script machine nesting exceeded the safe limit."); yield break; }
		StageScriptMachineDocument machine = null;
		foreach (StageScriptMachineDocument candidate in i_script.Machines)
			if (candidate.Id == i_machineId) { machine = candidate; break; }
		if (machine == null) { Debug.LogWarning("[ModLoader] Stage-script machine was not found: " + i_machineId); yield break; }
		StageScriptMachineStateDocument state = null;
		foreach (StageScriptMachineStateDocument candidate in machine.States)
			if (candidate.Id == i_state) { state = candidate; break; }
		if (state == null) { Debug.LogWarning("[ModLoader] Stage-script machine state was not found: " + i_machineId + "/" + i_state); yield break; }
		string key = i_script.Id + ":machine:" + i_machineId;
		if (m_machineStates.TryGetValue(key, out string current) && current == i_state) yield break;
		m_machineStates[key] = i_state;
		yield return RunActions(i_script, state.Actions, i_depth, io_budget);
		TriggerMachineState(i_script, i_machineId, i_state);
	}

	private IEnumerator WaitUntilConditions(StageScriptDefinition i_script, StageScriptActionDocument i_action)
	{
		float elapsed = 0f;
		while (!ConditionsPass(i_script, i_action.Conditions))
		{
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator RunNamedSequence(StageScriptDefinition i_script, string i_name, int i_depth,
		StageScriptExecutionBudget io_budget)
	{
		if (i_depth > 16) { Debug.LogWarning("[ModLoader] Stage-script sequence nesting exceeded the safe limit."); yield break; }
		StageScriptSequenceDocument sequence = FindSequence(i_script, i_name);
		if (sequence == null) yield break;
		string sequenceKey = SequenceKey(i_script, sequence.Id);
		// Calling an already-running parallel sequence behaves like a join instead of duplicating its effects.
		while (m_running.Contains(sequenceKey)) yield return null;
		if ((sequence.Once && m_completedOnce.Contains(sequenceKey)) || !ConditionsPass(i_script, sequence.Conditions)) yield break;
		if (sequence.Once) m_completedOnce.Add(sequenceKey);
		yield return RunActions(i_script, sequence.Actions, i_depth, io_budget);
	}

	private void CancelSequence(StageScriptDefinition i_script, string i_name)
	{
		string sequenceKey = SequenceKey(i_script, i_name);
		if (m_runningCoroutines.TryGetValue(sequenceKey, out Coroutine coroutine) && coroutine != null) StopCoroutine(coroutine);
		m_runningCoroutines.Remove(sequenceKey);
		m_running.Remove(sequenceKey);
	}

	private static StageScriptSequenceDocument FindSequence(StageScriptDefinition i_script, string i_name)
	{
		foreach (StageScriptSequenceDocument sequence in i_script.Sequences)
			if (sequence.Id == i_name) return sequence;
		return null;
	}

	private IEnumerator MoveObject(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		if (target == null) { Debug.LogWarning("[ModLoader] Stage script motion target was not found: " + i_action.ObjectId); yield break; }
		Vector3 from = target.position;
		Vector3 destination = transform.TransformPoint(new Vector3(i_action.X, i_action.Y, 0f));
		Vector3 to = new Vector3(destination.x, destination.y, from.z);
		if (i_action.Seconds <= 0f) { target.position = to; yield break; }
		float elapsed = 0f;
		while (elapsed < i_action.Seconds)
		{
			elapsed += Time.deltaTime;
			target.position = Vector3.LerpUnclamped(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / i_action.Seconds)));
			yield return null;
		}
		target.position = to;
	}

	private void SpawnActor(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		NPC actor = target == null ? null : target.GetComponent<NPC>();
		if (actor == null) { Debug.LogWarning("[ModLoader] Stage script actor was not found: " + i_action.ObjectId); return; }
		bool wasActive = actor.gameObject.activeSelf;
		actor.gameObject.SetActive(true);
		if (!actor.gameObject.activeInHierarchy)
			Debug.LogWarning("[ModLoader] Stage script actor is still hidden by an inactive parent: " + i_action.ObjectId);
		// Compatibility ports may still have a prefab callback that activated/spawned
		// the actor immediately before this script observes the interaction.
		if (!wasActive || actor.GetTimeSpawn() <= 0f) actor.Spawn(true);
		if (GetComponent<RuntimeContentIdentity>()?.TryGetContentId(out ContentId stageId) == true
			&& stageId.Path == "stage/fer-tiled-port")
		{
			Debug.Log("[ModLoader] FER actor spawn: " + i_action.ObjectId + " active="
				+ actor.gameObject.activeInHierarchy + " dead=" + actor.IsDead()
				+ " position=" + actor.transform.position);
			if (i_action.ObjectId == "npc-android")
				StartCoroutine(LogFerActorAfterSpawn(actor, actor.transform.position));
		}
	}

	private IEnumerator LogFerActorAfterSpawn(NPC i_actor, Vector3 i_spawnPosition)
	{
		yield return new WaitForSeconds(1f);
		if (i_actor == null)
		{
			Debug.LogWarning("[ModLoader] FER Android was destroyed within one second of spawning.");
			yield break;
		}
		SpriteRenderer[] renderers = i_actor.GetComponentsInChildren<SpriteRenderer>(true);
		int visible = 0;
		foreach (SpriteRenderer renderer in renderers)
			if (renderer.enabled && renderer.gameObject.activeInHierarchy && renderer.color.a > 0.05f) visible++;
		Debug.Log("[ModLoader] FER Android one second after spawn: active="
			+ i_actor.gameObject.activeInHierarchy + " dead=" + i_actor.IsDead()
			+ " position=" + i_actor.transform.position + " moved="
			+ Vector3.Distance(i_spawnPosition, i_actor.transform.position).ToString("F2")
			+ " visibleSprites=" + visible + "/" + renderers.Length);
	}

	private IEnumerator MoveObjectTo(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Transform destination = FindObject(transform, i_action.DestinationId);
		if (target == null || destination == null) { Debug.LogWarning("[ModLoader] Stage script move target or destination was not found: " + i_action.ObjectId + " -> " + i_action.DestinationId); yield break; }
		Vector3 from = target.position;
		Vector3 to = new Vector3(destination.position.x, destination.position.y, from.z);
		if (i_action.Seconds <= 0f) { target.position = to; yield break; }
		float elapsed = 0f;
		while (elapsed < i_action.Seconds)
		{
			elapsed += Time.deltaTime;
			target.position = Vector3.LerpUnclamped(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / i_action.Seconds)));
			yield return null;
		}
		target.position = to;
	}

	private IEnumerator RotateObject(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		if (target == null) { Debug.LogWarning("[ModLoader] Stage script rotation target was not found: " + i_action.ObjectId); yield break; }
		float from = target.localEulerAngles.z;
		float to = i_action.Value;
		if (i_action.Seconds <= 0f) { target.localRotation = Quaternion.Euler(0f, 0f, to); yield break; }
		float elapsed = 0f;
		while (elapsed < i_action.Seconds)
		{
			elapsed += Time.deltaTime;
			float angle = Mathf.LerpAngle(from, to, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / i_action.Seconds)));
			target.localRotation = Quaternion.Euler(0f, 0f, angle);
			yield return null;
		}
		target.localRotation = Quaternion.Euler(0f, 0f, to);
	}

	private IEnumerator WaitForDoor(StageScriptActionDocument i_action)
	{
		float elapsed = 0f;
		while (true)
		{
			Transform target = FindObject(transform, i_action.ObjectId);
			Door door = target == null ? null : target.GetComponent<Door>();
			DoorRoller roller = target == null ? null : target.GetComponent<DoorRoller>();
			bool found = door != null || roller != null;
			bool open = door != null ? door.GetIsOpen() : roller != null && roller.GetIsOpen();
			if (found && open == (i_action.State == "open")) yield break;
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator WaitForEnemyCount(StageScriptActionDocument i_action)
	{
		float elapsed = 0f;
		while (true)
		{
			Stage stage = GetComponent<Stage>();
			int count = 0;
			if (stage != null)
				foreach (NPC enemy in stage.GetAllNPCs())
					if (enemy != null && enemy.gameObject.activeSelf && !enemy.IsDead() && !enemy.IsIgnoreWave()) count++;
			if (i_action.State == "at-most" ? count <= i_action.Value : count >= i_action.Value) yield break;
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator WaitForSignal(StageScriptActionDocument i_action)
	{
		m_signalVersions.TryGetValue(i_action.Signal, out int initialVersion);
		float elapsed = 0f;
		while (true)
		{
			m_signalVersions.TryGetValue(i_action.Signal, out int currentVersion);
			if (currentVersion != initialVersion) yield break;
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator WaitForSequence(StageScriptDefinition i_script, StageScriptActionDocument i_action)
	{
		string sequenceKey = SequenceKey(i_script, i_action.Sequence);
		float elapsed = 0f;
		while (m_running.Contains(sequenceKey))
		{
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator WaitUntilPlayerDistant(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Transform destination = string.IsNullOrEmpty(i_action.DestinationId) ? null : FindObject(transform, i_action.DestinationId);
		if (target == null || (!string.IsNullOrEmpty(i_action.DestinationId) && destination == null)) { Debug.LogWarning("[ModLoader] Stage script distance target was not found: " + i_action.ObjectId); yield break; }
		float elapsed = 0f;
		while (true)
		{
			Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
			if (player == null) yield break;
			Vector2 playerPosition = player.GetPosHips();
			bool distant = Vector2.Distance(target.position, playerPosition) > i_action.Value
				&& (destination == null || Vector2.Distance(destination.position, playerPosition) > i_action.Value);
			if (distant) yield break;
			if (i_action.Seconds > 0f && elapsed >= i_action.Seconds) yield break;
			elapsed += 0.1f;
			yield return new WaitForSeconds(0.1f);
		}
	}

	private void ApplyDoor(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Door door = target == null ? null : target.GetComponent<Door>();
		DoorRoller roller = target == null ? null : target.GetComponent<DoorRoller>();
		if (door != null) { if (i_action.State == "open") door.Open(); else if (i_action.State == "closed") door.Close(); else door.OpenOrClose(); }
		else if (roller != null) { if (i_action.State == "open") roller.Open(); else if (i_action.State == "closed") roller.Close(); else roller.OpenOrClose(); }
		else Debug.LogWarning("[ModLoader] Stage script door was not found: " + i_action.ObjectId);
	}

	private void ApplyLight(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		LightBulb light = target == null ? null : target.GetComponent<LightBulb>();
		if (light != null) light.ApplyModAction(i_action.State);
		else Debug.LogWarning("[ModLoader] Stage script light was not found: " + i_action.ObjectId);
	}

	private void ActivateInteraction(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Interactable interaction = target == null ? null : target.GetComponent<Interactable>();
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (interaction != null) interaction.Activate(player, InteractableActivationType.Operator);
		else Debug.LogWarning("[ModLoader] Stage script interaction was not found: " + i_action.ObjectId);
	}

	private void QueueSpawns(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Spawner spawner = target == null ? null : target.GetComponent<Spawner>();
		if (spawner != null) for (int count = 0; count < i_action.Amount; count++) spawner.AddSpawnOneAmount();
		else Debug.LogWarning("[ModLoader] Stage script spawner was not found: " + i_action.ObjectId);
	}

	private void SetSpawnerEnabled(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Spawner spawner = target == null ? null : target.GetComponent<Spawner>();
		if (spawner == null) { Debug.LogWarning("[ModLoader] Stage script spawner was not found: " + i_action.ObjectId); return; }
		if (i_action.Active) { spawner.Enable(); return; }
		int pending = spawner.DisableAndDrainPendingSpawns();
		ManagerWave wave = GetComponentInChildren<ManagerWave>(true);
		if (wave != null) wave.RequeueSpawns(pending);
	}

	private void SetInteractionEnabled(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Interactable interaction = target == null ? null : target.GetComponent<Interactable>();
		if (interaction != null) interaction.SetIsUnInteractable(!i_action.Active);
		else Debug.LogWarning("[ModLoader] Stage script interaction was not found: " + i_action.ObjectId);
	}

	private void TeleportPlayer(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (target != null && player != null) player.PlaceFeetOnPos(target.position);
		else Debug.LogWarning("[ModLoader] Stage script teleport target or player was unavailable: " + i_action.ObjectId);
	}

	private void SetPlayerInput(StageScriptActionDocument i_action)
	{
		PlayerController controller = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayerController();
		if (controller == null) { Debug.LogWarning("[ModLoader] Stage script could not change player input because the controller was unavailable."); return; }
		controller.SetIsForceIgnoreInput(!i_action.Active);
		m_changedPlayerInput = true;
	}

	private void SetHudVisible(StageScriptActionDocument i_action)
	{
		ManagerHud hud = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerHud();
		if (hud == null) { Debug.LogWarning("[ModLoader] Stage script could not change HUD visibility because the HUD was unavailable."); return; }
		if (i_action.Active) hud.ShowHud(); else hud.HideHud();
		m_changedHudVisibility = true;
	}

	private static void SetPlayerFacing(StageScriptActionDocument i_action)
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player != null) player.SetIsFacingLeft(i_action.State == "left");
		else Debug.LogWarning("[ModLoader] Stage script could not set facing because the player was unavailable.");
	}

	private static void RemovePlayerClothing()
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player != null && player.GetSkeletonPlayer() != null) player.GetSkeletonPlayer().RemoveAllClothing();
		else Debug.LogWarning("[ModLoader] Stage script could not remove clothing because the player was unavailable.");
	}

	private void PlayPlayerAnimation(StageScriptActionDocument i_action)
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		Animator animator = player == null ? null : player.GetAnimator();
		if (animator == null) { Debug.LogWarning("[ModLoader] Stage script could not animate the player because its Animator was unavailable."); return; }
		RuntimeAnimatorController previous = animator.runtimeAnimatorController;
		if (i_action.Controller == "finisher")
		{
			RuntimeAnimatorController finisher = Resources.Load<RuntimeAnimatorController>("Raper/RapeAnimator");
			if (finisher == null) { Debug.LogWarning("[ModLoader] The reviewed player finisher controller was unavailable."); return; }
			if (m_originalPlayerAnimatorController == null) m_originalPlayerAnimatorController = previous;
			animator.runtimeAnimatorController = finisher;
		}
		int state = Animator.StringToHash(i_action.Animation);
		for (int layer = 0; layer < animator.layerCount; layer++)
			if (animator.HasState(layer, state)) { animator.Play(state, layer, 0f); return; }
		animator.runtimeAnimatorController = previous;
		Debug.LogWarning("[ModLoader] Stage script player animation state was not found: " + i_action.Animation);
	}

	private static void KillPlayer()
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player != null) player.Die();
		else Debug.LogWarning("[ModLoader] Stage script could not kill the player because the player was unavailable.");
	}

	private void RestoreTransientPlayerState()
	{
		if (!m_changedPlayerInput && !m_changedHudVisibility && m_originalPlayerAnimatorController == null) return;
		if (CommonReferences.Instance == null) return;
		if (m_changedPlayerInput && CommonReferences.Instance.GetPlayerController() != null)
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(false);
		if (m_changedHudVisibility && CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().ShowHud();
		if (m_originalPlayerAnimatorController != null)
		{
			Player player = CommonReferences.Instance.GetPlayer();
			if (player != null && player.GetAnimator() != null)
				player.GetAnimator().runtimeAnimatorController = m_originalPlayerAnimatorController;
		}
		m_changedPlayerInput = false; m_changedHudVisibility = false; m_originalPlayerAnimatorController = null;
	}

	private static void ApplyPlayerStatus(StageScriptActionDocument i_action)
	{
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player == null) { Debug.LogWarning("[ModLoader] Stage script could not apply a player status because the player was unavailable."); return; }
		if (i_action.Status == "jacky-curse")
		{
			if (player.IsStatusEffectAppliedAlready("Jacky Curse")) return;
			player.ApplyStatusEffect(new SEJackyCurse("Jacky Curse", "Stage Script", TypeStatusEffect.Negative,
				i_action.DurationSeconds, false, i_action.TicksPerSecond, i_action.Chance,
				i_action.ChanceIncrease, i_action.MaxActive));
		}
	}

	private static CameraXGame GetCamera()
	{
		return CommonReferences.Instance == null || CommonReferences.Instance.GetManagerCamerasXGame() == null
			? null : CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent();
	}

	private static void ApplyCameraShake(StageScriptActionDocument i_action)
	{
		CameraXGame camera = GetCamera();
		if (camera != null) camera.Shake(i_action.Value, i_action.Seconds);
	}

	private static void ApplyCameraZoom(StageScriptActionDocument i_action)
	{
		CameraXGame camera = GetCamera();
		if (camera != null) camera.ZoomToFOV(i_action.Value, i_action.Seconds);
	}

	private void ApplyGlobalLight(StageScriptActionDocument i_action)
	{
		Stage stage = GetComponent<Stage>();
		if (stage != null && stage.GetLightGlobal() != null) stage.GetLightGlobal().intensity = i_action.Value;
	}

	private void PlayParticles(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		ParticleSystem particles = target == null ? null : target.GetComponentInChildren<ParticleSystem>(true);
		if (particles != null) particles.Play(true);
		else Debug.LogWarning("[ModLoader] Stage script particle target was not found: " + i_action.ObjectId);
	}

	private void SetAudioPlaying(StageScriptActionDocument i_action, bool i_play)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		ModMapAudioSource authored = target == null ? null : target.GetComponent<ModMapAudioSource>();
		if (authored != null) { if (i_play) authored.RequestPlay(); else authored.RequestStop(); return; }
		AudioSource audio = target == null ? null : target.GetComponent<AudioSource>();
		if (audio == null) { Debug.LogWarning("[ModLoader] Stage script audio target was not found: " + i_action.ObjectId); return; }
		if (i_play) audio.Play(); else audio.Stop();
	}

	private void PlayAnimation(StageScriptActionDocument i_action)
	{
		Transform target = FindObject(transform, i_action.ObjectId);
		Animator animator = target == null ? null : target.GetComponentInChildren<Animator>(true);
		if (animator == null) { Debug.LogWarning("[ModLoader] Stage script animation target was not found: " + i_action.ObjectId); return; }
		int state = Animator.StringToHash(i_action.Animation);
		for (int layer = 0; layer < animator.layerCount; layer++)
			if (animator.HasState(layer, state)) { animator.Play(state, layer, 0f); return; }
		Debug.LogWarning("[ModLoader] Stage script animation state was not found: " + i_action.Animation);
	}

	private static string VariableKey(StageScriptDefinition i_script, string i_name) => i_script.Id + ":" + i_name;
	private static string SequenceKey(StageScriptDefinition i_script, string i_name) => i_script.Id + ":" + i_name;
	private static Transform FindObject(Transform i_root, string i_id)
	{
		foreach (ModStageMarker marker in i_root.GetComponentsInChildren<ModStageMarker>(true))
			if (marker.ObjectId == i_id) return marker.transform;
		foreach (Transform child in i_root.GetComponentsInChildren<Transform>(true))
		{
			ModRetainedCoreStageObject retained = child.GetComponent<ModRetainedCoreStageObject>();
			if ((retained != null && retained.ObjectId == i_id) || child.name == i_id
				|| child.name.EndsWith("_" + i_id, StringComparison.Ordinal) || ScriptObjectId(child.name) == i_id) return child;
		}
		return null;
	}
	private static string ScriptObjectId(string i_name)
	{
		string value = (i_name ?? "object").Trim().ToLowerInvariant();
		System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length);
		bool dash = false;
		foreach (char character in value)
		{
			if (char.IsLetterOrDigit(character)) { result.Append(character); dash = false; }
			else if (!dash && result.Length > 0) { result.Append('-'); dash = true; }
		}
		while (result.Length > 0 && result[result.Length - 1] == '-') result.Length--;
		if (result.Length == 0 || !char.IsLetter(result[0])) result.Insert(0, "object-");
		return result.ToString();
	}
	private static void Notify(string i_message)
	{
		if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(i_message, ColorTextNotification.Other, false);
	}
}

public sealed class ModStageActivationBridge : MonoBehaviour
{
	[SerializeField] private string m_objectId;
	private Interactable m_interaction;

	public void Configure(string i_objectId) { m_objectId = i_objectId; }

	private void OnEnable()
	{
		m_interaction = GetComponent<Interactable>();
		if (m_interaction != null) m_interaction.OnActivate += HandleActivation;
	}

	private void OnDisable()
	{
		if (m_interaction != null) m_interaction.OnActivate -= HandleActivation;
		m_interaction = null;
	}

	private void HandleActivation(Interactable i_interaction)
	{
		ModStageScriptRunner runner = GetComponentInParent<ModStageScriptRunner>();
		if (runner != null) runner.NotifyInteraction(m_objectId);
		else Debug.LogWarning("[ModLoader] Stage interaction has no script runner: " + m_objectId);
	}
}

public sealed class ModStageMarker : MonoBehaviour
{
	public string ObjectId { get; private set; }

	public void Configure(string i_objectId)
	{
		ObjectId = i_objectId;
	}
}

public sealed class ModStagePickupSignal : MonoBehaviour
{
	private PickUpable m_pickup;
	private ModStageScriptRunner m_runner;
	private string m_signal;
	private bool m_subscribed;
	private bool m_sent;

	public void Configure(string i_signal)
	{
		m_signal = i_signal;
		m_pickup = GetComponent<PickUpable>();
		m_runner = GetComponentInParent<ModStageScriptRunner>(true);
	}

	private void OnEnable()
	{
		if (m_pickup == null) m_pickup = GetComponent<PickUpable>();
		if (m_runner == null) m_runner = GetComponentInParent<ModStageScriptRunner>(true);
		if (!m_subscribed && m_pickup != null) { m_pickup.OnPickUp += HandlePickup; m_subscribed = true; }
	}

	private void OnDestroy()
	{
		if (m_subscribed && m_pickup != null) m_pickup.OnPickUp -= HandlePickup;
	}

	private void HandlePickup()
	{
		if (m_sent || m_runner == null || string.IsNullOrEmpty(m_signal)) return;
		m_sent = true;
		if (m_signal == "fer-keycard-picked-up")
			Debug.Log("[ModLoader] FER keycard pickup signal emitted.");
		m_runner.SendSignal(m_signal);
	}
}

public sealed class ModStageScriptTrigger : MonoBehaviour
{
	private readonly HashSet<Collider2D> m_playerColliders = new HashSet<Collider2D>();
	private string m_objectId;

	public void Configure(string i_objectId)
	{
		m_objectId = i_objectId;
	}

	private void OnTriggerEnter2D(Collider2D i_collider)
	{
		if (!IsPlayerCollider(i_collider) || !m_playerColliders.Add(i_collider) || m_playerColliders.Count != 1) return;
		NotifyRunner(true);
	}

	private void OnTriggerExit2D(Collider2D i_collider)
	{
		if (!m_playerColliders.Remove(i_collider) || m_playerColliders.Count != 0) return;
		NotifyRunner(false);
	}

	private void OnDisable()
	{
		m_playerColliders.Clear();
	}

	private void NotifyRunner(bool i_entered)
	{
		ModStageScriptRunner runner = GetComponentInParent<ModStageScriptRunner>();
		if (runner != null) runner.NotifyPlayerVolume(m_objectId, i_entered);
	}

	private static bool IsPlayerCollider(Collider2D i_collider)
	{
		if (i_collider == null) return false;
		if (i_collider.GetComponent<Player>() != null || i_collider.GetComponent<PlayerCollisionHandler>() != null) return true;
		BodyPartPlayer bodyPart = i_collider.GetComponent<BodyPartPlayer>();
		return bodyPart != null && bodyPart.GetOwner() is Player;
	}
}

/// <summary>Marks an explicitly selected compatibility object so authored-layout cleanup leaves it intact.</summary>
public sealed class ModRetainedCoreStageObject : MonoBehaviour
{
	public string ObjectId { get; private set; }

	public void Configure(string i_objectId)
	{
		ObjectId = i_objectId;
	}
}

public sealed class ModMapInteraction : Interactable
{
	private readonly List<Door> m_doors = new List<Door>();
	private readonly List<DoorRoller> m_rollerDoors = new List<DoorRoller>();
	private readonly List<LightBulb> m_lights = new List<LightBulb>();
	private readonly List<ModMapInteraction> m_chainTargets = new List<ModMapInteraction>();
	private readonly List<ModMapInteraction> m_requiredInteractions = new List<ModMapInteraction>();
	private readonly List<string> m_spawnerIds = new List<string>();
	private Stage m_stage;
	private string m_message;
	private string m_doorAction;
	private string m_lightAction;
	private float m_delaySeconds;
	private int m_giveMoney;
	private int m_minWave;
	private int m_maxWave;
	private int m_activationsRequired;
	private int m_activationCount;
	private int m_spawnCount;
	private bool m_isExecuting;
	private ContentId? m_requiredItem;
	private bool m_consumeRequiredItem;
	private ContentId? m_giveItem;
	private int m_giveItemAmount;
	private int m_minEnemies;
	private int m_maxEnemies;
	private readonly List<ContentId> m_requiredChallenges = new List<ContentId>();
	private int m_requiredMoney;
	private float m_restoreHealth;
	private Vector2 m_knockback;
	private float m_ragdollSeconds;
	private bool m_hasCompletedActions;

	public bool HasCompletedActivation => m_hasCompletedActions;

	public void Configure(TiledInteractionDefinition i_definition, IEnumerable<Door> i_doors,
		IEnumerable<DoorRoller> i_rollerDoors, IEnumerable<LightBulb> i_lights,
		IEnumerable<ModMapInteraction> i_chainTargets, IEnumerable<ModMapInteraction> i_requiredInteractions, Stage i_stage)
	{
		ResetModActivationLinks();
		m_name = string.IsNullOrWhiteSpace(i_definition.Area.Name) ? "Map interaction" : i_definition.Area.Name;
		m_priceToActivate = i_definition.Price;
		m_isSingleUse = i_definition.SingleUse;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = i_definition.Trigger == "use";
		m_isCanBeTouchedToActivate = i_definition.Trigger == "touch";
		m_isCanBeShotToActivate = i_definition.Trigger == "shoot";
		m_isCanBeActivatedByNPC = false;
		m_isShowOutline = false;
		m_isHideNotificationInteract = false;
		m_isObstructionPaths = false;
		m_isUnInteractable = false;
		m_message = i_definition.Message;
		m_doorAction = i_definition.DoorAction;
		m_lightAction = i_definition.LightAction;
		m_delaySeconds = i_definition.DelaySeconds;
		m_giveMoney = i_definition.GiveMoney;
		m_minWave = i_definition.MinWave;
		m_maxWave = i_definition.MaxWave;
		m_activationsRequired = i_definition.ActivationsRequired;
		m_activationCount = 0;
		m_spawnCount = i_definition.SpawnCount;
		m_requiredItem = i_definition.RequiredItem;
		m_consumeRequiredItem = i_definition.ConsumeRequiredItem;
		m_giveItem = i_definition.GiveItem;
		m_giveItemAmount = i_definition.GiveItemAmount;
		m_minEnemies = i_definition.MinEnemies;
		m_maxEnemies = i_definition.MaxEnemies;
		m_requiredChallenges.Clear();
		m_requiredChallenges.AddRange(i_definition.RequiredChallenges);
		m_requiredMoney = i_definition.RequiredMoney;
		m_restoreHealth = i_definition.RestoreHealth;
		m_knockback = new Vector2(i_definition.KnockbackX, i_definition.KnockbackY);
		m_ragdollSeconds = i_definition.RagdollSeconds;
		m_hasCompletedActions = false;
		m_stage = i_stage;
		m_doors.Clear();
		m_doors.AddRange(i_doors);
		m_rollerDoors.Clear();
		m_rollerDoors.AddRange(i_rollerDoors);
		m_lights.Clear();
		m_lights.AddRange(i_lights);
		m_chainTargets.Clear();
		m_chainTargets.AddRange(i_chainTargets);
		m_requiredInteractions.Clear();
		m_requiredInteractions.AddRange(i_requiredInteractions);
		m_spawnerIds.Clear();
		m_spawnerIds.AddRange(i_definition.TargetSpawnerIds);
	}

	public override void Use()
	{
		if (!ConditionsAvailable(showMessage: true)) return;
		base.Use();
	}

	public override void Activate(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!ConditionsAvailable(showMessage: i_activationType == InteractableActivationType.Use)) return;
		m_activationCount++;
		if (m_activationCount < m_activationsRequired)
		{
			Notify("Activated " + m_activationCount + "/" + m_activationsRequired + ".");
			return;
		}
		base.Activate(i_initiator, i_activationType);
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isExecuting) StartCoroutine(ExecuteActions(i_initiator, i_activationType));
	}

	private IEnumerator ExecuteActions(Actor i_initiator, InteractableActivationType i_activationType)
	{
		m_isExecuting = true;
		if (m_consumeRequiredItem)
		{
			PickUpable required = FindInventoryItem(m_requiredItem);
			if (required == null)
			{
				Notify("The required item is no longer available.");
				m_isExecuting = false;
				yield break;
			}
			Inventory inventory = CommonReferences.Instance.GetPlayerController().GetInventory();
			if (required.GetIsStackable()) inventory.RemovePickUpable(required, 1);
			else
			{
				Player player = CommonReferences.Instance.GetPlayer();
				if (required is Weapon && player != null && player.GetEquippableEquipped() == required)
					player.UnEquipEquippedWeapon();
				inventory.RemovePickUpable(required);
				UnityEngine.Object.Destroy(required.gameObject);
			}
		}
		if (m_delaySeconds > 0f) yield return new WaitForSeconds(m_delaySeconds);
		if (m_restoreHealth > 0f && i_initiator != null) i_initiator.RestoreHealth(m_restoreHealth);
		if (i_initiator != null && m_knockback.sqrMagnitude > 0f)
		{
			Rigidbody2D body = i_initiator.GetComponent<Rigidbody2D>();
			if (body != null && body.bodyType != RigidbodyType2D.Static) body.AddForce(m_knockback, ForceMode2D.Impulse);
		}
		if (m_ragdollSeconds > 0f && i_initiator != null) i_initiator.Ragdoll(m_ragdollSeconds);
		if (m_giveMoney > 0) CommonReferences.Instance.GetPlayerController().GainMoney(m_giveMoney);
		else if (m_giveMoney < 0) CommonReferences.Instance.GetPlayerController().LoseMoney(-m_giveMoney);
		foreach (Door door in m_doors)
		{
			if (m_doorAction == "open") door.Open();
			else if (m_doorAction == "close") door.Close();
			else door.OpenOrClose();
		}
		foreach (DoorRoller door in m_rollerDoors)
		{
			if (m_doorAction == "open") door.Open();
			else if (m_doorAction == "close") door.Close();
			else door.OpenOrClose();
		}
		foreach (LightBulb light in m_lights) light.ApplyModAction(m_lightAction);
		if (m_stage != null && m_spawnCount > 0)
			foreach (Spawner spawner in m_stage.GetComponentsInChildren<Spawner>(includeInactive: true))
				foreach (string id in m_spawnerIds)
					if (spawner.gameObject.name == "mod-spawner_" + id)
						for (int count = 0; count < m_spawnCount; count++) spawner.AddSpawnOneAmount();
		GrantItem();
		Notify(m_message);
		m_hasCompletedActions = true;
		foreach (ModMapInteraction interaction in m_chainTargets)
			if (interaction != null && !interaction.m_isExecuting)
				interaction.Activate(i_initiator, InteractableActivationType.Operator);
		m_isExecuting = false;
	}

	private bool ConditionsAvailable(bool showMessage)
	{
		int wave = m_stage != null && m_stage.GetManagerWave() != null ? m_stage.GetManagerWave().GetNumWaveCurrent() : 0;
		if (wave < m_minWave || (m_maxWave > 0 && wave > m_maxWave))
		{
			if (showMessage) Notify("This interaction is unavailable during the current wave.");
			return false;
		}
		if (m_requiredItem.HasValue && FindInventoryItem(m_requiredItem) == null)
		{
			if (showMessage) Notify("A required item is missing.");
			return false;
		}
		Inventory playerInventory = CommonReferences.Instance.GetPlayerController().GetInventory();
		if (m_requiredMoney > 0 && (playerInventory == null || playerInventory.GetMoney() < m_requiredMoney))
		{
			if (showMessage) Notify("This interaction requires at least " + m_requiredMoney + "$.");
			return false;
		}
		foreach (ModMapInteraction required in m_requiredInteractions)
			if (required == null || !required.HasCompletedActivation)
			{
				if (showMessage) Notify("Another step must be completed first.");
				return false;
			}
		int activeEnemies = CountActiveEnemies();
		if (activeEnemies < m_minEnemies || (m_maxEnemies > 0 && activeEnemies > m_maxEnemies))
		{
			if (showMessage) Notify("This interaction requires a different number of living enemies.");
			return false;
		}
		foreach (ContentId challenge in m_requiredChallenges)
			if (!IsChallengeComplete(challenge))
			{
				if (showMessage) Notify("A required challenge has not been completed.");
				return false;
			}
		if (m_giveItem.HasValue)
		{
			if (!TryGetItemTemplate(m_giveItem.Value, out PickUpable reward))
			{
				if (showMessage) Notify("The configured reward is unavailable.");
				return false;
			}
			Inventory inventory = CommonReferences.Instance.GetPlayerController().GetInventory();
			if (inventory != null && !inventory.IsHasRoomForPickUpable(reward))
			{
				if (showMessage) Notify("Not enough inventory space for the reward.");
				return false;
			}
		}
		return true;
	}

	private int CountActiveEnemies()
	{
		if (m_stage == null) return 0;
		int count = 0;
		foreach (NPC npc in m_stage.GetAllNPCs())
			if (npc != null && npc.gameObject.activeInHierarchy && !npc.IsDead()) count++;
		return count;
	}

	private static bool IsChallengeComplete(ContentId i_id)
	{
		if (CommonReferences.Instance == null || CommonReferences.Instance.GetManagerChallenge() == null) return false;
		foreach (Challenge challenge in CommonReferences.Instance.GetManagerChallenge().GetAllChallenges())
			if (RuntimeContentIdentity.TryResolve(challenge, out ContentId challengeId, out ContentCategory category)
				&& category == ContentCategory.Challenge && challengeId.Equals(i_id)) return challenge.GetState() != 0;
		return false;
	}

	private static PickUpable FindInventoryItem(ContentId? i_id)
	{
		if (!i_id.HasValue || CommonReferences.Instance == null) return null;
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		if (controller == null || controller.GetInventory() == null) return null;
		foreach (PickUpable item in controller.GetInventory().GetAllPickUpables())
			if (RuntimeContentIdentity.TryResolve(item, out ContentId itemId, out ContentCategory category)
				&& category == ContentCategory.Item && itemId.Equals(i_id.Value)) return item;
		return null;
	}

	private static bool TryGetItemTemplate(ContentId i_id, out PickUpable o_template)
	{
		o_template = null;
		return ModLoaderRuntime.Registry.TryGet(i_id, out ContentRegistration entry)
			&& (o_template = entry.RuntimeAsset as PickUpable) != null;
	}

	private void GrantItem()
	{
		if (!m_giveItem.HasValue || !TryGetItemTemplate(m_giveItem.Value, out PickUpable template)) return;
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		Player player = controller == null ? null : controller.GetPlayer();
		if (player == null) return;
		if (!template.GetIsStackable() && m_giveItemAmount != 1)
		{
			Notify("This reward cannot be granted in a stack.");
			return;
		}
		PickUpable granted = UnityEngine.Object.Instantiate(template, player.transform.parent);
		if (granted.GetIsStackable() && m_giveItemAmount > 1) granted.IncreaseAmount(m_giveItemAmount - 1);
		player.PickUp(granted, i_isDuplicate: false);
	}

	private static void Notify(string i_message)
	{
		if (!string.IsNullOrEmpty(i_message) && CommonReferences.Instance != null
			&& CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(i_message,
				ColorTextNotification.Other, i_isContinues: false);
	}
}

/// <summary>Swaps a pack-local interaction sprite after its successful one-time activation.</summary>
public sealed class ModMapInteractionVisual : MonoBehaviour
{
	private ModMapInteraction m_interaction;
	private SpriteRenderer m_renderer;
	private Sprite m_normal;
	private Sprite m_activated;

	public void Configure(ModMapInteraction i_interaction, SpriteRenderer i_renderer, Sprite i_normal, Sprite i_activated)
	{
		m_interaction = i_interaction;
		m_renderer = i_renderer;
		m_normal = i_normal;
		m_activated = i_activated;
		if (m_renderer != null) m_renderer.sprite = m_normal;
		if (m_interaction != null) m_interaction.OnActivate += HandleActivation;
	}

	public void RestoreCloneSubscription()
	{
		if (m_interaction != null) m_interaction.OnActivate += HandleActivation;
	}

	private void HandleActivation(Interactable i_interactable)
	{
		if (m_renderer != null && m_activated != null) m_renderer.sprite = m_activated;
	}

	private void OnDestroy()
	{
		if (m_interaction != null) m_interaction.OnActivate -= HandleActivation;
	}
}

/// <summary>Fades an authored 2D light toward its specified intensity while any actor is nearby.</summary>
public sealed class ModProximityLight : MonoBehaviour
{
	private UnityEngine.Rendering.Universal.Light2D m_light;
	private float m_radius;
	private float m_fadeSeconds;
	private float m_intensity;
	private float m_target;
	private Coroutine m_fade;

	public void Configure(UnityEngine.Rendering.Universal.Light2D i_light, float i_radius, float i_fadeSeconds, float i_intensity)
	{
		m_light = i_light;
		m_radius = i_radius;
		m_fadeSeconds = i_fadeSeconds;
		m_intensity = i_intensity;
		m_target = 0f;
	}

	private void OnEnable()
	{
		if (m_light != null) m_light.intensity = 0f;
		StartCoroutine(CheckActors());
	}

	private void OnDisable()
	{
		if (m_fade != null) { StopCoroutine(m_fade); m_fade = null; }
		StopAllCoroutines();
		m_target = 0f;
		if (m_light != null) m_light.intensity = 0f;
	}

	private IEnumerator CheckActors()
	{
		while (true)
		{
			bool nearby = false;
			if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerStages() != null)
			{
				Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
				if (stage != null)
					foreach (Actor actor in stage.GetAllActors())
						if (actor != null && Vector2.Distance(actor.GetPosHips(), transform.position) <= m_radius)
						{ nearby = true; break; }
			}
			float target = nearby ? m_intensity : 0f;
			if (!Mathf.Approximately(target, m_target))
			{
				m_target = target;
				if (m_fade != null) StopCoroutine(m_fade);
				m_fade = StartCoroutine(FadeTo(target));
			}
			yield return new WaitForSeconds(0.25f);
		}
	}

	private IEnumerator FadeTo(float i_target)
	{
		float from = m_light.intensity;
		float elapsed = 0f;
		while (elapsed < m_fadeSeconds)
		{
			elapsed += Time.fixedDeltaTime;
			float fraction = Mathf.Clamp01(elapsed / m_fadeSeconds);
			m_light.intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Smooth,
				AnimationTools.Transition.Steep, from, i_target, fraction);
			yield return new WaitForFixedUpdate();
		}
		m_light.intensity = i_target;
		m_fade = null;
	}
}

/// <summary>Recreates the two-phase AlarmLight intensity loop for a JSON-authored point light.</summary>
public sealed class ModPulsingLight : MonoBehaviour
{
	private UnityEngine.Rendering.Universal.Light2D m_light;
	private float m_from;
	private float m_to;
	private float m_seconds;

	public void Configure(UnityEngine.Rendering.Universal.Light2D i_light, float i_from, float i_to, float i_seconds)
	{
		m_light = i_light;
		m_from = i_from;
		m_to = i_to;
		m_seconds = i_seconds;
	}

	private void OnEnable()
	{
		if (m_light != null) StartCoroutine(Pulse());
	}

	private IEnumerator Pulse()
	{
		while (true)
		{
			float elapsed = 0f;
			while (elapsed < m_seconds)
			{
				elapsed += Time.fixedDeltaTime;
				m_light.intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep,
					AnimationTools.Transition.Smooth, m_from, m_to, elapsed / m_seconds);
				yield return new WaitForFixedUpdate();
			}
			float returnFrom = m_light.intensity;
			elapsed = 0f;
			while (elapsed < m_seconds)
			{
				elapsed += Time.fixedDeltaTime;
				m_light.intensity = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep,
					AnimationTools.Transition.Smooth, returnFrom, m_from, elapsed / m_seconds);
				yield return new WaitForFixedUpdate();
			}
		}
	}
}

public sealed class ModMovingPlatform : MonoBehaviour
{
	private Rigidbody2D m_body;
	private Vector2 m_startLocal;
	private Vector2 m_endLocal;
	private float m_travelSeconds;
	private float m_pauseSeconds;
	private float m_elapsed;
	private bool m_towardEnd = true;
	private float m_pauseRemaining;

	public void Configure(Vector2 i_offset, float i_travelSeconds, float i_pauseSeconds)
	{
		m_body = GetComponent<Rigidbody2D>();
		m_startLocal = transform.localPosition;
		m_endLocal = m_startLocal + i_offset;
		m_travelSeconds = i_travelSeconds;
		m_pauseSeconds = i_pauseSeconds;
		m_elapsed = 0f;
	}

	private void FixedUpdate()
	{
		if (m_body == null || transform.parent == null) return;
		if (m_pauseRemaining > 0f)
		{
			m_pauseRemaining -= Time.fixedDeltaTime;
			return;
		}
		m_elapsed += Time.fixedDeltaTime;
		float progress = Mathf.Clamp01(m_elapsed / m_travelSeconds);
		Vector2 from = m_towardEnd ? m_startLocal : m_endLocal;
		Vector2 to = m_towardEnd ? m_endLocal : m_startLocal;
		Vector2 localPosition = Vector2.Lerp(from, to, Mathf.SmoothStep(0f, 1f, progress));
		m_body.MovePosition(transform.parent.TransformPoint(localPosition));
		if (progress >= 1f)
		{
			m_towardEnd = !m_towardEnd;
			m_elapsed = 0f;
			m_pauseRemaining = m_pauseSeconds;
		}
	}
}

public sealed class ModMapAudioSource : MonoBehaviour
{
	private TiledAudioDefinition m_definition;
	private string m_source;
	private AudioSource m_audio;
	private bool m_loading;
	private bool m_playRequested;

	public void Configure(TiledAudioDefinition i_definition, string i_source)
	{
		m_definition = i_definition;
		m_source = i_source;
		m_playRequested = i_definition.PlayOnStart;
	}

	private void OnEnable()
	{
		if (m_audio != null && m_audio.clip != null) { if (m_playRequested) m_audio.Play(); }
		else if (!m_loading && m_definition != null) StartCoroutine(LoadAudio());
	}

	private void OnDisable()
	{
		if (m_audio != null) m_audio.Stop();
		if (m_definition != null) m_playRequested = m_definition.PlayOnStart;
	}

	public void RequestPlay()
	{
		m_playRequested = true;
		if (m_audio != null && m_audio.clip != null) m_audio.Play();
		else if (!m_loading && m_definition != null && isActiveAndEnabled) StartCoroutine(LoadAudio());
	}

	public void RequestStop()
	{
		m_playRequested = false;
		if (m_audio != null) m_audio.Stop();
	}

	private IEnumerator LoadAudio()
	{
		m_loading = true;
		string fullPath;
		try { fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(m_definition.AssetRoot, m_definition.FilePath)); }
		catch (Exception exception)
		{
			Report("stage.factory-audio-path", exception.Message, m_source);
			m_loading = false;
			yield break;
		}
		AudioType type = System.IO.Path.GetExtension(fullPath).Equals(".wav", StringComparison.OrdinalIgnoreCase)
			? AudioType.WAV : AudioType.OGGVORBIS;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(fullPath).AbsoluteUri, type))
		{
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				Report("stage.factory-audio-decode", request.error + ": " + m_definition.FilePath, m_source);
				m_loading = false;
				yield break;
			}
			AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
			if (clip == null)
			{
				Report("stage.factory-audio-decode", "Unity returned no audio clip for " + m_definition.FilePath + ".", m_source);
				m_loading = false;
				yield break;
			}
			clip.name = m_definition.FilePath;
			m_audio = gameObject.AddComponent<AudioSource>();
			m_audio.clip = clip;
			m_audio.loop = m_definition.Loop;
			m_audio.volume = m_definition.Volume;
			m_audio.playOnAwake = false;
			m_audio.spatialBlend = m_definition.Ambient ? 0f : 1f;
			m_audio.minDistance = m_definition.MinDistance;
			m_audio.maxDistance = m_definition.MaxDistance;
			m_audio.rolloffMode = AudioRolloffMode.Linear;
			m_audio.dopplerLevel = 0f;
			if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerAudio() != null)
			{
				var groups = CommonReferences.Instance.GetManagerAudio().GetAudioMixer().FindMatchingGroups(m_definition.MixerGroup);
				if (groups.Length > 0) m_audio.outputAudioMixerGroup = groups[0];
			}
			if (isActiveAndEnabled && m_playRequested) m_audio.Play();
		}
		m_loading = false;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}

public sealed class ModAnimatedSprite : MonoBehaviour
{
	private SpriteRenderer m_renderer;
	private IReadOnlyList<Sprite> m_frames;
	private IReadOnlyList<float> m_durations;
	private int m_index;
	private float m_remaining;

	public void Configure(SpriteRenderer i_renderer, IReadOnlyList<Sprite> i_frames, IReadOnlyList<float> i_durations)
	{
		m_renderer = i_renderer;
		m_frames = i_frames;
		m_durations = i_durations;
		m_index = 0;
		m_remaining = i_durations.Count > 0 ? i_durations[0] : 0f;
	}

	private void Update()
	{
		if (m_renderer == null || m_frames == null || m_frames.Count < 2) return;
		m_remaining -= Time.deltaTime;
		while (m_remaining <= 0f)
		{
			m_index = (m_index + 1) % m_frames.Count;
			m_renderer.sprite = m_frames[m_index];
			m_remaining += m_durations[m_index];
		}
	}
}

public sealed class ModParallaxLayer : MonoBehaviour
{
	private Vector2 m_factor = Vector2.one;
	private Vector3 m_layerOrigin;
	private Vector3 m_cameraOrigin;
	private Transform m_camera;
	private bool m_initialized;

	public void Configure(float i_x, float i_y) { m_factor = new Vector2(i_x, i_y); }

	private void LateUpdate()
	{
		if (!m_initialized)
		{
			CameraXGame camera = CommonReferences.Instance == null || CommonReferences.Instance.GetManagerCamerasXGame() == null
				? null : CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent();
			if (camera == null) return;
			m_camera = camera.transform;
			m_layerOrigin = transform.position;
			m_cameraOrigin = m_camera.position;
			m_initialized = true;
		}
		Vector3 cameraDelta = m_camera.position - m_cameraOrigin;
		transform.position = m_layerOrigin + new Vector3(cameraDelta.x * (1f - m_factor.x),
			cameraDelta.y * (1f - m_factor.y), 0f);
	}
}

public sealed class ModRoomController : MonoBehaviour
{
	private sealed class RoomEntry
	{
		public string RoomId;
		public Vector2 LocalPosition;
	}

	private readonly Dictionary<string, Rect> m_rooms = new Dictionary<string, Rect>(StringComparer.Ordinal);
	private readonly Dictionary<string, RoomEntry> m_entries = new Dictionary<string, RoomEntry>(StringComparer.Ordinal);
	private Stage m_stage;
	private string m_initialRoom;
	private string m_currentRoom;
	private float m_transitionCooldownUntil;

	public void Configure(Stage i_stage) { m_stage = i_stage; }

	public void RegisterRoom(string i_id, Rect i_localBounds, bool i_initial)
	{
		m_rooms[i_id] = i_localBounds;
		if (i_initial) m_initialRoom = i_id;
	}

	public void RegisterEntry(string i_id, string i_roomId, Vector2 i_localPosition)
	{
		m_entries[i_id] = new RoomEntry { RoomId = i_roomId, LocalPosition = i_localPosition };
	}

	private IEnumerator Start()
	{
		yield return null;
		if (!string.IsNullOrEmpty(m_initialRoom))
		{
			EnterRoom(m_initialRoom);
			yield break;
		}
		Player player = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayer();
		if (player == null || m_stage == null) yield break;
		Vector2 local = m_stage.transform.InverseTransformPoint(player.GetPos());
		foreach (KeyValuePair<string, Rect> room in m_rooms)
			if (room.Value.Contains(local))
			{
				EnterRoom(room.Key);
				break;
			}
	}

	public void EnterRoom(string i_roomId)
	{
		if (i_roomId == m_currentRoom || m_stage == null || !m_rooms.TryGetValue(i_roomId, out Rect localBounds)) return;
		m_currentRoom = i_roomId;
		Vector2 origin = m_stage.transform.position;
		Rect worldBounds = new Rect(origin.x + localBounds.x, origin.y + localBounds.y,
			localBounds.width, localBounds.height);
		CameraXGame camera = CommonReferences.Instance?.GetManagerCamerasXGame()?.GetCameraXGameCurrent();
		if (camera != null) camera.SetWorldBounds(worldBounds);
	}

	public bool Transition(Player i_player, string i_destinationId, bool i_requireNoEnemies)
	{
		if (i_player == null || m_stage == null || Time.unscaledTime < m_transitionCooldownUntil
			|| !m_entries.TryGetValue(i_destinationId, out RoomEntry entry)) return false;
		if (i_requireNoEnemies)
			foreach (NPC npc in m_stage.GetAllNPCs())
				if (npc != null && npc.gameObject.activeInHierarchy && !npc.IsDead()) return false;
		m_transitionCooldownUntil = Time.unscaledTime + 0.5f;
		i_player.PlaceFeetOnPos(m_stage.transform.TransformPoint(entry.LocalPosition));
		EnterRoom(entry.RoomId);
		CameraXGame camera = CommonReferences.Instance?.GetManagerCamerasXGame()?.GetCameraXGameCurrent();
		if (camera != null) camera.CenterCamera();
		return true;
	}
}

public sealed class ModRoomVolume : MonoBehaviour
{
	private ModRoomController m_controller;
	private string m_roomId;
	public void Configure(ModRoomController i_controller, string i_roomId)
	{
		m_controller = i_controller;
		m_roomId = i_roomId;
	}
	private void OnTriggerEnter2D(Collider2D i_other)
	{
		if (i_other.GetComponentInParent<Player>() != null) m_controller?.EnterRoom(m_roomId);
	}
}

public sealed class ModRoomTransition : MonoBehaviour
{
	private ModRoomController m_controller;
	private string m_destinationId;
	private bool m_requireNoEnemies;
	private bool m_oneShot;
	private bool m_used;
	public void Configure(ModRoomController i_controller, string i_destinationId, bool i_requireNoEnemies, bool i_oneShot)
	{
		m_controller = i_controller;
		m_destinationId = i_destinationId;
		m_requireNoEnemies = i_requireNoEnemies;
		m_oneShot = i_oneShot;
	}
	private void OnTriggerEnter2D(Collider2D i_other)
	{
		if (m_used) return;
		Player player = i_other.GetComponentInParent<Player>();
		if (player != null && m_controller != null
			&& m_controller.Transition(player, m_destinationId, m_requireNoEnemies) && m_oneShot)
			m_used = true;
	}
}

public sealed class ExternalStageFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(ManagerStages i_manager)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterEnemyFactories(i_manager));
	}

	private IEnumerator BuildAfterEnemyFactories(ManagerStages i_manager)
	{
		yield return null;
		yield return null;
		ExternalStageFactory.Build(i_manager);
	}
}

/// <summary>
/// Opt-in developer HUD used only by stages declaring testTools. It drives the
/// real modular enemy and finisher components instead of a parallel simulation.
/// </summary>
public sealed class ModFinisherTestHarness : MonoBehaviour
{
	private Stage m_stage;
	private string m_enemyId;
	private Vector2 m_spawnPosition;
	private string m_message = "Ready";
	private bool m_collapsed;

	public void Configure(Stage i_stage, StageTestToolsDefinition i_tools)
	{
		m_stage = i_stage;
		m_enemyId = i_tools?.Enemy;
		StagePointDefinition point = i_tools?.SpawnPosition;
		m_spawnPosition = point == null ? new Vector2(5f, 0f) : new Vector2(point.X, point.Y);
	}

	private void OnGUI()
	{
		if (m_stage == null || CommonReferences.Instance == null
			|| CommonReferences.Instance.GetManagerStages()?.GetStageCurrent() != m_stage) return;
		GUI.depth = -500;
		float width = Mathf.Min(390f, UnityEngine.Screen.width - 20f);
		GUILayout.BeginArea(new Rect(UnityEngine.Screen.width - width - 10f, 10f, width, m_collapsed ? 48f : 330f), GUI.skin.box);
		using (new GUILayout.HorizontalScope())
		{
			GUILayout.Label("MOD FINISHER TEST", GUILayout.ExpandWidth(true));
			if (GUILayout.Button(m_collapsed ? "+" : "-", GUILayout.Width(28f))) m_collapsed = !m_collapsed;
		}
		if (!m_collapsed)
		{
			GUILayout.Label(m_enemyId ?? "No enemy configured");
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Spawn Enemy")) SpawnEnemy();
				if (GUILayout.Button("Reset Test")) ResetTest();
			}
			using (new GUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Knock Down Player")) KnockDownPlayer();
				if (GUILayout.Button("Expose Player")) ExposePlayer();
			}
			if (GUILayout.Button("Start Finisher Immediately")) StartFinisher();
			GUILayout.Space(5f);
			Player player = CommonReferences.Instance.GetPlayer();
			ModularEnemyFinisher finisher = FindNearestFinisher(player);
			GUILayout.Label("Player: " + (player == null ? "missing" : player.GetStateActorCurrent().ToString()));
			if (finisher == null) GUILayout.Label("Finisher: no matching live enemy");
			else
			{
				GUILayout.Label("Active: " + finisher.GetIsActive() + "   Outcome: " + finisher.GetLastOutcome());
				GUILayout.Label("Phase " + (finisher.GetPhaseIndex() + 1) + ": " + finisher.GetPhaseId());
				GUILayout.Label("Enemy clip: " + finisher.GetEnemyAnimationName());
				GUILayout.Label("Player clip: " + finisher.GetPlayerAnimationReference());
				GUILayout.Label("NPC participants: " + finisher.GetParticipantCount() + " joined, " + finisher.GetOpenParticipantSlotCount() + " open");
				GUILayout.Label("Escape: " + finisher.GetMeterCurrent().ToString("0.0") + " / " + finisher.GetMeterMax().ToString("0.0")
					+ "   Time: " + finisher.GetTimeLeft().ToString("0.0") + "s");
			}
			GUILayout.Space(4f); GUILayout.Label(m_message);
		}
		GUILayout.EndArea();
	}

	private NPC SpawnEnemy()
	{
		if (!ContentId.TryParse(m_enemyId, out ContentId id) || !ModLoaderRuntime.Registry.TryGet(id, out ContentRegistration entry)
			|| !(entry.RuntimeAsset is NPC template)) { m_message = "Enemy runtime asset is unavailable."; return null; }
		Transform parent = m_stage.GetActorsParent(); NPC spawned = UnityEngine.Object.Instantiate(template, parent);
		Vector3 world = m_stage.transform.TransformPoint(m_spawnPosition); spawned.transform.position = world; spawned.gameObject.SetActive(true); spawned.Spawn(true);
		if (spawned is Walker) spawned.PlaceFeetOnPos(world); m_message = "Spawned " + m_enemyId + "."; return spawned;
	}

	private void KnockDownPlayer()
	{
		Player player = CommonReferences.Instance.GetPlayer(); if (player == null) { m_message = "Player is unavailable."; return; }
		player.SetIsExposing(false); player.Ragdoll(12f); m_message = "Player knocked down for 12 seconds.";
	}

	private void ExposePlayer()
	{
		Player player = CommonReferences.Instance.GetPlayer(); if (player == null) { m_message = "Player is unavailable."; return; }
		if (player.GetStateActorCurrent() == StateActor.Ragdoll) player.InterruptRagdoll(); player.SetIsExposing(true); m_message = "Player Expose enabled.";
	}

	private void StartFinisher()
	{
		Player player = CommonReferences.Instance.GetPlayer(); if (player == null) { m_message = "Player is unavailable."; return; }
		ModularEnemyFinisher finisher = FindNearestFinisher(player);
		if (finisher == null) { NPC spawned = SpawnEnemy(); finisher = spawned == null ? null : spawned.GetComponent<ModularEnemyFinisher>(); }
		if (finisher == null) { m_message = "The selected enemy has no configured finisher."; return; }
		if (!finisher.TryBeginFromAttack(player)) m_message = "Finisher could not start; reset the actors and try again.";
		else m_message = "Finisher started through the runtime component.";
	}

	private void ResetTest()
	{
		foreach (NPC npc in m_stage.GetAllNPCs().ToArray())
		{
			if (npc == null || !npc.gameObject.activeInHierarchy || !MatchesEnemy(npc)) continue;
			npc.GetComponent<ModularEnemyFinisher>()?.CancelForTest(); UnityEngine.Object.Destroy(npc.gameObject);
		}
		Player player = CommonReferences.Instance.GetPlayer();
		if (player != null) { if (player.GetStateActorCurrent() == StateActor.Ragdoll) player.InterruptRagdoll(); player.SetIsExposing(false); }
		m_message = "Test actors and player state reset.";
	}

	private ModularEnemyFinisher FindNearestFinisher(Player i_player)
	{
		ModularEnemyFinisher best = null; float distance = float.PositiveInfinity;
		foreach (NPC npc in m_stage.GetAllNPCs())
		{
			if (npc == null || !npc.gameObject.activeInHierarchy || npc.IsDead() || !MatchesEnemy(npc)) continue;
			ModularEnemyFinisher candidate = npc.GetComponent<ModularEnemyFinisher>(); if (candidate == null) continue;
			float current = i_player == null ? 0f : Vector2.Distance(npc.GetPosFeet(), i_player.GetPosHips());
			if (current < distance) { distance = current; best = candidate; }
		}
		return best;
	}

	private bool MatchesEnemy(NPC i_npc)
	{
		RuntimeContentIdentity identity = i_npc == null ? null : i_npc.GetComponent<RuntimeContentIdentity>();
		return identity != null && identity.TryGetContentId(out ContentId id) && id.ToString() == m_enemyId;
	}
}

public sealed class ModStageAudioLoader : MonoBehaviour
{
	public void Load(Stage i_stage, StageDefinition i_definition)
	{
		if (i_stage == null || i_definition == null || i_definition.Audio == null) return;
		BeginSlot(i_stage, i_definition, "ambience", i_definition.Audio.Ambience);
		BeginSlot(i_stage, i_definition, "entryMusic", i_definition.Audio.EntryMusic);
		BeginSlot(i_stage, i_definition, "waveMusic", i_definition.Audio.WaveMusic);
		BeginSlot(i_stage, i_definition, "waveComplete", i_definition.Audio.WaveComplete);
	}

	private void BeginSlot(Stage i_stage, StageDefinition i_definition, string i_slot, string i_relativePath)
	{
		if (!string.IsNullOrWhiteSpace(i_relativePath))
			StartCoroutine(LoadSlot(i_stage, i_definition, i_slot, i_relativePath));
	}

	private IEnumerator LoadSlot(Stage i_stage, StageDefinition i_definition, string i_slot, string i_relativePath)
	{
		string fullPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(i_definition.AssetRoot, i_relativePath));
		AudioType type = System.IO.Path.GetExtension(fullPath).Equals(".ogg", StringComparison.OrdinalIgnoreCase)
			? AudioType.OGGVORBIS : AudioType.WAV;
		using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(new Uri(fullPath).AbsoluteUri, type))
		{
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success)
			{
				ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, "stage.audio-load",
					"Could not load " + i_slot + " audio: " + request.error, i_definition.Source);
				yield break;
			}
			AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
			if (clip == null)
			{
				ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, "stage.audio-decode",
					"Could not decode " + i_slot + " audio: " + i_relativePath, i_definition.Source);
				yield break;
			}
			clip.name = i_definition.Id + "/audio/" + i_slot;
			if (i_stage != null) i_stage.ConfigureModAudioSlot(i_slot, clip);
		}
	}
}
