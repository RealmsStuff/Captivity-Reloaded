using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CaptivityReloaded.Modding.Tests
{
	public class ModLoaderBootstrapTests
	{
		private static Type FindRuntimeType(string i_name)
		{
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type type = assembly.GetType(i_name, throwOnError: false);
				if (type != null) return type;
			}
			return null;
		}

		private static object Invoke(object i_target, string i_method, params object[] i_arguments)
		{
			MethodInfo method = i_target.GetType().GetMethod(i_method,
				BindingFlags.Instance | BindingFlags.Public);
			Assert.That(method, Is.Not.Null, i_target.GetType().FullName + "." + i_method);
			return method.Invoke(i_target, i_arguments);
		}

		[UnityTest]
		public IEnumerator BeforeSceneLoad_LoadsPackagedCoreCatalog()
		{
			yield return null;
			Assert.That(ModLoaderRuntime.LoadedPacks.Any(pack => pack.Manifest.Id == "core"), Is.True);
			Assert.That(ModLoaderRuntime.CoreContentCatalog.Count, Is.GreaterThan(0));
			Assert.That(ModLoaderRuntime.LegacyContentMap.Count, Is.GreaterThan(0));
			Assert.That(ModLoaderRuntime.Registry, Is.Not.Null);
			Assert.That(ModLoaderRuntime.AssetSlots, Is.Not.Null);
			Assert.That(ModLoaderRuntime.EnemyDefinitions, Is.Not.Null);
			Assert.That(ModLoaderRuntime.ClothingDefinitions, Is.Not.Null);
			Assert.That(ModLoaderRuntime.WeaponDefinitions, Is.Not.Null);
			Assert.That(ModLoaderRuntime.UsableDefinitions, Is.Not.Null);
			Assert.That(ModLoaderRuntime.StageDefinitions, Is.Not.Null);
		}

		[UnityTest]
		public IEnumerator LoadedPacks_HaveUniqueStableIds()
		{
			yield return null;
			string[] ids = ModLoaderRuntime.LoadedPacks.Select(pack => pack.Manifest.Id).ToArray();
			Assert.That(ids, Is.Not.Empty);
			Assert.That(ids.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(ids.Length));
			Assert.That(ids, Does.Contain("core"));
		}

		[Test]
		public void CoreCatalog_LegacyIdsRoundTripThroughStableContentIds()
		{
			CoreContentCatalogEntry entry = ModLoaderRuntime.CoreContentCatalog.First(item => item.LegacyId.HasValue);
			Assert.That(ModLoaderRuntime.LegacyContentMap.TryGetContentId(entry.Category,
				entry.LegacyId.Value, out ContentId stableId), Is.True);
			Assert.That(stableId, Is.EqualTo(entry.Id));
			Assert.That(ModLoaderRuntime.LegacyContentMap.TryGetLegacyKey(stableId,
				out LegacyContentKey legacyKey), Is.True);
			Assert.That(legacyKey.Category, Is.EqualTo(entry.Category));
			Assert.That(legacyKey.Id, Is.EqualTo(entry.LegacyId.Value));
		}

		[Test]
		public void SavedContent_ReconnectsWhenTheSameIdReturns()
		{
			ContentId id = ContentId.Parse("release.test:enemy/persistent");
			SavedContentState state = new SavedContentState(id, ContentCategory.Enemy, "{\"kills\":7}");
			Assert.That(ContentSaveResolver.Resolve(state, new ContentRegistry()).Status,
				Is.EqualTo(SavedContentStatus.Missing));

			ContentRegistry restored = new ContentRegistry();
			restored.Register(new ContentRegistration(id, ContentCategory.Enemy,
				"release.test", "content/enemy.json"), new ValidationReport());
			SavedContentResolution resolution = ContentSaveResolver.Resolve(state, restored);
			Assert.That(resolution.Status, Is.EqualTo(SavedContentStatus.Available));
			Assert.That(resolution.State.StateJson, Is.EqualTo("{\"kills\":7}"));
		}

		[Test]
		public void ModEnableState_PersistsDisableAndReenable()
		{
			const string id = "release.test.enable-state";
			const string key = "CaptivityReloaded.ModEnabled." + id;
			PlayerPrefs.DeleteKey(key);
			try
			{
				Assert.That(ModEnableState.IsEnabled(id), Is.True);
				ModEnableState.SetEnabled(id, false);
				Assert.That(ModEnableState.IsEnabled(id), Is.False);
				ModEnableState.SetEnabled(id, true);
				Assert.That(ModEnableState.IsEnabled(id), Is.True);
			}
			finally { PlayerPrefs.DeleteKey(key); }
		}

		[Test]
		public void ModContentUnlockState_PersistsByStableId()
		{
			ContentId id = ContentId.Parse("release.test:clothing/unlock-state");
			string key = "CaptivityReloaded.ContentUnlocked." + id;
			PlayerPrefs.DeleteKey(key);
			try
			{
				Assert.That(ModContentUnlockState.IsUnlocked(id), Is.False);
				ModContentUnlockState.Unlock(id);
				Assert.That(ModContentUnlockState.IsUnlocked(id), Is.True);
			}
			finally { PlayerPrefs.DeleteKey(key); }
		}

		[UnityTest]
		public IEnumerator StageFallRecoveryThreshold_UsesSafeSpawnOffsetWithoutGeometry()
		{
			Type stageType = FindRuntimeType("Stage");
			Assert.That(stageType, Is.Not.Null);
			GameObject root = new GameObject("release-test-stage-empty");
			try
			{
				Component stage = root.AddComponent(stageType);
				Invoke(stage, "ConfigureModStage", -1, "Release Test", string.Empty, Vector2.zero);
				float threshold = (float)Invoke(stage, "GetFallRecoveryY");
				Assert.That(threshold, Is.EqualTo(-25f).Within(0.001f));
			}
			finally { UnityEngine.Object.Destroy(root); }
			yield return null;
		}

		[UnityTest]
		public IEnumerator StageFallRecoveryThreshold_IsBelowLowestPlatform()
		{
			Type stageType = FindRuntimeType("Stage");
			Assert.That(stageType, Is.Not.Null);
			int platformLayer = LayerMask.NameToLayer("Platform");
			Assert.That(platformLayer, Is.GreaterThanOrEqualTo(0));
			GameObject root = new GameObject("release-test-stage-platform");
			try
			{
				Component stage = root.AddComponent(stageType);
				GameObject platform = new GameObject("lowest-platform");
				platform.layer = platformLayer;
				platform.transform.SetParent(root.transform, false);
				platform.transform.localPosition = new Vector3(0f, -5f, 0f);
				BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
				collider.size = new Vector2(4f, 2f);
				Invoke(stage, "ConfigureModStage", -1, "Release Test", string.Empty, Vector2.zero);
				float threshold = (float)Invoke(stage, "GetFallRecoveryY");
				Assert.That(threshold, Is.LessThan(collider.bounds.min.y));
				Assert.That(threshold, Is.EqualTo(collider.bounds.min.y - 10f).Within(0.001f));
			}
			finally { UnityEngine.Object.Destroy(root); }
			yield return null;
		}

		[Test]
		public void PackagedCoreDefinitions_AreRegisteredByStableId()
		{
			List<ContentId> definitions = new List<ContentId>();
			definitions.AddRange(ModLoaderRuntime.CoreEnemyDefinitions.Select(item => item.Id));
			definitions.AddRange(ModLoaderRuntime.CoreStageDefinitions.Select(item => item.Id));
			definitions.AddRange(ModLoaderRuntime.CoreClothingDefinitions.Select(item => item.Id));
			definitions.AddRange(ModLoaderRuntime.CoreItemDefinitions.Select(item => item.Id));
			definitions.AddRange(ModLoaderRuntime.CoreChallengeDefinitions.Select(item => item.Id));
			Assert.That(definitions, Is.Not.Empty);
			foreach (ContentId id in definitions)
				Assert.That(ModLoaderRuntime.Registry.TryGet(id, out ContentRegistration registration),
					Is.True, id.ToString());
		}
	}
}
