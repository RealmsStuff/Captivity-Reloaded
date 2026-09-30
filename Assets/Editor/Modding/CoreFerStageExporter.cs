using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>Exports the editable, data-owned portion of the FER prefab into the public Tiled stage format.</summary>
	public static class CoreFerStageExporter
	{
		private const float PixelsPerUnit = 32f;
		private const string PrefabPath = "Assets/Stages/stage_fer.prefab";
		private const string MainScenePath = "Assets/Scenes/Main.unity";
		private const string PackVersion = "0.3.1";

		[MenuItem("Tools/Captivity Modding/Export FER as Tiled mod")]
		public static void Export()
		{
			Scene temporaryScene = default(Scene);
			GameObject root = null;
			try
			{
				StageFER sceneStage = FindSceneFerStage(out temporaryScene);
				root = PrefabUtility.LoadPrefabContents(PrefabPath);
				WritePack(root, sceneStage.gameObject, "Mods/core-fer-tiled-port");
				WritePack(root, sceneStage.gameObject, "ExampleMods/core-fer-tiled-port");
				Debug.Log("[Modding] Exported the FER geometry, spawners, portable artwork, and vendors as core-fer-tiled-port.");
			}
			finally
			{
				if (root != null) PrefabUtility.UnloadPrefabContents(root);
				if (temporaryScene.IsValid()) EditorSceneManager.CloseScene(temporaryScene, true);
			}
		}

		private static StageFER FindSceneFerStage(out Scene o_temporaryScene)
		{
			o_temporaryScene = default(Scene);
			Scene main = SceneManager.GetSceneByPath(MainScenePath);
			if (!main.IsValid() || !main.isLoaded)
			{
				main = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Additive);
				o_temporaryScene = main;
			}
			foreach (GameObject root in main.GetRootGameObjects())
				foreach (StageFER stage in root.GetComponentsInChildren<StageFER>(true))
					if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(stage.gameObject) == PrefabPath)
						return stage;
			throw new InvalidOperationException("The Main scene does not contain the FER stage instance with its enemy overrides.");
		}

		private static void WritePack(GameObject i_root, GameObject i_sceneRoot, string i_relativeRoot)
		{
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			string packRoot = System.IO.Path.Combine(projectRoot, i_relativeRoot);
			Directory.CreateDirectory(System.IO.Path.Combine(packRoot, "content"));
			Directory.CreateDirectory(System.IO.Path.Combine(packRoot, "levels"));
			Directory.CreateDirectory(System.IO.Path.Combine(packRoot, "tilesets"));
			Directory.CreateDirectory(System.IO.Path.Combine(packRoot, "assets", "art"));
			Directory.CreateDirectory(System.IO.Path.Combine(packRoot, "assets", "audio"));
			ClearGeneratedFiles(System.IO.Path.Combine(packRoot, "tilesets"), ".tsj");
			ClearGeneratedFiles(System.IO.Path.Combine(packRoot, "assets", "art"), ".png");

			Bounds bounds = CalculateBounds(i_root);
			float margin = 2f;
			float minX = bounds.min.x - margin;
			float minY = bounds.min.y - margin;
			float widthWorld = Mathf.Ceil(bounds.size.x + margin * 2f);
			float heightWorld = Mathf.Ceil(bounds.size.y + margin * 2f);
			int widthPixels = Mathf.CeilToInt(widthWorld * PixelsPerUnit);
			int heightPixels = Mathf.CeilToInt(heightWorld * PixelsPerUnit);
			float centerX = minX + widthWorld * 0.5f;

			JArray objects = new JArray();
			JArray tilesets = new JArray();
			Dictionary<Sprite, ArtExport> exportedSprites = new Dictionary<Sprite, ArtExport>();
			uint nextGid = 1;
			int nextId = 1;
			HashSet<BoxCollider2D> exportedSolidColliders = new HashSet<BoxCollider2D>();
			foreach (Platform platform in i_root.GetComponentsInChildren<Platform>(true))
			{
				foreach (BoxCollider2D collider in platform.GetComponents<BoxCollider2D>())
				{
					exportedSolidColliders.Add(collider);
					Bounds area = collider.bounds;
					objects.Add(Rectangle(nextId++, platform.name, "platform", area, minY, centerX, widthPixels, heightPixels,
						new JProperty("climbableLeft", platform.GetIsClimbableLeftLedge()),
						new JProperty("climbableRight", platform.GetIsClimbableRightLedge())));
				}
			}
			foreach (BoxCollider2D collider in i_root.GetComponentsInChildren<BoxCollider2D>(true))
			{
				if (collider == null || exportedSolidColliders.Contains(collider) || !collider.enabled || collider.isTrigger
					|| !collider.gameObject.activeInHierarchy || collider.GetComponentInParent<Actor>() != null
					|| collider.GetComponentInParent<Item>() != null || collider.GetComponentInParent<Interactable>() != null) continue;
				string layer = LayerMask.LayerToName(collider.gameObject.layer);
				if (layer != "Platform" && layer != "InvisibleWall") continue;
				objects.Add(Rectangle(nextId++, collider.name, "platform", collider.bounds, minY, centerX,
					widthPixels, heightPixels, new JProperty("climbableLeft", false),
					new JProperty("climbableRight", false)));
			}

			Stage stage = i_root.GetComponent<Stage>();
			Vector2 spawn = stage != null && stage.GetWaypointStart() != null
				? stage.GetWaypointStart().GetPos() : Vector2.zero;
			objects.Add(Point(nextId++, "player", "player-spawn", spawn, minY, centerX, widthPixels, heightPixels));
			if (ExportLabActivationAudio(i_root, packRoot))
				objects.Add(Point(nextId++, "fer-lab-activation-audio", "ambient-audio", spawn,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("file", "assets/audio/fer-lab-activate.ogg"),
					new JProperty("loop", false), new JProperty("playOnStart", false), new JProperty("volume", 1f),
					new JProperty("minDistance", 4f), new JProperty("maxDistance", 30f)));
			ExportFuseBoxes(i_root, packRoot, objects, ref nextId, minY, centerX, widthPixels, heightPixels);
			ExportProximityLights(i_root, packRoot, objects, ref nextId, minY, centerX, widthPixels, heightPixels);
			ExportPortableFreeformLights(i_root, packRoot, objects, ref nextId, minY, centerX, widthPixels, heightPixels);
			ExportGlobalLight(i_root, objects, ref nextId, minY, centerX, widthPixels, heightPixels);
			ExportPointLights(i_root, objects, ref nextId, minY, centerX, widthPixels, heightPixels);

			HashSet<string> adapterIds = new HashSet<string>(StringComparer.Ordinal);
			Dictionary<GameObject, string> coreStageObjectIds = new Dictionary<GameObject, string>();
			ExportDoors(i_root, objects, ref nextId, minY, centerX, widthPixels, heightPixels, adapterIds, coreStageObjectIds);
			ExportScriptedActors(i_root, objects, ref nextId, minY, centerX, widthPixels, heightPixels, adapterIds, coreStageObjectIds);
			List<SpawnerExport> spawners = new List<SpawnerExport>();
			Dictionary<Spawner, string> spawnerIds = new Dictionary<Spawner, string>();
			Spawner[] sourceSpawners = i_root.GetComponentsInChildren<Spawner>(true);
			Dictionary<string, Spawner> sceneSpawners = i_sceneRoot.GetComponentsInChildren<Spawner>(true)
				.ToDictionary(spawner => RelativePath(i_sceneRoot.transform, spawner.transform), StringComparer.Ordinal);
			for (int index = 0; index < sourceSpawners.Length; index++) spawnerIds[sourceSpawners[index]] = "fer-spawner-" + index;
			List<GameObject> coreStageObjects = CollectExplicitCoreStageObjects(i_root);
			HashSet<GameObject> coreStageObjectSet = new HashSet<GameObject>(coreStageObjects);
			foreach (GameObject retained in coreStageObjects)
				coreStageObjectIds.Add(retained, UniqueScriptId(retained.name, adapterIds));
			Dictionary<Spawner, List<string>> requiredDoors = BuildRequiredDoorMap(i_root, spawnerIds, coreStageObjectIds);
			foreach (Spawner spawner in sourceSpawners)
			{
				string id = spawnerIds[spawner];
				objects.Add(Point(nextId++, id, "enemy-spawner", spawner.transform.position, minY, centerX, widthPixels, heightPixels));
				requiredDoors.TryGetValue(spawner, out List<string> gates);
				string path = RelativePath(i_root.transform, spawner.transform);
				if (!sceneSpawners.TryGetValue(path, out Spawner sceneSpawner))
					throw new InvalidOperationException("FER scene spawner was not found: " + path);
				spawners.Add(ReadSpawner(sceneSpawner, id, gates));
			}
			ExportClimbNavigation(i_root, objects, ref nextId, minY, centerX, widthPixels, heightPixels);

			foreach (Vendor vendor in i_root.GetComponentsInChildren<Vendor>(true))
			{
				string type = vendor.GetVendorType() == VendorType.Weapons ? "weapon-vendor" : "usable-vendor";
				objects.Add(Point(nextId++, type + "-" + nextId, type, vendor.transform.position, minY, centerX, widthPixels, heightPixels));
			}
			foreach (AndroidStatuePose pose in i_root.GetComponentsInChildren<AndroidStatuePose>(true))
				objects.Add(Point(nextId++, ScriptId(pose.name), "stage-marker", pose.transform.position,
					minY, centerX, widthPixels, heightPixels));
			HashSet<string> audioIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (AudioSource source in i_root.GetComponentsInChildren<AudioSource>(true))
			{
				if (!IsUnderNamedAncestor(source.transform, "Audio", i_root.transform)) continue;
				string id = UniqueScriptId(source.name, audioIds);
				string file = ExportStageAudioClip(source.clip, packRoot);
				objects.Add(Point(nextId++, id, "audio-source", source.transform.position,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("file", file), new JProperty("loop", source.loop),
					new JProperty("playOnStart", source.playOnAwake && source.enabled && source.gameObject.activeInHierarchy),
					new JProperty("mixerGroup", source.outputAudioMixerGroup == null ? "SFX" : source.outputAudioMixerGroup.name),
					new JProperty("volume", source.volume), new JProperty("minDistance", source.minDistance),
					new JProperty("maxDistance", source.maxDistance)));
			}
			Dictionary<string, string> coreWeaponIds = ReadCoreWeaponIds(projectRoot);
			Dictionary<string, WeaponCase> sceneCases = i_sceneRoot.GetComponentsInChildren<WeaponCase>(true)
				.ToDictionary(weaponCase => RelativePath(i_sceneRoot.transform, weaponCase.transform), StringComparer.Ordinal);
			foreach (WeaponCase weaponCase in i_root.GetComponentsInChildren<WeaponCase>(true))
			{
				string path = RelativePath(i_root.transform, weaponCase.transform);
				if (!sceneCases.TryGetValue(path, out WeaponCase sceneCase))
					throw new InvalidOperationException("FER scene weapon case was not found: " + path);
				SerializedObject serialized = new SerializedObject(sceneCase);
				Weapon weapon = serialized.FindProperty("m_weapon")?.objectReferenceValue as Weapon;
				if (weapon == null || !coreWeaponIds.TryGetValue(weapon.GetName(), out string weaponId))
					throw new InvalidOperationException("FER weapon case has no recognized Core weapon: " + path);
				objects.Add(Point(nextId++, ScriptId(weaponCase.name), "weapon-case", weaponCase.transform.position,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("weapon", weaponId), new JProperty("caseSize", ReadInt(serialized, "m_weaponSize", 1))));
			}
			foreach (Weapon weapon in i_root.GetComponentsInChildren<Weapon>(true))
			{
				if (!IsUnderNamedAncestor(weapon.transform, "Items", i_root.transform)) continue;
				if (!coreWeaponIds.TryGetValue(weapon.GetName(), out string weaponId))
					throw new InvalidOperationException("FER world weapon has no recognized Core ID: " + weapon.GetName());
				objects.Add(Point(nextId++, ScriptId(weapon.name), "pickup", weapon.transform.position,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("item", weaponId), new JProperty("amount", 1),
					new JProperty("initiallyKinematic", weapon.name == "Schockgewehr")));
			}
			foreach (PickUpable pickup in i_root.GetComponentsInChildren<PickUpable>(true))
			{
				if (pickup is Weapon || !IsUnderNamedAncestor(pickup.transform, "Items", i_root.transform)) continue;
				string artFile = ExportStageItemTexture(pickup.GetSpriteIcon(), packRoot, ScriptId(pickup.name));
				BoxCollider2D collider = pickup.GetComponent<BoxCollider2D>();
				UnityEngine.Rendering.Universal.Light2D itemLight = pickup.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
				Vector2 colliderSize = collider == null ? new Vector2(0.75f, 0.75f) : collider.size;
				JObject itemPoint = Point(nextId++, ScriptId(pickup.name), "stage-item", pickup.transform.position,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("file", artFile), new JProperty("displayName", pickup.GetName()),
					new JProperty("description", pickup.GetDescription()), new JProperty("onPickupSignal", "fer-keycard-picked-up"),
					new JProperty("pixelsPerUnit", 32f), new JProperty("pivotX", 0.5f), new JProperty("pivotY", 0.5f),
					new JProperty("colliderWidth", colliderSize.x), new JProperty("colliderHeight", colliderSize.y),
					new JProperty("colliderOffsetX", collider == null ? 0f : collider.offset.x),
					new JProperty("colliderOffsetY", collider == null ? 0f : collider.offset.y),
					new JProperty("weight", pickup.GetWeight()), new JProperty("value", pickup.GetValue()),
					new JProperty("canDrop", pickup.GetIsCanDrop()),
					new JProperty("initiallyKinematic", true));
				if (itemLight != null)
				{
					AddProperty(itemPoint, "lightColor", "#" + ColorUtility.ToHtmlStringRGB(itemLight.color));
					AddProperty(itemPoint, "lightOffsetX", itemLight.transform.localPosition.x);
					AddProperty(itemPoint, "lightOffsetY", itemLight.transform.localPosition.y);
					AddProperty(itemPoint, "lightRadius", itemLight.shapeLightParametricRadius);
					AddProperty(itemPoint, "lightFalloffSize", itemLight.shapeLightFalloffSize);
					AddProperty(itemPoint, "lightSides", itemLight.shapeLightParametricSides);
					AddProperty(itemPoint, "lightIntensity", itemLight.intensity);
					AddProperty(itemPoint, "lightFalloffIntensity", itemLight.falloffIntensity);
				}
				objects.Add(itemPoint);
			}

			foreach (GameObject retained in coreStageObjects)
			{
				string id = coreStageObjectIds[retained];
				objects.Add(Point(nextId++, id, "core-prop", retained.transform.position,
					minY, centerX, widthPixels, heightPixels,
					new JProperty("asset", "core:stage-prop/fer/" + id),
					new JProperty("initiallyActive", retained.activeSelf)));
			}

			foreach (SpriteRenderer renderer in i_root.GetComponentsInChildren<SpriteRenderer>(true))
			{
				if (renderer.sprite == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy
					|| renderer.name.StartsWith("lightBulb", StringComparison.OrdinalIgnoreCase)
					|| HasSelectedAncestor(renderer.transform, coreStageObjectSet)
					|| renderer.GetComponentInParent<Actor>() != null
					|| HasSkeletonHierarchy(renderer.transform)
					|| renderer.GetComponentInParent<Item>() != null || renderer.GetComponentInParent<Interactable>() != null) continue;
				if (!exportedSprites.TryGetValue(renderer.sprite, out ArtExport art))
				{
					if (!TryExportSprite(renderer.sprite, packRoot, exportedSprites.Count, nextGid, out art)) continue;
					exportedSprites.Add(renderer.sprite, art);
					tilesets.Add(new JObject(new JProperty("firstgid", art.FirstGid),
						new JProperty("source", "../tilesets/" + art.FileStem + ".tsj")));
					nextGid++;
				}
				objects.Add(Decoration(nextId++, renderer, art, minY, centerX, widthPixels, heightPixels));
			}

			JObject map = new JObject(
				new JProperty("compressionlevel", -1), new JProperty("height", Mathf.CeilToInt(heightWorld)),
				new JProperty("infinite", false), new JProperty("layers", new JArray(new JObject(
					new JProperty("draworder", "topdown"), new JProperty("id", 1), new JProperty("name", "FER Port"),
					new JProperty("objects", objects), new JProperty("opacity", 1), new JProperty("type", "objectgroup"),
					new JProperty("visible", true), new JProperty("x", 0), new JProperty("y", 0)))),
				new JProperty("nextlayerid", 2), new JProperty("nextobjectid", nextId), new JProperty("orientation", "orthogonal"),
				new JProperty("renderorder", "right-down"), new JProperty("tiledversion", "1.10.2"),
				new JProperty("tileheight", 32), new JProperty("tilesets", tilesets), new JProperty("tilewidth", 32),
				new JProperty("type", "map"), new JProperty("version", "1.10"),
				new JProperty("width", Mathf.CeilToInt(widthWorld)));

			JArray spawnerJson = new JArray();
			foreach (SpawnerExport value in spawners) spawnerJson.Add(value.ToJson());
			JObject definition = new JObject(
				new JProperty("schemaVersion", 1), new JProperty("type", "stage"),
				new JProperty("id", "example.core-fer-tiled-port:stage/fer-tiled-port"),
				new JProperty("displayName", "FER (Tiled Port)"),
				new JProperty("extends", "core:stage/furry-entertainment-robotics"),
				new JProperty("description", "FER geometry, artwork, spawners, vendors, encounters, doors, weapon cases, fuses, display actors, and stage-specific sequences represented explicitly in Tiled and stage-script JSON."),
				new JProperty("layout", new JObject(new JProperty("type", "tiledJson"),
					new JProperty("path", "levels/fer-port.json"), new JProperty("pixelsPerUnit", 32),
					new JProperty("hideInheritedVisuals", true), new JProperty("preserveInheritedStageObjects", false),
					new JProperty("reuseInheritedSpawners", false))),
				new JProperty("camera", new JObject(new JProperty("unbounded", true))),
				new JProperty("waves", new JObject(new JProperty("firstWaveEnemyCount", 12))),
				new JProperty("spawners", spawnerJson));

			JObject manifest = new JObject(new JProperty("schemaVersion", 1),
				new JProperty("id", "example.core-fer-tiled-port"), new JProperty("displayName", "FER Tiled Port"),
				new JProperty("version", PackVersion), new JProperty("modApiVersion", 1),
				new JProperty("authors", new JArray("Captivity Reloaded contributors")),
				new JProperty("description", "Editable JSON/Tiled migration of Furry Entertainment Robotics."),
				new JProperty("dependencies", new JArray(new JObject(new JProperty("id", "core"), new JProperty("version", ">=0.1.0")))),
				new JProperty("conflicts", new JArray()), new JProperty("contentRoots", new JArray("content")));

			File.WriteAllText(System.IO.Path.Combine(packRoot, "manifest.json"), manifest.ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "content", "stage.json"), definition.ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "content", "fer-lab.stage-script.json"), BuildLabStageScript(i_root, coreStageObjectIds).ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "content", "fer-statues.stage-script.json"), BuildStatueStageScript(i_root, coreStageObjectIds).ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "content", "fer-encounters.stage-script.json"), BuildEncounterStageScript(i_root, spawnerIds, coreStageObjectIds).ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "content", "fer-jacky-curse.stage-script.json"), BuildJackyCurseStageScript(i_root).ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "levels", "fer-port.json"), map.ToString(Formatting.Indented));
			File.WriteAllText(System.IO.Path.Combine(packRoot, "README.md"),
				"# FER Tiled port\n\nOpen `levels/fer-port.json` in Tiled. Geometry, spawn points, statue pose markers, visible static artwork, sprite transforms, sorting, tint, original ledge climbability, vendors, doors, cases, fuses, actor-proximity lights, display actors, atmospheric particles, positional audio, animated props, decorative character rigs, and other FER gameplay objects are explicit map entries. Complex animated presentation objects use stable `core:stage-prop/fer/...` asset IDs, so the mod contains no fragile inherited hierarchy paths. `content/fer-lab.stage-script.json` recreates the three-fuse laboratory activation and completion cue. `fer-statues.stage-script.json`, `fer-encounters.stage-script.json`, and `fer-jacky-curse.stage-script.json` own the statue, encounter, and curse sequences. Re-export from Unity with `Tools > Captivity Modding > Export FER as Tiled mod`.\n");
		}

		private static void ExportDoors(GameObject i_root, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels,
			HashSet<string> io_ids, Dictionary<GameObject, string> io_objectIds)
		{
			foreach (Door door in i_root.GetComponentsInChildren<Door>(true))
			{
				string id = UniqueScriptId(door.name, io_ids);
				io_objectIds.Add(door.gameObject, id);
				SerializedObject serialized = new SerializedObject(door);
				PickUpable key = serialized.FindProperty("m_keyToOpen")?.objectReferenceValue as PickUpable;
				Sprite open = serialized.FindProperty("m_sprOpen")?.objectReferenceValue as Sprite;
				Sprite closed = serialized.FindProperty("m_sprClosed")?.objectReferenceValue as Sprite;
				io_objects.Add(Point(io_nextId++, id, "door", door.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels,
					new JProperty("doorType", door is JackyDoor ? "jacky" : "standard"),
					new JProperty("initiallyOpen", ReadBool(serialized, "m_isOpen", false)),
					new JProperty("price", ReadInt(serialized, "m_priceToActivate", 0)),
					new JProperty("singleUse", ReadBool(serialized, "m_isSingleUse", false)),
					new JProperty("initiallyInteractable", !ReadBool(serialized, "m_isUnInteractable", false)),
					new JProperty("requiredItemId", key == null ? string.Empty : ScriptId(key.name)),
					new JProperty("openSprite", open == null ? string.Empty : open.name),
					new JProperty("closedSprite", closed == null ? string.Empty : closed.name)));
			}
		}

		private static void ExportScriptedActors(GameObject i_root, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels,
			HashSet<string> io_ids, Dictionary<GameObject, string> io_objectIds)
		{
			foreach (NPC actor in i_root.GetComponentsInChildren<NPC>(true))
			{
				string enemy = EnemyId(actor);
				if (string.IsNullOrEmpty(enemy) && actor.GetName() == "Android") enemy = "core:enemy/android";
				if (string.IsNullOrEmpty(enemy))
					throw new InvalidOperationException("FER scripted actor has no recognized Core enemy: " + actor.name);
				string id = UniqueScriptId(actor.name, io_ids);
				io_objectIds.Add(actor.gameObject, id);
				io_objects.Add(Point(io_nextId++, id, "scripted-actor", actor.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels,
					new JProperty("enemy", enemy), new JProperty("initiallyActive", actor.gameObject.activeSelf)));
			}
		}

		private static List<GameObject> CollectExplicitCoreStageObjects(GameObject i_root)
		{
			HashSet<GameObject> selected = new HashSet<GameObject>();
			foreach (Interactable value in i_root.GetComponentsInChildren<Interactable>(true))
				if (!(value is Vendor) && !(value is WeaponCase) && !(value is FuseBox)
					&& !(value is LightBulbIllumination) && !(value is Door)) selected.Add(value.gameObject);
			foreach (AndroidStatue value in i_root.GetComponentsInChildren<AndroidStatue>(true)) selected.Add(value.gameObject);
			// Alarm lights are exported as portable point lights; retaining their hosts
			// would duplicate them and emit an unmapped Core-prop ID.
			foreach (ParticleSystem value in i_root.GetComponentsInChildren<ParticleSystem>(true))
				if (value.transform.parent == null || value.transform.parent.name == "AreaLights") selected.Add(value.gameObject);
			foreach (Animator value in i_root.GetComponentsInChildren<Animator>(true))
				if (IsUnderNamedAncestor(value.transform, "Decoration", i_root.transform)) selected.Add(value.gameObject);
			foreach (Transform value in i_root.GetComponentsInChildren<Transform>(true))
			{
				if (!value.name.StartsWith("Skeleton", StringComparison.OrdinalIgnoreCase)) continue;
				Transform highestSkeleton = value;
				for (Transform parent = value.parent; parent != null
					&& parent.name.StartsWith("Skeleton", StringComparison.OrdinalIgnoreCase); parent = parent.parent)
					highestSkeleton = parent;
				Transform decorativeRoot = highestSkeleton.parent;
				if (decorativeRoot != null && IsUnderNamedAncestor(decorativeRoot, "Decoration", i_root.transform))
					selected.Add(decorativeRoot.gameObject);
			}

			StageFER stage = i_root.GetComponent<StageFER>();
			SerializedObject serializedStage = stage == null ? null : new SerializedObject(stage);
			AddReferencedGameObjects(serializedStage == null ? null : serializedStage.FindProperty("m_objectsToActivateInLab"), selected);
			AddReferencedComponents(serializedStage == null ? null : serializedStage.FindProperty("m_interactablesToEnableInLab"), selected);
			foreach (Interactable interaction in i_root.GetComponentsInChildren<Interactable>(true))
			{
				SerializedObject serialized = new SerializedObject(interaction);
				AddReferencedGameObjects(serialized.FindProperty("m_objectsToEnableAfterActivation"), selected);
				AddReferencedGameObjects(serialized.FindProperty("m_objectsToDisableAfterActivation"), selected);
				AddReferencedComponents(serialized.FindProperty("m_interactablesToActivateAfterActivation"), selected);
			}
			foreach (PickUpable pickup in i_root.GetComponentsInChildren<PickUpable>(true))
			{
				SerializedObject serialized = new SerializedObject(pickup);
				AddReferencedGameObjects(serialized.FindProperty("m_objectsToEnableAfterPickUp"), selected);
			}
			HashSet<GameObject> establishedObjects = new HashSet<GameObject>(selected);
			foreach (UnityEngine.Rendering.Universal.Light2D value in
				i_root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true))
			{
				// Vendors are exported as whole prefabs. Their child lights must keep
				// their prefab-local positions instead of becoming separate Tiled objects.
				bool fuseIndicator = false;
				foreach (FuseBox fuse in i_root.GetComponentsInChildren<FuseBox>(true))
				{
					SpriteRenderer indicator = new SerializedObject(fuse).FindProperty("m_sprRendererLightBulbToTurnOn")?.objectReferenceValue as SpriteRenderer;
					if (indicator != null && value.transform.IsChildOf(indicator.transform)) { fuseIndicator = true; break; }
				}
				if (!fuseIndicator && value != stage.GetLightGlobal() && !IsPortableFreeformLight(value)
					&& !IsPortablePointLight(i_root, value) && value.GetComponentInParent<Vendor>(true) == null
					&& value.GetComponentInParent<LightBulbIllumination>(true) == null
					&& value.GetComponentInParent<PickUpable>(true) == null) selected.Add(value.gameObject);
			}
			selected.RemoveWhere(candidate => candidate.GetComponent<LightBulbIllumination>() != null);
			selected.RemoveWhere(candidate => IsPortableFreeformLight(candidate.GetComponent<UnityEngine.Rendering.Universal.Light2D>()));
			selected.RemoveWhere(candidate => IsPortablePointLight(i_root,
				candidate.GetComponent<UnityEngine.Rendering.Universal.Light2D>()));
			selected.RemoveWhere(candidate => candidate.name == "int_pLLight (7)"
				&& candidate.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true) != null);
			if (stage.GetLightGlobal() != null)
				selected.RemoveWhere(candidate => candidate.GetComponent<UnityEngine.Rendering.Universal.Light2D>() == stage.GetLightGlobal());
			// Other selection passes may also reach a vendor child. The whole vending
			// machine is already represented by its dedicated Tiled vendor point.
			selected.RemoveWhere(candidate => candidate.GetComponentInParent<Vendor>(true) != null);
			selected.RemoveWhere(candidate => candidate.GetComponentInParent<PickUpable>(true) != null);
			// These relationships are represented by door properties and stage-script
			// actor IDs now; following legacy serialized links must not re-add adapters.
			selected.RemoveWhere(candidate => candidate.GetComponent<Door>() != null
				|| candidate.GetComponent<NPC>() != null);
			List<GameObject> roots = new List<GameObject>();
			foreach (GameObject candidate in selected)
			{
				bool nested = false;
				for (Transform parent = candidate.transform.parent; parent != null && parent != i_root.transform; parent = parent.parent)
					if (selected.Contains(parent.gameObject)) { nested = true; break; }
				if (!nested) roots.Add(candidate);
			}
			roots.Sort((left, right) =>
			{
				int priority = (establishedObjects.Contains(left) ? 0 : 1)
					.CompareTo(establishedObjects.Contains(right) ? 0 : 1);
				return priority != 0 ? priority : string.CompareOrdinal(
					RelativePath(i_root.transform, left.transform), RelativePath(i_root.transform, right.transform));
			});
			return roots;
		}

		private static bool IsUnderNamedAncestor(Transform i_transform, string i_name, Transform i_root)
		{
			for (Transform current = i_transform; current != null && current != i_root; current = current.parent)
				if (string.Equals(current.name, i_name, StringComparison.Ordinal)) return true;
			return false;
		}

		private static void AddReferencedGameObjects(SerializedProperty i_array, HashSet<GameObject> io_selected)
		{
			for (int index = 0; i_array != null && index < i_array.arraySize; index++)
			{
				GameObject value = i_array.GetArrayElementAtIndex(index).objectReferenceValue as GameObject;
				if (value != null) io_selected.Add(value);
			}
		}

		private static void AddReferencedComponents(SerializedProperty i_array, HashSet<GameObject> io_selected)
		{
			for (int index = 0; i_array != null && index < i_array.arraySize; index++)
			{
				Component value = i_array.GetArrayElementAtIndex(index).objectReferenceValue as Component;
				if (value != null) io_selected.Add(value.gameObject);
			}
		}

		private static bool HasSelectedAncestor(Transform i_transform, HashSet<GameObject> i_selected)
		{
			for (Transform current = i_transform; current != null; current = current.parent)
				if (i_selected.Contains(current.gameObject)) return true;
			return false;
		}

		private static string RelativePath(Transform i_root, Transform i_target)
		{
			List<string> segments = new List<string>();
			for (Transform current = i_target; current != null && current != i_root; current = current.parent)
				segments.Add(current.name + "[" + current.GetSiblingIndex() + "]");
			segments.Reverse();
			return string.Join("/", segments);
		}

		private static string UniqueScriptId(string i_name, HashSet<string> io_ids)
		{
			string stem = ScriptId(i_name), result = stem;
			for (int suffix = 2; !io_ids.Add(result); suffix++) result = stem + "-" + suffix;
			return result;
		}

		private static string CoreObjectKind(GameObject i_object)
		{
			if (i_object.GetComponent<Door>() != null || i_object.GetComponent<DoorRoller>() != null) return "door";
			if (i_object.GetComponent<NPC>() != null) return "actor";
			if (i_object.GetComponent<Item>() != null) return "item";
			if (i_object.GetComponent<AndroidStatuePose>() != null) return "pose";
			if (i_object.GetComponent<LightBulb>() != null || i_object.GetComponent<AlarmLight>() != null
				|| i_object.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true) != null) return "light";
			if (i_object.GetComponent<Interactable>() != null) return "interaction";
			return "object";
		}

		private static JObject BuildJackyCurseStageScript(GameObject i_root)
		{
			JArray sequences = new JArray();
			int index = 0;
			foreach (JackyDoor door in i_root.GetComponentsInChildren<JackyDoor>(true))
			{
				sequences.Add(new JObject(
					new JProperty("id", "jacky-curse-" + (++index)),
					new JProperty("trigger", "door-state"),
					new JProperty("objectId", ScriptId(door.gameObject.name)),
					new JProperty("state", "open"),
					new JProperty("once", true),
					new JProperty("actions", new JArray(
						new JObject(new JProperty("type", "wait"), new JProperty("seconds", 15)),
						new JObject(new JProperty("type", "apply-player-status"),
							new JProperty("status", "jacky-curse"), new JProperty("durationSeconds", 99999),
							new JProperty("ticksPerSecond", 0.25), new JProperty("chance", 0.175),
							new JProperty("chanceIncrease", 0.05), new JProperty("maxActive", 4))))));
			}
			if (sequences.Count == 0)
				sequences.Add(new JObject(new JProperty("id", "no-jacky-door-exported"), new JProperty("trigger", "manual"),
					new JProperty("actions", new JArray(new JObject(new JProperty("type", "notify"), new JProperty("message", "No FER Jacky door was exported."))))));
			return new JObject(new JProperty("schemaVersion", 1), new JProperty("type", "stageScript"),
				new JProperty("id", "example.core-fer-tiled-port:stage-script/fer-jacky-curse"),
				new JProperty("stage", "example.core-fer-tiled-port:stage/fer-tiled-port"), new JProperty("sequences", sequences));
		}

		private static Dictionary<Spawner, List<string>> BuildRequiredDoorMap(GameObject i_root,
			Dictionary<Spawner, string> i_spawnerIds, Dictionary<GameObject, string> i_objectIds)
		{
			Dictionary<Spawner, List<string>> result = new Dictionary<Spawner, List<string>>();
			foreach (Door door in i_root.GetComponentsInChildren<Door>(true))
			{
				SerializedProperty links = new SerializedObject(door).FindProperty("m_spawnersToEnableAfterActivation");
				for (int index = 0; links != null && index < links.arraySize; index++)
				{
					Spawner spawner = links.GetArrayElementAtIndex(index).objectReferenceValue as Spawner;
					if (spawner == null || !i_spawnerIds.ContainsKey(spawner)) continue;
					if (!result.TryGetValue(spawner, out List<string> doors)) result[spawner] = doors = new List<string>();
					string id = ObjectId(door.gameObject, i_objectIds);
					if (!doors.Contains(id)) doors.Add(id);
				}
			}
			return result;
		}

		private static JObject BuildEncounterStageScript(GameObject i_root, Dictionary<Spawner, string> i_spawnerIds,
			Dictionary<GameObject, string> i_objectIds)
		{
			JArray sequences = new JArray();
			HashSet<string> sequenceIds = new HashSet<string>();
			foreach (Interactable interaction in i_root.GetComponentsInChildren<Interactable>(true))
			{
				SerializedObject serialized = new SerializedObject(interaction);
				JArray actions = new JArray();
				AddLinkedObjectActions(serialized.FindProperty("m_objectsToEnableAfterActivation"), true, actions, i_objectIds);
				AddLinkedObjectActions(serialized.FindProperty("m_objectsToDisableAfterActivation"), false, actions, i_objectIds);
				SerializedProperty spawners = serialized.FindProperty("m_spawnersToEnableAfterActivation");
				for (int index = 0; spawners != null && index < spawners.arraySize; index++)
				{
					Spawner spawner = spawners.GetArrayElementAtIndex(index).objectReferenceValue as Spawner;
					if (spawner != null && i_spawnerIds.TryGetValue(spawner, out string spawnerId))
						actions.Add(new JObject(new JProperty("type", "set-spawner-enabled"), new JProperty("objectId", spawnerId), new JProperty("active", true)));
				}
				if (actions.Count == 0) continue;
				string signal = ObjectId(interaction.gameObject, i_objectIds);
				string sequenceId = "linked-" + signal;
				for (int suffix = 2; !sequenceIds.Add(sequenceId); suffix++) sequenceId = "linked-" + signal + "-" + suffix;
				sequences.Add(new JObject(new JProperty("id", sequenceId), new JProperty("trigger", "interaction"),
					new JProperty("signal", signal), new JProperty("once", ReadBool(serialized, "m_isSingleUse", true)),
					new JProperty("actions", actions)));
			}
			foreach (PickUpable pickup in i_root.GetComponentsInChildren<PickUpable>(true))
			{
				if (pickup is Weapon || !IsUnderNamedAncestor(pickup.transform, "Items", i_root.transform)) continue;
				JArray actions = new JArray();
				SerializedProperty linked = new SerializedObject(pickup).FindProperty("m_objectsToEnableAfterPickUp");
				AddLinkedObjectActions(linked, true, actions, i_objectIds);
				if (actions.Count == 0) continue;
				sequences.Add(new JObject(new JProperty("id", "pickup-" + ScriptId(pickup.name)),
					new JProperty("trigger", "signal"), new JProperty("signal", "fer-keycard-picked-up"),
					new JProperty("once", true), new JProperty("actions", actions)));
			}
			if (sequences.Count == 0)
				sequences.Add(new JObject(new JProperty("id", "no-linked-encounters"), new JProperty("trigger", "manual"),
					new JProperty("actions", new JArray(new JObject(new JProperty("type", "notify"), new JProperty("message", "No FER encounter links were exported."))))));
			return new JObject(new JProperty("schemaVersion", 1), new JProperty("type", "stageScript"),
				new JProperty("id", "example.core-fer-tiled-port:stage-script/fer-encounters"),
				new JProperty("stage", "example.core-fer-tiled-port:stage/fer-tiled-port"), new JProperty("sequences", sequences));
		}

		private static void AddLinkedObjectActions(SerializedProperty i_objects, bool i_active, JArray io_actions,
			Dictionary<GameObject, string> i_objectIds)
		{
			for (int index = 0; i_objects != null && index < i_objects.arraySize; index++)
			{
				GameObject target = i_objects.GetArrayElementAtIndex(index).objectReferenceValue as GameObject;
				if (target == null) continue;
				NPC actor = target.GetComponent<NPC>();
				string objectId = ObjectId(target, i_objectIds);
				if (i_active && actor != null) io_actions.Add(new JObject(new JProperty("type", "spawn-actor"), new JProperty("objectId", objectId)));
				else io_actions.Add(new JObject(new JProperty("type", "set-object-active"), new JProperty("objectId", objectId), new JProperty("active", i_active)));
			}
		}

		private static JObject BuildLabStageScript(GameObject i_root, Dictionary<GameObject, string> i_objectIds)
		{
			JArray sequences = new JArray();
			FuseBox[] fuseBoxes = i_root.GetComponentsInChildren<FuseBox>(true);
			foreach (FuseBox fuse in fuseBoxes)
			{
				string fuseId = ObjectId(fuse.gameObject, i_objectIds);
				sequences.Add(new JObject(
					new JProperty("id", "activate-" + ScriptId(fuse.gameObject.name)),
					new JProperty("trigger", "interaction"), new JProperty("signal", fuseId),
					new JProperty("once", true), new JProperty("actions", new JArray(
						new JObject(new JProperty("type", "play-audio"), new JProperty("objectId", FuseAudioId(fuseId))),
						new JObject(new JProperty("type", "add-variable"), new JProperty("variable", "fusesActivated"), new JProperty("value", 1)),
						new JObject(new JProperty("type", "send-signal"), new JProperty("signal", "fer-fuse-changed"))))));
			}

			JArray labActions = new JArray();
			StageFER stage = i_root.GetComponent<StageFER>();
			SerializedObject serialized = stage == null ? null : new SerializedObject(stage);
			SerializedProperty objects = serialized == null ? null : serialized.FindProperty("m_objectsToActivateInLab");
			for (int index = 0; objects != null && index < objects.arraySize; index++)
			{
				GameObject target = objects.GetArrayElementAtIndex(index).objectReferenceValue as GameObject;
				if (target != null) labActions.Add(new JObject(new JProperty("type", "set-object-active"), new JProperty("objectId", ObjectId(target, i_objectIds)), new JProperty("active", true)));
			}
			SerializedProperty interactions = serialized == null ? null : serialized.FindProperty("m_interactablesToEnableInLab");
			for (int index = 0; interactions != null && index < interactions.arraySize; index++)
			{
				Interactable target = interactions.GetArrayElementAtIndex(index).objectReferenceValue as Interactable;
				if (target != null) labActions.Add(new JObject(new JProperty("type", "set-interaction-enabled"), new JProperty("objectId", ObjectId(target.gameObject, i_objectIds)), new JProperty("active", true)));
			}
			if (labActions.Count == 0) labActions.Add(new JObject(new JProperty("type", "notify"), new JProperty("message", "FER laboratory power restored.")));
			labActions.Add(new JObject(new JProperty("type", "play-audio"), new JProperty("objectId", "fer-lab-activation-audio")));
			sequences.Add(new JObject(new JProperty("id", "activate-laboratory"), new JProperty("trigger", "signal"),
				new JProperty("signal", "fer-fuse-changed"), new JProperty("once", true),
				new JProperty("conditions", new JArray(new JObject(new JProperty("variable", "fusesActivated"),
					new JProperty("operator", ">="), new JProperty("value", fuseBoxes.Length)))),
				new JProperty("actions", labActions)));
			return new JObject(new JProperty("schemaVersion", 1), new JProperty("type", "stageScript"),
				new JProperty("id", "example.core-fer-tiled-port:stage-script/fer-laboratory"),
				new JProperty("stage", "example.core-fer-tiled-port:stage/fer-tiled-port"),
				new JProperty("variables", new JObject(new JProperty("fusesActivated", 0))), new JProperty("sequences", sequences));
		}

		private static void ExportFuseBoxes(GameObject i_root, string i_packRoot, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			FuseBox[] fuses = i_root.GetComponentsInChildren<FuseBox>(true);
			HashSet<SpriteRenderer> exportedIndicators = new HashSet<SpriteRenderer>();
			foreach (FuseBox fuse in fuses)
			{
				SerializedObject serialized = new SerializedObject(fuse);
				SpriteRenderer fuseRenderer = fuse.GetComponent<SpriteRenderer>();
				Sprite fuseOn = serialized.FindProperty("m_sprFuseBoxOn")?.objectReferenceValue as Sprite;
				SpriteRenderer indicator = serialized.FindProperty("m_sprRendererLightBulbToTurnOn")?.objectReferenceValue as SpriteRenderer;
				Sprite indicatorOn = serialized.FindProperty("m_sprLightBulbTurnedOn")?.objectReferenceValue as Sprite;
				UnityEngine.Rendering.Universal.Light2D indicatorLight = serialized.FindProperty("m_lightLightBulb")?.objectReferenceValue as UnityEngine.Rendering.Universal.Light2D;
				AudioClip audio = serialized.FindProperty("m_audioActivate")?.objectReferenceValue as AudioClip;
				if (fuseRenderer == null || fuseRenderer.sprite == null || fuseOn == null || indicator == null
					|| indicator.sprite == null || indicatorOn == null || indicatorLight == null || audio == null)
					throw new InvalidOperationException("FER fuse box is missing a portable presentation reference: " + fuse.name);
				ExportNamedSpriteTexture(fuseRenderer.sprite, i_packRoot, "fer-fuse-box-closed.png");
				ExportNamedSpriteTexture(fuseOn, i_packRoot, "fer-fuse-box-open.png");
				ExportNamedSpriteTexture(indicator.sprite, i_packRoot, "fer-indicator-off.png");
				ExportNamedSpriteTexture(indicatorOn, i_packRoot, "fer-indicator-on.png");
				ExportNamedAudio(audio, i_packRoot, "fer-fuse-activate.ogg");

				string fuseId = ScriptId(fuse.name);
				string indicatorId = ScriptId(indicator.name);
				if (exportedIndicators.Add(indicator))
					io_objects.Add(Point(io_nextId++, indicatorId, "light-bulb", indicator.transform.position,
						i_minY, i_centerX, i_widthPixels, i_heightPixels,
						new JProperty("initiallyOn", true), new JProperty("flicker", 0f), new JProperty("interactive", false),
						new JProperty("visualFile", "assets/art/fer-indicator-off.png"),
						new JProperty("activatedVisualFile", "assets/art/fer-indicator-on.png"),
						new JProperty("pixelsPerUnit", 32f), new JProperty("pivotX", 0.5f), new JProperty("pivotY", 0.5f),
						new JProperty("sortingLayer", indicator.sortingLayerName), new JProperty("sortingOrder", indicator.sortingOrder),
						new JProperty("initialColor", "#" + ColorUtility.ToHtmlStringRGB(indicatorLight.color)),
						new JProperty("activatedColor", "#00FF00"),
						new JProperty("innerRadius", indicatorLight.pointLightInnerRadius),
						new JProperty("outerRadius", indicatorLight.pointLightOuterRadius),
						new JProperty("falloffIntensity", indicatorLight.falloffIntensity)));
				io_objects.Add(Point(io_nextId++, fuseId, "interaction", fuse.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels,
					new JProperty("trigger", "use"), new JProperty("price", ReadInt(serialized, "m_priceToActivate", 0)),
					new JProperty("singleUse", true), new JProperty("targetLights", indicatorId),
					new JProperty("lightAction", "activate"),
					new JProperty("visualFile", "assets/art/fer-fuse-box-closed.png"),
					new JProperty("activatedVisualFile", "assets/art/fer-fuse-box-open.png"),
					new JProperty("visualPixelsPerUnit", 32f), new JProperty("visualPivotX", 0.5f),
					new JProperty("visualPivotY", 0.5f), new JProperty("visualSortingLayer", fuseRenderer.sortingLayerName),
					new JProperty("visualSortingOrder", fuseRenderer.sortingOrder)));
				io_objects.Add(Point(io_nextId++, FuseAudioId(fuseId), "ambient-audio", fuse.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels,
					new JProperty("file", "assets/audio/fer-fuse-activate.ogg"), new JProperty("loop", false),
					new JProperty("playOnStart", false), new JProperty("volume", 1f)));
			}
		}

		private static string FuseAudioId(string i_fuseId)
		{
			return "fer-fuse-" + i_fuseId.Replace("int-fusebox", string.Empty) + "-audio";
		}

		private static void ExportProximityLights(GameObject i_root, string i_packRoot, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			foreach (LightBulbIllumination fixture in i_root.GetComponentsInChildren<LightBulbIllumination>(true))
			{
				SpriteRenderer renderer = fixture.GetComponent<SpriteRenderer>();
				UnityEngine.Rendering.Universal.Light2D light = fixture.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
				if (renderer == null || renderer.sprite == null || light == null)
					throw new InvalidOperationException("FER proximity fixture is missing its sprite or 2D light: " + fixture.name);
				ExportNamedSpriteTexture(renderer.sprite, i_packRoot, "fer-proximity-light.png");
				SerializedObject serialized = new SerializedObject(fixture);
				bool freeform = light.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Freeform;
				if (!freeform && light.lightType != UnityEngine.Rendering.Universal.Light2D.LightType.Point)
					throw new InvalidOperationException("FER proximity fixture has an unsupported light shape: " + fixture.name);
				List<JProperty> properties = new List<JProperty>
				{
					new JProperty("file", "assets/art/fer-proximity-light.png"),
					new JProperty("initiallyActive", fixture.gameObject.activeSelf),
					new JProperty("detectionRadius", ReadFloat(serialized, "m_distanceSeeActor", 12f)),
					new JProperty("fadeSeconds", 1.5f), new JProperty("intensity", light.intensity),
					new JProperty("lightType", freeform ? "freeform" : "point"),
					new JProperty("color", "#" + ColorUtility.ToHtmlStringRGB(light.color)),
					new JProperty("falloffIntensity", light.falloffIntensity),
					new JProperty("innerRadius", light.pointLightInnerRadius),
					new JProperty("outerRadius", light.pointLightOuterRadius),
					new JProperty("lightOffsetY", light.transform.localPosition.y),
					new JProperty("lightRotationZ", Mathf.DeltaAngle(0f, light.transform.localEulerAngles.z)),
					new JProperty("sortingLayer", renderer.sortingLayerName),
					new JProperty("sortingOrder", renderer.sortingOrder)
				};
				if (freeform)
				{
					string path = string.Join(";", light.shapePath.Select(vertex =>
						vertex.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," +
						vertex.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture)));
					properties.Add(new JProperty("shapeFalloffSize", light.shapeLightFalloffSize));
					properties.Add(new JProperty("shapePath", path));
				}
				io_objects.Add(Point(io_nextId++, ScriptId(fixture.name), "proximity-light", fixture.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels, properties.ToArray()));
			}
		}

		private static bool IsPortableFreeformLight(UnityEngine.Rendering.Universal.Light2D i_light)
		{
			return i_light != null && i_light.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Freeform
				&& i_light.transform.parent != null && (i_light.transform.parent.name == "AreaLights"
					|| i_light.transform.parent.name == "dc_stageLights"
					|| i_light.transform.parent.name == "int_pLLight (7)");
		}

		private static void ExportPortableFreeformLights(GameObject i_root, string i_packRoot, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			foreach (UnityEngine.Rendering.Universal.Light2D light in
				i_root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true))
			{
				if (!IsPortableFreeformLight(light)) continue;
				SerializedProperty sorting = new SerializedObject(light).FindProperty("m_ApplyToSortingLayers");
				if (sorting == null || !sorting.isArray || sorting.arraySize == 0)
					throw new InvalidOperationException("FER freeform light has no affected sorting layers: " + light.name);
				List<string> sortingNames = new List<string>();
				for (int index = 0; index < sorting.arraySize; index++)
					sortingNames.Add(SortingLayer.IDToName(sorting.GetArrayElementAtIndex(index).intValue));
				string path = string.Join(";", light.shapePath.Select(vertex =>
					vertex.x.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "," +
					vertex.y.ToString("R", System.Globalization.CultureInfo.InvariantCulture)));
				bool theatre = light.transform.parent.name == "dc_stageLights";
				bool labFixture = light.transform.parent.name == "int_pLLight (7)";
				GameObject fixture = labFixture ? light.transform.parent.gameObject : light.gameObject;
				List<JProperty> properties = new List<JProperty>
				{
					new JProperty("initiallyActive", fixture.activeSelf),
					new JProperty("intensity", light.intensity),
					new JProperty("color", "#" + ColorUtility.ToHtmlStringRGB(light.color)),
					new JProperty("falloffIntensity", light.falloffIntensity),
					new JProperty("shapeFalloffSize", light.shapeLightFalloffSize),
					new JProperty("shapePath", path),
					new JProperty("sortingLayers", string.Join(",", sortingNames)),
					new JProperty("suppressInheritedPath", theatre || labFixture
						? RelativePath(i_root.transform, fixture.transform) : string.Empty)
				};
				if (labFixture)
				{
					SpriteRenderer renderer = fixture.GetComponent<SpriteRenderer>();
					if (renderer == null || renderer.sprite == null)
						throw new InvalidOperationException("FER lab light fixture is missing its sprite.");
					ExportNamedSpriteTexture(renderer.sprite, i_packRoot, "fer-proximity-light.png");
					properties.Add(new JProperty("file", "assets/art/fer-proximity-light.png"));
					properties.Add(new JProperty("pixelsPerUnit", 32f));
					properties.Add(new JProperty("sortingLayer", renderer.sortingLayerName));
					properties.Add(new JProperty("sortingOrder", renderer.sortingOrder));
					properties.Add(new JProperty("lightOffsetY", light.transform.localPosition.y));
					properties.Add(new JProperty("lightRotationZ", light.transform.localEulerAngles.z));
				}
				io_objects.Add(Point(io_nextId++, theatre ? "freeform-light-2d-2" : ScriptId(fixture.name),
					"freeform-light", fixture.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels, properties.ToArray()));
			}
		}

		private static void ExportGlobalLight(GameObject i_root, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			Stage stage = i_root.GetComponent<Stage>();
			UnityEngine.Rendering.Universal.Light2D light = stage == null ? null : stage.GetLightGlobal();
			if (light == null || light.lightType != UnityEngine.Rendering.Universal.Light2D.LightType.Global)
				throw new InvalidOperationException("FER stage is missing its Global Light 2D.");
			SerializedProperty sorting = new SerializedObject(light).FindProperty("m_ApplyToSortingLayers");
			if (sorting == null || !sorting.isArray || sorting.arraySize == 0)
				throw new InvalidOperationException("FER global light has no affected sorting layers.");
			HashSet<int> availableIds = new HashSet<int>(SortingLayer.layers.Select(layer => layer.id));
			List<string> sortingNames = new List<string>();
			for (int index = 0; index < sorting.arraySize; index++)
			{
				int id = sorting.GetArrayElementAtIndex(index).intValue;
				// The source prefab also contains two stale layer IDs with no matching layer in TagManager.
				if (availableIds.Contains(id)) sortingNames.Add(SortingLayer.IDToName(id));
			}
			io_objects.Add(Point(io_nextId++, ScriptId(light.name), "global-light", light.transform.position,
				i_minY, i_centerX, i_widthPixels, i_heightPixels,
				new JProperty("intensity", light.intensity),
				new JProperty("color", "#" + ColorUtility.ToHtmlStringRGB(light.color)),
				new JProperty("falloffIntensity", light.falloffIntensity),
				new JProperty("sortingLayers", string.Join(",", sortingNames))));
		}

		private static bool IsPortablePointLight(GameObject i_root, UnityEngine.Rendering.Universal.Light2D i_light)
		{
			if (i_light == null || i_light.lightType != UnityEngine.Rendering.Universal.Light2D.LightType.Point)
				return false;
			string path = RelativePath(i_root.transform, i_light.transform);
			return path == "Decoration[2]/Lab[4]/dc_bluePrint (5)[14]/Point Light 2D[0]"
				|| path == "Decoration[2]/Lab[4]/Lock[10]/Parametric Light 2D[0]";
		}

		private static void ExportPointLights(GameObject i_root, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			foreach (UnityEngine.Rendering.Universal.Light2D light in
				i_root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>(true))
			{
				if (!IsPortablePointLight(i_root, light)) continue;
				SerializedProperty sorting = new SerializedObject(light).FindProperty("m_ApplyToSortingLayers");
				if (sorting == null || !sorting.isArray || sorting.arraySize == 0)
					throw new InvalidOperationException("FER point light has no affected sorting layers: " + light.name);
				List<string> sortingNames = new List<string>();
				for (int index = 0; index < sorting.arraySize; index++)
					sortingNames.Add(SortingLayer.IDToName(sorting.GetArrayElementAtIndex(index).intValue));
				AlarmLight alarm = light.GetComponent<AlarmLight>();
				SerializedObject serializedAlarm = alarm == null ? null : new SerializedObject(alarm);
				io_objects.Add(Point(io_nextId++, ScriptId(light.name), "point-light", light.transform.position,
					i_minY, i_centerX, i_widthPixels, i_heightPixels,
					new JProperty("initiallyActive", light.gameObject.activeSelf),
					new JProperty("intensity", light.intensity),
					new JProperty("color", "#" + ColorUtility.ToHtmlStringRGB(light.color)),
					new JProperty("falloffIntensity", light.falloffIntensity),
					new JProperty("innerRadius", light.pointLightInnerRadius),
					new JProperty("outerRadius", light.pointLightOuterRadius),
					new JProperty("innerAngle", light.pointLightInnerAngle),
					new JProperty("outerAngle", light.pointLightOuterAngle),
					new JProperty("rotationZ", light.transform.eulerAngles.z),
					new JProperty("sortingLayers", string.Join(",", sortingNames)),
					new JProperty("overlapOperation", light.overlapOperation == UnityEngine.Rendering.Universal.Light2D.OverlapOperation.AlphaBlend
						? "alpha-blend" : "additive"),
					new JProperty("pulseFrom", serializedAlarm == null ? 0f : ReadFloat(serializedAlarm, "m_intensityFrom", 0f)),
					new JProperty("pulseTo", serializedAlarm == null ? 0f : ReadFloat(serializedAlarm, "m_intensityTo", 0f)),
					new JProperty("pulseSeconds", serializedAlarm == null ? 0f : ReadFloat(serializedAlarm, "m_timeToMove", 0f)),
					new JProperty("suppressInheritedPath", RelativePath(i_root.transform, light.transform))));
			}
		}

		private static void ExportNamedSpriteTexture(Sprite i_sprite, string i_packRoot, string i_fileName)
		{
			string source = i_sprite == null || i_sprite.texture == null ? string.Empty : AssetDatabase.GetAssetPath(i_sprite.texture);
			if (string.IsNullOrWhiteSpace(source) || !source.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("FER portable state sprite must use a PNG texture: " + i_fileName);
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			File.Copy(System.IO.Path.Combine(projectRoot, source), System.IO.Path.Combine(i_packRoot, "assets", "art", i_fileName), true);
		}

		private static void ExportNamedAudio(AudioClip i_clip, string i_packRoot, string i_fileName)
		{
			string source = i_clip == null ? string.Empty : AssetDatabase.GetAssetPath(i_clip);
			if (string.IsNullOrWhiteSpace(source) || !source.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("FER portable interaction audio must use an OGG asset: " + i_fileName);
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			File.Copy(System.IO.Path.Combine(projectRoot, source), System.IO.Path.Combine(i_packRoot, "assets", "audio", i_fileName), true);
		}

		private static bool ExportLabActivationAudio(GameObject i_root, string i_packRoot)
		{
			StageFER stage = i_root.GetComponent<StageFER>();
			SerializedProperty property = stage == null ? null : new SerializedObject(stage).FindProperty("m_audioActivateLab");
			AudioClip clip = property == null ? null : property.objectReferenceValue as AudioClip;
			string source = clip == null ? string.Empty : AssetDatabase.GetAssetPath(clip);
			if (string.IsNullOrWhiteSpace(source) || !source.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
			{
				Debug.LogWarning("[Modding] FER laboratory activation audio could not be exported.");
				return false;
			}
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			string fullSource = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectRoot, source));
			string destination = System.IO.Path.Combine(i_packRoot, "assets", "audio", "fer-lab-activate.ogg");
			File.Copy(fullSource, destination, true);
			return true;
		}

		private static string ExportStageAudioClip(AudioClip i_clip, string i_packRoot)
		{
			string source = i_clip == null ? string.Empty : AssetDatabase.GetAssetPath(i_clip);
			string extension = System.IO.Path.GetExtension(source).ToLowerInvariant();
			if (string.IsNullOrWhiteSpace(source) || (extension != ".ogg" && extension != ".wav"))
				throw new InvalidOperationException("FER positional audio must be a WAV or OGG asset: " + source);
			string file = "fer-" + System.IO.Path.GetFileName(source);
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			File.Copy(System.IO.Path.Combine(projectRoot, source),
				System.IO.Path.Combine(i_packRoot, "assets", "audio", file), true);
			return "assets/audio/" + file;
		}

		private static string ExportStageItemTexture(Sprite i_sprite, string i_packRoot, string i_id)
		{
			Texture2D texture = i_sprite == null ? null : i_sprite.texture;
			string source = texture == null ? string.Empty : AssetDatabase.GetAssetPath(texture);
			if (string.IsNullOrWhiteSpace(source) || !source.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				throw new InvalidOperationException("FER stage item must use a PNG texture: " + i_id);
			string file = "fer-" + i_id + ".png";
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			File.Copy(System.IO.Path.Combine(projectRoot, source),
				System.IO.Path.Combine(i_packRoot, "assets", "art", file), true);
			return "assets/art/" + file;
		}

		private static Dictionary<string, string> ReadCoreWeaponIds(string i_projectRoot)
		{
			string catalogPath = System.IO.Path.Combine(i_projectRoot, "Assets", "Resources", "Modding", "Core", "catalog.json");
			JArray entries = JObject.Parse(File.ReadAllText(catalogPath))["entries"] as JArray;
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (JObject entry in entries?.OfType<JObject>() ?? Enumerable.Empty<JObject>())
			{
				string id = (string)entry["id"];
				string legacyName = (string)entry["legacyName"];
				if (!string.IsNullOrEmpty(legacyName) && id != null
					&& id.StartsWith("core:item/weapon/", StringComparison.Ordinal)) result[legacyName] = id;
			}
			return result;
		}

		private static JObject BuildStatueStageScript(GameObject i_root, Dictionary<GameObject, string> i_objectIds)
		{
			JArray sequences = new JArray();
			foreach (AndroidStatue statue in i_root.GetComponentsInChildren<AndroidStatue>(true))
			{
				SerializedProperty poses = new SerializedObject(statue).FindProperty("m_poses");
				for (int index = 0; poses != null && index < poses.arraySize; index++)
				{
					AndroidStatuePose pose = poses.GetArrayElementAtIndex(index).objectReferenceValue as AndroidStatuePose;
					if (pose == null) continue;
					string statueId = ObjectId(statue.gameObject, i_objectIds);
					string poseId = ObjectId(pose.gameObject, i_objectIds);
					sequences.Add(new JObject(
						new JProperty("id", statueId + "-wave-" + pose.GetNumWave()),
						new JProperty("trigger", "wave-end"), new JProperty("once", true),
						new JProperty("conditions", new JArray(new JObject(new JProperty("source", "wave"),
							new JProperty("operator", "=="), new JProperty("value", pose.GetNumWave())))),
						new JProperty("actions", new JArray(
							new JObject(new JProperty("type", "wait-until-player-distant"), new JProperty("objectId", statueId),
								new JProperty("destinationId", poseId), new JProperty("value", 20), new JProperty("seconds", 0)),
							new JObject(new JProperty("type", "move-object-to"), new JProperty("objectId", statueId),
								new JProperty("destinationId", poseId), new JProperty("seconds", 0)),
							new JObject(new JProperty("type", "play-animation"), new JProperty("objectId", statueId),
								new JProperty("animation", pose.GetNameStateAnimPose()))))));
				}
			}
			if (sequences.Count == 0)
				sequences.Add(new JObject(new JProperty("id", "no-statues-exported"), new JProperty("trigger", "manual"),
					new JProperty("actions", new JArray(new JObject(new JProperty("type", "notify"), new JProperty("message", "No FER statues were exported."))))));
			return new JObject(new JProperty("schemaVersion", 1), new JProperty("type", "stageScript"),
				new JProperty("id", "example.core-fer-tiled-port:stage-script/fer-statues"),
				new JProperty("stage", "example.core-fer-tiled-port:stage/fer-tiled-port"), new JProperty("sequences", sequences));
		}

		private static void ClearGeneratedFiles(string i_directory, string i_extension)
		{
			foreach (string file in Directory.GetFiles(i_directory))
				if (System.IO.Path.GetExtension(file).Equals(i_extension, System.StringComparison.OrdinalIgnoreCase))
					File.Delete(file);
		}

		private static Bounds CalculateBounds(GameObject i_root)
		{
			bool found = false; Bounds result = new Bounds(Vector3.zero, Vector3.zero);
			foreach (Platform platform in i_root.GetComponentsInChildren<Platform>(true))
				foreach (Collider2D collider in platform.GetComponents<Collider2D>())
				{
					if (!found) { result = collider.bounds; found = true; } else result.Encapsulate(collider.bounds);
				}
			return found ? result : new Bounds(Vector3.zero, new Vector3(224f, 48f, 0f));
		}

		private static JObject Point(int i_id, string i_name, string i_type, Vector2 i_world, float i_minY,
			float i_centerX, int i_widthPixels, int i_heightPixels, params JProperty[] i_properties)
		{
			JArray properties = new JArray();
			foreach (JProperty property in i_properties) properties.Add(new JObject(new JProperty("name", property.Name),
				new JProperty("type", property.Value.Type == JTokenType.Boolean ? "bool"
					: property.Value.Type == JTokenType.Integer ? "int" : property.Value.Type == JTokenType.Float ? "float" : "string"),
				new JProperty("value", property.Value)));
			return new JObject(new JProperty("id", i_id), new JProperty("name", Clean(i_name)), new JProperty("point", true),
				new JProperty("properties", properties), new JProperty("type", i_type), new JProperty("visible", true),
				new JProperty("x", (i_world.x - i_centerX) * PixelsPerUnit + i_widthPixels * 0.5f),
				new JProperty("y", i_heightPixels - (i_world.y - i_minY) * PixelsPerUnit));
		}

		private static void AddProperty(JObject i_point, string i_name, object i_value)
		{
			JToken value = JToken.FromObject(i_value);
			((JArray)i_point["properties"]).Add(new JObject(new JProperty("name", i_name),
				new JProperty("type", value.Type == JTokenType.Integer ? "int"
					: value.Type == JTokenType.Float ? "float" : "string"), new JProperty("value", value)));
		}

		private static void ExportClimbNavigation(GameObject i_root, JArray io_objects, ref int io_nextId,
			float i_minY, float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			List<NodeConnection> climbs = new List<NodeConnection>();
			HashSet<NavNode> participating = new HashSet<NavNode>();
			foreach (NodeConnection connection in i_root.GetComponentsInChildren<NodeConnection>(true))
			{
				if (connection.GetNodeConnectionType() != NodeConnectionType.Climb) continue;
				NavNode origin = connection.GetComponentInParent<NavNode>();
				NavNode destination = connection.GetNodeDest();
				if (origin == null || destination == null) continue;
				climbs.Add(connection); participating.Add(origin); participating.Add(destination);
			}
			Dictionary<NavNode, string> names = new Dictionary<NavNode, string>();
			List<NavNode> orderedNodes = new List<NavNode>(participating);
			orderedNodes.Sort((left, right) =>
			{
				int x = left.transform.position.x.CompareTo(right.transform.position.x);
				if (x != 0) return x;
				int y = left.transform.position.y.CompareTo(right.transform.position.y);
				return y != 0 ? y : string.CompareOrdinal(left.name, right.name);
			});
			int index = 0;
			foreach (NavNode node in orderedNodes) names.Add(node, "fer-climb-" + index++);
			Dictionary<NavNode, HashSet<NavNode>> bidirectionalLinks = orderedNodes.ToDictionary(
				node => node, node => new HashSet<NavNode>());
			foreach (NodeConnection connection in climbs)
			{
				NavNode origin = connection.GetComponentInParent<NavNode>();
				NavNode destination = connection.GetNodeDest();
				if (origin == null || destination == null || !bidirectionalLinks.ContainsKey(origin)
					|| !bidirectionalLinks.ContainsKey(destination)) continue;
				bidirectionalLinks[origin].Add(destination);
				bidirectionalLinks[destination].Add(origin);
			}
			foreach (NavNode node in orderedNodes)
			{
				List<string> links = bidirectionalLinks[node].Select(target => names[target])
					.OrderBy(target => target, System.StringComparer.Ordinal).ToList();
				List<JProperty> properties = new List<JProperty>
				{
					new JProperty("connectionType", "climb"),
					new JProperty("bidirectional", true)
				};
				if (links.Count > 0) properties.Add(new JProperty("links", string.Join(",", links)));
				io_objects.Add(Point(io_nextId++, names[node], "nav-node", node.transform.position, i_minY,
					i_centerX, i_widthPixels, i_heightPixels, properties.ToArray()));
			}
		}

		private static JObject Rectangle(int i_id, string i_name, string i_type, Bounds i_bounds, float i_minY,
			float i_centerX, int i_widthPixels, int i_heightPixels, params JProperty[] i_properties)
		{
			JArray properties = new JArray();
			foreach (JProperty property in i_properties) properties.Add(new JObject(new JProperty("name", property.Name),
				new JProperty("type", property.Value.Type == JTokenType.Boolean ? "bool" : property.Value.Type == JTokenType.Integer ? "int" : "string"),
				new JProperty("value", property.Value)));
			return new JObject(new JProperty("height", i_bounds.size.y * PixelsPerUnit), new JProperty("id", i_id),
				new JProperty("name", Clean(i_name)), new JProperty("properties", properties), new JProperty("rotation", 0),
				new JProperty("type", i_type), new JProperty("visible", true),
				new JProperty("width", i_bounds.size.x * PixelsPerUnit),
				new JProperty("x", (i_bounds.min.x - i_centerX) * PixelsPerUnit + i_widthPixels * 0.5f),
				new JProperty("y", i_heightPixels - (i_bounds.max.y - i_minY) * PixelsPerUnit));
		}

		private static JObject Decoration(int i_id, SpriteRenderer i_renderer, ArtExport i_art, float i_minY,
			float i_centerX, int i_widthPixels, int i_heightPixels)
		{
			Sprite sprite = i_renderer.sprite;
			Vector3 center = i_renderer.bounds.center;
			Vector3 scale = i_renderer.transform.lossyScale;
			float localWidth = i_renderer.drawMode == SpriteDrawMode.Simple ? sprite.bounds.size.x : i_renderer.size.x;
			float localHeight = i_renderer.drawMode == SpriteDrawMode.Simple ? sprite.bounds.size.y : i_renderer.size.y;
			float width = Mathf.Max(0.001f, localWidth * Mathf.Abs(scale.x) * PixelsPerUnit);
			float height = Mathf.Max(0.001f, localHeight * Mathf.Abs(scale.y) * PixelsPerUnit);
			bool flipX = i_renderer.flipX ^ (scale.x < 0f);
			bool flipY = i_renderer.flipY ^ (scale.y < 0f);
			uint gid = i_art.FirstGid | (flipX ? 0x80000000u : 0u) | (flipY ? 0x40000000u : 0u);
			float rotation = -Mathf.DeltaAngle(0f, i_renderer.transform.eulerAngles.z);
			JArray properties = new JArray(
				Property("sortingLayer", "string", NormalizeSortingLayer(i_renderer.sortingLayerName)),
				Property("sortingOrder", "int", i_renderer.sortingOrder),
				Property("opacity", "float", Mathf.Clamp01(i_renderer.color.a)),
				Property("tintColor", "color", "#" + ColorUtility.ToHtmlStringRGB(i_renderer.color)));
			if (i_renderer.drawMode == SpriteDrawMode.Tiled)
			{
				properties.Add(Property("repeat", "bool", true));
				properties.Add(Property("tileMode", "string", i_renderer.tileMode == SpriteTileMode.Adaptive ? "adaptive" : "continuous"));
				properties.Add(Property("adaptiveModeThreshold", "float", i_renderer.adaptiveModeThreshold));
			}
			return new JObject(new JProperty("gid", gid), new JProperty("height", height),
				new JProperty("id", i_id), new JProperty("name", Clean(i_renderer.name)),
				new JProperty("properties", properties), new JProperty("rotation", rotation),
				new JProperty("type", "decoration"), new JProperty("visible", true),
				new JProperty("width", width),
				new JProperty("x", (center.x - i_centerX) * PixelsPerUnit + i_widthPixels * 0.5f),
				new JProperty("y", i_heightPixels - (center.y - i_minY) * PixelsPerUnit));
		}

		private static JObject Property(string i_name, string i_type, object i_value)
		{
			return new JObject(new JProperty("name", i_name), new JProperty("type", i_type), new JProperty("value", i_value));
		}

		private static string NormalizeSortingLayer(string i_name)
		{
			if (i_name == "Sky" || i_name == "Background" || i_name == "Decoration" || i_name == "Platform") return i_name;
			if (!string.IsNullOrEmpty(i_name) && i_name.IndexOf("background", StringComparison.OrdinalIgnoreCase) >= 0) return "Background";
			if (!string.IsNullOrEmpty(i_name) && i_name.IndexOf("platform", StringComparison.OrdinalIgnoreCase) >= 0) return "Platform";
			return "Decoration";
		}

		private static bool HasSkeletonHierarchy(Transform i_transform)
		{
			for (Transform current = i_transform; current != null; current = current.parent)
				if (current.GetComponent<Skeleton>() != null || current.name.StartsWith("Skeleton", System.StringComparison.OrdinalIgnoreCase))
					return true;
			return false;
		}

		private static bool TryExportSprite(Sprite i_sprite, string i_packRoot, int i_index, uint i_firstGid,
			out ArtExport o_export)
		{
			o_export = null;
			if (i_sprite == null || i_sprite.texture == null) return false;
			Rect rect;
			try { rect = i_sprite.textureRect; }
			catch (Exception exception)
			{
				Debug.LogWarning("[Modding] FER art skipped because its packed sprite rectangle cannot be read: " + i_sprite.name + " (" + exception.Message + ")");
				return false;
			}
			int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
			int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));
			string fileStem = CleanFileName(i_sprite.name) + "-" + i_index.ToString("D3");
			string pngPath = System.IO.Path.Combine(i_packRoot, "assets", "art", fileStem + ".png");
			string tilesetPath = System.IO.Path.Combine(i_packRoot, "tilesets", fileStem + ".tsj");
			Color[] pixels = null;
			try { pixels = i_sprite.texture.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height); }
			catch (Exception exception) when (exception is UnityException || exception is ArgumentException) { }
			if (pixels == null) pixels = ReadSourceTexturePixels(i_sprite, rect, width, height);
			if (pixels != null)
			{
				Texture2D croppedReadable = new Texture2D(width, height, TextureFormat.RGBA32, false);
				try
				{
					croppedReadable.SetPixels(pixels);
					croppedReadable.Apply();
					File.WriteAllBytes(pngPath, croppedReadable.EncodeToPNG());
				}
				finally { UnityEngine.Object.DestroyImmediate(croppedReadable); }
				return WriteTileset(i_firstGid, fileStem, width, height, i_sprite.border, tilesetPath, out o_export);
			}
			if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
			{
				Debug.LogWarning("[Modding] FER art export requires a graphics device when the source texture is not readable: " + i_sprite.name);
				return false;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(i_sprite.texture.width, i_sprite.texture.height, 0, RenderTextureFormat.ARGB32);
			RenderTexture previous = RenderTexture.active;
			Texture2D readable = null;
			Texture2D cropped = null;
			try
			{
				Graphics.Blit(i_sprite.texture, temporary);
				RenderTexture.active = temporary;
				readable = new Texture2D(i_sprite.texture.width, i_sprite.texture.height, TextureFormat.RGBA32, false);
				readable.ReadPixels(new Rect(0, 0, readable.width, readable.height), 0, 0);
				readable.Apply();
				cropped = new Texture2D(width, height, TextureFormat.RGBA32, false);
				cropped.SetPixels(readable.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), width, height));
				cropped.Apply();
				File.WriteAllBytes(pngPath, cropped.EncodeToPNG());
			}
			catch (Exception exception)
			{
				Debug.LogWarning("[Modding] FER art export failed for " + i_sprite.name + ": " + exception.Message);
				return false;
			}
			finally
			{
				RenderTexture.active = previous;
				RenderTexture.ReleaseTemporary(temporary);
				if (readable != null) UnityEngine.Object.DestroyImmediate(readable);
				if (cropped != null) UnityEngine.Object.DestroyImmediate(cropped);
			}
			return WriteTileset(i_firstGid, fileStem, width, height, i_sprite.border, tilesetPath, out o_export);
		}

		private static Color[] ReadSourceTexturePixels(Sprite i_sprite, Rect i_rect, int i_width, int i_height)
		{
			string assetPath = AssetDatabase.GetAssetPath(i_sprite.texture);
			if (string.IsNullOrEmpty(assetPath)) return null;
			string projectRoot = System.IO.Path.GetDirectoryName(Application.dataPath);
			string sourcePath = System.IO.Path.GetFullPath(System.IO.Path.Combine(projectRoot, assetPath));
			if (!File.Exists(sourcePath)) return null;
			Texture2D source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			try
			{
				if (!source.LoadImage(File.ReadAllBytes(sourcePath), false)
					|| source.width != i_sprite.texture.width || source.height != i_sprite.texture.height) return null;
				int x = Mathf.RoundToInt(i_rect.x), y = Mathf.RoundToInt(i_rect.y);
				if (x < 0 || y < 0 || x + i_width > source.width || y + i_height > source.height) return null;
				return source.GetPixels(x, y, i_width, i_height);
			}
			catch (Exception exception) when (exception is IOException || exception is UnityException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				return null;
			}
			finally { UnityEngine.Object.DestroyImmediate(source); }
		}

		private static bool WriteTileset(uint i_firstGid, string i_fileStem, int i_width, int i_height,
			Vector4 i_border, string i_tilesetPath, out ArtExport o_export)
		{
			// A few legacy sprites contain tiny negative or out-of-range borders. Unity tolerates
			// those serialized values, but loose Tiled sprites cannot, so preserve the usable part.
			float left = Mathf.Clamp(i_border.x, 0f, i_width);
			float right = Mathf.Clamp(i_border.z, 0f, i_width - left);
			float bottom = Mathf.Clamp(i_border.y, 0f, i_height);
			float top = Mathf.Clamp(i_border.w, 0f, i_height - bottom);
			Vector4 border = new Vector4(left, bottom, right, top);
			JObject tileset = new JObject(new JProperty("columns", 1),
				new JProperty("image", "../assets/art/" + i_fileStem + ".png"),
				new JProperty("imageheight", i_height), new JProperty("imagewidth", i_width),
				new JProperty("margin", 0), new JProperty("name", i_fileStem), new JProperty("objectalignment", "center"), new JProperty("spacing", 0),
				new JProperty("tilecount", 1), new JProperty("tileheight", i_height), new JProperty("tilewidth", i_width),
				new JProperty("type", "tileset"), new JProperty("version", "1.10"),
				new JProperty("tiledversion", "1.10.2"));
			if (border.sqrMagnitude > 0f)
				tileset.Add(new JProperty("properties", new JArray(
					Property("spriteBorderLeft", "float", border.x),
					Property("spriteBorderBottom", "float", border.y),
					Property("spriteBorderRight", "float", border.z),
					Property("spriteBorderTop", "float", border.w))));
			File.WriteAllText(i_tilesetPath, tileset.ToString(Formatting.Indented));
			o_export = new ArtExport(i_firstGid, i_fileStem);
			return true;
		}

		private static string CleanFileName(string i_value)
		{
			string clean = Clean(i_value);
			char[] characters = clean.ToCharArray();
			for (int index = 0; index < characters.Length; index++)
				if (!char.IsLetterOrDigit(characters[index]) && characters[index] != '-') characters[index] = '-';
			return new string(characters).Trim('-');
		}

		private static SpawnerExport ReadSpawner(Spawner i_spawner, string i_id, List<string> i_requiredOpenDoors)
		{
			SerializedObject serialized = new SerializedObject(i_spawner);
			// The prefab stores placeholders. Main.unity overrides the actual per-spawner
			// enemy list, including Abby behind the basement door.
			List<string> enemies = new List<string>();
			SerializedProperty choices = serialized.FindProperty("m_npcsPossibleToSpawn");
			for (int index = 0; choices != null && index < choices.arraySize; index++)
			{
				NPC npc = choices.GetArrayElementAtIndex(index).objectReferenceValue as NPC;
				string enemyId = EnemyId(npc);
				if (string.IsNullOrEmpty(enemyId))
					throw new InvalidOperationException("FER scene spawner has an unrecognized enemy: " + i_id);
				enemies.Add(enemyId);
			}
			if (enemies.Count == 0) throw new InvalidOperationException("FER scene spawner has no enemy: " + i_id);
			// Jacky's dedicated keycard sequence owns his first appearance. This source
			// spawner must not put him into the ordinary wave pool at stage start.
			return new SpawnerExport(i_id, enemies,
				ReadFloat(serialized, "m_spawnChance01", 1f), ReadInt(serialized, "m_numWaveBeforeCanSpawn", 0),
				ReadFloat(serialized, "m_delayBetweenSpawns", 3f), ReadFloat(serialized, "m_delayRandomOffsetDelaySpawn", 0f),
				ReadFloat(serialized, "m_delayBeforeStartSpawning", 0f), ReadFloat(serialized, "m_delayRandomOffsetBeforeStartSpawning", 0f),
				ReadBool(serialized, "m_isSpawnOutOfSight", true), i_id != "fer-spawner-22" && ReadBool(serialized, "m_isEnabled", true), i_requiredOpenDoors);
		}

		private static float ReadFloat(SerializedObject i_object, string i_name, float i_fallback) { SerializedProperty p = i_object.FindProperty(i_name); return p == null ? i_fallback : p.floatValue; }
		private static int ReadInt(SerializedObject i_object, string i_name, int i_fallback) { SerializedProperty p = i_object.FindProperty(i_name); return p == null ? i_fallback : p.intValue; }
		private static bool ReadBool(SerializedObject i_object, string i_name, bool i_fallback) { SerializedProperty p = i_object.FindProperty(i_name); return p == null ? i_fallback : p.boolValue; }

		private static string EnemyId(string i_name)
		{
			string name = i_name.ToLowerInvariant().Replace(" ", string.Empty).Replace("-", string.Empty).Replace("_", string.Empty);
			if (name.Contains("zombiegrabber")) return "core:enemy/zombie-grabber";
			if (name.Contains("zombie1")) return "core:enemy/zombie-1";
			if (name.Contains("zombie2")) return "core:enemy/zombie-2";
			if (name.Contains("zombie3")) return "core:enemy/zombie-3";
			if (name.Contains("deathhound")) return "core:enemy/death-hound";
			if (name.Contains("goblinmarksman") || name.Contains("marksman")) return "core:enemy/goblin-marksman";
			if (name.Contains("goblintrapper") || name.Contains("trapper")) return "core:enemy/goblin-trapper";
			if (name.Contains("goblinminor") || name.Contains("gremlin")) return "core:enemy/gremlin";
			if (name.Contains("headhumper")) return "core:enemy/head-humper";
			if (name.Contains("spidermother")) return "core:enemy/spider-mother";
			if (name.Contains("litigant")) return "core:enemy/litigant";
			if (name.Contains("maggot")) return "core:enemy/maggot";
			if (name.Contains("musca")) return "core:enemy/musca";
			if (name.Contains("hunter")) return "core:enemy/hunter";
			if (name.Contains("sqoid")) return "core:enemy/sqoid";
			if (name.Contains("orc")) return "core:enemy/orc";
			if (name.Contains("fly")) return "core:enemy/fly";
			if (name.Contains("sunny")) return "core:enemy/sunny";
			if (name.Contains("jenny")) return "core:enemy/jenny";
			if (name.Contains("abby")) return "core:enemy/abby";
			if (name.Contains("jacky")) return "core:enemy/jacky";
			return string.Empty;
		}

		private static string EnemyId(NPC i_npc)
		{
			if (i_npc == null) return string.Empty;
			string[] ids = {
				string.Empty, "core:enemy/zombie-1", "core:enemy/zombie-2", "core:enemy/zombie-grabber",
				"core:enemy/death-hound", "core:enemy/hunter", "core:enemy/maggot", "core:enemy/fly",
				"core:enemy/musca", "core:enemy/orc", "core:enemy/goblin-marksman", "core:enemy/goblin-trapper",
				"core:enemy/gremlin", "core:enemy/litigant", "core:enemy/spider-mother", "core:enemy/head-humper",
				"core:enemy/sqoid", "core:enemy/sunny", "core:enemy/jenny", "core:enemy/abby",
				"core:enemy/jacky", "core:enemy/zombie-3"
			};
			int legacyId = i_npc.GetId();
			return legacyId > 0 && legacyId < ids.Length ? ids[legacyId] : EnemyId(i_npc.name);
		}

		private static string Clean(string i_value)
		{
			string value = (i_value ?? "object").Trim().ToLowerInvariant().Replace(' ', '-').Replace('_', '-');
			return value.Length == 0 ? "object" : value;
		}

		private static string ObjectId(GameObject i_object, Dictionary<GameObject, string> i_objectIds)
		{
			return i_object != null && i_objectIds.TryGetValue(i_object, out string id) ? id : ScriptId(i_object == null ? null : i_object.name);
		}

		private static string ScriptId(string i_value)
		{
			string value = (i_value ?? "object").Trim().ToLowerInvariant();
			System.Text.StringBuilder result = new System.Text.StringBuilder(value.Length);
			bool dash = false;
			foreach (char character in value)
			{
				if (char.IsLetterOrDigit(character)) { result.Append(character); dash = false; }
				else if (!dash && result.Length > 0) { result.Append('-'); dash = true; }
			}
			while (result.Length > 0 && result[result.Length - 1] == '-') result.Length--;
			if (result.Length == 0 || !(char.IsLetter(result[0]) || result[0] == '_')) result.Insert(0, "object-");
			return result.ToString();
		}

		private sealed class ArtExport
		{
			public uint FirstGid { get; }
			public string FileStem { get; }
			public ArtExport(uint i_firstGid, string i_fileStem) { FirstGid = i_firstGid; FileStem = i_fileStem; }
		}

		private sealed class SpawnerExport
		{
			private readonly string m_id; private readonly List<string> m_enemies; private readonly float m_weight;
			private readonly int m_wave; private readonly float m_delay, m_jitter, m_initial, m_initialJitter; private readonly bool m_hidden, m_enabled;
			private readonly List<string> m_requiredOpenDoors;
			public SpawnerExport(string i_id, List<string> i_enemies, float i_weight, int i_wave, float i_delay,
				float i_jitter, float i_initial, float i_initialJitter, bool i_hidden, bool i_enabled, List<string> i_requiredOpenDoors)
			{ m_id=i_id; m_enemies=i_enemies; m_weight=Mathf.Max(0.001f,i_weight); m_wave=i_wave; m_delay=Mathf.Max(0.02f,i_delay);
				m_jitter=Mathf.Clamp(i_jitter,0f,m_delay); m_initial=Mathf.Max(0f,i_initial); m_initialJitter=Mathf.Clamp(i_initialJitter,0f,m_initial); m_hidden=i_hidden; m_enabled=i_enabled;
				m_requiredOpenDoors=i_requiredOpenDoors ?? new List<string>(); }
			public JObject ToJson() { return new JObject(new JProperty("id",m_id),new JProperty("position",new JObject(new JProperty("x",0),new JProperty("y",0))),
				new JProperty("enemies",new JArray(m_enemies)),new JProperty("selectionWeight",m_weight),new JProperty("minimumWave",m_wave),
				new JProperty("delaySeconds",m_delay),new JProperty("delayJitterSeconds",m_jitter),new JProperty("initialDelaySeconds",m_initial),
				new JProperty("initialDelayJitterSeconds",m_initialJitter),new JProperty("spawnOutOfSight",m_hidden),new JProperty("enabled",m_enabled),
				new JProperty("requiredOpenDoors",new JArray(m_requiredOpenDoors))); }
		}
	}
}
