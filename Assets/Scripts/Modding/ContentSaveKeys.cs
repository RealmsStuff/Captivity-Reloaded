using System;
using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	public readonly struct LegacyContentKey : IEquatable<LegacyContentKey>
	{
		public ContentCategory Category { get; }
		public int Id { get; }

		public LegacyContentKey(ContentCategory i_category, int i_id)
		{
			if (i_id < 0) throw new ArgumentOutOfRangeException(nameof(i_id));
			Category = i_category;
			Id = i_id;
		}

		public bool Equals(LegacyContentKey i_other)
		{
			return Category == i_other.Category && Id == i_other.Id;
		}

		public override bool Equals(object i_object)
		{
			return i_object is LegacyContentKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			return ((int)Category * 397) ^ Id;
		}
	}

	public sealed class LegacyContentMap
	{
		private readonly Dictionary<LegacyContentKey, ContentId> m_contentIds = new Dictionary<LegacyContentKey, ContentId>();
		private readonly Dictionary<ContentId, LegacyContentKey> m_legacyKeys = new Dictionary<ContentId, LegacyContentKey>();

		public int Count => m_contentIds.Count;

		public LegacyContentMap(IEnumerable<CoreContentCatalogEntry> i_entries)
		{
			if (i_entries == null) return;
			foreach (CoreContentCatalogEntry entry in i_entries)
			{
				if (entry == null || !entry.LegacyId.HasValue) continue;
				LegacyContentKey key = new LegacyContentKey(entry.Category, entry.LegacyId.Value);
				m_contentIds.Add(key, entry.Id);
				m_legacyKeys.Add(entry.Id, key);
			}
		}

		public bool TryGetContentId(ContentCategory i_category, int i_legacyId, out ContentId o_contentId)
		{
			o_contentId = default;
			if (i_legacyId < 0) return false;
			return m_contentIds.TryGetValue(new LegacyContentKey(i_category, i_legacyId), out o_contentId);
		}

		public bool TryGetLegacyKey(ContentId i_contentId, out LegacyContentKey o_legacyKey)
		{
			return m_legacyKeys.TryGetValue(i_contentId, out o_legacyKey);
		}
	}

	public sealed class SavedContentState
	{
		public ContentId ContentId { get; }
		public ContentCategory Category { get; }
		public string StateJson { get; }

		public SavedContentState(ContentId i_contentId, ContentCategory i_category, string i_stateJson)
		{
			ContentId = i_contentId;
			Category = i_category;
			StateJson = string.IsNullOrWhiteSpace(i_stateJson) ? "{}" : i_stateJson;
		}

		public static bool TryCreate(string i_contentId, string i_category, string i_stateJson, out SavedContentState o_state)
		{
			o_state = null;
			if (!ContentId.TryParse(i_contentId, out ContentId contentId) ||
				!Enum.TryParse(i_category, ignoreCase: false, out ContentCategory category) ||
				!Enum.IsDefined(typeof(ContentCategory), category) ||
				!string.Equals(Enum.GetName(typeof(ContentCategory), category), i_category, StringComparison.Ordinal))
			{
				return false;
			}
			o_state = new SavedContentState(contentId, category, i_stateJson);
			return true;
		}
	}

	public enum SavedContentStatus
	{
		Available,
		Missing,
		CategoryMismatch
	}

	public sealed class SavedContentResolution
	{
		public SavedContentState State { get; }
		public SavedContentStatus Status { get; }
		public ContentRegistration Registration { get; }

		internal SavedContentResolution(SavedContentState i_state, SavedContentStatus i_status, ContentRegistration i_registration)
		{
			State = i_state;
			Status = i_status;
			Registration = i_registration;
		}
	}

	public static class ContentSaveResolver
	{
		public static bool TryResolveLegacy(ContentCategory i_category, int i_legacyId, string i_stateJson, LegacyContentMap i_legacyMap, ContentRegistry i_registry, out SavedContentResolution o_resolution)
		{
			o_resolution = null;
			if (i_legacyMap == null || !i_legacyMap.TryGetContentId(i_category, i_legacyId, out ContentId contentId)) return false;
			o_resolution = Resolve(new SavedContentState(contentId, i_category, i_stateJson), i_registry);
			return true;
		}

		public static SavedContentResolution Resolve(SavedContentState i_state, ContentRegistry i_registry)
		{
			if (i_state == null) throw new ArgumentNullException(nameof(i_state));
			if (i_registry != null && i_registry.TryGet(i_state.ContentId, out ContentRegistration registration))
			{
				if (registration.Category != i_state.Category)
				{
					return new SavedContentResolution(i_state, SavedContentStatus.CategoryMismatch, registration);
				}
				return new SavedContentResolution(i_state, SavedContentStatus.Available, registration);
			}
			return new SavedContentResolution(i_state, SavedContentStatus.Missing, null);
		}
	}
}
