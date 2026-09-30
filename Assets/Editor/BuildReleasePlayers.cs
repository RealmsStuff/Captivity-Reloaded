using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildReleasePlayers
{
	private static readonly string[] Scenes = { "Assets/Scenes/Main.unity" };
	private const string ReleaseVersion = "2.1.2";

	public static void BuildAll()
	{
		BuildWindows();
		BuildAndroid();
		BuildWebGL();
	}

	public static void BuildWindows()
	{
		Build("Builds/Windows-" + ReleaseVersion + "/Captivity Reloaded.exe", BuildTarget.StandaloneWindows64);
	}

	public static void BuildAndroid()
	{
		EditorUserBuildSettings.buildAppBundle = false;
		Build("Builds/Android/Captivity-Reloaded-" + ReleaseVersion + ".apk", BuildTarget.Android);
	}

	public static void BuildWebGL()
	{
		Build("Builds/WebGL-" + ReleaseVersion, BuildTarget.WebGL);
	}

	private static void Build(string i_path, BuildTarget i_target)
	{
		string directory = System.IO.Path.GetDirectoryName(i_path);
		if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
		BuildPlayerOptions options = new BuildPlayerOptions
		{
			scenes = Scenes,
			locationPathName = i_path,
			target = i_target,
			options = BuildOptions.None
		};
		BuildReport report = BuildPipeline.BuildPlayer(options);
		if (report.summary.result != BuildResult.Succeeded)
			throw new InvalidOperationException("Player build failed: " + report.summary.result);
	}
}
