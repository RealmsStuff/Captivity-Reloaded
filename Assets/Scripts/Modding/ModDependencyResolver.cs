using System;
using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	public sealed class DependencyResolutionResult
	{
		public List<ModPack> OrderedPacks { get; } = new List<ModPack>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ModDependencyResolver
	{
		public static DependencyResolutionResult Resolve(IEnumerable<ModPack> i_packs)
		{
			DependencyResolutionResult result = new DependencyResolutionResult();
			Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
			foreach (ModPack pack in i_packs)
			{
				if (pack == null || pack.Manifest == null) continue;
				if (packs.ContainsKey(pack.Manifest.Id))
				{
					result.Report.Add(ValidationSeverity.Error, "dependency.duplicate-pack", "Pack ID is loaded more than once: " + pack.Manifest.Id);
				}
				else
				{
					packs.Add(pack.Manifest.Id, pack);
				}
			}

			Dictionary<string, int> incoming = new Dictionary<string, int>(StringComparer.Ordinal);
			Dictionary<string, List<string>> dependents = new Dictionary<string, List<string>>(StringComparer.Ordinal);
			foreach (string id in packs.Keys)
			{
				incoming[id] = 0;
				dependents[id] = new List<string>();
			}

			foreach (ModPack pack in packs.Values)
			{
				foreach (ModDependency dependency in pack.Manifest.Dependencies)
				{
					if (!packs.TryGetValue(dependency.Id, out ModPack dependencyPack))
					{
						result.Report.Add(ValidationSeverity.Error, "dependency.missing", "Pack '" + pack.Manifest.Id + "' requires missing pack '" + dependency.Id + "'.");
						continue;
					}
					if (!VersionRange.TryParse(dependency.Version, out VersionRange range) || !range.Contains(dependencyPack.Version))
					{
						result.Report.Add(ValidationSeverity.Error, "dependency.version", "Pack '" + pack.Manifest.Id + "' requires '" + dependency.Id + "' " + dependency.Version + ", but " + dependencyPack.Version + " is loaded.");
					}
					incoming[pack.Manifest.Id]++;
					dependents[dependency.Id].Add(pack.Manifest.Id);
				}
			}

			SortedSet<string> ready = new SortedSet<string>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, int> entry in incoming)
			{
				if (entry.Value == 0) ready.Add(entry.Key);
			}

			while (ready.Count > 0)
			{
				string id = ready.Min;
				ready.Remove(id);
				result.OrderedPacks.Add(packs[id]);
				dependents[id].Sort(StringComparer.Ordinal);
				foreach (string dependent in dependents[id])
				{
					incoming[dependent]--;
					if (incoming[dependent] == 0) ready.Add(dependent);
				}
			}

			if (result.OrderedPacks.Count != packs.Count)
			{
				result.Report.Add(ValidationSeverity.Error, "dependency.cycle", "A dependency cycle prevents a deterministic load order.");
			}

			return result;
		}
	}
}
