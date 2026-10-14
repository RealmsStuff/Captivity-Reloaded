using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class GameReleaseUpdateTests
	{
		private const string ValidDocument = "{\"schemaVersion\":1,\"latest\":{\"version\":\"2.3.0\",\"name\":\"Captivity Reloaded 2.3\",\"summary\":\"A tested release.\",\"releaseUrl\":\"https://github.com/RealmsStuff/Captivity-Reloaded/releases/tag/v2.3.0\",\"prerelease\":false}}";

		[Test]
		public void Parse_AcceptsTrustedReleaseDocument()
		{
			Assert.That(GameReleaseUpdate.TryParse(ValidDocument, out GameReleaseInfo release, out string error), Is.True, error);
			Assert.That(release.version, Is.EqualTo("2.3.0"));
			Assert.That(release.prerelease, Is.False);
		}

		[Test]
		public void CheckedInReleaseChannel_IsValid()
		{
			string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "release-channel-v1.json");
			Assert.That(GameReleaseUpdate.TryParse(File.ReadAllText(path), out GameReleaseInfo release, out string error), Is.True, error);
			Assert.That(release.version, Is.Not.Empty);
		}

		[Test]
		public void IsNewerThanInstalled_UsesSemanticVersionOrdering()
		{
			GameReleaseUpdate.TryParse(ValidDocument, out GameReleaseInfo release, out _);
			Assert.That(GameReleaseUpdate.IsNewerThanInstalled(release, "2.2.1"), Is.True);
			Assert.That(GameReleaseUpdate.IsNewerThanInstalled(release, "2.3.0"), Is.False);
			Assert.That(GameReleaseUpdate.IsNewerThanInstalled(release, "2.4.0"), Is.False);
		}

		[TestCase("http://github.com/RealmsStuff/Captivity-Reloaded/releases/tag/v2.3.0")]
		[TestCase("https://github.com/OtherOwner/Captivity-Reloaded/releases/tag/v2.3.0")]
		[TestCase("https://example.com/RealmsStuff/Captivity-Reloaded/releases/tag/v2.3.0")]
		[TestCase("https://github.com/RealmsStuff/Captivity-Reloaded/archive/v2.3.0.zip")]
		public void Parse_RejectsUntrustedReleaseUrl(string i_url)
		{
			string json = ValidDocument.Replace("https://github.com/RealmsStuff/Captivity-Reloaded/releases/tag/v2.3.0", i_url);
			Assert.That(GameReleaseUpdate.TryParse(json, out _, out string error), Is.False);
			Assert.That(error, Does.Contain("release URL"));
		}

		[Test]
		public void Parse_RejectsOversizedDocuments()
		{
			Assert.That(GameReleaseUpdate.TryParse(new string('x', GameReleaseUpdate.MaximumDocumentCharacters + 1), out _, out string error), Is.False);
			Assert.That(error, Does.Contain("too large"));
		}
	}
}
