using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModDependency
	{
		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("version", Required = Required.Always)]
		public string Version { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ModManifest
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("displayName", Required = Required.Always)]
		public string DisplayName { get; set; }

		[JsonProperty("version", Required = Required.Always)]
		public string Version { get; set; }

		[JsonProperty("modApiVersion", Required = Required.Always)]
		public int ModApiVersion { get; set; }

		[JsonProperty("authors")]
		public List<string> Authors { get; set; } = new List<string>();

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("dependencies")]
		public List<ModDependency> Dependencies { get; set; } = new List<ModDependency>();

		[JsonProperty("contentRoots", Required = Required.Always)]
		public List<string> ContentRoots { get; set; } = new List<string>();
	}

	public sealed class ModPack
	{
		public ModManifest Manifest { get; }
		public SemanticVersion Version { get; }
		public string RootPath { get; }

		public ModPack(ModManifest i_manifest, SemanticVersion i_version, string i_rootPath)
		{
			Manifest = i_manifest;
			Version = i_version;
			RootPath = i_rootPath ?? string.Empty;
		}
	}
}
