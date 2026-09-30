using System;
using System.IO;
using CaptivityReloaded.Modding;
using UnityEditor;
using UnityEngine;

public sealed class CaptivityModPackagerWindow : EditorWindow
{
	private const string SourcePreference = "CaptivityReloaded.ModPackager.Source";
	private string m_sourceDirectory = string.Empty;
	private Vector2 m_scroll;
	private CapmodPackageResult m_result;
	private CaptivityModAuthoringProfile m_profile;

	[MenuItem("Captivity Reloaded/Modding/Captivity Mod Packager")]
	private static void Open()
	{
		GetWindow<CaptivityModPackagerWindow>("Mod Packager");
	}

	private void OnEnable()
	{
		m_sourceDirectory = EditorPrefs.GetString(SourcePreference, DefaultSourceDirectory());
		minSize = new Vector2(560f, 420f);
	}

	private void OnGUI()
	{
		EditorGUILayout.LabelField("Captivity Mod Packager", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Package a loose JSON/Tiled mod, or select a Unity authoring profile "
			+ "to build platform AssetBundles inside one .capmod file.", MessageType.Info);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Mod source folder");
		EditorGUILayout.BeginHorizontal();
		m_sourceDirectory = EditorGUILayout.TextField(m_sourceDirectory);
		if (GUILayout.Button("Browse...", GUILayout.Width(90f))) BrowseSource();
		EditorGUILayout.EndHorizontal();
		m_profile = (CaptivityModAuthoringProfile)EditorGUILayout.ObjectField("Unity authoring profile", m_profile,
			typeof(CaptivityModAuthoringProfile), false);

		EditorGUILayout.Space();
		EditorGUILayout.BeginHorizontal();
		GUI.enabled = !string.IsNullOrWhiteSpace(m_sourceDirectory);
		if (GUILayout.Button("Validate Mod", GUILayout.Height(30f))) ValidateMod();
		if (GUILayout.Button("Build .capmod", GUILayout.Height(30f))) BuildMod();
		if (GUILayout.Button("Build Unity .capmod", GUILayout.Height(30f))) BuildUnityMod();
		GUI.enabled = true;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		DrawResult();
	}

	private void BuildUnityMod()
	{
		m_result = CaptivityUnityBundlePackager.Validate(m_profile);
		if (!m_result.Report.IsValid) { Repaint(); return; }
		string fileName = m_result.Manifest.Id + "-" + m_result.Manifest.Version + CapmodPackageBuilder.Extension;
		string output = EditorUtility.SaveFilePanel("Build Unity-authored Captivity mod", ProjectRoot(), fileName, "capmod");
		if (string.IsNullOrEmpty(output)) return;
		bool overwrite = File.Exists(output);
		if (overwrite && !EditorUtility.DisplayDialog("Replace packaged mod?",
			"A file already exists at:\n" + output + "\n\nReplace it?", "Replace", "Cancel")) return;
		m_result = CaptivityUnityBundlePackager.Build(m_profile, output, overwrite);
		if (m_result.Report.IsValid)
		{
			EditorUtility.RevealInFinder(m_result.OutputPath);
			Debug.Log("[ModPackager] Built Unity-authored mod " + m_result.OutputPath);
		}
		Repaint();
	}

	private void BrowseSource()
	{
		string selected = EditorUtility.OpenFolderPanel("Choose a mod folder", m_sourceDirectory, string.Empty);
		if (string.IsNullOrEmpty(selected)) return;
		m_sourceDirectory = selected;
		EditorPrefs.SetString(SourcePreference, selected);
		m_result = null;
	}

	private void ValidateMod()
	{
		EditorPrefs.SetString(SourcePreference, m_sourceDirectory);
		m_result = CapmodPackageBuilder.ValidateSource(m_sourceDirectory);
		Repaint();
	}

	private void BuildMod()
	{
		ValidateMod();
		if (m_result == null || !m_result.Report.IsValid) return;
		string fileName = m_result.Manifest.Id + "-" + m_result.Manifest.Version + CapmodPackageBuilder.Extension;
		string output = EditorUtility.SaveFilePanel("Build Captivity mod", ProjectRoot(), fileName, "capmod");
		if (string.IsNullOrEmpty(output)) return;
		bool overwrite = File.Exists(output);
		if (overwrite && !EditorUtility.DisplayDialog("Replace packaged mod?",
			"A file already exists at:\n" + output + "\n\nReplace it?", "Replace", "Cancel")) return;
		m_result = CapmodPackageBuilder.Build(m_sourceDirectory, output, overwrite);
		if (m_result.Report.IsValid)
		{
			EditorUtility.RevealInFinder(m_result.OutputPath);
			Debug.Log("[ModPackager] Built " + m_result.OutputPath + " (" + m_result.PackageBytes
				+ " bytes, SHA-256 " + m_result.Sha256 + ")");
		}
		Repaint();
	}

	private void DrawResult()
	{
		if (m_result == null)
		{
			EditorGUILayout.HelpBox("Choose a mod folder, then validate it.", MessageType.None);
			return;
		}
		MessageType type = m_result.Report.IsValid ? MessageType.Info : MessageType.Error;
		string heading = m_result.Report.IsValid
			? "Valid mod: " + m_result.Manifest.DisplayName + " " + m_result.Manifest.Version
			: "The mod cannot be packaged until the errors below are fixed.";
		EditorGUILayout.HelpBox(heading, type);
		if (!string.IsNullOrEmpty(m_result.OutputPath))
			EditorGUILayout.SelectableLabel(m_result.OutputPath, EditorStyles.textField, GUILayout.Height(18f));

		m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
		if (m_result.Report.Issues.Count == 0) EditorGUILayout.LabelField("No validation issues.");
		foreach (ValidationIssue issue in m_result.Report.Issues)
		{
			MessageType issueType = issue.Severity == ValidationSeverity.Error ? MessageType.Error
				: issue.Severity == ValidationSeverity.Warning ? MessageType.Warning : MessageType.Info;
			EditorGUILayout.HelpBox(issue.Code + ": " + issue.Message
				+ (string.IsNullOrEmpty(issue.Source) ? string.Empty : "\n" + issue.Source), issueType);
		}
		EditorGUILayout.EndScrollView();
	}

	private static string DefaultSourceDirectory()
	{
		string root = ProjectRoot();
		string examples = System.IO.Path.Combine(root, "ExampleMods");
		if (!Directory.Exists(examples)) return root;
		string[] candidates = Directory.GetDirectories(examples);
		Array.Sort(candidates, StringComparer.Ordinal);
		foreach (string candidate in candidates)
			if (File.Exists(System.IO.Path.Combine(candidate, "manifest.json"))) return candidate;
		return examples;
	}

	private static string ProjectRoot()
	{
		return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, ".."));
	}
}
