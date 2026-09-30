using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledMapDocument
	{
		[JsonProperty("orientation")] public string Orientation { get; set; }
		[JsonProperty("infinite")] public bool Infinite { get; set; }
		[JsonProperty("width")] public int Width { get; set; }
		[JsonProperty("height")] public int Height { get; set; }
		[JsonProperty("tilewidth")] public int TileWidth { get; set; }
		[JsonProperty("tileheight")] public int TileHeight { get; set; }
		[JsonProperty("layers")] public List<TiledLayerDocument> Layers { get; set; }
		[JsonProperty("tilesets")] public List<TiledTilesetDocument> Tilesets { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledLayerDocument
	{
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("offsetx")] public float OffsetX { get; set; }
		[JsonProperty("offsety")] public float OffsetY { get; set; }
		[JsonProperty("x")] public int X { get; set; }
		[JsonProperty("y")] public int Y { get; set; }
		[JsonProperty("width")] public int Width { get; set; }
		[JsonProperty("height")] public int Height { get; set; }
		[JsonProperty("opacity")] public float? Opacity { get; set; }
		[JsonProperty("visible")] public bool? Visible { get; set; }
		[JsonProperty("data")] public List<uint> Data { get; set; }
		[JsonProperty("layers")] public List<TiledLayerDocument> Layers { get; set; }
		[JsonProperty("objects")] public List<TiledObjectDocument> Objects { get; set; }
		[JsonProperty("image")] public string Image { get; set; }
		[JsonProperty("repeatx")] public bool RepeatX { get; set; }
		[JsonProperty("repeaty")] public bool RepeatY { get; set; }
		[JsonProperty("parallaxx")] public float? ParallaxX { get; set; }
		[JsonProperty("parallaxy")] public float? ParallaxY { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledTilesetDocument
	{
		[JsonProperty("firstgid")] public uint FirstGid { get; set; }
		[JsonProperty("source")] public string Source { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("tilewidth")] public int TileWidth { get; set; }
		[JsonProperty("tileheight")] public int TileHeight { get; set; }
		[JsonProperty("tilecount")] public int TileCount { get; set; }
		[JsonProperty("columns")] public int Columns { get; set; }
		[JsonProperty("spacing")] public int Spacing { get; set; }
		[JsonProperty("margin")] public int Margin { get; set; }
		[JsonProperty("image")] public string Image { get; set; }
		[JsonProperty("imagewidth")] public int ImageWidth { get; set; }
		[JsonProperty("imageheight")] public int ImageHeight { get; set; }
		[JsonProperty("objectalignment")] public string ObjectAlignment { get; set; }
		[JsonProperty("tiles")] public List<TiledTileDocument> Tiles { get; set; }
		[JsonProperty("properties")] public List<TiledPropertyDocument> Properties { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledTileDocument
	{
		[JsonProperty("id")] public int Id { get; set; }
		[JsonProperty("animation")] public List<TiledAnimationFrameDocument> Animation { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledAnimationFrameDocument
	{
		[JsonProperty("tileid")] public int TileId { get; set; }
		[JsonProperty("duration")] public int Duration { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledObjectDocument
	{
		[JsonProperty("template")] public string Template { get; set; }
		[JsonProperty("id")] public int Id { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("class")] public string Class { get; set; }
		[JsonProperty("x")] public float X { get; set; }
		[JsonProperty("y")] public float Y { get; set; }
		[JsonProperty("width")] public float Width { get; set; }
		[JsonProperty("height")] public float Height { get; set; }
		[JsonProperty("point")] public bool Point { get; set; }
		[JsonProperty("gid")] public uint Gid { get; set; }
		[JsonProperty("rotation")] public float Rotation { get; set; }
		[JsonProperty("visible")] public bool? Visible { get; set; }
		[JsonProperty("properties")] public List<TiledPropertyDocument> Properties { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	internal sealed class TiledPropertyDocument
	{
		[JsonProperty("name")] public string Name { get; set; }
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("value")] public object Value { get; set; }
	}

	public sealed class TiledLevelObject
	{
		public int Id { get; }
		public string Name { get; }
		public float X { get; }
		public float Y { get; }
		public float Width { get; }
		public float Height { get; }
		public bool IsTileObject { get; }
		public bool ClimbableLeft { get; }
		public bool ClimbableRight { get; }
		public string VisualFile { get; }
		public string BrokenVisualFile { get; }
		public string OpenVisualFile { get; }
		public string ClosedVisualFile { get; }
		public string OnVisualFile { get; }
		public string OffVisualFile { get; }
		public float VisualPixelsPerUnit { get; }

		internal TiledLevelObject(TiledObjectDocument i_object, float i_offsetX, float i_offsetY)
		{
			Id = i_object.Id;
			Name = i_object.Name ?? string.Empty;
			// Tiled tile objects are anchored at their bottom-left; normalize preview-backed
			// gameplay objects to the same center point used by ordinary point markers.
			X = i_object.X + (i_object.Gid != 0 ? i_object.Width * 0.5f : 0f) + i_offsetX;
			Y = i_object.Y - (i_object.Gid != 0 ? i_object.Height * 0.5f : 0f) + i_offsetY;
			Width = i_object.Width;
			Height = i_object.Height;
			IsTileObject = i_object.Gid != 0;
			ClimbableLeft = ReadBoolProperty(i_object.Properties, "climbableLeft");
			ClimbableRight = ReadBoolProperty(i_object.Properties, "climbableRight");
			VisualFile = ReadStringProperty(i_object.Properties, "visualFile");
			BrokenVisualFile = ReadStringProperty(i_object.Properties, "brokenVisualFile");
			OpenVisualFile = ReadStringProperty(i_object.Properties, "openVisualFile");
			ClosedVisualFile = ReadStringProperty(i_object.Properties, "closedVisualFile");
			OnVisualFile = ReadStringProperty(i_object.Properties, "onVisualFile");
			OffVisualFile = ReadStringProperty(i_object.Properties, "offVisualFile");
			VisualPixelsPerUnit = ReadFloatProperty(i_object.Properties, "visualPixelsPerUnit", 32f);
		}

		private static string ReadStringProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name)
		{
			if (i_properties == null) return string.Empty;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && string.Equals(property.Name, i_name, StringComparison.Ordinal))
					return property.Value as string ?? string.Empty;
			return string.Empty;
		}

		private static float ReadFloatProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name, float i_fallback)
		{
			if (i_properties == null) return i_fallback;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && string.Equals(property.Name, i_name, StringComparison.Ordinal))
				{
					try { return Convert.ToSingle(property.Value); }
					catch (Exception) { return float.NaN; }
				}
			return i_fallback;
		}

		private static bool ReadBoolProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name)
		{
			if (i_properties == null) return false;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && string.Equals(property.Name, i_name, StringComparison.Ordinal)
					&& property.Value is bool value) return value;
			return false;
		}
	}

	public sealed class TiledTilesetDefinition
	{
		public uint FirstGid { get; }
		public string Name { get; }
		public int TileWidth { get; }
		public int TileHeight { get; }
		public int TileCount { get; }
		public int Columns { get; }
		public int Spacing { get; }
		public int Margin { get; }
		public int ImageWidth { get; }
		public int ImageHeight { get; }
		public string AssetRoot { get; }
		public string ImagePath { get; }
		public string ObjectAlignment { get; }
		public float BorderLeft { get; }
		public float BorderBottom { get; }
		public float BorderRight { get; }
		public float BorderTop { get; }
		public IReadOnlyDictionary<int, IReadOnlyList<TiledAnimationFrameDefinition>> Animations { get; }

		internal TiledTilesetDefinition(uint i_firstGid, TiledTilesetDocument i_document,
			string i_assetRoot, string i_imagePath)
		{
			FirstGid = i_firstGid;
			Name = i_document.Name ?? string.Empty;
			TileWidth = i_document.TileWidth;
			TileHeight = i_document.TileHeight;
			TileCount = i_document.TileCount;
			Columns = i_document.Columns;
			Spacing = i_document.Spacing;
			Margin = i_document.Margin;
			ImageWidth = i_document.ImageWidth;
			ImageHeight = i_document.ImageHeight;
			AssetRoot = i_assetRoot ?? string.Empty;
			ImagePath = i_imagePath ?? string.Empty;
			ObjectAlignment = string.IsNullOrWhiteSpace(i_document.ObjectAlignment)
				? "bottomleft" : i_document.ObjectAlignment.ToLowerInvariant();
			foreach (TiledPropertyDocument property in i_document.Properties ?? new List<TiledPropertyDocument>())
			{
				if (property == null) continue;
				if (property.Name == "spriteBorderLeft") BorderLeft = ReadFloat(property.Value);
				else if (property.Name == "spriteBorderBottom") BorderBottom = ReadFloat(property.Value);
				else if (property.Name == "spriteBorderRight") BorderRight = ReadFloat(property.Value);
				else if (property.Name == "spriteBorderTop") BorderTop = ReadFloat(property.Value);
			}
			Dictionary<int, IReadOnlyList<TiledAnimationFrameDefinition>> animations = new Dictionary<int, IReadOnlyList<TiledAnimationFrameDefinition>>();
			foreach (TiledTileDocument tile in i_document.Tiles ?? new List<TiledTileDocument>())
				if (tile.Animation != null && tile.Animation.Count > 0)
				{
					List<TiledAnimationFrameDefinition> frames = new List<TiledAnimationFrameDefinition>();
					foreach (TiledAnimationFrameDocument frame in tile.Animation)
						frames.Add(new TiledAnimationFrameDefinition(frame.TileId, frame.Duration));
					animations[tile.Id] = frames;
				}
			Animations = animations;
		}

		private static float ReadFloat(object i_value)
		{
			try { return Convert.ToSingle(i_value); } catch (Exception) { return 0f; }
		}
	}

	public sealed class TiledAnimationFrameDefinition
	{
		public int TileId { get; }
		public int DurationMilliseconds { get; }
		internal TiledAnimationFrameDefinition(int i_tileId, int i_durationMilliseconds)
		{
			TileId = i_tileId;
			DurationMilliseconds = i_durationMilliseconds;
		}
	}

	public sealed class TiledTileLayerDefinition
	{
		public string Name { get; }
		public int X { get; }
		public int Y { get; }
		public int Width { get; }
		public int Height { get; }
		public float Opacity { get; }
		public int DrawOrder { get; }
		public IReadOnlyList<uint> Data { get; }

		internal TiledTileLayerDefinition(TiledLayerDocument i_layer, int i_x, int i_y,
			float i_opacity, int i_drawOrder)
		{
			Name = i_layer.Name ?? string.Empty;
			X = i_x;
			Y = i_y;
			Width = i_layer.Width;
			Height = i_layer.Height;
			Opacity = i_opacity;
			DrawOrder = i_drawOrder;
			Data = i_layer.Data;
		}
	}

	public sealed class TiledImageLayerDefinition
	{
		public string Name { get; }
		public int X { get; }
		public int Y { get; }
		public float Opacity { get; }
		public bool RepeatX { get; }
		public bool RepeatY { get; }
		public float ParallaxX { get; }
		public float ParallaxY { get; }
		public int DrawOrder { get; }
		public string AssetRoot { get; }
		public string ImagePath { get; }

		internal TiledImageLayerDefinition(TiledLayerDocument i_layer, int i_x, int i_y, float i_opacity,
			int i_drawOrder, string i_assetRoot, string i_imagePath)
		{
			Name = i_layer.Name ?? string.Empty;
			X = i_x;
			Y = i_y;
			Opacity = i_opacity;
			RepeatX = i_layer.RepeatX;
			RepeatY = i_layer.RepeatY;
			ParallaxX = i_layer.ParallaxX ?? 1f;
			ParallaxY = i_layer.ParallaxY ?? 1f;
			DrawOrder = i_drawOrder;
			AssetRoot = i_assetRoot;
			ImagePath = i_imagePath;
		}
	}

	public sealed class TiledDecorationDefinition
	{
		public int Id { get; }
		public string Name { get; }
		public uint EncodedGid { get; }
		public float X { get; }
		public float Y { get; }
		public float Width { get; }
		public float Height { get; }
		public float Rotation { get; }
		public float Opacity { get; }
		public int DrawOrder { get; }
		public int SortingOrder { get; }
		public bool Repeat { get; }
		public bool AdaptiveTiling { get; }
		public float AdaptiveModeThreshold { get; }
		public string SortingLayer { get; }
		public string TintColor { get; }

		internal TiledDecorationDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			float i_opacity, int i_drawOrder)
		{
			Id = i_object.Id;
			Name = i_object.Name ?? string.Empty;
			EncodedGid = i_object.Gid;
			X = i_object.X + i_offsetX;
			Y = i_object.Y + i_offsetY;
			Width = i_object.Width;
			Height = i_object.Height;
			Rotation = i_object.Rotation;
			Opacity = i_opacity;
			DrawOrder = i_drawOrder;
			SortingOrder = i_drawOrder;
			SortingLayer = "Decoration";
			TintColor = "#FFFFFFFF";
			AdaptiveModeThreshold = 0.5f;
			if (i_object.Properties != null)
				foreach (TiledPropertyDocument property in i_object.Properties)
				{
					if (property == null) continue;
					if (string.Equals(property.Name, "repeat", StringComparison.Ordinal) && property.Value is bool repeat) Repeat = repeat;
					else if (string.Equals(property.Name, "tileMode", StringComparison.Ordinal) && property.Value is string tileMode) AdaptiveTiling = tileMode == "adaptive";
					else if (string.Equals(property.Name, "adaptiveModeThreshold", StringComparison.Ordinal)) AdaptiveModeThreshold = ReadFloat(property.Value, 0.5f);
					else if (string.Equals(property.Name, "sortingLayer", StringComparison.Ordinal) && property.Value is string sortingLayer) SortingLayer = sortingLayer;
					else if (string.Equals(property.Name, "sortingOrder", StringComparison.Ordinal)) SortingOrder = ReadInt(property.Value, i_drawOrder);
					else if (string.Equals(property.Name, "opacity", StringComparison.Ordinal)) Opacity *= ReadFloat(property.Value, 1f);
					else if (string.Equals(property.Name, "tintColor", StringComparison.Ordinal) && property.Value is string tint) TintColor = tint;
				}
		}

		private static int ReadInt(object i_value, int i_fallback)
		{
			try { return Convert.ToInt32(i_value); } catch (Exception) { return i_fallback; }
		}

		private static float ReadFloat(object i_value, float i_fallback)
		{
			try { return Convert.ToSingle(i_value); } catch (Exception) { return i_fallback; }
		}
	}

	public sealed class TiledWeaponCaseDefinition
	{
		public TiledLevelObject Point { get; }
		public ContentId Weapon { get; }
		public int CaseSize { get; }

		internal TiledWeaponCaseDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			ContentId i_weapon, int i_caseSize)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Weapon = i_weapon;
			CaseSize = i_caseSize;
		}
	}

	public sealed class TiledDoorDefinition
	{
		public TiledLevelObject Point { get; }
		public string DoorType { get; }
		public bool InitiallyOpen { get; }
		public float ProximityRadius { get; }
		public int Price { get; }
		public bool SingleUse { get; }
		public bool InitiallyInteractable { get; }
		public string RequiredItemId { get; }
		public string OpenSprite { get; }
		public string ClosedSprite { get; }

		internal TiledDoorDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_doorType, bool i_initiallyOpen, float i_proximityRadius, int i_price,
			bool i_singleUse, bool i_initiallyInteractable, string i_requiredItemId,
			string i_openSprite, string i_closedSprite)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			DoorType = i_doorType;
			InitiallyOpen = i_initiallyOpen;
			ProximityRadius = i_proximityRadius;
			Price = i_price;
			SingleUse = i_singleUse;
			InitiallyInteractable = i_initiallyInteractable;
			RequiredItemId = i_requiredItemId ?? string.Empty;
			OpenSprite = i_openSprite ?? string.Empty;
			ClosedSprite = i_closedSprite ?? string.Empty;
		}
	}

	public sealed class TiledScriptedActorDefinition
	{
		public TiledLevelObject Point { get; }
		public ContentId Enemy { get; }
		public bool InitiallyActive { get; }

		internal TiledScriptedActorDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			ContentId i_enemy, bool i_initiallyActive)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Enemy = i_enemy;
			InitiallyActive = i_initiallyActive;
		}
	}

	public sealed class TiledDoorSwitchDefinition
	{
		public TiledLevelObject Point { get; }
		public IReadOnlyList<string> TargetDoorIds { get; }

		internal TiledDoorSwitchDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			IReadOnlyList<string> i_targetDoorIds)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			TargetDoorIds = i_targetDoorIds;
		}
	}

	public sealed class TiledNoteDefinition
	{
		public TiledLevelObject Point { get; }
		public string Text { get; }
		public int FontSize { get; }

		internal TiledNoteDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_text, int i_fontSize)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Text = i_text;
			FontSize = i_fontSize;
		}
	}

	public sealed class TiledKeypadDefinition
	{
		public TiledLevelObject Point { get; }
		public string Code { get; }
		public IReadOnlyList<string> TargetDoorIds { get; }

		internal TiledKeypadDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_code, IReadOnlyList<string> i_targetDoorIds)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Code = i_code;
			TargetDoorIds = i_targetDoorIds;
		}
	}

	public sealed class TiledInteractionDefinition
	{
		public TiledLevelObject Area { get; }
		public string Trigger { get; }
		public int Price { get; }
		public bool SingleUse { get; }
		public string Message { get; }
		public string DoorAction { get; }
		public IReadOnlyList<string> TargetDoorIds { get; }
		public int MinWave { get; }
		public int MaxWave { get; }
		public int ActivationsRequired { get; }
		public float DelaySeconds { get; }
		public int GiveMoney { get; }
		public string LightAction { get; }
		public IReadOnlyList<string> TargetLightIds { get; }
		public IReadOnlyList<string> TargetSpawnerIds { get; }
		public int SpawnCount { get; }
		public IReadOnlyList<string> TargetInteractionIds { get; }
		public ContentId? RequiredItem { get; }
		public bool ConsumeRequiredItem { get; }
		public ContentId? GiveItem { get; }
		public int GiveItemAmount { get; }
		public CoreMapArtCatalogEntry VisualArt { get; }
		public float VisualScale { get; }
		public string AssetRoot { get; }
		public string VisualFile { get; }
		public string ActivatedVisualFile { get; }
		public float VisualPixelsPerUnit { get; }
		public float VisualPivotX { get; }
		public float VisualPivotY { get; }
		public string VisualSortingLayer { get; }
		public int VisualSortingOrder { get; }
		public int MinEnemies { get; }
		public int MaxEnemies { get; }
		public IReadOnlyList<ContentId> RequiredChallenges { get; }
		public IReadOnlyList<string> RequiredInteractionIds { get; }
		public int RequiredMoney { get; }
		public float RestoreHealth { get; }
		public float KnockbackX { get; }
		public float KnockbackY { get; }
		public float RagdollSeconds { get; }

		internal TiledInteractionDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_trigger, int i_price, bool i_singleUse, string i_message, string i_doorAction,
			IReadOnlyList<string> i_targetDoorIds, int i_minWave, int i_maxWave,
			int i_activationsRequired, float i_delaySeconds, int i_giveMoney, string i_lightAction,
			IReadOnlyList<string> i_targetLightIds, IReadOnlyList<string> i_targetSpawnerIds,
			int i_spawnCount, IReadOnlyList<string> i_targetInteractionIds, ContentId? i_requiredItem,
			bool i_consumeRequiredItem, ContentId? i_giveItem, int i_giveItemAmount)
			: this(i_object, i_offsetX, i_offsetY, i_trigger, i_price, i_singleUse, i_message, i_doorAction,
				i_targetDoorIds, i_minWave, i_maxWave, i_activationsRequired, i_delaySeconds, i_giveMoney,
				i_lightAction, i_targetLightIds, i_targetSpawnerIds, i_spawnCount, i_targetInteractionIds,
				i_requiredItem, i_consumeRequiredItem, i_giveItem, i_giveItemAmount, null, 1f) { }

		internal TiledInteractionDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_trigger, int i_price, bool i_singleUse, string i_message, string i_doorAction,
			IReadOnlyList<string> i_targetDoorIds, int i_minWave, int i_maxWave,
			int i_activationsRequired, float i_delaySeconds, int i_giveMoney, string i_lightAction,
			IReadOnlyList<string> i_targetLightIds, IReadOnlyList<string> i_targetSpawnerIds,
			int i_spawnCount, IReadOnlyList<string> i_targetInteractionIds, ContentId? i_requiredItem,
			bool i_consumeRequiredItem, ContentId? i_giveItem, int i_giveItemAmount,
			CoreMapArtCatalogEntry i_visualArt, float i_visualScale, int i_minEnemies = 0, int i_maxEnemies = 0,
			IReadOnlyList<ContentId> i_requiredChallenges = null, IReadOnlyList<string> i_requiredInteractionIds = null,
			int i_requiredMoney = 0, float i_restoreHealth = 0f, float i_knockbackX = 0f,
			float i_knockbackY = 0f, float i_ragdollSeconds = 0f, string i_assetRoot = null,
			string i_visualFile = null, string i_activatedVisualFile = null, float i_visualPixelsPerUnit = 32f,
			float i_visualPivotX = 0.5f, float i_visualPivotY = 0.5f,
			string i_visualSortingLayer = "Decoration", int i_visualSortingOrder = 0)
		{
			Area = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Trigger = i_trigger;
			Price = i_price;
			SingleUse = i_singleUse;
			Message = i_message;
			DoorAction = i_doorAction;
			TargetDoorIds = i_targetDoorIds;
			MinWave = i_minWave;
			MaxWave = i_maxWave;
			ActivationsRequired = i_activationsRequired;
			DelaySeconds = i_delaySeconds;
			GiveMoney = i_giveMoney;
			LightAction = i_lightAction;
			TargetLightIds = i_targetLightIds;
			TargetSpawnerIds = i_targetSpawnerIds;
			SpawnCount = i_spawnCount;
			TargetInteractionIds = i_targetInteractionIds;
			RequiredItem = i_requiredItem;
			ConsumeRequiredItem = i_consumeRequiredItem;
			GiveItem = i_giveItem;
			GiveItemAmount = i_giveItemAmount;
			VisualArt = i_visualArt;
			VisualScale = i_visualScale;
			AssetRoot = i_assetRoot ?? string.Empty;
			VisualFile = i_visualFile ?? string.Empty;
			ActivatedVisualFile = i_activatedVisualFile ?? string.Empty;
			VisualPixelsPerUnit = i_visualPixelsPerUnit;
			VisualPivotX = i_visualPivotX;
			VisualPivotY = i_visualPivotY;
			VisualSortingLayer = string.IsNullOrWhiteSpace(i_visualSortingLayer) ? "Decoration" : i_visualSortingLayer;
			VisualSortingOrder = i_visualSortingOrder;
			MinEnemies = i_minEnemies;
			MaxEnemies = i_maxEnemies;
			RequiredChallenges = i_requiredChallenges ?? new ContentId[0];
			RequiredInteractionIds = i_requiredInteractionIds ?? new string[0];
			RequiredMoney = i_requiredMoney;
			RestoreHealth = i_restoreHealth;
			KnockbackX = i_knockbackX;
			KnockbackY = i_knockbackY;
			RagdollSeconds = i_ragdollSeconds;
		}
	}

	public sealed class TiledLightDefinition
	{
		public TiledLevelObject Point { get; }
		public bool InitiallyOn { get; }
		public float Flicker { get; }
		public bool Interactive { get; }
		public string AssetRoot { get; }
		public string VisualFile { get; }
		public string ActivatedVisualFile { get; }
		public float PixelsPerUnit { get; }
		public float PivotX { get; }
		public float PivotY { get; }
		public string SortingLayer { get; }
		public int SortingOrder { get; }
		public string InitialColor { get; }
		public string ActivatedColor { get; }
		public float InnerRadius { get; }
		public float OuterRadius { get; }
		public float FalloffIntensity { get; }

		internal TiledLightDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			bool i_initiallyOn, float i_flicker, bool i_interactive = true, string i_assetRoot = null, string i_visualFile = null,
			string i_activatedVisualFile = null, float i_pixelsPerUnit = 32f, float i_pivotX = 0.5f,
			float i_pivotY = 0.5f, string i_sortingLayer = "Lighting", int i_sortingOrder = 0,
			string i_initialColor = "#FFFFFF", string i_activatedColor = "#FFFFFF",
			float i_innerRadius = 0f, float i_outerRadius = 1f, float i_falloffIntensity = 0.5f)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			InitiallyOn = i_initiallyOn;
			Flicker = i_flicker;
			Interactive = i_interactive;
			AssetRoot = i_assetRoot ?? string.Empty;
			VisualFile = i_visualFile ?? string.Empty;
			ActivatedVisualFile = i_activatedVisualFile ?? string.Empty;
			PixelsPerUnit = i_pixelsPerUnit;
			PivotX = i_pivotX;
			PivotY = i_pivotY;
			SortingLayer = string.IsNullOrWhiteSpace(i_sortingLayer) ? "Lighting" : i_sortingLayer;
			SortingOrder = i_sortingOrder;
			InitialColor = i_initialColor ?? "#FFFFFF";
			ActivatedColor = i_activatedColor ?? "#FFFFFF";
			InnerRadius = i_innerRadius;
			OuterRadius = i_outerRadius;
			FalloffIntensity = i_falloffIntensity;
		}
	}

	public sealed class TiledProximityLightDefinition
	{
		public TiledLevelObject Point { get; }
		public string AssetRoot { get; }
		public string FilePath { get; }
		public bool InitiallyActive { get; }
		public float DetectionRadius { get; }
		public float FadeSeconds { get; }
		public float Intensity { get; }
		public string LightType { get; }
		public string Color { get; }
		public float FalloffIntensity { get; }
		public float InnerRadius { get; }
		public float OuterRadius { get; }
		public float LightOffsetY { get; }
		public float LightRotationZ { get; }
		public float ShapeFalloffSize { get; }
		public float[] ShapePath { get; }
		public string SortingLayer { get; }
		public int SortingOrder { get; }

		internal TiledProximityLightDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_assetRoot, string i_filePath, bool i_initiallyActive, float i_detectionRadius,
			float i_fadeSeconds, float i_intensity, string i_lightType, string i_color,
			float i_falloffIntensity, float i_innerRadius, float i_outerRadius, float i_lightOffsetY,
			float i_lightRotationZ, float i_shapeFalloffSize, float[] i_shapePath, string i_sortingLayer, int i_sortingOrder)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			AssetRoot = i_assetRoot;
			FilePath = i_filePath;
			InitiallyActive = i_initiallyActive;
			DetectionRadius = i_detectionRadius;
			FadeSeconds = i_fadeSeconds;
			Intensity = i_intensity;
			LightType = i_lightType;
			Color = i_color;
			FalloffIntensity = i_falloffIntensity;
			InnerRadius = i_innerRadius;
			OuterRadius = i_outerRadius;
			LightOffsetY = i_lightOffsetY;
			LightRotationZ = i_lightRotationZ;
			ShapeFalloffSize = i_shapeFalloffSize;
			ShapePath = i_shapePath;
			SortingLayer = i_sortingLayer;
			SortingOrder = i_sortingOrder;
		}
	}

	public sealed class TiledFreeformLightDefinition
	{
		public TiledLevelObject Point { get; }
		public bool InitiallyActive { get; }
		public float Intensity { get; }
		public string Color { get; }
		public float FalloffIntensity { get; }
		public float ShapeFalloffSize { get; }
		public float[] ShapePath { get; }
		public string[] SortingLayers { get; }
		public string SuppressInheritedPath { get; }
		public string AssetRoot { get; }
		public string VisualFile { get; }
		public float PixelsPerUnit { get; }
		public string SortingLayer { get; }
		public int SortingOrder { get; }
		public float LightOffsetY { get; }
		public float LightRotationZ { get; }

		internal TiledFreeformLightDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			bool i_initiallyActive, float i_intensity, string i_color, float i_falloffIntensity,
			float i_shapeFalloffSize, float[] i_shapePath, string[] i_sortingLayers,
			string i_suppressInheritedPath, string i_assetRoot, string i_visualFile,
			float i_pixelsPerUnit, string i_sortingLayer, int i_sortingOrder,
			float i_lightOffsetY, float i_lightRotationZ)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			InitiallyActive = i_initiallyActive;
			Intensity = i_intensity;
			Color = i_color;
			FalloffIntensity = i_falloffIntensity;
			ShapeFalloffSize = i_shapeFalloffSize;
			ShapePath = i_shapePath;
			SortingLayers = i_sortingLayers;
			SuppressInheritedPath = i_suppressInheritedPath;
			AssetRoot = i_assetRoot;
			VisualFile = i_visualFile;
			PixelsPerUnit = i_pixelsPerUnit;
			SortingLayer = i_sortingLayer;
			SortingOrder = i_sortingOrder;
			LightOffsetY = i_lightOffsetY;
			LightRotationZ = i_lightRotationZ;
		}
	}

	public sealed class TiledGlobalLightDefinition
	{
		public TiledLevelObject Point { get; }
		public float Intensity { get; }
		public string Color { get; }
		public float FalloffIntensity { get; }
		public string[] SortingLayers { get; }

		internal TiledGlobalLightDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			float i_intensity, string i_color, float i_falloffIntensity, string[] i_sortingLayers)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Intensity = i_intensity;
			Color = i_color;
			FalloffIntensity = i_falloffIntensity;
			SortingLayers = i_sortingLayers;
		}
	}

	public sealed class TiledPointLightDefinition
	{
		public TiledLevelObject Point { get; }
		public bool InitiallyActive { get; }
		public float Intensity { get; }
		public string Color { get; }
		public float FalloffIntensity { get; }
		public float InnerRadius { get; }
		public float OuterRadius { get; }
		public float InnerAngle { get; }
		public float OuterAngle { get; }
		public float RotationZ { get; }
		public string[] SortingLayers { get; }
		public string OverlapOperation { get; }
		public float PulseFrom { get; }
		public float PulseTo { get; }
		public float PulseSeconds { get; }
		public string SuppressInheritedPath { get; }

		internal TiledPointLightDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			bool i_initiallyActive, float i_intensity, string i_color, float i_falloffIntensity,
			float i_innerRadius, float i_outerRadius, float i_innerAngle, float i_outerAngle,
			float i_rotationZ, string[] i_sortingLayers, string i_overlapOperation,
			float i_pulseFrom, float i_pulseTo, float i_pulseSeconds, string i_suppressInheritedPath)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			InitiallyActive = i_initiallyActive;
			Intensity = i_intensity;
			Color = i_color;
			FalloffIntensity = i_falloffIntensity;
			InnerRadius = i_innerRadius;
			OuterRadius = i_outerRadius;
			InnerAngle = i_innerAngle;
			OuterAngle = i_outerAngle;
			RotationZ = i_rotationZ;
			SortingLayers = i_sortingLayers;
			OverlapOperation = i_overlapOperation;
			PulseFrom = i_pulseFrom;
			PulseTo = i_pulseTo;
			PulseSeconds = i_pulseSeconds;
			SuppressInheritedPath = i_suppressInheritedPath;
		}
	}

	public sealed class TiledPickupDefinition
	{
		public TiledLevelObject Point { get; }
		public ContentId Item { get; }
		public int Amount { get; }
		public bool InitiallyKinematic { get; }

		internal TiledPickupDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			ContentId i_item, int i_amount, bool i_initiallyKinematic)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Item = i_item;
			Amount = i_amount;
			InitiallyKinematic = i_initiallyKinematic;
		}
	}

	public sealed class TiledStageItemDefinition
	{
		public TiledLevelObject Point { get; }
		public string AssetRoot { get; }
		public string FilePath { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public string OnPickupSignal { get; }
		public float PixelsPerUnit { get; }
		public float PivotX { get; }
		public float PivotY { get; }
		public float ColliderWidth { get; }
		public float ColliderHeight { get; }
		public float ColliderOffsetX { get; }
		public float ColliderOffsetY { get; }
		public int Weight { get; }
		public int Value { get; }
		public bool CanDrop { get; }
		public bool InitiallyKinematic { get; }
		public string LightColor { get; }
		public float LightOffsetX { get; }
		public float LightOffsetY { get; }
		public float LightRadius { get; }
		public float LightFalloffSize { get; }
		public int LightSides { get; }
		public float LightIntensity { get; }
		public float LightFalloffIntensity { get; }

		internal TiledStageItemDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_assetRoot, string i_filePath, string i_displayName, string i_description,
			string i_onPickupSignal, float i_pixelsPerUnit, float i_pivotX, float i_pivotY,
			float i_colliderWidth, float i_colliderHeight, float i_colliderOffsetX, float i_colliderOffsetY,
			int i_weight, int i_value, bool i_canDrop, bool i_initiallyKinematic, string i_lightColor, float i_lightOffsetX,
			float i_lightOffsetY, float i_lightRadius, float i_lightFalloffSize, int i_lightSides,
			float i_lightIntensity, float i_lightFalloffIntensity)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			AssetRoot = i_assetRoot; FilePath = i_filePath; DisplayName = i_displayName;
			Description = i_description; OnPickupSignal = i_onPickupSignal; PixelsPerUnit = i_pixelsPerUnit;
			PivotX = i_pivotX; PivotY = i_pivotY; ColliderWidth = i_colliderWidth; ColliderHeight = i_colliderHeight;
			ColliderOffsetX = i_colliderOffsetX; ColliderOffsetY = i_colliderOffsetY;
			Weight = i_weight; Value = i_value; CanDrop = i_canDrop; InitiallyKinematic = i_initiallyKinematic;
			LightColor = i_lightColor; LightOffsetX = i_lightOffsetX; LightOffsetY = i_lightOffsetY;
			LightRadius = i_lightRadius; LightFalloffSize = i_lightFalloffSize; LightSides = i_lightSides;
			LightIntensity = i_lightIntensity; LightFalloffIntensity = i_lightFalloffIntensity;
		}
	}

	public sealed class TiledMovingPlatformDefinition
	{
		public TiledLevelObject Rectangle { get; }
		public float OffsetX { get; }
		public float OffsetY { get; }
		public float TravelSeconds { get; }
		public float PauseSeconds { get; }
		public CoreMapArtCatalogEntry Art { get; }

		internal TiledMovingPlatformDefinition(TiledObjectDocument i_object, float i_layerOffsetX,
			float i_layerOffsetY, float i_offsetX, float i_offsetY, float i_travelSeconds, float i_pauseSeconds,
			CoreMapArtCatalogEntry i_art)
		{
			Rectangle = new TiledLevelObject(i_object, i_layerOffsetX, i_layerOffsetY);
			OffsetX = i_offsetX;
			OffsetY = i_offsetY;
			TravelSeconds = i_travelSeconds;
			PauseSeconds = i_pauseSeconds;
			Art = i_art;
		}
	}

	public sealed class TiledParticleDefinition
	{
		public TiledLevelObject Point { get; }
		public string Color { get; }
		public float Rate { get; }
		public float Lifetime { get; }
		public float Speed { get; }
		public float Size { get; }
		public float Radius { get; }

		internal TiledParticleDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_color, float i_rate, float i_lifetime, float i_speed, float i_size, float i_radius)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Color = i_color; Rate = i_rate; Lifetime = i_lifetime; Speed = i_speed; Size = i_size; Radius = i_radius;
		}
	}

	public sealed class TiledNavNodeDefinition
	{
		public TiledLevelObject Point { get; }
		public bool IsFly { get; }
		public string ConnectionType { get; }
		public bool Bidirectional { get; }
		public IReadOnlyList<string> Links { get; }

		internal TiledNavNodeDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			bool i_isFly, string i_connectionType, bool i_bidirectional, IReadOnlyList<string> i_links)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			IsFly = i_isFly;
			ConnectionType = i_connectionType;
			Bidirectional = i_bidirectional;
			Links = i_links;
		}
	}

	public sealed class TiledRoomDefinition
	{
		public TiledLevelObject Rectangle { get; }
		public bool Initial { get; }
		internal TiledRoomDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY, bool i_initial)
		{
			Rectangle = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Initial = i_initial;
		}
	}

	public sealed class TiledRoomEntryDefinition
	{
		public TiledLevelObject Point { get; }
		public string RoomId { get; }
		internal TiledRoomEntryDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY, string i_roomId)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			RoomId = i_roomId;
		}
	}

	public sealed class TiledRoomTransitionDefinition
	{
		public TiledLevelObject Rectangle { get; }
		public string DestinationId { get; }
		public bool RequireNoEnemies { get; }
		public bool OneShot { get; }
		internal TiledRoomTransitionDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_destinationId, bool i_requireNoEnemies, bool i_oneShot)
		{
			Rectangle = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			DestinationId = i_destinationId;
			RequireNoEnemies = i_requireNoEnemies;
			OneShot = i_oneShot;
		}
	}

	public sealed class TiledAudioDefinition
	{
		public TiledLevelObject Point { get; }
		public string AssetRoot { get; }
		public string FilePath { get; }
		public bool Ambient { get; }
		public bool Loop { get; }
		public bool PlayOnStart { get; }
		public string MixerGroup { get; }
		public float Volume { get; }
		public float MinDistance { get; }
		public float MaxDistance { get; }

		internal TiledAudioDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_assetRoot, string i_filePath, bool i_ambient, bool i_loop, bool i_playOnStart, string i_mixerGroup, float i_volume,
			float i_minDistance, float i_maxDistance)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			AssetRoot = i_assetRoot;
			FilePath = i_filePath;
			Ambient = i_ambient;
			Loop = i_loop;
			PlayOnStart = i_playOnStart;
			MixerGroup = i_mixerGroup;
			Volume = i_volume;
			MinDistance = i_minDistance;
			MaxDistance = i_maxDistance;
		}
	}

	public sealed class CoreMapArtCatalogEntry
	{
		public string Id { get; }
		public string SpriteName { get; }
		public string DefaultSortingLayer { get; }

		internal CoreMapArtCatalogEntry(string i_id, string i_spriteName, string i_defaultSortingLayer)
		{
			Id = i_id;
			SpriteName = i_spriteName;
			DefaultSortingLayer = i_defaultSortingLayer;
		}
	}

	public static class CoreMapArtCatalog
	{
		private static readonly Dictionary<string, CoreMapArtCatalogEntry> Entries =
			new Dictionary<string, CoreMapArtCatalogEntry>(StringComparer.Ordinal)
			{
				{ "field-day-yard-dirt", new CoreMapArtCatalogEntry("field-day-yard-dirt", "YardDirt", "Background") },
				{ "field-day-grass-platform", new CoreMapArtCatalogEntry("field-day-grass-platform", "YardGrassPlatform", "Platform") },
				{ "jungle-trees", new CoreMapArtCatalogEntry("jungle-trees", "BgTrees", "Background") },
				{ "jungle-hut", new CoreMapArtCatalogEntry("jungle-hut", "HutBg", "Background") },
				{ "space-station-bay", new CoreMapArtCatalogEntry("space-station-bay", "BayBg", "Background") },
				{ "fer-lab", new CoreMapArtCatalogEntry("fer-lab", "bgLab", "Background") },
				{ "fer-corridor", new CoreMapArtCatalogEntry("fer-corridor", "bgCorridor", "Background") },
				{ "welcome-poster", new CoreMapArtCatalogEntry("welcome-poster", "WelcomePoster", "Decoration") },
				{ "lamp", new CoreMapArtCatalogEntry("lamp", "Lamp", "Decoration") },
				{ "chair", new CoreMapArtCatalogEntry("chair", "Chair", "Decoration") },
				{ "barrel", new CoreMapArtCatalogEntry("barrel", "barrel", "Decoration") },
				{ "table", new CoreMapArtCatalogEntry("table", "Table", "Decoration") },
				{ "stage-lights", new CoreMapArtCatalogEntry("stage-lights", "stageLights", "Decoration") },
				{ "jungle-tree-mid", new CoreMapArtCatalogEntry("jungle-tree-mid", "treeMid", "Decoration") },
				{ "jungle-tree-root", new CoreMapArtCatalogEntry("jungle-tree-root", "treeRoot", "Decoration") },
				{ "jungle-tree-platform", new CoreMapArtCatalogEntry("jungle-tree-platform", "treePlatform", "Platform") },
				{ "vegetation-floor", new CoreMapArtCatalogEntry("vegetation-floor", "vegetationfloor", "Platform") },
				{ "cave-work-light-head", new CoreMapArtCatalogEntry("cave-work-light-head", "WorkLightHead", "Decoration") },
				{ "cave-work-light-stand", new CoreMapArtCatalogEntry("cave-work-light-stand", "WorkLightStand", "Decoration") },
				{ "cave-wall-light", new CoreMapArtCatalogEntry("cave-wall-light", "wallLight", "Decoration") },
				{ "cave-flashlight", new CoreMapArtCatalogEntry("cave-flashlight", "FlashLight", "Decoration") },
				{ "space-wife-poster", new CoreMapArtCatalogEntry("space-wife-poster", "PosterWifeBedroom", "Decoration") },
				{ "shack-tv-chair", new CoreMapArtCatalogEntry("shack-tv-chair", "tvChair", "Decoration") },
				{ "small-table-1", new CoreMapArtCatalogEntry("small-table-1", "Table1", "Decoration") },
				{ "small-table-2", new CoreMapArtCatalogEntry("small-table-2", "Table2", "Decoration") },
				{ "vase-flowers", new CoreMapArtCatalogEntry("vase-flowers", "VaseFlowers", "Decoration") },
				{ "fer-desk", new CoreMapArtCatalogEntry("fer-desk", "desk", "Decoration") },
				{ "fer-electric-light", new CoreMapArtCatalogEntry("fer-electric-light", "ElectricLightOn", "Decoration") },
				{ "fer-brain-machine", new CoreMapArtCatalogEntry("fer-brain-machine", "brainMachineBase", "Decoration") },
				{ "fer-intensive-care-machine", new CoreMapArtCatalogEntry("fer-intensive-care-machine", "IntensiveCareMachine", "Decoration") },
				{ "weapon-vendor", new CoreMapArtCatalogEntry("weapon-vendor", "WeaponVendor", "Decoration") },
				{ "usable-vendor", new CoreMapArtCatalogEntry("usable-vendor", "UsableVendor", "Decoration") },
				{ "light-switch-off", new CoreMapArtCatalogEntry("light-switch-off", "lightSwitchOff", "Decoration") },
				{ "light-switch-on", new CoreMapArtCatalogEntry("light-switch-on", "lightSwitchOn", "Decoration") },
				{ "fer-ball-contraption", new CoreMapArtCatalogEntry("fer-ball-contraption", "ballContraption", "Decoration") },
				{ "fer-theatre", new CoreMapArtCatalogEntry("fer-theatre", "TheatreBg", "Background") },
				{ "fer-theatre-back", new CoreMapArtCatalogEntry("fer-theatre-back", "theatreBackBackground", "Background") },
				{ "fer-crude-table", new CoreMapArtCatalogEntry("fer-crude-table", "shittyTable", "Decoration") },
				{ "fer-whiteboard", new CoreMapArtCatalogEntry("fer-whiteboard", "whiteboard", "Decoration") },
				{ "hub-code-note", new CoreMapArtCatalogEntry("hub-code-note", "noteWall", "Decoration") },
				{ "hub-wardrobe", new CoreMapArtCatalogEntry("hub-wardrobe", "Wardrobe", "Decoration") }
			};

		public static bool TryGet(string i_id, out CoreMapArtCatalogEntry o_entry)
		{
			return Entries.TryGetValue(i_id ?? string.Empty, out o_entry);
		}
	}

	public sealed class TiledCoreArtDefinition
	{
		public TiledLevelObject Rectangle { get; }
		public CoreMapArtCatalogEntry Art { get; }
		public string SortingLayer { get; }
		public float Opacity { get; }
		public int SortingOrder { get; }
		public float Rotation { get; }
		public bool FlipX { get; }
		public bool FlipY { get; }

		internal TiledCoreArtDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			CoreMapArtCatalogEntry i_art, string i_sortingLayer, float i_opacity, int i_sortingOrder)
		{
			Rectangle = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			Art = i_art;
			SortingLayer = i_sortingLayer;
			Opacity = i_opacity;
			SortingOrder = i_sortingOrder;
			Rotation = i_object.Rotation;
			FlipX = ReadBool(i_object.Properties, "flipX");
			FlipY = ReadBool(i_object.Properties, "flipY");
		}

		private static bool ReadBool(IEnumerable<TiledPropertyDocument> i_properties, string i_name)
		{
			if (i_properties == null) return false;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && property.Name == i_name && property.Value is bool value) return value;
			return false;
		}
	}

	/// <summary>Explicit adapter for a selected object in the inherited Core stage.</summary>
	public sealed class TiledCoreStageObjectDefinition
	{
		public TiledLevelObject Point { get; }
		public string SourcePath { get; }
		public string Kind { get; }
		public bool InitiallyActive { get; }

		internal TiledCoreStageObjectDefinition(TiledObjectDocument i_object, float i_offsetX, float i_offsetY,
			string i_sourcePath, string i_kind, bool i_initiallyActive)
		{
			Point = new TiledLevelObject(i_object, i_offsetX, i_offsetY);
			SourcePath = i_sourcePath;
			Kind = i_kind;
			InitiallyActive = i_initiallyActive;
		}
	}

	public sealed class TiledLevelDefinition
	{
		public int WidthPixels { get; }
		public int HeightPixels { get; }
		public int TileWidth { get; }
		public int TileHeight { get; }
		public IReadOnlyList<TiledLevelObject> Platforms { get; }
		public TiledLevelObject PlayerSpawn { get; }
		public IReadOnlyList<TiledLevelObject> EnemySpawners { get; }
		public IReadOnlyList<TiledTilesetDefinition> Tilesets { get; }
		public IReadOnlyList<TiledTileLayerDefinition> TileLayers { get; }
		public IReadOnlyList<TiledImageLayerDefinition> ImageLayers { get; }
		public IReadOnlyList<TiledDecorationDefinition> Decorations { get; }
		public IReadOnlyList<TiledLevelObject> WeaponVendors { get; }
		public IReadOnlyList<TiledLevelObject> UsableVendors { get; }
		public IReadOnlyList<TiledWeaponCaseDefinition> WeaponCases { get; }
		public IReadOnlyList<TiledDoorDefinition> Doors { get; }
		public IReadOnlyList<TiledScriptedActorDefinition> ScriptedActors { get; }
		public IReadOnlyList<TiledDoorSwitchDefinition> DoorSwitches { get; }
		public IReadOnlyList<TiledCoreArtDefinition> CoreArt { get; }
		public IReadOnlyList<TiledNoteDefinition> Notes { get; }
		public IReadOnlyList<TiledKeypadDefinition> Keypads { get; }
		public IReadOnlyList<TiledLevelObject> Altars { get; }
		public IReadOnlyList<TiledInteractionDefinition> Interactions { get; }
		public IReadOnlyList<TiledLightDefinition> Lights { get; }
		public IReadOnlyList<TiledProximityLightDefinition> ProximityLights { get; }
		public IReadOnlyList<TiledFreeformLightDefinition> FreeformLights { get; }
		public IReadOnlyList<TiledGlobalLightDefinition> GlobalLights { get; }
		public IReadOnlyList<TiledPointLightDefinition> PointLights { get; }
		public IReadOnlyList<TiledPickupDefinition> Pickups { get; }
		public IReadOnlyList<TiledStageItemDefinition> StageItems { get; }
		public IReadOnlyList<TiledMovingPlatformDefinition> MovingPlatforms { get; }
		public IReadOnlyList<TiledParticleDefinition> Particles { get; }
		public IReadOnlyList<TiledNavNodeDefinition> NavNodes { get; }
		public IReadOnlyList<TiledAudioDefinition> AudioSources { get; }
		public IReadOnlyList<TiledRoomDefinition> Rooms { get; }
		public IReadOnlyList<TiledRoomEntryDefinition> RoomEntries { get; }
		public IReadOnlyList<TiledRoomTransitionDefinition> RoomTransitions { get; }
		public IReadOnlyList<TiledLevelObject> ScriptTriggers { get; }
		public IReadOnlyList<TiledLevelObject> StageMarkers { get; }
		public IReadOnlyList<TiledCoreStageObjectDefinition> CoreStageObjects { get; }

		internal TiledLevelDefinition(int i_widthPixels, int i_heightPixels, int i_tileWidth, int i_tileHeight,
			IReadOnlyList<TiledLevelObject> i_platforms, TiledLevelObject i_playerSpawn,
			IReadOnlyList<TiledLevelObject> i_enemySpawners,
			IReadOnlyList<TiledTilesetDefinition> i_tilesets,
			IReadOnlyList<TiledTileLayerDefinition> i_tileLayers,
			IReadOnlyList<TiledImageLayerDefinition> i_imageLayers,
			IReadOnlyList<TiledDecorationDefinition> i_decorations,
			IReadOnlyList<TiledLevelObject> i_weaponVendors,
			IReadOnlyList<TiledLevelObject> i_usableVendors,
			IReadOnlyList<TiledWeaponCaseDefinition> i_weaponCases,
			IReadOnlyList<TiledDoorDefinition> i_doors,
			IReadOnlyList<TiledScriptedActorDefinition> i_scriptedActors,
			IReadOnlyList<TiledDoorSwitchDefinition> i_doorSwitches,
			IReadOnlyList<TiledCoreArtDefinition> i_coreArt,
			IReadOnlyList<TiledNoteDefinition> i_notes,
			IReadOnlyList<TiledKeypadDefinition> i_keypads,
			IReadOnlyList<TiledLevelObject> i_altars,
			IReadOnlyList<TiledInteractionDefinition> i_interactions,
			IReadOnlyList<TiledLightDefinition> i_lights,
			IReadOnlyList<TiledProximityLightDefinition> i_proximityLights,
			IReadOnlyList<TiledFreeformLightDefinition> i_freeformLights,
			IReadOnlyList<TiledGlobalLightDefinition> i_globalLights,
			IReadOnlyList<TiledPointLightDefinition> i_pointLights,
			IReadOnlyList<TiledPickupDefinition> i_pickups,
			IReadOnlyList<TiledStageItemDefinition> i_stageItems,
			IReadOnlyList<TiledMovingPlatformDefinition> i_movingPlatforms,
			IReadOnlyList<TiledParticleDefinition> i_particles,
			IReadOnlyList<TiledNavNodeDefinition> i_navNodes,
			IReadOnlyList<TiledAudioDefinition> i_audioSources,
			IReadOnlyList<TiledRoomDefinition> i_rooms,
			IReadOnlyList<TiledRoomEntryDefinition> i_roomEntries,
			IReadOnlyList<TiledRoomTransitionDefinition> i_roomTransitions,
			IReadOnlyList<TiledLevelObject> i_scriptTriggers,
			IReadOnlyList<TiledLevelObject> i_stageMarkers,
			IReadOnlyList<TiledCoreStageObjectDefinition> i_coreStageObjects)
		{
			WidthPixels = i_widthPixels;
			HeightPixels = i_heightPixels;
			TileWidth = i_tileWidth;
			TileHeight = i_tileHeight;
			Platforms = i_platforms;
			PlayerSpawn = i_playerSpawn;
			EnemySpawners = i_enemySpawners;
			Tilesets = i_tilesets;
			TileLayers = i_tileLayers;
			ImageLayers = i_imageLayers;
			Decorations = i_decorations;
			WeaponVendors = i_weaponVendors;
			UsableVendors = i_usableVendors;
			WeaponCases = i_weaponCases;
			Doors = i_doors;
			ScriptedActors = i_scriptedActors;
			DoorSwitches = i_doorSwitches;
			CoreArt = i_coreArt;
			Notes = i_notes;
			Keypads = i_keypads;
			Altars = i_altars;
			Interactions = i_interactions;
			Lights = i_lights;
			ProximityLights = i_proximityLights;
			FreeformLights = i_freeformLights;
			GlobalLights = i_globalLights;
			PointLights = i_pointLights;
			Pickups = i_pickups;
			StageItems = i_stageItems;
			MovingPlatforms = i_movingPlatforms;
			Particles = i_particles;
			NavNodes = i_navNodes;
			AudioSources = i_audioSources;
			Rooms = i_rooms;
			RoomEntries = i_roomEntries;
			RoomTransitions = i_roomTransitions;
			ScriptTriggers = i_scriptTriggers;
			StageMarkers = i_stageMarkers;
			CoreStageObjects = i_coreStageObjects;
		}

		public StagePointDefinition ToUnityPoint(TiledLevelObject i_object, int i_pixelsPerUnit)
		{
			return new StagePointDefinition
			{
				X = (i_object.X - WidthPixels * 0.5f) / i_pixelsPerUnit,
				Y = (HeightPixels - i_object.Y) / i_pixelsPerUnit
			};
		}

		public StagePointDefinition ToUnityRectangleCenter(TiledLevelObject i_object, int i_pixelsPerUnit)
		{
			if (i_object.IsTileObject) return ToUnityPoint(i_object, i_pixelsPerUnit);
			return new StagePointDefinition
			{
				X = (i_object.X + i_object.Width * 0.5f - WidthPixels * 0.5f) / i_pixelsPerUnit,
				Y = (HeightPixels - i_object.Y - i_object.Height * 0.5f) / i_pixelsPerUnit
			};
		}
	}

	public sealed class TiledLevelLoadResult
	{
		public TiledLevelDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class TiledLevelParser
	{
		public const int MaximumJsonBytes = 16 * 1024 * 1024;

		public static TiledLevelLoadResult Load(string i_packRoot, string i_relativePath, string i_source)
		{
			TiledLevelLoadResult result = new TiledLevelLoadResult();
			string path = i_relativePath ?? string.Empty;
			try
			{
				path = Path.GetFullPath(Path.Combine(i_packRoot ?? string.Empty, path));
				if (!ModPath.IsSafeRelativePath(i_relativePath) || !AssetPatchDiscovery.IsInside(path, i_packRoot)
					|| !File.Exists(path))
				{
					result.Report.Add(ValidationSeverity.Error, "tiled.file", "Tiled JSON is missing or outside its pack: " + i_relativePath, i_source);
					return result;
				}
				FileInfo file = new FileInfo(path);
				if (file.Length <= 0 || file.Length > MaximumJsonBytes)
				{
					result.Report.Add(ValidationSeverity.Error, "tiled.file-size", "Tiled JSON must be between 1 byte and 16 MiB.", path);
					return result;
				}
				return ParseInternal(File.ReadAllText(path), path, Path.GetFullPath(i_packRoot ?? string.Empty), Path.GetDirectoryName(path));
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException
				|| exception is ArgumentException || exception is NotSupportedException)
			{
				result.Report.Add(ValidationSeverity.Error, "tiled.read", exception.Message, path);
				return result;
			}
		}

		public static TiledLevelLoadResult Parse(string i_json, string i_source)
		{
			return ParseInternal(i_json, i_source, null, null);
		}

		private static TiledLevelLoadResult ParseInternal(string i_json, string i_source,
			string i_packRoot, string i_mapDirectory)
		{
			TiledLevelLoadResult result = new TiledLevelLoadResult();
			TiledMapDocument map;
			try
			{
				JObject mapJson = JObject.Parse(i_json);
				int resolvedTemplateCount = 0;
				ResolveObjectTemplates(mapJson, i_packRoot, i_mapDirectory, new HashSet<string>(StringComparer.OrdinalIgnoreCase),
					ref resolvedTemplateCount, result.Report, i_source);
				map = mapJson.ToObject<TiledMapDocument>();
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "tiled.json", exception.Message, i_source);
				return result;
			}
			if (map == null)
			{
				result.Report.Add(ValidationSeverity.Error, "tiled.null", "Tiled map resolved to null.", i_source);
				return result;
			}
			if (!string.Equals(map.Orientation, "orthogonal", StringComparison.Ordinal))
				Error(result, "orientation", "Only orthogonal Tiled maps are supported.", i_source);
			if (map.Infinite) Error(result, "infinite", "Infinite Tiled maps are not supported in the first authored-layout version.", i_source);
			if (map.Width < 1 || map.Height < 1 || map.TileWidth < 1 || map.TileHeight < 1
				|| (long)map.Width * map.TileWidth > 1000000 || (long)map.Height * map.TileHeight > 1000000)
				Error(result, "dimensions", "Map and tile dimensions must be positive and no axis may exceed 1,000,000 pixels.", i_source);

			List<TiledLevelObject> platforms = new List<TiledLevelObject>();
			List<TiledLevelObject> playerSpawns = new List<TiledLevelObject>();
			List<TiledLevelObject> enemySpawners = new List<TiledLevelObject>();
			List<TiledTilesetDefinition> tilesets = ReadTilesets(map.Tilesets, i_packRoot, i_mapDirectory, result.Report, i_source);
			List<TiledTileLayerDefinition> tileLayers = new List<TiledTileLayerDefinition>();
			List<TiledImageLayerDefinition> imageLayers = new List<TiledImageLayerDefinition>();
			List<TiledDecorationDefinition> decorations = new List<TiledDecorationDefinition>();
			List<TiledLevelObject> weaponVendors = new List<TiledLevelObject>();
			List<TiledLevelObject> usableVendors = new List<TiledLevelObject>();
			List<TiledWeaponCaseDefinition> weaponCases = new List<TiledWeaponCaseDefinition>();
			List<TiledDoorDefinition> doors = new List<TiledDoorDefinition>();
			List<TiledScriptedActorDefinition> scriptedActors = new List<TiledScriptedActorDefinition>();
			List<TiledDoorSwitchDefinition> doorSwitches = new List<TiledDoorSwitchDefinition>();
			List<TiledCoreArtDefinition> coreArt = new List<TiledCoreArtDefinition>();
			List<TiledNoteDefinition> notes = new List<TiledNoteDefinition>();
			List<TiledKeypadDefinition> keypads = new List<TiledKeypadDefinition>();
			List<TiledLevelObject> altars = new List<TiledLevelObject>();
			List<TiledInteractionDefinition> interactions = new List<TiledInteractionDefinition>();
			List<TiledLightDefinition> lights = new List<TiledLightDefinition>();
			List<TiledProximityLightDefinition> proximityLights = new List<TiledProximityLightDefinition>();
			List<TiledFreeformLightDefinition> freeformLights = new List<TiledFreeformLightDefinition>();
			List<TiledGlobalLightDefinition> globalLights = new List<TiledGlobalLightDefinition>();
			List<TiledPointLightDefinition> pointLights = new List<TiledPointLightDefinition>();
			List<TiledPickupDefinition> pickups = new List<TiledPickupDefinition>();
			List<TiledStageItemDefinition> stageItems = new List<TiledStageItemDefinition>();
			List<TiledMovingPlatformDefinition> movingPlatforms = new List<TiledMovingPlatformDefinition>();
			List<TiledParticleDefinition> particles = new List<TiledParticleDefinition>();
			List<TiledNavNodeDefinition> navNodes = new List<TiledNavNodeDefinition>();
			List<TiledAudioDefinition> audioSources = new List<TiledAudioDefinition>();
			List<TiledRoomDefinition> rooms = new List<TiledRoomDefinition>();
			List<TiledRoomEntryDefinition> roomEntries = new List<TiledRoomEntryDefinition>();
			List<TiledRoomTransitionDefinition> roomTransitions = new List<TiledRoomTransitionDefinition>();
			List<TiledLevelObject> scriptTriggers = new List<TiledLevelObject>();
			List<TiledLevelObject> stageMarkers = new List<TiledLevelObject>();
			List<TiledCoreStageObjectDefinition> coreStageObjects = new List<TiledCoreStageObjectDefinition>();
			int drawOrder = 0;
			ReadLayers(map.Layers, 0f, 0f, 0, 0, 1f, true, map.TileWidth, map.TileHeight,
				platforms, playerSpawns, enemySpawners, tileLayers, imageLayers, decorations, weaponVendors, usableVendors, weaponCases,
				doors, scriptedActors, doorSwitches, coreArt, notes, keypads, altars, interactions, lights, proximityLights, freeformLights, globalLights, pointLights, pickups, stageItems, movingPlatforms, particles, navNodes, audioSources,
				rooms, roomEntries, roomTransitions, scriptTriggers, stageMarkers, coreStageObjects,
				ref drawOrder, i_packRoot, i_mapDirectory, result.Report, i_source);
			ValidateTileGids(tileLayers, decorations, tilesets, result.Report, i_source);
			if (platforms.Count == 0) Error(result, "platforms", "An authored level requires at least one platform rectangle.", i_source);
			if (playerSpawns.Count != 1) Error(result, "player-spawn", "An authored level requires exactly one player-spawn point.", i_source);
			if (enemySpawners.Count == 0) Error(result, "enemy-spawners", "An authored level requires at least one enemy-spawner point.", i_source);
			HashSet<string> spawnerNames = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledLevelObject spawner in enemySpawners)
				if (string.IsNullOrWhiteSpace(spawner.Name) || !spawnerNames.Add(spawner.Name))
					Error(result, "enemy-spawner-name", "Enemy-spawner point names must be non-empty and unique.", i_source);
			HashSet<string> doorIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledDoorDefinition door in doors)
				if (string.IsNullOrWhiteSpace(door.Point.Name) || !doorIds.Add(door.Point.Name))
					Error(result, "door-id", "Door object names must be non-empty and unique because switches use them as IDs.", i_source);
			HashSet<string> actorIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledScriptedActorDefinition actor in scriptedActors)
				if (string.IsNullOrWhiteSpace(actor.Point.Name) || !actorIds.Add(actor.Point.Name))
					Error(result, "scripted-actor-id", "Scripted-actor names must be non-empty and unique.", i_source);
			foreach (TiledDoorSwitchDefinition doorSwitch in doorSwitches)
				foreach (string target in doorSwitch.TargetDoorIds)
					if (!doorIds.Contains(target)) Error(result, "door-switch-target", "Door switch references unknown door ID: " + target, i_source);
			foreach (TiledKeypadDefinition keypad in keypads)
				foreach (string target in keypad.TargetDoorIds)
					if (!doorIds.Contains(target)) Error(result, "keypad-target", "Keypad references unknown door ID: " + target, i_source);
			HashSet<string> lightIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledLightDefinition light in lights)
				if (string.IsNullOrWhiteSpace(light.Point.Name) || !lightIds.Add(light.Point.Name))
					Error(result, "light-id", "Light object names must be non-empty and unique because interactions target them.", i_source);
			if (proximityLights.Count > 128) Error(result, "proximity-light-count", "A Tiled stage supports at most 128 proximity lights.", i_source);
			foreach (TiledProximityLightDefinition light in proximityLights)
				if (string.IsNullOrWhiteSpace(light.Point.Name) || !lightIds.Add(light.Point.Name))
					Error(result, "proximity-light-id", "Proximity-light names must be non-empty and unique among stage lights.", i_source);
			if (freeformLights.Count > 128) Error(result, "freeform-light-count", "A Tiled stage supports at most 128 freeform lights.", i_source);
			foreach (TiledFreeformLightDefinition light in freeformLights)
				if (string.IsNullOrWhiteSpace(light.Point.Name) || !lightIds.Add(light.Point.Name))
					Error(result, "freeform-light-id", "Freeform-light names must be non-empty and unique among stage lights.", i_source);
			if (globalLights.Count > 1) Error(result, "global-light-count", "A Tiled stage supports exactly one or no global-light point.", i_source);
			foreach (TiledGlobalLightDefinition light in globalLights)
				if (string.IsNullOrWhiteSpace(light.Point.Name) || !lightIds.Add(light.Point.Name))
					Error(result, "global-light-id", "Global-light names must be non-empty and unique among stage lights.", i_source);
			if (pointLights.Count > 128) Error(result, "point-light-count", "A Tiled stage supports at most 128 point lights.", i_source);
			foreach (TiledPointLightDefinition light in pointLights)
				if (string.IsNullOrWhiteSpace(light.Point.Name) || !lightIds.Add(light.Point.Name))
					Error(result, "point-light-id", "Point-light names must be non-empty and unique among stage lights.", i_source);
			HashSet<string> interactionIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledInteractionDefinition interaction in interactions)
				if (string.IsNullOrWhiteSpace(interaction.Area.Name) || !interactionIds.Add(interaction.Area.Name))
					Error(result, "interaction-id", "Interaction object names must be non-empty and unique because interactions can chain them.", i_source);
			foreach (TiledInteractionDefinition interaction in interactions)
			{
				foreach (string target in interaction.TargetDoorIds)
					if (!doorIds.Contains(target)) Error(result, "interaction-target", "Interaction references unknown door ID: " + target, i_source);
				foreach (string target in interaction.TargetLightIds)
					if (!lightIds.Contains(target)) Error(result, "interaction-light-target", "Interaction references unknown light ID: " + target, i_source);
				foreach (string target in interaction.TargetSpawnerIds)
					if (!spawnerNames.Contains(target)) Error(result, "interaction-spawner-target", "Interaction references unknown enemy-spawner ID: " + target, i_source);
				foreach (string target in interaction.TargetInteractionIds)
					if (!interactionIds.Contains(target) || target == interaction.Area.Name)
						Error(result, "interaction-chain-target", "Interaction references an unknown or self-referencing interaction ID: " + target, i_source);
				foreach (string required in interaction.RequiredInteractionIds)
					if (!interactionIds.Contains(required) || required == interaction.Area.Name)
						Error(result, "interaction-condition-target", "Interaction requires an unknown or self-referencing interaction ID: " + required, i_source);
			}
			ValidateInteractionCycles(interactions, result.Report, i_source);
			HashSet<string> navNodeIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledNavNodeDefinition node in navNodes)
				if (string.IsNullOrWhiteSpace(node.Point.Name) || !navNodeIds.Add(node.Point.Name))
					Error(result, "nav-node-id", "nav-node names must be non-empty and unique.", i_source);
			foreach (TiledNavNodeDefinition node in navNodes)
				foreach (string target in node.Links)
					if (!navNodeIds.Contains(target) || target == node.Point.Name)
						Error(result, "nav-node-link", "nav-node references an unknown or self link: " + target, i_source);
			HashSet<string> roomIds = new HashSet<string>(StringComparer.Ordinal);
			int initialRoomCount = 0;
			if (rooms.Count > 64) Error(result, "room-count", "A Tiled stage supports at most 64 rooms.", i_source);
			if (roomEntries.Count > 256 || roomTransitions.Count > 256)
				Error(result, "room-progression-count", "A Tiled stage supports at most 256 room entries and 256 transitions.", i_source);
			foreach (TiledRoomDefinition room in rooms)
			{
				if (string.IsNullOrWhiteSpace(room.Rectangle.Name) || !roomIds.Add(room.Rectangle.Name))
					Error(result, "room-id", "Room names must be non-empty and unique.", i_source);
				if (room.Initial) initialRoomCount++;
			}
			if (initialRoomCount > 1) Error(result, "room-initial", "At most one room may set initial=true.", i_source);
			HashSet<string> roomEntryIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledRoomEntryDefinition entry in roomEntries)
			{
				if (string.IsNullOrWhiteSpace(entry.Point.Name) || !roomEntryIds.Add(entry.Point.Name))
					Error(result, "room-entry-id", "Room-entry names must be non-empty and unique.", i_source);
				if (!roomIds.Contains(entry.RoomId))
					Error(result, "room-entry-room", "Room entry references an unknown room: " + entry.RoomId, i_source);
			}
			HashSet<string> transitionIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (TiledRoomTransitionDefinition transition in roomTransitions)
			{
				if (string.IsNullOrWhiteSpace(transition.Rectangle.Name) || !transitionIds.Add(transition.Rectangle.Name))
					Error(result, "room-transition-id", "Room-transition names must be non-empty and unique.", i_source);
				if (!roomEntryIds.Contains(transition.DestinationId))
					Error(result, "room-transition-destination", "Room transition references an unknown room entry: " + transition.DestinationId, i_source);
			}
			HashSet<string> scriptTriggerIds = new HashSet<string>(StringComparer.Ordinal);
			if (scriptTriggers.Count > 256) Error(result, "script-trigger-count", "A Tiled stage supports at most 256 script-trigger rectangles.", i_source);
			foreach (TiledLevelObject trigger in scriptTriggers)
				if (string.IsNullOrWhiteSpace(trigger.Name) || !scriptTriggerIds.Add(trigger.Name))
					Error(result, "script-trigger-id", "script-trigger names must be non-empty and unique.", i_source);
			HashSet<string> stageMarkerIds = new HashSet<string>(StringComparer.Ordinal);
			if (stageMarkers.Count > 256) Error(result, "stage-marker-count", "A Tiled stage supports at most 256 stage-marker points.", i_source);
			foreach (TiledLevelObject marker in stageMarkers)
				if (string.IsNullOrWhiteSpace(marker.Name) || !stageMarkerIds.Add(marker.Name))
					Error(result, "stage-marker-id", "stage-marker names must be non-empty and unique.", i_source);
			HashSet<string> stageItemIds = new HashSet<string>(StringComparer.Ordinal);
			if (stageItems.Count > 128) Error(result, "stage-item-count", "A Tiled stage supports at most 128 stage-item points.", i_source);
			foreach (TiledStageItemDefinition item in stageItems)
				if (string.IsNullOrWhiteSpace(item.Point.Name) || !stageItemIds.Add(item.Point.Name))
					Error(result, "stage-item-id", "stage-item names must be non-empty and unique.", i_source);
			HashSet<string> coreStageObjectIds = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> coreStageObjectPaths = new HashSet<string>(StringComparer.Ordinal);
			if (coreStageObjects.Count > 256) Error(result, "core-stage-object-count", "A Tiled stage supports at most 256 explicit Core stage-object adapters.", i_source);
			foreach (TiledCoreStageObjectDefinition adapter in coreStageObjects)
			{
				if (string.IsNullOrWhiteSpace(adapter.Point.Name) || !coreStageObjectIds.Add(adapter.Point.Name))
					Error(result, "core-stage-object-id", "Core stage-object names must be non-empty and unique.", i_source);
				if (!coreStageObjectPaths.Add(adapter.SourcePath))
					Error(result, "core-stage-object-source-duplicate", "A Core stage object may only be retained once: " + adapter.SourcePath, i_source);
			}
			if (result.Report.IsValid)
				result.Definition = new TiledLevelDefinition(map.Width * map.TileWidth, map.Height * map.TileHeight,
					map.TileWidth, map.TileHeight,
					platforms, playerSpawns[0], enemySpawners, tilesets, tileLayers, imageLayers, decorations, weaponVendors, usableVendors,
					weaponCases, doors, scriptedActors, doorSwitches, coreArt, notes, keypads, altars, interactions, lights, proximityLights, freeformLights, globalLights, pointLights, pickups, stageItems, movingPlatforms, particles, navNodes, audioSources,
					rooms, roomEntries, roomTransitions, scriptTriggers, stageMarkers, coreStageObjects);
			return result;
		}

		private static void ValidateInteractionCycles(IEnumerable<TiledInteractionDefinition> i_interactions,
			ValidationReport io_report, string i_source)
		{
			Dictionary<string, TiledInteractionDefinition> byName = new Dictionary<string, TiledInteractionDefinition>(StringComparer.Ordinal);
			foreach (TiledInteractionDefinition interaction in i_interactions)
				if (!string.IsNullOrWhiteSpace(interaction.Area.Name) && !byName.ContainsKey(interaction.Area.Name))
					byName.Add(interaction.Area.Name, interaction);
			HashSet<string> visiting = new HashSet<string>(StringComparer.Ordinal);
			HashSet<string> visited = new HashSet<string>(StringComparer.Ordinal);
			foreach (string name in byName.Keys)
				if (HasInteractionCycle(name, byName, visiting, visited))
				{
					io_report.Add(ValidationSeverity.Error, "tiled.interaction-chain-cycle", "Interaction chains must not contain cycles (at " + name + ").", i_source);
					return;
				}
		}

		private static bool HasInteractionCycle(string i_name, Dictionary<string, TiledInteractionDefinition> i_byName,
			HashSet<string> io_visiting, HashSet<string> io_visited)
		{
			if (io_visited.Contains(i_name)) return false;
			if (!io_visiting.Add(i_name)) return true;
			TiledInteractionDefinition interaction = i_byName[i_name];
			foreach (string target in interaction.TargetInteractionIds)
				if (i_byName.ContainsKey(target) && HasInteractionCycle(target, i_byName, io_visiting, io_visited)) return true;
			io_visiting.Remove(i_name);
			io_visited.Add(i_name);
			return false;
		}

		private static void ResolveObjectTemplates(JToken i_token, string i_packRoot, string i_currentDirectory,
			HashSet<string> io_stack, ref int io_resolvedCount, ValidationReport io_report, string i_source)
		{
			if (i_token == null) return;
			if (i_token is JArray array)
			{
				foreach (JToken child in array) ResolveObjectTemplates(child, i_packRoot, i_currentDirectory,
					io_stack, ref io_resolvedCount, io_report, i_source);
				return;
			}
			if (!(i_token is JObject document)) return;
			JToken templateToken = document["template"];
			if (templateToken != null)
			{
				string relativePath = templateToken.Type == JTokenType.String ? (string)templateToken : null;
				if (i_packRoot == null || i_currentDirectory == null || string.IsNullOrWhiteSpace(relativePath)
					|| Path.IsPathRooted(relativePath) || !relativePath.EndsWith(".tx", StringComparison.OrdinalIgnoreCase))
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-path", "Tiled object templates must be relative pack-local .tx files.", i_source);
					return;
				}
				string path;
				try { path = Path.GetFullPath(Path.Combine(i_currentDirectory, relativePath)); }
				catch (Exception exception) when (exception is ArgumentException || exception is NotSupportedException || exception is PathTooLongException)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-path", exception.Message, i_source);
					return;
				}
				if (!AssetPatchDiscovery.IsInside(path, i_packRoot) || !File.Exists(path))
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-file", "Tiled object template is missing or outside its pack: " + relativePath, i_source);
					return;
				}
				if (io_resolvedCount >= 4096 || io_stack.Count >= 16)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-limit", "Tiled map exceeds the object-template expansion limit.", i_source);
					return;
				}
				if (!io_stack.Add(path))
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-cycle", "Tiled object-template cycle detected: " + relativePath, i_source);
					return;
				}
				try
				{
					FileInfo file = new FileInfo(path);
					if (file.Length <= 0 || file.Length > 1024 * 1024)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.template-size", "Tiled object templates must be between 1 byte and 1 MiB.", path);
						return;
					}
					JObject templateRoot = JObject.Parse(File.ReadAllText(path));
					if (!(templateRoot["object"] is JObject templateObject))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.template-object", "Tiled template has no object definition: " + relativePath, path);
						return;
					}
					ResolveObjectTemplates(templateObject, i_packRoot, Path.GetDirectoryName(path), io_stack,
						ref io_resolvedCount, io_report, path);
					JObject merged = (JObject)templateObject.DeepClone();
					JObject instance = (JObject)document.DeepClone();
					instance.Remove("template");
					merged.Merge(instance, new JsonMergeSettings
					{
						MergeArrayHandling = MergeArrayHandling.Replace,
						MergeNullValueHandling = MergeNullValueHandling.Merge
					});
					document.RemoveAll();
					foreach (JProperty property in new List<JProperty>(merged.Properties()))
					{
						property.Remove();
						document.Add(property);
					}
					io_resolvedCount++;
				}
				catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.template-read", exception.Message, path);
				}
				finally { io_stack.Remove(path); }
			}
			foreach (JProperty property in new List<JProperty>(document.Properties()))
				ResolveObjectTemplates(property.Value, i_packRoot, i_currentDirectory, io_stack,
					ref io_resolvedCount, io_report, i_source);
		}

		private static void ValidateTileGids(IEnumerable<TiledTileLayerDefinition> i_layers,
			IEnumerable<TiledDecorationDefinition> i_decorations,
			IReadOnlyList<TiledTilesetDefinition> i_tilesets, ValidationReport io_report, string i_source)
		{
			const uint gidMask = 0x1FFFFFFF;
			bool warnedDiagonal = false;
			foreach (TiledTileLayerDefinition layer in i_layers)
				foreach (uint encodedGid in layer.Data)
				{
					if (!warnedDiagonal && (encodedGid & 0x20000000) != 0)
					{
						io_report.Add(ValidationSeverity.Warning, "tiled.tile-diagonal-flip", "Diagonal tile flips are experimental and currently render without the diagonal transform.", i_source);
						warnedDiagonal = true;
					}
					uint gid = encodedGid & gidMask;
					if (gid == 0) continue;
					TiledTilesetDefinition match = null;
					for (int index = i_tilesets.Count - 1; index >= 0; index--)
						if (gid >= i_tilesets[index].FirstGid) { match = i_tilesets[index]; break; }
					if (match == null || gid - match.FirstGid >= match.TileCount)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tile-gid", "Tile layer '" + layer.Name + "' references unknown gid " + gid + ".", i_source);
						return;
					}
				}
			foreach (TiledDecorationDefinition decoration in i_decorations)
			{
				uint gid = decoration.EncodedGid & gidMask;
				TiledTilesetDefinition match = null;
				for (int index = i_tilesets.Count - 1; index >= 0; index--)
					if (gid >= i_tilesets[index].FirstGid) { match = i_tilesets[index]; break; }
				if (gid == 0 || match == null || gid - match.FirstGid >= match.TileCount)
					io_report.Add(ValidationSeverity.Error, "tiled.decoration-gid", "Decoration '" + decoration.Name + "' references unknown gid " + gid + ".", i_source);
			}
		}

		private static List<TiledTilesetDefinition> ReadTilesets(IEnumerable<TiledTilesetDocument> i_tilesets,
			string i_packRoot, string i_mapDirectory, ValidationReport io_report, string i_source)
		{
			List<TiledTilesetDefinition> result = new List<TiledTilesetDefinition>();
			if (i_tilesets == null) return result;
			uint previousFirstGid = 0;
			foreach (TiledTilesetDocument reference in i_tilesets)
			{
				if (reference == null) continue;
				uint firstGid = reference.FirstGid;
				TiledTilesetDocument document = reference;
				string assetRoot = i_mapDirectory ?? string.Empty;
				if (!string.IsNullOrWhiteSpace(reference.Source))
				{
					if (i_packRoot == null || string.IsNullOrWhiteSpace(reference.Source) || Path.IsPathRooted(reference.Source))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tileset-source", "External tilesets require a relative pack-local .json or .tsj source.", i_source);
						continue;
					}
					string tilesetPath = Path.GetFullPath(Path.Combine(i_mapDirectory, reference.Source));
					if (!AssetPatchDiscovery.IsInside(tilesetPath, i_packRoot) || !File.Exists(tilesetPath)
						|| (!tilesetPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
							&& !tilesetPath.EndsWith(".tsj", StringComparison.OrdinalIgnoreCase)))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tileset-source", "External tileset is missing, outside its pack, or is not JSON/TSJ: " + reference.Source, i_source);
						continue;
					}
					try
					{
						document = JsonConvert.DeserializeObject<TiledTilesetDocument>(File.ReadAllText(tilesetPath));
						assetRoot = Path.GetDirectoryName(tilesetPath);
					}
					catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tileset-read", exception.Message, tilesetPath);
						continue;
					}
				}
				if (document == null || firstGid < 1 || firstGid <= previousFirstGid)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-gid", "Tileset firstgid values must be positive and strictly increasing.", i_source);
					continue;
				}
				previousFirstGid = firstGid;
				string objectAlignment = string.IsNullOrWhiteSpace(document.ObjectAlignment)
					? "bottomleft" : document.ObjectAlignment.ToLowerInvariant();
				if (objectAlignment != "unspecified" && objectAlignment != "topleft" && objectAlignment != "top"
					&& objectAlignment != "topright" && objectAlignment != "left" && objectAlignment != "center"
					&& objectAlignment != "right" && objectAlignment != "bottomleft" && objectAlignment != "bottom"
					&& objectAlignment != "bottomright")
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-object-alignment", "Tileset objectalignment is not a supported Tiled alignment.", i_source);
					continue;
				}
				if (document.TileWidth < 1 || document.TileHeight < 1 || document.TileCount < 1 || document.TileCount > 1000000
					|| document.Columns < 1 || document.Columns > 1000000
					|| document.ImageWidth < 1 || document.ImageHeight < 1 || document.Spacing < 0 || document.Margin < 0)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-dimensions", "Tilesets require positive tile/image dimensions, tilecount, and columns.", i_source);
					continue;
				}
				int rows = (document.TileCount + document.Columns - 1) / document.Columns;
				long requiredWidth = document.Margin * 2L + document.Columns * (long)document.TileWidth + (document.Columns - 1L) * document.Spacing;
				long requiredHeight = document.Margin * 2L + rows * (long)document.TileHeight + (rows - 1L) * document.Spacing;
				if (requiredWidth > document.ImageWidth || requiredHeight > document.ImageHeight)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-image-size", "Tileset tile grid does not fit inside its declared image dimensions.", i_source);
					continue;
				}
				if (!ModPath.IsSafeRelativePath(document.Image) || !document.Image.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				{
					if (i_packRoot == null || string.IsNullOrWhiteSpace(document.Image) || Path.IsPathRooted(document.Image)
						|| !document.Image.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tileset-image", "Tileset images must be relative PNG paths inside the pack.", i_source);
						continue;
					}
				}
				string runtimeRoot = assetRoot;
				string runtimeImage = document.Image;
				if (i_packRoot != null)
				{
					string imagePath = Path.GetFullPath(Path.Combine(assetRoot, document.Image));
					if (!AssetPatchDiscovery.IsInside(imagePath, i_packRoot) || !File.Exists(imagePath))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tileset-image", "Tileset PNG is missing or outside its pack: " + document.Image, i_source);
						continue;
					}
					runtimeRoot = i_packRoot;
					runtimeImage = imagePath.Substring(i_packRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
						.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
				}
				bool animationValid = true;
				HashSet<int> animatedTileIds = new HashSet<int>();
				foreach (TiledTileDocument tile in document.Tiles ?? new List<TiledTileDocument>())
				{
					if (tile.Animation == null || tile.Animation.Count == 0) continue;
					if (tile.Id < 0 || tile.Id >= document.TileCount || !animatedTileIds.Add(tile.Id)
						|| tile.Animation.Count < 2 || tile.Animation.Count > 256)
					{
						animationValid = false;
						break;
					}
					foreach (TiledAnimationFrameDocument frame in tile.Animation)
						if (frame == null || frame.TileId < 0 || frame.TileId >= document.TileCount
							|| frame.Duration < 16 || frame.Duration > 60000) { animationValid = false; break; }
					if (!animationValid) break;
				}
				if (!animationValid)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-animation", "Animated tiles require 2..256 valid in-tileset frames with durations of 16..60000 ms.", i_source);
					continue;
				}
				TiledTilesetDefinition definition = new TiledTilesetDefinition(firstGid, document, runtimeRoot, runtimeImage);
				if (definition.BorderLeft < 0f || definition.BorderBottom < 0f || definition.BorderRight < 0f || definition.BorderTop < 0f
					|| definition.BorderLeft + definition.BorderRight > definition.TileWidth
					|| definition.BorderBottom + definition.BorderTop > definition.TileHeight)
				{
					io_report.Add(ValidationSeverity.Error, "tiled.tileset-border", "Sprite borders must be non-negative and fit inside the tileset tile dimensions.", i_source);
					continue;
				}
				result.Add(definition);
			}
			return result;
		}

		private static void ReadLayers(IEnumerable<TiledLayerDocument> i_layers, float i_parentX, float i_parentY,
			int i_parentTileX, int i_parentTileY, float i_parentOpacity, bool i_parentVisible,
			int i_mapTileWidth, int i_mapTileHeight,
			List<TiledLevelObject> io_platforms, List<TiledLevelObject> io_playerSpawns,
			List<TiledLevelObject> io_enemySpawners, List<TiledTileLayerDefinition> io_tileLayers,
			List<TiledImageLayerDefinition> io_imageLayers,
			List<TiledDecorationDefinition> io_decorations,
			List<TiledLevelObject> io_weaponVendors, List<TiledLevelObject> io_usableVendors,
			List<TiledWeaponCaseDefinition> io_weaponCases,
			List<TiledDoorDefinition> io_doors, List<TiledScriptedActorDefinition> io_scriptedActors, List<TiledDoorSwitchDefinition> io_doorSwitches,
			List<TiledCoreArtDefinition> io_coreArt,
			List<TiledNoteDefinition> io_notes, List<TiledKeypadDefinition> io_keypads,
			List<TiledLevelObject> io_altars,
			List<TiledInteractionDefinition> io_interactions,
			List<TiledLightDefinition> io_lights,
			List<TiledProximityLightDefinition> io_proximityLights,
			List<TiledFreeformLightDefinition> io_freeformLights,
			List<TiledGlobalLightDefinition> io_globalLights,
			List<TiledPointLightDefinition> io_pointLights,
			List<TiledPickupDefinition> io_pickups,
			List<TiledStageItemDefinition> io_stageItems,
			List<TiledMovingPlatformDefinition> io_movingPlatforms,
			List<TiledParticleDefinition> io_particles,
			List<TiledNavNodeDefinition> io_navNodes,
			List<TiledAudioDefinition> io_audioSources,
			List<TiledRoomDefinition> io_rooms,
			List<TiledRoomEntryDefinition> io_roomEntries,
			List<TiledRoomTransitionDefinition> io_roomTransitions,
			List<TiledLevelObject> io_scriptTriggers,
			List<TiledLevelObject> io_stageMarkers,
			List<TiledCoreStageObjectDefinition> io_coreStageObjects,
			ref int io_drawOrder, string i_packRoot, string i_mapDirectory, ValidationReport io_report, string i_source)
		{
			if (i_layers == null) return;
			foreach (TiledLayerDocument layer in i_layers)
			{
				if (layer == null) continue;
				float offsetX = i_parentX + layer.OffsetX;
				float offsetY = i_parentY + layer.OffsetY;
				int tileX = i_parentTileX + layer.X;
				int tileY = i_parentTileY + layer.Y;
				float opacity = i_parentOpacity * (layer.Opacity ?? 1f);
				bool visible = i_parentVisible && (layer.Visible ?? true);
				if (string.Equals(layer.Type, "group", StringComparison.Ordinal))
					ReadLayers(layer.Layers, offsetX, offsetY, tileX, tileY, opacity, visible, i_mapTileWidth, i_mapTileHeight,
						io_platforms, io_playerSpawns, io_enemySpawners, io_tileLayers, io_imageLayers, io_decorations,
						io_weaponVendors, io_usableVendors, io_weaponCases, io_doors, io_scriptedActors, io_doorSwitches, io_coreArt,
						io_notes, io_keypads, io_altars, io_interactions, io_lights, io_proximityLights, io_freeformLights, io_globalLights, io_pointLights, io_pickups, io_stageItems, io_movingPlatforms, io_particles, io_navNodes, io_audioSources,
						io_rooms, io_roomEntries, io_roomTransitions, io_scriptTriggers, io_stageMarkers, io_coreStageObjects,
						ref io_drawOrder, i_packRoot, i_mapDirectory, io_report, i_source);
				int layerDrawOrder = -1;
				if (string.Equals(layer.Type, "tilelayer", StringComparison.Ordinal)
					|| string.Equals(layer.Type, "imagelayer", StringComparison.Ordinal)
					|| string.Equals(layer.Type, "objectgroup", StringComparison.Ordinal)) layerDrawOrder = io_drawOrder++;
				if (string.Equals(layer.Type, "imagelayer", StringComparison.Ordinal))
				{
					if (!visible) continue;
					float parallaxX = layer.ParallaxX ?? 1f;
					float parallaxY = layer.ParallaxY ?? 1f;
					if (!Finite(offsetX) || !Finite(offsetY) || opacity < 0f || opacity > 1f
						|| !Finite(parallaxX) || !Finite(parallaxY) || parallaxX < 0f || parallaxX > 2f || parallaxY < 0f || parallaxY > 2f)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.image-layer-values", "Image-layer offsets must be finite; opacity must be 0..1 and parallax factors must be 0..2.", i_source);
						continue;
					}
					if (i_packRoot == null || i_mapDirectory == null || string.IsNullOrWhiteSpace(layer.Image)
						|| Path.IsPathRooted(layer.Image) || !layer.Image.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.image-layer-file", "Image layers require a relative pack-local PNG.", i_source);
						continue;
					}
					string imagePath = Path.GetFullPath(Path.Combine(i_mapDirectory, layer.Image));
					if (!AssetPatchDiscovery.IsInside(imagePath, i_packRoot) || !File.Exists(imagePath))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.image-layer-file", "Image-layer PNG is missing or outside its pack: " + layer.Image, i_source);
						continue;
					}
					string runtimeImage = imagePath.Substring(i_packRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length)
						.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
					int pixelX = tileX * i_mapTileWidth + (int)offsetX;
					int pixelY = tileY * i_mapTileHeight + (int)offsetY;
					io_imageLayers.Add(new TiledImageLayerDefinition(layer, pixelX, pixelY, opacity, layerDrawOrder,
						i_packRoot, runtimeImage));
					continue;
				}
				if (string.Equals(layer.Type, "tilelayer", StringComparison.Ordinal))
				{
					if (!visible) continue;
					long cellCount = layer.Width * (long)layer.Height;
					if (layer.Width < 1 || layer.Height < 1 || cellCount > 1000000 || layer.Data == null || layer.Data.Count != cellCount)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tile-layer-data", "Visible tile layers require at most 1,000,000 cells in an uncompressed numeric data array matching width times height.", i_source);
						continue;
					}
					if (!Finite(offsetX) || !Finite(offsetY) || opacity < 0f || opacity > 1f)
					{
						io_report.Add(ValidationSeverity.Error, "tiled.tile-layer-values", "Tile layer offsets and opacity must be finite and opacity must be between 0 and 1.", i_source);
						continue;
					}
					int pixelX = tileX * i_mapTileWidth + (int)offsetX;
					int pixelY = tileY * i_mapTileHeight + (int)offsetY;
					io_tileLayers.Add(new TiledTileLayerDefinition(layer, pixelX, pixelY, opacity, layerDrawOrder));
					continue;
				}
				if (!string.Equals(layer.Type, "objectgroup", StringComparison.Ordinal) || layer.Objects == null) continue;
				for (int objectIndex = 0; objectIndex < layer.Objects.Count; objectIndex++)
				{
					TiledObjectDocument item = layer.Objects[objectIndex];
					if (item == null) continue;
					string semanticType = string.IsNullOrWhiteSpace(item.Type) ? item.Class : item.Type;
					semanticType = (semanticType ?? string.Empty).Trim().ToLowerInvariant();
					if (semanticType != "platform" && semanticType != "player-spawn" && semanticType != "enemy-spawner"
						&& semanticType != "decoration" && semanticType != "weapon-vendor" && semanticType != "usable-vendor"
						&& semanticType != "weapon-case" && semanticType != "door" && semanticType != "scripted-actor" && semanticType != "door-switch"
						&& semanticType != "core-art" && semanticType != "note" && semanticType != "keypad"
						&& semanticType != "altar" && semanticType != "interaction" && semanticType != "machine"
						&& semanticType != "logic-switch"
						&& semanticType != "easter-egg-step" && semanticType != "light-bulb" && semanticType != "proximity-light" && semanticType != "freeform-light" && semanticType != "global-light" && semanticType != "point-light"
						&& semanticType != "pickup" && semanticType != "stage-item" && semanticType != "moving-platform" && semanticType != "particle-emitter"
						&& semanticType != "nav-node" && semanticType != "ambient-audio" && semanticType != "audio-source"
						&& semanticType != "room" && semanticType != "room-entry" && semanticType != "room-transition"
						&& semanticType != "script-trigger" && semanticType != "stage-marker" && semanticType != "core-stage-object"
						&& semanticType != "core-prop") continue;
					if (!Finite(item.X) || !Finite(item.Y) || !Finite(item.Width) || !Finite(item.Height))
					{
						io_report.Add(ValidationSeverity.Error, "tiled.object-coordinate", "Gameplay object coordinates must be finite and bounded.", i_source);
						continue;
					}
					if (semanticType == "core-stage-object")
					{
						string sourcePath = (ReadStringProperty(item.Properties, "sourcePath") ?? string.Empty).Trim();
						string kind = (ReadStringProperty(item.Properties, "coreObjectKind") ?? "object").Trim().ToLowerInvariant();
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", true);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.core-stage-object-shape", "core-stage-object adapters must be points.", i_source);
						else if (sourcePath.Length == 0 || sourcePath.Length > 512 || sourcePath.IndexOf('\\') >= 0
							|| sourcePath.StartsWith("/", StringComparison.Ordinal) || sourcePath.EndsWith("/", StringComparison.Ordinal)
							|| sourcePath.Contains("//") || sourcePath == "." || sourcePath == ".."
							|| sourcePath.StartsWith("../", StringComparison.Ordinal) || sourcePath.Contains("/../"))
							io_report.Add(ValidationSeverity.Error, "tiled.core-stage-object-source", "core-stage-object requires a bounded relative sourcePath inside its inherited Core stage.", i_source);
						else if (kind != "object" && kind != "door" && kind != "interaction" && kind != "actor"
							&& kind != "item" && kind != "pose" && kind != "light")
							io_report.Add(ValidationSeverity.Error, "tiled.core-stage-object-kind", "coreObjectKind must be object, door, interaction, actor, item, pose, or light.", i_source);
						else io_coreStageObjects.Add(new TiledCoreStageObjectDefinition(item, offsetX, offsetY, sourcePath, kind, initiallyActive));
						continue;
					}
					if (semanticType == "core-prop")
					{
						string asset = (ReadStringProperty(item.Properties, "asset") ?? string.Empty).Trim();
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", true);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.core-prop-shape", "core-prop objects must be points.", i_source);
						else if (!ContentId.TryParse(asset, out ContentId assetId) || assetId.Namespace != "core"
							|| !assetId.Path.StartsWith("stage-prop/", StringComparison.Ordinal))
							io_report.Add(ValidationSeverity.Error, "tiled.core-prop-asset", "core-prop requires a core:stage-prop/... asset ID.", i_source);
						else io_coreStageObjects.Add(new TiledCoreStageObjectDefinition(item, offsetX, offsetY,
							assetId.ToString(), "catalog-prop", initiallyActive));
						continue;
					}
					if (semanticType == "weapon-case")
					{
						TiledLevelObject visual = new TiledLevelObject(item, offsetX, offsetY);
						if (!item.Point || item.Width != 0 || item.Height != 0)
						{
							io_report.Add(ValidationSeverity.Error, "tiled.weapon-case-shape", "weapon-case objects must be points.", i_source);
							continue;
						}
						string weaponText = ReadStringProperty(item.Properties, "weapon");
						int caseSize = ReadIntProperty(item.Properties, "caseSize", 0);
						if (!ContentId.TryParse(weaponText, out ContentId weapon) || !weapon.Path.StartsWith("item/weapon/", StringComparison.Ordinal))
							io_report.Add(ValidationSeverity.Error, "tiled.weapon-case-weapon", "weapon-case requires a weapon property containing an item/weapon/ content ID.", i_source);
						else if (caseSize < 1 || caseSize > 3)
							io_report.Add(ValidationSeverity.Error, "tiled.weapon-case-size", "weapon-case caseSize must be 1, 2, or 3.", i_source);
						else if (!ValidObjectVisualPair(i_packRoot, visual.VisualFile, visual.BrokenVisualFile,
							visual.VisualPixelsPerUnit))
							io_report.Add(ValidationSeverity.Error, "tiled.weapon-case-visual", "visualFile and brokenVisualFile must both be pack-local PNGs with visualPixelsPerUnit from 1 to 1024.", i_source);
						else io_weaponCases.Add(new TiledWeaponCaseDefinition(item, offsetX, offsetY, weapon, caseSize));
						continue;
					}
					if (semanticType == "door")
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
						{
							io_report.Add(ValidationSeverity.Error, "tiled.door-shape", "door objects must be points.", i_source);
							continue;
						}
						string doorType = (ReadStringProperty(item.Properties, "doorType") ?? string.Empty).Trim().ToLowerInvariant();
						bool initiallyOpen = ReadBoolProperty(item.Properties, "initiallyOpen", false);
						float proximityRadius = ReadFloatProperty(item.Properties, "proximityRadius", 8f);
						int price = ReadIntProperty(item.Properties, "price", 0);
						bool singleUse = ReadBoolProperty(item.Properties, "singleUse", false);
						bool initiallyInteractable = ReadBoolProperty(item.Properties, "initiallyInteractable", true);
						string requiredItemId = (ReadStringProperty(item.Properties, "requiredItemId") ?? string.Empty).Trim();
						string openSprite = (ReadStringProperty(item.Properties, "openSprite") ?? string.Empty).Trim();
						string closedSprite = (ReadStringProperty(item.Properties, "closedSprite") ?? string.Empty).Trim();
						TiledLevelObject visual = new TiledLevelObject(item, offsetX, offsetY);
						if (doorType != "standard" && doorType != "jacky" && doorType != "roller")
							io_report.Add(ValidationSeverity.Error, "tiled.door-type", "doorType must be standard, jacky, or roller.", i_source);
						else if (!Finite(proximityRadius) || proximityRadius <= 0f || proximityRadius > 100f)
							io_report.Add(ValidationSeverity.Error, "tiled.door-proximity", "proximityRadius must be greater than 0 and at most 100.", i_source);
						else if (price < 0 || price > 1000000)
							io_report.Add(ValidationSeverity.Error, "tiled.door-price", "door price must be between 0 and 1,000,000.", i_source);
						else if (requiredItemId.Length > 128 || openSprite.Length > 128 || closedSprite.Length > 128)
							io_report.Add(ValidationSeverity.Error, "tiled.door-properties", "door item and sprite identifiers may not exceed 128 characters.", i_source);
						else if ((doorType == "roller" && (!ValidOptionalObjectVisual(i_packRoot,
							visual.VisualFile, visual.VisualPixelsPerUnit) || visual.OpenVisualFile.Length > 0
							|| visual.ClosedVisualFile.Length > 0))
							|| (doorType != "roller" && (visual.VisualFile.Length > 0
							|| !ValidObjectVisualPair(i_packRoot, visual.OpenVisualFile, visual.ClosedVisualFile,
								visual.VisualPixelsPerUnit))))
							io_report.Add(ValidationSeverity.Error, "tiled.door-visual", "Standard and Jacky doors require an open/closed PNG pair; roller doors use one visualFile PNG.", i_source);
						else io_doors.Add(new TiledDoorDefinition(item, offsetX, offsetY, doorType, initiallyOpen, proximityRadius,
							price, singleUse, initiallyInteractable, requiredItemId, openSprite, closedSprite));
						continue;
					}
					if (semanticType == "scripted-actor")
					{
						string enemyText = (ReadStringProperty(item.Properties, "enemy") ?? string.Empty).Trim();
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", false);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.scripted-actor-shape", "scripted-actor objects must be points.", i_source);
						else if (!ContentId.TryParse(enemyText, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal))
							io_report.Add(ValidationSeverity.Error, "tiled.scripted-actor-enemy", "scripted-actor requires an enemy content ID.", i_source);
						else io_scriptedActors.Add(new TiledScriptedActorDefinition(item, offsetX, offsetY, enemy, initiallyActive));
						continue;
					}
					if (semanticType == "door-switch")
					{
						TiledLevelObject visual = new TiledLevelObject(item, offsetX, offsetY);
						if (!item.Point || item.Width != 0 || item.Height != 0)
						{
							io_report.Add(ValidationSeverity.Error, "tiled.door-switch-shape", "door-switch objects must be points.", i_source);
							continue;
						}
						string targetsText = ReadStringProperty(item.Properties, "targetDoors") ?? string.Empty;
						List<string> targets = new List<string>();
						HashSet<string> uniqueTargets = new HashSet<string>(StringComparer.Ordinal);
						foreach (string targetText in targetsText.Split(','))
						{
							string target = targetText.Trim();
							if (target.Length > 0 && uniqueTargets.Add(target)) targets.Add(target);
						}
						if (targets.Count == 0)
							io_report.Add(ValidationSeverity.Error, "tiled.door-switch-targets", "door-switch requires a comma-separated targetDoors property.", i_source);
						else if (!ValidObjectVisualPair(i_packRoot, visual.OnVisualFile, visual.OffVisualFile,
							visual.VisualPixelsPerUnit))
							io_report.Add(ValidationSeverity.Error, "tiled.door-switch-visual", "onVisualFile and offVisualFile must both be pack-local PNGs with visualPixelsPerUnit from 1 to 1024.", i_source);
						else io_doorSwitches.Add(new TiledDoorSwitchDefinition(item, offsetX, offsetY, targets));
						continue;
					}
					if (semanticType == "note")
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
						{
							io_report.Add(ValidationSeverity.Error, "tiled.note-shape", "note objects must be points.", i_source);
							continue;
						}
						string text = ReadStringProperty(item.Properties, "text") ?? string.Empty;
						int fontSize = ReadIntProperty(item.Properties, "fontSize", 24);
						if (string.IsNullOrWhiteSpace(text) || text.Length > 4096)
							io_report.Add(ValidationSeverity.Error, "tiled.note-text", "note text must contain 1 through 4096 characters.", i_source);
						else if (fontSize < 8 || fontSize > 100)
							io_report.Add(ValidationSeverity.Error, "tiled.note-font-size", "note fontSize must be between 8 and 100.", i_source);
						else io_notes.Add(new TiledNoteDefinition(item, offsetX, offsetY, text, fontSize));
						continue;
					}
					if (semanticType == "keypad")
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
						{
							io_report.Add(ValidationSeverity.Error, "tiled.keypad-shape", "keypad objects must be points.", i_source);
							continue;
						}
						string code = (ReadStringProperty(item.Properties, "code") ?? string.Empty).Trim();
						List<string> targets = ParseCommaSeparatedProperty(item.Properties, "targetDoors");
						bool digitsOnly = code.Length > 0;
						foreach (char character in code) if (character < '0' || character > '9') digitsOnly = false;
						if (!digitsOnly || code.Length > 16)
							io_report.Add(ValidationSeverity.Error, "tiled.keypad-code", "keypad code must contain 1 through 16 digits.", i_source);
						else if (targets.Count == 0)
							io_report.Add(ValidationSeverity.Error, "tiled.keypad-targets", "keypad requires a comma-separated targetDoors property.", i_source);
						else io_keypads.Add(new TiledKeypadDefinition(item, offsetX, offsetY, code, targets));
						continue;
					}
					if (semanticType == "altar")
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.altar-shape", "altar objects must be points.", i_source);
						else io_altars.Add(new TiledLevelObject(item, offsetX, offsetY));
						continue;
					}
					if (semanticType == "interaction" || semanticType == "machine"
						|| semanticType == "logic-switch" || semanticType == "easter-egg-step")
					{
						string trigger = (ReadStringProperty(item.Properties, "trigger") ?? "use").Trim().ToLowerInvariant();
						int price = ReadIntProperty(item.Properties, "price", 0);
						bool singleUse = ReadBoolProperty(item.Properties, "singleUse", false);
						string message = (ReadStringProperty(item.Properties, "message") ?? string.Empty).Trim();
						string doorAction = (ReadStringProperty(item.Properties, "doorAction") ?? "toggle").Trim().ToLowerInvariant();
						List<string> targets = ParseCommaSeparatedProperty(item.Properties, "targetDoors");
						int minWave = ReadIntProperty(item.Properties, "minWave", 0);
						int maxWave = ReadIntProperty(item.Properties, "maxWave", 0);
						int activationsRequired = ReadIntProperty(item.Properties, "activationsRequired", 1);
						float delaySeconds = ReadFloatProperty(item.Properties, "delaySeconds", 0f);
						int giveMoney = ReadIntProperty(item.Properties, "giveMoney", 0);
						string lightAction = (ReadStringProperty(item.Properties, "lightAction") ?? "toggle").Trim().ToLowerInvariant();
						List<string> targetLights = ParseCommaSeparatedProperty(item.Properties, "targetLights");
						List<string> targetSpawners = ParseCommaSeparatedProperty(item.Properties, "targetSpawners");
						int spawnCount = ReadIntProperty(item.Properties, "spawnCount", targetSpawners.Count > 0 ? 1 : 0);
						List<string> targetInteractions = ParseCommaSeparatedProperty(item.Properties, "targetInteractions");
						string requiredItemText = (ReadStringProperty(item.Properties, "requiredItem") ?? string.Empty).Trim();
						ContentId? requiredItem = null;
						if (requiredItemText.Length > 0 && ContentId.TryParse(requiredItemText, out ContentId parsedRequiredItem)) requiredItem = parsedRequiredItem;
						bool consumeRequiredItem = ReadBoolProperty(item.Properties, "consumeRequiredItem", false);
						string giveItemText = (ReadStringProperty(item.Properties, "giveItem") ?? string.Empty).Trim();
						ContentId? giveItem = null;
						if (giveItemText.Length > 0 && ContentId.TryParse(giveItemText, out ContentId parsedGiveItem)) giveItem = parsedGiveItem;
						int giveItemAmount = ReadIntProperty(item.Properties, "giveItemAmount", giveItem.HasValue ? 1 : 0);
						string visualArtText = (ReadStringProperty(item.Properties, "visualArt") ?? string.Empty).Trim();
						CoreMapArtCatalogEntry visualArt = null;
						if (visualArtText.Length > 0) CoreMapArtCatalog.TryGet(visualArtText, out visualArt);
						float visualScale = ReadFloatProperty(item.Properties, "visualScale", 1f);
						string visualFile = (ReadStringProperty(item.Properties, "visualFile") ?? string.Empty).Trim().Replace('\\', '/');
						string activatedVisualFile = (ReadStringProperty(item.Properties, "activatedVisualFile") ?? string.Empty).Trim().Replace('\\', '/');
						float visualPixelsPerUnit = ReadFloatProperty(item.Properties, "visualPixelsPerUnit", 32f);
						float visualPivotX = ReadFloatProperty(item.Properties, "visualPivotX", 0.5f);
						float visualPivotY = ReadFloatProperty(item.Properties, "visualPivotY", 0.5f);
						string visualSortingLayer = (ReadStringProperty(item.Properties, "visualSortingLayer") ?? "Decoration").Trim();
						int visualSortingOrder = ReadIntProperty(item.Properties, "visualSortingOrder", 0);
						bool visualFilesValid = ValidPackPng(i_packRoot, visualFile) && ValidPackPng(i_packRoot, activatedVisualFile);
						int minEnemies = ReadIntProperty(item.Properties, "minEnemies", 0);
						int maxEnemies = ReadIntProperty(item.Properties, "maxEnemies", 0);
						List<ContentId> requiredChallenges = new List<ContentId>();
						List<string> requiredInteractions = ParseCommaSeparatedProperty(item.Properties, "requiredInteractions");
						int requiredMoney = ReadIntProperty(item.Properties, "requiredMoney", 0);
						float restoreHealth = ReadFloatProperty(item.Properties, "restoreHealth", 0f);
						float knockbackX = ReadFloatProperty(item.Properties, "knockbackX", 0f);
						float knockbackY = ReadFloatProperty(item.Properties, "knockbackY", 0f);
						float ragdollSeconds = ReadFloatProperty(item.Properties, "ragdollSeconds", 0f);
						bool challengesValid = true;
						foreach (string challengeText in ParseCommaSeparatedProperty(item.Properties, "requiredChallenges"))
						{
							if (!ContentId.TryParse(challengeText, out ContentId challengeId) || !challengeId.Path.StartsWith("challenge/", StringComparison.Ordinal))
							{
								challengesValid = false;
								break;
							}
							requiredChallenges.Add(challengeId);
						}
						bool pointShape = IsPointMarker(item);
						bool rectangleShape = !item.Point && item.Width > 0 && item.Height > 0;
						if (trigger != "use" && trigger != "touch" && trigger != "shoot")
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-trigger", "interaction trigger must be use, touch, or shoot.", i_source);
						else if ((trigger == "use" && !pointShape) || (trigger != "use" && !rectangleShape))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-shape", "use interactions must be points; touch and shoot interactions must be non-empty rectangles.", i_source);
						else if (price < 0 || price > 1000000 || (trigger != "use" && price != 0))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-price", "price must be 0 through 1000000 and is only supported for use interactions.", i_source);
						else if (message.Length > 512)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-message", "interaction message may contain at most 512 characters.", i_source);
						else if (doorAction != "open" && doorAction != "close" && doorAction != "toggle")
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-door-action", "doorAction must be open, close, or toggle.", i_source);
						else if (minWave < 0 || maxWave < 0 || (maxWave > 0 && maxWave < minWave))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-wave", "minWave/maxWave must be non-negative and maxWave must be zero or at least minWave.", i_source);
						else if (activationsRequired < 1 || activationsRequired > 10000 || !Finite(delaySeconds) || delaySeconds < 0f || delaySeconds > 3600f)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-timing", "activationsRequired must be 1..10000 and delaySeconds must be 0..3600.", i_source);
						else if (giveMoney < -1000000 || giveMoney > 1000000)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-money", "giveMoney must be between -1000000 and 1000000.", i_source);
						else if (lightAction != "on" && lightAction != "off" && lightAction != "toggle" && lightAction != "activate")
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-light-action", "lightAction must be on, off, toggle, or activate.", i_source);
						else if (spawnCount < 0 || spawnCount > 1000 || (spawnCount > 0) != (targetSpawners.Count > 0))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-spawn", "spawnCount must be 1..1000 when targetSpawners is set, or 0 when it is empty.", i_source);
						else if ((requiredItemText.Length > 0 && !requiredItem.HasValue) || (requiredItem.HasValue && !requiredItem.Value.Path.StartsWith("item/", StringComparison.Ordinal))
							|| (consumeRequiredItem && !requiredItem.HasValue) || (requiredItem.HasValue && trigger != "use"))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-required-item", "requiredItem must be an item content ID on a use interaction; consumeRequiredItem requires it.", i_source);
						else if ((giveItemText.Length > 0 && !giveItem.HasValue) || (giveItem.HasValue && !giveItem.Value.Path.StartsWith("item/", StringComparison.Ordinal))
							|| giveItemAmount < 0 || giveItemAmount > 10000 || (giveItemAmount > 0) != giveItem.HasValue)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-give-item", "giveItem must be an item content ID with giveItemAmount 1..10000, or both must be empty/zero.", i_source);
						else if ((visualArtText.Length > 0 && visualArt == null) || !Finite(visualScale) || visualScale < 0.1f || visualScale > 20f)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-visual", "visualArt must be a public Core map-art ID and visualScale must be 0.1..20.", i_source);
						else if ((visualFile.Length == 0) != (activatedVisualFile.Length == 0) || (visualFile.Length > 0 && (!visualFilesValid
							|| !Finite(visualPixelsPerUnit) || visualPixelsPerUnit < 1f || visualPixelsPerUnit > 1024f
							|| !Finite(visualPivotX) || visualPivotX < 0f || visualPivotX > 1f
							|| !Finite(visualPivotY) || visualPivotY < 0f || visualPivotY > 1f
							|| visualSortingLayer.Length == 0 || visualSortingLayer.Length > 64 || visualSortingOrder < -32768 || visualSortingOrder > 32767)))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-portable-visual", "visualFile and activatedVisualFile must both be pack-local PNGs with valid visual settings.", i_source);
						else if (minEnemies < 0 || minEnemies > 10000 || maxEnemies < 0 || maxEnemies > 10000 || (maxEnemies > 0 && maxEnemies < minEnemies))
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-enemy-count", "minEnemies/maxEnemies must be 0..10000 and maxEnemies must be zero or at least minEnemies.", i_source);
						else if (!challengesValid || requiredChallenges.Count > 100)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-challenges", "requiredChallenges must contain at most 100 challenge content IDs.", i_source);
						else if (requiredInteractions.Count > 100)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-required-interactions", "requiredInteractions may contain at most 100 interaction IDs.", i_source);
						else if (requiredMoney < 0 || requiredMoney > 1000000)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-required-money", "requiredMoney must be between 0 and 1000000.", i_source);
						else if (!Finite(restoreHealth) || restoreHealth < 0f || restoreHealth > 10000f)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-health", "restoreHealth must be between 0 and 10000.", i_source);
						else if (!Finite(knockbackX) || !Finite(knockbackY) || Math.Abs(knockbackX) > 1000f || Math.Abs(knockbackY) > 1000f
							|| !Finite(ragdollSeconds) || ragdollSeconds < 0f || ragdollSeconds > 60f)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-physics", "knockback components must be -1000..1000 and ragdollSeconds must be 0..60.", i_source);
						else if (targets.Count == 0 && targetLights.Count == 0 && targetSpawners.Count == 0
							&& targetInteractions.Count == 0 && message.Length == 0 && giveMoney == 0
							&& !giveItem.HasValue && restoreHealth == 0f && knockbackX == 0f && knockbackY == 0f && ragdollSeconds == 0f
							&& activatedVisualFile.Length == 0)
							io_report.Add(ValidationSeverity.Error, "tiled.interaction-effect", "interaction requires at least one supported action.", i_source);
						else io_interactions.Add(new TiledInteractionDefinition(item, offsetX, offsetY, trigger, price,
							singleUse, message, doorAction, targets, minWave, maxWave, activationsRequired,
							delaySeconds, giveMoney, lightAction, targetLights, targetSpawners, spawnCount, targetInteractions,
							requiredItem, consumeRequiredItem, giveItem, giveItemAmount, visualArt, visualScale,
							minEnemies, maxEnemies, requiredChallenges, requiredInteractions, requiredMoney, restoreHealth,
							knockbackX, knockbackY, ragdollSeconds, i_packRoot, visualFile, activatedVisualFile,
							visualPixelsPerUnit, visualPivotX, visualPivotY, visualSortingLayer, visualSortingOrder));
						continue;
					}
					if (semanticType == "light-bulb")
					{
						bool initiallyOn = ReadBoolProperty(item.Properties, "initiallyOn", true);
						float flicker = ReadFloatProperty(item.Properties, "flicker", 0f);
						bool interactive = ReadBoolProperty(item.Properties, "interactive", true);
						string visualFile = (ReadStringProperty(item.Properties, "visualFile") ?? string.Empty).Trim().Replace('\\', '/');
						string activatedVisualFile = (ReadStringProperty(item.Properties, "activatedVisualFile") ?? string.Empty).Trim().Replace('\\', '/');
						float pixelsPerUnit = ReadFloatProperty(item.Properties, "pixelsPerUnit", 32f);
						float pivotX = ReadFloatProperty(item.Properties, "pivotX", 0.5f);
						float pivotY = ReadFloatProperty(item.Properties, "pivotY", 0.5f);
						string sortingLayer = (ReadStringProperty(item.Properties, "sortingLayer") ?? "Lighting").Trim();
						int sortingOrder = ReadIntProperty(item.Properties, "sortingOrder", 0);
						string initialColor = (ReadStringProperty(item.Properties, "initialColor") ?? "#FFFFFF").Trim();
						string activatedColor = (ReadStringProperty(item.Properties, "activatedColor") ?? "#FFFFFF").Trim();
						float innerRadius = ReadFloatProperty(item.Properties, "innerRadius", 0f);
						float outerRadius = ReadFloatProperty(item.Properties, "outerRadius", 1f);
						float falloffIntensity = ReadFloatProperty(item.Properties, "falloffIntensity", 0.5f);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.light-shape", "light-bulb objects must be points.", i_source);
						else if (!Finite(flicker) || flicker < 0f || flicker > 1f)
							io_report.Add(ValidationSeverity.Error, "tiled.light-flicker", "light-bulb flicker must be between 0 and 1.", i_source);
						else if ((visualFile.Length == 0) != (activatedVisualFile.Length == 0)
							|| (visualFile.Length > 0 && (!ValidPackPng(i_packRoot, visualFile) || !ValidPackPng(i_packRoot, activatedVisualFile)
								|| !Finite(pixelsPerUnit) || pixelsPerUnit < 1f || pixelsPerUnit > 1024f
								|| !Finite(pivotX) || pivotX < 0f || pivotX > 1f || !Finite(pivotY) || pivotY < 0f || pivotY > 1f
								|| sortingLayer.Length == 0 || sortingLayer.Length > 64 || sortingOrder < -32768 || sortingOrder > 32767
								|| !ValidHtmlColor(initialColor) || !ValidHtmlColor(activatedColor)
								|| !Finite(innerRadius) || innerRadius < 0f || !Finite(outerRadius) || outerRadius <= innerRadius || outerRadius > 100f
								|| !Finite(falloffIntensity) || falloffIntensity < 0f || falloffIntensity > 1f)))
							io_report.Add(ValidationSeverity.Error, "tiled.light-portable-visual", "Portable light visuals require paired pack-local PNGs and valid sprite, color, and radius settings.", i_source);
						else io_lights.Add(new TiledLightDefinition(item, offsetX, offsetY, initiallyOn, flicker, interactive,
							i_packRoot, visualFile, activatedVisualFile, pixelsPerUnit, pivotX, pivotY, sortingLayer,
							sortingOrder, initialColor, activatedColor, innerRadius, outerRadius, falloffIntensity));
						continue;
					}
					if (semanticType == "proximity-light")
					{
						string file = (ReadStringProperty(item.Properties, "file") ?? string.Empty).Trim().Replace('\\', '/');
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", true);
						float detectionRadius = ReadFloatProperty(item.Properties, "detectionRadius", 12f);
						float fadeSeconds = ReadFloatProperty(item.Properties, "fadeSeconds", 1.5f);
						float intensity = ReadFloatProperty(item.Properties, "intensity", 0.5f);
						string lightType = (ReadStringProperty(item.Properties, "lightType") ?? "point").Trim().ToLowerInvariant();
						string color = (ReadStringProperty(item.Properties, "color") ?? "#FFFFFF").Trim();
						float falloffIntensity = ReadFloatProperty(item.Properties, "falloffIntensity", 0.579f);
						float innerRadius = ReadFloatProperty(item.Properties, "innerRadius", 0.58f);
						float outerRadius = ReadFloatProperty(item.Properties, "outerRadius", 13.04f);
						float lightOffsetY = ReadFloatProperty(item.Properties, "lightOffsetY", 0.128f);
						float lightRotationZ = ReadFloatProperty(item.Properties, "lightRotationZ", 0f);
						float shapeFalloffSize = ReadFloatProperty(item.Properties, "shapeFalloffSize", 1f);
						float[] shapePath = ParseShapePath(ReadStringProperty(item.Properties, "shapePath"));
						string sortingLayer = (ReadStringProperty(item.Properties, "sortingLayer") ?? "Decoration").Trim();
						int sortingOrder = ReadIntProperty(item.Properties, "sortingOrder", 0);
						if (!item.Point || item.Width != 0f || item.Height != 0f)
							io_report.Add(ValidationSeverity.Error, "tiled.proximity-light-shape", "proximity-light must be a point.", i_source);
						else if (!ValidPackPng(i_packRoot, file))
							io_report.Add(ValidationSeverity.Error, "tiled.proximity-light-file", "proximity-light requires a pack-local PNG no larger than 16 MiB.", i_source);
						else if (!Finite(detectionRadius) || detectionRadius <= 0f || detectionRadius > 100f
							|| !Finite(fadeSeconds) || fadeSeconds <= 0f || fadeSeconds > 30f
							|| !Finite(intensity) || intensity <= 0f || intensity > 10f
							|| !ValidHtmlColor(color) || !Finite(falloffIntensity) || falloffIntensity < 0f || falloffIntensity > 1f
							|| !Finite(innerRadius) || innerRadius < 0f || !Finite(outerRadius) || outerRadius <= innerRadius || outerRadius > 100f
							|| !Finite(lightOffsetY) || Math.Abs(lightOffsetY) > 20f || !Finite(lightRotationZ) || Math.Abs(lightRotationZ) > 360f
							|| !Finite(shapeFalloffSize) || shapeFalloffSize < 0f || shapeFalloffSize > 100f
							|| (lightType != "point" && lightType != "freeform") || (lightType == "freeform" && (shapePath == null || shapePath.Length < 6))
							|| sortingLayer.Length == 0 || sortingLayer.Length > 64 || sortingOrder < -32768 || sortingOrder > 32767)
							io_report.Add(ValidationSeverity.Error, "tiled.proximity-light-values", "proximity-light has invalid visual, detection, or shape settings.", i_source);
						else io_proximityLights.Add(new TiledProximityLightDefinition(item, offsetX, offsetY, i_packRoot, file,
							initiallyActive, detectionRadius, fadeSeconds, intensity, lightType, color, falloffIntensity,
							innerRadius, outerRadius, lightOffsetY, lightRotationZ, shapeFalloffSize, shapePath, sortingLayer, sortingOrder));
						continue;
					}
					if (semanticType == "freeform-light")
					{
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", true);
						float intensity = ReadFloatProperty(item.Properties, "intensity", 1f);
						string color = (ReadStringProperty(item.Properties, "color") ?? "#FFFFFF").Trim();
						float falloff = ReadFloatProperty(item.Properties, "falloffIntensity", 1f);
						float shapeFalloff = ReadFloatProperty(item.Properties, "shapeFalloffSize", 1f);
						float[] path = ParseShapePath(ReadStringProperty(item.Properties, "shapePath"));
						string[] layers = (ReadStringProperty(item.Properties, "sortingLayers") ?? string.Empty)
							.Split(',').Select(value => value.Trim()).ToArray();
						string suppressInheritedPath = (ReadStringProperty(item.Properties, "suppressInheritedPath") ?? string.Empty).Trim();
						string visualFile = (ReadStringProperty(item.Properties, "file") ?? string.Empty).Trim().Replace('\\', '/');
						float pixelsPerUnit = ReadFloatProperty(item.Properties, "pixelsPerUnit", 32f);
						string sortingLayer = (ReadStringProperty(item.Properties, "sortingLayer") ?? "Decoration").Trim();
						int sortingOrder = ReadIntProperty(item.Properties, "sortingOrder", 0);
						float lightOffsetY = ReadFloatProperty(item.Properties, "lightOffsetY", 0f);
						float lightRotationZ = ReadFloatProperty(item.Properties, "lightRotationZ", 0f);
						if (!item.Point || item.Width != 0f || item.Height != 0f)
							io_report.Add(ValidationSeverity.Error, "tiled.freeform-light-shape", "freeform-light must be a point.", i_source);
						else if ((visualFile.Length > 0 && !ValidPackPng(i_packRoot, visualFile))
							|| !Finite(pixelsPerUnit) || pixelsPerUnit < 1f || pixelsPerUnit > 1024f
							|| sortingLayer.Length == 0 || sortingLayer.Length > 64 || sortingOrder < -32768 || sortingOrder > 32767
							|| !Finite(lightOffsetY) || Math.Abs(lightOffsetY) > 20f
							|| !Finite(lightRotationZ) || Math.Abs(lightRotationZ) > 360f
							|| !Finite(intensity) || intensity <= 0f || intensity > 10f || !ValidHtmlColor(color)
							|| !Finite(falloff) || falloff < 0f || falloff > 1f
							|| !Finite(shapeFalloff) || shapeFalloff < 0f || shapeFalloff > 256f
							|| path == null || layers.Length == 0 || layers.Length > 16
							|| layers.Any(value => value != "Default" && value != "Sky" && value != "Background"
								&& value != "Decoration" && value != "Platform" && value != "Interactable"
								&& value != "Actor" && value != "Player" && value != "Projectile" && value != "Item")
							|| (suppressInheritedPath.Length > 0 && (suppressInheritedPath.Length > 512
								|| suppressInheritedPath.IndexOf('\\') >= 0 || suppressInheritedPath.StartsWith("/", StringComparison.Ordinal)
								|| suppressInheritedPath.EndsWith("/", StringComparison.Ordinal) || suppressInheritedPath.Contains("//")
								|| suppressInheritedPath == "." || suppressInheritedPath == ".."
								|| suppressInheritedPath.StartsWith("../", StringComparison.Ordinal) || suppressInheritedPath.Contains("/../"))))
							io_report.Add(ValidationSeverity.Error, "tiled.freeform-light-values", "freeform-light has invalid color, shape, intensity, or sorting layers.", i_source);
						else io_freeformLights.Add(new TiledFreeformLightDefinition(item, offsetX, offsetY,
							initiallyActive, intensity, color, falloff, shapeFalloff, path, layers,
							suppressInheritedPath, i_packRoot, visualFile, pixelsPerUnit, sortingLayer,
							sortingOrder, lightOffsetY, lightRotationZ));
						continue;
					}
					if (semanticType == "global-light")
					{
						float intensity = ReadFloatProperty(item.Properties, "intensity", 1f);
						string color = (ReadStringProperty(item.Properties, "color") ?? "#FFFFFF").Trim();
						float falloff = ReadFloatProperty(item.Properties, "falloffIntensity", 0.5f);
						string[] layers = (ReadStringProperty(item.Properties, "sortingLayers") ?? string.Empty)
							.Split(',').Select(value => value.Trim()).ToArray();
						if (!item.Point || item.Width != 0f || item.Height != 0f)
							io_report.Add(ValidationSeverity.Error, "tiled.global-light-shape", "global-light must be a point.", i_source);
						else if (!Finite(intensity) || intensity <= 0f || intensity > 10f || !ValidHtmlColor(color)
							|| !Finite(falloff) || falloff < 0f || falloff > 1f || layers.Length == 0 || layers.Length > 16
							|| layers.Any(value => value != "Default" && value != "Sky" && value != "Background"
								&& value != "Decoration" && value != "Platform" && value != "Interactable"
								&& value != "Actor" && value != "Player" && value != "Projectile"
								&& value != "Item" && value != "Hud" && value != "Node"))
							io_report.Add(ValidationSeverity.Error, "tiled.global-light-values", "global-light has invalid color, intensity, falloff, or sorting layers.", i_source);
						else io_globalLights.Add(new TiledGlobalLightDefinition(item, offsetX, offsetY,
							intensity, color, falloff, layers));
						continue;
					}
					if (semanticType == "point-light")
					{
						bool initiallyActive = ReadBoolProperty(item.Properties, "initiallyActive", true);
						float intensity = ReadFloatProperty(item.Properties, "intensity", 1f);
						string color = (ReadStringProperty(item.Properties, "color") ?? "#FFFFFF").Trim();
						float falloff = ReadFloatProperty(item.Properties, "falloffIntensity", 0.5f);
						float innerRadius = ReadFloatProperty(item.Properties, "innerRadius", 0f);
						float outerRadius = ReadFloatProperty(item.Properties, "outerRadius", 3f);
						float innerAngle = ReadFloatProperty(item.Properties, "innerAngle", 360f);
						float outerAngle = ReadFloatProperty(item.Properties, "outerAngle", 360f);
						float rotationZ = ReadFloatProperty(item.Properties, "rotationZ", 0f);
						string[] layers = (ReadStringProperty(item.Properties, "sortingLayers") ?? string.Empty)
							.Split(',').Select(value => value.Trim()).ToArray();
						string overlap = (ReadStringProperty(item.Properties, "overlapOperation") ?? "additive").Trim().ToLowerInvariant();
						float pulseFrom = ReadFloatProperty(item.Properties, "pulseFrom", 0f);
						float pulseTo = ReadFloatProperty(item.Properties, "pulseTo", 0f);
						float pulseSeconds = ReadFloatProperty(item.Properties, "pulseSeconds", 0f);
						string suppress = (ReadStringProperty(item.Properties, "suppressInheritedPath") ?? string.Empty).Trim();
						if (!item.Point || item.Width != 0f || item.Height != 0f)
							io_report.Add(ValidationSeverity.Error, "tiled.point-light-shape", "point-light must be a point.", i_source);
						else if (!Finite(intensity) || intensity <= 0f || intensity > 10f || !ValidHtmlColor(color)
							|| !Finite(falloff) || falloff < 0f || falloff > 1f
							|| !Finite(innerRadius) || innerRadius < 0f || !Finite(outerRadius) || outerRadius <= innerRadius || outerRadius > 100f
							|| !Finite(innerAngle) || innerAngle < 0f || !Finite(outerAngle) || outerAngle < innerAngle || outerAngle > 360f
							|| !Finite(rotationZ) || Math.Abs(rotationZ) > 360f
							|| layers.Length == 0 || layers.Length > 16
							|| layers.Any(value => value != "Default" && value != "Sky" && value != "Background"
								&& value != "Decoration" && value != "Platform" && value != "Interactable"
								&& value != "Actor" && value != "Player" && value != "Projectile"
								&& value != "Item" && value != "Hud" && value != "Node")
							|| (overlap != "additive" && overlap != "alpha-blend")
							|| !Finite(pulseFrom) || pulseFrom < 0f || pulseFrom > 10f
							|| !Finite(pulseTo) || pulseTo < 0f || pulseTo > 10f
							|| !Finite(pulseSeconds) || pulseSeconds < 0f || pulseSeconds > 60f
							|| (pulseSeconds > 0f && pulseTo <= 0f)
							|| (suppress.Length > 0 && (suppress.Length > 512 || suppress.IndexOf('\\') >= 0
								|| suppress.StartsWith("/", StringComparison.Ordinal) || suppress.EndsWith("/", StringComparison.Ordinal)
								|| suppress.Contains("//") || suppress == "." || suppress == ".."
								|| suppress.StartsWith("../", StringComparison.Ordinal) || suppress.Contains("/../"))))
							io_report.Add(ValidationSeverity.Error, "tiled.point-light-values", "point-light has invalid cone, pulse, or rendering settings.", i_source);
						else io_pointLights.Add(new TiledPointLightDefinition(item, offsetX, offsetY, initiallyActive,
							intensity, color, falloff, innerRadius, outerRadius, innerAngle, outerAngle,
							rotationZ, layers, overlap, pulseFrom, pulseTo, pulseSeconds, suppress));
						continue;
					}
					if (semanticType == "pickup")
					{
						string itemText = (ReadStringProperty(item.Properties, "item") ?? string.Empty).Trim();
						int amount = ReadIntProperty(item.Properties, "amount", 1);
						bool initiallyKinematic = ReadBoolProperty(item.Properties, "initiallyKinematic", false);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.pickup-shape", "pickup objects must be points.", i_source);
						else if (!ContentId.TryParse(itemText, out ContentId pickupItem) || !pickupItem.Path.StartsWith("item/", StringComparison.Ordinal))
							io_report.Add(ValidationSeverity.Error, "tiled.pickup-item", "pickup requires an item property containing an item content ID.", i_source);
						else if (amount < 1 || amount > 10000)
							io_report.Add(ValidationSeverity.Error, "tiled.pickup-amount", "pickup amount must be between 1 and 10000.", i_source);
						else io_pickups.Add(new TiledPickupDefinition(item, offsetX, offsetY, pickupItem, amount, initiallyKinematic));
						continue;
					}
					if (semanticType == "stage-item")
					{
						string file = (ReadStringProperty(item.Properties, "file") ?? string.Empty).Trim().Replace('\\', '/');
						string displayName = (ReadStringProperty(item.Properties, "displayName") ?? item.Name ?? string.Empty).Trim();
						string description = ReadStringProperty(item.Properties, "description") ?? string.Empty;
						string signal = (ReadStringProperty(item.Properties, "onPickupSignal") ?? string.Empty).Trim();
						float pixelsPerUnit = ReadFloatProperty(item.Properties, "pixelsPerUnit", 32f);
						float pivotX = ReadFloatProperty(item.Properties, "pivotX", 0.5f);
						float pivotY = ReadFloatProperty(item.Properties, "pivotY", 0.5f);
						float colliderWidth = ReadFloatProperty(item.Properties, "colliderWidth", 0.75f);
						float colliderHeight = ReadFloatProperty(item.Properties, "colliderHeight", 0.75f);
						float colliderOffsetX = ReadFloatProperty(item.Properties, "colliderOffsetX", 0f);
						float colliderOffsetY = ReadFloatProperty(item.Properties, "colliderOffsetY", 0f);
						int weight = ReadIntProperty(item.Properties, "weight", 0);
						int value = ReadIntProperty(item.Properties, "value", 0);
						bool canDrop = ReadBoolProperty(item.Properties, "canDrop", false);
						bool initiallyKinematic = ReadBoolProperty(item.Properties, "initiallyKinematic", false);
						string lightColor = (ReadStringProperty(item.Properties, "lightColor") ?? string.Empty).Trim();
						float lightOffsetX = ReadFloatProperty(item.Properties, "lightOffsetX", 0f);
						float lightOffsetY = ReadFloatProperty(item.Properties, "lightOffsetY", 0f);
						float lightRadius = ReadFloatProperty(item.Properties, "lightRadius", 0.06f);
						float lightFalloffSize = ReadFloatProperty(item.Properties, "lightFalloffSize", 0.1f);
						int lightSides = ReadIntProperty(item.Properties, "lightSides", 6);
						float lightIntensity = ReadFloatProperty(item.Properties, "lightIntensity", 1f);
						float lightFalloffIntensity = ReadFloatProperty(item.Properties, "lightFalloffIntensity", 0.5f);
						bool validLightColor = lightColor.Length == 0 || ((lightColor.Length == 7 || lightColor.Length == 9)
							&& lightColor[0] == '#' && lightColor.Skip(1).All(Uri.IsHexDigit));
						string fullPath = string.Empty;
						try { fullPath = Path.GetFullPath(Path.Combine(i_packRoot ?? string.Empty, file)); } catch (Exception) { }
						bool validFile = i_packRoot != null && ModPath.IsSafeRelativePath(file) && Path.GetExtension(file).Equals(".png", StringComparison.OrdinalIgnoreCase)
							&& fullPath.Length > 0 && AssetPatchDiscovery.IsInside(fullPath, i_packRoot) && File.Exists(fullPath)
							&& new FileInfo(fullPath).Length > 0 && new FileInfo(fullPath).Length <= 16L * 1024L * 1024L;
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.stage-item-shape", "stage-item objects must be points.", i_source);
						else if (!validFile) io_report.Add(ValidationSeverity.Error, "tiled.stage-item-file", "stage-item requires a pack-local PNG no larger than 16 MiB.", i_source);
						else if (displayName.Length == 0 || displayName.Length > 100 || description.Length > 500 || signal.Length == 0 || signal.Length > 64)
							io_report.Add(ValidationSeverity.Error, "tiled.stage-item-text", "stage-item requires a displayName and bounded onPickupSignal.", i_source);
						else if (!validLightColor || !Finite(lightOffsetX) || Math.Abs(lightOffsetX) > 20f
							|| !Finite(lightOffsetY) || Math.Abs(lightOffsetY) > 20f || !Finite(lightRadius)
							|| lightRadius <= 0f || lightRadius > 20f || !Finite(lightFalloffSize)
							|| lightFalloffSize < 0f || lightFalloffSize > 20f || lightSides < 3 || lightSides > 32
							|| !Finite(lightIntensity) || lightIntensity < 0f || lightIntensity > 10f
							|| !Finite(lightFalloffIntensity) || lightFalloffIntensity < 0f || lightFalloffIntensity > 1f
							|| !Finite(pixelsPerUnit) || pixelsPerUnit < 1f || pixelsPerUnit > 1024f || !Finite(pivotX) || pivotX < 0f || pivotX > 1f
							|| !Finite(pivotY) || pivotY < 0f || pivotY > 1f || !Finite(colliderWidth) || colliderWidth <= 0f || colliderWidth > 20f
							|| !Finite(colliderHeight) || colliderHeight <= 0f || colliderHeight > 20f
							|| !Finite(colliderOffsetX) || Math.Abs(colliderOffsetX) > 20f || !Finite(colliderOffsetY) || Math.Abs(colliderOffsetY) > 20f
							|| weight < 0 || weight > 10000 || value < 0 || value > 1000000)
							io_report.Add(ValidationSeverity.Error, "tiled.stage-item-values", "stage-item visual, collider, weight, or value properties are outside their supported bounds.", i_source);
						else io_stageItems.Add(new TiledStageItemDefinition(item, offsetX, offsetY, i_packRoot, file,
							displayName, description, signal, pixelsPerUnit, pivotX, pivotY, colliderWidth, colliderHeight,
							colliderOffsetX, colliderOffsetY, weight, value, canDrop, initiallyKinematic, lightColor, lightOffsetX,
							lightOffsetY, lightRadius, lightFalloffSize, lightSides, lightIntensity, lightFalloffIntensity));
						continue;
					}
					if (semanticType == "moving-platform")
					{
						float moveX = ReadFloatProperty(item.Properties, "moveX", 0f);
						float moveY = ReadFloatProperty(item.Properties, "moveY", 0f);
						float travelSeconds = ReadFloatProperty(item.Properties, "travelSeconds", 3f);
						float pauseSeconds = ReadFloatProperty(item.Properties, "pauseSeconds", 0.5f);
						string movingArtId = (ReadStringProperty(item.Properties, "art") ?? "field-day-grass-platform").Trim();
						if (item.Point || item.Width <= 0 || item.Height <= 0)
							io_report.Add(ValidationSeverity.Error, "tiled.moving-platform-shape", "moving-platform objects must be non-empty rectangles.", i_source);
						else if (!Finite(moveX) || !Finite(moveY) || (moveX == 0f && moveY == 0f) || Math.Abs(moveX) > 100000f || Math.Abs(moveY) > 100000f)
							io_report.Add(ValidationSeverity.Error, "tiled.moving-platform-offset", "moveX/moveY must define a finite non-zero destination within 100000 pixels.", i_source);
						else if (!Finite(travelSeconds) || travelSeconds < 0.1f || travelSeconds > 3600f || !Finite(pauseSeconds) || pauseSeconds < 0f || pauseSeconds > 3600f)
							io_report.Add(ValidationSeverity.Error, "tiled.moving-platform-timing", "travelSeconds must be 0.1..3600 and pauseSeconds must be 0..3600.", i_source);
						else if (!CoreMapArtCatalog.TryGet(movingArtId, out CoreMapArtCatalogEntry movingArt))
							io_report.Add(ValidationSeverity.Error, "tiled.moving-platform-art", "Unknown public Core map-art ID: " + movingArtId, i_source);
						else io_movingPlatforms.Add(new TiledMovingPlatformDefinition(item, offsetX, offsetY, moveX, moveY, travelSeconds, pauseSeconds, movingArt));
						continue;
					}
					if (semanticType == "particle-emitter")
					{
						string color = (ReadStringProperty(item.Properties, "color") ?? "#FFFFFF").Trim();
						float rate = ReadFloatProperty(item.Properties, "rate", 10f);
						float lifetime = ReadFloatProperty(item.Properties, "lifetime", 2f);
						float speed = ReadFloatProperty(item.Properties, "speed", 1f);
						float size = ReadFloatProperty(item.Properties, "size", 0.25f);
						float radius = ReadFloatProperty(item.Properties, "radius", 0.25f);
						bool colorValid = (color.Length == 7 || color.Length == 9) && color[0] == '#';
						for (int colorIndex = 1; colorValid && colorIndex < color.Length; colorIndex++) colorValid = Uri.IsHexDigit(color[colorIndex]);
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.particle-shape", "particle-emitter objects must be points.", i_source);
						else if (!colorValid)
							io_report.Add(ValidationSeverity.Error, "tiled.particle-color", "particle color must use #RRGGBB or #RRGGBBAA.", i_source);
						else if (!Finite(rate) || rate < 0.1f || rate > 1000f || !Finite(lifetime) || lifetime < 0.05f || lifetime > 60f
							|| !Finite(speed) || speed < 0f || speed > 100f || !Finite(size) || size < 0.01f || size > 20f
							|| !Finite(radius) || radius < 0f || radius > 100f)
							io_report.Add(ValidationSeverity.Error, "tiled.particle-values", "particle values are outside their supported bounds.", i_source);
						else io_particles.Add(new TiledParticleDefinition(item, offsetX, offsetY, color, rate, lifetime, speed, size, radius));
						continue;
					}
					if (semanticType == "nav-node")
					{
						bool isFly = ReadBoolProperty(item.Properties, "fly", false);
						string connectionType = (ReadStringProperty(item.Properties, "connectionType") ?? "move").Trim().ToLowerInvariant();
						bool bidirectional = ReadBoolProperty(item.Properties, "bidirectional", false);
						List<string> links = ParseCommaSeparatedProperty(item.Properties, "links");
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.nav-node-shape", "nav-node objects must be points.", i_source);
						else if (connectionType != "move" && connectionType != "climb")
							io_report.Add(ValidationSeverity.Error, "tiled.nav-node-connection", "nav-node connectionType must be move or climb.", i_source);
						else io_navNodes.Add(new TiledNavNodeDefinition(item, offsetX, offsetY, isFly, connectionType, bidirectional, links));
						continue;
					}
					if (semanticType == "room")
					{
						bool initial = ReadBoolProperty(item.Properties, "initial", false);
						if (item.Point || item.Width <= 0 || item.Height <= 0)
							io_report.Add(ValidationSeverity.Error, "tiled.room-shape", "room objects must be non-empty rectangles.", i_source);
						else io_rooms.Add(new TiledRoomDefinition(item, offsetX, offsetY, initial));
						continue;
					}
					if (semanticType == "room-entry")
					{
						string roomId = (ReadStringProperty(item.Properties, "room") ?? string.Empty).Trim();
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.room-entry-shape", "room-entry objects must be points.", i_source);
						else if (roomId.Length == 0)
							io_report.Add(ValidationSeverity.Error, "tiled.room-entry-room", "room-entry requires a room property.", i_source);
						else io_roomEntries.Add(new TiledRoomEntryDefinition(item, offsetX, offsetY, roomId));
						continue;
					}
					if (semanticType == "room-transition")
					{
						string destination = (ReadStringProperty(item.Properties, "destination") ?? string.Empty).Trim();
						bool requireNoEnemies = ReadBoolProperty(item.Properties, "requireNoEnemies", false);
						bool oneShot = ReadBoolProperty(item.Properties, "oneShot", false);
						if (item.Point || item.Width <= 0 || item.Height <= 0)
							io_report.Add(ValidationSeverity.Error, "tiled.room-transition-shape", "room-transition objects must be non-empty rectangles.", i_source);
						else if (destination.Length == 0)
							io_report.Add(ValidationSeverity.Error, "tiled.room-transition-destination", "room-transition requires a destination room-entry ID.", i_source);
						else io_roomTransitions.Add(new TiledRoomTransitionDefinition(item, offsetX, offsetY,
							destination, requireNoEnemies, oneShot));
						continue;
					}
					if (semanticType == "script-trigger")
					{
						if (item.Point || item.Width <= 0 || item.Height <= 0)
							io_report.Add(ValidationSeverity.Error, "tiled.script-trigger-shape", "script-trigger objects must be non-empty rectangles.", i_source);
						else io_scriptTriggers.Add(new TiledLevelObject(item, offsetX, offsetY));
						continue;
					}
					if (semanticType == "stage-marker")
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.stage-marker-shape", "stage-marker objects must be points.", i_source);
						else io_stageMarkers.Add(new TiledLevelObject(item, offsetX, offsetY));
						continue;
					}
					if (semanticType == "ambient-audio" || semanticType == "audio-source")
					{
						string file = (ReadStringProperty(item.Properties, "file") ?? string.Empty).Trim().Replace('\\', '/');
						bool ambient = semanticType == "ambient-audio";
						bool loop = ReadBoolProperty(item.Properties, "loop", true);
						bool playOnStart = ReadBoolProperty(item.Properties, "playOnStart", true);
						string mixerGroup = (ReadStringProperty(item.Properties, "mixerGroup")
							?? (ambient ? "Ambience" : "SFX")).Trim();
						float volume = ReadFloatProperty(item.Properties, "volume", 1f);
						float minDistance = ReadFloatProperty(item.Properties, "minDistance", 4f);
						float maxDistance = ReadFloatProperty(item.Properties, "maxDistance", 30f);
						string extension = Path.GetExtension(file).ToLowerInvariant();
						string fullAudioPath = string.Empty;
						try { fullAudioPath = Path.GetFullPath(Path.Combine(i_packRoot ?? string.Empty, file)); }
						catch (Exception) { }
						bool audioFileValid = i_packRoot != null && fullAudioPath.Length > 0 && AssetPatchDiscovery.IsInside(fullAudioPath, i_packRoot)
							&& File.Exists(fullAudioPath);
						if (audioFileValid)
						{
							long audioBytes = new FileInfo(fullAudioPath).Length;
							audioFileValid = audioBytes > 0 && audioBytes <= 64L * 1024L * 1024L;
						}
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.audio-shape", semanticType + " objects must be points.", i_source);
						else if (!ModPath.IsSafeRelativePath(file) || (extension != ".wav" && extension != ".ogg")
							|| !audioFileValid)
							io_report.Add(ValidationSeverity.Error, "tiled.audio-file", "Audio must be a pack-local WAV or OGG between 1 byte and 64 MiB: " + file, i_source);
						else if (mixerGroup != "Ambience" && mixerGroup != "SFX")
							io_report.Add(ValidationSeverity.Error, "tiled.audio-mixer-group", "mixerGroup must be Ambience or SFX.", i_source);
						else if (!Finite(volume) || volume < 0f || volume > 1f || !Finite(minDistance) || minDistance < 0.1f
							|| minDistance > 1000f || !Finite(maxDistance) || maxDistance < minDistance || maxDistance > 10000f)
							io_report.Add(ValidationSeverity.Error, "tiled.audio-values", "volume must be 0..1 and distance must satisfy 0.1 <= minDistance <= maxDistance <= 10000.", i_source);
						else io_audioSources.Add(new TiledAudioDefinition(item, offsetX, offsetY, i_packRoot, file,
							ambient, loop, playOnStart, mixerGroup, volume, minDistance, maxDistance));
						continue;
					}
					if (semanticType == "core-art")
					{
						if (!visible || !(item.Visible ?? true)) continue;
						if (item.Point || item.Width <= 0 || item.Height <= 0 || !Finite(item.Rotation))
						{
							io_report.Add(ValidationSeverity.Error, "tiled.core-art-shape", "core-art objects must be non-empty rectangles with finite rotation.", i_source);
							continue;
						}
						string artId = (ReadStringProperty(item.Properties, "art") ?? string.Empty).Trim();
						if (!CoreMapArtCatalog.TryGet(artId, out CoreMapArtCatalogEntry art))
						{
							io_report.Add(ValidationSeverity.Error, "tiled.core-art-id", "Unknown public Core map-art ID: " + artId, i_source);
							continue;
						}
						string sortingLayer = (ReadStringProperty(item.Properties, "sortingLayer") ?? art.DefaultSortingLayer).Trim();
						int sortingOrder = ReadIntProperty(item.Properties, "sortingOrder", 0);
						if (sortingLayer != "Background" && sortingLayer != "Decoration" && sortingLayer != "Platform")
							io_report.Add(ValidationSeverity.Error, "tiled.core-art-sorting-layer", "core-art sortingLayer must be Background, Decoration, or Platform.", i_source);
						else if (sortingOrder < -10000 || sortingOrder > 10000)
							io_report.Add(ValidationSeverity.Error, "tiled.core-art-sorting-order", "core-art sortingOrder must be between -10000 and 10000.", i_source);
						else io_coreArt.Add(new TiledCoreArtDefinition(item, offsetX, offsetY, art, sortingLayer, opacity, sortingOrder));
						continue;
					}
					if (semanticType == "decoration")
					{
						if (!visible || !(item.Visible ?? true)) continue;
						if (item.Point || item.Gid == 0 || item.Width <= 0 || item.Height <= 0 || !Finite(item.Rotation))
							io_report.Add(ValidationSeverity.Error, "tiled.decoration-shape", "Decoration objects must be visible, non-empty tile objects with finite rotation.", i_source);
						else
						{
							TiledDecorationDefinition decoration = new TiledDecorationDefinition(item, offsetX, offsetY, opacity,
								layerDrawOrder * 1000 + objectIndex);
							if (decoration.SortingLayer != "Sky" && decoration.SortingLayer != "Background" && decoration.SortingLayer != "Decoration" && decoration.SortingLayer != "Platform")
								io_report.Add(ValidationSeverity.Error, "tiled.decoration-sorting-layer", "Decoration sortingLayer must be Sky, Background, Decoration, or Platform.", i_source);
							else if (decoration.SortingOrder < -10000 || decoration.SortingOrder > 10000)
								io_report.Add(ValidationSeverity.Error, "tiled.decoration-sorting-order", "Decoration sortingOrder must be between -10000 and 10000.", i_source);
						else if (decoration.Opacity < 0f || decoration.Opacity > 1f)
							io_report.Add(ValidationSeverity.Error, "tiled.decoration-opacity", "Decoration opacity must be between 0 and 1.", i_source);
						else if (decoration.AdaptiveModeThreshold < 0f || decoration.AdaptiveModeThreshold > 1f)
							io_report.Add(ValidationSeverity.Error, "tiled.decoration-adaptive-threshold", "Decoration adaptiveModeThreshold must be between 0 and 1.", i_source);
							else io_decorations.Add(decoration);
						}
						continue;
					}
					if (semanticType == "weapon-vendor" || semanticType == "usable-vendor")
					{
						TiledLevelObject visual = new TiledLevelObject(item, offsetX, offsetY);
						if (!IsPointMarker(item))
							io_report.Add(ValidationSeverity.Error, "tiled.vendor-shape", semanticType + " objects must be points.", i_source);
						else if (!ValidOptionalObjectVisual(i_packRoot, visual.VisualFile, visual.VisualPixelsPerUnit))
							io_report.Add(ValidationSeverity.Error, "tiled.vendor-visual", "visualFile must be a pack-local PNG no larger than 16 MiB, with visualPixelsPerUnit from 1 to 1024.", i_source);
						else if (semanticType == "weapon-vendor") io_weaponVendors.Add(new TiledLevelObject(item, offsetX, offsetY));
						else io_usableVendors.Add(new TiledLevelObject(item, offsetX, offsetY));
						continue;
					}
					TiledLevelObject parsed = new TiledLevelObject(item, offsetX, offsetY);
					if (semanticType == "platform")
					{
						if (item.Point || item.Width <= 0 || item.Height <= 0)
							io_report.Add(ValidationSeverity.Error, "tiled.platform-shape", "Platform objects must be non-empty rectangles.", i_source);
						else io_platforms.Add(parsed);
					}
					else
					{
						if (!item.Point || item.Width != 0 || item.Height != 0)
							io_report.Add(ValidationSeverity.Error, "tiled.point-shape", semanticType + " objects must be points.", i_source);
						else if (semanticType == "player-spawn") io_playerSpawns.Add(parsed);
						else io_enemySpawners.Add(parsed);
					}
				}
			}
		}

		private static string ReadStringProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name)
		{
			if (i_properties == null) return null;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && string.Equals(property.Name, i_name, StringComparison.Ordinal)) return property.Value as string;
			return null;
		}

		private static bool IsPointMarker(TiledObjectDocument i_object)
		{
			return (i_object.Point && i_object.Width == 0f && i_object.Height == 0f)
				|| (i_object.Gid != 0 && i_object.Width > 0f && i_object.Height > 0f);
		}

		private static bool ValidPackPng(string i_packRoot, string i_file)
		{
			if (string.IsNullOrWhiteSpace(i_packRoot) || !ModPath.IsSafeRelativePath(i_file)
				|| !Path.GetExtension(i_file).Equals(".png", StringComparison.OrdinalIgnoreCase)) return false;
			string fullPath;
			try { fullPath = Path.GetFullPath(Path.Combine(i_packRoot, i_file)); }
			catch (Exception) { return false; }
			if (!AssetPatchDiscovery.IsInside(fullPath, i_packRoot) || !File.Exists(fullPath)) return false;
			long length = new FileInfo(fullPath).Length;
			return length > 0 && length <= 16L * 1024L * 1024L;
		}

		private static bool ValidOptionalObjectVisual(string i_packRoot, string i_file, float i_pixelsPerUnit)
		{
			return !float.IsNaN(i_pixelsPerUnit) && !float.IsInfinity(i_pixelsPerUnit)
				&& i_pixelsPerUnit >= 1f && i_pixelsPerUnit <= 1024f
				&& (string.IsNullOrEmpty(i_file) || ValidPackPng(i_packRoot, i_file));
		}

		private static bool ValidObjectVisualPair(string i_packRoot, string i_first, string i_second, float i_pixelsPerUnit)
		{
			return ValidOptionalObjectVisual(i_packRoot, i_first, i_pixelsPerUnit)
				&& ValidOptionalObjectVisual(i_packRoot, i_second, i_pixelsPerUnit)
				&& (string.IsNullOrEmpty(i_first) == string.IsNullOrEmpty(i_second));
		}

		private static bool ValidHtmlColor(string i_color)
		{
			if (string.IsNullOrEmpty(i_color) || (i_color.Length != 7 && i_color.Length != 9) || i_color[0] != '#') return false;
			for (int index = 1; index < i_color.Length; index++)
			{
				char value = i_color[index];
				if (!((value >= '0' && value <= '9') || (value >= 'a' && value <= 'f') || (value >= 'A' && value <= 'F'))) return false;
			}
			return true;
		}

		private static float[] ParseShapePath(string i_text)
		{
			if (string.IsNullOrWhiteSpace(i_text)) return null;
			string[] points = i_text.Split(';');
			if (points.Length < 3 || points.Length > 64) return null;
			float[] coordinates = new float[points.Length * 2];
			for (int index = 0; index < points.Length; index++)
			{
				string[] pair = points[index].Split(',');
				if (pair.Length != 2 || !float.TryParse(pair[0], System.Globalization.NumberStyles.Float,
					System.Globalization.CultureInfo.InvariantCulture, out coordinates[index * 2])
					|| !float.TryParse(pair[1], System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture, out coordinates[index * 2 + 1])
					|| !Finite(coordinates[index * 2]) || !Finite(coordinates[index * 2 + 1])
					|| Math.Abs(coordinates[index * 2]) > 256f || Math.Abs(coordinates[index * 2 + 1]) > 256f) return null;
			}
			return coordinates;
		}

		private static int ReadIntProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name, int i_default)
		{
			if (i_properties == null) return i_default;
			foreach (TiledPropertyDocument property in i_properties)
			{
				if (property == null || !string.Equals(property.Name, i_name, StringComparison.Ordinal) || property.Value == null) continue;
				try { return Convert.ToInt32(property.Value); }
				catch (Exception) { return i_default; }
			}
			return i_default;
		}

		private static bool ReadBoolProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name, bool i_default)
		{
			if (i_properties == null) return i_default;
			foreach (TiledPropertyDocument property in i_properties)
				if (property != null && string.Equals(property.Name, i_name, StringComparison.Ordinal)
					&& property.Value is bool value) return value;
			return i_default;
		}

		private static float ReadFloatProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name, float i_default)
		{
			if (i_properties == null) return i_default;
			foreach (TiledPropertyDocument property in i_properties)
			{
				if (property == null || !string.Equals(property.Name, i_name, StringComparison.Ordinal) || property.Value == null) continue;
				try { return Convert.ToSingle(property.Value); }
				catch (Exception) { return i_default; }
			}
			return i_default;
		}

		private static List<string> ParseCommaSeparatedProperty(IEnumerable<TiledPropertyDocument> i_properties, string i_name)
		{
			string text = ReadStringProperty(i_properties, i_name) ?? string.Empty;
			List<string> result = new List<string>();
			HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
			foreach (string part in text.Split(','))
			{
				string value = part.Trim();
				if (value.Length > 0 && unique.Add(value)) result.Add(value);
			}
			return result;
		}

		private static bool Finite(float i_value)
		{
			return !float.IsNaN(i_value) && !float.IsInfinity(i_value) && i_value >= -1000000f && i_value <= 1000000f;
		}

		private static void Error(TiledLevelLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "tiled." + i_code, i_message, i_source);
		}
	}
}
