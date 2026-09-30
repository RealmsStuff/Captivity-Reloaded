using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreClothingSpriteReference
	{
		[JsonProperty("resource", Required = Required.Always)] public string Resource { get; set; }
		[JsonProperty("sourceAsset")] public string SourceAsset { get; set; }
		[JsonProperty("pivotX")] public float PivotX { get; set; } = 0.5f;
		[JsonProperty("pivotY")] public float PivotY { get; set; } = 0.5f;
		[JsonProperty("pixelsPerUnit")] public float PixelsPerUnit { get; set; } = 32f;
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreClothingPieceRecord
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("slot", Required = Required.Always)] public string Slot { get; set; }
		[JsonProperty("sprite", Required = Required.Always)] public CoreClothingSpriteReference Sprite { get; set; }
		[JsonProperty("bone", Required = Required.Always)] public string Bone { get; set; }
		[JsonProperty("offsetX")] public float OffsetX { get; set; }
		[JsonProperty("offsetY")] public float OffsetY { get; set; }
		[JsonProperty("rotation")] public float Rotation { get; set; }
		[JsonProperty("sortingOffset")] public int SortingOffset { get; set; }
		[JsonProperty("attachToBone")] public bool AttachToBone { get; set; }
		[JsonProperty("hideBodyPart")] public bool HideBodyPart { get; set; }
		[JsonProperty("droppable")] public bool Droppable { get; set; }
		[JsonProperty("destroyable")] public bool Destroyable { get; set; }
		[JsonProperty("dropOnOralThrust")] public bool DropOnOralThrust { get; set; }
		[JsonProperty("destroyOnOralThrust")] public bool DestroyOnOralThrust { get; set; }
		[JsonProperty("playRipSound")] public bool PlayRipSound { get; set; }
		[JsonProperty("pieceType")] public string PieceType { get; set; } = "standard";
		[JsonProperty("hidesHair")] public bool HidesHair { get; set; }
		[JsonProperty("connectedPieces")] public List<string> ConnectedPieces { get; set; } = new List<string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreClothingRecord
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("legacyId", Required = Required.Always)] public int LegacyId { get; set; }
		[JsonProperty("category", Required = Required.Always)] public string Category { get; set; }
		[JsonProperty("migrationVersion")] public int? MigrationVersion { get; set; }
		[JsonProperty("sourceAsset")] public string SourceAsset { get; set; }
		[JsonProperty("icon")] public CoreClothingSpriteReference Icon { get; set; }
		[JsonProperty("pieces")] public List<CoreClothingPieceRecord> Pieces { get; set; } = new List<CoreClothingPieceRecord>();
		[JsonProperty("incompatibleCategories")] public List<string> IncompatibleCategories { get; set; } = new List<string>();
		[JsonProperty("incompatibleClothing")] public List<int> IncompatibleClothing { get; set; } = new List<int>();
		[JsonProperty("compatibleOverrides")] public List<int> CompatibleOverrides { get; set; } = new List<int>();
		[JsonProperty("migrationWarnings")] public List<string> MigrationWarnings { get; set; } = new List<string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class CoreClothingCatalogDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("entries", Required = Required.Always)] public List<CoreClothingRecord> Entries { get; set; }
	}

	public sealed class CoreClothingDefinition
	{
		public ContentId Id { get; }
		public int LegacyId { get; }
		public string Category { get; }
		public string Source { get; }
		public int? MigrationVersion { get; }
		public string SourceAsset { get; }
		public CoreClothingSpriteReference Icon { get; }
		public IReadOnlyList<CoreClothingPieceRecord> Pieces { get; }
		public IReadOnlyList<string> IncompatibleCategories { get; }
		public IReadOnlyList<int> IncompatibleClothing { get; }
		public IReadOnlyList<int> CompatibleOverrides { get; }
		public IReadOnlyList<string> MigrationWarnings { get; }
		// Version 1 already contains complete icon, piece, rig, and compatibility
		// metadata. Version 2 only improves exporter fidelity, so both generations
		// can be reconstructed and expose their public artwork slots.
		public bool HasLoosePresentation => MigrationVersion >= 1 && Icon != null && Pieces.Count > 0;
		internal CoreClothingDefinition(ContentId i_id, CoreClothingRecord i_record, string i_source)
		{
			Id = i_id; LegacyId = i_record.LegacyId; Category = i_record.Category; Source = i_source ?? string.Empty;
			MigrationVersion = i_record.MigrationVersion; SourceAsset = i_record.SourceAsset;
			Icon = i_record.Icon; Pieces = i_record.Pieces ?? new List<CoreClothingPieceRecord>();
			IncompatibleCategories = i_record.IncompatibleCategories ?? new List<string>();
			IncompatibleClothing = i_record.IncompatibleClothing ?? new List<int>();
			CompatibleOverrides = i_record.CompatibleOverrides ?? new List<int>();
			MigrationWarnings = i_record.MigrationWarnings ?? new List<string>();
		}
	}

	public sealed class CoreClothingCatalogLoadResult
	{
		public List<CoreClothingDefinition> Definitions { get; } = new List<CoreClothingDefinition>();
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class CoreClothingCatalogParser
	{
		public static CoreClothingCatalogLoadResult Parse(string i_json, string i_source)
		{
			CoreClothingCatalogLoadResult result = new CoreClothingCatalogLoadResult();
			CoreClothingCatalogDocument document;
			try { document = JsonConvert.DeserializeObject<CoreClothingCatalogDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Core clothing catalog resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "coreClothingCatalog") Error(result, "type", "Definition type must be 'coreClothingCatalog'.", i_source);
			if (document.Entries == null || document.Entries.Count == 0) Error(result, "entries", "entries must contain at least one Core clothing record.", i_source);
			HashSet<ContentId> ids = new HashSet<ContentId>();
			HashSet<int> legacyIds = new HashSet<int>();
			foreach (CoreClothingRecord record in document.Entries ?? new List<CoreClothingRecord>())
			{
				bool validId = ContentId.TryParse(record.Id, out ContentId id) && id.Namespace == "core" && id.Path.StartsWith("clothing/", StringComparison.Ordinal);
				if (!validId) Error(result, "id", "Entry IDs must use a core:clothing/ path.", i_source);
				else if (!ids.Add(id)) Error(result, "duplicate-id", "Duplicate Core clothing ID: " + id, i_source);
				if (record.LegacyId < 0) Error(result, "legacy-id", "legacyId cannot be negative.", i_source);
				else if (!legacyIds.Add(record.LegacyId)) Error(result, "duplicate-legacy-id", "Duplicate Core clothing legacyId: " + record.LegacyId, i_source);
				if (!IsCategory(record.Category)) Error(result, "category", "category must name a valid clothing category.", i_source);
				ValidateMigration(record, result, i_source);
				if (validId && IsCategory(record.Category)) result.Definitions.Add(new CoreClothingDefinition(id, record, i_source));
			}
			if (!result.Report.IsValid) result.Definitions.Clear();
			return result;
		}

		private static void ValidateMigration(CoreClothingRecord i_record, CoreClothingCatalogLoadResult io_result, string i_source)
		{
			if (!i_record.MigrationVersion.HasValue) return;
			if (i_record.MigrationVersion != 1 && i_record.MigrationVersion != 2) Error(io_result, "migration-version", "Unsupported clothing migrationVersion.", i_source);
			bool sourceType = !string.IsNullOrWhiteSpace(i_record.SourceAsset)
				&& (i_record.SourceAsset.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
					|| i_record.SourceAsset.EndsWith(".unity", StringComparison.OrdinalIgnoreCase));
			if (!sourceType || !i_record.SourceAsset.StartsWith("Assets/", StringComparison.Ordinal))
				Error(io_result, "source-asset", "sourceAsset must identify an Assets/*.prefab or Assets/*.unity migration source.", i_source);
			ValidateSprite(i_record.Icon, "icon", io_result, i_source);
			if (i_record.Pieces == null || i_record.Pieces.Count == 0 || i_record.Pieces.Count > 64)
				Error(io_result, "pieces", "Migrated clothing requires 1 through 64 pieces.", i_source);
			HashSet<string> pieceIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (CoreClothingPieceRecord piece in i_record.Pieces ?? new List<CoreClothingPieceRecord>())
			{
				if (piece == null || string.IsNullOrWhiteSpace(piece.Id) || !pieceIds.Add(piece.Id)) Error(io_result, "piece-id", "Piece IDs must be non-empty and unique within a garment.", i_source);
				if (piece == null) continue;
				if (!ClothingSlotCatalog.IsValidPieceSlot(piece.Slot)) Error(io_result, "piece-slot", "Unknown migrated clothing slot: " + piece.Slot, i_source);
				if (string.IsNullOrWhiteSpace(piece.Bone)) Error(io_result, "piece-bone", "Every migrated piece requires a player bone.", i_source);
				if (piece.PieceType != "standard" && piece.PieceType != "hat") Error(io_result, "piece-type", "pieceType must be standard or hat.", i_source);
				if (piece.HidesHair && piece.PieceType != "hat") Error(io_result, "piece-hair", "Only hat pieces may hide hair.", i_source);
				if (piece.SortingOffset < -100 || piece.SortingOffset > 100) Error(io_result, "piece-sorting", "Piece sortingOffset must be between -100 and 100.", i_source);
				ValidateSprite(piece.Sprite, piece.Id, io_result, i_source);
			}
			foreach (CoreClothingPieceRecord piece in i_record.Pieces ?? new List<CoreClothingPieceRecord>())
				foreach (string connected in piece?.ConnectedPieces ?? new List<string>())
					if (!pieceIds.Contains(connected)) Error(io_result, "piece-connection", "Connected piece does not exist: " + connected, i_source);
			foreach (string category in i_record.IncompatibleCategories ?? new List<string>())
				if (!IsCategory(category)) Error(io_result, "incompatible-category", "Unknown incompatible clothing category: " + category, i_source);
		}

		private static void ValidateSprite(CoreClothingSpriteReference i_sprite, string i_name, CoreClothingCatalogLoadResult io_result, string i_source)
		{
			if (i_sprite == null || string.IsNullOrWhiteSpace(i_sprite.Resource) || i_sprite.Resource.Contains("..")
				|| i_sprite.Resource.StartsWith("/", StringComparison.Ordinal) || i_sprite.Resource.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				Error(io_result, "sprite-resource", "Sprite resource paths must be extensionless project-relative Resource paths: " + i_name, i_source);
			// Unity permits sprite pivots outside the texture rectangle. A number of
			// original garments intentionally use them to align artwork to a distant
			// rig bone, so migrated Core metadata must preserve those values losslessly.
			else if (float.IsNaN(i_sprite.PixelsPerUnit) || float.IsInfinity(i_sprite.PixelsPerUnit)
				|| i_sprite.PixelsPerUnit <= 0f || i_sprite.PixelsPerUnit > 1024f
				|| float.IsNaN(i_sprite.PivotX) || float.IsInfinity(i_sprite.PivotX)
				|| float.IsNaN(i_sprite.PivotY) || float.IsInfinity(i_sprite.PivotY)
				|| Math.Abs(i_sprite.PivotX) > 64f || Math.Abs(i_sprite.PivotY) > 64f)
				Error(io_result, "sprite-metadata", "Sprite pivot or pixelsPerUnit is invalid: " + i_name, i_source);
		}

		private static bool IsCategory(string i_value)
		{
			string[] values = { "Hair", "Upper", "Lower", "Shoes", "Hat", "Sleeves", "Stockings", "Other" };
			foreach (string value in values) if (string.Equals(value, i_value, StringComparison.OrdinalIgnoreCase)) return true;
			return false;
		}

		private static void Error(CoreClothingCatalogLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "core-clothing." + i_code, i_message, i_source);
		}
	}
}
