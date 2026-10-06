using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[Serializable]
	public sealed class CoreEnemyAssetSlotCatalogDocument
	{
		[JsonProperty("schemaVersion")] public int SchemaVersion { get; set; }
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("enemies")] public List<CoreEnemyAssetSlotEnemy> Enemies { get; set; } = new List<CoreEnemyAssetSlotEnemy>();
	}

	[Serializable]
	public sealed class CoreEnemyAssetSlotEnemy
	{
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("sourceRig")] public string SourceRig { get; set; }
		[JsonProperty("slots")] public List<CoreEnemyAssetSlotRecord> Slots { get; set; } = new List<CoreEnemyAssetSlotRecord>();
		[JsonProperty("legacyAliases")] public List<CoreEnemyAssetSlotAlias> LegacyAliases { get; set; } = new List<CoreEnemyAssetSlotAlias>();
	}

	[Serializable]
	public sealed class CoreEnemyAssetSlotRecord
	{
		[JsonProperty("slot")] public string Slot { get; set; }
		[JsonProperty("bone")] public string Bone { get; set; }
		[JsonProperty("rendererPath")] public string RendererPath { get; set; }
		[JsonProperty("spriteName")] public string SpriteName { get; set; }
		[JsonProperty("animatedSpriteName")] public string AnimatedSpriteName { get; set; }
		[JsonProperty("sourceAnimation")] public string SourceAnimation { get; set; }
	}

	[Serializable]
	public sealed class CoreEnemyAssetSlotAlias
	{
		[JsonProperty("slot")] public string Slot { get; set; }
		[JsonProperty("targets")] public List<string> Targets { get; set; } = new List<string>();
	}

	/// <summary>Loads the generated, runtime-authoritative catalog of every patchable Core enemy renderer.</summary>
	public static class CoreEnemyAssetSlotCatalog
	{
		public const string ResourcePath = "Modding/Core/enemy-asset-slots";
		private static CoreEnemyAssetSlotCatalogDocument s_document;
		private static Dictionary<string, CoreEnemyAssetSlotEnemy> s_byEnemy;
		private static bool s_loadAttempted;

		public static CoreEnemyAssetSlotCatalogDocument Document
		{
			get
			{
				if (!EnsureLoaded()) throw new InvalidOperationException("Generated Core enemy asset-slot catalog is missing from Resources: " + ResourcePath);
				return s_document;
			}
		}

		public static bool TryGet(string i_enemyId, out CoreEnemyAssetSlotEnemy o_enemy)
		{
			if (!EnsureLoaded()) { o_enemy = null; return false; }
			return s_byEnemy.TryGetValue(i_enemyId ?? string.Empty, out o_enemy);
		}

		private static bool EnsureLoaded()
		{
			if (s_loadAttempted) return s_document != null;
			s_loadAttempted = true;
			TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
			if (asset == null) return false;
			s_document = JsonConvert.DeserializeObject<CoreEnemyAssetSlotCatalogDocument>(asset.text);
			if (s_document == null || s_document.SchemaVersion != 1 || s_document.Type != "coreEnemyAssetSlotCatalog")
				throw new InvalidOperationException("Generated Core enemy asset-slot catalog is invalid or unsupported.");
			s_byEnemy = new Dictionary<string, CoreEnemyAssetSlotEnemy>(StringComparer.Ordinal);
			foreach (CoreEnemyAssetSlotEnemy enemy in s_document.Enemies)
			{
				if (enemy == null || string.IsNullOrEmpty(enemy.Id) || s_byEnemy.ContainsKey(enemy.Id))
					throw new InvalidOperationException("Generated Core enemy asset-slot catalog contains a missing or duplicate enemy ID.");
				s_byEnemy.Add(enemy.Id, enemy);
			}
			return true;
		}
	}
}
