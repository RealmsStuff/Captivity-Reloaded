using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameDisplayMode
{
	BorderedFullscreen,
	Fullscreen,
	BorderlessFullscreen,
	Windowed
}

public struct GameDisplayResolution
{
	public int Width;

	public int Height;

	public int RefreshRate;

	public GameDisplayResolution(int i_width, int i_height, int i_refreshRate)
	{
		Width = i_width;
		Height = i_height;
		RefreshRate = i_refreshRate;
	}

	public override string ToString()
	{
		return Width + " x " + Height;
	}
}

public static class DisplaySettings
{
	private const string PREF_DISPLAY_MODE = "DisplayMode";

	private const string PREF_DISPLAY_WIDTH = "DisplayWidth";

	private const string PREF_DISPLAY_HEIGHT = "DisplayHeight";

	private const string PREF_DISPLAY_REFRESH_RATE = "DisplayRefreshRate";

	private static readonly string[] m_displayModeNames = new string[4]
	{
		"Bordered Fullscreen",
		"Fullscreen",
		"Borderless Fullscreen",
		"Windowed"
	};

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void InitializeBeforeScene()
	{
		EnsureDefaults();
		DisplaySettingsRuntime.EnsureExists();
		if (!Application.isEditor && !HasCommandLineResolutionOverride())
		{
			ApplySaved();
		}
	}

	public static void EnsureDefaults()
	{
		GameDisplayResolution nativeResolution = GetNativeResolution();
		bool flag = false;
		if (!PlayerPrefs.HasKey(PREF_DISPLAY_MODE))
		{
			PlayerPrefs.SetInt(PREF_DISPLAY_MODE, (int)GameDisplayMode.BorderlessFullscreen);
			flag = true;
		}
		if (!PlayerPrefs.HasKey(PREF_DISPLAY_WIDTH))
		{
			PlayerPrefs.SetInt(PREF_DISPLAY_WIDTH, nativeResolution.Width);
			flag = true;
		}
		if (!PlayerPrefs.HasKey(PREF_DISPLAY_HEIGHT))
		{
			PlayerPrefs.SetInt(PREF_DISPLAY_HEIGHT, nativeResolution.Height);
			flag = true;
		}
		if (!PlayerPrefs.HasKey(PREF_DISPLAY_REFRESH_RATE))
		{
			PlayerPrefs.SetInt(PREF_DISPLAY_REFRESH_RATE, nativeResolution.RefreshRate);
			flag = true;
		}
		if (flag)
		{
			PlayerPrefs.Save();
		}
	}

	public static string[] GetDisplayModeNames()
	{
		return (string[])m_displayModeNames.Clone();
	}

	public static GameDisplayMode GetSavedMode()
	{
		EnsureDefaults();
		int num = PlayerPrefs.GetInt(PREF_DISPLAY_MODE);
		if (!Enum.IsDefined(typeof(GameDisplayMode), num))
		{
			return GameDisplayMode.BorderlessFullscreen;
		}
		return (GameDisplayMode)num;
	}

	public static GameDisplayResolution GetSavedResolution()
	{
		EnsureDefaults();
		int num = PlayerPrefs.GetInt(PREF_DISPLAY_WIDTH);
		int num2 = PlayerPrefs.GetInt(PREF_DISPLAY_HEIGHT);
		int num3 = PlayerPrefs.GetInt(PREF_DISPLAY_REFRESH_RATE);
		List<GameDisplayResolution> supportedResolutions = GetSupportedResolutions();
		for (int i = 0; i < supportedResolutions.Count; i++)
		{
			if (supportedResolutions[i].Width == num && supportedResolutions[i].Height == num2)
			{
				return new GameDisplayResolution(num, num2, supportedResolutions[i].RefreshRate);
			}
		}
		return GetNativeResolution();
	}

	public static GameDisplayResolution GetNativeResolution()
	{
		Resolution currentResolution = UnityEngine.Screen.currentResolution;
		int num = (Display.main != null) ? Display.main.systemWidth : 0;
		int num2 = (Display.main != null) ? Display.main.systemHeight : 0;
		if (num <= 0)
		{
			num = currentResolution.width;
		}
		if (num2 <= 0)
		{
			num2 = currentResolution.height;
		}
		return new GameDisplayResolution(Mathf.Max(num, 640), Mathf.Max(num2, 360), Mathf.Max(currentResolution.refreshRate, 1));
	}

	public static List<GameDisplayResolution> GetSupportedResolutions()
	{
		Dictionary<string, GameDisplayResolution> dictionary = new Dictionary<string, GameDisplayResolution>();
		Resolution[] resolutions = UnityEngine.Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			if (resolutions[i].width < 640 || resolutions[i].height < 360)
			{
				continue;
			}
			string key = resolutions[i].width + "x" + resolutions[i].height;
			GameDisplayResolution value;
			if (!dictionary.TryGetValue(key, out value) || resolutions[i].refreshRate > value.RefreshRate)
			{
				dictionary[key] = new GameDisplayResolution(resolutions[i].width, resolutions[i].height, resolutions[i].refreshRate);
			}
		}
		GameDisplayResolution nativeResolution = GetNativeResolution();
		string key2 = nativeResolution.Width + "x" + nativeResolution.Height;
		if (!dictionary.ContainsKey(key2))
		{
			dictionary.Add(key2, nativeResolution);
		}
		List<GameDisplayResolution> list = new List<GameDisplayResolution>(dictionary.Values);
		list.Sort(delegate(GameDisplayResolution i_left, GameDisplayResolution i_right)
		{
			int num = i_right.Width.CompareTo(i_left.Width);
			return (num != 0) ? num : i_right.Height.CompareTo(i_left.Height);
		});
		return list;
	}

	public static GameDisplayResolution GetSafeWindowedResolution(GameDisplayResolution i_resolution)
	{
		GameDisplayResolution nativeResolution = GetNativeResolution();
		int num = Mathf.Max(nativeResolution.Width - 80, 640);
		int num2 = Mathf.Max(nativeResolution.Height - 120, 360);
		if (i_resolution.Width <= num && i_resolution.Height <= num2)
		{
			return i_resolution;
		}
		List<GameDisplayResolution> supportedResolutions = GetSupportedResolutions();
		GameDisplayResolution result = new GameDisplayResolution(Mathf.Min(num, 1280), Mathf.Min(num2, 720), nativeResolution.RefreshRate);
		long num3 = 0L;
		for (int i = 0; i < supportedResolutions.Count; i++)
		{
			if (supportedResolutions[i].Width > num || supportedResolutions[i].Height > num2)
			{
				continue;
			}
			long num4 = (long)supportedResolutions[i].Width * (long)supportedResolutions[i].Height;
			if (num4 > num3)
			{
				num3 = num4;
				result = supportedResolutions[i];
			}
		}
		return result;
	}

	public static void SaveAndApply(GameDisplayMode i_mode, GameDisplayResolution i_resolution)
	{
		if (i_mode == GameDisplayMode.BorderedFullscreen)
		{
			i_resolution = GetNativeResolution();
		}
		else if (i_mode == GameDisplayMode.Windowed)
		{
			i_resolution = GetSafeWindowedResolution(i_resolution);
		}
		PlayerPrefs.SetInt(PREF_DISPLAY_MODE, (int)i_mode);
		PlayerPrefs.SetInt(PREF_DISPLAY_WIDTH, i_resolution.Width);
		PlayerPrefs.SetInt(PREF_DISPLAY_HEIGHT, i_resolution.Height);
		PlayerPrefs.SetInt(PREF_DISPLAY_REFRESH_RATE, i_resolution.RefreshRate);
		PlayerPrefs.Save();
		Apply(i_mode, i_resolution);
	}

	public static void ApplySaved()
	{
		Apply(GetSavedMode(), GetSavedResolution());
	}

	private static void Apply(GameDisplayMode i_mode, GameDisplayResolution i_resolution)
	{
		if (i_mode == GameDisplayMode.Windowed)
		{
			i_resolution = GetSafeWindowedResolution(i_resolution);
		}
		int num = Mathf.Max(i_resolution.Width, 640);
		int num2 = Mathf.Max(i_resolution.Height, 360);
		int num3 = Mathf.Max(i_resolution.RefreshRate, 1);
		DisplaySettingsRuntime.PrepareForMode(i_mode);
		switch (i_mode)
		{
		case GameDisplayMode.BorderedFullscreen:
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
			UnityEngine.Screen.SetResolution(num, num2, FullScreenMode.Windowed, num3);
			DisplaySettingsRuntime.ScheduleBorderedFullscreen();
#else
			UnityEngine.Screen.SetResolution(num, num2, FullScreenMode.MaximizedWindow, num3);
#endif
			break;
		case GameDisplayMode.Fullscreen:
			UnityEngine.Screen.SetResolution(num, num2, FullScreenMode.ExclusiveFullScreen, num3);
			break;
		case GameDisplayMode.BorderlessFullscreen:
			UnityEngine.Screen.SetResolution(num, num2, FullScreenMode.FullScreenWindow, num3);
			break;
		default:
			UnityEngine.Screen.SetResolution(num, num2, FullScreenMode.Windowed, num3);
			break;
		}
	}

	private static bool HasCommandLineResolutionOverride()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			string text = commandLineArgs[i].ToLowerInvariant();
			if (text == "-screen-width" || text == "-screen-height" || text == "-screen-fullscreen")
			{
				return true;
			}
		}
		return false;
	}
}

public sealed class DisplaySettingsRuntime : MonoBehaviour
{
	private static DisplaySettingsRuntime m_instance;

	private Coroutine m_coroutineBorderedFullscreen;

	public static void EnsureExists()
	{
		if (m_instance != null)
		{
			return;
		}
		GameObject gameObject = new GameObject("DisplaySettingsRuntime");
		m_instance = gameObject.AddComponent<DisplaySettingsRuntime>();
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
	}

	private void Awake()
	{
		if (m_instance != null && m_instance != this)
		{
			UnityEngine.Object.Destroy(gameObject);
			return;
		}
		m_instance = this;
		UnityEngine.Object.DontDestroyOnLoad(gameObject);
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDestroy()
	{
		if (m_instance == this)
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
			m_instance = null;
		}
	}

	private void OnSceneLoaded(Scene i_scene, LoadSceneMode i_mode)
	{
		CanvasScaler[] array = Resources.FindObjectsOfTypeAll<CanvasScaler>();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject.scene.IsValid() && array[i].uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
			{
				array[i].screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
			}
		}
	}

	public static void PrepareForMode(GameDisplayMode i_mode)
	{
		EnsureExists();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		if (i_mode != GameDisplayMode.BorderedFullscreen)
		{
			IntPtr playerWindow = GetPlayerWindow();
			if (playerWindow != IntPtr.Zero && IsZoomed(playerWindow))
			{
				ShowWindow(playerWindow, 9);
			}
		}
#endif
	}

	public static void ScheduleBorderedFullscreen()
	{
		EnsureExists();
		if (m_instance.m_coroutineBorderedFullscreen != null)
		{
			m_instance.StopCoroutine(m_instance.m_coroutineBorderedFullscreen);
		}
		m_instance.m_coroutineBorderedFullscreen = m_instance.StartCoroutine(m_instance.CoroutineApplyBorderedFullscreen());
	}

	private IEnumerator CoroutineApplyBorderedFullscreen()
	{
		for (int i = 0; i < 120 && UnityEngine.Screen.fullScreenMode != FullScreenMode.Windowed; i++)
		{
			yield return null;
		}
		yield return null;
		yield return new WaitForEndOfFrame();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		IntPtr playerWindow = GetPlayerWindow();
		if (playerWindow != IntPtr.Zero)
		{
			ShowWindow(playerWindow, 3);
		}
#endif
		m_coroutineBorderedFullscreen = null;
	}

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
	private delegate bool EnumWindowsCallback(IntPtr i_window, IntPtr i_parameter);

	private static IntPtr GetPlayerWindow()
	{
		Process currentProcess = Process.GetCurrentProcess();
		IntPtr result = IntPtr.Zero;
		EnumWindows(delegate(IntPtr i_window, IntPtr i_parameter)
		{
			uint processId;
			GetWindowThreadProcessId(i_window, out processId);
			if (processId != (uint)currentProcess.Id)
			{
				return true;
			}
			StringBuilder stringBuilder = new StringBuilder(64);
			GetClassName(i_window, stringBuilder, stringBuilder.Capacity);
			if (stringBuilder.ToString() != "UnityWndClass")
			{
				return true;
			}
			result = i_window;
			return false;
		}, IntPtr.Zero);
		if (result != IntPtr.Zero)
		{
			return result;
		}
		currentProcess.Refresh();
		return currentProcess.MainWindowHandle;
	}

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowsCallback i_callback, IntPtr i_parameter);

	[DllImport("user32.dll")]
	private static extern uint GetWindowThreadProcessId(IntPtr i_window, out uint o_processId);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetClassName(IntPtr i_window, StringBuilder o_className, int i_maximumCount);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr i_window, int i_command);

	[DllImport("user32.dll")]
	private static extern bool IsZoomed(IntPtr i_window);
#endif
}
