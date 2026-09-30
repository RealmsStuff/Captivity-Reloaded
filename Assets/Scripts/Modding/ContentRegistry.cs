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
		public UnityEngine.Object RuntimeAsset { get; private set; }

		public ContentRegistration(ContentId i_id, ContentCategory i_category, string i_packId, string i_source, UnityEngine.Object i_runtimeAsset = null)
		{
			Id = i_id;
			Category = i_category;
			PackId = i_packId;
			Source = i_source ?? string.Empty;
			RuntimeAsset = i_runtimeAsset;
		}

		internal void BindRuntimeAsset(UnityEngine.Object i_runtimeAsset)
		{
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

		public bool BindRuntimeAsset(ContentId i_id, UnityEngine.Object i_runtimeAsset, ValidationReport io_report)
		{
			if (!m_entries.TryGetValue(i_id, out ContentRegistration registration))
			{
				io_report?.Add(ValidationSeverity.Error, "registry.bind-missing", "Cannot bind an unregistered content ID: " + i_id);
				return false;
			}
			if (i_runtimeAsset == null)
			{
				io_report?.Add(ValidationSeverity.Error, "registry.bind-null", "Cannot bind a null runtime asset: " + i_id, registration.Source);
				return false;
			}
			registration.BindRuntimeAsset(i_runtimeAsset);
			return true;
		}

		public IReadOnlyList<ContentRegistration> GetByCategory(ContentCategory i_category)
		{
			if (m_categories.TryGetValue(i_category, out List<ContentRegistration> category)) return category;
			return new ContentRegistration[0];
		}
	}
}
