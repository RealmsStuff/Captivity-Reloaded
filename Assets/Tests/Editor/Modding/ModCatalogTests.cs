using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class ModCatalogTests
	{
		private const string ValidCatalog = "{\"schemaVersion\":1,\"packs\":[{" +
			"\"id\":\"example.test-pack\",\"displayName\":\"Test Pack\",\"authors\":[\"Tester\"]," +
			"\"summary\":\"A test listing.\",\"sourceRepository\":\"https://github.com/example/test-pack\"," +
			"\"versions\":[{\"version\":\"1.0.0\",\"modApiVersion\":1,\"gameVersion\":\">=1.0.0\"," +
			"\"download\":\"https://github.com/example/test-pack/releases/download/v1.0.0/test-pack.zip\"," +
			"\"sha256\":\"0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef\",\"sizeBytes\":1234}]}]}";

		[Test]
		public void Parse_ValidListing_PreservesMetadata()
		{
			ModCatalogParseResult result = ModCatalogParser.Parse(ValidCatalog, "test");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Catalog.Packs.Single().Id, Is.EqualTo("example.test-pack"));
			Assert.That(result.Catalog.Packs.Single().Versions.Single().Version, Is.EqualTo("1.0.0"));
		}

		[Test]
		public void Parse_AcceptsCapmodReleaseArchives()
		{
			ModCatalogParseResult result = ModCatalogParser.Parse(
				ValidCatalog.Replace("test-pack.zip", "test-pack.capmod"), "test");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Catalog.Packs.Single().Versions.Single().Download, Does.EndWith(".capmod"));
		}

		[Test]
		public void Parse_AcceptsRepositoryHostedCapmodArchives()
		{
			const string raw = "https://raw.githubusercontent.com/example/test-pack/main/release-assets/test-pack.capmod";
			ModCatalogParseResult result = ModCatalogParser.Parse(ValidCatalog.Replace(
				"https://github.com/example/test-pack/releases/download/v1.0.0/test-pack.zip", raw), "test");
			Assert.That(result.Report.IsValid, Is.True);
			Assert.That(result.Catalog.Packs.Single().Versions.Single().Download, Is.EqualTo(raw));
		}

		[Test]
		public void PlatformPolicy_UsesDiskOnWindowsAndAndroid_AndBoundsWebGlMemoryDownloads()
		{
			Assert.That(ModCatalogPlatformPolicy.UsesMemoryDownload(RuntimePlatform.WindowsPlayer), Is.False);
			Assert.That(ModCatalogPlatformPolicy.UsesMemoryDownload(RuntimePlatform.Android), Is.False);
			Assert.That(ModCatalogPlatformPolicy.UsesMemoryDownload(RuntimePlatform.WebGLPlayer), Is.True);
			Assert.That(ModCatalogPlatformPolicy.MaximumArchiveBytes(RuntimePlatform.WindowsPlayer),
				Is.EqualTo(ModCatalogParser.MaxArchiveBytes));
			Assert.That(ModCatalogPlatformPolicy.MaximumArchiveBytes(RuntimePlatform.Android),
				Is.EqualTo(ModCatalogParser.MaxArchiveBytes));
			Assert.That(ModCatalogPlatformPolicy.MaximumArchiveBytes(RuntimePlatform.WebGLPlayer),
				Is.EqualTo(32L * 1024L * 1024L));
			Assert.That(ModCatalogPlatformPolicy.MaximumBatchBytes(RuntimePlatform.WebGLPlayer),
				Is.EqualTo(64L * 1024L * 1024L));
		}

		[Test]
		public void CatalogCache_RoundTripsAndRejectsInvalidReplacementWithoutLosingGoodCopy()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-catalog-cache-" + Guid.NewGuid().ToString("N"));
			string path = Path.Combine(root, "catalog-v1.json");
			try
			{
				AssertValid(ModCatalogCache.WriteValidated(path, ValidCatalog));
				Assert.That(ModCatalogCache.TryRead(path, out ModCatalogDocument catalog, out ValidationReport read), Is.True);
				AssertValid(read);
				Assert.That(catalog.Packs.Single().Id, Is.EqualTo("example.test-pack"));
				ValidationReport rejected = ModCatalogCache.WriteValidated(path, "{broken json");
				Assert.That(rejected.IsValid, Is.False);
				Assert.That(ModCatalogCache.TryRead(path, out catalog, out read), Is.True);
				Assert.That(catalog.Packs.Single().Id, Is.EqualTo("example.test-pack"));
				File.WriteAllText(path, "{broken json");
				Assert.That(ModCatalogCache.TryRead(path, out _, out ValidationReport corrupt), Is.False);
				Assert.That(corrupt.IsValid, Is.False);
				Assert.That(ModCatalogCache.WriteValidated(null, ValidCatalog).IsValid, Is.False);
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void BundledPermanentCatalog_UsesValidatedGitHubListings()
		{
			string repository = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string path = Path.Combine(repository, "ModCatalog", "catalog-v1.json");
			ModCatalogParseResult result = ModCatalogParser.Parse(File.ReadAllText(path), path);
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Catalog.Packs.Count, Is.EqualTo(25));
			Assert.That(result.Catalog.Packs.Select(pack => pack.Id), Does.Contain("legacy.c4c"));
			Assert.That(result.Catalog.Packs.Select(pack => pack.Id), Does.Contain("legacy.captivity-multi-tool"));
			Assert.That(result.Catalog.Packs.SelectMany(pack => pack.Versions),
				Has.All.Matches<ModCatalogVersion>(version =>
					version.Download.StartsWith("https://raw.githubusercontent.com/RealmsStuff/CR-Mods/main/release-assets/",
						StringComparison.Ordinal) && version.SizeBytes > 0 && version.Sha256.Length == 64));
		}

		[Test]
		public void BundledPermanentCatalog_OverpowerPlansMissingC4CFirst()
		{
			string repository = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
			string path = Path.Combine(repository, "ModCatalog", "catalog-v1.json");
			ModCatalogParseResult result = ModCatalogParser.Parse(File.ReadAllText(path), path);
			Assert.That(result.Report.IsValid, Is.True);
			ModCatalogPack selected = result.Catalog.Packs.Single(pack => pack.Id == "legacy.overpower");
			ModCatalogVersion latest = selected.Versions.Single(version => version.Version == "1.0.0");
			ModPackStatus core = new ModPackStatus("core", "Core", "0.1.0", "core", ModPackState.Loaded, string.Empty);
			ModInstallPlan plan = ModInstallPlanner.Plan(result.Catalog, selected, latest, new[] { core }, _ => true);
			AssertValid(plan.Report);
			Assert.That(plan.Downloads.Select(choice => choice.Pack.Id),
				Is.EqualTo(new[] { "legacy.c4c", "legacy.overpower" }));
		}

		[TestCase("\"schemaVersion\":1", "\"schemaVersion\":2", "catalog.schema")]
		[TestCase("\"id\":\"example.test-pack\"", "\"id\":\"core\"", "catalog.id")]
		[TestCase("\"sizeBytes\":1234", "\"sizeBytes\":0", "catalog.archive")]
		[TestCase("\"gameVersion\":\">=1.0.0\"", "\"gameVersion\":\"soon\"", "catalog.compatibility")]
		[TestCase("https://github.com/example/test-pack/releases/download/v1.0.0/test-pack.zip", "https://evil.example/test-pack.zip", "catalog.download")]
		[TestCase("https://github.com/example/test-pack/releases/download/v1.0.0/test-pack.zip", "https://github.com/other/test-pack/releases/download/v1.0.0/test-pack.zip", "catalog.download-repository")]
		[TestCase("https://github.com/example/test-pack/releases/download/v1.0.0/test-pack.zip", "https://raw.githubusercontent.com/other/test-pack/main/release-assets/test-pack.zip", "catalog.download-repository")]
		public void Parse_RejectsInvalidListings(string i_old, string i_new, string i_code)
		{
			ModCatalogParseResult result = ModCatalogParser.Parse(ValidCatalog.Replace(i_old, i_new), "test");
			Assert.That(result.Catalog, Is.Null);
			Assert.That(result.Report.Issues.Any(issue => issue.Code == i_code), Is.True);
		}

		[Test]
		public void Parse_RejectsUnknownFieldsAndOversizedCatalogs()
		{
			Assert.That(ModCatalogParser.Parse(ValidCatalog.Replace("\"packs\":", "\"extra\":true,\"packs\":"), "test").Catalog, Is.Null);
			Assert.That(ModCatalogParser.Parse(new string('x', ModCatalogParser.MaxJsonCharacters + 1), "test").Catalog, Is.Null);
		}

		[Test]
		public void Parse_RejectsInvalidCatalogRequirementsAndConflicts()
		{
			string selfDependency = ValidCatalog.Replace("\"sizeBytes\":1234",
				"\"sizeBytes\":1234,\"dependencies\":[{\"id\":\"example.test-pack\",\"version\":\">=1.0.0\"}]");
			Assert.That(ModCatalogParser.Parse(selfDependency, "test").Report.Issues.Any(issue => issue.Code == "catalog.dependencies"), Is.True);
			string badConflict = ValidCatalog.Replace("\"sizeBytes\":1234",
				"\"sizeBytes\":1234,\"conflicts\":[\"example.test-pack\"]");
			Assert.That(ModCatalogParser.Parse(badConflict, "test").Report.Issues.Any(issue => issue.Code == "catalog.conflicts"), Is.True);
		}

		[Test]
		public void Parse_AcceptsReleasePreviewsAndRejectsForeignOrNonPngImages()
		{
			string preview = "https://github.com/example/test-pack/releases/download/v1.0.0/preview.png";
			string withPreview = ValidCatalog.Replace("\"versions\":", "\"previewImages\":[\"" + preview + "\"],\"versions\":");
			ModCatalogParseResult valid = ModCatalogParser.Parse(withPreview, "test");
			AssertValid(valid.Report);
			Assert.That(valid.Catalog.Packs.Single().PreviewImages.Single(), Is.EqualTo(preview));
			Assert.That(ModCatalogParser.Parse(withPreview.Replace(preview,
				"https://github.com/other/repository/releases/download/v1.0.0/preview.png"), "test")
				.Report.Issues.Any(issue => issue.Code == "catalog.previews"), Is.True);
			Assert.That(ModCatalogParser.Parse(withPreview.Replace("preview.png", "preview.jpg"), "test")
				.Report.Issues.Any(issue => issue.Code == "catalog.previews"), Is.True);
		}

		[Test]
		public void Parse_AcceptsRepositoryHostedPreviews()
		{
			const string preview = "https://raw.githubusercontent.com/example/test-pack/main/previews/preview.png";
			string catalog = ValidCatalog.Replace("\"versions\":", "\"previewImages\":[\"" + preview + "\"],\"versions\":");
			ModCatalogParseResult result = ModCatalogParser.Parse(catalog, "test");
			AssertValid(result.Report);
			Assert.That(result.Catalog.Packs.Single().PreviewImages.Single(), Is.EqualTo(preview));
		}

		[Test]
		public void ArchivePreflight_RejectsCorruptZipWithMatchingCatalogDigest()
		{
			string path = Path.Combine(Path.GetTempPath(), "captivity-corrupt-mod-" + Guid.NewGuid().ToString("N") + ".zip");
			try
			{
				File.WriteAllBytes(path, Encoding.UTF8.GetBytes("this is not a zip archive"));
				byte[] bytes = File.ReadAllBytes(path);
				string digest;
				using (SHA256 sha = SHA256.Create())
					digest = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
				ValidationReport report = ModArchiveValidator.Validate(path,
					new ModCatalogPack { Id = "example.test-pack" },
					new ModCatalogVersion { Version = "1.0.0", ModApiVersion = 1, SizeBytes = bytes.Length, Sha256 = digest });
				Assert.That(report.Issues.Any(issue => issue.Code == "archive.read"), Is.True);
			}
			finally { if (File.Exists(path)) File.Delete(path); }
		}

		[TestCase("content/test.json", "{}", null)]
		[TestCase("../escape.json", "{}", "archive.path")]
		[TestCase("content/../../escape.json", "{}", "archive.path")]
		[TestCase("content/CON.txt", "bad", "archive.path")]
		[TestCase("content/plugin.dll", "binary", "archive.extension")]
		public void ArchivePreflight_AcceptsDataOnlyPackAndRejectsUnsafeEntries(string i_name, string i_body, string i_error)
		{
			string path = Path.Combine(Path.GetTempPath(), "captivity-mod-archive-test-" + Guid.NewGuid().ToString("N") + ".zip");
			try
			{
				using (FileStream file = File.Create(path))
				using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create))
				{
					WriteEntry(zip, "manifest.json", "{\"schemaVersion\":1,\"id\":\"example.test-pack\",\"displayName\":\"Test Pack\",\"version\":\"1.0.0\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
					WriteEntry(zip, i_name, i_body);
				}
				byte[] bytes = File.ReadAllBytes(path);
				string digest;
				using (SHA256 sha = SHA256.Create())
					digest = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
				ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
				ModCatalogVersion version = new ModCatalogVersion
				{
					Version = "1.0.0", ModApiVersion = 1, SizeBytes = bytes.Length, Sha256 = digest
				};
				ValidationReport report = ModArchiveValidator.Validate(path, pack, version);
				if (i_error == null) Assert.That(report.IsValid, Is.True,
					string.Join("\n", report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
				else Assert.That(report.Issues.Any(issue => issue.Code == i_error), Is.True);
				version.Sha256 = new string('0', 64);
				Assert.That(ModArchiveValidator.Validate(path, pack, version).Issues.Any(issue => issue.Code == "archive.sha256"), Is.True);
			}
			finally { if (File.Exists(path)) File.Delete(path); }
		}

		[Test]
		public void Install_FirstInstallSucceedsButNeverOverwritesAnExistingPack()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-install-test-" + Guid.NewGuid().ToString("N"));
			string archive = Path.Combine(root, "pack.zip");
			string mods = Path.Combine(root, "Mods");
			try
			{
				Directory.CreateDirectory(root);
				using (FileStream file = File.Create(archive))
				using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create))
				{
					WriteEntry(zip, "manifest.json", "{\"schemaVersion\":1,\"id\":\"example.test-pack\",\"displayName\":\"Test Pack\",\"version\":\"1.0.0\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
					zip.CreateEntry("content/");
				}
				byte[] bytes = File.ReadAllBytes(archive);
				string digest;
				using (SHA256 sha = SHA256.Create())
					digest = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty);
				ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
				ModCatalogVersion version = new ModCatalogVersion
				{
					Version = "1.0.0", ModApiVersion = 1, SizeBytes = bytes.Length, Sha256 = digest
				};
				ValidationReport first = ModArchiveInstaller.Install(archive, mods, pack, version);
				Assert.That(first.IsValid, Is.True, string.Join("\n", first.Issues.Select(issue => issue.Code + ": " + issue.Message)));
				string installed = Path.Combine(mods, pack.Id, "manifest.json");
				Assert.That(File.Exists(installed), Is.True);
				ValidationReport second = ModArchiveInstaller.Install(archive, mods, pack, version);
				Assert.That(second.Issues.Any(issue => issue.Code == "install.exists"), Is.True);
				Assert.That(File.ReadAllText(installed), Does.Contain("example.test-pack"));
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Update_StagesReplacement_KeepsPreviousAndRollsBack()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-update-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				string firstZip = Path.Combine(root, "first.zip");
				string secondZip = Path.Combine(root, "second.zip");
				string thirdZip = Path.Combine(root, "third.zip");
				ModCatalogVersion first = MakeArchive(firstZip, "1.0.0");
				ModCatalogVersion second = MakeArchive(secondZip, "1.1.0");
				ModCatalogVersion third = MakeArchive(thirdZip, "1.2.0");
				AssertValid(ModArchiveInstaller.Install(firstZip, mods, pack, first));
				AssertValid(ModInstallRecovery.Update(secondZip, mods, pack, second));
				Assert.That(File.ReadAllText(Path.Combine(mods, pack.Id, "manifest.json")), Does.Contain("1.1.0"));
				Assert.That(ModInstallRecovery.BackupVersion(mods, pack.Id), Is.EqualTo("1.0.0"));
				Assert.That(ModInstallRecovery.Update(secondZip, mods, pack, second).IsValid, Is.False);
				AssertValid(ModInstallRecovery.Update(thirdZip, mods, pack, third));
				Assert.That(ModInstallRecovery.BackupVersion(mods, pack.Id), Is.EqualTo("1.1.0"));
				Assert.That(Directory.GetDirectories(Path.Combine(root, ".mod-backups"), pack.Id + ".older-*").Length, Is.EqualTo(1));
				AssertValid(ModInstallRecovery.Rollback(mods, pack.Id));
				Assert.That(File.ReadAllText(Path.Combine(mods, pack.Id, "manifest.json")), Does.Contain("1.1.0"));
				Assert.That(ModInstallRecovery.BackupVersion(mods, pack.Id), Is.EqualTo("1.2.0"));
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Update_InvalidReplacementLeavesInstalledVersionUntouched()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-update-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				string firstZip = Path.Combine(root, "first.zip");
				string secondZip = Path.Combine(root, "second.zip");
				ModCatalogVersion first = MakeArchive(firstZip, "1.0.0");
				ModCatalogVersion second = MakeArchive(secondZip, "1.1.0");
				AssertValid(ModArchiveInstaller.Install(firstZip, mods, pack, first));
				second.Sha256 = new string('0', 64);
				Assert.That(ModInstallRecovery.Update(secondZip, mods, pack, second).IsValid, Is.False);
				Assert.That(File.ReadAllText(Path.Combine(mods, pack.Id, "manifest.json")), Does.Contain("1.0.0"));
				Assert.That(ModInstallRecovery.BackupVersion(mods, pack.Id), Is.Null);
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Uninstall_MovesPackOutOfDiscovery_AndRestoreReturnsIt()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-remove-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				string archive = Path.Combine(root, "pack.zip");
				AssertValid(ModArchiveInstaller.Install(archive, mods, pack, MakeArchive(archive, "1.0.0")));
				AssertValid(ModInstallRecovery.Uninstall(mods, pack.Id));
				Assert.That(Directory.Exists(Path.Combine(mods, pack.Id)), Is.False);
				Assert.That(ModInstallRecovery.HasRemoved(mods, pack.Id), Is.True);
				Assert.That(ModInstallRecovery.Uninstall(mods, pack.Id).IsValid, Is.False);
				AssertValid(ModInstallRecovery.Restore(mods, pack.Id));
				Assert.That(File.Exists(Path.Combine(mods, pack.Id, "manifest.json")), Is.True);
				Assert.That(ModInstallRecovery.HasRemoved(mods, pack.Id), Is.False);
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Recovery_RestoresPreviousPackWhenUpdateWasInterrupted()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-recovery-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				string firstZip = Path.Combine(root, "first.zip");
				string secondZip = Path.Combine(root, "second.zip");
				AssertValid(ModArchiveInstaller.Install(firstZip, mods, pack, MakeArchive(firstZip, "1.0.0")));
				AssertValid(ModInstallRecovery.Update(secondZip, mods, pack, MakeArchive(secondZip, "1.1.0")));
				Directory.Move(Path.Combine(mods, pack.Id), Path.Combine(root, "held-new-version"));
				AssertValid(ModInstallRecovery.RecoverInterruptedUpdates(mods));
				Assert.That(File.ReadAllText(Path.Combine(mods, pack.Id, "manifest.json")), Does.Contain("1.0.0"));
				Assert.That(Directory.Exists(Path.Combine(root, "held-new-version")), Is.True);
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void DependencyPlan_OrdersDownloadsAndRejectsCyclesAndIncompatibleRanges()
		{
			ModCatalogPack root = CatalogPack("example.root", "1.0.0", "example.dep", ">=1.0.0");
			ModCatalogPack dependency = CatalogPack("example.dep", "1.2.0");
			ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
				Packs = new System.Collections.Generic.List<ModCatalogPack> { root, dependency } };
			ModInstallPlan plan = ModInstallPlanner.Plan(catalog, root, root.Versions[0], new ModPackStatus[0], _ => true);
			AssertValid(plan.Report);
			Assert.That(plan.Downloads.Select(item => item.Pack.Id), Is.EqualTo(new[] { "example.dep", "example.root" }));
			dependency.Versions[0].Dependencies.Add(new ModDependency { Id = "example.root", Version = "1.0.0" });
			Assert.That(ModInstallPlanner.Plan(catalog, root, root.Versions[0], new ModPackStatus[0], _ => true)
				.Report.Issues.Any(issue => issue.Code == "plan.cycle"), Is.True);
			dependency.Versions[0].Dependencies.Clear();
			root.Versions[0].Dependencies[0].Version = ">=2.0.0";
			Assert.That(ModInstallPlanner.Plan(catalog, root, root.Versions[0], new ModPackStatus[0], _ => true)
				.Report.Issues.Any(issue => issue.Code == "plan.version"), Is.True);
		}

		[Test]
		public void DependencyPlan_RejectsConflictsBeforeDownload()
		{
			ModCatalogPack root = CatalogPack("example.root", "1.0.0");
			root.Versions[0].Conflicts.Add("example.other");
			ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
				Packs = new System.Collections.Generic.List<ModCatalogPack> { root } };
			ModPackStatus other = new ModPackStatus("example.other", "Other", "1.0.0", "other",
				ModPackState.Loaded, string.Empty);
			ModInstallPlan plan = ModInstallPlanner.Plan(catalog, root, root.Versions[0], new[] { other }, _ => true);
			Assert.That(plan.Report.Issues.Any(issue => issue.Code == "plan.conflict"), Is.True);
		}

		[Test]
		public void DependencyTransaction_StagesAllBeforeCommit_AndRejectsUndeclaredRequirements()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "captivity-mod-dependency-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(rootPath, "Mods");
			try
			{
				Directory.CreateDirectory(rootPath);
				string dependencyZip = Path.Combine(rootPath, "dependency.zip");
				string rootZip = Path.Combine(rootPath, "root.zip");
				ModCatalogPack dependency = CatalogPack("example.dep", "1.0.0");
				dependency.Versions[0] = MakeDependencyArchive(dependencyZip, "example.dep", "1.0.0", null);
				ModCatalogPack root = CatalogPack("example.root", "1.0.0", "example.dep", ">=1.0.0");
				root.Versions[0] = MakeDependencyArchive(rootZip, "example.root", "1.0.0", "example.dep");
				root.Versions[0].Dependencies.Add(new ModDependency { Id = "example.dep", Version = ">=1.0.0" });
				ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
					Packs = new System.Collections.Generic.List<ModCatalogPack> { root, dependency } };
				ModInstallPlan plan = ModInstallPlanner.Plan(catalog, root, root.Versions[0], new ModPackStatus[0], _ => true);
				AssertValid(plan.Report);
				var archives = new System.Collections.Generic.Dictionary<string, string>
				{ ["example.dep"] = dependencyZip, ["example.root"] = rootZip };
				AssertValid(ModInstallTransaction.Apply(mods, plan, archives, new ModPack[0]));
				Assert.That(File.Exists(Path.Combine(mods, "example.dep", "manifest.json")), Is.True);
				Assert.That(File.Exists(Path.Combine(mods, "example.root", "manifest.json")), Is.True);
				AssertValid(ModInstallRecovery.Uninstall(mods, "example.root"));
				AssertValid(ModInstallRecovery.Uninstall(mods, "example.dep"));
				// A catalog that hides a manifest dependency must fail while staging, before any active folder is created.
				string secondMods = Path.Combine(rootPath, "second", "Mods");
				root.Versions[0].Dependencies.Clear();
				ModInstallPlan hiddenPlan = ModInstallPlanner.Plan(catalog, root, root.Versions[0], new ModPackStatus[0], _ => true);
				Assert.That(ModInstallTransaction.Apply(secondMods, hiddenPlan, archives, new ModPack[0]).IsValid, Is.False);
				Assert.That(Directory.Exists(Path.Combine(secondMods, "example.root")), Is.False);
			}
			finally { if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true); }
		}

		[Test]
		public void DependencyTransaction_CorruptArchiveLeavesEveryPackUninstalled()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "captivity-mod-corrupt-batch-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(rootPath, "Mods");
			try
			{
				Directory.CreateDirectory(rootPath);
				string dependencyZip = Path.Combine(rootPath, "dependency.zip");
				string rootZip = Path.Combine(rootPath, "root.zip");
				ModCatalogPack dependency = CatalogPack("example.dep", "1.0.0");
				dependency.Versions[0] = MakeDependencyArchive(dependencyZip, "example.dep", "1.0.0", null);
				ModCatalogPack selected = CatalogPack("example.root", "1.0.0", "example.dep", ">=1.0.0");
				selected.Versions[0] = MakeDependencyArchive(rootZip, "example.root", "1.0.0", "example.dep");
				selected.Versions[0].Dependencies.Add(new ModDependency { Id = "example.dep", Version = ">=1.0.0" });
				File.WriteAllText(rootZip, "corrupt after catalog metadata was prepared");
				ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
					Packs = new System.Collections.Generic.List<ModCatalogPack> { selected, dependency } };
				ModInstallPlan plan = ModInstallPlanner.Plan(catalog, selected, selected.Versions[0], new ModPackStatus[0], _ => true);
				AssertValid(plan.Report);
				var archives = new System.Collections.Generic.Dictionary<string, string>
					{ ["example.dep"] = dependencyZip, ["example.root"] = rootZip };
				Assert.That(ModInstallTransaction.Apply(mods, plan, archives, new ModPack[0]).IsValid, Is.False);
				Assert.That(Directory.Exists(Path.Combine(mods, "example.dep")), Is.False);
				Assert.That(Directory.Exists(Path.Combine(mods, "example.root")), Is.False);
			}
			finally { if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true); }
		}

		[Test]
		public void DependencyTransaction_CommitFailureRollsBackEveryMovedPack()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "captivity-mod-commit-rollback-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(rootPath, "Mods");
			try
			{
				Directory.CreateDirectory(rootPath);
				string installedZip = Path.Combine(rootPath, "installed-root.zip");
				string dependencyZip = Path.Combine(rootPath, "dependency.zip");
				string rootZip = Path.Combine(rootPath, "root.zip");
				ModCatalogPack installedCatalogPack = CatalogPack("example.root", "1.0.0");
				installedCatalogPack.Versions[0] = MakeDependencyArchive(installedZip, "example.root", "1.0.0", null);
				AssertValid(ModArchiveInstaller.Install(installedZip, mods, installedCatalogPack, installedCatalogPack.Versions[0]));
				string installedManifestPath = Path.Combine(mods, "example.root", "manifest.json");
				ManifestLoadResult installedManifest = ModManifestParser.Parse(File.ReadAllText(installedManifestPath), installedManifestPath, false);
				ModPack installedPack = new ModPack(installedManifest.Manifest, installedManifest.Version,
					Path.GetDirectoryName(installedManifestPath));
				ModPackStatus installedStatus = new ModPackStatus(installedPack, ModPackState.Loaded, string.Empty);
				ModCatalogPack dependency = CatalogPack("example.dep", "1.0.0");
				dependency.Versions[0] = MakeDependencyArchive(dependencyZip, "example.dep", "1.0.0", null);
				ModCatalogPack selected = CatalogPack("example.root", "1.1.0", "example.dep", ">=1.0.0");
				selected.Versions[0] = MakeDependencyArchive(rootZip, "example.root", "1.1.0", "example.dep");
				selected.Versions[0].Dependencies.Add(new ModDependency { Id = "example.dep", Version = ">=1.0.0" });
				ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
					Packs = new System.Collections.Generic.List<ModCatalogPack> { selected, dependency } };
				ModInstallPlan plan = ModInstallPlanner.Plan(catalog, selected, selected.Versions[0],
					new[] { installedStatus }, _ => true);
				AssertValid(plan.Report);
				// A file occupying the backup root fails the update only after the new dependency has moved.
				File.WriteAllText(Path.Combine(rootPath, ".mod-backups"), "occupied");
				var archives = new System.Collections.Generic.Dictionary<string, string>
					{ ["example.dep"] = dependencyZip, ["example.root"] = rootZip };
				ValidationReport report = ModInstallTransaction.Apply(mods, plan, archives, new[] { installedPack });
				Assert.That(report.IsValid, Is.False);
				Assert.That(Directory.Exists(Path.Combine(mods, "example.dep")), Is.False);
				Assert.That(File.ReadAllText(installedManifestPath), Does.Contain("1.0.0"));
			}
			finally { if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true); }
		}

		[Test]
		public void Integrity_RecognizesCatalogInstallAndDetectsChangedFiles()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-integrity-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			string archive = Path.Combine(root, "pack.zip");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				ModCatalogVersion version = MakeArchive(archive, "1.0.0");
				AssertValid(ModArchiveInstaller.Install(archive, mods, pack, version));
				Assert.That(ModInstallIntegrity.Check(mods, pack.Id).State, Is.EqualTo(ModIntegrityState.Unchanged));
				File.WriteAllText(Path.Combine(mods, pack.Id, "local-edit.txt"), "changed");
				ModIntegrityCheck changed = ModInstallIntegrity.Check(mods, pack.Id);
				Assert.That(changed.State, Is.EqualTo(ModIntegrityState.Modified));
				Assert.That(changed.NeedsConfirmation, Is.True);
				AssertValid(ModInstallIntegrity.PreserveModifiedCopy(mods, pack.Id));
				string modifiedRoot = Path.Combine(root, ".mod-modified");
				string copy = Directory.GetDirectories(modifiedRoot).Single();
				Assert.That(File.ReadAllText(Path.Combine(copy, "local-edit.txt")), Is.EqualTo("changed"));
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Update_RejectsUntrackedOrModifiedPackUntilPreservationIsConfirmed()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-integrity-update-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			string firstZip = Path.Combine(root, "first.zip");
			string secondZip = Path.Combine(root, "second.zip");
			ModCatalogPack pack = new ModCatalogPack { Id = "example.test-pack" };
			try
			{
				Directory.CreateDirectory(root);
				ModCatalogVersion first = MakeArchive(firstZip, "1.0.0");
				ModCatalogVersion second = MakeArchive(secondZip, "1.1.0");
				AssertValid(ModArchiveInstaller.Install(firstZip, mods, pack, first));
				File.WriteAllText(Path.Combine(mods, pack.Id, "changed.txt"), "keep me");
				ValidationReport blocked = ModInstallRecovery.Update(secondZip, mods, pack, second);
				Assert.That(blocked.Issues.Any(issue => issue.Code == "update.modified"), Is.True);
				Assert.That(File.ReadAllText(Path.Combine(mods, pack.Id, "manifest.json")), Does.Contain("1.0.0"));
				AssertValid(ModInstallRecovery.Update(secondZip, mods, pack, second, true));
				string copy = Directory.GetDirectories(Path.Combine(root, ".mod-modified")).Single();
				Assert.That(File.ReadAllText(Path.Combine(copy, "changed.txt")), Is.EqualTo("keep me"));
				Assert.That(ModInstallIntegrity.Check(mods, pack.Id).State, Is.EqualTo(ModIntegrityState.Unchanged));
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void Integrity_UntrackedManualPackRequiresConfirmation()
		{
			string root = Path.Combine(Path.GetTempPath(), "captivity-mod-untracked-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(root, "Mods");
			string pack = Path.Combine(mods, "example.test-pack");
			try
			{
				Directory.CreateDirectory(pack);
				File.WriteAllText(Path.Combine(pack, "manifest.json"),
					"{\"schemaVersion\":1,\"id\":\"example.test-pack\",\"displayName\":\"Manual\",\"version\":\"1.0.0\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
				Directory.CreateDirectory(Path.Combine(pack, "content"));
				Assert.That(ModInstallIntegrity.Check(mods, "example.test-pack").State, Is.EqualTo(ModIntegrityState.Untracked));
			}
			finally { if (Directory.Exists(root)) Directory.Delete(root, true); }
		}

		[Test]
		public void DependencyTransaction_RequiresExplicitModifiedCopyPreservation()
		{
			string rootPath = Path.Combine(Path.GetTempPath(), "captivity-mod-transaction-integrity-test-" + Guid.NewGuid().ToString("N"));
			string mods = Path.Combine(rootPath, "Mods");
			string firstZip = Path.Combine(rootPath, "first.zip");
			string secondZip = Path.Combine(rootPath, "second.zip");
			try
			{
				Directory.CreateDirectory(rootPath);
				ModCatalogPack firstPack = CatalogPack("example.root", "1.0.0");
				firstPack.Versions[0] = MakeDependencyArchive(firstZip, "example.root", "1.0.0", null);
				AssertValid(ModArchiveInstaller.Install(firstZip, mods, firstPack, firstPack.Versions[0]));
				string manifestPath = Path.Combine(mods, "example.root", "manifest.json");
				ManifestLoadResult manifest = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false);
				ModPack installedPack = new ModPack(manifest.Manifest, manifest.Version, Path.GetDirectoryName(manifestPath));
				ModPackStatus installed = new ModPackStatus(installedPack, ModPackState.Loaded, string.Empty);
				File.WriteAllText(Path.Combine(mods, "example.root", "local.txt"), "preserve");

				ModCatalogPack updatePack = CatalogPack("example.root", "1.1.0");
				updatePack.Versions[0] = MakeDependencyArchive(secondZip, "example.root", "1.1.0", null);
				ModCatalogDocument catalog = new ModCatalogDocument { SchemaVersion = 1,
					Packs = new System.Collections.Generic.List<ModCatalogPack> { updatePack } };
				ModInstallPlan plan = ModInstallPlanner.Plan(catalog, updatePack, updatePack.Versions[0], new[] { installed }, _ => true);
				AssertValid(plan.Report);
				var archives = new System.Collections.Generic.Dictionary<string, string> { ["example.root"] = secondZip };
				ValidationReport blocked = ModInstallTransaction.Apply(mods, plan, archives, new[] { installedPack });
				Assert.That(blocked.Issues.Any(issue => issue.Code == "transaction.modified"), Is.True);
				Assert.That(File.ReadAllText(manifestPath), Does.Contain("1.0.0"));
				AssertValid(ModInstallTransaction.Apply(mods, plan, archives, new[] { installedPack },
					new System.Collections.Generic.HashSet<string> { "example.root" }));
				Assert.That(File.ReadAllText(manifestPath), Does.Contain("1.1.0"));
				string copy = Directory.GetDirectories(Path.Combine(rootPath, ".mod-modified")).Single();
				Assert.That(File.ReadAllText(Path.Combine(copy, "local.txt")), Is.EqualTo("preserve"));
			}
			finally { if (Directory.Exists(rootPath)) Directory.Delete(rootPath, true); }
		}

		private static ModCatalogPack CatalogPack(string i_id, string i_version, string i_dependency = null, string i_range = null)
		{
			ModCatalogVersion version = new ModCatalogVersion { Version = i_version, ModApiVersion = 1, GameVersion = "*",
				Dependencies = new System.Collections.Generic.List<ModDependency>() };
			if (i_dependency != null) version.Dependencies.Add(new ModDependency { Id = i_dependency, Version = i_range });
			return new ModCatalogPack { Id = i_id, DisplayName = i_id,
				Versions = new System.Collections.Generic.List<ModCatalogVersion> { version } };
		}

		private static ModCatalogVersion MakeDependencyArchive(string i_path, string i_id, string i_version, string i_dependency)
		{
			string requirements = i_dependency == null ? string.Empty
				: ",\"dependencies\":[{\"id\":\"" + i_dependency + "\",\"version\":\">=1.0.0\"}]";
			using (FileStream file = File.Create(i_path))
			using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create))
			{
				WriteEntry(zip, "manifest.json", "{\"schemaVersion\":1,\"id\":\"" + i_id
					+ "\",\"displayName\":\"Test\",\"version\":\"" + i_version
					+ "\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]" + requirements + "}");
				zip.CreateEntry("content/");
			}
			byte[] bytes = File.ReadAllBytes(i_path);
			using (SHA256 sha = SHA256.Create())
				return new ModCatalogVersion { Version = i_version, ModApiVersion = 1, GameVersion = "*", SizeBytes = bytes.Length,
					Sha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty) };
		}

		private static ModCatalogVersion MakeArchive(string i_path, string i_version)
		{
			using (FileStream file = File.Create(i_path))
			using (ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create))
			{
				WriteEntry(zip, "manifest.json", "{\"schemaVersion\":1,\"id\":\"example.test-pack\",\"displayName\":\"Test Pack\",\"version\":\"" + i_version + "\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
				zip.CreateEntry("content/");
			}
			byte[] bytes = File.ReadAllBytes(i_path);
			using (SHA256 sha = SHA256.Create())
				return new ModCatalogVersion { Version = i_version, ModApiVersion = 1, SizeBytes = bytes.Length,
					Sha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty) };
		}

		private static void AssertValid(ValidationReport i_report)
		{
			Assert.That(i_report.IsValid, Is.True,
				string.Join("\n", i_report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
		}

		private static void WriteEntry(ZipArchive i_zip, string i_name, string i_body)
		{
			using (StreamWriter writer = new StreamWriter(i_zip.CreateEntry(i_name).Open(), new UTF8Encoding(false)))
				writer.Write(i_body);
		}
	}
}
