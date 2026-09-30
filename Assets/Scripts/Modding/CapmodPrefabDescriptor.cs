using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public enum CapmodPrefabKind
	{
		Prop = 0
	}

	/// <summary>Public, compiled-in identity attached to the root of an approved bundle prefab.</summary>
	[DisallowMultipleComponent]
	public sealed class CapmodPrefabDescriptor : MonoBehaviour
	{
		[SerializeField] private string m_id;
		[SerializeField] private CapmodPrefabKind m_kind = CapmodPrefabKind.Prop;
		[SerializeField, TextArea] private string m_description;

		public string Id => m_id;
		public CapmodPrefabKind Kind => m_kind;
		public string Description => m_description;
	}
}
