using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CaptivityReloaded.Modding.Tests
{
	public class ModLoaderBootstrapTests
	{
		[UnityTest]
		public IEnumerator BeforeSceneLoad_LoadsPackagedCoreWithoutGameplayEntries()
		{
			yield return null;
			Assert.That(ModLoaderRuntime.LoadedPacks.Any(pack => pack.Manifest.Id == "core"), Is.True);
			Assert.That(ModLoaderRuntime.Registry, Is.Not.Null);
			Assert.That(ModLoaderRuntime.Registry.Count, Is.Zero);
		}
	}
}
