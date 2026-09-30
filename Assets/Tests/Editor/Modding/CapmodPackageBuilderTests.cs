using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class CapmodPackageBuilderTests
	{
		private string m_root;

		[SetUp]
		public void SetUp()
		{
			m_root = Path.Combine(Path.GetTempPath(), "captivity-capmod-" + Guid.NewGuid().ToString("N"));
			Directory.CreateDirectory(Path.Combine(m_root, "source", "content"));
			File.WriteAllText(Path.Combine(m_root, "source", "manifest.json"),
				"{\"schemaVersion\":1,\"id\":\"example.packager-test\",\"displayName\":\"Packager Test\","
				+ "\"version\":\"1.0.0\",\"modApiVersion\":1,\"contentRoots\":[\"content\"]}");
			File.WriteAllText(Path.Combine(m_root, "source", "content", "difficulty.json"),
				"{\"schemaVersion\":1,\"type\":\"difficulty\",\"id\":\"example.packager-test:difficulty/test\","
				+ "\"displayName\":\"Packager Test\",\"enemyHealthMultiplier\":1,"
				+ "\"playerDamageTakenMultiplier\":1,\"escapeStrengthMultiplier\":1}");
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(m_root)) Directory.Delete(m_root, true);
		}

		[Test]
		public void Build_CreatesInstallCompatibleCapmodWithRootManifest()
		{
			string output = Path.Combine(m_root, "example.capmod");
			CapmodPackageResult result = CapmodPackageBuilder.Build(Path.Combine(m_root, "source"), output);
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
			Assert.That(File.Exists(output), Is.True);
			Assert.That(result.Sha256, Has.Length.EqualTo(64));
			using (FileStream stream = File.OpenRead(output))
			using (ZipArchive zip = new ZipArchive(stream, ZipArchiveMode.Read))
				Assert.That(zip.Entries.Select(entry => entry.FullName), Is.EqualTo(new[]
				{
					"content/difficulty.json", "manifest.json"
				}));
		}

		[Test]
		public void ValidateSource_RejectsUnitySourceFilesInDataOnlySlice()
		{
			File.WriteAllText(Path.Combine(m_root, "source", "content", "actor.prefab"), "not a prefab");
			CapmodPackageResult result = CapmodPackageBuilder.ValidateSource(Path.Combine(m_root, "source"));
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "capmod.extension"), Is.True);
		}

		[Test]
		public void Build_AcceptsDeclaredBundleAndVerifiesItsDigest()
		{
			string bundleDirectory = Path.Combine(m_root, "source", "bundles", "windows");
			Directory.CreateDirectory(bundleDirectory);
			string bundle = Path.Combine(bundleDirectory, "content.bundle");
			File.WriteAllBytes(bundle, new byte[] { 1, 2, 3, 4, 5 });
			string digest;
			using (SHA256 sha = SHA256.Create())
				digest = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(bundle))).Replace("-", string.Empty).ToLowerInvariant();
			string manifestPath = Path.Combine(m_root, "source", "manifest.json");
			string manifest = File.ReadAllText(manifestPath).Replace("\"contentRoots\":[\"content\"]}",
				"\"contentRoots\":[\"content\"],\"assetBundles\":[{\"platform\":\"windows\","
				+ "\"path\":\"bundles/windows/content.bundle\",\"sha256\":\"" + digest + "\",\"sizeBytes\":5}]}");
			File.WriteAllText(manifestPath, manifest);

			CapmodPackageResult result = CapmodPackageBuilder.Build(Path.Combine(m_root, "source"),
				Path.Combine(m_root, "bundled.capmod"));
			Assert.That(result.Report.IsValid, Is.True, Issues(result));
		}

		[Test]
		public void ValidateSource_RejectsUndeclaredBundle()
		{
			string bundleDirectory = Path.Combine(m_root, "source", "bundles", "windows");
			Directory.CreateDirectory(bundleDirectory);
			File.WriteAllBytes(Path.Combine(bundleDirectory, "content.bundle"), new byte[] { 1 });
			CapmodPackageResult result = CapmodPackageBuilder.ValidateSource(Path.Combine(m_root, "source"));
			Assert.That(result.Report.Issues.Any(issue => issue.Code == "capmod.bundle-undeclared"), Is.True);
		}

		[Test]
		public void Build_RequiresCapmodExtensionAndDoesNotOverwriteByDefault()
		{
			string wrong = Path.Combine(m_root, "example.zip");
			Assert.That(CapmodPackageBuilder.Build(Path.Combine(m_root, "source"), wrong).Report.Issues
				.Any(issue => issue.Code == "capmod.output-extension"), Is.True);
			string output = Path.Combine(m_root, "example.capmod");
			File.WriteAllText(output, "existing");
			Assert.That(CapmodPackageBuilder.Build(Path.Combine(m_root, "source"), output).Report.Issues
				.Any(issue => issue.Code == "capmod.output-exists"), Is.True);
			Assert.That(File.ReadAllText(output), Is.EqualTo("existing"));
		}

		[Test]
		public void LocalImport_InspectsPackageAndRequiresExplicitReplacement()
		{
			string mods = Path.Combine(m_root, "Mods");
			Directory.CreateDirectory(mods);
			string archive = Path.Combine(mods, "example.capmod");
			Assert.That(CapmodPackageBuilder.Build(Path.Combine(m_root, "source"), archive).Report.IsValid, Is.True);
			LocalCapmodArchive local = LocalCapmodInstaller.Inspect(archive);
			Assert.That(local.Report.IsValid, Is.True, string.Join("\n", local.Report.Issues));
			Assert.That(local.Manifest.Id, Is.EqualTo("example.packager-test"));
			Assert.That(LocalCapmodInstaller.FindArchives(mods), Is.EqualTo(new[] { archive }));
			Directory.CreateDirectory(Path.Combine(mods, local.Manifest.Id));
			ValidationReport rejected = LocalCapmodInstaller.Install(local, mods, false);
			Assert.That(rejected.Issues.Any(issue => issue.Code == "local.confirm"), Is.True);
			Assert.That(File.Exists(archive), Is.True);
		}

		[Test]
		public void LocalImport_FindsLegacyFolderByManifestId()
		{
			string mods = Path.Combine(m_root, "Mods");
			string legacy = Path.Combine(mods, "old-folder-name");
			Directory.CreateDirectory(legacy);
			File.Copy(Path.Combine(m_root, "source", "manifest.json"), Path.Combine(legacy, "manifest.json"));
			Assert.That(LocalCapmodInstaller.FindInstalledFolder(mods, "example.packager-test"), Is.EqualTo(legacy));
		}

		[Test]
		public void LocalImport_ReplacesSameVersionLegacyFolderAndKeepsRollbackCopy()
		{
			string mods = Path.Combine(m_root, "Mods");
			string legacy = Path.Combine(mods, "old-folder-name");
			Directory.CreateDirectory(Path.Combine(legacy, "content"));
			File.Copy(Path.Combine(m_root, "source", "manifest.json"), Path.Combine(legacy, "manifest.json"));
			File.Copy(Path.Combine(m_root, "source", "content", "difficulty.json"),
				Path.Combine(legacy, "content", "difficulty.json"));
			string archive = Path.Combine(mods, "example.capmod");
			Assert.That(CapmodPackageBuilder.Build(Path.Combine(m_root, "source"), archive).Report.IsValid, Is.True);
			LocalCapmodArchive local = LocalCapmodInstaller.Inspect(archive);
			ValidationReport installed = LocalCapmodInstaller.Install(local, mods, true, Array.Empty<ModPack>());
			Assert.That(installed.IsValid, Is.True, string.Join("\n", installed.Issues));
			Assert.That(Directory.Exists(legacy), Is.False);
			Assert.That(File.Exists(Path.Combine(mods, "example.packager-test", "manifest.json")), Is.True);
			Assert.That(File.Exists(Path.Combine(m_root, ".mod-backups", "example.packager-test", "manifest.json")), Is.True);
			Assert.That(File.Exists(archive), Is.False);
			Assert.That(Directory.GetFiles(Path.Combine(m_root, ".mod-imported"), "*.capmod"), Has.Length.EqualTo(1));
		}

		[Test]
		public void LocalImport_RejectsMalformedArchiveWithoutExtracting()
		{
			string mods = Path.Combine(m_root, "Mods");
			Directory.CreateDirectory(mods);
			string archive = Path.Combine(mods, "broken.capmod");
			File.WriteAllText(archive, "not a ZIP");
			LocalCapmodArchive local = LocalCapmodInstaller.Inspect(archive);
			Assert.That(local.Report.IsValid, Is.False);
			Assert.That(LocalCapmodInstaller.Install(local, mods, false).IsValid, Is.False);
			Assert.That(Directory.GetDirectories(mods), Is.Empty);
		}

		private static string Issues(CapmodPackageResult i_result)
		{
			return string.Join("\n", i_result.Report.Issues.Select(issue => issue.ToString()));
		}
	}
}
