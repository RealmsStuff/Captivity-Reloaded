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
	}

	public class ClothingDefinitionParserTests
	{
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
		public void Parse_RejectsNonPistolTemplatesUntilTheirSlotsArePublished()
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
	}

	public class ModContentDiscoveryTests
	{
		[Test]
		public void ConvertedFemboyShirtExample_DiscoversAndFitsItsAtlas()
		{
			string examples = Path.GetFullPath(Path.Combine(Application.dataPath, "../ExampleMods"));
			ModDiscoveryResult packs = ModDiscovery.Discover(examples);
			Assert.That(packs.Report.IsValid, Is.True);
			ModPack femboy = packs.Packs.Single(pack => pack.Manifest.Id == "mousai.femboy-refitted-shirt");
			ModContentDiscoveryResult content = ModContentDiscovery.Discover(new[] { femboy });
			Assert.That(content.Report.IsValid, Is.True);
			Assert.That(content.Clothing, Has.Count.EqualTo(1));
			ClothingDefinition clothing = content.Clothing.Single();
			Assert.That(clothing.Extends, Is.EqualTo(ContentId.Parse("core:clothing/shirt-default")));
			Assert.That(clothing.Visual.Regions, Has.Count.EqualTo(3));
			Assert.That(clothing.UnlockedByDefault, Is.True);

			Texture2D atlas = new Texture2D(2, 2);
			try
			{
				Assert.That(atlas.LoadImage(File.ReadAllBytes(Path.Combine(femboy.RootPath, clothing.Visual.Atlas))), Is.True);
				Assert.That(atlas.width, Is.EqualTo(64));
				Assert.That(atlas.height, Is.EqualTo(32));
				foreach (AtlasRegionDefinition region in clothing.Visual.Regions.Values)
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
			Assert.That(content.Enemies, Has.Count.EqualTo(1));
			EnemyDefinition enemy = content.Enemies.Single();
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
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Enemy), Is.EqualTo(21));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Stage), Is.EqualTo(7));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Clothing), Is.EqualTo(98));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Item), Is.EqualTo(33));
			Assert.That(result.Entries.Count(entry => entry.Category == ContentCategory.Challenge), Is.EqualTo(79));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:enemy/gremlin")).LegacyId, Is.EqualTo(12));
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
	}
}
