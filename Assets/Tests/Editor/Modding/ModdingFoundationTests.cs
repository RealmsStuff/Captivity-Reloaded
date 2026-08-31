using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public class ContentIdTests
	{
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
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:enemy/gremlin")).LegacyId, Is.EqualTo(12));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:stage/field-day")).LegacyId, Is.EqualTo(6));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/weapon/pistol")).LegacyName, Is.EqualTo("Pistol"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/usable/morphine")).LegacyName, Is.EqualTo("Morphine"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:item/consumable/ammo-box")).LegacyName, Is.EqualTo("Ammo Box"));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:clothing/hazmat-suit")).LegacyId, Is.EqualTo(78));
			Assert.That(result.Entries.Single(entry => entry.Id == ContentId.Parse("core:clothing/lingerie-white-lower")).LegacyId, Is.EqualTo(97));
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
}
