using UnityEngine;

[CreateAssetMenu(fileName = "CaptivityModProfile", menuName = "Captivity Reloaded/Mod Authoring Profile")]
public sealed class CaptivityModAuthoringProfile : ScriptableObject
{
	[Tooltip("Absolute path, or a path relative to the Unity project, containing the loose mod and manifest.json.")]
	public string sourceDirectory;

	[Tooltip("Prefab assets to place in the mod's Unity AssetBundle.")]
	public GameObject[] prefabs = new GameObject[0];

	public bool buildWindows = true;
	public bool buildAndroid = true;
	public bool buildWebGl = true;
}
