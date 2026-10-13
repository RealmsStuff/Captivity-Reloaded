using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildReleasePlayers
{
	private static readonly string[] Scenes = { "Assets/Scenes/Main.unity" };
	private const string ReleaseVersion = "2.2.1";

	public static void BuildAll()
	{
		BuildWindows();
		BuildAndroid();
		BuildWebGL();
	}

	public static void BuildWindows()
	{
		string directory = "Builds/Windows-" + ReleaseVersion;
		Build(directory + "/Captivity Reloaded.exe", BuildTarget.StandaloneWindows64);
		File.WriteAllText(System.IO.Path.Combine(directory, "README-FIRST.txt"),
			"CAPTIVITY RELOADED - WINDOWS INSTALLATION\r\n\r\n"
			+ "1. Extract the entire downloaded ZIP to a normal folder.\r\n"
			+ "2. Open that extracted folder.\r\n"
			+ "3. Run Captivity Reloaded.exe from there.\r\n\r\n"
			+ "Keep Captivity Reloaded.exe beside Captivity Reloaded_Data, "
			+ "MonoBleedingEdge, and UnityPlayer.dll. Do not run the executable "
			+ "from inside the ZIP, move it by itself, or rename it. Doing so causes "
			+ "Unity's 'cannot find data folder' error.\r\n");
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
