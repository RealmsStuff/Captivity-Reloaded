using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class CoreEnemyAssetSlotCatalogTests
	{
		[Test]
		public void GeneratedCatalog_CoversEveryRendererInEveryNormalizedCoreRig()
		{
			string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string catalogPath = Path.Combine(projectRoot, "Assets", "Resources", "Modding", "Core", "enemy-asset-slots.json");
			JObject catalog = JObject.Parse(File.ReadAllText(catalogPath));
			Assert.That((int)catalog["schemaVersion"], Is.EqualTo(1));
			Assert.That((string)catalog["type"], Is.EqualTo("coreEnemyAssetSlotCatalog"));
			JArray enemies = (JArray)catalog["enemies"];
			Assert.That(enemies.Count, Is.EqualTo(21));

			int covered = 0;
			foreach (JObject enemy in enemies.OfType<JObject>())
			{
				string enemyId = (string)enemy["id"];
				string rigPath = Path.Combine(projectRoot, ((string)enemy["sourceRig"]).Replace('/', Path.DirectorySeparatorChar));
				JObject rig = JObject.Parse(File.ReadAllText(rigPath));
				Assert.That((string)rig["enemy"], Is.EqualTo(enemyId));

				string sampleRoot = (string)rig["sampleRoot"] ?? string.Empty;
				HashSet<string> rigRenderers = new HashSet<string>(
					((JArray)rig["bones"]).OfType<JObject>()
						.SelectMany(i_bone => ((JArray)i_bone["sprites"] ?? new JArray()).Values<JObject>())
						.Select(i_sprite => CombineUnityPath(sampleRoot, (string)i_sprite["unityPath"])), StringComparer.Ordinal);
				JArray slots = (JArray)enemy["slots"];
				HashSet<string> catalogRenderers = new HashSet<string>(slots.OfType<JObject>()
					.Select(i_slot => (string)i_slot["rendererPath"]), StringComparer.Ordinal);
				Assert.That(catalogRenderers, Is.EquivalentTo(rigRenderers), enemyId + " has unreachable or stale sprite renderers.");
				Assert.That(slots.Values<string>("slot").Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(slots.Count), enemyId + " has duplicate slots.");
				Assert.That(slots.Values<string>("slot").All(i_slot => ContentId.TryParse(enemyId + "/" + i_slot, out _)), Is.True, enemyId);

				string prefabPath = (string)rig["prefab"];
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				Assert.That(prefab, Is.Not.Null, enemyId + " prefab");
				foreach (JObject slot in slots.OfType<JObject>())
				{
					string rendererPath = (string)slot["rendererPath"];
					Transform target = prefab.transform.Find(rendererPath);
					Assert.That(target, Is.Not.Null, enemyId + "/" + slot["slot"] + " path " + rendererPath);
					Assert.That(target.GetComponent<SpriteRenderer>(), Is.Not.Null, enemyId + "/" + slot["slot"]);
				}

				HashSet<string> slotNames = new HashSet<string>(slots.Values<string>("slot"), StringComparer.Ordinal);
				foreach (JObject alias in ((JArray)enemy["legacyAliases"]).OfType<JObject>())
				{
					Assert.That(ContentId.TryParse(enemyId + "/" + (string)alias["slot"], out _), Is.True);
					Assert.That(((JArray)alias["targets"]).Values<string>().All(slotNames.Contains), Is.True, enemyId + "/" + alias["slot"]);
				}
				covered += slots.Count;
			}
			Assert.That((int)catalog["enemyCount"], Is.EqualTo(enemies.Count));
			Assert.That((int)catalog["slotCount"], Is.EqualTo(covered));
		}

		private static string CombineUnityPath(string i_parent, string i_child)
		{
			if (string.IsNullOrEmpty(i_parent)) return i_child ?? string.Empty;
			if (string.IsNullOrEmpty(i_child)) return i_parent;
			return i_parent.TrimEnd('/') + "/" + i_child.TrimStart('/');
		}

		[Test]
		public void GeneratedCatalog_PublishesEnemySpecificMultipartAnatomyAndJennyArtwork()
		{
			CoreEnemyAssetSlotCatalogDocument catalog = CoreEnemyAssetSlotCatalog.Document;
			CoreEnemyAssetSlotEnemy jenny = catalog.Enemies.Single(i_enemy => i_enemy.Id == "core:enemy/jenny");
			HashSet<string> slots = new HashSet<string>(jenny.Slots.Select(i_slot => i_slot.Slot), StringComparer.Ordinal);
			Assert.That(slots, Does.Contain("body/l-foot-base"));
			Assert.That(slots, Does.Contain("body/r-foot-base"));
			Assert.That(slots, Does.Contain("body/l-foot-end"));
			Assert.That(slots, Does.Contain("body/r-foot-end"));
			Assert.That(slots, Does.Contain("body/breast"));
			Assert.That(slots, Does.Contain("body/breast-bg"));
			Assert.That(slots, Does.Contain("body/penis-rod"));
			Assert.That(slots, Does.Contain("body/penis-base"));
			Assert.That(slots, Does.Contain("body/penis-end"));

			foreach (CoreEnemyAssetSlotEnemy enemy in catalog.Enemies)
			{
				HashSet<string> enemySlots = new HashSet<string>(enemy.Slots.Select(i_slot => i_slot.Slot), StringComparer.Ordinal);
				foreach (CoreEnemyAssetSlotRecord anatomy in enemy.Slots.Where(i_slot =>
					i_slot.Bone.IndexOf("penis", StringComparison.Ordinal) >= 0 || i_slot.Bone == "balls"))
					Assert.That(enemySlots.Contains(anatomy.Slot), Is.True, enemy.Id + " " + anatomy.Bone);
			}

			CoreEnemyAssetSlotRecord fearHead = catalog.Enemies.Single(i_enemy => i_enemy.Id == "core:enemy/jacky")
				.Slots.Single(i_slot => i_slot.Slot == "body/head/variant/head-fear");
			Assert.That(fearHead.AnimatedSpriteName, Is.EqualTo("HeadFear"));
			Assert.That(fearHead.SourceAnimation, Does.EndWith("jacky/scare.json"));
		}

		[Test]
		public void RuntimeBinding_ReplacesEnemyScopedAnimationOnlySprite()
		{
			PropertyInfo slotsProperty = typeof(ModLoaderRuntime).GetProperty("AssetSlots", BindingFlags.Static | BindingFlags.Public);
			PropertyInfo reportProperty = typeof(ModLoaderRuntime).GetProperty("LastReport", BindingFlags.Static | BindingFlags.Public);
			AssetSlotRegistry previousSlots = ModLoaderRuntime.AssetSlots;
			ValidationReport previousReport = ModLoaderRuntime.LastReport;
			GameObject jackyObject = null;
			Texture2D texture = null;
			Sprite replacement = null;
			Type binder = Type.GetType("CoreAssetSlotBinder, Assembly-CSharp");
			MethodInfo reset = binder?.GetMethod("ResetRuntimeState", BindingFlags.Static | BindingFlags.NonPublic);
			try
			{
				reset?.Invoke(null, null);
				slotsProperty.SetValue(null, new AssetSlotRegistry());
				reportProperty.SetValue(null, new ValidationReport());
				Type npcType = Type.GetType("NPC, Assembly-CSharp");
				MethodInfo register = binder.GetMethod("RegisterCoreEnemySlots", BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo applyAnimated = binder.GetMethod("ApplyAnimatedNamedSpriteReplacements", BindingFlags.Static | BindingFlags.NonPublic);
				Sprite fearBaseline = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/HeadFear.asset");
				jackyObject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_jacky.prefab"));
				register.Invoke(null, new object[] { jackyObject.GetComponent(npcType), ContentId.Parse("core:enemy/jacky") });

				ContentId slotId = ContentId.Parse("core:enemy/jacky/body/head/variant/head-fear");
				Assert.That(ModLoaderRuntime.AssetSlots.TryGet(slotId, out AssetSlotRegistration slot), Is.True);
				Assert.That(slot.BaselineAsset, Is.SameAs(fearBaseline));
				CoreEnemyAssetSlotRecord record = CoreEnemyAssetSlotCatalog.Document.Enemies.Single(i_enemy => i_enemy.Id == "core:enemy/jacky")
					.Slots.Single(i_slot => i_slot.Slot == "body/head/variant/head-fear");
				SpriteRenderer renderer = jackyObject.transform.Find(record.RendererPath).GetComponent<SpriteRenderer>();
				texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				replacement = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f), fearBaseline.pixelsPerUnit);
				ModLoaderRuntime.AssetSlots.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("test.enemy-slots:patch/fear-head"), slotId,
						"test.enemy-slots", "test", replacement, true)
				}, ModLoaderRuntime.LastReport);
				renderer.sprite = fearBaseline;
				applyAnimated.Invoke(null, null);
				Assert.That(renderer.sprite, Is.SameAs(replacement));
				Assert.That(ModLoaderRuntime.LastReport.IsValid, Is.True);
			}
			finally
			{
				reset?.Invoke(null, null);
				slotsProperty.SetValue(null, previousSlots);
				reportProperty.SetValue(null, previousReport);
				if (jackyObject != null) UnityEngine.Object.DestroyImmediate(jackyObject);
				if (replacement != null) UnityEngine.Object.DestroyImmediate(replacement);
				if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
			}
		}

		[Test]
		public void RuntimeBinding_UsesExactPathsAndKeepsSharedAnatomyEnemySpecific()
		{
			PropertyInfo slotsProperty = typeof(ModLoaderRuntime).GetProperty("AssetSlots", BindingFlags.Static | BindingFlags.Public);
			PropertyInfo reportProperty = typeof(ModLoaderRuntime).GetProperty("LastReport", BindingFlags.Static | BindingFlags.Public);
			AssetSlotRegistry previousSlots = ModLoaderRuntime.AssetSlots;
			ValidationReport previousReport = ModLoaderRuntime.LastReport;
			AssetSlotRegistry slots = new AssetSlotRegistry();
			GameObject jennyObject = null;
			GameObject abbyObject = null;
			Texture2D texture = null;
			Sprite replacement = null;
			try
			{
				slotsProperty.SetValue(null, slots);
				reportProperty.SetValue(null, new ValidationReport());
				Type binder = Type.GetType("CoreAssetSlotBinder, Assembly-CSharp");
				Assert.That(binder, Is.Not.Null);
				Type npcType = Type.GetType("NPC, Assembly-CSharp");
				Assert.That(npcType, Is.Not.Null);
				MethodInfo register = binder.GetMethod("RegisterCoreEnemySlots", BindingFlags.Static | BindingFlags.NonPublic);
				Assert.That(register, Is.Not.Null);

				jennyObject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_jenny.prefab"));
				abbyObject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_abby.prefab"));
				register.Invoke(null, new object[] { jennyObject.GetComponent(npcType), ContentId.Parse("core:enemy/jenny") });
				register.Invoke(null, new object[] { abbyObject.GetComponent(npcType), ContentId.Parse("core:enemy/abby") });

				Assert.That(slots.TryGet(ContentId.Parse("core:enemy/jenny/body/l-foot-base"), out _), Is.True);
				Assert.That(slots.TryGet(ContentId.Parse("core:enemy/jenny/body/breast"), out _), Is.True);
				Assert.That(slots.TryGet(ContentId.Parse("core:enemy/jenny/body/arm-upper"), out _), Is.True, "legacy alias");
				Assert.That(slots.TryGet(ContentId.Parse("core:enemy/jenny/body/penis-rod"), out AssetSlotRegistration rodSlot), Is.True);

				CoreEnemyAssetSlotRecord jennyRod = CoreEnemyAssetSlotCatalog.Document.Enemies.Single(i_enemy => i_enemy.Id == "core:enemy/jenny")
					.Slots.Single(i_slot => i_slot.Slot == "body/penis-rod");
				CoreEnemyAssetSlotRecord abbyRod = CoreEnemyAssetSlotCatalog.Document.Enemies.Single(i_enemy => i_enemy.Id == "core:enemy/abby")
					.Slots.Single(i_slot => i_slot.Slot == "body/penis-rod");
				SpriteRenderer jennyRenderer = jennyObject.transform.Find(jennyRod.RendererPath).GetComponent<SpriteRenderer>();
				SpriteRenderer abbyRenderer = abbyObject.transform.Find(abbyRod.RendererPath).GetComponent<SpriteRenderer>();
				Sprite abbyBaseline = abbyRenderer.sprite;
				texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				replacement = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(.5f, .5f), ((Sprite)rodSlot.BaselineAsset).pixelsPerUnit);
				slots.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("test.enemy-slots:patch/jenny"),
						ContentId.Parse("core:enemy/jenny/body/penis-rod"), "test.enemy-slots", "test", replacement, true)
				}, ModLoaderRuntime.LastReport);
				Assert.That(jennyRenderer.sprite, Is.SameAs(replacement));
				Assert.That(abbyRenderer.sprite, Is.SameAs(abbyBaseline), "Jenny's formerly shared PenisRod_0 patch must not affect Abby.");
				Assert.That(ModLoaderRuntime.LastReport.IsValid, Is.True);
			}
			finally
			{
				slotsProperty.SetValue(null, previousSlots);
				reportProperty.SetValue(null, previousReport);
				if (jennyObject != null) UnityEngine.Object.DestroyImmediate(jennyObject);
				if (abbyObject != null) UnityEngine.Object.DestroyImmediate(abbyObject);
				if (replacement != null) UnityEngine.Object.DestroyImmediate(replacement);
				if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
			}
		}
	}
}
