using System;
using System.Collections.Generic;
using System.IO;

namespace CaptivityReloaded.Modding
{
	public sealed class ModInstallChoice
	{
		public ModCatalogPack Pack { get; }
		public ModCatalogVersion Version { get; }
		public bool IsUpdate { get; }
		public string CurrentVersion { get; }

		public ModInstallChoice(ModCatalogPack i_pack, ModCatalogVersion i_version, bool i_isUpdate,
			string i_currentVersion)
		{ Pack = i_pack; Version = i_version; IsUpdate = i_isUpdate; CurrentVersion = i_currentVersion; }
	}

	public sealed class ModInstallPlan
	{
		public List<ModInstallChoice> Downloads { get; } = new List<ModInstallChoice>();
		public List<string> SatisfiedRequirements { get; } = new List<string>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	/// <summary>Resolves version-specific catalog requirements before any network or filesystem work.</summary>
	public static class ModInstallPlanner
	{
		public static ModInstallPlan Plan(ModCatalogDocument i_catalog, ModCatalogPack i_root,
			ModCatalogVersion i_rootVersion, IEnumerable<ModPackStatus> i_installed,
			Func<ModCatalogVersion, bool> i_isCompatible)
		{
			ModInstallPlan plan = new ModInstallPlan();
			if (i_catalog?.Packs == null || i_root == null || i_rootVersion == null || i_isCompatible == null)
			{ Error(plan, "plan.input", "Catalog, selected pack, version, and compatibility check are required."); return plan; }
			Dictionary<string, ModCatalogPack> catalog = new Dictionary<string, ModCatalogPack>(StringComparer.Ordinal);
			foreach (ModCatalogPack pack in i_catalog.Packs) if (pack != null && !catalog.ContainsKey(pack.Id)) catalog.Add(pack.Id, pack);
			Dictionary<string, ModPackStatus> installed = new Dictionary<string, ModPackStatus>(StringComparer.Ordinal);
			foreach (ModPackStatus status in i_installed ?? new ModPackStatus[0])
				if (status != null && !installed.ContainsKey(status.Id)) installed.Add(status.Id, status);
			if (!catalog.ContainsKey(i_root.Id) || !i_isCompatible(i_rootVersion))
			{ Error(plan, "plan.compatibility", "The selected release is not compatible with this game or platform."); return plan; }
			if (installed.TryGetValue(i_root.Id, out ModPackStatus rootStatus)
				&& (rootStatus.Pack == null || rootStatus.State == ModPackState.Invalid
					|| !SemanticVersion.TryParse(rootStatus.Version, out SemanticVersion rootInstalled)
					|| Parse(i_rootVersion.Version).CompareTo(rootInstalled) <= 0))
			{ Error(plan, "plan.installed-version", "Installed '" + i_root.Id + "' cannot be safely updated to this release."); return plan; }

			Dictionary<string, ModCatalogVersion> chosen = new Dictionary<string, ModCatalogVersion>(StringComparer.Ordinal)
			{ [i_root.Id] = i_rootVersion };
			Dictionary<string, List<ModDependency>> constraints = null;
			bool settled = false;
			for (int iteration = 0; iteration < 64; iteration++)
			{
				constraints = new Dictionary<string, List<ModDependency>>(StringComparer.Ordinal);
				foreach (KeyValuePair<string, ModCatalogVersion> entry in chosen)
					foreach (ModDependency dependency in entry.Value.Dependencies ?? new List<ModDependency>())
					{
						if (!constraints.TryGetValue(dependency.Id, out List<ModDependency> list))
							constraints[dependency.Id] = list = new List<ModDependency>();
						list.Add(dependency);
						if (!catalog.ContainsKey(dependency.Id) && !installed.ContainsKey(dependency.Id))
							Error(plan, "plan.missing", "Required mod '" + dependency.Id + "' is neither installed nor in the catalog.");
					}
				if (!plan.Report.IsValid) return plan;
				Dictionary<string, ModCatalogVersion> next = new Dictionary<string, ModCatalogVersion>(StringComparer.Ordinal)
				{ [i_root.Id] = i_rootVersion };
				foreach (KeyValuePair<string, List<ModDependency>> requirement in constraints)
				{
					if (requirement.Key == i_root.Id)
						continue; // A cycle is reported with its full path below.
					if (installed.TryGetValue(requirement.Key, out ModPackStatus present)
						&& SemanticVersion.TryParse(present.Version, out SemanticVersion installedVersion)
						&& Satisfies(installedVersion, requirement.Value))
					{
						if (present.State != ModPackState.Loaded)
							Error(plan, "plan.disabled", "Required mod '" + requirement.Key + "' is installed but " + present.State + ". Enable or repair it first.");
						else plan.SatisfiedRequirements.Add(requirement.Key + " v" + present.Version + " (already installed)");
						continue;
					}
					if (!catalog.TryGetValue(requirement.Key, out ModCatalogPack candidatePack))
					{ Error(plan, "plan.version", "No catalog release can satisfy requirements for '" + requirement.Key + "'."); continue; }
					ModCatalogVersion best = null;
					SemanticVersion bestVersion = default;
					foreach (ModCatalogVersion candidate in candidatePack.Versions ?? new List<ModCatalogVersion>())
						if (candidate != null && i_isCompatible(candidate)
							&& SemanticVersion.TryParse(candidate.Version, out SemanticVersion parsed)
							&& Satisfies(parsed, requirement.Value)
							&& (best == null || parsed.CompareTo(bestVersion) > 0))
						{ best = candidate; bestVersion = parsed; }
					if (best == null)
					{
						Error(plan, "plan.version", "No compatible release of '" + requirement.Key + "' satisfies " + Ranges(requirement.Value) + ".");
						continue;
					}
					if (installed.TryGetValue(requirement.Key, out present))
					{
						if (present.State == ModPackState.Invalid || present.Pack == null
							|| !SemanticVersion.TryParse(present.Version, out installedVersion)
							|| bestVersion.CompareTo(installedVersion) <= 0)
						{
							Error(plan, "plan.installed-version", "Installed '" + requirement.Key + "' cannot be safely updated to satisfy " + Ranges(requirement.Value) + ".");
							continue;
						}
					}
					next[requirement.Key] = best;
				}
				if (!plan.Report.IsValid) return plan;
				if (SameChoices(chosen, next)) { chosen = next; settled = true; break; }
				chosen = next;
				plan.SatisfiedRequirements.Clear();
			}
			if (!settled)
			{ Error(plan, "plan.unstable", "Dependency version selection did not converge."); return plan; }
			if (constraints.TryGetValue(i_root.Id, out List<ModDependency> rootRequirements)
				&& !Satisfies(Parse(i_rootVersion.Version), rootRequirements))
				Error(plan, "plan.version", "Selected version does not satisfy a dependency back onto itself: " + Ranges(rootRequirements) + ".");
			if (!plan.Report.IsValid) return plan;

			HashSet<string> visiting = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
			Visit(i_root.Id, chosen, catalog, installed, visiting, visited, new List<string>(), plan);
			if (!plan.Report.IsValid) return plan;
			CheckConflicts(chosen, installed, plan);
			return plan;
		}

		private static void Visit(string i_id, Dictionary<string, ModCatalogVersion> i_chosen,
			Dictionary<string, ModCatalogPack> i_catalog, Dictionary<string, ModPackStatus> i_installed,
			HashSet<string> io_visiting, HashSet<string> io_visited, List<string> io_path, ModInstallPlan io_plan)
		{
			if (io_visiting.Contains(i_id))
			{ Error(io_plan, "plan.cycle", "Required dependency cycle: " + string.Join(" -> ", io_path) + " -> " + i_id + "."); return; }
			if (io_visited.Contains(i_id) || !i_chosen.TryGetValue(i_id, out ModCatalogVersion version)) return;
			io_visiting.Add(i_id); io_path.Add(i_id);
			foreach (ModDependency dependency in version.Dependencies ?? new List<ModDependency>())
				Visit(dependency.Id, i_chosen, i_catalog, i_installed, io_visiting, io_visited, io_path, io_plan);
			io_path.RemoveAt(io_path.Count - 1); io_visiting.Remove(i_id); io_visited.Add(i_id);
			if (io_plan.Report.IsValid)
				io_plan.Downloads.Add(new ModInstallChoice(i_catalog[i_id], version, i_installed.ContainsKey(i_id),
					i_installed.TryGetValue(i_id, out ModPackStatus present) ? present.Version : null));
		}

		private static void CheckConflicts(Dictionary<string, ModCatalogVersion> i_chosen,
			Dictionary<string, ModPackStatus> i_installed, ModInstallPlan io_plan)
		{
			foreach (KeyValuePair<string, ModCatalogVersion> entry in i_chosen)
				foreach (string conflict in entry.Value.Conflicts ?? new List<string>())
					if (i_chosen.ContainsKey(conflict) || (i_installed.TryGetValue(conflict, out ModPackStatus other)
						&& other.State == ModPackState.Loaded))
						Error(io_plan, "plan.conflict", "'" + entry.Key + "' conflicts with '" + conflict + "'. Disable or remove the conflicting mod first.");
			foreach (ModPackStatus status in i_installed.Values)
				if (status.State == ModPackState.Loaded && !i_chosen.ContainsKey(status.Id))
					foreach (string conflict in status.Pack?.Manifest?.Conflicts ?? new List<string>())
						if (i_chosen.ContainsKey(conflict))
							Error(io_plan, "plan.conflict", "Installed '" + status.Id + "' conflicts with '" + conflict + "'. Disable or remove it first.");
		}

		private static bool SameChoices(Dictionary<string, ModCatalogVersion> i_left, Dictionary<string, ModCatalogVersion> i_right)
		{
			if (i_left.Count != i_right.Count) return false;
			foreach (KeyValuePair<string, ModCatalogVersion> entry in i_left)
				if (!i_right.TryGetValue(entry.Key, out ModCatalogVersion other) || other.Version != entry.Value.Version) return false;
			return true;
		}

		private static bool Satisfies(SemanticVersion i_version, List<ModDependency> i_requirements)
		{
			foreach (ModDependency requirement in i_requirements)
				if (!VersionRange.TryParse(requirement.Version, out VersionRange range) || !range.Contains(i_version)) return false;
			return true;
		}

		private static SemanticVersion Parse(string i_text)
		{ SemanticVersion.TryParse(i_text, out SemanticVersion version); return version; }

		private static string Ranges(List<ModDependency> i_requirements)
		{
			List<string> ranges = new List<string>();
			foreach (ModDependency requirement in i_requirements) ranges.Add(requirement.Version);
			return string.Join(" and ", ranges);
		}

		private static void Error(ModInstallPlan io_plan, string i_code, string i_message)
		{ io_plan.Report.Add(ValidationSeverity.Error, i_code, i_message); }
	}

	/// <summary>Stages every release first, then commits the batch with rollback on filesystem errors.</summary>
	public static class ModInstallTransaction
	{
		private sealed class MoveRecord
		{
			public string Id;
			public string Staged;
			public string Target;
			public string Backup;
			public string ArchivedBackup;
			public bool Archived;
			public bool MovedCurrent;
			public bool MovedNew;
		}

		public static ValidationReport Apply(string i_modsDirectory, ModInstallPlan i_plan,
			IReadOnlyDictionary<string, string> i_archives, IReadOnlyList<ModPack> i_loadedPacks,
			ISet<string> i_preserveModifiedIds = null)
		{
			ValidationReport report = new ValidationReport();
			if (i_plan == null || !i_plan.Report.IsValid || i_archives == null || i_plan.Downloads.Count == 0)
			{ Error(report, "transaction.input", "A valid nonempty install plan and downloaded archives are required."); return report; }
			string mods = null;
			foreach (ModInstallChoice choice in i_plan.Downloads)
			{
				if (!ModInstallRecovery.TryPaths(i_modsDirectory, choice.Pack.Id, out string resolvedMods,
					out string target, out string backup, out string removed) || (mods != null && mods != resolvedMods)
					|| !i_archives.ContainsKey(choice.Pack.Id))
				{ Error(report, "transaction.path", "Unsafe pack target or missing archive for '" + choice.Pack.Id + "'."); return report; }
				mods = resolvedMods;
			}
			string transactionRoot = Path.Combine(Directory.GetParent(mods).FullName, ".mod-transaction-" + Guid.NewGuid().ToString("N"));
			string stagingMods = Path.Combine(transactionRoot, "Mods");
			List<MoveRecord> moves = new List<MoveRecord>();
			bool cleanup = true;
			try
			{
				Dictionary<string, ModPack> available = new Dictionary<string, ModPack>(StringComparer.Ordinal);
				foreach (ModPack pack in i_loadedPacks ?? new ModPack[0]) if (pack?.Manifest != null) available[pack.Manifest.Id] = pack;
				foreach (ModInstallChoice choice in i_plan.Downloads)
				{
					List<ModPack> dependencies = new List<ModPack>(available.Values);
					report.Merge(ModArchiveInstaller.Install(i_archives[choice.Pack.Id], stagingMods,
						choice.Pack, choice.Version, dependencies, false));
					if (!report.IsValid) return report;
					string staged = Path.Combine(stagingMods, choice.Pack.Id);
					string manifestPath = Path.Combine(staged, "manifest.json");
					ManifestLoadResult manifest = ModManifestParser.Parse(File.ReadAllText(manifestPath), manifestPath, false);
					report.Merge(manifest.Report);
					if (!report.IsValid) return report;
					available[choice.Pack.Id] = new ModPack(manifest.Manifest, manifest.Version, staged);
				}
				// Recheck every destination after staging, before the first installed pack moves.
				foreach (ModInstallChoice choice in i_plan.Downloads)
				{
					if (!ModInstallRecovery.TryPaths(mods, choice.Pack.Id, out string resolvedMods,
						out string target, out string backup, out string removed))
					{ Error(report, "transaction.changed", "Target path became unsafe for '" + choice.Pack.Id + "'."); return report; }
					if (Directory.Exists(removed) || File.Exists(removed)
						|| (choice.IsUpdate && (ModInstallRecovery.ReadVersion(target, choice.Pack.Id) != choice.CurrentVersion
							|| File.Exists(backup) || (Directory.Exists(backup) && ModInstallRecovery.ReadVersion(backup, choice.Pack.Id) == null)))
						|| (!choice.IsUpdate && (Directory.Exists(target) || File.Exists(target))))
					{ Error(report, "transaction.changed", "Installed state changed or recovery storage is occupied for '" + choice.Pack.Id + "'."); return report; }
					if (choice.IsUpdate && ModInstallIntegrity.Check(mods, choice.Pack.Id).NeedsConfirmation
						&& (i_preserveModifiedIds == null || !i_preserveModifiedIds.Contains(choice.Pack.Id)))
					{ Error(report, "transaction.modified", "Local files changed for '" + choice.Pack.Id + "'; explicit preservation confirmation is required."); return report; }
				}
				foreach (ModInstallChoice choice in i_plan.Downloads)
					if (choice.IsUpdate && i_preserveModifiedIds != null && i_preserveModifiedIds.Contains(choice.Pack.Id))
					{
						report.Merge(ModInstallIntegrity.PreserveModifiedCopy(mods, choice.Pack.Id));
						if (!report.IsValid) return report;
					}
				Directory.CreateDirectory(mods);
				foreach (ModInstallChoice choice in i_plan.Downloads)
				{
					if (!ModInstallRecovery.TryPaths(mods, choice.Pack.Id, out string resolvedMods,
						out string target, out string backup, out string removed))
						throw new InvalidDataException("Target path became unsafe for '" + choice.Pack.Id + "'.");
					MoveRecord move = new MoveRecord
					{
						Id = choice.Pack.Id, Staged = Path.Combine(stagingMods, choice.Pack.Id),
						Target = target, Backup = backup,
						ArchivedBackup = backup + ".older-" + Guid.NewGuid().ToString("N")
					};
					moves.Add(move);
					if (choice.IsUpdate)
					{
						Directory.CreateDirectory(Path.GetDirectoryName(backup));
						if (Directory.Exists(backup))
						{ Directory.Move(backup, move.ArchivedBackup); move.Archived = true; }
						Directory.Move(target, backup); move.MovedCurrent = true;
					}
					Directory.Move(move.Staged, target); move.MovedNew = true;
				}
				foreach (ModInstallChoice choice in i_plan.Downloads)
					report.Merge(ModInstallIntegrity.Record(mods, choice.Pack.Id,
						choice.Version.Version, choice.Version.Sha256));
			}
			catch (Exception exception) when (RecoverableException(exception))
			{ Error(report, "transaction.failed", exception.Message); }
			finally
			{
				if (!report.IsValid)
				{
					for (int index = moves.Count - 1; index >= 0; index--)
					{
						MoveRecord move = moves[index];
						try
						{
							if (move.MovedNew && ModInstallRecovery.SafeDirectory(move.Target))
								Directory.Move(move.Target, move.Staged);
							if (move.MovedCurrent && !Directory.Exists(move.Target) && ModInstallRecovery.SafeDirectory(move.Backup))
								Directory.Move(move.Backup, move.Target);
							if (move.Archived && !Directory.Exists(move.Backup) && ModInstallRecovery.SafeDirectory(move.ArchivedBackup))
								Directory.Move(move.ArchivedBackup, move.Backup);
						}
						catch (Exception exception) when (RecoverableException(exception))
						{ cleanup = false; Error(report, "transaction.recovery", "Manual recovery may be needed for '" + move.Id + "': " + exception.Message); }
					}
				}
				if (cleanup && ModInstallRecovery.SafeDirectory(transactionRoot))
				{
					try { Directory.Delete(transactionRoot, true); }
					catch (Exception exception) when (RecoverableException(exception))
					{ Error(report, "transaction.cleanup", "Staged files remain at " + transactionRoot + ": " + exception.Message); }
				}
			}
			return report;
		}

		private static bool RecoverableException(Exception i_exception)
		{
			return i_exception is IOException || i_exception is UnauthorizedAccessException
				|| i_exception is InvalidDataException || i_exception is ArgumentException || i_exception is NotSupportedException;
		}

		private static void Error(ValidationReport io_report, string i_code, string i_message)
		{ io_report.Add(ValidationSeverity.Error, i_code, i_message); }
	}
}
