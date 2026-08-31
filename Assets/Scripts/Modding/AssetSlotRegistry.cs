using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public sealed class AssetSlotRegistration
	{
		private readonly Action<UnityEngine.Object> m_apply;

		public ContentId Id { get; }
		public ContentId OwnerId { get; }
		public string PackId { get; }
		public string Source { get; }
		public Type AssetType { get; }
		public UnityEngine.Object BaselineAsset { get; }
		public UnityEngine.Object ResolvedAsset { get; private set; }

		public AssetSlotRegistration(ContentId i_id, ContentId i_ownerId, string i_packId, string i_source,
			UnityEngine.Object i_baselineAsset, Type i_assetType = null, Action<UnityEngine.Object> i_apply = null)
		{
			Id = i_id;
			OwnerId = i_ownerId;
			PackId = i_packId ?? string.Empty;
			Source = i_source ?? string.Empty;
			BaselineAsset = i_baselineAsset;
			ResolvedAsset = i_baselineAsset;
			AssetType = i_assetType ?? i_baselineAsset?.GetType() ?? typeof(UnityEngine.Object);
			m_apply = i_apply;
		}

		internal void Apply(UnityEngine.Object i_asset)
		{
			ResolvedAsset = i_asset;
			m_apply?.Invoke(i_asset);
		}
	}

	public sealed class AssetPatchRequest
	{
		public ContentId PatchId { get; }
		public ContentId TargetSlotId { get; }
		public string PackId { get; }
		public string Source { get; }
		public UnityEngine.Object ReplacementAsset { get; }

		public AssetPatchRequest(ContentId i_patchId, ContentId i_targetSlotId, string i_packId,
			string i_source, UnityEngine.Object i_replacementAsset)
		{
			PatchId = i_patchId;
			TargetSlotId = i_targetSlotId;
			PackId = i_packId ?? string.Empty;
			Source = i_source ?? string.Empty;
			ReplacementAsset = i_replacementAsset;
		}
	}

	public sealed class AssetSlotRegistry
	{
		private readonly Dictionary<ContentId, AssetSlotRegistration> m_slots = new Dictionary<ContentId, AssetSlotRegistration>();

		public int Count => m_slots.Count;

		public bool Register(AssetSlotRegistration i_slot, ValidationReport io_report)
		{
			if (i_slot == null)
			{
				io_report?.Add(ValidationSeverity.Error, "asset-slot.null", "Cannot register a null asset slot.");
				return false;
			}
			if (i_slot.Id.Namespace != i_slot.PackId || i_slot.OwnerId.Namespace != i_slot.PackId)
			{
				io_report?.Add(ValidationSeverity.Error, "asset-slot.namespace", "Asset slot and owner namespaces must match their defining pack: " + i_slot.Id, i_slot.Source);
				return false;
			}
			if (i_slot.BaselineAsset == null)
			{
				io_report?.Add(ValidationSeverity.Error, "asset-slot.baseline", "Asset slot requires a baseline asset: " + i_slot.Id, i_slot.Source);
				return false;
			}
			if (m_slots.ContainsKey(i_slot.Id))
			{
				io_report?.Add(ValidationSeverity.Error, "asset-slot.duplicate", "Asset slot is already registered: " + i_slot.Id, i_slot.Source);
				return false;
			}

			m_slots.Add(i_slot.Id, i_slot);
			return true;
		}

		public bool TryGet(ContentId i_id, out AssetSlotRegistration o_slot)
		{
			return m_slots.TryGetValue(i_id, out o_slot);
		}

		public void Resolve(IEnumerable<AssetPatchRequest> i_patches, ValidationReport io_report)
		{
			foreach (AssetSlotRegistration slot in m_slots.Values) slot.Apply(slot.BaselineAsset);

			Dictionary<ContentId, List<AssetPatchRequest>> patchesBySlot = new Dictionary<ContentId, List<AssetPatchRequest>>();
			if (i_patches == null) return;
			foreach (AssetPatchRequest patch in i_patches)
			{
				if (patch == null) continue;
				if (patch.PatchId.Namespace != patch.PackId)
				{
					io_report?.Add(ValidationSeverity.Error, "asset-patch.namespace", "Patch ID namespace must match its defining pack: " + patch.PatchId, patch.Source);
					continue;
				}
				if (!m_slots.TryGetValue(patch.TargetSlotId, out AssetSlotRegistration slot))
				{
					io_report?.Add(ValidationSeverity.Error, "asset-patch.target", "Patch targets an unknown public asset slot: " + patch.TargetSlotId, patch.Source);
					continue;
				}
				if (patch.ReplacementAsset == null || !slot.AssetType.IsInstanceOfType(patch.ReplacementAsset))
				{
					io_report?.Add(ValidationSeverity.Error, "asset-patch.type", "Replacement does not match slot asset type " + slot.AssetType.Name + ": " + patch.TargetSlotId, patch.Source);
					continue;
				}
				if (!patchesBySlot.TryGetValue(patch.TargetSlotId, out List<AssetPatchRequest> list))
				{
					list = new List<AssetPatchRequest>();
					patchesBySlot.Add(patch.TargetSlotId, list);
				}
				list.Add(patch);
			}

			foreach (KeyValuePair<ContentId, List<AssetPatchRequest>> pair in patchesBySlot)
			{
				if (pair.Value.Count != 1)
				{
					io_report?.Add(ValidationSeverity.Error, "asset-patch.conflict", pair.Value.Count + " enabled patches target the same slot; Core asset retained: " + pair.Key);
					continue;
				}
				m_slots[pair.Key].Apply(pair.Value[0].ReplacementAsset);
			}
		}
	}
}
