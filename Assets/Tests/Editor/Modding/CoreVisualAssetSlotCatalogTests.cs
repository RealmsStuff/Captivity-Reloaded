using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class CoreVisualAssetSlotCatalogTests
	{
		[Test]
		public void LegacyCatalog_HasUniqueNamesAndStableUniqueSlots()
		{
			Type catalogType = Type.GetType("CoreLegacyVisualSpriteCatalog, Assembly-CSharp");
			Type binderType = Type.GetType("CoreAssetSlotBinder, Assembly-CSharp");
			Assert.That(catalogType, Is.Not.Null);
			Assert.That(binderType, Is.Not.Null);
			string[] names = (string[])catalogType.GetField("Names", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			MethodInfo slug = binderType.GetMethod("StableSpriteSlug", BindingFlags.Static | BindingFlags.NonPublic);
			Assert.That(names.Length, Is.GreaterThanOrEqualTo(458));
			Assert.That(names.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(names.Length));

			string[] slots = names.Select(i_name => "core:sprite/" + (string)slug.Invoke(null, new object[] { i_name }) + "/image").ToArray();
			Assert.That(slots.Distinct(StringComparer.Ordinal).Count(), Is.EqualTo(slots.Length), "Two Core sprite names produce the same public slot ID.");
			Assert.That(slots, Does.Contain("core:sprite/btn-keypad-idle/image"));
			Assert.That(slots, Does.Contain("core:sprite/door-visitor-closed-left/image"));
			Assert.That(slots, Does.Contain("core:sprite/bullet-7/image"));
		}

		[Test]
		public void LegacyCatalog_NamesResolveToShippedSpritesOrDocumentedRuntimeBaselines()
		{
			Type catalogType = Type.GetType("CoreLegacyVisualSpriteCatalog, Assembly-CSharp");
			string[] names = (string[])catalogType.GetField("Names", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			HashSet<string> shippedNames = new HashSet<string>(StringComparer.Ordinal);
			foreach (string guid in AssetDatabase.FindAssets("t:Sprite", new[] { "Assets" }))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
					if (asset is Sprite sprite) shippedNames.Add(sprite.name);
			}

			HashSet<string> runtimeBaselines = new HashSet<string>(new[] { "UsableVendor", "titleBg" }, StringComparer.Ordinal);
			string[] missing = names.Where(i_name => !shippedNames.Contains(i_name) && !runtimeBaselines.Contains(i_name)).ToArray();
			Assert.That(missing, Is.Empty, "Catalog entries have no shipped or documented runtime baseline: " + string.Join(", ", missing));
		}

		[Test]
		public void AssetSlotRegistry_SnapshotIsOrdinalAndDetached()
		{
			AssetSlotRegistry registry = new AssetSlotRegistry();
			Texture2D texture = new Texture2D(1, 1);
			try
			{
				registry.Register(new AssetSlotRegistration(ContentId.Parse("core:test/z"), ContentId.Parse("core:test"), "core", "z", texture), null);
				registry.Register(new AssetSlotRegistration(ContentId.Parse("core:test/a"), ContentId.Parse("core:test"), "core", "a", texture), null);
				IReadOnlyList<AssetSlotRegistration> snapshot = registry.Snapshot();
				Assert.That(snapshot.Select(i_slot => i_slot.Id.ToString()), Is.EqualTo(new[] { "core:test/a", "core:test/z" }));
				registry.Register(new AssetSlotRegistration(ContentId.Parse("core:test/m"), ContentId.Parse("core:test"), "core", "m", texture), null);
				Assert.That(snapshot.Count, Is.EqualTo(2));
			}
			finally { UnityEngine.Object.DestroyImmediate(texture); }
		}
	}
}
