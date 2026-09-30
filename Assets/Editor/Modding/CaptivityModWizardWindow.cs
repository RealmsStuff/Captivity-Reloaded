using System;
using System.Collections.Generic;
using System.IO;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed class CaptivityModWizardWindow : EditorWindow
	{
		[SerializeField] private string m_packId = "yourname.my-mod";
		[SerializeField] private string m_displayName = "My Mod";
		[SerializeField] private string m_author = "Your name";
		[SerializeField] private string m_description = "";
		[SerializeField] private string m_parentDirectory = "ExampleMods";
		private string m_message;

		[MenuItem("Captivity Reloaded/Modding/Create Mod...")]
		public static void Open()
		{
			CaptivityModWizardWindow window = GetWindow<CaptivityModWizardWindow>();
			window.titleContent = new GUIContent("Create Mod");
			window.minSize = new Vector2(440f, 290f);
			window.Show();
		}

		private void OnGUI()
		{
			EditorGUILayout.Space(8f);
			EditorGUILayout.LabelField("New Captivity mod", EditorStyles.boldLabel);
			EditorGUILayout.HelpBox("Creates a loose JSON mod and a Unity authoring profile. Add content to the content folder, then use Captivity Mod Packager to export a .capmod. Bundle prefabs are optional.", MessageType.Info);
			m_packId = EditorGUILayout.TextField("Pack ID", m_packId);
			m_displayName = EditorGUILayout.TextField("Display name", m_displayName);
			m_author = EditorGUILayout.TextField("Author", m_author);
			m_description = EditorGUILayout.TextField("Description", m_description);
			using (new EditorGUILayout.HorizontalScope())
			{
				m_parentDirectory = EditorGUILayout.TextField("Create inside", m_parentDirectory);
				if (GUILayout.Button("Browse...", GUILayout.Width(80f)))
				{
					string selected = EditorUtility.OpenFolderPanel("Choose a folder inside the Unity project", ProjectRoot(), "");
					if (!string.IsNullOrEmpty(selected)) m_parentDirectory = selected;
				}
			}
			EditorGUILayout.LabelField("Folder: " + m_packId, EditorStyles.miniLabel);
			if (!string.IsNullOrEmpty(m_message)) EditorGUILayout.HelpBox(m_message, MessageType.Warning);
			if (GUILayout.Button("Create mod", GUILayout.Height(30f))) CreateMod();
		}

		private static string ProjectRoot() { return FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..")); }

		private void CreateMod()
		{
			try
			{
				if (!ContentId.IsValidNamespace(m_packId) || m_packId == "core") throw new InvalidDataException("Use a unique lowercase pack ID, for example yourname.my-mod.");
				if (string.IsNullOrWhiteSpace(m_displayName)) throw new InvalidDataException("Display name is required.");
				string project = ProjectRoot();
				string parent = FilePath.GetFullPath(FilePath.IsPathRooted(m_parentDirectory) ? m_parentDirectory : FilePath.Combine(project, m_parentDirectory));
				if (!parent.StartsWith(project + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Choose a folder inside this Unity project.");
				string root = FilePath.GetFullPath(FilePath.Combine(parent, m_packId));
				if (!root.StartsWith(parent + FilePath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || Directory.Exists(root) || File.Exists(root)) throw new IOException("That mod folder already exists. Nothing was replaced.");
				string profileDirectory = "Assets/ModAuthoring/" + m_packId;
				if (AssetDatabase.IsValidFolder(profileDirectory)) throw new IOException("An authoring profile folder already exists for this pack ID.");
				ModManifest manifest = new ModManifest
				{
					SchemaVersion = 1, Id = m_packId, DisplayName = m_displayName.Trim(), Version = "1.0.0", ModApiVersion = 1,
					Description = m_description.Trim(), Authors = new List<string> { string.IsNullOrWhiteSpace(m_author) ? "Unknown" : m_author.Trim() },
					ContentRoots = new List<string> { "content" }
				};
				Directory.CreateDirectory(FilePath.Combine(root, "content"));
				Directory.CreateDirectory(FilePath.Combine(root, "assets"));
				File.WriteAllText(FilePath.Combine(root, "manifest.json"), JsonConvert.SerializeObject(manifest, Formatting.Indented));
				if (!AssetDatabase.IsValidFolder("Assets/ModAuthoring")) AssetDatabase.CreateFolder("Assets", "ModAuthoring");
				AssetDatabase.CreateFolder("Assets/ModAuthoring", m_packId);
				CaptivityModAuthoringProfile profile = CreateInstance<CaptivityModAuthoringProfile>();
				profile.sourceDirectory = root;
				AssetDatabase.CreateAsset(profile, profileDirectory + "/ModProfile.asset");
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Selection.activeObject = profile;
				EditorGUIUtility.PingObject(profile);
				m_message = "Created " + root + ". The authoring profile is selected in the Project window.";
			}
			catch (Exception exception) { m_message = exception.Message; }
		}
	}
}
