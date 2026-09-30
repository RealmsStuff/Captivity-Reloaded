using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

/// <summary>Reconstructs migrated vanilla clothing from packaged Core data without cloning garment prefabs.</summary>
public static class CoreClothingFactory
{
	public static void Build(LibraryClothes i_library)
	{
		if (i_library == null) return;
		Dictionary<int, Clothing> built = new Dictionary<int, Clothing>();
		Dictionary<Clothing, CoreClothingDefinition> definitions = new Dictionary<Clothing, CoreClothingDefinition>();
		foreach (CoreClothingDefinition definition in ModLoaderRuntime.CoreClothingDefinitions)
		{
			if (!definition.HasLoosePresentation) continue;
			if (!TryBuild(i_library.transform, definition, out Clothing clothing))
			{
				Report("core-clothing.factory-fallback", "Could not reconstruct Core clothing; the original compatibility object remains active: " + definition.Id, definition.Source);
				continue;
			}
			built[definition.LegacyId] = clothing;
			definitions[clothing] = definition;
		}

		foreach (KeyValuePair<Clothing, CoreClothingDefinition> pair in definitions)
		{
			List<Clothing> incompatible = ResolveClothing(pair.Value.IncompatibleClothing, built, i_library);
			List<Clothing> compatible = ResolveClothing(pair.Value.CompatibleOverrides, built, i_library);
			pair.Key.ConfigureCoreCompatibility(incompatible, compatible);
			i_library.ReplaceCoreClothing(pair.Value.LegacyId, pair.Key);
			ModLoaderRuntime.Registry.BindRuntimeAsset(pair.Value.Id, pair.Key, ModLoaderRuntime.LastReport);
			CoreAssetSlotBinder.RegisterCoreClothingSlots(pair.Key, pair.Value.Id);
		}
		if (built.Count > 0)
		{
			CoreAssetSlotBinder.BindAfterSceneObjectsStarted();
			Debug.Log("[ModLoader] Reconstructed " + built.Count + " Core clothing entries from packaged data.");
		}
	}

	private static bool TryBuild(Transform i_parent, CoreClothingDefinition i_definition, out Clothing o_clothing)
	{
		o_clothing = null;
		Texture2D iconTexture = Resources.Load<Texture2D>(i_definition.Icon.Resource);
		if (iconTexture == null) return false;
		GameObject root = new GameObject(i_definition.Id.ToString());
		root.transform.SetParent(i_parent, false);
		int playerLayer = LayerMask.NameToLayer("Player");
		if (playerLayer >= 0) root.layer = playerLayer;
		try
		{
			Clothing clothing = root.AddComponent<Clothing>();
			clothing.SetIcon(CreateSprite(iconTexture, i_definition.Icon, i_definition.Id + "/icon"));
			Dictionary<string, ClothingPiece> pieces = new Dictionary<string, ClothingPiece>(StringComparer.Ordinal);
			foreach (CoreClothingPieceRecord record in i_definition.Pieces)
			{
				Texture2D texture = Resources.Load<Texture2D>(record.Sprite.Resource);
				if (texture == null) throw new InvalidOperationException("Missing piece texture: " + record.Sprite.Resource);
				GameObject pieceObject = new GameObject("clp_" + record.Id);
				pieceObject.transform.SetParent(root.transform, false);
				if (playerLayer >= 0) pieceObject.layer = playerLayer;
				SpriteRenderer renderer = pieceObject.AddComponent<SpriteRenderer>();
				renderer.sprite = CreateSprite(texture, record.Sprite, i_definition.Id + "/" + record.Id);
				renderer.sortingLayerName = "Player";
				ClothingPiece piece;
				if (record.PieceType == "hat")
				{
					ClothingPieceHat hat = pieceObject.AddComponent<ClothingPieceHat>();
					hat.ConfigureModHat(record.HidesHair); piece = hat;
				}
				else piece = pieceObject.AddComponent<ClothingPiece>();
				pieceObject.AddComponent<ModClothingSlotIdentity>().Configure(record.Slot);
				piece.ConfigureModAttachment(ToAttachment(record));
				piece.ConfigureRipSound(record.PlayRipSound);
				pieces.Add(record.Id, piece);
			}
			foreach (CoreClothingPieceRecord record in i_definition.Pieces)
			{
				List<ClothingPiece> connected = new List<ClothingPiece>();
				foreach (string id in record.ConnectedPieces)
					if (pieces.TryGetValue(id, out ClothingPiece piece)) connected.Add(piece);
				pieces[record.Id].ConfigureConnectedPieces(connected);
			}
			clothing.Initialize();
			clothing.SetId(i_definition.LegacyId);
			clothing.ConfigureModClothing(i_definition.Category, i_definition.IncompatibleCategories, null);
			RuntimeContentIdentity identity = root.AddComponent<RuntimeContentIdentity>();
			identity.Configure(i_definition.Id, ContentCategory.Clothing);
			root.SetActive(false);
			o_clothing = clothing;
			return true;
		}
		catch (Exception exception)
		{
			UnityEngine.Object.Destroy(root);
			Report("core-clothing.factory", exception.Message, i_definition.Source);
			return false;
		}
	}

	private static Sprite CreateSprite(Texture2D i_texture, CoreClothingSpriteReference i_reference, string i_name)
	{
		i_texture.filterMode = FilterMode.Point;
		Sprite sprite = Sprite.Create(i_texture, new Rect(0f, 0f, i_texture.width, i_texture.height),
			new Vector2(i_reference.PivotX, i_reference.PivotY), i_reference.PixelsPerUnit, 0, SpriteMeshType.FullRect);
		sprite.name = i_name;
		return sprite;
	}

	private static ClothingAttachmentDefinition ToAttachment(CoreClothingPieceRecord i_record)
	{
		return new ClothingAttachmentDefinition
		{
			Bone = i_record.Bone, OffsetX = i_record.OffsetX, OffsetY = i_record.OffsetY,
			Rotation = i_record.Rotation, PivotX = i_record.Sprite.PivotX, PivotY = i_record.Sprite.PivotY,
			SortingOffset = i_record.SortingOffset, AttachToBone = i_record.AttachToBone,
			HideBodyPart = i_record.HideBodyPart, Droppable = i_record.Droppable,
			Destroyable = i_record.Destroyable, DropOnOralThrust = i_record.DropOnOralThrust,
			DestroyOnOralThrust = i_record.DestroyOnOralThrust
		};
	}

	private static List<Clothing> ResolveClothing(IEnumerable<int> i_ids, IDictionary<int, Clothing> i_built, LibraryClothes i_library)
	{
		List<Clothing> result = new List<Clothing>();
		foreach (int id in i_ids ?? new int[0])
		{
			Clothing clothing;
			if (!i_built.TryGetValue(id, out clothing)) clothing = i_library.GetClothing(id);
			if (clothing != null && !result.Contains(clothing)) result.Add(clothing);
		}
		return result;
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Warning, i_code, i_message, i_source);
	}
}
