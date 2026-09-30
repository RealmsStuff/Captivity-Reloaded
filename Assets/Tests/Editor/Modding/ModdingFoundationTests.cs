using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public class ContentIdTests
	{
		[Test]
		public void StickCircleGesture_RequiresContinuousRotation()
		{
			object gesture = CreateStickCircleGesture();
			bool completed = false;
			for (int degrees = 0; degrees <= 330; degrees += 30)
			{
				float radians = degrees * Mathf.Deg2Rad;
				completed |= UpdateStickCircleGesture(gesture, new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
			}
			Assert.That(completed, Is.True);
		}

		[Test]
		public void StickCircleGesture_BackAndForthDoesNotComplete()
		{
			object gesture = CreateStickCircleGesture();
			bool completed = false;
			for (int index = 0; index < 20; index++)
			{
				completed |= UpdateStickCircleGesture(gesture, index % 2 == 0 ? Vector2.right : Vector2.up);
			}
			Assert.That(completed, Is.False);
		}

		[Test]
		public void StickCircleGesture_ReturningToDeadzoneResetsProgress()
		{
			object gesture = CreateStickCircleGesture();
			for (int degrees = 0; degrees <= 180; degrees += 30)
			{
				float radians = degrees * Mathf.Deg2Rad;
				UpdateStickCircleGesture(gesture, new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
			}
			UpdateStickCircleGesture(gesture, Vector2.zero);
			bool completed = false;
			for (int degrees = 180; degrees <= 360; degrees += 30)
			{
				float radians = degrees * Mathf.Deg2Rad;
				completed |= UpdateStickCircleGesture(gesture, new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)));
			}
			Assert.That(completed, Is.False);
		}

		private static object CreateStickCircleGesture()
		{
			System.Type type = System.Type.GetType("StickCircleGesture, Assembly-CSharp");
			Assert.That(type, Is.Not.Null);
			return System.Activator.CreateInstance(type);
		}

		private static bool UpdateStickCircleGesture(object i_gesture, Vector2 i_input)
		{
			return (bool)i_gesture.GetType().GetMethod("Update").Invoke(i_gesture, new object[] { i_input });
		}

		[Test]
		public void RuntimeIdentity_SurvivesInstantiationWithItsStableId()
		{
			GameObject template = new GameObject("Content identity template");
			GameObject clone = null;
			try
			{
				RuntimeContentIdentity identity = template.AddComponent<RuntimeContentIdentity>();
				ContentId id = ContentId.Parse("example.pack:enemy/test");
				identity.Configure(id, ContentCategory.Enemy);
				clone = Object.Instantiate(template);
				Assert.That(RuntimeContentIdentity.TryResolve(clone.transform, out ContentId resolved, out ContentCategory category), Is.True);
				Assert.That(resolved, Is.EqualTo(id));
				Assert.That(category, Is.EqualTo(ContentCategory.Enemy));
			}
			finally
			{
				if (clone != null) Object.DestroyImmediate(clone);
				Object.DestroyImmediate(template);
			}
		}

		[TestCase("core:enemy/gremlin")]
		[TestCase("example.pack:patch/player-art")]
		[TestCase("author_name:stage/field-day")]
		public void TryParse_AcceptsValidIds(string i_value)
		{
			Assert.That(ContentId.TryParse(i_value, out ContentId parsed), Is.True);
			Assert.That(parsed.ToString(), Is.EqualTo(i_value));
		}

		[TestCase("Core:enemy/gremlin")]
		[TestCase("core")]
		[TestCase("core:")]
		[TestCase("core:/enemy")]
		[TestCase("core:enemy//gremlin")]
		[TestCase("core:enemy/../gremlin")]
		[TestCase("core:enemy/Gremlin")]
		public void TryParse_RejectsInvalidIds(string i_value)
		{
			Assert.That(ContentId.TryParse(i_value, out _), Is.False);
		}
	}

	public class SemanticVersionTests
	{
		[Test]
		public void VersionRange_HandlesExactAndMinimumVersions()
		{
			Assert.That(SemanticVersion.TryParse("1.2.3", out SemanticVersion installed), Is.True);
			Assert.That(VersionRange.TryParse("1.2.3", out VersionRange exact), Is.True);
			Assert.That(VersionRange.TryParse(">=1.0.0", out VersionRange minimum), Is.True);
			Assert.That(exact.Contains(installed), Is.True);
			Assert.That(minimum.Contains(installed), Is.True);
		}

		[Test]
		public void StableVersion_SortsAfterPreRelease()
		{
			SemanticVersion.TryParse("1.0.0", out SemanticVersion stable);
			SemanticVersion.TryParse("1.0.0-beta.1", out SemanticVersion beta);
			Assert.That(stable.CompareTo(beta), Is.GreaterThan(0));
		}
	}

	public class ManifestParserTests
	{
		private const string ValidManifest = @"{
  'schemaVersion': 1,
  'id': 'example.pack',
  'displayName': 'Example Pack',
  'version': '1.0.0',
  'modApiVersion': 1,
  'dependencies': [{ 'id': 'core', 'version': '>=0.1.0' }],
  'contentRoots': ['content']
}";

		[Test]
		public void Parse_AcceptsValidExternalManifest()
		{
			ManifestLoadResult result = ModManifestParser.Parse(ValidManifest, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Manifest.Id, Is.EqualTo("example.pack"));
		}

		[Test]
		public void Parse_AcceptsOptionalDependencies()
		{
			string json = ValidManifest.Replace("'contentRoots'", "'optionalDependencies': [{ 'id': 'example.integration', 'version': '>=2.0.0' }], 'contentRoots'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Manifest.OptionalDependencies.Single().Id, Is.EqualTo("example.integration"));
		}

		[Test]
		public void Parse_AcceptsOrderedPreviewImages()
		{
			string json = ValidManifest.Replace("'contentRoots'", "'description': 'A visible description.', 'previewImages': ['assets/one.png', 'assets/two.png'], 'contentRoots'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Manifest.PreviewImages, Is.EqualTo(new[] { "assets/one.png", "assets/two.png" }));
		}

		[TestCase("../outside.png")]
		[TestCase("C:/outside.png")]
		[TestCase("assets/preview.jpg")]
		public void Parse_RejectsUnsafePreviewImage(string i_path)
		{
			string json = ValidManifest.Replace("'contentRoots'", "'previewImages': ['" + i_path + "'], 'contentRoots'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "manifest.preview-path"), Is.True);
		}

		[Test]
		public void Parse_RejectsDependencyDeclaredAsRequiredAndOptional()
		{
			string json = ValidManifest.Replace("'contentRoots'", "'optionalDependencies': [{ 'id': 'core', 'version': '>=0.1.0' }], 'contentRoots'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "manifest.duplicate-dependency"), Is.True);
		}

		[Test]
		public void PackagedCoreManifest_IsPresentAndValid()
		{
			TextAsset asset = Resources.Load<TextAsset>("Modding/Core/manifest");
			Assert.That(asset, Is.Not.Null);
			ManifestLoadResult result = ModManifestParser.Parse(asset.text, "core/manifest.json", i_isCore: true);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Manifest.Id, Is.EqualTo("core"));
		}

		[Test]
		public void Parse_RejectsUnknownFields()
		{
			string json = ValidManifest.Replace("'contentRoots'", "'unexpected': true, 'contentRoots'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "manifest.json"), Is.True);
		}

		[Test]
		public void Parse_RejectsReservedCoreIdForExternalPack()
		{
			string json = ValidManifest.Replace("'example.pack'", "'core'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "manifest.reserved-id"), Is.True);
		}

		[TestCase("../outside")]
		[TestCase("content/../outside")]
		[TestCase("C:/outside")]
		[TestCase("content\\outside")]
		public void Parse_RejectsUnsafeContentRoots(string i_root)
		{
			string json = ValidManifest.Replace("'content'", "'" + i_root.Replace("\\", "\\\\") + "'");
			ManifestLoadResult result = ModManifestParser.Parse(json, "manifest.json", i_isCore: false);
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "manifest.content-root-path"), Is.True);
		}
	}

	public class DependencyResolverTests
	{
		[Test]
		public void Resolve_OrdersDependenciesBeforeDependents()
		{
			ModPack core = CreatePack("core", "0.1.0");
			ModPack addon = CreatePack("example.addon", "1.0.0", new ModDependency { Id = "core", Version = ">=0.1.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon, core });
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.OrderedPacks.Select(pack => pack.Manifest.Id), Is.EqualTo(new[] { "core", "example.addon" }));
		}

		[Test]
		public void Resolve_ReportsMissingDependency()
		{
			ModPack addon = CreatePack("example.addon", "1.0.0", new ModDependency { Id = "core", Version = ">=0.1.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon });
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.missing"), Is.True);
		}

		[Test]
		public void Resolve_ReportsDependencyCycle()
		{
			ModPack first = CreatePack("example.first", "1.0.0", new ModDependency { Id = "example.second", Version = "1.0.0" });
			ModPack second = CreatePack("example.second", "1.0.0", new ModDependency { Id = "example.first", Version = "1.0.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { first, second });
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.cycle"), Is.True);
		}

		[Test]
		public void Resolve_HonorsSoftOrderingAndPriority()
		{
			ModPack first = CreatePack("example.first", "1.0.0");
			ModPack second = CreatePack("example.second", "1.0.0");
			second.Manifest.LoadAfter.Add(first.Manifest.Id);
			first.Manifest.Priority = 50;
			second.Manifest.Priority = -50;
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { second, first });
			Assert.That(result.OrderedPacks.Select(pack => pack.Manifest.Id), Is.EqualTo(new[] { "example.first", "example.second" }));
		}

		[Test]
		public void Resolve_DisablesDeterministicConflictLoser()
		{
			ModPack winner = CreatePack("example.winner", "1.0.0");
			ModPack loser = CreatePack("example.loser", "1.0.0");
			winner.Manifest.Priority = 10;
			winner.Manifest.Conflicts.Add(loser.Manifest.Id);
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { loser, winner });
			Assert.That(result.OrderedPacks.Select(pack => pack.Manifest.Id), Is.EqualTo(new[] { "example.winner" }));
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.conflict"), Is.True);
			Assert.That(result.Statuses.Single(status => status.Id == "example.loser").State, Is.EqualTo(ModPackState.Conflicting));
		}

		[Test]
		public void Resolve_MissingOptionalDependencyDoesNotDisablePack()
		{
			ModPack addon = CreatePack("example.addon", "1.0.0");
			addon.Manifest.OptionalDependencies.Add(new ModDependency { Id = "example.integration", Version = ">=1.0.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon });
			Assert.That(result.OrderedPacks.Single(), Is.SameAs(addon));
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.optional-missing"), Is.True);
			Assert.That(result.Statuses.Single().State, Is.EqualTo(ModPackState.Loaded));
		}

		[Test]
		public void Resolve_OptionalDependencyOrdersBeforeConsumerWhenAvailable()
		{
			ModPack integration = CreatePack("example.integration", "2.0.0");
			ModPack addon = CreatePack("example.addon", "1.0.0");
			addon.Manifest.OptionalDependencies.Add(new ModDependency { Id = integration.Manifest.Id, Version = ">=1.0.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon, integration });
			Assert.That(result.OrderedPacks.Select(pack => pack.Manifest.Id), Is.EqualTo(new[] { "example.integration", "example.addon" }));
		}

		[Test]
		public void Resolve_DisabledRequiredDependencyDisablesConsumerWithClearStatus()
		{
			ModPack framework = CreatePack("example.framework", "1.0.0");
			ModPack addon = CreatePack("example.addon", "1.0.0", new ModDependency { Id = framework.Manifest.Id, Version = ">=1.0.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon, framework }, id => id != framework.Manifest.Id);
			Assert.That(result.OrderedPacks, Is.Empty);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.disabled"), Is.True);
			Assert.That(result.Statuses.Single(status => status.Id == framework.Manifest.Id).Reason, Does.Contain("user"));
			Assert.That(result.Statuses.Single(status => status.Id == addon.Manifest.Id).State, Is.EqualTo(ModPackState.Disabled));
		}

		[Test]
		public void Resolve_DisabledOptionalDependencyKeepsConsumerLoaded()
		{
			ModPack integration = CreatePack("example.integration", "1.0.0");
			ModPack addon = CreatePack("example.addon", "1.0.0");
			addon.Manifest.OptionalDependencies.Add(new ModDependency { Id = integration.Manifest.Id, Version = ">=1.0.0" });
			DependencyResolutionResult result = ModDependencyResolver.Resolve(new[] { addon, integration }, id => id != integration.Manifest.Id);
			Assert.That(result.OrderedPacks.Select(pack => pack.Manifest.Id), Is.EqualTo(new[] { "example.addon" }));
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "dependency.optional-disabled"), Is.True);
		}

		private static ModPack CreatePack(string i_id, string i_version, params ModDependency[] i_dependencies)
		{
			SemanticVersion.TryParse(i_version, out SemanticVersion version);
			ModManifest manifest = new ModManifest
			{
				SchemaVersion = 1,
				Id = i_id,
				DisplayName = i_id,
				Version = i_version,
				ModApiVersion = 1,
				Dependencies = new List<ModDependency>(i_dependencies),
				ContentRoots = new List<string> { "content" }
			};
			return new ModPack(manifest, version, i_id);
		}
	}

	public class DifficultyDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversVanillaDifficultiesFromJson()
		{
			TextAsset[] assets = Resources.LoadAll<TextAsset>("Modding/Core/Content");
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(assets);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Difficulties.Select(item => item.Id.ToString()), Is.EquivalentTo(new[]
			{
				"core:difficulty/casual", "core:difficulty/normal", "core:difficulty/hard"
			}));
			DifficultyDefinition hard = result.Difficulties.Single(item => item.Id == ContentId.Parse("core:difficulty/hard"));
			Assert.That(hard.EnemyHealthMultiplier, Is.EqualTo(1.25f));
			Assert.That(hard.PlayerDamageTakenMultiplier, Is.EqualTo(1.25f));
			Assert.That(hard.EscapeStrengthMultiplier, Is.EqualTo(0.75f));
		}

		[Test]
		public void Parse_AcceptsExtendedDifficultyMultipliers()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'difficulty',
  'id': 'example.difficulty:difficulty/nightmare', 'displayName': 'Nightmare', 'sortOrder': 400,
  'enemyHealthMultiplier': 1.5, 'playerDamageTakenMultiplier': 1.5, 'escapeStrengthMultiplier': 0.25
}";
			DifficultyDefinitionLoadResult result = DifficultyDefinitionParser.Parse(json, "example.difficulty", "nightmare.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.EnemyHealthMultiplier, Is.EqualTo(1.5f));
			Assert.That(result.Definition.PlayerDamageTakenMultiplier, Is.EqualTo(1.5f));
			Assert.That(result.Definition.EscapeStrengthMultiplier, Is.EqualTo(0.25f));
		}

		[TestCase(0f)]
		[TestCase(10.1f)]
		public void Parse_RejectsUnsafeMultipliers(float i_multiplier)
		{
			string json = "{'schemaVersion':1,'type':'difficulty','id':'example.difficulty:difficulty/test','displayName':'Test','enemyHealthMultiplier':" +
				i_multiplier.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",'playerDamageTakenMultiplier':1,'escapeStrengthMultiplier':1}";
			DifficultyDefinitionLoadResult result = DifficultyDefinitionParser.Parse(json, "example.difficulty", "test.json");
			Assert.That(result.Report.IsValid, Is.False);
		}
	}

	public class ContentRegistryTests
	{
		private sealed class RuntimeAssetStub : ScriptableObject
		{
		}

		[Test]
		public void Register_RejectsDuplicateContentId()
		{
			ContentRegistry registry = new ContentRegistry();
			ValidationReport report = new ValidationReport();
			ContentRegistration registration = new ContentRegistration(ContentId.Parse("example.pack:enemy/test"), ContentCategory.Enemy, "example.pack", "enemy.json");
			Assert.That(registry.Register(registration, report), Is.True);
			Assert.That(registry.Register(registration, report), Is.False);
			Assert.That(report.Issues.Any(issue => issue.Code == "registry.duplicate"), Is.True);
		}

		[Test]
		public void Register_RejectsNamespaceMismatch()
		{
			ContentRegistry registry = new ContentRegistry();
			ValidationReport report = new ValidationReport();
			ContentRegistration registration = new ContentRegistration(ContentId.Parse("other.pack:enemy/test"), ContentCategory.Enemy, "example.pack", "enemy.json");
			Assert.That(registry.Register(registration, report), Is.False);
			Assert.That(report.Issues.Any(issue => issue.Code == "registry.namespace"), Is.True);
		}

		[Test]
		public void Register_PreservesRuntimeAssetReference()
		{
			RuntimeAssetStub asset = ScriptableObject.CreateInstance<RuntimeAssetStub>();
			try
			{
				ContentRegistration registration = new ContentRegistration(ContentId.Parse("core:enemy/gremlin"), ContentCategory.Enemy, "core", "core/catalog.json", asset);
				ContentRegistry registry = new ContentRegistry();
				Assert.That(registry.Register(registration, new ValidationReport()), Is.True);
				Assert.That(registry.TryGet(ContentId.Parse("core:enemy/gremlin"), out ContentRegistration resolved), Is.True);
				Assert.That(resolved.RuntimeAsset, Is.SameAs(asset));
			}
			finally
			{
				Object.DestroyImmediate(asset);
			}
		}

		[Test]
		public void BindRuntimeAsset_UpgradesAValidatedDataRegistration()
		{
			RuntimeAssetStub asset = ScriptableObject.CreateInstance<RuntimeAssetStub>();
			try
			{
				ContentId id = ContentId.Parse("example.pack:enemy/atlas-enemy");
				ContentRegistry registry = new ContentRegistry();
				registry.Register(new ContentRegistration(id, ContentCategory.Enemy, "example.pack", "enemy.json"), new ValidationReport());
				Assert.That(registry.BindRuntimeAsset(id, asset, new ValidationReport()), Is.True);
				Assert.That(registry.TryGet(id, out ContentRegistration resolved), Is.True);
				Assert.That(resolved.RuntimeAsset, Is.SameAs(asset));
			}
			finally
			{
				Object.DestroyImmediate(asset);
			}
		}
	}

	public class AssetSlotRegistryTests
	{
		[Test]
		public void Register_RejectsDuplicateAndNamespaceMismatch()
		{
			Texture2D baseline = new Texture2D(1, 1);
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				ValidationReport report = new ValidationReport();
				AssetSlotRegistration slot = CreateSlot("core:weapon/pistol/body", baseline);
				Assert.That(registry.Register(slot, report), Is.True);
				Assert.That(registry.Register(slot, report), Is.False);
				Assert.That(registry.Register(new AssetSlotRegistration(
					ContentId.Parse("other:weapon/pistol/slide"),
					ContentId.Parse("core:item/weapon/pistol"),
					"core", "test", baseline), report), Is.False);
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-slot.duplicate"), Is.True);
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-slot.namespace"), Is.True);
			}
			finally
			{
				Object.DestroyImmediate(baseline);
			}
		}

		[Test]
		public void Resolve_AppliesOneExplicitCompatiblePatch()
		{
			Texture2D baseline = new Texture2D(1, 1);
			Texture2D replacement = new Texture2D(2, 2);
			UnityEngine.Object applied = null;
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				AssetSlotRegistration slot = new AssetSlotRegistration(
					ContentId.Parse("core:weapon/pistol/body"),
					ContentId.Parse("core:item/weapon/pistol"),
					"core", "test", baseline, typeof(Texture2D), asset => applied = asset);
				registry.Register(slot, new ValidationReport());
				ValidationReport report = new ValidationReport();
				registry.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("example.nerf:patch/pistol"), slot.Id, "example.nerf", "patch.json", replacement)
				}, report);
				Assert.That(report.IsValid, Is.True);
				Assert.That(slot.ResolvedAsset, Is.SameAs(replacement));
				Assert.That(applied, Is.SameAs(replacement));
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(replacement);
			}
		}

		[Test]
		public void AddBinding_ImmediatelyReceivesCurrentResolvedAsset()
		{
			Texture2D baseline = new Texture2D(1, 1);
			Texture2D replacement = new Texture2D(2, 2);
			UnityEngine.Object lateBound = null;
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				AssetSlotRegistration slot = CreateSlot("core:weapon/pistol/body", baseline);
				registry.Register(slot, new ValidationReport());
				registry.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("example.mod:patch/pistol"), slot.Id, "example.mod", "patch.json", replacement)
				}, new ValidationReport());
				slot.AddBinding(asset => lateBound = asset);
				Assert.That(lateBound, Is.SameAs(replacement));
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(replacement);
			}
		}

		[Test]
		public void Resolve_ConflictingPatchesRetainCoreBaseline()
		{
			Texture2D baseline = new Texture2D(1, 1);
			Texture2D first = new Texture2D(2, 2);
			Texture2D second = new Texture2D(3, 3);
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				AssetSlotRegistration slot = CreateSlot("core:weapon/pistol/body", baseline);
				registry.Register(slot, new ValidationReport());
				ValidationReport report = new ValidationReport();
				registry.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("example.first:patch/pistol"), slot.Id, "example.first", "first.json", first),
					new AssetPatchRequest(ContentId.Parse("example.second:patch/pistol"), slot.Id, "example.second", "second.json", second)
				}, report);
				Assert.That(slot.ResolvedAsset, Is.SameAs(baseline));
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-patch.conflict"), Is.True);
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(first);
				Object.DestroyImmediate(second);
			}
		}

		[Test]
		public void Resolve_LaterExplicitOverrideWinsConflict()
		{
			Texture2D baseline = new Texture2D(1, 1);
			Texture2D first = new Texture2D(2, 2);
			Texture2D second = new Texture2D(3, 3);
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				AssetSlotRegistration slot = CreateSlot("core:weapon/pistol/body", baseline);
				registry.Register(slot, new ValidationReport());
				ValidationReport report = new ValidationReport();
				registry.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("example.first:patch/pistol"), slot.Id, "example.first", "first.json", first),
					new AssetPatchRequest(ContentId.Parse("example.second:patch/pistol"), slot.Id, "example.second", "second.json", second, true)
				}, report);
				Assert.That(slot.ResolvedAsset, Is.SameAs(second));
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-patch.override"), Is.True);
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(first);
				Object.DestroyImmediate(second);
			}
		}

		[Test]
		public void Resolve_RejectsUnknownSlotsAndWrongAssetTypes()
		{
			Texture2D baseline = new Texture2D(1, 1);
			AudioClip wrongType = AudioClip.Create("test", 1, 1, 44100, false);
			try
			{
				AssetSlotRegistry registry = new AssetSlotRegistry();
				AssetSlotRegistration slot = CreateSlot("core:weapon/pistol/body", baseline);
				registry.Register(slot, new ValidationReport());
				ValidationReport report = new ValidationReport();
				registry.Resolve(new[]
				{
					new AssetPatchRequest(ContentId.Parse("example.mod:patch/unknown"), ContentId.Parse("core:weapon/pistol/unknown"), "example.mod", "unknown.json", baseline),
					new AssetPatchRequest(ContentId.Parse("example.mod:patch/wrong-type"), slot.Id, "example.mod", "wrong.json", wrongType)
				}, report);
				Assert.That(slot.ResolvedAsset, Is.SameAs(baseline));
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-patch.target"), Is.True);
				Assert.That(report.Issues.Any(issue => issue.Code == "asset-patch.type"), Is.True);
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(wrongType);
			}
		}

		private static AssetSlotRegistration CreateSlot(string i_id, Texture2D i_baseline)
		{
			return new AssetSlotRegistration(
				ContentId.Parse(i_id),
				ContentId.Parse("core:item/weapon/pistol"),
				"core", "test", i_baseline, typeof(Texture2D));
		}
	}

	public class AssetPatchParserTests
	{
		private const string ValidPatch = @"{
  'schemaVersion': 1,
  'type': 'assetPatch',
  'id': 'example.nerf:patch/starter-pistol',
  'target': 'core:weapon/pistol',
  'replacements': {
    'body': 'assets/pistol/body.png',
    'slide': 'assets/pistol/slide.png',
    'base': 'assets/pistol/base.png'
  }
}";

		[Test]
		public void Parse_ExpandsRelativeKeysIntoStablePublicSlots()
		{
			AssetPatchLoadResult result = AssetPatchParser.Parse(ValidPatch, "example.nerf", "patch.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.nerf:patch/starter-pistol")));
			Assert.That(result.Definition.Replacements.Select(item => item.SlotId), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:weapon/pistol/body"),
				ContentId.Parse("core:weapon/pistol/slide"),
				ContentId.Parse("core:weapon/pistol/base")
			}));
		}

		[TestCase("../outside.png")]
		[TestCase("assets\\outside.png")]
		[TestCase("C:/outside.png")]
		[TestCase("assets/not-a-png.txt")]
		public void Parse_RejectsUnsafeOrUnsupportedAssetPaths(string i_path)
		{
			string json = ValidPatch.Replace("assets/pistol/body.png", i_path.Replace("\\", "\\\\"));
			AssetPatchLoadResult result = AssetPatchParser.Parse(json, "example.nerf", "patch.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "asset-patch.asset-path"), Is.True);
		}

		[Test]
		public void Parse_RejectsForeignPatchNamespaceAndUnsafeSlotKey()
		{
			string json = ValidPatch
				.Replace("example.nerf:patch/starter-pistol", "other.pack:patch/starter-pistol")
				.Replace("'body':", "'../body':");
			AssetPatchLoadResult result = AssetPatchParser.Parse(json, "example.nerf", "patch.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "asset-patch.id"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "asset-patch.slot"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownFields()
		{
			AssetPatchLoadResult result = AssetPatchParser.Parse(ValidPatch.Replace("'target'", "'unexpected': true, 'target'"), "example.nerf", "patch.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "asset-patch.json"), Is.True);
		}
	}

	public class RuntimeSpritePatchLoaderTests
	{
		[Test]
		public void ReplacementPivot_ForFullSourceCanvas_PreservesAbsoluteTexturePivot()
		{
			Texture2D texture = new Texture2D(32, 32);
			Sprite baseline = Sprite.Create(texture, new Rect(10, 1, 13, 25), new Vector2(0.53f, 0.79f), 32f);
			try
			{
				Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(baseline, 32, 32);
				Assert.That(pivot.x, Is.EqualTo((baseline.rect.x + baseline.pivot.x) / 32f).Within(0.0001f));
				Assert.That(pivot.y, Is.EqualTo((baseline.rect.y + baseline.pivot.y) / 32f).Within(0.0001f));
				Assert.That(pivot.y, Is.Not.EqualTo(baseline.pivot.y / baseline.rect.height).Within(0.01f));
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(texture);
			}
		}

		[Test]
		public void ConvertedLegacyNerfExample_DiscoversAndDecodesAllThreeSlots()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack nerf = packs.Packs.Single(pack => pack.Manifest.Id == "somescrub.simple-nerf-gun");
			AssetPatchDiscoveryResult definitions = AssetPatchDiscovery.Discover(new[] { nerf });
			Assert.That(definitions.Report.IsValid, Is.True);
			Assert.That(definitions.Definitions, Has.Count.EqualTo(1));

			Texture2D baselineTexture = new Texture2D(32, 32);
			List<Sprite> baselines = new List<Sprite>();
			List<AssetPatchRequest> requests = null;
			try
			{
				AssetSlotRegistry slots = new AssetSlotRegistry();
				foreach (string part in new[] { "body", "slide", "base" })
				{
					Sprite baseline = Sprite.Create(baselineTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
					baselines.Add(baseline);
					slots.Register(new AssetSlotRegistration(
						ContentId.Parse("core:weapon/pistol/" + part), ContentId.Parse("core:item/weapon/pistol"),
						"core", "test", baseline, typeof(Sprite)), new ValidationReport());
				}
				ValidationReport report = new ValidationReport();
				requests = RuntimeSpritePatchLoader.Load(definitions.Definitions, new[] { nerf }, slots, report);
				Assert.That(report.IsValid, Is.True);
				Assert.That(requests, Has.Count.EqualTo(3));
				Assert.That(requests.All(request => ((Sprite)request.ReplacementAsset).texture.filterMode == FilterMode.Point), Is.True);
				Assert.That(requests.Select(request => request.TargetSlotId), Is.EquivalentTo(new[]
				{
					ContentId.Parse("core:weapon/pistol/body"),
					ContentId.Parse("core:weapon/pistol/slide"),
					ContentId.Parse("core:weapon/pistol/base")
				}));
			}
			finally
			{
				if (requests != null)
				{
					foreach (AssetPatchRequest request in requests)
					{
						Sprite sprite = (Sprite)request.ReplacementAsset;
						Texture2D texture = sprite.texture;
						Object.DestroyImmediate(sprite);
						Object.DestroyImmediate(texture);
					}
				}
				foreach (Sprite baseline in baselines) Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(baselineTexture);
			}
		}

		[Test]
		public void ConvertedShadedGirlExample_DiscoversAndDecodesAllBodySlots()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack shaded = packs.Packs.Single(pack => pack.Manifest.Id == "dudleytheschemer.shaded-girl");
			AssetPatchDiscoveryResult definitions = AssetPatchDiscovery.Discover(new[] { shaded });
			Assert.That(definitions.Report.IsValid, Is.True);
			Assert.That(definitions.Definitions.Single().Replacements, Has.Count.EqualTo(52));

			Texture2D baselineTexture = new Texture2D(32, 32);
			List<Sprite> baselines = new List<Sprite>();
			List<AssetPatchRequest> requests = null;
			try
			{
				AssetSlotRegistry slots = new AssetSlotRegistry();
				foreach (AssetReplacementDefinition replacement in definitions.Definitions.Single().Replacements)
				{
					Sprite baseline = Sprite.Create(baselineTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
					baselines.Add(baseline);
					slots.Register(new AssetSlotRegistration(replacement.SlotId, ContentId.Parse("core:player"),
						"core", "test", baseline, typeof(Sprite)), new ValidationReport());
				}
				ValidationReport report = new ValidationReport();
				requests = RuntimeSpritePatchLoader.Load(definitions.Definitions, new[] { shaded }, slots, report);
				Assert.That(report.IsValid, Is.True);
				Assert.That(requests, Has.Count.EqualTo(52));
				Assert.That(requests.Select(request => request.TargetSlotId).Distinct().Count(), Is.EqualTo(52));
			}
			finally
			{
				DestroyRuntimeRequests(requests);
				foreach (Sprite baseline in baselines) Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(baselineTexture);
			}
		}

		[Test]
		public void Load_DecodesPackRelativePngAndPreservesSpriteScale()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ModdingAssetPatchTests"));
			string assetDirectory = Path.Combine(root, "assets");
			Directory.CreateDirectory(assetDirectory);
			Texture2D sourceTexture = new Texture2D(4, 6);
			Texture2D baselineTexture = new Texture2D(8, 8);
			Sprite baseline = Sprite.Create(baselineTexture, new Rect(0, 0, 8, 8), new Vector2(0.25f, 0.75f), 32f);
			try
			{
				File.WriteAllBytes(Path.Combine(assetDirectory, "body.png"), sourceTexture.EncodeToPNG());
				ModPack pack = CreatePack(root);
				AssetPatchLoadResult parsed = AssetPatchParser.Parse(@"{
  'schemaVersion': 1,
  'type': 'assetPatch',
  'id': 'example.nerf:patch/pistol',
  'target': 'core:weapon/pistol',
  'replacements': { 'body': 'assets/body.png' }
}", "example.nerf", "patch.json");
				AssetSlotRegistry slots = new AssetSlotRegistry();
				slots.Register(new AssetSlotRegistration(
					ContentId.Parse("core:weapon/pistol/body"), ContentId.Parse("core:item/weapon/pistol"),
					"core", "test", baseline, typeof(Sprite)), new ValidationReport());
				ValidationReport report = new ValidationReport();
				List<AssetPatchRequest> requests = RuntimeSpritePatchLoader.Load(new[] { parsed.Definition }, new[] { pack }, slots, report);
				Assert.That(report.IsValid, Is.True);
				Assert.That(requests, Has.Count.EqualTo(1));
				Sprite loaded = (Sprite)requests[0].ReplacementAsset;
				Assert.That(loaded.texture.width, Is.EqualTo(4));
				Assert.That(loaded.texture.height, Is.EqualTo(6));
				Assert.That(loaded.pixelsPerUnit, Is.EqualTo(32f));
				Assert.That(loaded.pivot.x / loaded.rect.width, Is.EqualTo(0.25f).Within(0.001f));
				Assert.That(loaded.pivot.y / loaded.rect.height, Is.EqualTo(0.75f).Within(0.001f));
				Texture2D loadedTexture = loaded.texture;
				Object.DestroyImmediate(loaded);
				Object.DestroyImmediate(loadedTexture);
			}
			finally
			{
				Object.DestroyImmediate(baseline);
				Object.DestroyImmediate(baselineTexture);
				Object.DestroyImmediate(sourceTexture);
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		private static ModPack CreatePack(string i_root)
		{
			SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
			return new ModPack(new ModManifest
			{
				SchemaVersion = 1,
				Id = "example.nerf",
				DisplayName = "Example Nerf",
				Version = "1.0.0",
				ModApiVersion = 1,
				ContentRoots = new List<string> { "content" }
			}, version, i_root);
		}

		private static void DestroyRuntimeRequests(IEnumerable<AssetPatchRequest> i_requests)
		{
			if (i_requests == null) return;
			foreach (AssetPatchRequest request in i_requests)
			{
				Sprite sprite = request.ReplacementAsset as Sprite;
				if (sprite == null) continue;
				Texture2D texture = sprite.texture;
				Object.DestroyImmediate(sprite);
				Object.DestroyImmediate(texture);
			}
		}
	}

	public class EnemyDefinitionParserTests
	{
		private const string ValidEnemy = @"{
  'schemaVersion': 1,
  'type': 'enemy',
  'id': 'example.enemies:enemy/acid-gremlin',
  'displayName': 'Acid Gremlin',
  'extends': 'core:enemy/gremlin',
  'description': 'A data-driven test enemy.',
  'stats': {
    'healthMax': 24,
    'speedAcceleration': 12,
    'speedMax': 4.5,
    'traction': 0.8,
    'bounty': 15,
    'healthIncreasePerWave': 2
  },
  'visual': {
    'type': 'coreRigAtlas',
    'atlas': 'assets/enemies/acid-gremlin.png',
    'pixelsPerUnit': 32,
    'regions': {
      'body/head': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 },
      'body/torso': { 'x': 32, 'y': 0, 'width': 32, 'height': 32 }
    }
  }
}";

		[Test]
		public void Parse_AcceptsCoreRigAtlasEnemyWithBoundedOverrides()
		{
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(ValidEnemy, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.enemies:enemy/acid-gremlin")));
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:enemy/gremlin")));
			Assert.That(result.Definition.Visual.Regions, Has.Count.EqualTo(2));
			Assert.That(result.Definition.Spawn.InheritTemplateSpawners, Is.False);
		}

		[Test]
		public void Parse_AcceptsInheritedEnemyWithoutVisualOverride()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemy',
  'id': 'example.enemies:enemy/animation-reference',
  'displayName': 'Animation Reference',
  'extends': 'core:enemy/zombie-1',
  'spawn': { 'inheritTemplateSpawners': false },
  'animationRefs': { 'idle': 'example.enemies:enemy-animation/zombie/idle' }
}";
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json,
				"example.enemies", "animation-reference.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Visual, Is.Null);
			Assert.That(result.Definition.AnimationReferences["idle"],
				Is.EqualTo("example.enemies:enemy-animation/zombie/idle"));
		}

		[Test]
		public void Parse_AcceptsTypedAttackBehaviorAnimationAndDrops()
		{
			string json = ValidEnemy.Replace("'visual': {", @"'behavior': { 'visionRange': 30, 'ignoreWave': false },
  'animation': { 'speedMultiplier': 1.2 },
  'attacks': [{ 'index': 0, 'damage': 8, 'cooldownSeconds': 1.5 }],
  'drops': { 'chance': 0.1, 'items': ['core:item/consumable/ammo-box'] },
  'visual': {");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Attacks.Single().Damage, Is.EqualTo(8));
			Assert.That(result.Definition.Drops.Items.Single(), Is.EqualTo("core:item/consumable/ammo-box"));
		}

		[Test]
		public void Parse_AcceptsBoundedInheritedSpawnerSelectionWeight()
		{
			string json = ValidEnemy.Replace("'visual': {",
				"'spawn': { 'inheritTemplateSpawners': true, 'selectionWeight': 0.25 }, 'visual': {");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Spawn.SelectionWeight, Is.EqualTo(0.25f));

			EnemyDefinitionLoadResult invalid = EnemyDefinitionParser.Parse(
				json.Replace("'selectionWeight': 0.25", "'selectionWeight': 0"), "example.enemies", "enemy.json");
			Assert.That(invalid.Report.IsValid, Is.False);
		}

		[Test]
		public void Parse_RejectsForeignNamespaceAndUnsafeAtlas()
		{
			string json = ValidEnemy
				.Replace("example.enemies:enemy/acid-gremlin", "other.pack:enemy/acid-gremlin")
				.Replace("assets/enemies/acid-gremlin.png", "../acid-gremlin.png");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.id"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.visual.atlas"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeRegionsAndOutOfRangeStats()
		{
			string json = ValidEnemy
				.Replace("'healthMax': 24", "'healthMax': 0")
				.Replace("'traction': 0.8", "'traction': 2")
				.Replace("'body/head'", "'../head'")
				.Replace("'width': 32", "'width': 0");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.stats.health-max"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.stats.traction"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.visual.region-name"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.visual.region-rect"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownGameplayFieldsAndUnsupportedVisualTypes()
		{
			string json = ValidEnemy
				.Replace("'healthMax'", "'arbitraryScript': 'Hack.dll', 'healthMax'")
				.Replace("'coreRigAtlas'", "'arbitraryPrefab'");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy.json"), Is.True);
		}

		[Test]
		public void Parse_AcceptsBoundedBehaviorModules()
		{
			string json = ValidEnemy.Replace("'visual': {", "'behavior': { 'modules': [{ 'type': 'regeneration', 'amount': 2, 'intervalSeconds': 1 }, { 'type': 'berserk', 'healthThreshold': 0.2, 'speedBonus': 1 }, { 'type': 'thorns', 'amount': 2 }, { 'type': 'onHitRagdoll', 'durationSeconds': 0.5 }, { 'type': 'spawnOnDeath', 'enemy': 'core:enemy/zombie-1', 'count': 2, 'radius': 1 }, { 'type': 'speedPulse', 'speedBonus': 1, 'intervalSeconds': 3, 'durationSeconds': 1 }] }, 'visual': {");
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "enemy.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.Modules, Has.Count.EqualTo(6));
		}

		[Test]
		public void Parse_AcceptsFullyOriginalSkeletonEnemy()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/stalker', 'displayName': 'Stalker',
  'stats': { 'healthMax': 50, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [{ 'type': 'downedFinisher', 'triggerRange': 1.5, 'durationSeconds': 8,
    'meterMax': 100, 'inputPower': 10, 'statuses': { 'active': { 'title': 'Pinned', 'description': '{enemy} caught you!' }, 'birth': { 'description': 'Born: {child}' } },
	'playerAnimation': { 'durationSeconds': 0.8, 'loop': true,
      'frames': [{ 'time': 0, 'bones': { 'chest': { 'rotation': -5 } } }, { 'time': 0.8, 'bones': { 'chest': { 'rotation': 5 } } }],
			  'events': [{ 'time': 0.4, 'type': 'pleasure', 'amount': 2 },
			    { 'time': 0.4, 'type': 'scaledPleasure', 'amount': 0.2 }, { 'time': 0.4, 'type': 'libido', 'amount': 1 },
			    { 'time': 0.4, 'type': 'strengthDamage', 'amount': 1 }, { 'time': 0.4, 'type': 'struggleDamage', 'amount': 10 },
			    { 'time': 0.4, 'type': 'healthDamage', 'amount': 1 }] } }] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'retreatRange': 0.5, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'swipe', 'type': 'melee', 'animation': 'swipe',
    'chance': 1, 'damage': 8, 'cooldownSeconds': 1, 'initiateRange': 1.5, 'hitRange': 1.2, 'durationSeconds': 0.5 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 0.5, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] },
    'swipe': { 'durationSeconds': 0.5, 'frames': [{ 'time': 0.2, 'bones': { 'head': { 'region': 'head-attack', 'rotation': 15 } } }],
      'events': [{ 'time': 0.05, 'type': 'sound', 'file': 'assets/swipe.ogg', 'volume': 0.8 },
        { 'time': 0.1, 'type': 'impulse', 'x': 0.5 }, { 'time': 0.15, 'type': 'cameraShake', 'amount': 0.2 },
        { 'time': 0.18, 'type': 'spriteEffect', 'region': 'head-attack', 'bone': 'head', 'durationSeconds': 0.1 },
        { 'time': 0.2, 'type': 'attackHit' }] }
  } },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/stalker.png', 'pixelsPerUnit': 32,
    'bodyWidth': 0.8, 'bodyHeight': 2,
    'regions': { 'hips': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 }, 'head': { 'x': 32, 'y': 0, 'width': 32, 'height': 32 }, 'head-attack': { 'x': 64, 'y': 0, 'width': 32, 'height': 32 } },
    'bones': [{ 'id': 'hips', 'region': 'hips' }, { 'id': 'head', 'parent': 'hips', 'region': 'head', 'y': 0.8 }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 0.8, 'height': 0.8 }, { 'bone': 'head', 'shape': 'circle', 'radius': 0.35, 'damageMultiplier': 'critical' }]
  }
}";
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(json, "example.enemies", "stalker.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Definition.Extends, Is.Null);
			Assert.That(result.Definition.Visual.Bones, Has.Count.EqualTo(2));
			Assert.That(result.Definition.Visual.HitZones.Single(zone => zone.Bone == "head").DamageMultiplier, Is.EqualTo("critical"));
			Assert.That(result.Definition.Animation.Clips.ContainsKey("swipe"), Is.True);
			Assert.That(result.Definition.Animation.Clips["swipe"].Frames[0].Bones["head"].Region, Is.EqualTo("head-attack"));
			Assert.That(result.Definition.Animation.Clips["swipe"].Events.Select(item => item.Type),
				Is.EqualTo(new[] { "sound", "impulse", "cameraShake", "spriteEffect", "attackHit" }));
			Assert.That(result.Definition.Ai.Type, Is.EqualTo("groundChase"));
			Assert.That(result.Definition.Ai.RetreatRange, Is.EqualTo(0.5f));
			Assert.That(result.Definition.Behavior.Modules.Single().PlayerAnimation.Events.Select(item => item.Type),
				Is.EqualTo(new[] { "pleasure", "scaledPleasure", "libido", "strengthDamage", "struggleDamage", "healthDamage" }));
			Assert.That(result.Definition.Behavior.Modules.Single().PlayerAnimation.Events[0].Amount, Is.EqualTo(2f));
			Assert.That(result.Definition.Behavior.Modules.Single().Statuses["active"].Title, Is.EqualTo("Pinned"));

			EnemyDefinitionLoadResult finisherEvent = EnemyDefinitionParser.Parse(
				json.Replace("'durationSeconds': 8,", "'durationSeconds': 8, 'animation': 'finisher',")
					.Replace("'idle': {", "'finisher': { 'durationSeconds': 0.5, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }], 'events': [{ 'time': 0.2, 'type': 'cameraShake', 'amount': 0.1 }] }, 'idle': {"),
				"example.enemies", "stalker-finisher-event.json");
			Assert.That(finisherEvent.Report.IsValid, Is.True,
				string.Join("\n", finisherEvent.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));

			EnemyDefinitionLoadResult invalidEvent = EnemyDefinitionParser.Parse(
				json.Replace("'type': 'scaledPleasure', 'amount': 0.2", "'type': 'scaledPleasure', 'amount': 101"), "example.enemies", "stalker-invalid-event.json");
			Assert.That(invalidEvent.Report.IsValid, Is.False);
			Assert.That(invalidEvent.Report.Issues.Any(issue => issue.Code == "enemy.behavior.finisher-player-event-amount"), Is.True);

			EnemyDefinitionLoadResult invalidEnemyEvent = EnemyDefinitionParser.Parse(
				json.Replace("'type': 'attackHit'", "'type': 'arbitraryMethod'"), "example.enemies", "stalker-invalid-enemy-event.json");
			Assert.That(invalidEnemyEvent.Report.IsValid, Is.False);
			Assert.That(invalidEnemyEvent.Report.Issues.Any(issue => issue.Code == "enemy.animation.event-type"), Is.True);
		}
	}

	public class NormalizedPlayerAnimationParserTests
	{
		private const string ValidAnimation = @"{
  'schemaVersion': 1, 'type': 'playerAnimation',
  'id': 'example.animations:player-animation/test-finisher',
  'rig': 'core:player-rig/alex', 'displayName': 'Test Finisher',
  'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [
    { 'target': 'bone/chest', 'property': 'rotation.z', 'keys': [
      { 'time': 0, 'value': -10, 'inTangent': null, 'outTangent': null },
      { 'time': 1, 'value': 10, 'inTangent': null, 'outTangent': null }
    ] },
    { 'target': 'bone/hips', 'property': 'position.y', 'keys': [
      { 'time': 0.5, 'value': 0.25, 'inTangent': null, 'outTangent': null }
    ] }
  ],
  'events': [{ 'time': 0.5, 'name': 'pleasure', 'floatValue': 2 }]
}";

		[Test]
		public void Parse_ConvertsNormalizedTracksToRuntimeFinisherFrames()
		{
			NormalizedPlayerAnimationLoadResult result = NormalizedPlayerAnimationParser.Parse(
				ValidAnimation, "example.animations", "test-animation.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			EnemyAnimationClipDefinition clip = result.Definition.CreateFinisherClip();
			Assert.That(clip.Frames.Select(frame => frame.Time), Is.EqualTo(new[] { 0f, 0.5f, 1f }));
			Assert.That(clip.Frames[1].Bones["chest"].Rotation, Is.EqualTo(0f).Within(0.001f));
			Assert.That(clip.Frames[1].Bones["hips"].Y, Is.EqualTo(0.25f));
			Assert.That(clip.Events.Single().Type, Is.EqualTo("pleasure"));
			Assert.That(clip.Events.Single().Amount, Is.EqualTo(2f));
		}

		[Test]
		public void Parse_RejectsForeignIdsUnsafeEventsAndDuplicateTracks()
		{
			string invalid = ValidAnimation
				.Replace("example.animations:player-animation/test-finisher", "other.pack:player-animation/test-finisher")
				.Replace("'name': 'pleasure'", "'name': 'ArbitraryUnityMethod'")
				.Replace("'events':", "'tracks': [{ 'target': 'bone/chest', 'property': 'rotation.z', 'keys': [{ 'time': 0, 'value': 0, 'inTangent': null, 'outTangent': null }] }], 'events':");
			NormalizedPlayerAnimationLoadResult result = NormalizedPlayerAnimationParser.Parse(
				invalid, "example.animations", "invalid-animation.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-animation.id"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-animation.event-name"), Is.True);
		}
	}

	public class NormalizedEnemyAnimationParserTests
	{
		[Test]
		public void PackagedCore_DiscoversNormalizedEnemyAnimations()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'core:enemy-animation/zombie-1/idle', 'enemy': 'core:enemy/zombie-1',
  'displayName': 'Zombie I idle', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [{ 'target': 'bone/hips', 'property': 'position.y', 'keys': [{ 'time': 0, 'value': 0 }] }]
}";
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(new[] { new TextAsset(json) });
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(result.EnemyAnimations, Has.Count.EqualTo(1));
			Assert.That(result.EnemyAnimations[0].Enemy, Is.EqualTo(ContentId.Parse("core:enemy/zombie-1")));
		}

		[Test]
		public void Registry_GroupsEveryAnimationByItsOwningEnemy()
		{
			const string zombieIdle = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'core:enemy-animation/zombie-1/idle', 'enemy': 'core:enemy/zombie-1',
  'displayName': 'Idle', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [{ 'target': 'bone/hips', 'property': 'position.y', 'keys': [{ 'time': 0, 'value': 0 }] }]
}";
			const string zombieMove = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'core:enemy-animation/zombie-1/move', 'enemy': 'core:enemy/zombie-1',
  'displayName': 'Move', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [{ 'target': 'bone/hips', 'property': 'position.x', 'keys': [{ 'time': 0, 'value': 0 }] }]
}";
			const string flyIdle = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'core:enemy-animation/fly/idle', 'enemy': 'core:enemy/fly',
  'displayName': 'Idle', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [{ 'target': 'bone/hips', 'property': 'position.y', 'keys': [{ 'time': 0, 'value': 0 }] }]
}";
			NormalizedEnemyAnimationLoadResult first = NormalizedEnemyAnimationParser.Parse(zombieIdle, "core", "zombie-idle.json");
			NormalizedEnemyAnimationLoadResult second = NormalizedEnemyAnimationParser.Parse(zombieMove, "core", "zombie-move.json");
			NormalizedEnemyAnimationLoadResult third = NormalizedEnemyAnimationParser.Parse(flyIdle, "core", "fly-idle.json");
			ValidationReport report = new ValidationReport();
			NormalizedEnemyAnimationRegistry.Initialize(new[] { first.Definition, second.Definition, third.Definition }, report);

			Assert.That(report.IsValid, Is.True);
			Assert.That(NormalizedEnemyAnimationRegistry.GetForEnemy(ContentId.Parse("core:enemy/zombie-1"))
				.Select(item => item.Id.ToString()), Is.EqualTo(new[]
				{
					"core:enemy-animation/zombie-1/idle", "core:enemy-animation/zombie-1/move"
				}));
			Assert.That(NormalizedEnemyAnimationRegistry.GetForEnemy(ContentId.Parse("core:enemy/fly")), Has.Count.EqualTo(1));
			Assert.That(NormalizedEnemyAnimationRegistry.GetForEnemy(ContentId.Parse("core:enemy/musca")), Is.Empty);
		}

		[Test]
		public void Parse_ConvertsTransformSpriteColorAndSortingTracks()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'example.enemy:enemy-animation/move', 'enemy': 'example.enemy:enemy/stalker',
  'displayName': 'Move', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [
    { 'target': 'bone/hips', 'property': 'position.y', 'keys': [{ 'time': 0, 'value': 0, 'inTangent': null, 'outTangent': null }, { 'time': 1, 'value': 1, 'inTangent': null, 'outTangent': null }] },
    { 'target': 'sprite/hips', 'property': 'color.a', 'keys': [{ 'time': 0.5, 'value': 0.5, 'inTangent': null, 'outTangent': null }] },
    { 'target': 'sprite/hips', 'property': 'sortingOrder', 'keys': [{ 'time': 0.5, 'value': 12, 'inTangent': null, 'outTangent': null }] }
  ],
  'objectTracks': [{ 'target': 'sprite/hips', 'property': 'sprite', 'keys': [{ 'time': 0, 'asset': null, 'name': 'hips-alt' }] }]
}";
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(json, "example.enemy", "move.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			EnemyAnimationClipDefinition clip = result.Definition.CreateClip(new HashSet<string> { "hips" }, new HashSet<string> { "hips-alt" }, result.Report);
			Assert.That(clip.Frames.Select(frame => frame.Time), Is.EqualTo(new[] { 0f, 0.5f, 1f }));
			Assert.That(clip.Frames[1].Bones["hips"].Y, Is.EqualTo(0.5f).Within(0.001f));
			Assert.That(clip.Frames[1].Bones["hips"].ColorA, Is.EqualTo(0.5f));
			Assert.That(clip.Frames[1].Bones["hips"].RuntimeSortingOrder, Is.EqualTo(12));
			Assert.That(clip.Frames[1].Bones["hips"].Region, Is.EqualTo("hips-alt"));
		}

		[Test]
		public void Parse_AcceptsBoundedSemanticCueEvents()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'example.enemy:enemy-animation/attack', 'enemy': 'example.enemy:enemy/stalker',
  'displayName': 'Attack', 'durationSeconds': 1, 'frameRate': 30, 'loop': false,
  'tracks': [{ 'target': 'bone/root', 'property': 'position.x', 'keys': [{ 'time': 0, 'value': 0 }] }],
  'events': [{ 'time': 0.5, 'type': 'cue', 'cue': 'attack.projectile', 'index': 2, 'amount': 3 }]
}";
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(json, "example.enemy", "attack.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			EnemyAnimationClipDefinition clip = result.Definition.CreateClip(new HashSet<string> { "root" }, new HashSet<string>(), result.Report);
			Assert.That(clip.Events.Single().Cue, Is.EqualTo("attack.projectile"));
			Assert.That(clip.Events.Single().Index, Is.EqualTo(2));
		}

		[Test]
		public void Parse_AllowsInertCuesButRejectsGameplayEventsOnLoopingClips()
		{
			const string cue = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'example.enemy:enemy-animation/loop', 'enemy': 'example.enemy:enemy/stalker',
  'displayName': 'Loop', 'durationSeconds': 1, 'frameRate': 30, 'loop': true,
  'tracks': [{ 'target': 'bone/root', 'property': 'position.x', 'keys': [{ 'time': 0, 'value': 0 }] }],
  'events': [{ 'time': 0.5, 'type': 'cue', 'cue': 'finisher.thrust' }]
}";
			NormalizedEnemyAnimationLoadResult accepted = NormalizedEnemyAnimationParser.Parse(cue, "example.enemy", "loop.json");
			Assert.That(accepted.Report.IsValid, Is.True, string.Join("\n", accepted.Report.Issues.Select(issue => issue.ToString())));

			NormalizedEnemyAnimationLoadResult rejected = NormalizedEnemyAnimationParser.Parse(
				cue.Replace("'type': 'cue', 'cue': 'finisher.thrust'", "'type': 'attackHit'"), "example.enemy", "unsafe-loop.json");
			Assert.That(rejected.Report.Issues.Any(issue => issue.Code == "enemy-animation.event-loop"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeCueNamesAndIndexes()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'enemyAnimation',
  'id': 'example.enemy:enemy-animation/attack', 'enemy': 'example.enemy:enemy/stalker',
  'displayName': 'Attack', 'durationSeconds': 1, 'frameRate': 30, 'loop': false,
  'tracks': [{ 'target': 'bone/root', 'property': 'position.x', 'keys': [{ 'time': 0, 'value': 0 }] }],
  'events': [{ 'time': 0.5, 'type': 'cue', 'cue': 'Call Arbitrary Method()', 'index': 999 }]
}";
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(json, "example.enemy", "attack.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy-animation.event-cue"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "enemy-animation.event-cue-index"), Is.True);
		}
	}

	public class ClothingDefinitionParserTests
	{
		[Test]
		public void Parse_AcceptsPersistentPlayerAttachmentWithSkinVariants()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'playerAttachment',
  'id': 'example.clothes:player-attachment/anatomy',
  'sprite': 'assets/anatomy.png', 'skinSprites': { 'tan': 'assets/anatomy-tan.png', 'brown': 'assets/anatomy-brown.png' },
  'bone': 'Hips', 'pivotX': 0.2, 'pivotY': 0.4, 'sortingOffset': 2, 'tintWithSkin': true
}";
			PlayerAttachmentLoadResult result = PlayerAttachmentParser.Parse(json, "example.clothes", "attachment.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.clothes:player-attachment/anatomy")));
			Assert.That(result.Definition.Document.SkinSprites["tan"], Is.EqualTo("assets/anatomy-tan.png"));
			Assert.That(result.Definition.Document.SkinSprites["brown"], Is.EqualTo("assets/anatomy-brown.png"));
			Assert.That(result.Definition.Document.TintWithSkin, Is.True);
		}

		[Test]
		public void Parse_AcceptsBoundedPregnancyGrowthAttachment()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'playerAttachment',
  'id': 'example.clothes:player-attachment/belly', 'sprite': 'assets/belly.png', 'bone': 'Spine',
  'pregnancyGrowth': {
    'transitionSeconds': 1.5, 'breakSpineClothing': true, 'clothingBreakDelaySeconds': 0.5,
    'stages': [
      { 'minimumFetuses': 0, 'offsetX': 0.095, 'offsetY': 0.131, 'scaleX': 0, 'scaleY': 0.75 },
      { 'minimumFetuses': 1, 'offsetX': 0.085, 'offsetY': 0.131, 'scaleX': 0.5, 'scaleY': 0.75 }
    ]
  }
}";
			PlayerAttachmentLoadResult result = PlayerAttachmentParser.Parse(json, "example.clothes", "belly.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(result.Definition.Document.PregnancyGrowth.Stages, Has.Count.EqualTo(2));
			Assert.That(result.Definition.Document.PregnancyGrowth.BreakSpineClothing, Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafePregnancyGrowthAttachment()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'playerAttachment',
  'id': 'example.clothes:player-attachment/belly', 'sprite': 'assets/belly.png', 'bone': 'Spine',
  'pregnancyGrowth': { 'transitionSeconds': 99, 'stages': [
    { 'minimumFetuses': 1, 'offsetX': 0, 'offsetY': 0, 'scaleX': 1, 'scaleY': 1 },
    { 'minimumFetuses': 1, 'offsetX': 0, 'offsetY': 0, 'scaleX': 20, 'scaleY': 1 }
  ] }
}";
			PlayerAttachmentLoadResult result = PlayerAttachmentParser.Parse(json, "example.clothes", "belly.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.pregnancy-timing"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.pregnancy-stage"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.pregnancy-stage-zero"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafePersistentPlayerAttachment()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'playerAttachment',
  'id': 'wrong.pack:player-attachment/anatomy', 'sprite': '../anatomy.png', 'bone': 'Tail'
}";
			PlayerAttachmentLoadResult result = PlayerAttachmentParser.Parse(json, "example.clothes", "attachment.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.id"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.sprite"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "player-attachment.bone"), Is.True);
		}

		private const string ValidClothing = @"{
  'schemaVersion': 1,
  'type': 'clothing',
  'id': 'example.clothes:clothing/refitted-shirt',
  'displayName': 'Refitted Shirt',
  'extends': 'core:clothing/shirt-default',
  'visual': {
    'type': 'coreClothingAtlas',
    'atlas': 'assets/clothing/refitted-shirt.png',
    'pixelsPerUnit': 32,
    'regions': {
      'piece/shirt-spine': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 },
      'piece/shirt-chest': { 'x': 32, 'y': 0, 'width': 32, 'height': 32 }
    }
  }
}";

		[Test]
		public void Parse_AcceptsCoreTemplateClothingAtlas()
		{
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(ValidClothing, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.clothes:clothing/refitted-shirt")));
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:clothing/shirt-default")));
			Assert.That(result.Definition.Visual.Regions, Has.Count.EqualTo(2));
			Assert.That(result.Definition.UnlockedByDefault, Is.False);
		}

		[Test]
		public void Parse_AcceptsPublishedCategoryCompatibility()
		{
			string json = ValidClothing.Replace("'visual': {", "'category': 'Upper', 'incompatibleCategories': ['Upper', 'Other'], 'visual': {");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Category, Is.EqualTo("Upper"));
			Assert.That(result.Definition.IncompatibleCategories, Is.EquivalentTo(new[] { "Upper", "Other" }));
		}

		[Test]
		public void Parse_AcceptsIndividualCoreClothingSprites()
		{
			string json = ValidClothing.Replace("'type': 'coreClothingAtlas',\n    'atlas': 'assets/clothing/refitted-shirt.png',\n    'pixelsPerUnit': 32,\n    'regions': {\n      'piece/shirt-spine': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 },\n      'piece/shirt-chest': { 'x': 32, 'y': 0, 'width': 32, 'height': 32 }\n    }", "'type': 'coreClothingSprites', 'pixelsPerUnit': 32, 'sprites': { 'piece/shirt-chest': 'assets/chest.png' }");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Visual.Sprites["piece/shirt-chest"], Is.EqualTo("assets/chest.png"));
		}

		[Test]
		public void Parse_AcceptsClothingRegionsLargerThanCoreSourceCells()
		{
			string json = ValidClothing
				.Replace("'x': 32, 'y': 0, 'width': 32, 'height': 32", "'x': 64, 'y': 0, 'width': 64, 'height': 96")
				.Replace("'piece/shirt-spine': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 }", "'piece/shirt-spine': { 'x': 0, 'y': 0, 'width': 64, 'height': 96 }");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "extended-shirt.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Visual.Regions["piece/shirt-chest"].Height, Is.EqualTo(96));
		}

		[Test]
		public void Parse_AcceptsAttachmentTearingAndArmorOverrides()
		{
			string json = ValidClothing
				.Replace("'visual': {", "'effects': { 'damageTakenMultiplier': 0.8, 'statModifiers': { 'SpeedMax': -0.5, 'HealthMax': 10 } }, 'visual': {")
				.Replace("'regions': {", "'attachments': { 'piece/shirt-chest': { 'bone': 'Chest', 'offsetX': 0.1, 'offsetY': -0.2, 'rotation': 3, 'pivotX': 0.45, 'pivotY': 0.55, 'sortingOffset': 2, 'hideBodyPart': true, 'droppable': true, 'dropOnOralThrust': true } }, 'regions': {");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Effects.DamageTakenMultiplier, Is.EqualTo(0.8f));
			Assert.That(result.Definition.Visual.Attachments["piece/shirt-chest"].Bone, Is.EqualTo("Chest"));
		}

		[Test]
		public void Parse_AcceptsOriginalRigBackPhysicsAndBodyVariant()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'clothing',
  'id': 'example.clothes:clothing/swaying-backpack',
  'displayName': 'Swaying Backpack', 'category': 'Other', 'unlockedByDefault': true,
  'visual': {
    'type': 'originalClothingSprites', 'pixelsPerUnit': 32,
    'sprites': { 'icon': 'assets/icon.png', 'piece/back': 'assets/back.png', 'piece/custom-charm': 'assets/charm.png' },
    'attachments': {
      'piece/back': { 'bone': 'Spine', 'attachToBone': true, 'sortingOffset': -3,
        'physics': { 'mode': 'sway', 'spring': 40, 'damping': 8, 'gravity': 0.5, 'motionInfluence': 1.5, 'maxAngle': 35 } },
      'piece/custom-charm': { 'bone': 'Hips', 'attachToBone': true }
    },
    'bodyVariants': { 'example.body-pack': { 'piece/back': 'assets/back-body-variant.png' } }
  }
}";
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "backpack.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.IsOriginal, Is.True);
			Assert.That(result.Definition.Visual.Attachments["piece/back"].Physics.Mode, Is.EqualTo("sway"));
			Assert.That(result.Definition.Visual.BodyVariants.ContainsKey("example.body-pack"), Is.True);
		}

		[Test]
		public void Parse_RejectsOriginalRigWithoutAttachmentForEveryPiece()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'clothing',
  'id': 'example.clothes:clothing/broken', 'displayName': 'Broken', 'category': 'Other',
  'visual': { 'type': 'originalClothingSprites',
    'sprites': { 'icon': 'assets/icon.png', 'piece/unattached': 'assets/piece.png' }, 'attachments': {} }
}";
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "broken.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "clothing.visual.original-attachment"), Is.True);
		}

		[Test]
		public void Parse_RejectsForeignNamespaceUnsafeAtlasAndUnknownFields()
		{
			string json = ValidClothing
				.Replace("example.clothes:clothing/refitted-shirt", "another.pack:clothing/refitted-shirt")
				.Replace("assets/clothing/refitted-shirt.png", "../refitted-shirt.png")
				.Replace("'displayName'", "'script': 'Legacy.dll', 'displayName'");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "clothing.json"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownClothingSlotsBeforeRuntimeConstruction()
		{
			string json = ValidClothing.Replace("piece/shirt-chest", "piece/shirt-chets");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(json, "example.clothes", "shirt.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "clothing.visual.region-name"), Is.True);
		}

		[TestCase("clp_shirtChest", "piece/shirt-chest")]
		[TestCase("clp_lLegLowerArmor", "piece/l-leg-lower-armor")]
		[TestCase("clp_hair", "piece/hair")]
		public void SlotCatalog_UsesStableNamesForCorePieces(string i_pieceName, string i_expectedSlot)
		{
			Assert.That(ClothingSlotCatalog.FromCorePieceName(i_pieceName), Is.EqualTo(i_expectedSlot));
			Assert.That(ClothingSlotCatalog.IsPublished(i_expectedSlot), Is.True);
		}
	}

	public class ClothingContentStateTests
	{
		[Test]
		public void State_RoundTripsUnlockAndEquipmentFlags()
		{
			ClothingContentState state = new ClothingContentState { Unlocked = true, Equipped = true };
			Assert.That(ClothingContentState.TryParse(state.ToJson(), out ClothingContentState restored), Is.True);
			Assert.That(restored.Unlocked, Is.True);
			Assert.That(restored.Equipped, Is.True);
		}

		[Test]
		public void State_RejectsUnknownFields()
		{
			Assert.That(ClothingContentState.TryParse("{\"legacyId\":12}", out _), Is.False);
		}
	}

	public class StageContentStateTests
	{
		[Test]
		public void State_RoundTripsNamespacedHighscore()
		{
			StageContentState state = new StageContentState { Highscore = 17 };
			Assert.That(StageContentState.TryParse(state.ToJson(), out StageContentState restored), Is.True);
			Assert.That(restored.Highscore, Is.EqualTo(17));
		}

		[Test]
		public void State_RejectsNegativeHighscoresAndUnknownFields()
		{
			Assert.That(StageContentState.TryParse("{\"highscore\":-1}", out _), Is.False);
			Assert.That(StageContentState.TryParse("{\"legacyStageId\":6}", out _), Is.False);
		}
	}

	public class WeaponDefinitionParserTests
	{
		private const string ValidWeapon = @"{
  'schemaVersion': 1,
  'type': 'weapon',
  'id': 'example.weapons:item/weapon/toy-pistol',
  'displayName': 'Toy Pistol',
  'extends': 'core:item/weapon/pistol',
  'stats': {
    'damage': 3,
    'ammoMax': 120,
    'magazineSize': 12,
    'bulletsPerShot': 1,
    'penetration': 0,
    'rangeMultiplier': 1,
    'fireIntervalSeconds': 0.12,
    'recoil': 0.1,
    'movementRecoil': 0,
    'knockbackX': 0,
    'knockbackY': 0
  },
  'visual': {
    'type': 'coreWeaponSprites',
    'pixelsPerUnit': 32,
    'sprites': {
      'body': 'assets/body.png',
      'slide': 'assets/slide.png',
      'base': 'assets/base.png'
    }
  }
}";

		[Test]
		public void Parse_AcceptsAdditiveCorePistolDefinition()
		{
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(ValidWeapon, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.weapons:item/weapon/toy-pistol")));
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:item/weapon/pistol")));
			Assert.That(result.Definition.Stats.Damage, Is.EqualTo(3));
			Assert.That(result.Definition.Stats.MagazineSize, Is.EqualTo(12));
			Assert.That(result.Definition.Visual.Sprites.Keys, Is.EquivalentTo(new[] { "body", "slide", "base" }));
		}

		[Test]
		public void Parse_AcceptsFullyOriginalEventFreeWeapon()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'weapon',
  'id': 'example.weapons:item/weapon/original-sidearm',
  'displayName': 'Original Sidearm',
  'stats': {
    'damage': 5, 'ammoMax': 60, 'magazineSize': 10, 'rangeMultiplier': 1,
    'fireIntervalSeconds': 0.2, 'reloadSeconds': 1, 'equipSeconds': 0.4,
    'weight': 1, 'value': 25, 'semiAutomatic': true, 'marketable': true,
    'holdType': 'OneHanded', 'reloadType': 'Magazine', 'weaponType': 'Pistol'
  },
  'behavior': {
    'ammunition': { 'type': 'fmj', 'damageMultiplier': 0.9, 'penetrationBonus': 2, 'weakpointMultiplier': 0.85 },
    'alternateFire': { 'mode': 'hitscan', 'burstCount': 3, 'burstIntervalSeconds': 0.08, 'recoilMultiplier': 0.5 },
    'animations': { 'clips': { 'fire': { 'frames': [
      { 'durationSeconds': 0.05, 'sprites': { 'slide': 'assets/fire.png' } }
    ] } } }
  },
  'visual': {
    'type': 'originalWeaponSprites', 'pixelsPerUnit': 32,
    'pivotX': 0.2, 'pivotY': 0.5, 'muzzleOffsetX': 0.8, 'muzzleOffsetY': 0.1,
    'colliderWidth': 0.8, 'colliderHeight': 0.3,
    'sprites': { 'body': 'assets/body.png', 'slide': 'assets/slide.png', 'icon': 'assets/icon.png' },
    'parts': { 'slide': { 'offsetX': 0.1, 'sortingOrder': 2 } }
  }
}";
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "original.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.IsOriginal, Is.True);
			Assert.That(result.Definition.Extends.HasValue, Is.False);
			Assert.That(result.Definition.Stats.WeaponType, Is.EqualTo("Pistol"));
			Assert.That(result.Definition.Visual.MuzzleOffsetX, Is.EqualTo(0.8f));
			Assert.That(result.Definition.Visual.Parts["slide"].SortingOrder, Is.EqualTo(2));
			Assert.That(result.Definition.Behavior.Ammunition.Type, Is.EqualTo("fmj"));
			Assert.That(result.Definition.Behavior.AlternateFire.BurstCount, Is.EqualTo(3));
		}

		[Test]
		public void Parse_RejectsIncompleteOrInheritedOriginalWeapon()
		{
			const string incomplete = @"{
  'schemaVersion': 1, 'type': 'weapon',
  'id': 'example.weapons:item/weapon/broken', 'displayName': 'Broken',
  'extends': 'core:item/weapon/pistol',
  'stats': { 'damage': 5, 'reloadType': 'PumpAction' },
  'visual': { 'type': 'originalWeaponSprites', 'sprites': { 'body': 'assets/body.png' } }
}";
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(incomplete, "example.weapons", "broken.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.extends"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.stats.original-required"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.stats.original-reload-type"), Is.True);
		}

		[Test]
		public void Parse_AcceptsExpandedWeaponBehaviorAndEconomy()
		{
			string json = ValidWeapon.Replace("'knockbackY': 0", "'knockbackY': 0, 'reloadSeconds': 0.9, 'equipSeconds': 0.4, 'weight': 1, 'value': 20, 'semiAutomatic': true, 'marketable': true, 'holdType': 'OneHanded', 'reloadType': 'Magazine'");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Stats.ReloadSeconds, Is.EqualTo(0.9f));
			Assert.That(result.Definition.Stats.HoldType, Is.EqualTo("OneHanded"));
		}

		[Test]
		public void Parse_AcceptsReviewedPresentationAndBurstBehavior()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'burstCount': 3, 'burstIntervalSeconds': 0.08,
    'tracerColor': '#44CCFFFF', 'tracerThickness': 2, 'tracerFrames': 4,
    'soundVolume': 0.5,
    'shootSounds': ['assets/fire.wav'], 'reloadSounds': ['assets/reload.ogg'],
    'muzzleFlashSprites': ['assets/flash.png'], 'effectPixelsPerUnit': 32,
    'casingSprite': 'assets/casing.png', 'casingOn': 'shoot',
    'casingForceX': 1.5, 'casingForceY': 2.5, 'casingLifetimeSeconds': 2
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.BurstCount, Is.EqualTo(3));
			Assert.That(result.Definition.Behavior.TracerColor, Is.EqualTo("#44CCFFFF"));
			Assert.That(result.Definition.Behavior.MuzzleFlashSprites.Single(), Is.EqualTo("assets/flash.png"));
		}

		[Test]
		public void Parse_AcceptsPhysicalExplosiveProjectile()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'projectile': {
      'mode': 'physical', 'sprite': 'assets/dart.png', 'pixelsPerUnit': 32,
      'speed': 18, 'gravity': 1.5, 'lifetimeSeconds': 4,
      'impactSprites': ['assets/impact.png'], 'impactLifetimeSeconds': 0.2,
      'explosionRadius': 2.5, 'explosionDamageMultiplier': 0.75
    }
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.Projectile.Mode, Is.EqualTo("physical"));
			Assert.That(result.Definition.Behavior.Projectile.ExplosionRadius, Is.EqualTo(2.5f));
		}

		[Test]
		public void Parse_AcceptsBeamMeleeAlternateAndSpriteAnimations()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'melee': { 'input': 'alternate', 'range': 1.5, 'radius': 0.6, 'damageMultiplier': 2, 'maxTargets': 2 },
    'beam': { 'input': 'primary', 'durationSeconds': 0.5, 'tickIntervalSeconds': 0.1, 'damageMultiplier': 0.25, 'ammoPerTick': 1, 'color': '#44CCFFFF', 'thickness': 2 },
    'alternateFire': { 'mode': 'melee', 'damageMultiplier': 1.5, 'ammoCost': 0, 'cooldownSeconds': 0.4 },
    'animations': { 'fireSpeedMultiplier': 1.5, 'clips': { 'fire': { 'frames': [
      { 'durationSeconds': 0.05, 'sprites': { 'slide': 'assets/slide-fire-1.png' } },
      { 'durationSeconds': 0.05, 'sprites': { 'slide': 'assets/slide-fire-2.png' } }
    ] } } }
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.Melee.Input, Is.EqualTo("alternate"));
			Assert.That(result.Definition.Behavior.Animations.Clips["fire"].Frames, Has.Count.EqualTo(2));
		}

		[Test]
		public void Parse_AcceptsChargedPrimaryFire()
		{
			string json = ValidWeapon.Replace("'visual': {", "'behavior': { 'charge': { 'seconds': 1.2, 'minimumDamageMultiplier': 0.5, 'maximumDamageMultiplier': 3, 'chargingText': 'CHARGING', 'readyText': 'READY', 'chargingColor': '#FFAA33FF', 'readyColor': '#55FF77FF', 'tintStrength': 0.65 } }, 'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.Charge.MaximumDamageMultiplier, Is.EqualTo(3f));
			Assert.That(result.Definition.Behavior.Charge.ReadyText, Is.EqualTo("READY"));
		}

		[Test]
		public void Parse_AcceptsThrowableWeaponBehavior()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'shootSounds': ['assets/throw.wav'],
    'throwable': {
      'throwSpeed': 12, 'gravity': 1, 'angularVelocity': 180, 'lifetimeSeconds': 15,
      'impactDamageMultiplier': 1, 'destroyOnImpact': false, 'fuseSeconds': 3,
      'explosionRadius': 3, 'explosionDamageMultiplier': 2, 'explosionForce': 12,
      'tickIntervalSeconds': 0.5, 'impactSounds': ['assets/hit.wav'],
      'tickSounds': ['assets/tick.wav'], 'explosionSounds': ['assets/explosion.ogg'],
	      'explosionColor': '#FF7722FF', 'explodeOnImpact': true, 'explosionStyle': 'fire',
	      'igniteDurationSeconds': 5, 'igniteDamagePerTick': 2, 'igniteTicksPerSecond': 2
    }
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "throwable.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Behavior.Throwable.ThrowSpeed, Is.EqualTo(12f));
			Assert.That(result.Definition.Behavior.Throwable.ExplosionRadius, Is.EqualTo(3f));
			Assert.That(result.Definition.Behavior.Throwable.TickSounds.Single(), Is.EqualTo("assets/tick.wav"));
			Assert.That(result.Definition.Behavior.Throwable.ExplodeOnImpact, Is.True);
			Assert.That(result.Definition.Behavior.Throwable.ExplosionStyle, Is.EqualTo("fire"));
			Assert.That(result.Definition.Behavior.Throwable.IgniteDurationSeconds, Is.EqualTo(5f));
		}

		[Test]
		public void Parse_RejectsUnsafeOrConflictingThrowableBehavior()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'projectile': { 'mode': 'hitscan' },
	    'throwable': { 'throwSpeed': 1000, 'explosionRadius': 3, 'explosionDamageMultiplier': 0,
	      'explosionStyle': 'smoke', 'igniteDurationSeconds': 5,
	      'impactSounds': ['../hit.mp3'] }
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "throwable.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.throwable-speed"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.throwable-impact-sounds"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.throwable.explosion-damage"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.throwable.explosion-style"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.throwable.ignite"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.throwable.primary-conflict"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeOrUnboundedProjectile()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'projectile': { 'mode': 'script', 'sprite': '../dart.exe', 'speed': 9999 }
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.projectile.mode"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.projectile-sprite"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.projectile-speed"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeWeaponBehavior()
		{
			string json = ValidWeapon.Replace("'visual': {", @"'behavior': {
    'burstCount': 20, 'tracerColor': 'blue',
    'shootSounds': ['../fire.mp3'], 'casingOn': 'everywhere'
  },
  'visual': {");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.burst-count"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.tracer-color"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.behavior.shoot-sounds"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnusedPistolTwoHandedPose()
		{
			string json = ValidWeapon.Replace("'knockbackY': 0", "'knockbackY': 0, 'holdType': 'PistolTwoHanded'");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.stats.hold-type"), Is.True);
		}

		[Test]
		public void Parse_AcceptsPublishedShotgunAndRevolverTemplates()
		{
			string shotgunJson = ValidWeapon
				.Replace("core:item/weapon/pistol", "core:item/weapon/tenelli-so3")
				.Replace("'slide': 'assets/slide.png'", "'shell': 'assets/shell.png'");
			WeaponDefinitionLoadResult shotgun = WeaponDefinitionParser.Parse(shotgunJson, "example.weapons", "shotgun.json");
			Assert.That(shotgun.Report.IsValid, Is.True);

			string revolverJson = ValidWeapon
				.Replace("core:item/weapon/pistol", "core:item/weapon/revolver-44")
				.Replace("'slide': 'assets/slide.png'", "'hammer': 'assets/hammer.png'");
			WeaponDefinitionLoadResult revolver = WeaponDefinitionParser.Parse(revolverJson, "example.weapons", "revolver.json");
			Assert.That(revolver.Report.IsValid, Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownSlotsUnsafePathsAndArbitraryFields()
		{
			string invalidSlot = ValidWeapon.Replace("'body': 'assets/body.png'", "'barrel': '../body.png'");
			WeaponDefinitionLoadResult slotResult = WeaponDefinitionParser.Parse(invalidSlot, "example.weapons", "weapon.json");
			Assert.That(slotResult.Report.IsValid, Is.False);
			Assert.That(slotResult.Report.Issues.Any(issue => issue.Code == "weapon.visual.slot"), Is.True);
			Assert.That(slotResult.Report.Issues.Any(issue => issue.Code == "weapon.visual.asset-path"), Is.True);

			string arbitraryField = ValidWeapon.Replace("'displayName'", "'script': 'WeaponHack.dll', 'displayName'");
			WeaponDefinitionLoadResult fieldResult = WeaponDefinitionParser.Parse(arbitraryField, "example.weapons", "weapon.json");
			Assert.That(fieldResult.Report.Issues.Any(issue => issue.Code == "weapon.json"), Is.True);
		}

		[Test]
		public void Parse_RejectsCoreTemplatesWhoseSlotsAreNotPublished()
		{
			string json = ValidWeapon.Replace("core:item/weapon/pistol", "core:item/weapon/m4b1");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(json, "example.weapons", "weapon.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "weapon.extends"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeStatRangesAndUnpairedAmmunitionOverrides()
		{
			string ranges = ValidWeapon
				.Replace("'damage': 3", "'damage': 10001")
				.Replace("'bulletsPerShot': 1", "'bulletsPerShot': 65")
				.Replace("'recoil': 0.1", "'recoil': 1.1");
			WeaponDefinitionLoadResult rangeResult = WeaponDefinitionParser.Parse(ranges, "example.weapons", "weapon.json");
			Assert.That(rangeResult.Report.Issues.Any(issue => issue.Code == "weapon.stats.damage"), Is.True);
			Assert.That(rangeResult.Report.Issues.Any(issue => issue.Code == "weapon.stats.bullets-per-shot"), Is.True);
			Assert.That(rangeResult.Report.Issues.Any(issue => issue.Code == "weapon.stats.recoil"), Is.True);

			string unpairedAmmo = ValidWeapon.Replace("'magazineSize': 12,", string.Empty);
			WeaponDefinitionLoadResult ammoResult = WeaponDefinitionParser.Parse(unpairedAmmo, "example.weapons", "weapon.json");
			Assert.That(ammoResult.Report.Issues.Any(issue => issue.Code == "weapon.stats.ammo-pair"), Is.True);

			string oversizedMagazine = ValidWeapon.Replace("'magazineSize': 12", "'magazineSize': 121");
			WeaponDefinitionLoadResult magazineResult = WeaponDefinitionParser.Parse(oversizedMagazine, "example.weapons", "weapon.json");
			Assert.That(magazineResult.Report.Issues.Any(issue => issue.Code == "weapon.stats.magazine-size"), Is.True);
		}
	}

	public class CoreWeaponDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaGunCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreWeapons, Has.Count.EqualTo(22));
			CoreWeaponDefinition pistol = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/pistol"));
			Assert.That(pistol.Id, Is.EqualTo(ContentId.Parse("core:item/weapon/pistol")));
			Assert.That(pistol.LegacyName, Is.EqualTo("Pistol"));
			Assert.That(pistol.Stats.Damage, Is.EqualTo(2));
			Assert.That(pistol.Stats.AmmoMax, Is.EqualTo(396));
			Assert.That(pistol.Stats.MagazineSize, Is.EqualTo(12));
			Assert.That(pistol.Stats.InfiniteAmmo, Is.True);
			Assert.That(pistol.Stats.SemiAutomatic, Is.True);

			CoreWeaponDefinition qlock17S = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/qlock-17-s"));
			Assert.That(qlock17S.Stats.Damage, Is.EqualTo(3));
			Assert.That(qlock17S.Stats.MagazineSize, Is.EqualTo(15));
			Assert.That(qlock17S.Stats.FireIntervalSeconds, Is.EqualTo(0.05f));

			CoreWeaponDefinition muger = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/muger-p08"));
			Assert.That(muger.Stats.Damage, Is.EqualTo(5));
			Assert.That(muger.Stats.MagazineSize, Is.EqualTo(8));

			CoreWeaponDefinition qlock17A = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/qlock-17-a"));
			Assert.That(qlock17A.Stats.AmmoMax, Is.EqualTo(300));
			Assert.That(qlock17A.Stats.SemiAutomatic, Is.False);

			CoreWeaponDefinition snub = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/snub-revolver-32"));
			Assert.That(snub.Stats.Damage, Is.EqualTo(8));
			Assert.That(snub.Stats.FireIntervalSeconds, Is.EqualTo(0.02f));

			CoreWeaponDefinition revolver = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/revolver-44"));
			Assert.That(revolver.Stats.Damage, Is.EqualTo(13));
			Assert.That(revolver.Stats.MagazineSize, Is.EqualTo(6));

			CoreWeaponDefinition usi = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/usi"));
			Assert.That(usi.Stats.AmmoMax, Is.EqualTo(384));
			Assert.That(usi.Stats.SemiAutomatic, Is.False);

			CoreWeaponDefinition np40 = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/np-40"));
			Assert.That(np40.Stats.HoldType, Is.EqualTo("TwoHanded1"));
			Assert.That(np40.Stats.KnockbackX, Is.EqualTo(1f));

			CoreWeaponDefinition np9 = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/np-9"));
			Assert.That(np9.Stats.Recoil, Is.EqualTo(0.12f));

			CoreWeaponDefinition ssph42 = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/ssph-42"));
			Assert.That(ssph42.Stats.MagazineSize, Is.EqualTo(66));
			Assert.That(ssph42.Stats.HoldType, Is.EqualTo("TwoHanded2"));

			CoreWeaponDefinition defender = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/sl-defender"));
			Assert.That(defender.Stats.FireIntervalSeconds, Is.EqualTo(0.075f));

			CoreWeaponDefinition harrington = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/harrington-model-1892"));
			Assert.That(harrington.Stats.BulletsPerShot, Is.EqualTo(4));
			Assert.That(harrington.Stats.ReloadType, Is.EqualTo("SingleBarrel"));

			CoreWeaponDefinition ronigsberg = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/ronigsberg-543"));
			Assert.That(ronigsberg.Stats.BulletsPerShot, Is.EqualTo(6));
			Assert.That(ronigsberg.Stats.RangeMultiplier, Is.EqualTo(0.65f));

			CoreWeaponDefinition sawedOff = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/sawed-off"));
			Assert.That(sawedOff.Stats.BulletsPerShot, Is.EqualTo(9));
			Assert.That(sawedOff.Stats.ReloadType, Is.EqualTo("SingleBarrel"));

			CoreWeaponDefinition mg32 = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/mg32"));
			Assert.That(mg32.Stats.MagazineSize, Is.EqualTo(250));

			CoreWeaponDefinition schockgewehr = result.CoreWeapons.Single(weapon => weapon.Id == ContentId.Parse("core:item/weapon/schockgewehr"));
			Assert.That(schockgewehr.Stats.Damage, Is.EqualTo(0));
			Assert.That(schockgewehr.Stats.FireIntervalSeconds, Is.EqualTo(0.02f));
		}

		[Test]
		public void Parse_RejectsExternalIdsAndUnknownFields()
		{
			const string invalid = @"{
  'schemaVersion': 1, 'type': 'coreWeapon',
  'id': 'external:item/weapon/pistol', 'legacyName': 'Pistol', 'displayName': 'Pistol',
  'script': 'WeaponHack.dll', 'stats': { 'damage': 2 }
}";
			CoreWeaponLoadResult result = CoreWeaponParser.Parse(invalid, "invalid.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-weapon.json"), Is.True);
		}
	}

	public class CoreEnemyDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaEnemyCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreEnemies, Has.Count.EqualTo(21));

			CoreEnemyDefinition zombie = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/zombie-1"));
			Assert.That(zombie.LegacyId, Is.EqualTo(1));
			Assert.That(zombie.DisplayName, Is.EqualTo("Zombie I"));
			Assert.That(zombie.Stats.HealthMax, Is.EqualTo(25f));
			Assert.That(zombie.Stats.SpeedMax, Is.EqualTo(2.5f));
			Assert.That(zombie.Stats.Bounty, Is.EqualTo(35));

			CoreEnemyDefinition grabber = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/zombie-grabber"));
			Assert.That(grabber.Stats.SpeedMax, Is.EqualTo(3.5f));

			CoreEnemyDefinition zombie3 = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/zombie-3"));
			Assert.That(zombie3.Stats.HealthMax, Is.EqualTo(100f));
			Assert.That(zombie3.Stats.Bounty, Is.EqualTo(500));
			Assert.That(zombie3.Behavior.VisionRange, Is.EqualTo(20f));

			CoreEnemyDefinition fly = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/fly"));
			Assert.That(fly.Stats.HealthMax, Is.EqualTo(1f));
			Assert.That(fly.Stats.SpeedMax, Is.EqualTo(30f));

			CoreEnemyDefinition musca = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/musca"));
			Assert.That(musca.Stats.HealthMax, Is.EqualTo(65f));

			CoreEnemyDefinition gremlin = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/gremlin"));
			Assert.That(gremlin.DisplayName, Is.EqualTo("Gremlin"));
			Assert.That(gremlin.Stats.Traction, Is.EqualTo(1f));

			CoreEnemyDefinition abby = result.CoreEnemies.Single(enemy => enemy.Id == ContentId.Parse("core:enemy/abby"));
			Assert.That(abby.Stats.SpeedAcceleration, Is.EqualTo(18.5f));
		}

		[Test]
		public void Parse_RejectsExternalIdsAndArbitraryBehaviorModules()
		{
			const string invalid = @"{
  'schemaVersion': 1, 'type': 'coreEnemy', 'id': 'external:enemy/zombie-1',
  'legacyId': 1, 'displayName': 'Zombie I', 'stats': { 'healthMax': 25 },
  'behavior': { 'modules': [{ 'type': 'custom-code' }] }
}";
			CoreEnemyLoadResult result = CoreEnemyParser.Parse(invalid, "invalid.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-enemy.id"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-enemy.behavior-modules"), Is.True);
		}

		[Test]
		public void Parse_AcceptsNormalizedCoreAnimationReferencesAndRejectsForeignOnes()
		{
			const string valid = @"{
  'schemaVersion': 1, 'type': 'coreEnemy', 'id': 'core:enemy/zombie-1',
  'legacyId': 1, 'displayName': 'Zombie I', 'stats': { 'healthMax': 25 },
  'animationReferences': {
    'idle': 'core:enemy-animation/zombie-1/idle',
    'move': 'core:enemy-animation/zombie-1/move'
  }
}";
			CoreEnemyLoadResult result = CoreEnemyParser.Parse(valid, "zombie-1.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.AnimationReferences, Has.Count.EqualTo(2));

			CoreEnemyLoadResult invalid = CoreEnemyParser.Parse(
				valid.Replace("core:enemy-animation/zombie-1/idle", "foreign:enemy-animation/zombie-1/idle"), "invalid.json");
			Assert.That(invalid.Report.Issues.Any(issue => issue.Code == "core-enemy.animation-reference"), Is.True);
		}
	}

	public class CoreItemDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaUsableAndConsumableCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreItems, Has.Count.EqualTo(11));

			CoreItemDefinition aspirin = result.CoreItems.Single(item => item.Id == ContentId.Parse("core:item/usable/aspirin"));
			Assert.That(aspirin.LegacyName, Is.EqualTo("Aspirin"));
			Assert.That(aspirin.Stats.Value, Is.EqualTo(250));
			Assert.That(aspirin.Stats.EquipSeconds, Is.EqualTo(0.5f));
			Assert.That(aspirin.Stats.GoodEffectDescriptions.Single(), Is.EqualTo("+35% lost Health"));
			Assert.That(aspirin.EffectMode, Is.EqualTo("replace"));
			Assert.That(aspirin.Effects.Single().Type, Is.EqualTo("restoreHealth"));
			Assert.That(aspirin.Effects.Single().Amount, Is.EqualTo(35f));

			CoreItemDefinition hyper = result.CoreItems.Single(item => item.Id == ContentId.Parse("core:item/usable/hyper"));
			Assert.That(hyper.Stats.Marketable, Is.False);
			Assert.That(hyper.EffectMode, Is.EqualTo("replace"));
			Assert.That(hyper.Effects, Has.Count.EqualTo(3));
			Assert.That(hyper.Effects.Select(effect => effect.Stat), Is.EquivalentTo(new[] { "SpeedAccel", "SpeedSprint", "PowerDash" }));

			CoreItemDefinition ammoBox = result.CoreItems.Single(item => item.Id == ContentId.Parse("core:item/consumable/ammo-box"));
			Assert.That(ammoBox.Stats.Weight, Is.EqualTo(0));
			Assert.That(ammoBox.Stats.Value, Is.EqualTo(0));
			Assert.That(ammoBox.Stats.EquipSeconds, Is.Null);
			Assert.That(ammoBox.EffectMode, Is.EqualTo("inherit"));
			Assert.That(ammoBox.Effects, Is.Empty);
		}

		[Test]
		public void Parse_RejectsExternalIdsAndWeaponFields()
		{
			const string invalid = @"{
  'schemaVersion': 1, 'type': 'coreItem', 'id': 'external:item/usable/aspirin',
  'legacyName': 'Aspirin', 'displayName': 'Aspirin',
  'stats': { 'weight': 0, 'value': 250, 'damage': 100 }
}";
			CoreItemLoadResult result = CoreItemParser.Parse(invalid, "invalid.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-item.json"), Is.True);
		}
	}

	public class CoreStageDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaStageCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreStages, Has.Count.EqualTo(7));

			CoreStageDefinition hub = result.CoreStages.Single(stage => stage.Id == ContentId.Parse("core:stage/hub"));
			Assert.That(hub.LegacyId, Is.EqualTo(0));
			Assert.That(hub.FirstWaveEnemyCount, Is.Null);

			CoreStageDefinition shack = result.CoreStages.Single(stage => stage.Id == ContentId.Parse("core:stage/shack"));
			Assert.That(shack.DisplayName, Is.EqualTo("Shack"));
			Assert.That(shack.FirstWaveEnemyCount, Is.EqualTo(12));

			CoreStageDefinition spaceStation = result.CoreStages.Single(stage => stage.Id == ContentId.Parse("core:stage/space-station"));
			Assert.That(spaceStation.FirstWaveEnemyCount, Is.EqualTo(10));

			CoreStageDefinition fieldDay = result.CoreStages.Single(stage => stage.Id == ContentId.Parse("core:stage/field-day"));
			Assert.That(fieldDay.LegacyId, Is.EqualTo(6));
			Assert.That(fieldDay.FirstWaveEnemyCount, Is.EqualTo(2));
		}

		[Test]
		public void Parse_RejectsExternalIdsAndHubWaves()
		{
			const string invalid = @"{
  'schemaVersion': 1, 'type': 'coreStage', 'id': 'external:stage/hub',
  'legacyId': 0, 'displayName': 'Hub', 'firstWaveEnemyCount': 12,
  'scenePath': 'Assets/Scenes/Hack.unity'
}";
			CoreStageLoadResult result = CoreStageParser.Parse(invalid, "invalid.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-stage.json"), Is.True);
		}
	}

	public class CoreClothingDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaClothingCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreClothing, Has.Count.EqualTo(98));
			CoreClothingDefinition hair = result.CoreClothing.Single(item => item.Id == ContentId.Parse("core:clothing/hair-default"));
			Assert.That(hair.LegacyId, Is.EqualTo(0));
			Assert.That(hair.Category, Is.EqualTo("Hair"));
			Assert.That(hair.HasLoosePresentation, Is.True);
			CoreClothingDefinition hazmat = result.CoreClothing.Single(item => item.Id == ContentId.Parse("core:clothing/hazmat-suit"));
			Assert.That(hazmat.LegacyId, Is.EqualTo(78));
			Assert.That(hazmat.Category, Is.EqualTo("Upper"));
			CoreClothingPieceRecord hazmatHead = hazmat.Pieces.Single(piece => piece.Slot == "piece/head");
			Assert.That(hazmatHead.PieceType, Is.EqualTo("hat"));
			Assert.That(hazmatHead.HidesHair, Is.True);
			CoreClothingDefinition lingerie = result.CoreClothing.Single(item => item.Id == ContentId.Parse("core:clothing/lingerie-white-lower"));
			Assert.That(lingerie.LegacyId, Is.EqualTo(97));
			Assert.That(lingerie.Category, Is.EqualTo("Lower"));
		}

		[Test]
		public void Parse_RejectsDuplicateLegacyIdsAndUnknownFields()
		{
			const string invalid = @"{
  'schemaVersion': 1, 'type': 'coreClothingCatalog',
  'entries': [
    { 'id': 'core:clothing/a', 'legacyId': 1, 'category': 'Hat' },
    { 'id': 'core:clothing/b', 'legacyId': 1, 'category': 'Hat', 'script': 'hack.dll' }
  ]
}";
			CoreClothingCatalogLoadResult result = CoreClothingCatalogParser.Parse(invalid, "invalid.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "core-clothing.json"), Is.True);
		}

		[Test]
		public void Parse_AcceptsLosslessMigratedClothingPresentation()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'coreClothingCatalog',
  'entries': [{
    'id': 'core:clothing/test-shirt', 'legacyId': 9, 'category': 'Upper',
    'migrationVersion': 2, 'sourceAsset': 'Assets/Clothes/Upper/test.prefab',
    'icon': { 'resource': 'Modding/Core/Clothing/test-shirt/icon', 'pivotX': 0.5, 'pivotY': 0.5, 'pixelsPerUnit': 32 },
    'pieces': [{
      'id': 'piece-0-chest', 'slot': 'piece/chest',
      'sprite': { 'resource': 'Modding/Core/Clothing/test-shirt/00-piece-chest', 'pivotX': 0.25, 'pivotY': 0.75, 'pixelsPerUnit': 32 },
      'bone': 'Chest', 'offsetX': 0.1, 'offsetY': -0.2, 'sortingOffset': 2,
      'attachToBone': true, 'hideBodyPart': false, 'droppable': true,
      'connectedPieces': []
    }],
    'incompatibleCategories': ['Upper'], 'incompatibleClothing': [10], 'compatibleOverrides': [11]
  }]
}";
			CoreClothingCatalogLoadResult result = CoreClothingCatalogParser.Parse(json, "core-clothing.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			CoreClothingDefinition clothing = result.Definitions.Single();
			Assert.That(clothing.HasLoosePresentation, Is.True);
			Assert.That(clothing.SourceAsset, Is.EqualTo("Assets/Clothes/Upper/test.prefab"));
			Assert.That(clothing.Pieces.Single().Slot, Is.EqualTo("piece/chest"));
			Assert.That(clothing.Pieces.Single().Sprite.PivotY, Is.EqualTo(0.75f));
			Assert.That(clothing.IncompatibleClothing, Is.EqualTo(new[] { 10 }));
		}

		[Test]
		public void Parse_AcceptsUnityPivotsOutsideSpriteRectangle()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'coreClothingCatalog',
  'entries': [{
    'id': 'core:clothing/offset-hair', 'legacyId': 99, 'category': 'Hair',
    'migrationVersion': 1, 'sourceAsset': 'Assets/Clothes/Hair/offset.prefab',
    'icon': { 'resource': 'Modding/Core/Clothing/offset-hair/icon', 'pivotX': 0.4, 'pivotY': -0.2, 'pixelsPerUnit': 32 },
    'pieces': [{
      'id': 'piece-0-hair', 'slot': 'piece/hair',
      'sprite': { 'resource': 'Modding/Core/Clothing/offset-hair/00-piece-hair', 'pivotX': -0.5, 'pivotY': 1.8, 'pixelsPerUnit': 32 },
      'bone': 'Head', 'sortingOffset': 0, 'attachToBone': true,
      'hideBodyPart': false, 'droppable': false, 'connectedPieces': []
    }]
  }]
}";
			CoreClothingCatalogLoadResult result = CoreClothingCatalogParser.Parse(json, "core-clothing.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
		}
	}

	public class CoreChallengeDefinitionTests
	{
		[Test]
		public void PackagedCore_DiscoversCompleteVanillaChallengeCatalog()
		{
			ModContentDiscoveryResult result = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.CoreChallenges, Has.Count.EqualTo(79));
			CoreChallengeDefinition virgin = result.CoreChallenges.Single(item => item.Id == ContentId.Parse("core:challenge/virgin"));
			Assert.That(virgin.LegacyId, Is.EqualTo(1));
			Assert.That(virgin.ObjectiveAdapter, Is.EqualTo("rapeGeneral"));
			CoreChallengeDefinition wunderwaffe = result.CoreChallenges.Single(item => item.Id == ContentId.Parse("core:challenge/wunderwaffe"));
			Assert.That(wunderwaffe.LegacyId, Is.EqualTo(79));
			Assert.That(wunderwaffe.ObjectiveAdapter, Is.EqualTo("pickUp"));
		}
	}

	public class ChallengeDefinitionParserTests
	{
		private static string BuildObjective(string i_objective, string i_extra = "")
		{
			return @"{
  'schemaVersion': 1, 'type': 'challenge',
  'id': 'example.challenges:challenge/test',
  'displayName': 'Test', 'description': 'Test objective',
  'objective': '" + i_objective + @"', 'count': 3,
  'rewards': ['core:clothing/hair-2']" + i_extra + @"
}";
		}

		[Test]
		public void Parse_AcceptsEveryPublishedDataObjective()
		{
			string[] objectives = {
				"killCount", "reachWave", "surviveWaves", "pickupCount", "interactionCount",
				"shotsFired", "damageTaken", "birthCount", "impregnationCount", "rapeCount",
				"orgasmCount", "mindBreakCount", "useItemCount", "weaponKillCount", "flawlessWaves"
			};
			foreach (string objective in objectives)
			{
				ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(BuildObjective(objective), "example.challenges", objective + ".json");
				Assert.That(result.Report.IsValid, Is.True, objective);
			}
		}

		[Test]
		public void Parse_AcceptsEnemyAndWeaponFiltersForWeaponKills()
		{
			ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(
				BuildObjective("weaponKillCount", ", 'enemies': ['core:enemy/zombie-1'], 'items': ['core:item/weapon/pistol']"),
				"example.challenges", "weapon-kills.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Enemies, Has.Count.EqualTo(1));
			Assert.That(result.Definition.Items, Has.Count.EqualTo(1));
		}

		[Test]
		public void Parse_AcceptsMixedCoreAndCustomEnemyFiltersForEnemyEvents()
		{
			string[] objectives = { "killCount", "weaponKillCount", "rapeCount", "orgasmCount", "impregnationCount" };
			foreach (string objective in objectives)
			{
				string items = objective == "weaponKillCount" ? ", 'items': ['core:item/weapon/pistol']" : string.Empty;
				ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(
					BuildObjective(objective, ", 'enemies': ['core:enemy/zombie-1', 'example.enemies:enemy/stalker']" + items),
					"example.challenges", objective + "-mixed-enemies.json");
				Assert.That(result.Report.IsValid, Is.True, objective);
				Assert.That(result.Definition.Enemies, Has.Count.EqualTo(2), objective);
			}
		}

		[Test]
		public void Parse_AcceptsInheritanceWithoutReplacingLegacyObjective()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'challenge',
  'id': 'example.challenges:challenge/lamarr-remix',
  'displayName': 'Lamarr Remix', 'description': 'Inherits the complete Core behavior.',
  'extends': 'core:challenge/lamarr',
  'rewards': ['core:clothing/hair-2']
}";
			ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(json, "example.challenges", "inherited.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:challenge/lamarr")));
			Assert.That(result.Definition.Objective, Is.Empty);
		}

		[Test]
		public void Parse_AcceptsInheritanceForEveryPackagedCoreChallenge()
		{
			ModContentDiscoveryResult core = ModContentDiscovery.DiscoverPackagedCore(Resources.LoadAll<TextAsset>("Modding/Core/Content"));
			Assert.That(core.CoreChallenges, Has.Count.EqualTo(79));
			foreach (CoreChallengeDefinition template in core.CoreChallenges)
			{
				string json = @"{
  'schemaVersion': 1, 'type': 'challenge',
  'id': 'example.challenges:challenge/inherited-" + template.LegacyId + @"',
  'displayName': 'Inherited', 'description': 'Inherited Core behavior',
  'extends': '" + template.Id + @"',
  'rewards': ['core:clothing/hair-2']
}";
				ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(json, "example.challenges", template.Id + ".json");
				Assert.That(result.Report.IsValid, Is.True, template.Id.ToString());
			}
		}

		[Test]
		public void Parse_RejectsObjectiveOverridesOnInheritedChallenges()
		{
			string json = BuildObjective("killCount", ", 'extends': 'core:challenge/shack-beginner'");
			ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(json, "example.challenges", "invalid-inherited.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "challenge.inheritance-fields"), Is.True);
		}

		[Test]
		public void Parse_AcceptsOrderedSameRunStepsAndExpandedRewards()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'challenge',
  'id': 'example.challenges:challenge/sequence',
  'displayName': 'Sequence', 'description': 'Complete these in order.',
  'steps': [
    { 'objective': 'killCount', 'count': 2, 'enemies': ['core:enemy/zombie-1'] },
    { 'objective': 'shotsFired', 'count': 5 }
  ],
  'rewardBundle': {
    'currency': 250,
    'items': [{ 'id': 'core:item/consumable/ammo-box', 'amount': 2 }],
    'weapons': [{ 'id': 'core:item/weapon/pistol' }],
    'content': ['core:clothing/hair-2']
  }
}";
			ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(json, "example.challenges", "sequence.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Steps, Has.Count.EqualTo(2));
			Assert.That(result.Definition.RewardCurrency, Is.EqualTo(250));
			Assert.That(result.Definition.RewardItems.Single().Amount, Is.EqualTo(2));
			Assert.That(result.Definition.RewardWeapons.Single().Id, Is.EqualTo(ContentId.Parse("core:item/weapon/pistol")));
			Assert.That(result.Definition.RewardContent.Single(), Is.EqualTo(ContentId.Parse("core:clothing/hair-2")));
		}

		[Test]
		public void Parse_RejectsMixingOrderedStepsWithSingleObjectiveFields()
		{
			string json = BuildObjective("killCount", ", 'steps': [{ 'objective': 'shotsFired', 'count': 2 }]");
			ChallengeDefinitionLoadResult result = ChallengeDefinitionParser.Parse(json, "example.challenges", "mixed.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "challenge.steps-fields"), Is.True);
		}
	}

	public class UsableDefinitionParserTests
	{
		private const string ValidUsable = @"{
  'schemaVersion': 1,
  'type': 'usable',
  'id': 'example.medicine:item/usable/strong-aspirin',
  'displayName': 'Strong Aspirin',
  'extends': 'core:item/usable/aspirin',
  'description': 'An additive medicine that inherits Aspirin behavior.',
  'visual': {
    'type': 'coreUsableSprites',
    'icon': 'assets/strong-aspirin.png',
    'pixelsPerUnit': 32
  }
}";

		[Test]
		public void Parse_AcceptsCoreUsableWithSharedIconAndWorldSprite()
		{
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(ValidUsable, "example.medicine", "usable.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.medicine:item/usable/strong-aspirin")));
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:item/usable/aspirin")));
			Assert.That(result.Definition.Visual.World, Is.Null);
		}

		[Test]
		public void Parse_AcceptsSeparateWorldSprite()
		{
			string json = ValidUsable.Replace("'pixelsPerUnit'", "'world': 'assets/strong-aspirin-world.png', 'pixelsPerUnit'");
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(json, "example.medicine", "usable.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Visual.World, Is.EqualTo("assets/strong-aspirin-world.png"));
		}

		[Test]
		public void Parse_AcceptsTypedReplacementEffects()
		{
			string json = ValidUsable.Replace("'visual': {", "'effectMode': 'replace', 'effects': [{ 'type': 'restoreHealth', 'amount': 50 }, { 'type': 'statModifier', 'stat': 'SpeedMax', 'value': 2, 'durationSeconds': 10 }], 'visual': {");
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(json, "example.medicine", "usable.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.EffectMode, Is.EqualTo("replace"));
			Assert.That(result.Definition.Effects, Has.Count.EqualTo(2));
		}

		[Test]
		public void Parse_AcceptsAmmoAndEconomyEffects()
		{
			string json = ValidUsable.Replace("'visual': {", "'effectMode': 'replace', 'effects': [{ 'type': 'refillAmmo', 'amount': 30 }, { 'type': 'fillAllAmmo' }, { 'type': 'gainMoney', 'amount': 100 }], 'visual': {");
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(json, "example.medicine", "usable.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Effects.Select(effect => effect.Type),
				Is.EqualTo(new[] { "refillAmmo", "fillAllAmmo", "gainMoney" }));
		}

		[Test]
		public void Parse_AcceptsFullyOriginalSafeUsable()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'usable',
  'id': 'example.medicine:item/usable/field-kit', 'displayName': 'Field Kit',
  'description': 'A prefab-free data-defined item.',
  'effectMode': 'replace',
  'effects': [
    { 'type': 'restoreHealth', 'amount': 20 },
    { 'type': 'restoreStamina', 'amount': 30 },
    { 'type': 'reducePleasure', 'amount': 10 },
    { 'type': 'repairClothing' }
  ],
  'visual': {
    'type': 'originalUsableSprites', 'icon': 'assets/kit-icon.png', 'world': 'assets/kit-world.png',
    'pixelsPerUnit': 32, 'pivotX': 0.5, 'pivotY': 0.25,
    'colliderWidth': 0.8, 'colliderHeight': 0.5, 'sortingOrder': 2
  }
}";
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(json, "example.medicine", "field-kit.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.HasBaseTemplate, Is.False);
			Assert.That(result.Definition.Effects.Select(effect => effect.Type),
				Is.EqualTo(new[] { "restoreHealth", "restoreStamina", "reducePleasure", "repairClothing" }));
		}

		[Test]
		public void Parse_RejectsOriginalUsableThatAttemptsInheritedEffects()
		{
			string json = ValidUsable.Replace("'extends': 'core:item/usable/aspirin',", string.Empty)
				.Replace("'type': 'coreUsableSprites'", "'type': 'originalUsableSprites'");
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(json, "example.medicine", "unsafe-original.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "usable.effect-mode"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeAssetsNonUsableTemplatesAndArbitraryFields()
		{
			string invalid = ValidUsable
				.Replace("core:item/usable/aspirin", "core:item/weapon/pistol")
				.Replace("assets/strong-aspirin.png", "../strong-aspirin.png");
			UsableDefinitionLoadResult result = UsableDefinitionParser.Parse(invalid, "example.medicine", "usable.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "usable.extends"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "usable.visual.icon"), Is.True);

			string arbitrary = ValidUsable.Replace("'description'", "'effectClass': 'CustomEffect', 'description'");
			UsableDefinitionLoadResult fieldResult = UsableDefinitionParser.Parse(arbitrary, "example.medicine", "usable.json");
			Assert.That(fieldResult.Report.Issues.Any(issue => issue.Code == "usable.json"), Is.True);
		}
	}

	public class StageScriptParserTests
	{
		private const string Valid = @"{
  'schemaVersion': 1, 'type': 'stageScript', 'id': 'example.stage:stage-script/sequence',
  'stage': 'example.stage:stage/map', 'variables': { 'uses': 0 }, 'sequences': [
    { 'id': 'open', 'trigger': 'interaction', 'signal': 'switch-a', 'once': true,
      'conditions': [{ 'variable': 'uses', 'operator': '==', 'value': 0 }],
      'actions': [
        { 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },
        { 'type': 'set-light', 'objectId': 'light-a', 'state': 'toggle' },
        { 'type': 'queue-spawns', 'objectId': 'west-ground', 'amount': 2 }
      ] }
  ]
}";

		[Test]
		public void Parse_AcceptsReviewedStageActionsAndInteractionTrigger()
		{
			StageScriptLoadResult result = StageScriptParser.Parse(Valid, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Sequences.Single().Actions, Has.Count.EqualTo(3));
		}

		[Test]
		public void Parse_RejectsInvalidDoorState()
		{
			StageScriptLoadResult result = StageScriptParser.Parse(Valid.Replace("'state': 'open'", "'state': 'explode'"),
				"example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage-script.door-state"), Is.True);
		}

		[Test]
		public void Parse_AcceptsWorldStateWaitActions()
		{
			string json = Valid.Replace("\r\n", "\n").Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'wait-for-door', 'objectId': 'door-a', 'state': 'open', 'seconds': 10 }, { 'type': 'wait-for-enemy-count', 'state': 'at-most', 'value': 0, 'seconds': 0 }, { 'type': 'wait-until-player-distant', 'objectId': 'doll-a', 'destinationId': 'pose-a', 'value': 20, 'seconds': 0 }, { 'type': 'play-audio', 'objectId': 'room-cue' }, { 'type': 'move-object', 'objectId': 'doll-a', 'x': 4, 'y': 2, 'seconds': 1 }, { 'type': 'move-object-to', 'objectId': 'doll-a', 'destinationId': 'pose-a', 'seconds': 0 },");
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Sequences.Single().Actions, Has.Count.EqualTo(8));
		}

		[Test]
		public void Parse_AcceptsReviewedPlayerStatusModule()
		{
			string json = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'apply-player-status', 'status': 'jacky-curse', 'durationSeconds': 99999, 'ticksPerSecond': 0.25, 'chance': 0.175, 'chanceIncrease': 0.05, 'maxActive': 4 },");
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
		}

		[Test]
		public void Parse_AcceptsReviewedPlayerCutsceneActions()
		{
			string json = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'set-player-input', 'active': false }, { 'type': 'set-hud-visible', 'active': false }, { 'type': 'set-player-facing', 'state': 'right' }, { 'type': 'remove-player-clothing' }, { 'type': 'play-player-animation', 'controller': 'finisher', 'animation': 'Sqoid7' }, { 'type': 'kill-player' },");
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
		}

		[Test]
		public void Parse_RejectsUnreviewedPlayerAnimationControllerAndFacing()
		{
			string controller = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'play-player-animation', 'controller': 'arbitrary', 'animation': 'Idle' },");
			string facing = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'set-player-facing', 'state': 'up' },");
			Assert.That(StageScriptParser.Parse(controller, "example.stage", "script.json").Report.Issues.Any(issue => issue.Code == "stage-script.player-animation"), Is.True);
			Assert.That(StageScriptParser.Parse(facing, "example.stage", "script.json").Report.Issues.Any(issue => issue.Code == "stage-script.player-facing"), Is.True);
		}

		[Test]
		public void Parse_AcceptsManualSequenceComposition()
		{
			string json = Valid.Replace("\r\n", "\n").Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'run-sequence', 'sequence': 'helper' }, { 'type': 'start-sequence', 'sequence': 'parallel' }, { 'type': 'wait-for-signal', 'signal': 'ready', 'seconds': 2 }, { 'type': 'wait-for-sequence', 'sequence': 'parallel', 'seconds': 2 }, { 'type': 'cancel-sequence', 'sequence': 'parallel' },")
				.Replace("]\n}", ", { 'id': 'helper', 'trigger': 'manual', 'actions': [{ 'type': 'notify', 'message': 'Helper' }] }, { 'id': 'parallel', 'trigger': 'manual', 'actions': [{ 'type': 'wait', 'seconds': 1 }] }]\n}");
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
		}

		[Test]
		public void Parse_AcceptsGeneralWaitAndBoundedRepeatActions()
		{
			string json = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'repeat', 'amount': 3, 'actions': [{ 'type': 'add-variable', 'variable': 'uses', 'value': 1 }, { 'type': 'wait', 'seconds': 0.05 }] }, { 'type': 'wait-until', 'seconds': 5, 'conditions': [{ 'variable': 'uses', 'operator': '>=', 'value': 3 }] },");
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
			Assert.That(result.Definition.Sequences.Single().Actions.Single(action => action.Type == "repeat").Actions,
				Has.Count.EqualTo(2));
		}

		[Test]
		public void Parse_RejectsUnsafeRepeatGraphsAndNestedMissingSequences()
		{
			string excessive = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'repeat', 'amount': 100, 'actions': [{ 'type': 'repeat', 'amount': 100, 'actions': [{ 'type': 'wait', 'seconds': 0 }] }] },");
			StageScriptLoadResult excessiveResult = StageScriptParser.Parse(excessive, "example.stage", "script.json");
			Assert.That(excessiveResult.Report.Issues.Any(issue => issue.Code == "stage-script.action-budget"), Is.True);

			string missing = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'repeat', 'amount': 2, 'actions': [{ 'type': 'run-sequence', 'sequence': 'missing' }] },");
			StageScriptLoadResult missingResult = StageScriptParser.Parse(missing, "example.stage", "script.json");
			Assert.That(missingResult.Report.Issues.Any(issue => issue.Code == "stage-script.sequence-reference"), Is.True);
		}

		[Test]
		public void Parse_RejectsWaitUntilWithoutConditions()
		{
			string json = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'wait-until', 'seconds': 5 },");
			Assert.That(StageScriptParser.Parse(json, "example.stage", "script.json").Report.Issues
				.Any(issue => issue.Code == "stage-script.wait-until"), Is.True);
		}

		[Test]
		public void Parse_RejectsMissingAndCyclicSequenceReferences()
		{
			string missing = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'run-sequence', 'sequence': 'missing' },");
			Assert.That(StageScriptParser.Parse(missing, "example.stage", "script.json").Report.Issues.Any(issue => issue.Code == "stage-script.sequence-reference"), Is.True);

			string cyclic = Valid.Replace("{ 'type': 'set-door', 'objectId': 'door-a', 'state': 'open' },",
				"{ 'type': 'run-sequence', 'sequence': 'open' },");
			Assert.That(StageScriptParser.Parse(cyclic, "example.stage", "script.json").Report.Issues.Any(issue => issue.Code == "stage-script.sequence-cycle"), Is.True);
		}

		[Test]
		public void Parse_AcceptsWorldTriggersAndSelectorConditions()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'stageScript', 'id': 'example.stage:stage-script/world-events',
  'stage': 'example.stage:stage/map', 'sequences': [
    { 'id': 'door-opened', 'trigger': 'door-state', 'objectId': 'lab-door', 'state': 'open',
      'conditions': [{ 'source': 'wave', 'operator': '>=', 'value': 2 }],
      'actions': [{ 'type': 'set-interaction-enabled', 'objectId': 'console', 'active': true }] },
    { 'id': 'room-cleared', 'trigger': 'enemy-count', 'state': 'at-most', 'value': 0, 'enemy': 'core:enemy/sunny',
      'conditions': [{ 'source': 'object-active', 'objectId': 'console', 'operator': '==', 'value': 1 }],
      'actions': [{ 'type': 'notify', 'message': 'Room clear' }] },
    { 'id': 'pulse', 'trigger': 'timer', 'seconds': 5,
      'actions': [{ 'type': 'send-signal', 'signal': 'pulse' }] }
  ]
}";
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "script.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
		}

		[Test]
		public void Parse_AcceptsPlayerVolumeTriggers()
		{
			string json = Valid.Replace("'trigger': 'interaction', 'signal': 'switch-a'",
				"'trigger': 'player-enter', 'objectId': 'secret-room-volume'");
			Assert.That(StageScriptParser.Parse(json, "example.stage", "script.json").Report.IsValid, Is.True);
			json = json.Replace("'trigger': 'player-enter'", "'trigger': 'player-exit'");
			Assert.That(StageScriptParser.Parse(json, "example.stage", "script.json").Report.IsValid, Is.True);
		}

		[Test]
		public void Parse_AcceptsReusableMachineStatesAndTransitions()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'stageScript', 'id': 'example.stage:stage-script/machine',
  'stage': 'example.stage:stage/map',
  'machines': [{ 'id': 'generator', 'objectId': 'generator-console', 'initialState': 'off', 'states': [
    { 'id': 'off', 'actions': [{ 'type': 'set-light', 'objectId': 'warning-light', 'state': 'off' }] },
    { 'id': 'active', 'actions': [{ 'type': 'play-audio', 'objectId': 'generator-hum' }] }
  ] }],
  'sequences': [
    { 'id': 'start', 'trigger': 'interaction', 'signal': 'generator-console',
      'actions': [{ 'type': 'set-machine-state', 'objectId': 'generator', 'state': 'active' }] },
    { 'id': 'activated', 'trigger': 'machine-state', 'objectId': 'generator', 'state': 'active',
      'actions': [{ 'type': 'notify', 'message': 'Generator active' }] }
  ]
}";
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "machine.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Message)));
			Assert.That(result.Definition.Machines, Has.Count.EqualTo(1));
			Assert.That(result.Definition.Machines.Single().States, Has.Count.EqualTo(2));
		}

		[Test]
		public void Parse_RejectsMissingMachineStatesAndReferences()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'stageScript', 'id': 'example.stage:stage-script/machine',
  'stage': 'example.stage:stage/map',
  'machines': [{ 'id': 'generator', 'objectId': 'generator-console', 'initialState': 'missing', 'states': [
    { 'id': 'off', 'actions': [{ 'type': 'set-machine-state', 'objectId': 'generator', 'state': 'active' }] }
  ] }],
  'sequences': [{ 'id': 'bad-trigger', 'trigger': 'machine-state', 'objectId': 'generator', 'state': 'active',
    'actions': [{ 'type': 'notify', 'message': 'Never' }] }]
}";
			StageScriptLoadResult result = StageScriptParser.Parse(json, "example.stage", "machine.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage-script.machine-initial-state"), Is.True);
			Assert.That(result.Report.Issues.Count(issue => issue.Code == "stage-script.machine-reference"), Is.EqualTo(2));
		}
	}

	public class StageDefinitionParserTests
	{
		private const string ValidStage = @"{
  'schemaVersion': 1,
  'type': 'stage',
  'id': 'example.stages:stage/training-yard',
  'displayName': 'Training Yard',
  'extends': 'core:stage/field-day',
  'description': 'A template-backed stage with data-defined encounters.',
  'layout': {
    'type': 'coreStageLayout',
    'playerSpawn': { 'x': 4, 'y': 2 }
  },
  'waves': {
    'firstWaveEnemyCount': 5
  },
  'spawners': [
    {
      'id': 'west-ground',
      'position': { 'x': -12, 'y': 3 },
      'enemies': [ 'core:enemy/zombie-1', 'example.stages:enemy/training-zombie' ],
      'selectionWeight': 1,
      'minimumWave': 0,
      'delaySeconds': 1.5,
      'delayJitterSeconds': 0.25,
      'initialDelaySeconds': 1,
      'initialDelayJitterSeconds': 0.25,
      'spawnOutOfSight': true
    }
  ]
}";

		[Test]
		public void Parse_AcceptsTemplateLayoutAndStableEnemyReferences()
		{
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(ValidStage, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.stages:stage/training-yard")));
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse("core:stage/field-day")));
			Assert.That(result.Definition.Layout.PlayerSpawn.X, Is.EqualTo(4));
			Assert.That(result.Definition.Waves.FirstWaveEnemyCount, Is.EqualTo(5));
			Assert.That(result.Definition.Spawners.Single().Enemies, Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:enemy/zombie-1"),
				ContentId.Parse("example.stages:enemy/training-zombie")
			}));
		}

		[Test]
		public void Parse_AcceptsReservedStageIndependentRuntimeTemplate()
		{
			string json = ValidStage.Replace("core:stage/field-day", "core:stage/mod-template")
				.Replace("'type': 'coreStageLayout',\n    'playerSpawn': { 'x': 4, 'y': 2 }",
					"'type': 'tiledJson',\n    'path': 'levels/training-yard.json',\n    'pixelsPerUnit': 32");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(json, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.UsesRuntimeTemplate, Is.True);
			Assert.That(result.Definition.Extends.Path, Is.EqualTo(StageDefinition.RuntimeTemplatePath));
		}

		[Test]
		public void Parse_RejectsRuntimeTemplateWithPrefabLayoutOrInheritedObjects()
		{
			string prefabLayout = ValidStage.Replace("core:stage/field-day", "core:stage/mod-template");
			StageDefinitionLoadResult prefabResult = StageDefinitionParser.Parse(
				prefabLayout, "example.stages", "stage.json");
			Assert.That(prefabResult.Report.Issues.Any(issue => issue.Code == "stage.runtime-template-layout"), Is.True);

			string inherited = prefabLayout.Replace("'type': 'coreStageLayout',\n    'playerSpawn': { 'x': 4, 'y': 2 }",
				"'type': 'tiledJson',\n    'path': 'levels/training-yard.json',\n    'pixelsPerUnit': 32,\n    'preserveInheritedStageObjects': true");
			StageDefinitionLoadResult inheritedResult = StageDefinitionParser.Parse(
				inherited, "example.stages", "stage.json");
			Assert.That(inheritedResult.Report.Issues.Any(issue => issue.Code == "stage.runtime-template-inheritance"), Is.True);
		}

		[Test]
		public void Parse_AcceptsDoorGatedSpawnersAndRejectsContradictions()
		{
			string valid = ValidStage.Replace("'spawnOutOfSight': true", "'spawnOutOfSight': true, 'enabled': false, 'requiredOpenDoors': ['lab-door']");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(valid, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Spawners.Single().RequiredOpenDoors.Single(), Is.EqualTo("lab-door"));
			Assert.That(result.Definition.Spawners.Single().Enabled, Is.False);
			string invalid = valid.Replace("'requiredOpenDoors': ['lab-door']", "'requiredOpenDoors': ['lab-door'], 'requiredClosedDoors': ['lab-door']");
			Assert.That(StageDefinitionParser.Parse(invalid, "example.stages", "stage.json").Report.Issues.Any(
				issue => issue.Code == "stage.spawner.door-conflict"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeSpawnerTimingDuplicateReferencesAndHubInheritance()
		{
			string json = ValidStage
				.Replace("core:stage/field-day", "core:stage/hub")
				.Replace("'delayJitterSeconds': 0.25", "'delayJitterSeconds': 2")
				.Replace("'core:enemy/zombie-1', 'example.stages:enemy/training-zombie'", "'core:enemy/zombie-1', 'core:enemy/zombie-1'");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(json, "example.stages", "stage.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage.extends"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage.spawner.delay-jitter"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage.spawner.duplicate-enemy"), Is.True);
		}

		[Test]
		public void Parse_RejectsOutOfBoundsCoordinatesAndUnknownPrefabFields()
		{
			string coordinates = ValidStage.Replace("'x': -12", "'x': 100001");
			StageDefinitionLoadResult coordinateResult = StageDefinitionParser.Parse(coordinates, "example.stages", "stage.json");
			Assert.That(coordinateResult.Report.Issues.Any(issue => issue.Code == "stage.spawner.position"), Is.True);

			string arbitrary = ValidStage.Replace("'description'", "'prefab': 'Assets/CustomStage.prefab', 'description'");
			StageDefinitionLoadResult fieldResult = StageDefinitionParser.Parse(arbitrary, "example.stages", "stage.json");
			Assert.That(fieldResult.Report.Issues.Any(issue => issue.Code == "stage.json"), Is.True);
		}

		[Test]
		public void Parse_AcceptsPackRelativeTiledLayout()
		{
			string json = ValidStage.Replace("'type': 'coreStageLayout',\n    'playerSpawn': { 'x': 4, 'y': 2 }",
				"'type': 'tiledJson',\n    'path': 'levels/training-yard.json',\n    'pixelsPerUnit': 32,\n    'hideInheritedVisuals': true");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(json, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Layout.Path, Is.EqualTo("levels/training-yard.json"));
			Assert.That(result.Definition.Layout.PlayerSpawn, Is.Null);
			Assert.That(result.Definition.Layout.HideInheritedVisuals, Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeTiledLayoutPathAndScale()
		{
			string json = ValidStage.Replace("'type': 'coreStageLayout',\n    'playerSpawn': { 'x': 4, 'y': 2 }",
				"'type': 'tiledJson',\n    'path': '../training-yard.json',\n    'pixelsPerUnit': 0");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(json, "example.stages", "stage.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage.layout.path"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "stage.layout.pixels-per-unit"), Is.True);
		}

		[Test]
		public void Parse_AcceptsPartialStageAudioOverridesAndRejectsUnsafeAudioPaths()
		{
			string valid = ValidStage.Replace("'waves': {", "'audio': { 'ambience': 'assets/audio/night.ogg', 'waveMusic': 'assets/audio/wave.wav' },\n  'waves': {");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(valid, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Audio.Ambience, Is.EqualTo("assets/audio/night.ogg"));
			Assert.That(result.Definition.Audio.EntryMusic, Is.Null);

			string unsafeJson = valid.Replace("assets/audio/night.ogg", "../night.mp3");
			StageDefinitionLoadResult unsafeResult = StageDefinitionParser.Parse(unsafeJson, "example.stages", "stage.json");
			Assert.That(unsafeResult.Report.Issues.Any(issue => issue.Code == "stage.audio.ambience"), Is.True);
		}

		[Test]
		public void Parse_AcceptsViewportCameraBoundsAndRejectsInvertedBounds()
		{
			string valid = ValidStage.Replace("'waves': {", "'camera': { 'bounds': { 'minX': -20, 'minY': 0, 'maxX': 30, 'maxY': 18 } },\n  'waves': {");
			StageDefinitionLoadResult result = StageDefinitionParser.Parse(valid, "example.stages", "stage.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Camera.Bounds.MaxX, Is.EqualTo(30f));
			StageDefinitionLoadResult invalid = StageDefinitionParser.Parse(valid.Replace("'maxX': 30", "'maxX': -30"), "example.stages", "stage.json");
			Assert.That(invalid.Report.Issues.Any(issue => issue.Code == "stage.camera.bounds"), Is.True);
			string unbounded = ValidStage.Replace("'waves': {", "'camera': { 'unbounded': true },\n  'waves': {");
			StageDefinitionLoadResult unboundedResult = StageDefinitionParser.Parse(unbounded, "example.stages", "stage.json");
			Assert.That(unboundedResult.Report.IsValid, Is.True);
			Assert.That(unboundedResult.Definition.Camera.Unbounded, Is.True);
		}
	}

	public class TiledLevelParserTests
	{
		private const string ValidMap = @"{
  'orientation': 'orthogonal', 'infinite': false,
  'width': 20, 'height': 10, 'tilewidth': 32, 'tileheight': 32,
  'layers': [{ 'type': 'objectgroup', 'name': 'Gameplay', 'objects': [
    { 'id': 1, 'name': 'ground', 'type': 'platform', 'x': 32, 'y': 256, 'width': 576, 'height': 32 },
    { 'id': 2, 'name': 'player', 'type': 'player-spawn', 'point': true, 'x': 320, 'y': 256 },
    { 'id': 3, 'name': 'west-ground', 'type': 'enemy-spawner', 'point': true, 'x': 64, 'y': 256 }
  ]}]
}";

		[Test]
		public void Parse_ExtractsGameplayObjectsAndConvertsTiledCoordinates()
		{
			TiledLevelLoadResult result = TiledLevelParser.Parse(ValidMap, "level.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Platforms, Has.Count.EqualTo(1));
			Assert.That(result.Definition.EnemySpawners.Single().Name, Is.EqualTo("west-ground"));
			StagePointDefinition spawn = result.Definition.ToUnityPoint(result.Definition.PlayerSpawn, 32);
			Assert.That(spawn.X, Is.EqualTo(0));
			Assert.That(spawn.Y, Is.EqualTo(2));
		}

		[Test]
		public void Parse_AcceptsBidirectionalNavigationNodes()
		{
			string map = ValidMap.Replace("{ 'id': 3, 'name': 'west-ground', 'type': 'enemy-spawner', 'point': true, 'x': 64, 'y': 256 }",
				"{ 'id': 3, 'name': 'west-ground', 'type': 'enemy-spawner', 'point': true, 'x': 64, 'y': 256 }, { 'id': 4, 'name': 'lower', 'type': 'nav-node', 'point': true, 'x': 96, 'y': 256, 'properties': [{ 'name': 'connectionType', 'type': 'string', 'value': 'climb' }, { 'name': 'bidirectional', 'type': 'bool', 'value': true }, { 'name': 'links', 'type': 'string', 'value': 'upper' }] }, { 'id': 5, 'name': 'upper', 'type': 'nav-node', 'point': true, 'x': 96, 'y': 128 }");
			TiledLevelLoadResult result = TiledLevelParser.Parse(map, "level.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.NavNodes.Single(node => node.Point.Name == "lower").Bidirectional, Is.True);
		}

		[Test]
		public void Parse_AcceptsExplicitCoreStageObjectAdapter()
		{
			string map = ValidMap.Replace("\n  ]}]", @",
    { 'id': 4, 'name': 'lab-fuse', 'type': 'core-stage-object', 'point': true, 'x': 128, 'y': 224,
      'properties': [
		{ 'name': 'sourcePath', 'type': 'string', 'value': 'Interactables/Lab/int_fuseBoxLab' },
		{ 'name': 'coreObjectKind', 'type': 'string', 'value': 'interaction' },
        { 'name': 'initiallyActive', 'type': 'bool', 'value': false }
      ] }
  ]}]");
			TiledLevelLoadResult result = TiledLevelParser.Parse(map, "level.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			TiledCoreStageObjectDefinition adapter = result.Definition.CoreStageObjects.Single();
			Assert.That(adapter.SourcePath, Is.EqualTo("Interactables/Lab/int_fuseBoxLab"));
			Assert.That(adapter.Kind, Is.EqualTo("interaction"));
			Assert.That(adapter.InitiallyActive, Is.False);
		}

		[Test]
		public void Parse_RejectsInfiniteMapsAndIncorrectGameplayShapes()
		{
			string invalid = ValidMap.Replace("'infinite': false", "'infinite': true")
				.Replace("'type': 'platform', 'x'", "'type': 'platform', 'point': true, 'x'")
				.Replace("'name': 'player', 'type': 'player-spawn', 'point': true",
					"'name': 'player', 'type': 'player-spawn', 'point': false");
			TiledLevelLoadResult result = TiledLevelParser.Parse(invalid, "level.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.infinite"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.platform-shape"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.point-shape"), Is.True);
		}

		[Test]
		public void Parse_ExtractsInlineTilesetsAndVisibleTileLayers()
		{
			string visualMap = ValidMap.Replace("'layers': [",
				"'tilesets': [{ 'firstgid': 1, 'name': 'terrain', 'tilewidth': 32, 'tileheight': 32, 'tilecount': 1, 'columns': 1, 'image': 'assets/terrain.png', 'imagewidth': 32, 'imageheight': 32 }],\n  'layers': [{ 'type': 'tilelayer', 'name': 'Ground', 'width': 2, 'height': 1, 'x': 3, 'y': 7, 'opacity': 0.5, 'data': [1, 0] },");
			TiledLevelLoadResult result = TiledLevelParser.Parse(visualMap, "level.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Tilesets, Has.Count.EqualTo(1));
			Assert.That(result.Definition.TileLayers, Has.Count.EqualTo(1));
			Assert.That(result.Definition.TileLayers[0].X, Is.EqualTo(96));
			Assert.That(result.Definition.TileLayers[0].Y, Is.EqualTo(224));
			Assert.That(result.Definition.TileLayers[0].Opacity, Is.EqualTo(0.5f));
		}

		[Test]
		public void Parse_RejectsIncompleteOrUnsafeSharedObjectVisuals()
		{
			string objects = @",
    { 'id': 4, 'name': 'case', 'type': 'weapon-case', 'point': true, 'x': 96, 'y': 224,
      'properties': [
        { 'name': 'weapon', 'type': 'string', 'value': 'core:item/weapon/revolver-44' },
        { 'name': 'caseSize', 'type': 'int', 'value': 1 },
        { 'name': 'visualFile', 'type': 'file', 'value': '../outside.png' }
      ] },
    { 'id': 5, 'name': 'door', 'type': 'door', 'point': true, 'x': 128, 'y': 224,
      'properties': [
        { 'name': 'doorType', 'type': 'string', 'value': 'standard' },
        { 'name': 'openVisualFile', 'type': 'file', 'value': 'assets/open.png' }
      ] },
    { 'id': 6, 'name': 'switch', 'type': 'door-switch', 'point': true, 'x': 160, 'y': 224,
      'properties': [
        { 'name': 'targetDoors', 'type': 'string', 'value': 'door' },
        { 'name': 'onVisualFile', 'type': 'file', 'value': 'assets/on.png' }
      ] }";
			string map = ValidMap.Replace("\n  ]}]", objects + "\n  ]}]");
			TiledLevelLoadResult result = TiledLevelParser.Parse(map, "level.json");
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.weapon-case-visual"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.door-visual"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "tiled.door-switch-visual"), Is.True);
		}

		[Test]
		public void Parse_AcceptsPackLocalSharedObjectVisualPair()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ModSDK/MapTemplates/TiledStage"));
			string objects = @",
    { 'id': 4, 'name': 'case', 'type': 'weapon-case', 'point': true, 'x': 96, 'y': 224,
      'properties': [
        { 'name': 'weapon', 'type': 'string', 'value': 'core:item/weapon/revolver-44' },
        { 'name': 'caseSize', 'type': 'int', 'value': 1 },
        { 'name': 'visualFile', 'type': 'file', 'value': 'assets/previews/WeaponVendor.png' },
        { 'name': 'brokenVisualFile', 'type': 'file', 'value': 'assets/previews/WeaponVendor.png' },
        { 'name': 'visualPixelsPerUnit', 'type': 'float', 'value': 64 }
      ] }";
			string map = ValidMap.Replace("\n  ]}]", objects + "\n  ]}]");
			System.Reflection.MethodInfo parse = typeof(TiledLevelParser).GetMethod("ParseInternal",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			Assert.That(parse, Is.Not.Null);
			TiledLevelLoadResult result = (TiledLevelLoadResult)parse.Invoke(null, new object[]
			{
				map, "test-map.json", root, root
			});
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Definition.WeaponCases.Single().Point.VisualFile,
				Is.EqualTo("assets/previews/WeaponVendor.png"));
			Assert.That(result.Definition.WeaponCases.Single().Point.VisualPixelsPerUnit, Is.EqualTo(64f));
		}

		[Test]
		public void Parse_AcceptsPreviewBackedMachinesAndOrderedSteps()
		{
			string objects = @",
    { 'id': 4, 'name': 'power', 'class': 'logic-switch', 'gid': 1, 'x': 128, 'y': 224, 'width': 16, 'height': 16,
      'properties': [{ 'name': 'message', 'type': 'string', 'value': 'Power on.' }] },
    { 'id': 5, 'name': 'reward', 'class': 'easter-egg-step', 'gid': 1, 'x': 192, 'y': 224, 'width': 16, 'height': 16,
      'properties': [
        { 'name': 'message', 'type': 'string', 'value': 'Sequence complete.' },
        { 'name': 'requiredInteractions', 'type': 'string', 'value': 'power' },
        { 'name': 'requiredMoney', 'type': 'int', 'value': 250 },
        { 'name': 'restoreHealth', 'type': 'float', 'value': 20 }
      ] }";
			string map = ValidMap.Replace("\n  ]}]", objects + "\n  ]}]");
			TiledLevelLoadResult result = TiledLevelParser.Parse(map, "level.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Definition.Interactions, Has.Count.EqualTo(2));
			TiledInteractionDefinition reward = result.Definition.Interactions.Single(item => item.Area.Name == "reward");
			Assert.That(reward.RequiredInteractionIds, Is.EqualTo(new[] { "power" }));
			Assert.That(reward.RequiredMoney, Is.EqualTo(250));
			Assert.That(reward.RestoreHealth, Is.EqualTo(20f));
		}

		[Test]
		public void Parse_AcceptsValidatedMultiRoomProgressionWithoutCheckpoints()
		{
			string objects = @",
    { 'id': 4, 'name': 'west-room', 'type': 'room', 'x': 0, 'y': 0, 'width': 320, 'height': 320,
      'properties': [{ 'name': 'initial', 'type': 'bool', 'value': true }] },
    { 'id': 5, 'name': 'east-room', 'type': 'room', 'x': 320, 'y': 0, 'width': 320, 'height': 320 },
    { 'id': 6, 'name': 'east-entry', 'type': 'room-entry', 'point': true, 'x': 352, 'y': 256,
      'properties': [{ 'name': 'room', 'type': 'string', 'value': 'east-room' }] },
    { 'id': 7, 'name': 'to-east', 'type': 'room-transition', 'x': 288, 'y': 192, 'width': 32, 'height': 64,
      'properties': [
        { 'name': 'destination', 'type': 'string', 'value': 'east-entry' },
        { 'name': 'requireNoEnemies', 'type': 'bool', 'value': true },
        { 'name': 'oneShot', 'type': 'bool', 'value': true }
      ] }";
			string map = ValidMap.Replace("\n  ]}]", objects + "\n  ]}]");
			TiledLevelLoadResult result = TiledLevelParser.Parse(map, "level.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Definition.Rooms, Has.Count.EqualTo(2));
			Assert.That(result.Definition.Rooms.Single(room => room.Initial).Rectangle.Name, Is.EqualTo("west-room"));
			Assert.That(result.Definition.RoomEntries.Single().RoomId, Is.EqualTo("east-room"));
			Assert.That(result.Definition.RoomTransitions.Single().DestinationId, Is.EqualTo("east-entry"));
			Assert.That(result.Definition.RoomTransitions.Single().RequireNoEnemies, Is.True);
			Assert.That(result.Definition.RoomTransitions.Single().OneShot, Is.True);
		}
	}

	public class ModContentDiscoveryTests
	{
		[Test]
		public void TiledTrainingYardExample_DiscoversAuthoredLayout()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack pack = packs.Packs.Single(item => item.Manifest.Id == "example.tiled-training-yard");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			Assert.That(content.Report.IsValid, Is.True);
			StageDefinition stage = content.Stages.Single();
			Assert.That(stage.Layout.TiledLevel.Platforms, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.EnemySpawners, Has.Count.EqualTo(2));
			StageScriptDefinition script = content.StageScripts.Single();
			Assert.That(script.Stage, Is.EqualTo(stage.Id));
			Assert.That(script.Sequences.Select(item => item.Trigger),
				Is.EquivalentTo(new[] { "stage-open", "manual", "wave-start" }));
		}

		[Test]
		public void ExternalStageClone_RestoresFerScriptsAndObjectIds()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack pack = ModDiscovery.Discover(examples).Packs.Single(item => item.Manifest.Id == "example.core-fer-tiled-port");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			StageDefinition definition = content.Stages.Single();
			Assert.That(content.StageScripts, Has.Count.EqualTo(4), "The FER clone test must carry all four real stage scripts.");
			System.Type stageType = System.Type.GetType("Stage, Assembly-CSharp");
			System.Type runnerType = System.Type.GetType("ModStageScriptRunner, Assembly-CSharp");
			System.Type markerType = System.Type.GetType("ModStageMarker, Assembly-CSharp");
			System.Type bridgeType = System.Type.GetType("ModStageActivationBridge, Assembly-CSharp");
			System.Type factoryType = System.Type.GetType("ExternalStageFactory, Assembly-CSharp");
			Assert.That(stageType != null && runnerType != null && markerType != null
				&& bridgeType != null && factoryType != null, Is.True);
			GameObject source = new GameObject("FER clone configuration source");
			GameObject clone = null;
			try
			{
				source.SetActive(false);
				Component sourceStage = source.AddComponent(stageType);
				source.AddComponent<RuntimeContentIdentity>().Configure(definition.Id, ContentCategory.Stage);
				Component sourceRunner = source.AddComponent(runnerType);
				runnerType.GetMethod("Configure").Invoke(sourceRunner,
					new object[] { content.StageScripts.Where(item => item.Stage == definition.Id).ToArray() });
				GameObject markerObject = new GameObject("portable FER marker");
				markerObject.transform.SetParent(source.transform, false);
				Component sourceMarker = markerObject.AddComponent(markerType);
				markerType.GetMethod("Configure").Invoke(sourceMarker, new object[] { "int-doorbasement" });
				bridgeType.GetMethod("Configure").Invoke(markerObject.AddComponent(bridgeType),
					new object[] { "int-doorbasement" });
				clone = Object.Instantiate(source);
				Component cloneRunner = clone.GetComponent(runnerType);
				System.Reflection.FieldInfo scriptsField = runnerType.GetField("m_scripts",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				Assert.That(((System.Collections.ICollection)scriptsField.GetValue(cloneRunner)).Count, Is.Zero,
					"Unity does not serialize the configured runner's script list into the playable clone.");
				factoryType.GetMethod("RestoreRuntimeStageClone").Invoke(null,
					new object[] { sourceStage, clone.GetComponent(stageType) });
				Assert.That(((System.Collections.ICollection)scriptsField.GetValue(cloneRunner)).Count,
					Is.EqualTo(content.StageScripts.Count));
				Assert.That((string)markerType.GetProperty("ObjectId").GetValue(clone.GetComponentInChildren(markerType, true)),
					Is.EqualTo("int-doorbasement"));
				Assert.That((string)bridgeType.GetField("m_objectId",
					System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
					.GetValue(clone.GetComponentInChildren(bridgeType, true)), Is.EqualTo("int-doorbasement"));
			}
			finally
			{
				if (clone != null) Object.DestroyImmediate(clone);
				Object.DestroyImmediate(source);
			}
		}

		[Test]
		public void CoreFerTiledPort_ResolvesPortableArtworkAndGameplayObjects()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack pack = packs.Packs.Single(item => item.Manifest.Id == "example.core-fer-tiled-port");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));

			StageDefinition stage = content.Stages.Single();
			TiledLevelDefinition level = stage.Layout.TiledLevel;
			Assert.That(stage.Extends, Is.EqualTo(ContentId.Parse("core:stage/furry-entertainment-robotics")));
			Assert.That(stage.Camera.Unbounded, Is.True,
				"FER uses the original unclamped camera movement, including attic presentation outside the export rectangle.");
			Assert.That(stage.Layout.HideInheritedVisuals, Is.True);
			Assert.That(stage.Spawners.SelectMany(spawner => spawner.Enemies).Distinct(),
				Is.EquivalentTo(new[] { ContentId.Parse("core:enemy/sunny"), ContentId.Parse("core:enemy/jenny"),
					ContentId.Parse("core:enemy/abby"), ContentId.Parse("core:enemy/jacky") }));
			for (int index = 13; index <= 16; index++)
				Assert.That(stage.Spawners.Single(spawner => spawner.Id == "fer-spawner-" + index).Enemies,
					Is.EquivalentTo(new[] { ContentId.Parse("core:enemy/abby") }),
					"The four basement Abby spawners must preserve Main.unity's enemy overrides.");
			Assert.That(stage.Spawners.Count(spawner => spawner.Enemies.Contains(ContentId.Parse("core:enemy/abby"))),
				Is.EqualTo(4), "Abby must not leak into FER's initially enabled wave spawners.");
			Assert.That(stage.Spawners.Single(spawner => spawner.Id == "fer-spawner-22").Enemies,
				Is.EquivalentTo(new[] { ContentId.Parse("core:enemy/jacky") }),
				"FER's final spawner must preserve its Main.unity Jacky override.");
			Assert.That(stage.Spawners.Single(spawner => spawner.Id == "fer-spawner-22").Enabled, Is.False,
				"Jacky must not enter the normal wave before the keycard-triggered encounter.");
			Assert.That(stage.Layout.PreserveInheritedStageObjects, Is.False);
			Assert.That(level.CoreStageObjects, Has.Count.EqualTo(28),
				"FER's exact animated presentation hierarchies must resolve from the stable Core prop catalog.");
			TiledStageItemDefinition glowingKeycard = level.StageItems.Single(item => item.Point.Name == "item-jackyroomkeycard");
			Assert.That(glowingKeycard.LightColor, Is.EqualTo("#FFEF00"));
			Assert.That(glowingKeycard.LightSides, Is.EqualTo(6));
			Assert.That(glowingKeycard.LightRadius, Is.EqualTo(0.06f).Within(0.0001f),
				"The keycard glow must travel with its portable stage item, not remain as a Core prop.");
			string rawFerMap = File.ReadAllText(Path.Combine(pack.RootPath, "levels", "fer-port.json"));
			Assert.That(rawFerMap, Does.Not.Contain("\"type\": \"core-stage-object\""),
				"The distributable FER map must not expose fragile prefab hierarchy adapters.");
			Assert.That(rawFerMap.Split(new[] { "\"type\": \"core-prop\"" },
				System.StringSplitOptions.None).Length - 1, Is.EqualTo(28));
			Assert.That(level.StageMarkers.Select(item => item.Name), Is.EquivalentTo(new[]
			{
				"atp-jennydining1", "atp-jennydining2", "atp-jennydining3",
				"atp-jennytheatre1", "atp-jennytheatre2", "atp-jennytheatre3",
				"atp-sunnydining1", "atp-sunnydining2", "atp-sunnydining3",
				"atp-sunnytheatre1", "atp-sunnytheatre2", "atp-sunnytheatre3"
			}), "All statue destinations must be portable Tiled points.");
			Assert.That(level.CoreStageObjects.Any(item => item.Kind == "pose"), Is.False,
				"Statue destinations must no longer depend on the inherited FER pose hierarchy.");
			Assert.That(level.CoreStageObjects.Any(item => item.Kind == "door" || item.Kind == "actor"), Is.False,
				"FER doors and scripted actors must no longer require inherited stage-object adapters.");
			Assert.That(level.Doors, Has.Count.EqualTo(7));
			Assert.That(level.Doors.All(item => !item.InitiallyOpen && item.SingleUse), Is.True);
			Assert.That(level.Doors.Single(item => item.Point.Name == "int-doorgewehr").RequiredItemId,
				Is.EqualTo("item-jackyroomkeycard"));
			Assert.That(level.Doors.Single(item => item.Point.Name == "int-doorjackyroom").InitiallyInteractable, Is.False);
			Assert.That(level.ScriptedActors.Select(item => item.Enemy), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:enemy/abby"), ContentId.Parse("core:enemy/android"),
				ContentId.Parse("core:enemy/jacky")
			}));
			Assert.That(level.ScriptedActors.All(item => !item.InitiallyActive), Is.True);
			string[] fuseIds = { "int-fuseboxattic", "int-fuseboxbasement", "int-fuseboxlab" };
			Assert.That(level.Interactions.Where(item => fuseIds.Contains(item.Area.Name)).Select(item => item.Area.Name),
				Is.EquivalentTo(fuseIds), "FER's fuse boxes must be portable Tiled interactions.");
			Assert.That(level.Interactions.Where(item => fuseIds.Contains(item.Area.Name))
				.All(item => item.Price == 250 && item.SingleUse && item.VisualFile.EndsWith("fer-fuse-box-closed.png")
					&& item.ActivatedVisualFile.EndsWith("fer-fuse-box-open.png")), Is.True,
				"Portable fuse boxes must preserve their original price and visual state change.");
			Assert.That(level.CoreStageObjects.Any(item => fuseIds.Contains(item.Point.Name)), Is.False,
				"FER's fuse boxes must no longer require inherited FuseBox components.");
			string[] fuseIndicatorIds = { "lightbulb1", "lightbulb2", "lightbulb3" };
			Assert.That(level.Lights.Where(item => fuseIndicatorIds.Contains(item.Point.Name))
				.Select(item => item.Point.Name), Is.EquivalentTo(fuseIndicatorIds),
				"FER's three fuse indicators must be live portable lights, not static off-state decorations.");
			Assert.That(level.Lights.Where(item => fuseIndicatorIds.Contains(item.Point.Name))
				.All(item => item.InitiallyOn && !item.Interactive && item.InitialColor == "#FF0000" && item.ActivatedColor == "#00FF00"
					&& item.VisualFile.EndsWith("fer-indicator-off.png") && item.ActivatedVisualFile.EndsWith("fer-indicator-on.png")),
				Is.True, "Fuse indicators must preserve the original red-to-green light and sprite transition.");
			Assert.That(level.CoreStageObjects.Any(item => fuseIndicatorIds.Contains(item.Point.Name)), Is.False,
				"FER's fuse indicators must no longer require inherited lamp objects.");
			Assert.That(level.Decorations.Any(item => item.Name.StartsWith("lightbulb")), Is.False);
			string[] proximityLightIds =
			{
				"int-pllight", "int-pllight-1", "int-pllight-2", "int-pllight-3",
				"int-pllight-5", "int-pllight-6", "int-pllightgewehr"
			};
			Assert.That(level.ProximityLights, Has.Count.EqualTo(7));
			Assert.That(level.ProximityLights.Single(item => item.Point.Name == "int-pllightgewehr").LightRotationZ,
				Is.EqualTo(-180f).Within(0.01f),
				"The gun-room light must preserve its inverted source shape.");
			Assert.That(level.ProximityLights.Where(item => proximityLightIds.Contains(item.Point.Name))
				.Select(item => item.Point.Name), Is.EquivalentTo(proximityLightIds),
				"All seven actor-proximity lights must be portable Tiled light fixtures.");
			Assert.That(level.ProximityLights.Where(item => proximityLightIds.Contains(item.Point.Name))
				.All(item => !item.InitiallyActive && item.FilePath.EndsWith("fer-proximity-light.png")
					&& item.FadeSeconds == 1.5f), Is.True,
				"FER's proximity fixtures must preserve their dormant start and portable sprite/fade behavior.");
			Assert.That(level.ProximityLights.All(item => File.Exists(Path.Combine(pack.RootPath,
				item.FilePath.Replace('/', Path.DirectorySeparatorChar)))), Is.True);
			Assert.That(level.ProximityLights.Where(item => item.Point.Name != "int-pllightgewehr")
				.All(item => item.LightType == "point" && item.DetectionRadius == 12f), Is.True);
			Assert.That(level.ProximityLights.Single(item => item.Point.Name == "int-pllightgewehr").LightType,
				Is.EqualTo("freeform"));
			Assert.That(level.ProximityLights.Single(item => item.Point.Name == "int-pllightgewehr").DetectionRadius,
				Is.EqualTo(16f));
			Assert.That(level.CoreStageObjects.Any(item => proximityLightIds.Contains(item.Point.Name)), Is.False,
				"Portable proximity lights must not retain inherited LightBulbIllumination objects.");
			string[] staticLightIds = { "freeform-light-2d-1", "freeform-light-2d", "light-visitorarea", "freeform-light-2d-2" };
			Assert.That(level.FreeformLights.Select(item => item.Point.Name),
				Is.EquivalentTo(staticLightIds.Concat(new[] { "int-pllight-7" })),
				"FER's static area, theatre, and dormant lab lights must be portable freeform shapes.");
			Assert.That(level.FreeformLights.Where(item => staticLightIds.Contains(item.Point.Name))
				.All(item => item.InitiallyActive && item.ShapePath.Length >= 10
				&& item.SortingLayers.Length >= 2), Is.True);
			Assert.That(level.CoreStageObjects.Any(item => staticLightIds.Contains(item.Point.Name)
				|| item.Point.Name == "int-pllight-7"), Is.False);
			TiledFreeformLightDefinition labLight = level.FreeformLights.Single(item => item.Point.Name == "int-pllight-7");
			Assert.That(labLight.InitiallyActive, Is.False);
			Assert.That(labLight.VisualFile, Does.EndWith("fer-proximity-light.png"));
			Assert.That(labLight.LightRotationZ, Is.EqualTo(180f));
			Assert.That(level.FreeformLights.Single(item => item.Point.Name == "freeform-light-2d-2").SuppressInheritedPath,
				Is.EqualTo("Decoration[2]/Theatre[6]/dc_stageLights[1]/Freeform Light 2D[0]"));
			Assert.That(level.GlobalLights.Select(item => item.Point.Name), Is.EquivalentTo(new[] { "global-light-2d" }));
			Assert.That(level.GlobalLights[0].Intensity, Is.EqualTo(0.2f));
			Assert.That(level.GlobalLights[0].SortingLayers,
				Does.Contain("Sky"), "The JSON global light must cover FER's sky layer.");
			Assert.That(level.CoreStageObjects.Any(item => item.Point.Name == "global-light-2d"), Is.False,
				"Stage.GetLightGlobal must be rebound to the portable light, not the retained prefab light.");
			Assert.That(level.CoreStageObjects.Single(item => item.Point.Name == "int-lampjackyroom").Kind,
				Is.EqualTo("catalog-prop"));
			Assert.That(level.CoreStageObjects.Single(item => item.Point.Name == "int-lampjackyroom").InitiallyActive,
				Is.True, "The original flickering Jacky-room lamp starts active.");
			Assert.That(level.PointLights.Select(item => item.Point.Name),
				Is.EquivalentTo(new[] { "point-light-2d", "parametric-light-2d" }));
			TiledPointLightDefinition alarmLight = level.PointLights.Single(item => item.Point.Name == "point-light-2d");
			Assert.That(alarmLight.InitiallyActive, Is.False);
			Assert.That(alarmLight.PulseFrom, Is.EqualTo(0.5f));
			Assert.That(alarmLight.PulseTo, Is.EqualTo(1.5f));
			Assert.That(alarmLight.PulseSeconds, Is.EqualTo(1f));
			Assert.That(alarmLight.OverlapOperation, Is.EqualTo("alpha-blend"));
			Assert.That(level.PointLights.Single(item => item.Point.Name == "parametric-light-2d").InitiallyActive,
				Is.False);
			Assert.That(level.CoreStageObjects.Any(item => item.Point.Name == "point-light-2d"
				|| item.Point.Name == "parametric-light-2d"), Is.False);
			Assert.That(level.CoreStageObjects.Select(item => item.SourcePath).Distinct().Count(), Is.EqualTo(level.CoreStageObjects.Count));
			Assert.That(level.CoreStageObjects.All(item => item.SourcePath.StartsWith("core:stage-prop/fer/")), Is.True,
				"FER presentation objects must use stable Core prop IDs instead of hierarchy paths.");
			Assert.That(level.CoreStageObjects.Any(item => item.SourcePath.Contains("vendor")), Is.False,
				"Vendor child lights must remain attached to the cloned vendor, not be positioned separately by Tiled.");
			GameObject ferPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Stages/stage_fer.prefab");
			Assert.That(ferPrefab, Is.Not.Null, "The Core FER prefab is required to verify catalog prop targets.");
			System.Type vendorType = System.Type.GetType("Vendor, Assembly-CSharp");
			Assert.That(vendorType, Is.Not.Null);
			System.Type pickupType = System.Type.GetType("PickUpable, Assembly-CSharp");
			Assert.That(pickupType, Is.Not.Null);
			System.Type factoryType = System.Type.GetType("ExternalStageFactory, Assembly-CSharp");
			Assert.That(factoryType, Is.Not.Null);
			System.Reflection.MethodInfo normalizeTiledObjectId = factoryType.GetMethod("NormalizeTiledObjectId",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			Assert.That(normalizeTiledObjectId, Is.Not.Null);
			System.Type npcType = System.Type.GetType("NPC, Assembly-CSharp");
			Assert.That(npcType, Is.Not.Null);
			foreach (TiledScriptedActorDefinition actorDefinition in level.ScriptedActors)
			{
				Component exactActor = ferPrefab.GetComponentsInChildren(npcType, true).SingleOrDefault(candidate =>
					(string)normalizeTiledObjectId.Invoke(null, new object[] { candidate.gameObject.name })
					== actorDefinition.Point.Name);
				Assert.That(exactActor, Is.Not.Null,
					"Each FER scripted actor must reuse its exact dormant stage object: " + actorDefinition.Point.Name);
				Assert.That(exactActor.gameObject.activeSelf, Is.False);
				if (actorDefinition.Point.Name == "abbycrawler")
					Assert.That((string)npcType.GetMethod("GetName").Invoke(exactActor, null), Is.EqualTo("Abby Crawler"),
						"The basement sequence must use FER's unique crawler configuration, not a generic Abby clone.");
			}
			System.Reflection.MethodInfo resolveCoreProp = factoryType.GetMethod("ResolveCoreStageProp",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
			Assert.That(resolveCoreProp, Is.Not.Null);
			HashSet<Transform> retainedRoots = new HashSet<Transform>();
			foreach (TiledCoreStageObjectDefinition prop in level.CoreStageObjects)
			{
				string sourcePath = (string)resolveCoreProp.Invoke(null, new object[] { prop.SourcePath });
				Transform retainedRoot = ResolveIndexedTransformPath(ferPrefab.transform, sourcePath);
				Assert.That(retainedRoot, Is.Not.Null,
					"FER Core prop must resolve through the runtime catalog: " + prop.SourcePath);
				retainedRoots.Add(retainedRoot);
			}
			System.Type fuseType = System.Type.GetType("FuseBox, Assembly-CSharp");
			System.Type proximityType = System.Type.GetType("LightBulbIllumination, Assembly-CSharp");
			Assert.That(fuseType, Is.Not.Null);
			Assert.That(proximityType, Is.Not.Null);
			System.Reflection.FieldInfo fuseLightField = fuseType.GetField("m_lightLightBulb",
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			Assert.That(fuseLightField, Is.Not.Null);
			HashSet<Component> portableFuseLights = new HashSet<Component>();
			foreach (Component fuse in ferPrefab.GetComponentsInChildren(fuseType, true))
				portableFuseLights.Add((Component)fuseLightField.GetValue(fuse));
			HashSet<Transform> portablePointSources = new HashSet<Transform>();
			foreach (TiledPointLightDefinition point in level.PointLights)
				portablePointSources.Add(ResolveIndexedTransformPath(ferPrefab.transform, point.SuppressInheritedPath));
			foreach (Component light in ferPrefab.GetComponentsInChildren<Component>(true)
				.Where(component => component != null
					&& component.GetType().FullName == "UnityEngine.Rendering.Universal.Light2D"))
			{
				if (light.GetComponentInParent(vendorType, true) != null)
					continue;
				Component pickup = light.GetComponentInParent(pickupType, true);
				if (pickup != null)
				{
					string portableItemId = (string)normalizeTiledObjectId.Invoke(null,
						new object[] { pickup.gameObject.name });
					if (level.StageItems.Any(item => item.Point.Name == portableItemId))
						continue;
				}
				if (portableFuseLights.Contains(light) || portablePointSources.Contains(light.transform)
					|| light.GetComponentInParent(proximityType, true) != null)
					continue;
				string parentName = light.transform.parent == null ? string.Empty : light.transform.parent.name;
				if (parentName == "int_pLLight (7)"
					|| (parentName == "AreaLights" && (light.name == "Global Light 2D"
					|| light.name == "light_visitorArea" || light.name.StartsWith("Freeform Light 2D")))
					|| (parentName == "dc_stageLights" && light.name == "Freeform Light 2D"))
					continue;
				bool retained = false;
				for (Transform current = light.transform; current != null && current != ferPrefab.transform;
					current = current.parent)
					if (retainedRoots.Contains(current)) { retained = true; break; }
				Assert.That(retained, Is.True, "Every FER 2D light must be covered by a positioned retained root: "
					+ light.transform.name);
			}
			GameObject editableFer = UnityEditor.PrefabUtility.LoadPrefabContents("Assets/Stages/stage_fer.prefab");
			try
			{
				Component[] vendorLights = editableFer.GetComponentsInChildren<Component>(true)
					.Where(component => component != null
						&& component.GetType().FullName == "UnityEngine.Rendering.Universal.Light2D"
						&& component.GetComponentInParent(vendorType, true) != null).ToArray();
				Assert.That(vendorLights, Has.Length.EqualTo(7),
					"Both FER vendors must retain their seven original attached lights.");
				Vector3[] localPositions = vendorLights.Select(light => light.transform.localPosition).ToArray();
				System.Type stageType = System.Type.GetType("Stage, Assembly-CSharp");
				Assert.That(stageType, Is.Not.Null);
				Component sourceStage = editableFer.GetComponent(stageType);
				Component waypoint = (Component)stageType.GetMethod("GetWaypointStart").Invoke(sourceStage, null);
				Vector2 shiftedSpawn = waypoint.transform.position + new Vector3(5f, 0f, 0f);
				factoryType.GetMethod("AlignInheritedStageLights",
					System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
					.Invoke(null, new object[] { sourceStage, shiftedSpawn, stage });
				for (int index = 0; index < vendorLights.Length; index++)
					Assert.That(vendorLights[index].transform.localPosition, Is.EqualTo(localPositions[index]),
						"Vendor light offsets must not change before the complete vendor is cloned.");
				Component originalGlobal = (Component)stageType.GetMethod("GetLightGlobal").Invoke(sourceStage, null);
				Assert.That(originalGlobal, Is.Not.Null);
				GameObject testLayout = new GameObject("test-portable-global-light-layout");
				testLayout.transform.SetParent(editableFer.transform, false);
				factoryType.GetMethod("BuildTiledGlobalLight",
					System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
					.Invoke(null, new object[] { sourceStage, testLayout.transform, stage });
				Component portableGlobal = (Component)stageType.GetMethod("GetLightGlobal").Invoke(sourceStage, null);
				Assert.That(portableGlobal, Is.Not.SameAs(originalGlobal),
					"The stage API must reference the JSON-created global light.");
				Assert.That(originalGlobal.gameObject.activeSelf, Is.False,
					"The inherited global light must be disabled to prevent double lighting.");
			}
			finally { UnityEditor.PrefabUtility.UnloadPrefabContents(editableFer); }
			Assert.That(level.CoreStageObjects.Count(item => item.Point.Name.StartsWith("particle-system")), Is.EqualTo(2),
				"Both original FER atmospheric particle systems must be retained and positioned by Tiled.");
			string[] atticAndroidIds = { "android-1", "android-2", "android-3", "android-4",
				"android-5", "android-6", "android-7", "android-8" };
			Assert.That(level.CoreStageObjects.Count(item => atticAndroidIds.Contains(item.Point.Name)), Is.EqualTo(8),
				"All eight articulated attic mannequins must remain visible.");
			string[] labClosetAndroidIds = { "android", "android-1-2", "android-2-2", "android-3-2", "android-4-2" };
			Assert.That(level.CoreStageObjects.Count(item => labClosetAndroidIds.Contains(item.Point.Name)), Is.EqualTo(5),
				"All five articulated lab-closet mannequins must remain visible.");
			Assert.That(level.CoreStageObjects.Count(item => item.Point.Name == "android-9"), Is.EqualTo(1));
			Assert.That(level.CoreStageObjects.Count(item => item.Point.Name.StartsWith("dc-deadbody")
				|| item.Point.Name == "dc-hangingbody"), Is.EqualTo(4),
				"All four articulated basement body props must remain visible.");
			Assert.That(level.CoreStageObjects.Count(item => item.Point.Name.StartsWith("dc-jackyhanging")), Is.EqualTo(2),
				"Both hanging Jacky props must remain visible.");
			Assert.That(level.CoreStageObjects.Any(item => item.SourcePath.Contains("audio")), Is.False,
				"FER's positional ambience must no longer rely on inherited AudioSource objects.");
			TiledAudioDefinition[] positionalAudio = level.AudioSources
				.Where(item => item.Point.Name.StartsWith("as-", System.StringComparison.Ordinal)).ToArray();
			Assert.That(positionalAudio.Select(item => item.Point.Name), Is.EquivalentTo(new[]
				{ "as-abbyarea", "as-dripping", "as-dripping-2", "as-dripping-3" }));
			Assert.That(positionalAudio.All(item => !item.Ambient && item.Loop && item.PlayOnStart
				&& item.MixerGroup == "Ambience" && File.Exists(Path.Combine(pack.RootPath,
				item.FilePath.Replace('/', Path.DirectorySeparatorChar)))), Is.True,
				"All four FER positional ambience clips must be pack-local and keep their original mixer routing.");
			Assert.That(level.CoreStageObjects.Count(item => item.Point.Name == "dc-brainmachine"), Is.EqualTo(1),
				"The animated brain machine must be retained as one live hierarchy so its Animator and SpriteMask survive.");
			HashSet<string> portableDoorIds = new HashSet<string>(level.Doors.Select(item => item.Point.Name));
			Assert.That(stage.Spawners.SelectMany(item => item.RequiredOpenDoors).All(portableDoorIds.Contains), Is.True,
				"Every FER locked-room spawner must reference a portable door.");
			Assert.That(stage.Spawners.Single(item => item.Id == "fer-spawner-0").RequiredOpenDoors,
				Is.EqualTo(new[] { "int-doorhalls-2" }));
			Assert.That(stage.Spawners.Single(item => item.Id == "fer-spawner-2").RequiredOpenDoors,
				Is.EqualTo(new[] { "int-doorhalls-2" }));
			Assert.That(stage.Spawners.Single(item => item.Id == "fer-spawner-1").RequiredOpenDoors,
				Is.EqualTo(new[] { "int-doorhalls" }));
			Assert.That(stage.Spawners.Single(item => item.Id == "fer-spawner-3").RequiredOpenDoors,
				Is.EqualTo(new[] { "int-doorhalls" }));
			Assert.That(level.CoreStageObjects.Any(item => item.Point.Name.StartsWith("int-weaponcase")), Is.False,
				"FER weapon cases must no longer rely on its inherited hierarchy.");
			Assert.That(level.WeaponCases.Select(item => item.Point.Name), Is.EquivalentTo(new[]
			{
				"int-weaponcase", "int-weaponcase-1", "int-weaponcase-2", "int-weaponcase-3",
				"int-weaponcase-4", "int-weaponcase-5", "int-weaponcase-6", "int-weaponcase-7"
			}));
			Assert.That(level.WeaponCases.ToDictionary(item => item.Point.Name, item => item.Weapon.ToString()),
				Is.EquivalentTo(new Dictionary<string, string>
				{
					{ "int-weaponcase", "core:item/weapon/snub-revolver-32" },
					{ "int-weaponcase-1", "core:item/weapon/harrington-model-1892" },
					{ "int-weaponcase-2", "core:item/weapon/muger-p08" },
					{ "int-weaponcase-3", "core:item/weapon/usi" },
					{ "int-weaponcase-4", "core:item/weapon/np-40" },
					{ "int-weaponcase-5", "core:item/weapon/ronigsberg-543" },
					{ "int-weaponcase-6", "core:item/weapon/sawed-off" },
					{ "int-weaponcase-7", "core:item/weapon/revolver-44" }
				}), "FER's Main.unity weapon-case overrides must survive the export.");
			Assert.That(level.ScriptedActors.Select(item => item.Point.Name),
				Is.EquivalentTo(new[] { "abbycrawler", "npc-android", "npc-jacky-1" }));
			Assert.That(level.ScriptedActors.All(item => !item.InitiallyActive), Is.True,
				"FER's three scripted actors must remain dormant until their original encounter activates them.");
			Assert.That(level.CoreStageObjects.Any(item => item.Kind == "item"), Is.False,
				"FER's loose items must no longer rely on its inherited hierarchy.");
			Assert.That(level.Pickups.Single(item => item.Point.Name == "schockgewehr").Item,
				Is.EqualTo(ContentId.Parse("core:item/weapon/schockgewehr")),
				"The loose Schockgewehr must be a portable Core-content pickup.");
			Assert.That(level.Pickups.Single(item => item.Point.Name == "schockgewehr").InitiallyKinematic,
				Is.True, "The secret gun must stay on its pedestal until collected.");
			TiledStageItemDefinition keycard = level.StageItems.Single(item => item.Point.Name == "item-jackyroomkeycard");
			Assert.That(keycard.DisplayName, Is.EqualTo("Scientist Keycard"));
			Assert.That(keycard.OnPickupSignal, Is.EqualTo("fer-keycard-picked-up"));
			Assert.That(keycard.CanDrop, Is.False);
			Assert.That(keycard.InitiallyKinematic, Is.True,
				"The authored keycard must stay on its cabinet until pickup.");
			Assert.That(keycard.ColliderWidth, Is.EqualTo(0.5897217f).Within(0.00001f));
			Assert.That(keycard.ColliderHeight, Is.EqualTo(0.31194496f).Within(0.00001f));
			Assert.That(keycard.ColliderOffsetX, Is.EqualTo(0.015258789f).Within(0.00001f));
			Assert.That(File.Exists(Path.Combine(pack.RootPath,
				keycard.FilePath.Replace('/', Path.DirectorySeparatorChar))), Is.True);
			Assert.That(level.Platforms, Has.Count.EqualTo(36));
			Assert.That(level.Platforms.Count(item => !item.ClimbableLeft && !item.ClimbableRight), Is.EqualTo(23));
			Assert.That(level.Platforms.Count(item => !item.ClimbableLeft && item.ClimbableRight), Is.EqualTo(4));
			Assert.That(level.Platforms.Count(item => item.ClimbableLeft && !item.ClimbableRight), Is.EqualTo(6));
			Assert.That(level.Platforms.Count(item => item.ClimbableLeft && item.ClimbableRight), Is.EqualTo(3),
				"FER platform ledges must preserve the original prefab's asymmetric climbability.");
			Assert.That(level.NavNodes, Has.Count.GreaterThan(0), "FER's authored climb links must survive the Tiled export.");
			foreach (TiledNavNodeDefinition node in level.NavNodes)
				foreach (string target in node.Links)
				{
					TiledNavNodeDefinition reverse = level.NavNodes.Single(candidate => candidate.Point.Name == target);
					Assert.That(reverse.Links, Does.Contain(node.Point.Name), "FER climb/drop navigation must be bidirectional.");
				}
			Assert.That(level.EnemySpawners, Has.Count.EqualTo(23));
			Assert.That(level.Tilesets, Has.Count.GreaterThanOrEqualTo(118));
			Assert.That(level.Decorations, Has.Count.GreaterThanOrEqualTo(300));
			Assert.That(level.Tilesets.All(item => item.ObjectAlignment == "center"), Is.True);
			Assert.That(level.Tilesets.Any(item => item.BorderLeft + item.BorderBottom + item.BorderRight + item.BorderTop > 0f), Is.True);
			Assert.That(level.Decorations.Any(item => item.Repeat && item.AdaptiveTiling), Is.True);
			Assert.That(level.Decorations.Any(item => item.SortingLayer == "Sky"), Is.True);
			Assert.That(level.Decorations.All(item => item.SortingOrder >= -10000 && item.SortingOrder <= 10000), Is.True);
			Assert.That(level.Decorations.All(item => item.Opacity >= 0f && item.Opacity <= 1f), Is.True);
			Assert.That(level.Decorations.All(item => !string.IsNullOrWhiteSpace(item.TintColor)), Is.True);
			Assert.That(level.Tilesets.All(tileset =>
				File.Exists(Path.Combine(pack.RootPath, tileset.ImagePath.Replace('/', Path.DirectorySeparatorChar)))), Is.True,
				"Every exported FER tileset must resolve to a pack-local PNG.");
			StageScriptDefinition statues = content.StageScripts.Single(item => item.Id.Path == "stage-script/fer-statues");
			Assert.That(statues.Sequences.SelectMany(item => item.Actions)
				.Where(item => !string.IsNullOrEmpty(item.DestinationId)).Select(item => item.DestinationId).Distinct(),
				Is.EquivalentTo(level.StageMarkers.Select(item => item.Name)),
				"Every statue script destination must resolve to an exported map marker.");
			StageScriptDefinition encounters = content.StageScripts.Single(item => item.Id.Path == "stage-script/fer-encounters");
			Assert.That(encounters.Sequences.Where(item => item.Id.StartsWith("linked-int-doorhalls"))
				.Select(item => item.Signal), Is.EquivalentTo(new[] { "int-doorhalls", "int-doorhalls-2" }),
				"FER's duplicate-named hallway doors must keep distinct authored interaction IDs.");
			Assert.That(encounters.Sequences.Single(item => item.Signal == "int-doorbasement").Actions
				.Any(item => item.Type == "spawn-actor" && item.ObjectId == "abbycrawler"), Is.True);
			StageScriptSequenceDocument atticFuse = encounters.Sequences.Single(item => item.Signal == "int-fuseboxattic");
			Assert.That(atticFuse.Actions.Any(item => item.Type == "spawn-actor" && item.ObjectId == "npc-android"), Is.True);
			Assert.That(atticFuse.Actions.Any(item => item.Type == "set-object-active"
				&& item.ObjectId == "android-6" && !item.Active), Is.True);
			Assert.That(encounters.Sequences.Single(item => item.Signal == "fer-keycard-picked-up").Actions
				.Any(item => item.Type == "spawn-actor" && item.ObjectId == "npc-jacky-1"), Is.True,
				"Picking up the portable scientist keycard must spawn the original dormant Jacky actor.");
			Assert.That(encounters.Sequences.Single(item => item.Signal == "int-doorgewehr").Actions
				.Any(item => item.Type == "set-object-active" && item.ObjectId == "int-pllightgewehr" && item.Active), Is.True);
			StageScriptDefinition laboratory = content.StageScripts.Single(item => item.Id.Path == "stage-script/fer-laboratory");
			Assert.That(laboratory.Sequences.Where(item => item.Id.StartsWith("activate-int-fusebox"))
				.All(sequence => sequence.Actions.Any(action => action.Type == "play-audio"
					&& level.AudioSources.Any(audio => audio.Point.Name == action.ObjectId))), Is.True,
				"Each portable fuse interaction must play its pack-local activation sound.");
			StageScriptSequenceDocument laboratoryPower = laboratory.Sequences.Single(item => item.Id == "activate-laboratory");
			Assert.That(laboratoryPower.Actions.Any(item => item.Type == "play-audio"
				&& level.AudioSources.Any(audio => audio.Point.Name == item.ObjectId)), Is.True,
				"FER's laboratory cue must resolve to a named, pack-local audio point.");
			Assert.That(laboratoryPower.Actions.Any(item => item.Type == "set-object-active"
				&& item.ObjectId == "int-pllight-7" && item.Active), Is.True);
			Assert.That(laboratoryPower.Actions.Any(item => item.Type == "set-object-active"
				&& item.ObjectId == "point-light-2d" && item.Active), Is.True);
			StageScriptDefinition jackyCurse = content.StageScripts.Single(item => item.Id.Path == "stage-script/fer-jacky-curse");
			StageScriptSequenceDocument curseSequence = jackyCurse.Sequences.Single();
			Assert.That(curseSequence.ObjectId, Is.EqualTo("int-doorjackyroom"));
			Assert.That(curseSequence.State, Is.EqualTo("open"));
			Assert.That(curseSequence.Actions[0].Type, Is.EqualTo("wait"));
			Assert.That(curseSequence.Actions[0].Seconds, Is.EqualTo(15f));
			StageScriptActionDocument curseAction = curseSequence.Actions[1];
			Assert.That(curseAction.Type, Is.EqualTo("apply-player-status"));
			Assert.That(curseAction.Status, Is.EqualTo("jacky-curse"));
			Assert.That(curseAction.DurationSeconds, Is.EqualTo(99999f));
			Assert.That(curseAction.TicksPerSecond, Is.EqualTo(0.25f));
			Assert.That(curseAction.Chance, Is.EqualTo(0.175f));
			Assert.That(curseAction.ChanceIncrease, Is.EqualTo(0.05f));
			Assert.That(curseAction.MaxActive, Is.EqualTo(4));
		}

		[Test]
		public void TiledStarterKit_DiscoversModernClassObjectsAndMatchingEncounterPoints()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ModSDK/MapTemplates/TiledStage"));
			SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
			ModPack pack = new ModPack(new ModManifest
			{
				Id = "example.tiled-starter-kit",
				DisplayName = "Tiled Starter Kit",
				Version = "1.0.0",
				ModApiVersion = 1,
				ContentRoots = new List<string> { "content" }
			}, version, root);
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			Assert.That(content.Report.IsValid, Is.True);
			StageDefinition stage = content.Stages.Single();
			Assert.That(stage.UsesRuntimeTemplate, Is.True,
				"The starter kit must remain a canary for the stage-independent runtime shell.");
			Assert.That(stage.Extends, Is.EqualTo(ContentId.Parse("core:stage/mod-template")));
			Assert.That(stage.Layout.PreserveInheritedStageObjects, Is.False);
			Assert.That(stage.Layout.ReuseInheritedSpawners, Is.False);
			Assert.That(stage.Layout.TiledLevel.CoreStageObjects, Is.Empty,
				"The independent starter must not quietly restore hierarchy objects from a playable Core stage.");
			Assert.That(stage.Layout.TiledLevel.Platforms, Has.Count.EqualTo(4));
			Assert.That(stage.Layout.HideInheritedVisuals, Is.True);
			Assert.That(stage.Layout.TiledLevel.Tilesets, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.TileLayers, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.ImageLayers, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.ImageLayers[0].ImagePath, Is.EqualTo("assets/space-bay-background.png"));
			Assert.That(stage.Layout.TiledLevel.ImageLayers[0].RepeatX, Is.True);
			Assert.That(stage.Layout.TiledLevel.ImageLayers[0].ParallaxX, Is.EqualTo(0.2f));
			Assert.That(stage.Layout.TiledLevel.ImageLayers[0].ParallaxY, Is.EqualTo(0.35f));
			Assert.That(stage.Camera.Bounds.MaxX, Is.EqualTo(128f));
			Assert.That(stage.Layout.TiledLevel.TileLayers.Sum(layer => layer.Data.Count(gid => gid != 0)), Is.EqualTo(68));
			Assert.That(stage.Layout.TiledLevel.WidthPixels, Is.EqualTo(8192));
			Assert.That(stage.Layout.TiledLevel.HeightPixels, Is.EqualTo(3072));
			Assert.That(stage.Layout.TiledLevel.Platforms.Count(platform => platform.ClimbableLeft && platform.ClimbableRight), Is.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.Tilesets.All(tileset => !tileset.ImagePath.Contains("\\")), Is.True);
			Assert.That(stage.Layout.TiledLevel.Decorations, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.Decorations.Single(item => item.Name == "example-lamp").EncodedGid & 0x1FFFFFFF, Is.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.Decorations.Count(item => item.Repeat && item.SortingLayer == "Platform"), Is.EqualTo(2));
			Assert.That(stage.Layout.TiledLevel.WeaponVendors, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.UsableVendors, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.WeaponCases, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.WeaponCases[0].Weapon.ToString(), Is.EqualTo("core:item/weapon/revolver-44"));
			Assert.That(stage.Layout.TiledLevel.WeaponCases[0].CaseSize, Is.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Doors, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.Doors.Select(door => door.DoorType),
				Is.EquivalentTo(new[] { "standard", "jacky", "roller" }));
			Assert.That(stage.Layout.TiledLevel.Doors.Single(door => door.DoorType == "roller").ProximityRadius,
				Is.EqualTo(8f));
			Assert.That(stage.Layout.TiledLevel.DoorSwitches, Has.Count.EqualTo(3));
			Assert.That(stage.Layout.TiledLevel.DoorSwitches.SelectMany(doorSwitch => doorSwitch.TargetDoorIds),
				Is.EquivalentTo(new[] { "standard-door", "jacky-door", "roller-door" }));
			Assert.That(stage.Layout.TiledLevel.CoreArt, Has.Count.EqualTo(4));
			Assert.That(stage.Layout.TiledLevel.CoreArt.Select(art => art.Art.Id),
				Is.EquivalentTo(new[] { "space-station-bay", "welcome-poster", "barrel", "chair" }));
			Assert.That(stage.Layout.TiledLevel.CoreArt.Single(art => art.Art.Id == "space-station-bay").SortingOrder,
				Is.EqualTo(10));
			Assert.That(stage.Layout.TiledLevel.Notes, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Notes[0].FontSize, Is.EqualTo(32));
			Assert.That(stage.Layout.TiledLevel.Keypads, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Keypads[0].Code, Is.EqualTo("1234"));
			Assert.That(stage.Layout.TiledLevel.Keypads[0].TargetDoorIds, Is.EqualTo(new[] { "standard-door" }));
			Assert.That(stage.Layout.TiledLevel.Altars, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Interactions, Has.Count.EqualTo(1));
			TiledInteractionDefinition paidButton = stage.Layout.TiledLevel.Interactions.Single(item => item.Trigger == "use");
			Assert.That(paidButton.Price, Is.EqualTo(25));
			Assert.That(paidButton.SingleUse, Is.True);
			Assert.That(paidButton.DoorAction, Is.EqualTo("open"));
			Assert.That(paidButton.TargetDoorIds, Is.EqualTo(new[] { "jacky-door" }));
			Assert.That(paidButton.TargetLightIds, Is.EqualTo(new[] { "flickering-light" }));
			Assert.That(paidButton.TargetSpawnerIds, Is.EqualTo(new[] { "west-ground" }));
			Assert.That(paidButton.SpawnCount, Is.EqualTo(2));
			Assert.That(paidButton.MinWave, Is.EqualTo(1));
			Assert.That(paidButton.DelaySeconds, Is.EqualTo(0.25f));
			Assert.That(paidButton.RequiredItem, Is.EqualTo(ContentId.Parse("core:item/weapon/revolver-44")));
			Assert.That(paidButton.ConsumeRequiredItem, Is.False);
			Assert.That(paidButton.GiveItem, Is.EqualTo(ContentId.Parse("core:item/consumable/ammo-box")));
			Assert.That(paidButton.GiveItemAmount, Is.EqualTo(1));
			Assert.That(paidButton.MaxEnemies, Is.EqualTo(20));
			Assert.That(paidButton.VisualArt.Id, Is.EqualTo("fer-brain-machine"));
			Assert.That(paidButton.VisualScale, Is.EqualTo(0.75f));
			Assert.That(stage.Layout.TiledLevel.Lights, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Lights[0].InitiallyOn, Is.True);
			Assert.That(stage.Layout.TiledLevel.Lights[0].Flicker, Is.EqualTo(0.15f));
			Assert.That(stage.Layout.TiledLevel.Pickups, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Pickups[0].Item.ToString(), Is.EqualTo("core:item/weapon/revolver-44"));
			Assert.That(stage.Layout.TiledLevel.MovingPlatforms, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.MovingPlatforms[0].OffsetY, Is.EqualTo(-256f));
			Assert.That(stage.Layout.TiledLevel.Particles, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.Particles[0].Color, Is.EqualTo("#66CCFFFF"));
			Assert.That(stage.Layout.TiledLevel.NavNodes, Has.Count.EqualTo(2));
			Assert.That(stage.Layout.TiledLevel.NavNodes.SelectMany(node => node.Links),
				Is.EquivalentTo(new[] { "manual-nav-left", "manual-nav-right" }));
			Assert.That(stage.Layout.TiledLevel.AudioSources, Has.Count.EqualTo(2));
			Assert.That(stage.Layout.TiledLevel.AudioSources.Count(audio => audio.Ambient), Is.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.AudioSources.All(audio => audio.Loop), Is.True);
			Assert.That(stage.Layout.TiledLevel.AudioSources.All(audio => audio.PlayOnStart), Is.True);
			StageScriptDefinition machineScript = content.StageScripts.Single(script =>
				script.Id == ContentId.Parse("example.tiled-starter-kit:stage-script/touch-box-demo"));
			Assert.That(machineScript.Machines, Has.Count.EqualTo(1));
			Assert.That(machineScript.Machines.Single().InitialState, Is.EqualTo("off"));
			Assert.That(machineScript.Machines.Single().States.Select(state => state.Id),
				Is.EquivalentTo(new[] { "off", "active" }));
			Assert.That(stage.Layout.TiledLevel.ScriptTriggers.Select(trigger => trigger.Name),
				Is.EqualTo(new[] { "script-volume-demo" }));
			Assert.That(stage.Layout.TiledLevel.Tilesets.Single(tileset => tileset.Name == "Starter Lamp").Animations, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.EnemySpawners.Select(point => point.Name),
				Is.EquivalentTo(new[] { "west-ground", "east-ground" }));
			StagePointDefinition player = stage.Layout.TiledLevel.ToUnityPoint(
				stage.Layout.TiledLevel.PlayerSpawn, stage.Layout.PixelsPerUnit);
			Assert.That(player.X, Is.EqualTo(0));
			Assert.That(player.Y, Is.EqualTo(6));
		}

		[Test]
		public void TiledStressTest_UsesNeutralShellWithoutLosingItsEncounter()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods/tiled-stress-arena"));
			SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
			ModPack pack = new ModPack(new ModManifest
			{
				Id = "example.tiled-stress-arena",
				DisplayName = "Tiled Stress-Test Arena",
				Version = "1.0.0",
				ModApiVersion = 1,
				ContentRoots = new List<string> { "content" }
			}, version, root);
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			Assert.That(content.Report.IsValid, Is.True);
			StageDefinition stage = content.Stages.Single();
			Assert.That(stage.UsesRuntimeTemplate, Is.True);
			Assert.That(stage.Spawners, Has.Count.EqualTo(15));
			Assert.That(stage.Waves.FirstWaveEnemyCount, Is.EqualTo(60));
			Assert.That(stage.Layout.TiledLevel.CoreStageObjects, Is.Empty);
			Assert.That(stage.Layout.TiledLevel.MovingPlatforms, Has.Count.EqualTo(1));
			Assert.That(stage.Layout.TiledLevel.TileLayers.All(layer => layer.Name != "elevator platform"), Is.True,
				"The authored moving platform renders its own art; a static tile layer would duplicate it.");
			Assert.That(stage.Layout.TiledLevel.MovingPlatforms[0].Rectangle.ClimbableLeft, Is.True);
			Assert.That(stage.Layout.TiledLevel.MovingPlatforms[0].Rectangle.ClimbableRight, Is.True);
		}

		[Test]
		public void TiledNeutralStarter_UsesPackLocalVendorSprites()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath,
				"../ModSDK/MapTemplates/TiledStage"));
			SemanticVersion.TryParse("1.1.0", out SemanticVersion version);
			ModPack pack = new ModPack(new ModManifest
			{
				Id = "example.tiled-neutral-starter",
				DisplayName = "Tiled Neutral Starter",
				Version = "1.1.0",
				ModApiVersion = 1,
				ContentRoots = new List<string> { "content" }
			}, version, root);
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { pack });
			Assert.That(content.Report.IsValid, Is.True);
			TiledLevelDefinition level = content.Stages.Single().Layout.TiledLevel;
			Assert.That(level.WeaponVendors.Single().VisualFile, Is.EqualTo("assets/skins/weapon-vendor-dark.png"));
			Assert.That(level.UsableVendors.Single().VisualFile, Is.EqualTo("assets/skins/usable-vendor-dark.png"));
			Assert.That(level.MovingPlatforms.Single().Rectangle.ClimbableLeft, Is.True);
			Assert.That(level.MovingPlatforms.Single().Rectangle.ClimbableRight, Is.True);
		}

		[Test]
		public void ModStoragePaths_KeepWindowsFolderAndUsePersistentStorageOnMobileAndWebGl()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string dataPath = Path.Combine(root, "Game_Data");
			string persistent = Path.Combine(root, "UserData");
			Assert.That(ModStoragePaths.GetModsDirectory(RuntimePlatform.WindowsPlayer, dataPath, persistent),
				Is.EqualTo(Path.Combine(root, "Mods")));
			Assert.That(ModStoragePaths.GetModsDirectory(RuntimePlatform.Android, dataPath, persistent),
				Is.EqualTo(Path.Combine(persistent, "Mods")));
			Assert.That(ModStoragePaths.GetModsDirectory(RuntimePlatform.WebGLPlayer, dataPath, persistent),
				Is.EqualTo(Path.Combine(persistent, "Mods")));
			Assert.That(ModStoragePaths.GetModsDirectory(RuntimePlatform.Android, dataPath, null), Is.Null);
		}

		[Test]
		public void SharedStageTemplateAssets_AreHiddenAndFullyPopulated()
		{
			GameObject shell = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/Resources/Modding/StageTemplates/stage_mod_shell.prefab");
			GameObject objects = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/Resources/Modding/StageTemplates/stage_mod_objects.prefab");
			Assert.That(shell, Is.Not.Null);
			Assert.That(objects, Is.Not.Null);
			Assert.That(shell.activeSelf, Is.False);
			Assert.That(objects.activeSelf, Is.False);

			System.Type stageType = System.Type.GetType("Stage, Assembly-CSharp");
			System.Type libraryType = System.Type.GetType("ModStageTemplateLibrary, Assembly-CSharp");
			Assert.That(stageType, Is.Not.Null);
			Assert.That(libraryType, Is.Not.Null);
			Component stage = shell.GetComponent(stageType);
			Component library = objects.GetComponent(libraryType);
			Assert.That(stage, Is.Not.Null);
			Assert.That(library, Is.Not.Null);
			object[] validationArguments = { null };
			bool missingTemplate = (bool)libraryType.GetMethod("TryGetMissingTemplate").Invoke(library, validationArguments);
			Assert.That(missingTemplate, Is.False, validationArguments[0] as string);

			UnityEditor.SerializedObject stageData = new UnityEditor.SerializedObject(stage);
			Assert.That(stageData.FindProperty("m_id").intValue, Is.EqualTo(-1));
			Assert.That(stageData.FindProperty("m_isRuntimeTemplate").boolValue, Is.True);
			foreach (string field in new[] { "m_waypointStart", "m_lightGlobal", "m_actorsParent", "m_itemsParent" })
				Assert.That(stageData.FindProperty(field).objectReferenceValue, Is.Not.Null, field);

			foreach (string property in new[]
			{
				"StageShell", "WeaponCase", "WeaponVendor", "UsableVendor", "StandardDoor", "RollerDoor",
				"JackyDoor", "Switch", "FuseBox", "LightBulb", "Note", "Keypad", "Altar", "Activator", "FireHazard"
			})
			{
				System.Reflection.PropertyInfo info = libraryType.GetProperty(property);
				Assert.That(info, Is.Not.Null, property);
				Assert.That(info.GetValue(library), Is.Not.Null, property);
			}
			foreach (Transform child in objects.transform)
				Assert.That(child.gameObject.activeSelf, Is.False, child.name);
			Component usableVendor = (Component)libraryType.GetProperty("UsableVendor").GetValue(library);
			SpriteRenderer vendorRenderer = usableVendor.GetComponent<SpriteRenderer>();
			Assert.That(vendorRenderer, Is.Not.Null);
			Assert.That(vendorRenderer.sprite, Is.Not.Null,
				"The shared usable vendor needs Core artwork for the public perk-machine sprite slot.");
			Assert.That(vendorRenderer.sprite.name, Is.EqualTo("UsableVendor"));
		}

		[Test]
		public void SharedLightTemplate_PortablePresentationDoesNotTileOrRetainCoreFixtureArt()
		{
			GameObject objects = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
				"Assets/Resources/Modding/StageTemplates/stage_mod_objects.prefab");
			GameObject clone = Object.Instantiate(objects);
			Texture2D texture = new Texture2D(32, 32);
			Sprite indicator = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
			try
			{
				System.Type libraryType = System.Type.GetType("ModStageTemplateLibrary, Assembly-CSharp");
				Component library = clone.GetComponent(libraryType);
				Component light = (Component)libraryType.GetProperty("LightBulb").GetValue(library);
				light.GetType().GetMethod("ConfigureModPresentation").Invoke(light, new object[]
				{
					indicator, indicator, Color.red, Color.green, 0.19f, 0.59f, 0.68f, "Decoration", 0
				});
				SpriteRenderer root = light.GetComponent<SpriteRenderer>();
				Assert.That(root.drawMode, Is.EqualTo(SpriteDrawMode.Simple));
				Assert.That(root.sprite, Is.SameAs(indicator));
				Assert.That(light.GetComponentsInChildren<SpriteRenderer>(true)
					.Where(renderer => renderer != root).All(renderer => !renderer.enabled), Is.True);
			}
			finally
			{
				Object.DestroyImmediate(indicator);
				Object.DestroyImmediate(texture);
				Object.DestroyImmediate(clone);
			}
		}

		[Test]
		public void ConvertedFemboyExample_RefitsExistingCoreClothingWithoutAddingDuplicates()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack femboy = packs.Packs.Single(pack => pack.Manifest.Id == "mousai.femboy-refitted-shirt");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { femboy });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Clothing, Is.Empty,
				"The original Femboy edition refitted Core clothing; it did not add another default shirt.");
			ContentId[] expectedClothingTargets =
			{
				ContentId.Parse("core:clothing/bikini-jewelry"),
				ContentId.Parse("core:clothing/bikini-marine"),
				ContentId.Parse("core:clothing/bikini-orange"),
				ContentId.Parse("core:clothing/bikini-pink"),
				ContentId.Parse("core:clothing/bikini-white"),
				ContentId.Parse("core:clothing/black-top"),
				ContentId.Parse("core:clothing/dark-purple-top"),
				ContentId.Parse("core:clothing/dress-ada"),
				ContentId.Parse("core:clothing/hazmat-suit"),
				ContentId.Parse("core:clothing/knight-breast-plate"),
				ContentId.Parse("core:clothing/lingerie-black-upper"),
				ContentId.Parse("core:clothing/lingerie-white-upper"),
				ContentId.Parse("core:clothing/playboy-bunny-body"),
				ContentId.Parse("core:clothing/scientist-set"),
				ContentId.Parse("core:clothing/security-shirt"),
				ContentId.Parse("core:clothing/shirt-1"),
				ContentId.Parse("core:clothing/shirt-2"),
				ContentId.Parse("core:clothing/shirt-3"),
				ContentId.Parse("core:clothing/shirt-damaged"),
				ContentId.Parse("core:clothing/shirt-default"),
				ContentId.Parse("core:clothing/shirt-jill"),
				ContentId.Parse("core:clothing/shirt-top-blue"),
				ContentId.Parse("core:clothing/shirt-top-orange"),
				ContentId.Parse("core:clothing/sweater-1"),
				ContentId.Parse("core:clothing/sweater-2"),
				ContentId.Parse("core:clothing/white-top")
			};
			Assert.That(content.AssetPatches.Where(patch => patch.Target.Path.StartsWith("clothing/"))
				.Select(patch => patch.Target), Is.EquivalentTo(expectedClothingTargets));

			AssetPatchDefinition body = content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:player"));
			Assert.That(body.Replacements, Has.Count.EqualTo(16),
				"Torso, chest, neck, and anatomy-bearing butt need all four Core skin palettes.");
			Assert.That(content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:sprite/butt-5")).Replacements, Has.Count.EqualTo(1));
			Assert.That(content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:sprite/torso-lower-0")).Replacements, Has.Count.EqualTo(1));
			AssetPatchDefinition defaultShirt = content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:clothing/shirt-default"));
			Assert.That(defaultShirt.Replacements.Select(replacement => replacement.SlotId), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:clothing/shirt-default/icon"),
				ContentId.Parse("core:clothing/shirt-default/piece/shirt-spine"),
				ContentId.Parse("core:clothing/shirt-default/piece/shirt-chest")
			}));
			Assert.That(content.AssetPatches, Has.Count.EqualTo(29));
			Assert.That(content.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(82));
			foreach (AssetPatchDefinition patch in content.AssetPatches)
				foreach (AssetReplacementDefinition replacement in patch.Replacements)
					Assert.That(File.Exists(Path.Combine(femboy.RootPath, replacement.AssetPath)), Is.True,
						replacement.AssetPath);
		}

		[Test]
		public void ConvertedSmallerBodyExamples_PreserveEveryAuthoredSpriteWithoutCompressionNoise()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);

			ModPack breast = packs.Packs.Single(pack => pack.Manifest.Id == "legacy.smaller-breast");
			ModContentDiscoveryResult breastContent = ModContentDiscovery.Discover(new[] { breast });
			Assert.That(breastContent.Report.IsValid, Is.True);
			Assert.That(breastContent.Clothing, Is.Empty);
			Assert.That(breastContent.AssetPatches, Has.Count.EqualTo(30));
			Assert.That(breastContent.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(66));
			Assert.That(breastContent.AssetPatches.Count(patch => patch.Target.Path.StartsWith("clothing/")),
				Is.EqualTo(26));
			Assert.That(breastContent.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:player")).Replacements, Has.Count.EqualTo(4));
			foreach (AssetPatchDefinition patch in breastContent.AssetPatches)
				foreach (AssetReplacementDefinition replacement in patch.Replacements)
					Assert.That(File.Exists(Path.Combine(breast.RootPath, replacement.AssetPath)), Is.True,
						replacement.AssetPath);

			ModPack combined = packs.Packs.Single(pack => pack.Manifest.Id == "legacy.smaller-breast-and-butt");
			Assert.That(combined.Manifest.Dependencies.Any(dependency =>
				dependency.Id == "legacy.smaller-breast"), Is.True);
			ModContentDiscoveryResult combinedContent = ModContentDiscovery.Discover(new[] { combined });
			Assert.That(combinedContent.Report.IsValid, Is.True);
			Assert.That(combinedContent.Clothing, Is.Empty);
			Assert.That(combinedContent.AssetPatches, Has.Count.EqualTo(15));
			Assert.That(combinedContent.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(18));
			Assert.That(combinedContent.AssetPatches.Count(patch => patch.Target.Path.StartsWith("clothing/")),
				Is.EqualTo(11));
			foreach (AssetPatchDefinition patch in combinedContent.AssetPatches)
				foreach (AssetReplacementDefinition replacement in patch.Replacements)
					Assert.That(File.Exists(Path.Combine(combined.RootPath, replacement.AssetPath)), Is.True,
						replacement.AssetPath);
		}

		[Test]
		public void ConvertedPreyBunnyGirlsExample_PreservesAllAuthoredVisualReplacements()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack converted = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "draco66electro.prey-bunny-girls");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { converted });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Enemies, Is.Empty,
				"The legacy release changed existing artwork; it did not add the Green Stalker demo.");
			Assert.That(content.Clothing, Is.Empty,
				"The overhaul reskins the existing wardrobe instead of adding duplicate garments.");
			Assert.That(content.AssetPatches, Has.Count.EqualTo(90));
			Assert.That(content.AssetPatches.Count(patch => patch.Target.Path.StartsWith("clothing/")),
				Is.EqualTo(52));
			Assert.That(content.AssetPatches.Count(patch => patch.Target.Path.StartsWith("enemy/")),
				Is.EqualTo(4));
			Assert.That(content.AssetPatches.Where(patch => patch.Target.Path.StartsWith("enemy/"))
				.Select(patch => patch.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:enemy/zombie-1"),
				ContentId.Parse("core:enemy/zombie-2"),
				ContentId.Parse("core:enemy/zombie-3"),
				ContentId.Parse("core:enemy/zombie-grabber")
			}));
			Assert.That(content.AssetPatches.Count(patch => patch.Target.Path.StartsWith("sprite/")),
				Is.EqualTo(33));
			Assert.That(content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:player")).Replacements, Has.Count.EqualTo(56));

			AssetReplacementDefinition[] replacements = content.AssetPatches
				.SelectMany(patch => patch.Replacements).ToArray();
			Assert.That(replacements, Has.Length.EqualTo(197));
			string[] authoredPngs = replacements.Select(replacement => replacement.AssetPath)
				.Distinct().ToArray();
			Assert.That(authoredPngs, Has.Length.EqualTo(154));
			Assert.That(authoredPngs.Any(path => path.StartsWith("assets/legacy/2182-")), Is.False);
			Assert.That(authoredPngs.Any(path => path.StartsWith("assets/legacy/2502-")), Is.False);
			Assert.That(authoredPngs.Any(path => path.StartsWith("assets/legacy/2912-")), Is.False);
			Assert.That(authoredPngs.Any(path => path.StartsWith("assets/legacy/2937-")), Is.False);
			foreach (string path in authoredPngs)
				Assert.That(File.Exists(Path.Combine(converted.RootPath, path)), Is.True, path);
		}

		[Test]
		public void ConvertedFutanariSmallTweaksExample_InheritsTweaksAndPreservesFiveUniqueSprites()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack converted = packs.Packs.Single(pack =>
				pack.Manifest.Id == "legacy.futanari-small-tweaks");
			Assert.That(converted.Manifest.Dependencies.Any(dependency =>
				dependency.Id == "legacy.small-tweaks"), Is.True);

			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { converted });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.AssetPatches, Has.Count.EqualTo(2));
			Assert.That(content.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(5));
			Assert.That(content.AssetPatches.Select(patch => patch.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:player"),
				ContentId.Parse("core:sprite/butt-5")
			}));
			AssetPatchDefinition player = content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:player"));
			Assert.That(player.Replacements.Select(replacement => replacement.SlotId), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:player/body/butt/pale"),
				ContentId.Parse("core:player/body/butt/white"),
				ContentId.Parse("core:player/body/butt/tan"),
				ContentId.Parse("core:player/body/butt/black")
			}));
			foreach (AssetPatchDefinition patch in content.AssetPatches)
				foreach (AssetReplacementDefinition replacement in patch.Replacements)
					Assert.That(File.Exists(Path.Combine(converted.RootPath, replacement.AssetPath)), Is.True,
						replacement.AssetPath);
		}

		[Test]
		public void ConvertedFutaZombiesExample_PatchesAllThreeCoreRigsWithFortySourceSprites()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack converted = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "legacy.futazombies");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { converted });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Enemies, Is.Empty,
				"The legacy archive replaced the three Core zombies; it did not add enemy variants.");
			Assert.That(content.AssetPatches.Select(patch => patch.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:enemy/zombie-1"),
				ContentId.Parse("core:enemy/zombie-2"),
				ContentId.Parse("core:enemy/zombie-3")
			}));
			Assert.That(content.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(42),
				"Zombie II and III each share one authored foot image between two rig slots.");
			string[] paths = content.AssetPatches.SelectMany(patch => patch.Replacements)
				.Select(replacement => replacement.AssetPath).Distinct().ToArray();
			Assert.That(paths, Has.Length.EqualTo(40));
			foreach (string path in paths)
			{
				string fullPath = Path.Combine(converted.RootPath, path);
				Assert.That(File.Exists(fullPath), Is.True, path);
				Texture2D texture = new Texture2D(2, 2);
				try
				{
					Assert.That(texture.LoadImage(File.ReadAllBytes(fullPath)), Is.True, path);
					Assert.That(texture.width, Is.EqualTo(32), path + " must retain its full legacy canvas.");
					Assert.That(texture.height, Is.EqualTo(32), path + " must retain its full legacy canvas.");
				}
				finally { Object.DestroyImmediate(texture); }
			}
		}

		[Test]
		public void ConvertedTweakedFerExample_PreservesAllThirtyRetouchedSprites()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack converted = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "legacy.tweaked-fer");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { converted });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Stages, Is.Empty,
				"The legacy archive retouched FER sprites; it did not replace FER's stage logic.");
			Assert.That(content.AssetPatches.Select(patch => patch.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:enemy-anatomy"),
				ContentId.Parse("core:sprite/butt-6"),
				ContentId.Parse("core:stage-art/fer")
			}));
			AssetReplacementDefinition[] replacements = content.AssetPatches
				.SelectMany(patch => patch.Replacements).ToArray();
			Assert.That(replacements, Has.Length.EqualTo(30));
			Assert.That(replacements.Select(replacement => replacement.AssetPath).Distinct().Count(), Is.EqualTo(30));
			Dictionary<string, int> canvasCounts = new Dictionary<string, int>(System.StringComparer.Ordinal);
			foreach (AssetReplacementDefinition replacement in replacements)
			{
				string path = Path.Combine(converted.RootPath, replacement.AssetPath);
				Assert.That(File.Exists(path), Is.True, replacement.AssetPath);
				Texture2D texture = new Texture2D(2, 2);
				try
				{
					Assert.That(texture.LoadImage(File.ReadAllBytes(path)), Is.True, replacement.AssetPath);
					string canvas = texture.width + "x" + texture.height;
					canvasCounts[canvas] = canvasCounts.TryGetValue(canvas, out int count) ? count + 1 : 1;
				}
				finally { Object.DestroyImmediate(texture); }
			}
			Assert.That(canvasCounts, Is.EquivalentTo(new Dictionary<string, int>
			{
				{ "32x32", 10 },
				{ "32x48", 5 },
				{ "64x64", 12 },
				{ "192x192", 2 },
				{ "512x32", 1 }
			}));
		}

		[Test]
		public void ConvertedCaptivitySfwExample_PreservesAllCensorSpritesAndCnrRules()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack sfw = packs.Packs.Single(pack => pack.Manifest.Id == "legacy.captivity-sfw");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { sfw });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			AssetPatchDefinition anatomy = content.AssetPatches.Single(patch =>
				patch.Target == ContentId.Parse("core:enemy-anatomy"));
			Assert.That(anatomy.Replacements, Has.Count.EqualTo(32));
			foreach (AssetReplacementDefinition replacement in anatomy.Replacements)
			{
				string path = Path.Combine(sfw.RootPath, replacement.AssetPath);
				Assert.That(File.Exists(path), Is.True, replacement.AssetPath);
				Texture2D texture = new Texture2D(2, 2);
				try
				{
					Assert.That(texture.LoadImage(File.ReadAllBytes(path)), Is.True, replacement.AssetPath);
					Assert.That(texture.GetPixels32().All(pixel => pixel.a == 0), Is.True,
						replacement.AssetPath + " should remain a transparent censor sprite.");
				}
				finally { Object.DestroyImmediate(texture); }
			}

			RuleProfileDefinition profile = content.RuleProfiles.Single();
			PlayerRule rules = profile.PlayerRules.Single();
			Assert.That(rules.EnemyFinishersEnabled, Is.False);
			Assert.That(rules.ClothingDamageEnabled, Is.False);
			Assert.That(rules.SafeKnockouts, Is.True);
			Assert.That(rules.PlayerHealthMultiplier, Is.EqualTo(3f));
		}

		[TestCase("DefaultShirt", "example.default-shirt-template", "core:clothing/shirt-default", 3, 96, 32)]
		[TestCase("LabCoat", "example.lab-coat-template", "core:clothing/scientist-set", 13, 128, 128)]
		[TestCase("HazmatSuit", "example.hazmat-suit-template", "core:clothing/hazmat-suit", 22, 160, 160)]
		public void LibreSpriteClothingTemplates_ParseAndFitTheirAtlases(string i_directory, string i_packId,
			string i_extends, int i_regionCount, int i_width, int i_height)
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ModSDK/ClothingTemplates", i_directory));
			string source = Path.Combine(root, "content/clothing-variant.json");
			ClothingDefinitionLoadResult result = ClothingDefinitionParser.Parse(File.ReadAllText(source), i_packId, source);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse(i_extends)));
			Assert.That(result.Definition.Visual.Regions, Has.Count.EqualTo(i_regionCount));

			Texture2D atlas = new Texture2D(2, 2);
			try
			{
				Assert.That(atlas.LoadImage(File.ReadAllBytes(Path.Combine(root, result.Definition.Visual.Atlas))), Is.True);
				Assert.That(atlas.width, Is.EqualTo(i_width));
				Assert.That(atlas.height, Is.EqualTo(i_height));
				foreach (AtlasRegionDefinition region in result.Definition.Visual.Regions.Values)
				{
					Assert.That(region.X + region.Width, Is.LessThanOrEqualTo(atlas.width));
					Assert.That(region.Y + region.Height, Is.LessThanOrEqualTo(atlas.height));
				}
			}
			finally
			{
				Object.DestroyImmediate(atlas);
			}
		}

		[Test]
		public void GoblinSlayerExample_ReplacesTheCoreKnightSetWithoutChangingItsUnlocksOrStats()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack goblinSlayer = packs.Packs.Single(pack => pack.Manifest.Id == "datz.goblin-slayer-armor");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { goblinSlayer });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Clothing, Is.Empty,
				"The legacy mod replaced the existing Knight set instead of registering duplicate garments.");
			Assert.That(content.Challenges, Is.Empty,
				"The Jungle unlock belongs to the Core Knight set; the port must not invent replacement missions.");
			Assert.That(content.AssetPatches.Select(item => item.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:clothing/knight-helmet"),
				ContentId.Parse("core:clothing/knight-breast-plate"),
				ContentId.Parse("core:clothing/knight-pants"),
				ContentId.Parse("core:clothing/knight-boots")
			}));
			foreach (AssetPatchDefinition patch in content.AssetPatches)
				foreach (string path in patch.Replacements.Select(replacement => replacement.AssetPath))
					Assert.That(File.Exists(Path.Combine(goblinSlayer.RootPath, path)), Is.True, path);
			AssetPatchDefinition pants = content.AssetPatches.Single(item =>
				item.Target == ContentId.Parse("core:clothing/knight-pants"));
			Assert.That(pants.Replacements.Any(item =>
				item.SlotId.ToString() == "core:clothing/knight-pants/icon"), Is.False,
				"The legacy archive did not author a pants icon; the Core wardrobe icon must be inherited.");
		}

		[Test]
		public void CodWonderweaponExample_ReplacesOnlyTheCoreSchockgewehrArtwork()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack wonderweapon = packs.Packs.Single(pack => pack.Manifest.Id == "legacy.cod-wonderweapon");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { wonderweapon });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Weapons, Is.Empty, "The legacy archive replaced Schockgewehr artwork, not the weapon definition.");
			AssetPatchDefinition patch = content.AssetPatches.Single();
			Assert.That(patch.Target, Is.EqualTo(ContentId.Parse("core:weapon/schockgewehr")));
			AssetReplacementDefinition replacement = patch.Replacements.Single();
			Assert.That(replacement.SlotId, Is.EqualTo(ContentId.Parse("core:weapon/schockgewehr/body")));
			Assert.That(File.Exists(Path.Combine(wonderweapon.RootPath, replacement.AssetPath)), Is.True);
		}

		[Test]
		public void SimpleNerfGunExample_ReskinsTheStarterPistolWithoutAddingAWeapon()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack nerf = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "somescrub.simple-nerf-gun");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { nerf });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Weapons, Is.Empty,
				"The legacy archive only replaced starter-pistol artwork.");
			AssetPatchDefinition patch = content.AssetPatches.Single();
			Assert.That(patch.Target, Is.EqualTo(ContentId.Parse("core:weapon/pistol")));
			Assert.That(patch.Replacements.Select(replacement => replacement.SlotId), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:weapon/pistol/body"),
				ContentId.Parse("core:weapon/pistol/slide"),
				ContentId.Parse("core:weapon/pistol/base")
			}));
			foreach (AssetReplacementDefinition replacement in patch.Replacements)
				Assert.That(File.Exists(Path.Combine(nerf.RootPath, replacement.AssetPath)), Is.True,
					replacement.AssetPath);
		}

		[Test]
		public void RaygunRevolverExample_ReplacesOnlyTheCoreRevolverBaseArtwork()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack raygun = packs.Packs.Single(pack => pack.Manifest.Id == "legacy.raygun-revolver");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { raygun });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Weapons, Is.Empty, "The legacy archive replaced revolver artwork, not its gameplay definition.");
			AssetPatchDefinition patch = content.AssetPatches.Single();
			Assert.That(patch.Target, Is.EqualTo(ContentId.Parse("core:weapon/revolver-44")));
			AssetReplacementDefinition replacement = patch.Replacements.Single();
			Assert.That(replacement.SlotId, Is.EqualTo(ContentId.Parse("core:weapon/revolver-44/base")));
			Assert.That(File.Exists(Path.Combine(raygun.RootPath, replacement.AssetPath)), Is.True);
		}

		[Test]
		public void StartWithNoGunExample_PreservesTheOriginalUnarmedFiveHundredDollarStart()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack startWithNoGun = packs.Packs.Single(pack => pack.Manifest.Id == "apothem.start-with-no-gun");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { startWithNoGun });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			RuleProfileDefinition profile = content.RuleProfiles.Single();
			Assert.That(profile.EconomyRules.Single().StartingMoney, Is.EqualTo(500));
			Assert.That(profile.PlayerRules.Single().StartWithoutWeapon, Is.True);
			Assert.That(profile.WeaponProgressions, Is.Empty,
				"The original mod removed the starter weapon; it did not replace it with another gun.");
		}

		[Test]
		public void SmallTweaksExample_PreservesAllNineLegacySpriteReplacements()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack smallTweaks = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "legacy.small-tweaks");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { smallTweaks });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.AssetPatches, Has.Count.EqualTo(4));
			Assert.That(content.AssetPatches.Sum(patch => patch.Replacements.Count), Is.EqualTo(10));
			Assert.That(content.AssetPatches.Select(patch => patch.Target), Is.EquivalentTo(new[]
			{
				ContentId.Parse("core:player"),
				ContentId.Parse("core:clothing/hair-brown-pony-tail"),
				ContentId.Parse("core:sprite/chest-7"),
				ContentId.Parse("core:weapon-effects/muzzle-flashes")
			}));
			foreach (AssetPatchDefinition patch in content.AssetPatches)
				foreach (AssetReplacementDefinition replacement in patch.Replacements)
					Assert.That(File.Exists(Path.Combine(smallTweaks.RootPath, replacement.AssetPath)), Is.True,
						replacement.AssetPath);
		}

		[Test]
		public void CryWhenRapedExample_ReplacesOnlyThePlayerBlushLayer()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack car = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "legacy.cry-when-raped");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { car });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			AssetPatchDefinition patch = content.AssetPatches.Single();
			Assert.That(patch.Target, Is.EqualTo(ContentId.Parse("core:player/face/blush")));
			AssetReplacementDefinition replacement = patch.Replacements.Single();
			Assert.That(replacement.SlotId, Is.EqualTo(ContentId.Parse("core:player/face/blush/image")));
			Assert.That(File.Exists(Path.Combine(car.RootPath, replacement.AssetPath)), Is.True);
		}

		[Test]
		public void SmilingBlushExample_DependsOnSmallTweaksAndAddsTwelveMouths()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModPack smiling = ModDiscovery.Discover(examples).Packs.Single(pack =>
				pack.Manifest.Id == "legacy.smiling-blush-small-tweaks");
			Assert.That(smiling.Manifest.Dependencies.Any(dependency =>
				dependency.Id == "legacy.small-tweaks"), Is.True);
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { smiling });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			AssetPatchDefinition patch = content.AssetPatches.Single();
			Assert.That(patch.Target, Is.EqualTo(ContentId.Parse("core:player/face/mouth")));
			Assert.That(patch.Replacements, Has.Count.EqualTo(12));
			foreach (AssetReplacementDefinition replacement in patch.Replacements)
				Assert.That(File.Exists(Path.Combine(smiling.RootPath, replacement.AssetPath)), Is.True,
					replacement.AssetPath);
		}

		[Test]
		public void ExtendedDifficultiesExample_DiscoversOriginalProfiles()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack extended = packs.Packs.Single(pack => pack.Manifest.Id == "dakozan.extended-difficulties");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { extended });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Difficulties.Select(item => item.DisplayName), Is.EqualTo(new[] { "Nightmare", "Very Hard" }));
			DifficultyDefinition nightmare = content.Difficulties.Single(item => item.DisplayName == "Nightmare");
			Assert.That(nightmare.EnemyHealthMultiplier, Is.EqualTo(1.5f));
			Assert.That(nightmare.PlayerDamageTakenMultiplier, Is.EqualTo(1.5f));
			Assert.That(nightmare.EscapeStrengthMultiplier, Is.EqualTo(0.25f));
		}

		[Test]
		public void OriginalDeveloperExtras_DiscoversThrowablesAndPregnancyBelly()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack extras = packs.Packs.Single(pack => pack.Manifest.Id == "perveloper.original-dev-extras");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { extras });
			Assert.That(content.Report.IsValid, Is.True, string.Join("\n", content.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(content.Weapons.Select(weapon => weapon.DisplayName), Is.EquivalentTo(new[] { "Rock", "Frag Grenade" }));
			Assert.That(content.Weapons.All(weapon => weapon.Behavior.Throwable != null), Is.True);
			PlayerAttachmentDefinition belly = content.PlayerAttachments.Single();
			Assert.That(belly.Document.PregnancyGrowth.Stages, Has.Count.EqualTo(5));
			Assert.That(belly.Document.PregnancyGrowth.TransitionSeconds, Is.EqualTo(1.5f));
			StageDefinition brothel = content.Stages.Single();
			Assert.That(brothel.Layout.TiledLevel.Tilesets, Has.Count.EqualTo(8));
			Assert.That(brothel.Layout.TiledLevel.Decorations, Has.Count.EqualTo(20));
			Assert.That(brothel.Layout.TiledLevel.Platforms, Has.Count.EqualTo(13));
			Assert.That(brothel.Layout.TiledLevel.Pickups.Single().Item,
				Is.EqualTo(ContentId.Parse("perveloper.original-dev-extras:item/weapon/rock")));
			foreach (string path in belly.Document.SkinSprites.Values)
			{
				Texture2D texture = new Texture2D(2, 2);
				try
				{
					Assert.That(texture.LoadImage(File.ReadAllBytes(Path.Combine(extras.RootPath, path))), Is.True, path);
					Assert.That(texture.width, Is.EqualTo(32), path);
					Assert.That(texture.height, Is.EqualTo(32), path);
				}
				finally { Object.DestroyImmediate(texture); }
			}
		}

		[Test]
		public void GunGameExample_UsesOnlyOriginalCoreWeaponPool()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			ModPack gunGame = packs.Packs.Single(pack => pack.Manifest.Id == "apothem.gun-game");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { gunGame });
			Assert.That(content.Report.IsValid, Is.True);
			IReadOnlyList<ContentId> pool = content.RuleProfiles.Single().WeaponProgressions.Single().WeaponPool;
			Assert.That(pool, Has.Count.EqualTo(21));
			Assert.That(pool.All(id => id.Namespace == "core"), Is.True);
		}

		[Test]
		public void TrainingYardStageExample_DiscoversWithCoreEnemies()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack trainingYard = packs.Packs.Single(pack => pack.Manifest.Id == "example.training-yard");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { trainingYard });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Stages, Has.Count.EqualTo(1));
			StageDefinition stage = content.Stages.Single();
			Assert.That(stage.Extends, Is.EqualTo(ContentId.Parse("core:stage/field-day")));
			Assert.That(stage.Spawners, Has.Count.EqualTo(2));
			Assert.That(stage.Spawners.SelectMany(spawner => spawner.Enemies).All(enemy => enemy.Namespace == "core"), Is.True);
		}

		[Test]
		public void AdditiveNerfPistolExample_DiscoversWithAllPublishedSprites()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack nerf = packs.Packs.Single(pack => pack.Manifest.Id == "somescrub.additive-nerf-pistol");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { nerf });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Weapons, Has.Count.EqualTo(1));
			WeaponDefinition weapon = content.Weapons.Single();
			Assert.That(weapon.Extends, Is.EqualTo(ContentId.Parse("core:item/weapon/pistol")));
			Assert.That(weapon.Stats.HoldType, Is.EqualTo("OneHanded"));
			Assert.That(weapon.Behavior.BurstCount, Is.EqualTo(3));
			Assert.That(weapon.Behavior.TracerColor, Is.EqualTo("#44CCFFFF"));
			Assert.That(weapon.Visual.Sprites.Keys, Is.EquivalentTo(new[] { "body", "slide", "base" }));
			foreach (string path in weapon.Visual.Sprites.Values)
			{
				string fullPath = Path.Combine(nerf.RootPath, path);
				Assert.That(File.Exists(fullPath), Is.True, path);
				Texture2D spriteTexture = new Texture2D(2, 2);
				try
				{
					Assert.That(spriteTexture.LoadImage(File.ReadAllBytes(fullPath)), Is.True, path);
					Assert.That(spriteTexture.width, Is.GreaterThan(0), path);
					Assert.That(spriteTexture.height, Is.GreaterThan(0), path);
				}
				finally
				{
					Object.DestroyImmediate(spriteTexture);
				}
			}
			foreach (string path in weapon.Behavior.MuzzleFlashSprites.Concat(new[] { weapon.Behavior.CasingSprite }))
				Assert.That(File.Exists(Path.Combine(nerf.RootPath, path)), Is.True, path);
		}

		[TestCase("Pistol", "example.pistol-template", "core:item/weapon/pistol", 4, 32, 32)]
		[TestCase("TenelliSO3", "example.tenelli-so3-template", "core:item/weapon/tenelli-so3", 4, 48, 32)]
		[TestCase("Revolver44", "example.revolver-44-template", "core:item/weapon/revolver-44", 10, 32, 32)]
		public void LibreSpriteWeaponTemplates_ParseAndExposeValidSpriteFiles(string i_directory, string i_packId,
			string i_extends, int i_spriteCount, int i_width, int i_height)
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ModSDK/WeaponTemplates", i_directory));
			string source = Path.Combine(root, "content/weapon-variant.json");
			WeaponDefinitionLoadResult result = WeaponDefinitionParser.Parse(File.ReadAllText(source), i_packId, source);
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Extends, Is.EqualTo(ContentId.Parse(i_extends)));
			Assert.That(result.Definition.Visual.Sprites, Has.Count.EqualTo(i_spriteCount));
			Assert.That(result.Definition.Visual.Sprites.Keys,
				Is.EquivalentTo(WeaponTemplateCatalog.GetSlots(result.Definition.Extends.Value)));

			foreach (string path in result.Definition.Visual.Sprites.Values)
			{
				Texture2D texture = new Texture2D(2, 2);
				try
				{
					Assert.That(texture.LoadImage(File.ReadAllBytes(Path.Combine(root, path))), Is.True, path);
					Assert.That(texture.width, Is.EqualTo(i_width), path);
					Assert.That(texture.height, Is.EqualTo(i_height), path);
				}
				finally
				{
					Object.DestroyImmediate(texture);
				}
			}
		}

		[Test]
		public void AnimationReferenceGallery_DiscoversAllSixRigFamiliesAndTheirClips()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True,
				string.Join("\n", packs.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			ModPack gallery = packs.Packs.Single(pack => pack.Manifest.Id == "example.animation-reference-gallery");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { gallery });
			Assert.That(content.Report.IsValid, Is.True,
				string.Join("\n", content.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(content.Enemies, Has.Count.EqualTo(6));
			Assert.That(content.EnemyAnimations, Has.Count.EqualTo(72));
			Assert.That(content.Enemies.All(enemy => !enemy.Spawn.InheritTemplateSpawners), Is.True);

			Dictionary<ContentId, NormalizedEnemyAnimationDefinition> animations =
				content.EnemyAnimations.ToDictionary(animation => animation.Id);
			foreach (EnemyDefinition enemy in content.Enemies)
				foreach (KeyValuePair<string, string> reference in enemy.AnimationReferences)
				{
					ContentId animationId = ContentId.Parse(reference.Value);
					Assert.That(animations.ContainsKey(animationId), Is.True, "Missing " + reference.Value);
					Assert.That(animations[animationId].Enemy, Is.EqualTo(enemy.Id),
						"Animation owner mismatch for " + reference.Value);
				}

			string rigRoot = Path.Combine(gallery.RootPath, "reference", "rigs");
			Assert.That(new[] { "zombie", "death-hound", "fly", "maggot", "musca", "gremlin" }
				.All(name => File.Exists(Path.Combine(rigRoot, name, "rig.json"))), Is.True);
		}

		[Test]
		public void ConvertedPreyZombieExample_DiscoversAndFitsItsAtlas()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack prey = packs.Packs.Single(pack => pack.Manifest.Id == "draco66electro.prey-green-zombie");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { prey });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Enemies, Has.Count.EqualTo(2));
			EnemyDefinition enemy = content.Enemies.Single(item =>
				item.Id == ContentId.Parse("draco66electro.prey-green-zombie:enemy/green-zombie"));
			Assert.That(enemy.Extends, Is.EqualTo(ContentId.Parse("core:enemy/zombie-1")));
			Assert.That(enemy.Visual.Regions, Has.Count.EqualTo(13));
			Assert.That(enemy.Spawn.InheritTemplateSpawners, Is.True);

			string atlasPath = Path.Combine(prey.RootPath, enemy.Visual.Atlas);
			Texture2D atlas = new Texture2D(2, 2);
			try
			{
				Assert.That(atlas.LoadImage(File.ReadAllBytes(atlasPath)), Is.True);
				Assert.That(atlas.width, Is.EqualTo(128));
				Assert.That(atlas.height, Is.EqualTo(128));
				foreach (AtlasRegionDefinition region in enemy.Visual.Regions.Values)
				{
					Assert.That(region.X + region.Width, Is.LessThanOrEqualTo(atlas.width));
					Assert.That(region.Y + region.Height, Is.LessThanOrEqualTo(atlas.height));
				}
			}
			finally
			{
				Object.DestroyImmediate(atlas);
			}
		}

		[Test]
		public void ZombieOneLibreSpriteTemplateExample_DiscoversAndFitsItsAtlas()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack templatePack = packs.Packs.Single(pack => pack.Manifest.Id == "example.zombie-1-template");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { templatePack });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Enemies, Has.Count.EqualTo(1));
			EnemyDefinition enemy = content.Enemies.Single();
			Assert.That(enemy.Extends, Is.EqualTo(ContentId.Parse("core:enemy/zombie-1")));
			Assert.That(enemy.Visual.Regions, Has.Count.EqualTo(13));
			Assert.That(enemy.Spawn.InheritTemplateSpawners, Is.True);
			Assert.That(enemy.Spawn.SelectionWeight, Is.EqualTo(0.25f));
			Assert.That(enemy.Stats.HealthMax, Is.EqualTo(80));

			string atlasPath = Path.Combine(templatePack.RootPath, enemy.Visual.Atlas);
			Texture2D atlas = new Texture2D(2, 2);
			try
			{
				Assert.That(atlas.LoadImage(File.ReadAllBytes(atlasPath)), Is.True);
				Assert.That(atlas.width, Is.EqualTo(128));
				Assert.That(atlas.height, Is.EqualTo(128));
				foreach (AtlasRegionDefinition region in enemy.Visual.Regions.Values)
				{
					Assert.That(region.X + region.Width, Is.LessThanOrEqualTo(atlas.width));
					Assert.That(region.Y + region.Height, Is.LessThanOrEqualTo(atlas.height));
				}
			}
			finally
			{
				Object.DestroyImmediate(atlas);
			}
		}

		[Test]
		public void Discover_DispatchesEnemyDefinitionsFromDeclaredRoots()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ModContentDiscoveryTests"));
			string content = Path.Combine(root, "content");
			Directory.CreateDirectory(content);
			try
			{
				File.WriteAllText(Path.Combine(content, "enemy.json"), @"{
  'schemaVersion': 1,
  'type': 'enemy',
  'id': 'example.enemies:enemy/test',
  'displayName': 'Test Enemy',
  'extends': 'core:enemy/gremlin',
  'visual': {
    'type': 'coreRigAtlas',
    'atlas': 'assets/test.png',
    'regions': { 'body/head': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } }
  }
}");
				SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
				ModPack pack = new ModPack(new ModManifest
				{
					SchemaVersion = 1,
					Id = "example.enemies",
					DisplayName = "Example Enemies",
					Version = "1.0.0",
					ModApiVersion = 1,
					ContentRoots = new List<string> { "content" }
				}, version, root);
				ModContentDiscoveryResult result = ModContentDiscovery.Discover(new[] { pack });
				Assert.That(result.Report.IsValid, Is.True);
				Assert.That(result.Enemies, Has.Count.EqualTo(1));
				Assert.That(result.Enemies[0].Id, Is.EqualTo(ContentId.Parse("example.enemies:enemy/test")));
				Assert.That(result.AssetPatches, Is.Empty);
			}
			finally
			{
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		[Test]
		public void Discover_DispatchesClothingDefinitionsFromDeclaredRoots()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ModClothingDiscoveryTests"));
			string content = Path.Combine(root, "content");
			Directory.CreateDirectory(content);
			try
			{
				File.WriteAllText(Path.Combine(content, "clothing.json"), @"{
  'schemaVersion': 1,
  'type': 'clothing',
  'id': 'example.clothes:clothing/test-shirt',
  'displayName': 'Test Shirt',
  'extends': 'core:clothing/shirt-default',
  'visual': {
    'type': 'coreClothingAtlas',
    'atlas': 'assets/test-shirt.png',
    'regions': { 'piece/shirt-chest': { 'x': 0, 'y': 0, 'width': 32, 'height': 32 } }
  }
}");
				SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
				ModPack pack = new ModPack(new ModManifest
				{
					SchemaVersion = 1,
					Id = "example.clothes",
					DisplayName = "Example Clothes",
					Version = "1.0.0",
					ModApiVersion = 1,
					ContentRoots = new List<string> { "content" }
				}, version, root);
				ModContentDiscoveryResult result = ModContentDiscovery.Discover(new[] { pack });
				Assert.That(result.Report.IsValid, Is.True);
				Assert.That(result.Clothing, Has.Count.EqualTo(1));
				Assert.That(result.Clothing[0].Id, Is.EqualTo(ContentId.Parse("example.clothes:clothing/test-shirt")));
				Assert.That(result.Enemies, Is.Empty);
			}
			finally
			{
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		[Test]
		public void Discover_ReportsUnsupportedDefinitionTypes()
		{
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ModContentUnsupportedTests"));
			string content = Path.Combine(root, "content");
			Directory.CreateDirectory(content);
			try
			{
				File.WriteAllText(Path.Combine(content, "unknown.json"), "{ 'type': 'arbitraryScript' }");
				SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
				ModPack pack = new ModPack(new ModManifest
				{
					Id = "example.invalid",
					ContentRoots = new List<string> { "content" }
				}, version, root);
				ModContentDiscoveryResult result = ModContentDiscovery.Discover(new[] { pack });
				Assert.That(result.Report.IsValid, Is.False);
				Assert.That(result.Report.Issues.Any(issue => issue.Code == "content.type"), Is.True);
			}
			finally
			{
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		private static Transform ResolveIndexedTransformPath(Transform i_root, string i_path)
		{
			Transform current = i_root;
			foreach (string rawSegment in (i_path ?? string.Empty).Split('/'))
			{
				string name = rawSegment;
				int siblingIndex = -1;
				int bracket = rawSegment.LastIndexOf('[');
				if (bracket > 0 && rawSegment.EndsWith("]", System.StringComparison.Ordinal)
					&& int.TryParse(rawSegment.Substring(bracket + 1, rawSegment.Length - bracket - 2), out int parsed))
				{
					name = rawSegment.Substring(0, bracket);
					siblingIndex = parsed;
				}
				Transform next = null;
				for (int index = 0; current != null && index < current.childCount; index++)
				{
					Transform child = current.GetChild(index);
					if (child.name == name && (siblingIndex < 0 || child.GetSiblingIndex() == siblingIndex))
					{
						next = child;
						break;
					}
				}
				if (next == null) return null;
				current = next;
			}
			return current == i_root ? null : current;
		}
	}

	public class CoreContentCatalogParserTests
	{
		[Test]
		public void PackagedCatalog_MapsAllCanonicalEnemiesAndStages()
		{
			TextAsset asset = Resources.Load<TextAsset>("Modding/Core/catalog");
			Assert.That(asset, Is.Not.Null);
			CoreContentCatalogLoadResult result = CoreContentCatalogParser.Parse(asset.text, "core/catalog.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Enemy), Is.EqualTo(22));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Stage), Is.EqualTo(7));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Clothing), Is.EqualTo(98));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Item), Is.EqualTo(33));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Challenge), Is.EqualTo(79));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:enemy/gremlin")).LegacyId, Is.EqualTo(12));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:enemy/android")).LegacyName, Is.EqualTo("Android"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:stage/field-day")).LegacyId, Is.EqualTo(6));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/weapon/pistol")).LegacyName, Is.EqualTo("Pistol"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/usable/morphine")).LegacyName, Is.EqualTo("Morphine"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/consumable/ammo-box")).LegacyName, Is.EqualTo("Ammo Box"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:clothing/hazmat-suit")).LegacyId, Is.EqualTo(78));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:clothing/lingerie-white-lower")).LegacyId, Is.EqualTo(97));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:challenge/shack-expert")).LegacyId, Is.EqualTo(20));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:challenge/incest")).LegacyId, Is.EqualTo(38));
		}

		[Test]
		public void Parse_RejectsDuplicateLegacyIdsWithinCategory()
		{
			const string json = @"{
  'schemaVersion': 1,
  'entries': [
    { 'id': 'core:enemy/first', 'category': 'Enemy', 'legacyId': 1 },
    { 'id': 'core:enemy/second', 'category': 'Enemy', 'legacyId': 1 }
  ]
}";
			CoreContentCatalogLoadResult result = CoreContentCatalogParser.Parse(json, "core/catalog.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "catalog.duplicate-legacy-selector"), Is.True);
		}

		[Test]
		public void Parse_RequiresExactlyOneLegacySelector()
		{
			const string json = @"{
  'schemaVersion': 1,
  'entries': [
    { 'id': 'core:item/weapon/missing', 'category': 'Item' },
    { 'id': 'core:item/weapon/ambiguous', 'category': 'Item', 'legacyId': 1, 'legacyName': 'Pistol' }
  ]
}";
			CoreContentCatalogLoadResult result = CoreContentCatalogParser.Parse(json, "core/catalog.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Count(issue => issue.Code == "catalog.legacy-selector"), Is.EqualTo(2));
		}
	}

	public class RuleProfileParserTests
	{
		private const string ValidRule = @"{
  'schemaVersion': 1,
  'type': 'ruleProfile',
  'id': 'example.rules:rule/gun-game',
  'displayName': 'Gun Game',
  'modules': [{
    'type': 'weaponProgression',
    'trigger': 'enemyKilled',
    'selection': 'random',
    'starterWeapon': 'core:item/weapon/pistol',
    'replaceExistingWeapons': true,
    'weaponPool': ['core:item/weapon/pistol', 'core:item/weapon/m4b1']
  }]
}";

		[Test]
		public void Parse_AcceptsBoundedWeaponProgression()
		{
			RuleProfileLoadResult result = RuleProfileParser.Parse(ValidRule, "example.rules", "gun-game.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.Id, Is.EqualTo(ContentId.Parse("example.rules:rule/gun-game")));
			Assert.That(result.Definition.WeaponProgressions, Has.Count.EqualTo(1));
			Assert.That(result.Definition.WeaponProgressions[0].WeaponPool, Has.Count.EqualTo(2));
			Assert.That(result.Definition.WeaponProgressions[0].ReplaceExistingWeapons, Is.True);
		}

		[Test]
		public void Parse_AcceptsBoundedSpawnModifiers()
		{
			const string json = @"{
  'schemaVersion': 1,
  'type': 'ruleProfile',
  'id': 'example.rules:rule/rush',
  'displayName': 'Rush',
  'modules': [{
    'type': 'spawnModifiers',
    'waveSpawnMultiplier': 1.5,
    'waveSpawnAdd': 3,
    'maxStageEnemiesMultiplier': 1.25,
    'maxRoomEnemiesAdd': 5,
    'spawnerChanceMultiplier': 2,
    'spawnDelayMultiplier': 0.5,
    'initialSpawnDelayMultiplier': 0.25
  }]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "rush.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.SpawnModifiers, Has.Count.EqualTo(1));
			Assert.That(result.Definition.SpawnModifiers[0].WaveSpawnMultiplier, Is.EqualTo(1.5f));
			Assert.That(result.Definition.SpawnModifiers[0].WaveSpawnAdd, Is.EqualTo(3));
			Assert.That(result.Definition.SpawnModifiers[0].SpawnDelayMultiplier, Is.EqualTo(0.5f));
		}

		[Test]
		public void Parse_AcceptsCompleteGameModeModules()
		{
			const string json = @"{
  'schemaVersion': 1,
  'type': 'ruleProfile',
  'id': 'example.rules:rule/survival-sprint',
  'displayName': 'Survival Sprint',
  'modules': [
    { 'type': 'waveRules', 'spawnGrowthPerWave': 4, 'intermissionSeconds': 8, 'maximumWaves': 10 },
    { 'type': 'economyRules', 'startingMoney': 250, 'bountyMultiplier': 1.5, 'ammoDropChance': 0.2 },
    { 'type': 'playerRules', 'playerDamageTakenMultiplier': 1.25, 'gunDamageMultiplier': 1.1 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "survival-sprint.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.WaveRules.Single().MaximumWaves, Is.EqualTo(10));
			Assert.That(result.Definition.EconomyRules.Single().StartingMoney, Is.EqualTo(250));
			Assert.That(result.Definition.PlayerRules.Single().GunDamageMultiplier, Is.EqualTo(1.1f));
		}

		[Test]
		public void Parse_AcceptsDeveloperModeRules()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/developer', 'displayName': 'Developer Mode',
  'modules': [
    { 'type': 'economyRules', 'startingMoney': 1000000, 'infiniteMoney': true },
	{ 'type': 'playerRules', 'playerDamageTakenMultiplier': 0, 'grantAllWeapons': true, 'ignoreWeightLimit': true, 'debugHotkeys': true }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "developer.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.EconomyRules.Single().InfiniteMoney, Is.True);
			Assert.That(result.Definition.PlayerRules.Single().GrantAllWeapons, Is.True);
			Assert.That(result.Definition.PlayerRules.Single().IgnoreWeightLimit, Is.True);
			Assert.That(result.Definition.PlayerRules.Single().DebugHotkeys, Is.True);
		}

		[Test]
		public void Parse_AcceptsExperimentalPlayerAndStoreMechanics()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/store-mechanics', 'displayName': 'Store Mechanics',
  'modules': [
    { 'type': 'economyRules', 'repeatConsumablePurchases': true, 'clothingRepairEnabled': true, 'clothingRepairCostMultiplier': 0.5 },
    { 'type': 'playerRules', 'selfPleasureEnabled': true, 'selfPleasurePerSecond': 20, 'selfPleasureHeartCost': 0 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "store-mechanics.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.EconomyRules.Single().RepeatConsumablePurchases, Is.True);
			Assert.That(result.Definition.EconomyRules.Single().ClothingRepairCostMultiplier, Is.EqualTo(0.5f));
			Assert.That(result.Definition.PlayerRules.Single().SelfPleasurePerSecond, Is.EqualTo(20f));
			Assert.That(result.Definition.PlayerRules.Single().SelfPleasureHeartCost, Is.Zero);
		}

		[Test]
		public void Parse_AcceptsLegacyOverhaulRulesWithoutManagedCode()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/legacy-overhaul', 'displayName': 'Legacy Overhaul',
  'modules': [
    { 'type': 'playerRules', 'maximumHearts': 5, 'libidoMaximumMultiplier': 5,
      'retainBuffsOnClimax': true, 'strengthDamageWhileRestrainedMultiplier': 0.6666667,
      'strengthDamageFreeMultiplier': 1.5, 'clothingDamageChance': 0.8,
      'forceAutomaticWeapons': true, 'autoReloadOnEmpty': true,
      'weaponRangeMultiplier': 2, 'cameraZoomMultiplier': 1.4 },
    { 'type': 'consumableOverride', 'item': 'morphine', 'healthRestore': 100,
      'pleasureReduction': 25, 'restoreHeart': true },
    { 'type': 'eventReward', 'trigger': 'enemyKilled', 'healthReward': 2,
      'strengthReward': 2, 'pleasureReductionReward': 2, 'moneyReward': 10,
      'perCompletedExperimentMultiplier': 0.075 },
    { 'type': 'experimentScaling', 'speedPerExperimentPercent': 1.6666667,
      'dashPerExperimentPercent': 1.3888889, 'gunDamagePerExperimentPercent': 8,
      'healthPerExperimentPercent': 2, 'knockbackPerExperimentPercent': 10 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "legacy-overhaul.json");
			Assert.That(result.Report.IsValid, Is.True, string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(result.Definition.PlayerRules.Single().MaximumHearts, Is.EqualTo(5));
			Assert.That(result.Definition.PlayerRules.Single().ForceAutomaticWeapons, Is.True);
			Assert.That(result.Definition.ConsumableRules.Single().RestoreHeart, Is.True);
			Assert.That(result.Definition.EventRewardRules.Single().Trigger, Is.EqualTo("enemyKilled"));
			Assert.That(result.Definition.ExperimentScalingRules.Single().GunDamagePercent, Is.EqualTo(8f));
		}

		[Test]
		public void Parse_RejectsUnsafeLegacyOverhaulRules()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/unsafe-overhaul', 'displayName': 'Unsafe Overhaul',
  'modules': [
    { 'type': 'playerRules', 'maximumHearts': 99, 'clothingDamageChance': 2 },
    { 'type': 'consumableOverride', 'item': 'unknown', 'healthRestore': -1 },
    { 'type': 'eventReward', 'trigger': 'arbitraryCode', 'moneyReward': -1 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "unsafe-overhaul.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.maximum-hearts"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.consumable-item"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.event-reward-trigger"), Is.True);
		}

		[Test]
		public void LegacyCodeHeavyProfiles_ParseThroughPortableRuleModules()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			foreach (string folder in new[] { "legacy-clothing-overhaul", "legacy-luins-balance", "legacy-c4c", "legacy-overpower" })
			{
				string content = Directory.GetFiles(Path.Combine(examples, folder, "content"), "*.json").Single();
				string manifestPath = Path.Combine(examples, folder, "manifest.json");
				string packId = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false).Manifest.Id;
				RuleProfileLoadResult result = RuleProfileParser.Parse(File.ReadAllText(content), packId, content);
				Assert.That(result.Report.IsValid, Is.True, folder + ": " + string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			}
		}

		[Test]
		public void Parse_AcceptsStartingWithoutAWeapon()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/unarmed-start', 'displayName': 'Unarmed Start',
  'modules': [
    { 'type': 'economyRules', 'startingMoney': 500 },
    { 'type': 'playerRules', 'startWithoutWeapon': true }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "unarmed-start.json");
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.ToString())));
			Assert.That(result.Definition.EconomyRules.Single().StartingMoney, Is.EqualTo(500));
			Assert.That(result.Definition.PlayerRules.Single().StartWithoutWeapon, Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafePlayerAndStoreMechanicValues()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/unsafe-store', 'displayName': 'Unsafe Store',
  'modules': [
    { 'type': 'economyRules', 'clothingRepairCostMultiplier': 11 },
    { 'type': 'playerRules', 'selfPleasurePerSecond': 0, 'selfPleasureHeartCost': 2 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "unsafe-store.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.clothing-repair-cost-multiplier"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.self-pleasure-per-second"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.self-pleasure-heart-cost"), Is.True);
		}

		[Test]
		public void Parse_AcceptsScoringVictoryAndLossGoals()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/score-rush', 'displayName': 'Score Rush',
  'modules': [
    { 'type': 'scoringRules', 'killScore': 100, 'waveScore': 500, 'damageTakenPenalty': 25 },
    { 'type': 'goalRules', 'targetScore': 5000, 'targetKills': 30, 'goalMatch': 'any', 'timeLimitSeconds': 600, 'maxDamageEvents': 10 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "score-rush.json");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Definition.ScoringRules.Single().WaveScore, Is.EqualTo(500));
			Assert.That(result.Definition.GoalRules.Single().GoalMatch, Is.EqualTo("any"));
			Assert.That(result.Definition.GoalRules.Single().TimeLimitSeconds, Is.EqualTo(600f));
		}

		[Test]
		public void Parse_RejectsUnsafeGameModeValuesAndDuplicateWaveRules()
		{
			const string json = @"{
  'schemaVersion': 1, 'type': 'ruleProfile',
  'id': 'example.rules:rule/unsafe', 'displayName': 'Unsafe',
  'modules': [
    { 'type': 'waveRules', 'intermissionSeconds': 301 },
    { 'type': 'waveRules', 'maximumWaves': 10 },
    { 'type': 'economyRules', 'ammoDropChance': 2 },
    { 'type': 'playerRules', 'gunDamageMultiplier': 0 }
  ]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "unsafe.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.wave-rules-duplicate"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.ammo-drop-chance"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.gun-damage-multiplier"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownModulesAndDuplicateWeapons()
		{
			string unknown = ValidRule.Replace("weaponProgression", "arbitraryScript");
			string duplicate = ValidRule.Replace("'core:item/weapon/m4b1'", "'core:item/weapon/pistol'");
			Assert.That(RuleProfileParser.Parse(unknown, "example.rules", "unknown.json").Report.IsValid, Is.False);
			RuleProfileLoadResult duplicateResult = RuleProfileParser.Parse(duplicate, "example.rules", "duplicate.json");
			Assert.That(duplicateResult.Report.Issues.Any(issue => issue.Code == "rule.weapon-pool"), Is.True);
		}

		[Test]
		public void Parse_RejectsUnsafeSpawnModifiers()
		{
			const string json = @"{
  'schemaVersion': 1,
  'type': 'ruleProfile',
  'id': 'example.rules:rule/unsafe-rush',
  'displayName': 'Unsafe Rush',
  'modules': [{
    'type': 'spawnModifiers',
    'waveSpawnMultiplier': 0,
    'waveSpawnAdd': 1001,
    'spawnDelayMultiplier': 20
  }]
}";
			RuleProfileLoadResult result = RuleProfileParser.Parse(json, "example.rules", "unsafe-rush.json");
			Assert.That(result.Report.IsValid, Is.False);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.wave-spawn-multiplier"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.wave-spawn-add"), Is.True);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "rule.spawn-delay-multiplier"), Is.True);
		}
	}

	public class InputGlyphAssetTests
	{
		[Test]
		public void KenneyGlyphSets_ArePackagedForControllersAndStyleC()
		{
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/Xbox/north"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/PlayStation/north"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/Switch/north"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/SteamDeck/north"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/Mobile/Icons/icon_hand"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/Mobile/C/button_circle"), Is.Not.Null);
			Assert.That(Resources.Load<Texture2D>("InputGlyphs/Mobile/C/joystick_circle_pad_a"), Is.Not.Null);
		}
	}

	public class ContentSaveKeyTests
	{
		private static CoreContentCatalogLoadResult LoadCatalog()
		{
			TextAsset asset = Resources.Load<TextAsset>("Modding/Core/catalog");
			return CoreContentCatalogParser.Parse(asset.text, "core/catalog.json");
		}

		[Test]
		public void LegacyMap_RoundTripsNumericCoreContentByCategory()
		{
			LegacyContentMap map = new LegacyContentMap(LoadCatalog().Entries);
			Assert.That(map.Count, Is.EqualTo(205));
			Assert.That(map.TryGetContentId(ContentCategory.Enemy, 12, out ContentId enemy), Is.True);
			Assert.That(enemy, Is.EqualTo(ContentId.Parse("core:enemy/gremlin")));
			Assert.That(map.TryGetContentId(ContentCategory.Stage, 1, out ContentId stage), Is.True);
			Assert.That(stage, Is.EqualTo(ContentId.Parse("core:stage/shack")));
			Assert.That(map.TryGetLegacyKey(enemy, out LegacyContentKey legacy), Is.True);
			Assert.That(legacy.Category, Is.EqualTo(ContentCategory.Enemy));
			Assert.That(legacy.Id, Is.EqualTo(12));
		}

		[Test]
		public void LegacyMap_DoesNotConfuseEqualIdsAcrossCategoriesOrNamedItems()
		{
			LegacyContentMap map = new LegacyContentMap(LoadCatalog().Entries);
			Assert.That(map.TryGetContentId(ContentCategory.Enemy, 1, out ContentId enemy), Is.True);
			Assert.That(map.TryGetContentId(ContentCategory.Stage, 1, out ContentId stage), Is.True);
			Assert.That(enemy, Is.Not.EqualTo(stage));
			Assert.That(map.TryGetLegacyKey(ContentId.Parse("core:item/weapon/pistol"), out _), Is.False);
		}

		[Test]
		public void LegacyResolver_TranslatesExistingRowsWithoutRewritingThem()
		{
			LegacyContentMap map = new LegacyContentMap(LoadCatalog().Entries);
			Assert.That(ContentSaveResolver.TryResolveLegacy(ContentCategory.Clothing, 78, "{\"unlocked\":true}", map, new ContentRegistry(), out SavedContentResolution resolution), Is.True);
			Assert.That(resolution.State.ContentId, Is.EqualTo(ContentId.Parse("core:clothing/hazmat-suit")));
			Assert.That(resolution.State.StateJson, Is.EqualTo("{\"unlocked\":true}"));
			Assert.That(resolution.Status, Is.EqualTo(SavedContentStatus.Missing));
			Assert.That(ContentSaveResolver.TryResolveLegacy(ContentCategory.Clothing, 9999, "{}", map, new ContentRegistry(), out _), Is.False);
		}

		[Test]
		public void SavedState_PreservesValidMissingModContent()
		{
			SavedContentState state = new SavedContentState(ContentId.Parse("example.pack:enemy/retired"), ContentCategory.Enemy, "{\"kills\":4}");
			SavedContentResolution resolution = ContentSaveResolver.Resolve(state, new ContentRegistry());
			Assert.That(resolution.Status, Is.EqualTo(SavedContentStatus.Missing));
			Assert.That(resolution.State.ContentId, Is.EqualTo(ContentId.Parse("example.pack:enemy/retired")));
			Assert.That(resolution.State.StateJson, Is.EqualTo("{\"kills\":4}"));
			Assert.That(resolution.Registration, Is.Null);
		}

		[Test]
		public void SavedState_ResolvesAvailableContentAndDetectsCategoryMismatch()
		{
			ContentRegistry registry = new ContentRegistry();
			ContentId id = ContentId.Parse("example.pack:enemy/test");
			registry.Register(new ContentRegistration(id, ContentCategory.Enemy, "example.pack", "enemy.json"), new ValidationReport());
			SavedContentResolution available = ContentSaveResolver.Resolve(new SavedContentState(id, ContentCategory.Enemy, "{}"), registry);
			SavedContentResolution mismatch = ContentSaveResolver.Resolve(new SavedContentState(id, ContentCategory.Stage, "{}"), registry);
			Assert.That(available.Status, Is.EqualTo(SavedContentStatus.Available));
			Assert.That(available.Registration, Is.Not.Null);
			Assert.That(mismatch.Status, Is.EqualTo(SavedContentStatus.CategoryMismatch));
		}

		[Test]
		public void SavedStateParser_RejectsInvalidKeysAndNumericCategoryNames()
		{
			Assert.That(SavedContentState.TryCreate("not-an-id", "Enemy", "{}", out _), Is.False);
			Assert.That(SavedContentState.TryCreate("example.pack:enemy/test", "0", "{}", out _), Is.False);
			Assert.That(SavedContentState.TryCreate("example.pack:enemy/test", "enemy", "{}", out _), Is.False);
			Assert.That(SavedContentState.TryCreate("example.pack:enemy/test", "Enemy", "", out SavedContentState state), Is.True);
			Assert.That(state.StateJson, Is.EqualTo("{}"));
		}

		[Test]
		public void RenamedContentId_DoesNotSilentlyAliasOldSaveData()
		{
			ContentRegistry registry = new ContentRegistry();
			ContentId replacement = ContentId.Parse("example.pack:enemy/new-name");
			registry.Register(new ContentRegistration(replacement, ContentCategory.Enemy, "example.pack", "enemy.json"), new ValidationReport());
			SavedContentResolution resolution = ContentSaveResolver.Resolve(
				new SavedContentState(ContentId.Parse("example.pack:enemy/old-name"), ContentCategory.Enemy, "{\"kills\":9}"), registry);
			Assert.That(resolution.Status, Is.EqualTo(SavedContentStatus.Missing));
			Assert.That(resolution.State.ContentId.ToString(), Is.EqualTo("example.pack:enemy/old-name"));
			Assert.That(resolution.State.StateJson, Is.EqualTo("{\"kills\":9}"));
		}

		[Test]
		public void UpgradedContent_WithStableIdReconnectsExistingState()
		{
			ContentId id = ContentId.Parse("example.pack:enemy/stable-name");
			SavedContentState state = new SavedContentState(id, ContentCategory.Enemy, "{\"kills\":12}");
			ContentRegistry upgradedRegistry = new ContentRegistry();
			upgradedRegistry.Register(new ContentRegistration(id, ContentCategory.Enemy,
				"example.pack", "content/enemy-v2.json"), new ValidationReport());

			SavedContentResolution resolution = ContentSaveResolver.Resolve(state, upgradedRegistry);
			Assert.That(resolution.Status, Is.EqualTo(SavedContentStatus.Available));
			Assert.That(resolution.State.ContentId, Is.EqualTo(id));
			Assert.That(resolution.State.StateJson, Is.EqualTo("{\"kills\":12}"));
			Assert.That(resolution.Registration.Source, Is.EqualTo("content/enemy-v2.json"));
		}

		[Test]
		public void RemovedThenRestoredContent_ReconnectsPreservedState()
		{
			ContentId id = ContentId.Parse("example.pack:clothing/restored");
			SavedContentState state = new SavedContentState(id, ContentCategory.Clothing,
				"{\"unlocked\":true,\"equipped\":false}");

			SavedContentResolution removed = ContentSaveResolver.Resolve(state, new ContentRegistry());
			Assert.That(removed.Status, Is.EqualTo(SavedContentStatus.Missing));
			Assert.That(removed.State.StateJson, Is.EqualTo(state.StateJson));

			ContentRegistry restoredRegistry = new ContentRegistry();
			restoredRegistry.Register(new ContentRegistration(id, ContentCategory.Clothing,
				"example.pack", "content/clothing.json"), new ValidationReport());
			SavedContentResolution restored = ContentSaveResolver.Resolve(removed.State, restoredRegistry);
			Assert.That(restored.Status, Is.EqualTo(SavedContentStatus.Available));
			Assert.That(restored.State.StateJson, Is.EqualTo(state.StateJson));
		}
	}

}
