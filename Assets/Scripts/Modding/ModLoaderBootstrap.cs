using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class ModLoaderRuntime
	{
		public static ContentRegistry Registry { get; private set; } = new ContentRegistry();
		public static AssetSlotRegistry AssetSlots { get; private set; } = new AssetSlotRegistry();
		public static IReadOnlyList<ModPack> LoadedPacks { get; private set; } = new ModPack[0];
		public static IReadOnlyList<CoreContentCatalogEntry> CoreContentCatalog { get; private set; } = new CoreContentCatalogEntry[0];
		public static LegacyContentMap LegacyContentMap { get; private set; } = new LegacyContentMap(null);
		public static IReadOnlyList<AssetPatchDefinition> AssetPatches { get; private set; } = new AssetPatchDefinition[0];
		public static ValidationReport LastReport { get; private set; } = new ValidationReport();

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			Registry = new ContentRegistry();
			AssetSlots = new AssetSlotRegistry();
			ValidationReport report = new ValidationReport();
			List<ModPack> packs = new List<ModPack>();
			CoreContentCatalog = new CoreContentCatalogEntry[0];
			LegacyContentMap = new LegacyContentMap(null);
			AssetPatches = new AssetPatchDefinition[0];

			TextAsset coreManifestAsset = Resources.Load<TextAsset>("Modding/Core/manifest");
			if (coreManifestAsset == null)
			{
				report.Add(ValidationSeverity.Error, "bootstrap.core-missing", "Packaged Core manifest could not be loaded.");
			}
			else
			{
				ManifestLoadResult core = ModManifestParser.Parse(coreManifestAsset.text, "core/manifest.json", i_isCore: true);
				report.Merge(core.Report);
				if (core.Report.IsValid) packs.Add(new ModPack(core.Manifest, core.Version, "core"));
			}

			TextAsset coreCatalogAsset = Resources.Load<TextAsset>("Modding/Core/catalog");
			if (coreCatalogAsset == null)
			{
				report.Add(ValidationSeverity.Error, "bootstrap.catalog-missing", "Packaged Core content catalog could not be loaded.");
			}
			else
			{
				CoreContentCatalogLoadResult catalog = CoreContentCatalogParser.Parse(coreCatalogAsset.text, "core/catalog.json");
				report.Merge(catalog.Report);
				if (catalog.Report.IsValid)
				{
					CoreContentCatalog = catalog.Entries;
					LegacyContentMap = new LegacyContentMap(catalog.Entries);
				}
			}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			string projectOrPlayerRoot = Directory.GetParent(Application.dataPath)?.FullName;
			if (!string.IsNullOrEmpty(projectOrPlayerRoot))
			{
				ModDiscoveryResult discovery = ModDiscovery.Discover(Path.Combine(projectOrPlayerRoot, "Mods"));
				report.Merge(discovery.Report);
				packs.AddRange(discovery.Packs);
			}
#else
			report.Add(ValidationSeverity.Info, "bootstrap.external-disabled", "External mod discovery is disabled on this platform in Mod API v1.");
#endif

			DependencyResolutionResult dependencies = ModDependencyResolver.Resolve(packs);
			report.Merge(dependencies.Report);
			LoadedPacks = dependencies.OrderedPacks;
			AssetPatchDiscoveryResult assetPatches = AssetPatchDiscovery.Discover(LoadedPacks);
			report.Merge(assetPatches.Report);
			AssetPatches = assetPatches.Definitions;
			LastReport = report;

			foreach (ValidationIssue issue in report.Issues)
			{
				if (issue.Severity == ValidationSeverity.Error) Debug.LogError("[ModLoader] " + issue);
				else if (issue.Severity == ValidationSeverity.Warning) Debug.LogWarning("[ModLoader] " + issue);
				else Debug.Log("[ModLoader] " + issue);
			}

			Debug.Log("[ModLoader] Validation complete. Packs=" + LoadedPacks.Count + ", registry entries=" + Registry.Count + ", valid=" + report.IsValid + ".");
		}
	}
}
