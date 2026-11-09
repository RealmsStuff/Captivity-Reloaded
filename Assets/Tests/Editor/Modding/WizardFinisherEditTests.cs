using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaptivityReloaded.Modding.Tests
{
	// The New Enemy Wizard's "paired interaction" step edits an enemy's first finisher. On an enemy that already had a
	// behavior block it saved nothing at all while saying it had (assigning the block back to the enemy JSON stored a
	// copy, so the edits went to a detached object). And it rebuilt the finisher from only the fields it shows, so once
	// it does save, everything else the finisher had would go: attachment, startFreezeSeconds, phaseBlendSeconds,
	// statuses, and each phase's reactions, struggle and blendSeconds. It now saves and keeps them.
	// The wizard lives in the editor assembly, reached by reflection.
	public sealed class WizardFinisherEditTests
	{
		private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
		private string m_pack;
		private string m_backup;

		[SetUp]
		public void SetUp()
		{
			m_pack = Path.Combine(Path.GetTempPath(), "cr-wizard-finisher-" + Guid.NewGuid().ToString("N"));
			CopyDirectory(Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ExampleMods", "prey-green-zombie")), m_pack);
		}

		[TearDown]
		public void TearDown()
		{
			if (Directory.Exists(m_pack)) Directory.Delete(m_pack, true);
			// The wizard backs the enemy file up under ModAuthoringBackups/<pack>/<enemy>/finisher-<time>/.
			if (string.IsNullOrEmpty(m_backup)) return;
			string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "ModAuthoringBackups"));
			string directory = Path.GetDirectoryName(m_backup);
			while (directory != null && Path.GetFullPath(directory).StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
				&& !Path.GetFileName(directory).StartsWith("finisher-", StringComparison.Ordinal))
				directory = Path.GetDirectoryName(directory);
			if (directory == null || !Path.GetFileName(directory).StartsWith("finisher-", StringComparison.Ordinal)) return;
			Directory.Delete(directory, true);
			for (string parent = Path.GetDirectoryName(directory); parent != null && !string.Equals(Path.GetFullPath(parent), root, StringComparison.OrdinalIgnoreCase)
				&& Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any(); parent = Path.GetDirectoryName(parent))
				Directory.Delete(parent);
		}

		private static void CopyDirectory(string i_from, string i_to)
		{
			Directory.CreateDirectory(i_to);
			foreach (string file in Directory.GetFiles(i_from)) File.Copy(file, Path.Combine(i_to, Path.GetFileName(file)));
			foreach (string directory in Directory.GetDirectories(i_from)) CopyDirectory(directory, Path.Combine(i_to, Path.GetFileName(directory)));
		}

		private string EnemyPath => Path.Combine(m_pack, "content", "original-green-stalker.json");
		private static JObject Finisher(JObject i_enemy) => i_enemy["behavior"]["modules"].OfType<JObject>().First(i_module => (string)i_module["type"] == "downedFinisher");

		[Test]
		public void EditingAFinisher_KeepsTheFieldsTheWizardDoesNotShow()
		{
			// The example's finisher, with the fields the wizard has no controls for. Its phases use the one enemy clip
			// the wizard can list (an animationRefs clip), so saving changes nothing it shows.
			JObject enemy = JObject.Parse(File.ReadAllText(EnemyPath));
			JObject finisher = Finisher(enemy);
			finisher["startFreezeSeconds"] = 0.5;
			finisher["phaseBlendSeconds"] = 0.2;
			finisher["attachment"] = JObject.Parse("{ 'target': 'spine', 'offsetY': 0.25, 'rootX': -0.5, 'rootY': -0.6, 'followRotation': 0.8, 'settleSeconds': 0.4 }");
			JArray phases = (JArray)finisher["phases"];
			phases[0]["animation"] = "move";
			phases[0]["blendSeconds"] = 0.3;
			phases[0]["reactions"] = JObject.Parse("{ 'oral': true, 'camera': 'enemyHips' }");
			phases[1]["animation"] = "move";
			phases[1]["struggle"] = false;
			// Their own clips are then unused, and only clips an attack or finisher uses may carry events.
			foreach (string clip in new[] { "finisher", "finisher-intense" }) ((JObject)enemy["animation"]["clips"][clip]).Remove("events");
			File.WriteAllText(EnemyPath, enemy.ToString());
			CapmodPackageResult fixture = CapmodPackageBuilder.ValidateSource(m_pack);
			Assert.That(fixture.Report.IsValid, Is.True, "Fixture: " + string.Join("\n", fixture.Report.Issues.Select(i_issue => i_issue.ToString())));

			Type wizardType = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType("CaptivityReloaded.Editor.Modding.NewEnemyWizardWindow", false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(wizardType, Is.Not.Null);
			Object wizard = ScriptableObject.CreateInstance(wizardType);
			try
			{
				wizardType.GetField("m_modFolder", Instance).SetValue(wizard, m_pack);
				wizardType.GetField("m_slug", Instance).SetValue(wizard, "original-green-stalker");
				wizardType.GetMethod("RefreshFinisherChoices", Instance).Invoke(wizard, null);
				Assert.That(wizardType.GetField("m_statusType", Instance).GetValue(wizard).ToString(), Is.EqualTo("Info"), (string)wizardType.GetField("m_status", Instance).GetValue(wizard));
				// The author changes the trigger range in the wizard and saves.
				wizardType.GetField("m_finisherTriggerRange", Instance).SetValue(wizard, 2.5f);
				wizardType.GetMethod("ConfigureFinisher", Instance).Invoke(wizard, null);
				string status = (string)wizardType.GetField("m_status", Instance).GetValue(wizard);
				int backup = status.IndexOf("Backup: ", StringComparison.Ordinal);
				if (backup >= 0) m_backup = status.Substring(backup + 8).Trim();
				Assert.That(wizardType.GetField("m_statusType", Instance).GetValue(wizard).ToString(), Is.EqualTo("Info"), status);
			}
			finally { Object.DestroyImmediate(wizard); }

			JObject saved = Finisher(JObject.Parse(File.ReadAllText(EnemyPath)));
			Assert.That((float?)saved["triggerRange"], Is.EqualTo(2.5f), "The wizard said it configured the finisher, so the change must be in the file.");
			Assert.That((float?)saved["startFreezeSeconds"], Is.EqualTo(0.5f), "startFreezeSeconds");
			Assert.That((float?)saved["phaseBlendSeconds"], Is.EqualTo(0.2f), "phaseBlendSeconds");
			Assert.That((string)saved["attachment"]?["target"], Is.EqualTo("spine"), "attachment");
			Assert.That(saved["statuses"]?["active"]?["title"]?.ToString(), Is.EqualTo("Rape"), "statuses");
			JArray savedPhases = (JArray)saved["phases"];
			Assert.That(savedPhases.Select(i_phase => (string)i_phase["id"]), Is.EqualTo(new[] { "restraint", "drain" }));
			Assert.That((bool?)savedPhases[0]["reactions"]?["oral"], Is.True, "phase reactions");
			Assert.That((float?)savedPhases[0]["blendSeconds"], Is.EqualTo(0.3f), "phase blendSeconds");
			Assert.That((bool?)savedPhases[1]["struggle"], Is.False, "phase struggle");
		}
	}
}
