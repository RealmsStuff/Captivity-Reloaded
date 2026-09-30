using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using IOPath = System.IO.Path;

public static class CaptivityUnityBundlePackager
{
	private sealed class Target
	{
		public string Platform;
		public BuildTarget BuildTarget;
		public BuildTargetGroup Group;
	}

	public static CapmodPackageResult Validate(CaptivityModAuthoringProfile i_profile)
	{
		if (i_profile == null) return Error("bundle.profile", "Choose a Mod Authoring Profile.", string.Empty);
		string source = ResolveSource(i_profile.sourceDirectory);
		CapmodPackageResult result = CapmodPackageBuilder.ValidateSource(source);
		if (!result.Report.IsValid) return result;
		if (result.Manifest.AssetBundles.Count != 0)
			result.Report.Add(ValidationSeverity.Error, "bundle.generated-metadata",
				"Remove assetBundles from the source manifest; the packager generates it.", source);
		if (i_profile.prefabs == null || i_profile.prefabs.Length == 0)
			result.Report.Add(ValidationSeverity.Error, "bundle.assets", "Assign at least one prefab to the authoring profile.", AssetDatabase.GetAssetPath(i_profile));

		HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
		HashSet<string> paths = new HashSet<string>(StringComparer.Ordinal);
		foreach (GameObject prefab in i_profile.prefabs ?? new GameObject[0])
		{
			if (prefab == null)
			{
				result.Report.Add(ValidationSeverity.Error, "bundle.asset-null", "The prefab list contains an empty entry.", AssetDatabase.GetAssetPath(i_profile));
				continue;
			}
			string path = AssetDatabase.GetAssetPath(prefab);
			if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal)
				|| PrefabUtility.GetPrefabAssetType(prefab) == PrefabAssetType.NotAPrefab || !paths.Add(path))
			{
				result.Report.Add(ValidationSeverity.Error, "bundle.asset", "Every entry must be a unique prefab asset below Assets/.", path);
				continue;
			}
			CapmodPrefabDescriptor descriptor = prefab.GetComponent<CapmodPrefabDescriptor>();
			if (descriptor == null)
			{
				result.Report.Add(ValidationSeverity.Error, "bundle.descriptor", "Prefab root requires CapmodPrefabDescriptor: " + prefab.name, path);
				continue;
			}
			if (!ContentId.TryParse(descriptor.Id, out ContentId id) || id.Namespace != result.Manifest.Id
				|| !id.Path.StartsWith("prefab/", StringComparison.Ordinal) || !ids.Add(descriptor.Id))
				result.Report.Add(ValidationSeverity.Error, "bundle.prefab-id",
					"Prefab IDs must be unique and use " + result.Manifest.Id + ":prefab/...", path);

			foreach (MonoBehaviour behaviour in prefab.GetComponentsInChildren<MonoBehaviour>(true))
				if (behaviour != null && !(behaviour is CapmodPrefabDescriptor))
					result.Report.Add(ValidationSeverity.Error, "bundle.script",
						"The first prefab slice only permits CapmodPrefabDescriptor; unsupported script: "
						+ behaviour.GetType().FullName, path);
		}

		List<Target> targets = SelectedTargets(i_profile);
		if (targets.Count == 0)
			result.Report.Add(ValidationSeverity.Error, "bundle.platforms", "Select at least one bundle platform.", AssetDatabase.GetAssetPath(i_profile));
		foreach (Target target in targets)
			if (!BuildPipeline.IsBuildTargetSupported(target.Group, target.BuildTarget))
				result.Report.Add(ValidationSeverity.Error, "bundle.platform-module",
					"Unity support for " + target.Platform + " is not installed.", AssetDatabase.GetAssetPath(i_profile));
		return result;
	}

	public static CapmodPackageResult Build(CaptivityModAuthoringProfile i_profile, string i_outputPath, bool i_overwrite)
	{
		CapmodPackageResult result = Validate(i_profile);
		if (!result.Report.IsValid) return result;
		string project = ProjectRoot();
		string temporary = IOPath.Combine(project, "Temp", "CapmodPackager", Guid.NewGuid().ToString("N"));
		string staging = IOPath.Combine(temporary, "staging");
		try
		{
			CopySource(ResolveSource(i_profile.sourceDirectory), staging);
			string[] assetNames = AssetPaths(i_profile.prefabs);
			result.Manifest.AssetBundles.Clear();
			List<Target> targets = SelectedTargets(i_profile);
			for (int index = 0; index < targets.Count; index++)
			{
				Target target = targets[index];
				EditorUtility.DisplayProgressBar("Building .capmod",
					"Building " + target.Platform.ToUpperInvariant() + " bundle (" + (index + 1) + "/" + targets.Count + ")",
					(float)index / (targets.Count + 1));
				Debug.Log("[ModPackager] Building " + target.Platform + " AssetBundle (" + (index + 1) + "/" + targets.Count + ").");
				string buildDirectory = IOPath.Combine(temporary, "unity-" + target.Platform);
				Directory.CreateDirectory(buildDirectory);
				AssetBundleBuild build = new AssetBundleBuild
				{
					assetBundleName = "content.bundle",
					assetNames = assetNames
				};
				AssetBundleManifest built = BuildPipeline.BuildAssetBundles(buildDirectory, new[] { build },
					BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle
						| BuildAssetBundleOptions.StrictMode, target.BuildTarget);
				string generated = IOPath.Combine(buildDirectory, "content.bundle");
				if (built == null || !File.Exists(generated))
				{
					result.Report.Add(ValidationSeverity.Error, "bundle.build", "Unity failed to build the " + target.Platform + " AssetBundle.", buildDirectory);
					return result;
				}
				string relative = "bundles/" + target.Platform + "/content.bundle";
				string destination = IOPath.Combine(staging, relative.Replace('/', IOPath.DirectorySeparatorChar));
				Directory.CreateDirectory(IOPath.GetDirectoryName(destination));
				File.Copy(generated, destination, false);
				FileInfo file = new FileInfo(destination);
				result.Manifest.AssetBundles.Add(new ModAssetBundlePayload
				{
					Platform = target.Platform,
					Path = relative,
					SizeBytes = file.Length,
					Sha256 = Sha256(destination)
				});
			}

			EditorUtility.DisplayProgressBar("Building .capmod", "Creating final .capmod package",
				(float)targets.Count / (targets.Count + 1));
			File.WriteAllText(IOPath.Combine(staging, "manifest.json"),
				JsonConvert.SerializeObject(result.Manifest, Formatting.Indented));
			return CapmodPackageBuilder.Build(staging, i_outputPath, i_overwrite);
		}
		catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
			|| exception is ArgumentException || exception is InvalidOperationException)
		{
			result.Report.Add(ValidationSeverity.Error, "bundle.package", exception.Message, temporary);
			return result;
		}
		finally
		{
			try { if (Directory.Exists(temporary)) Directory.Delete(temporary, true); }
			catch (Exception exception) { Debug.LogWarning("[ModPackager] Could not remove temporary directory: " + exception.Message); }
			EditorUtility.ClearProgressBar();
		}
	}

	private static List<Target> SelectedTargets(CaptivityModAuthoringProfile i_profile)
	{
		List<Target> result = new List<Target>();
		if (i_profile.buildWindows) result.Add(new Target { Platform = "windows", BuildTarget = BuildTarget.StandaloneWindows64, Group = BuildTargetGroup.Standalone });
		if (i_profile.buildAndroid) result.Add(new Target { Platform = "android", BuildTarget = BuildTarget.Android, Group = BuildTargetGroup.Android });
		if (i_profile.buildWebGl) result.Add(new Target { Platform = "webgl", BuildTarget = BuildTarget.WebGL, Group = BuildTargetGroup.WebGL });
		return result;
	}

	private static string[] AssetPaths(GameObject[] i_prefabs)
	{
		string[] result = new string[i_prefabs.Length];
		for (int index = 0; index < result.Length; index++) result[index] = AssetDatabase.GetAssetPath(i_prefabs[index]);
		Array.Sort(result, StringComparer.Ordinal);
		return result;
	}

	private static void CopySource(string i_source, string i_destination)
	{
		Directory.CreateDirectory(i_destination);
		foreach (string file in Directory.GetFiles(i_source, "*", SearchOption.AllDirectories))
		{
			string relative = file.Substring(i_source.TrimEnd(IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar).Length + 1);
			if (relative.Equals("bundles", StringComparison.OrdinalIgnoreCase)
				|| relative.StartsWith("bundles" + IOPath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
			string destination = IOPath.Combine(i_destination, relative);
			Directory.CreateDirectory(IOPath.GetDirectoryName(destination));
			File.Copy(file, destination, false);
		}
	}

	private static string ResolveSource(string i_source)
	{
		if (string.IsNullOrWhiteSpace(i_source)) return string.Empty;
		return IOPath.GetFullPath(IOPath.IsPathRooted(i_source) ? i_source : IOPath.Combine(ProjectRoot(), i_source));
	}

	private static string ProjectRoot()
	{
		return IOPath.GetFullPath(IOPath.Combine(Application.dataPath, ".."));
	}

	private static string Sha256(string i_path)
	{
		using (FileStream stream = File.OpenRead(i_path))
		using (SHA256 sha = SHA256.Create())
			return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
	}

	private static CapmodPackageResult Error(string i_code, string i_message, string i_source)
	{
		CapmodPackageResult result = new CapmodPackageResult();
		result.Report.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		return result;
	}
}
