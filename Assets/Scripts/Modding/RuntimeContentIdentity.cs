using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[DisallowMultipleComponent]
	public sealed class RuntimeContentIdentity : MonoBehaviour
	{
		[SerializeField]
		private string m_contentId;

		[SerializeField]
		private ContentCategory m_category;

		public ContentCategory Category => m_category;

		public void Configure(ContentId i_contentId, ContentCategory i_category)
		{
			m_contentId = i_contentId.ToString();
			m_category = i_category;
		}

		public bool TryGetContentId(out ContentId o_contentId)
		{
			return ContentId.TryParse(m_contentId, out o_contentId);
		}

		public static bool TryResolve(Component i_component, out ContentId o_contentId, out ContentCategory o_category)
		{
			o_contentId = default;
			o_category = default;
			if (i_component == null) return false;
			RuntimeContentIdentity identity = i_component.GetComponent<RuntimeContentIdentity>();
			if (identity == null || !identity.TryGetContentId(out o_contentId)) return false;
			o_category = identity.Category;
			return true;
		}
	}
}
