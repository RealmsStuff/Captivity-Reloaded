using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	public enum ContentCategory
	{
		Enemy,
		Stage,
		Clothing,
		Item,
		Challenge,
		Rule,
		AssetPatch
	}

	public sealed class ContentRegistration
	{
		public ContentId Id { get; }
		public ContentCategory Category { get; }
		public string PackId { get; }
		public string Source { get; }
		public UnityEngine.Object RuntimeAsset { get; }

		public ContentRegistration(ContentId i_id, ContentCategory i_category, string i_packId, string i_source, UnityEngine.Object i_runtimeAsset = null)
		{
			Id = i_id;
			Category = i_category;
			PackId = i_packId;
			Source = i_source ?? string.Empty;
			RuntimeAsset = i_runtimeAsset;
		}
	}

	public sealed class ContentRegistry
	{
		private readonly Dictionary<ContentId, ContentRegistration> m_entries = new Dictionary<ContentId, ContentRegistration>();
		private readonly Dictionary<ContentCategory, List<ContentRegistration>> m_categories = new Dictionary<ContentCategory, List<ContentRegistration>>();

		public int Count => m_entries.Count;

		public bool Register(ContentRegistration i_registration, ValidationReport io_report)
		{
			if (i_registration == null)
			{
				io_report?.Add(ValidationSeverity.Error, "registry.null", "Cannot register a null content entry.");
				return false;
			}
			if (i_registration.Id.Namespace != i_registration.PackId)
			{
				io_report?.Add(ValidationSeverity.Error, "registry.namespace", "Content ID namespace must match its defining pack: " + i_registration.Id, i_registration.Source);
				return false;
			}
			if (m_entries.ContainsKey(i_registration.Id))
			{
				io_report?.Add(ValidationSeverity.Error, "registry.duplicate", "Content ID is already registered: " + i_registration.Id, i_registration.Source);
				return false;
			}

			m_entries.Add(i_registration.Id, i_registration);
			if (!m_categories.TryGetValue(i_registration.Category, out List<ContentRegistration> category))
			{
				category = new List<ContentRegistration>();
				m_categories.Add(i_registration.Category, category);
			}
			category.Add(i_registration);
			return true;
		}

		public bool TryGet(ContentId i_id, out ContentRegistration o_registration)
		{
			return m_entries.TryGetValue(i_id, out o_registration);
		}

		public IReadOnlyList<ContentRegistration> GetByCategory(ContentCategory i_category)
		{
			if (m_categories.TryGetValue(i_category, out List<ContentRegistration> category)) return category;
			return new ContentRegistration[0];
		}
	}
}
