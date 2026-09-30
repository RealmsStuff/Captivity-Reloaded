using System;
using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	public enum ModPackState { Loaded, Disabled, Conflicting, Invalid }

	public sealed class ModPackStatus
	{
		public string Id { get; }
		public string DisplayName { get; }
		public string Version { get; }
		public string RootPath { get; }
		public ModPackState State { get; }
		public string Reason { get; }
		public ModPack Pack { get; }
		public bool IsUserConfigurable => Pack != null && Id != "core";

		public ModPackStatus(ModPack i_pack, ModPackState i_state, string i_reason)
			: this(i_pack?.Manifest?.Id, i_pack?.Manifest?.DisplayName, i_pack?.Manifest?.Version, i_pack?.RootPath, i_state, i_reason, i_pack) { }

		public ModPackStatus(string i_id, string i_displayName, string i_version, string i_rootPath,
			ModPackState i_state, string i_reason, ModPack i_pack = null)
		{
			Id = i_id ?? string.Empty;
			DisplayName = string.IsNullOrWhiteSpace(i_displayName) ? Id : i_displayName;
			Version = i_version ?? string.Empty;
			RootPath = i_rootPath ?? string.Empty;
			State = i_state;
			Reason = i_reason ?? string.Empty;
			Pack = i_pack;
		}
	}

	public sealed class DependencyResolutionResult
	{
		public List<ModPack> OrderedPacks { get; } = new List<ModPack>();
		public List<ModPackStatus> Statuses { get; } = new List<ModPackStatus>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModDependencyResolver
	{
		public static DependencyResolutionResult Resolve(IEnumerable<ModPack> i_packs, Func<string, bool> i_isEnabled = null)
		{
			DependencyResolutionResult result = new DependencyResolutionResult();
			Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
			HashSet<string> duplicateIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (ModPack pack in i_packs ?? new ModPack[0])
			{
				if (pack?.Manifest == null) continue;
				if (packs.ContainsKey(pack.Manifest.Id))
				{
					string reason = "Another installed pack already uses ID '" + pack.Manifest.Id + "'.";
					result.Report.Add(ValidationSeverity.Error, "dependency.duplicate-pack", reason, pack.RootPath);
					result.Statuses.Add(new ModPackStatus(pack, ModPackState.Invalid, reason));
					duplicateIds.Add(pack.Manifest.Id);
				}
				else packs.Add(pack.Manifest.Id, pack);
			}

			Dictionary<string, ModPackState> states = new Dictionary<string, ModPackState>(StringComparer.Ordinal);
			Dictionary<string, string> reasons = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (string id in duplicateIds)
			{
				states[id] = ModPackState.Invalid;
				reasons[id] = "Multiple installed packs use this ID.";
			}
			foreach (ModPack pack in packs.Values)
				if (pack.Manifest.Id != "core" && i_isEnabled != null && !i_isEnabled(pack.Manifest.Id))
				{
					states[pack.Manifest.Id] = ModPackState.Disabled;
					reasons[pack.Manifest.Id] = "Disabled by the user. Enable it in Mods and restart the game.";
				}

			ResolveConflicts(packs, states, reasons, result.Report);
			ResolveRequiredDependencies(packs, states, reasons, result.Report);

			Dictionary<string, int> incoming = new Dictionary<string, int>(StringComparer.Ordinal);
			Dictionary<string, List<string>> dependents = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			foreach (string id in packs.Keys)
				if (!states.ContainsKey(id)) { incoming[id] = 0; dependents[id] = new List<string>(); }

			foreach (ModPack pack in packs.Values)
			{
				string id = pack.Manifest.Id;
				if (states.ContainsKey(id)) continue;
				foreach (ModDependency dependency in pack.Manifest.Dependencies ?? new List<ModDependency>()) AddEdge(dependency.Id, id, incoming, dependents);
				foreach (ModDependency dependency in pack.Manifest.OptionalDependencies ?? new List<ModDependency>())
				{
					if (!packs.TryGetValue(dependency.Id, out ModPack optional))
					{
						result.Report.Add(ValidationSeverity.Info, "dependency.optional-missing", "Optional dependency '" + dependency.Id + "' for pack '" + id + "' is not installed.", pack.RootPath);
						continue;
					}
					if (states.ContainsKey(dependency.Id))
					{
						result.Report.Add(ValidationSeverity.Info, "dependency.optional-disabled", "Optional dependency '" + dependency.Id + "' for pack '" + id + "' is disabled; optional integration will not load.", pack.RootPath);
						continue;
					}
					if (!VersionRange.TryParse(dependency.Version, out VersionRange optionalRange) || !optionalRange.Contains(optional.Version))
					{
						result.Report.Add(ValidationSeverity.Warning, "dependency.optional-version", "Optional dependency '" + dependency.Id + "' for pack '" + id + "' needs " + dependency.Version + ", but " + optional.Version + " is installed; optional integration will not load.", pack.RootPath);
						continue;
					}
					AddEdge(dependency.Id, id, incoming, dependents);
				}
				foreach (string after in pack.Manifest.LoadAfter ?? new List<string>()) AddEdge(after, id, incoming, dependents);
				foreach (string before in pack.Manifest.LoadBefore ?? new List<string>()) AddEdge(id, before, incoming, dependents);
			}

			SortedSet<string> ready = new SortedSet<string>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, int> entry in incoming) if (entry.Value == 0) ready.Add(entry.Key);
			while (ready.Count > 0)
			{
				string id = PickReady(ready, packs);
				ready.Remove(id);
				result.OrderedPacks.Add(packs[id]);
				dependents[id].Sort(StringComparer.Ordinal);
				foreach (string dependent in dependents[id]) if (--incoming[dependent] == 0) ready.Add(dependent);
			}

			foreach (string id in incoming.Keys)
				if (!ContainsPack(result.OrderedPacks, id))
				{
					states[id] = ModPackState.Invalid;
					reasons[id] = "A dependency or ordering cycle prevents this pack from loading.";
					result.Report.Add(ValidationSeverity.Error, "dependency.cycle", "Pack '" + id + "' is part of a dependency or ordering cycle.", packs[id].RootPath);
				}

			foreach (ModPack pack in packs.Values)
				if (states.TryGetValue(pack.Manifest.Id, out ModPackState state)) result.Statuses.Add(new ModPackStatus(pack, state, reasons[pack.Manifest.Id]));
				else result.Statuses.Add(new ModPackStatus(pack, ModPackState.Loaded, "Loaded successfully."));
			result.Statuses.Sort((left, right) => string.CompareOrdinal(left.Id, right.Id));
			return result;
		}

		private static void ResolveRequiredDependencies(Dictionary<string, ModPack> i_packs,
			Dictionary<string, ModPackState> io_states, Dictionary<string, string> io_reasons, ValidationReport io_report)
		{
			bool changed;
			do
			{
				changed = false;
				foreach (ModPack pack in i_packs.Values)
				{
					string id = pack.Manifest.Id;
					if (io_states.ContainsKey(id)) continue;
					foreach (ModDependency dependency in pack.Manifest.Dependencies ?? new List<ModDependency>())
					{
						if (!i_packs.TryGetValue(dependency.Id, out ModPack dependencyPack))
						{
							Disable(id, "Required dependency '" + dependency.Id + "' is not installed.", "dependency.missing", ValidationSeverity.Error, pack.RootPath, io_states, io_reasons, io_report);
							changed = true; break;
						}
						if (io_states.TryGetValue(dependency.Id, out ModPackState dependencyState))
						{
							Disable(id, "Required dependency '" + dependency.Id + "' is " + StateLabel(dependencyState) + ". Enable or repair that dependency, then restart.", "dependency.disabled", ValidationSeverity.Warning, pack.RootPath, io_states, io_reasons, io_report);
							changed = true; break;
						}
						if (!VersionRange.TryParse(dependency.Version, out VersionRange range) || !range.Contains(dependencyPack.Version))
						{
							Disable(id, "Required dependency '" + dependency.Id + "' needs " + dependency.Version + ", but " + dependencyPack.Version + " is installed.", "dependency.version", ValidationSeverity.Error, pack.RootPath, io_states, io_reasons, io_report);
							changed = true; break;
						}
					}
				}
			}
			while (changed);
		}

		private static void ResolveConflicts(Dictionary<string, ModPack> i_packs,
			Dictionary<string, ModPackState> io_states, Dictionary<string, string> io_reasons, ValidationReport io_report)
		{
			List<string> ids = new List<string>(i_packs.Keys); ids.Sort(StringComparer.Ordinal);
			foreach (string id in ids)
			{
				ModPack pack = i_packs[id];
				if (io_states.ContainsKey(id)) continue;
				foreach (string conflict in pack.Manifest.Conflicts ?? new List<string>())
				{
					if (!i_packs.TryGetValue(conflict, out ModPack other) || io_states.ContainsKey(conflict)) continue;
					ModPack loser = id == "core" ? other : conflict == "core" ? pack : pack.Manifest.Priority == other.Manifest.Priority
						? (string.CompareOrdinal(id, conflict) > 0 ? pack : other)
						: (pack.Manifest.Priority < other.Manifest.Priority ? pack : other);
					ModPack winner = loser == pack ? other : pack;
					string selection = winner.Manifest.Id == "core" ? "Core always wins conflicts"
						: pack.Manifest.Priority == other.Manifest.Priority ? "equal priorities were resolved by pack ID"
						: "priority " + winner.Manifest.Priority + " beats priority " + loser.Manifest.Priority;
					string reason = "Conflicts with '" + winner.Manifest.Id + "' (" + winner.Manifest.DisplayName
						+ "); " + selection + ". " + (winner.Manifest.Id == "core"
							? "Remove this conflict or mod to load it." : "Disable '" + winner.Manifest.Id + "' and restart to load this mod.");
					io_states[loser.Manifest.Id] = ModPackState.Conflicting;
					io_reasons[loser.Manifest.Id] = reason;
					io_report.Add(ValidationSeverity.Warning, "dependency.conflict", "Disabled pack '" + loser.Manifest.Id + "': " + reason, loser.RootPath);
				}
			}
		}

		private static void Disable(string i_id, string i_reason, string i_code, ValidationSeverity i_severity, string i_source,
			Dictionary<string, ModPackState> io_states, Dictionary<string, string> io_reasons, ValidationReport io_report)
		{
			io_states[i_id] = ModPackState.Disabled;
			io_reasons[i_id] = i_reason;
			io_report.Add(i_severity, i_code, "Disabled pack '" + i_id + "': " + i_reason, i_source);
		}

		private static void AddEdge(string i_first, string i_second, Dictionary<string, int> io_incoming, Dictionary<string, List<string>> io_dependents)
		{
			if (!io_incoming.ContainsKey(i_first) || !io_incoming.ContainsKey(i_second) || io_dependents[i_first].Contains(i_second)) return;
			io_incoming[i_second]++;
			io_dependents[i_first].Add(i_second);
		}

		private static bool ContainsPack(IEnumerable<ModPack> i_packs, string i_id) { foreach (ModPack pack in i_packs) if (pack.Manifest.Id == i_id) return true; return false; }
		private static string StateLabel(ModPackState i_state) { return i_state == ModPackState.Conflicting ? "disabled by a conflict" : i_state == ModPackState.Invalid ? "invalid" : "disabled"; }

		private static string PickReady(SortedSet<string> i_ready, Dictionary<string, ModPack> i_packs)
		{
			string best = null;
			foreach (string id in i_ready)
				if (best == null || i_packs[id].Manifest.Priority < i_packs[best].Manifest.Priority ||
					(i_packs[id].Manifest.Priority == i_packs[best].Manifest.Priority && string.CompareOrdinal(id, best) < 0)) best = id;
			return best;
		}
	}
}
