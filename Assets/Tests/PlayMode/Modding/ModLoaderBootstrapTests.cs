using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CaptivityReloaded.Modding.Tests
{
	public class ModLoaderBootstrapTests
	{
		[UnityTest]
		public IEnumerator BeforeSceneLoad_LoadsPackagedCoreCatalog()
		{
			yield return null;
			Assert.That(ModLoaderRuntime.LoadedPacks.Any(pack => pack.Manifest.Id == "core"), Is.True);
			Assert.That(ModLoaderRuntime.CoreContentCatalog.Count, Is.EqualTo(238));
			Assert.That(ModLoaderRuntime.LegacyContentMap.Count, Is.EqualTo(205));
			Assert.That(ModLoaderRuntime.Registry, Is.Not.Null);
			Assert.That(ModLoaderRuntime.AssetSlots, Is.Not.Null);
			Assert.That(ModLoaderRuntime.EnemyDefinitions, Is.Not.Null);
			Assert.That(ModLoaderRuntime.ClothingDefinitions, Is.Not.Null);
		}
	}
}
