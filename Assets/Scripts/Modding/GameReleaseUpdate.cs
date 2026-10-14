using System;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[Serializable]
	public sealed class GameReleaseChannelDocument
	{
		public int schemaVersion;
		public GameReleaseInfo latest;
	}

	[Serializable]
	public sealed class GameReleaseInfo
	{
		public string version;
		public string name;
		public string summary;
		public string releaseUrl;
		public bool prerelease;
	}

	public static class GameReleaseUpdate
	{
		public const int SupportedSchemaVersion = 1;
		public const int MaximumDocumentCharacters = 16384;

		public static bool TryParse(string i_json, out GameReleaseInfo o_release, out string o_error)
		{
			o_release = null;
			o_error = string.Empty;
			if (string.IsNullOrWhiteSpace(i_json) || i_json.Length > MaximumDocumentCharacters)
			{
				o_error = "The release channel document is empty or too large.";
				return false;
			}

			GameReleaseChannelDocument document;
			try { document = JsonUtility.FromJson<GameReleaseChannelDocument>(i_json); }
			catch (Exception exception)
			{
				o_error = "The release channel document is not valid JSON: " + exception.Message;
				return false;
			}

			if (document == null || document.schemaVersion != SupportedSchemaVersion || document.latest == null)
			{
				o_error = "The release channel schema is not supported.";
				return false;
			}
			if (!SemanticVersion.TryParse(document.latest.version, out _))
			{
				o_error = "The release version is not valid semantic versioning.";
				return false;
			}
			if (string.IsNullOrWhiteSpace(document.latest.name) || document.latest.name.Length > 96
				|| string.IsNullOrWhiteSpace(document.latest.summary) || document.latest.summary.Length > 320)
			{
				o_error = "The release name or summary is missing or too long.";
				return false;
			}
			if (!IsTrustedReleaseUrl(document.latest.releaseUrl))
			{
				o_error = "The release URL is not an approved Captivity Reloaded GitHub release page.";
				return false;
			}

			o_release = document.latest;
			return true;
		}

		public static bool IsNewerThanInstalled(GameReleaseInfo i_release, string i_installedVersion)
		{
			return i_release != null
				&& SemanticVersion.TryParse(i_release.version, out SemanticVersion available)
				&& SemanticVersion.TryParse(i_installedVersion, out SemanticVersion installed)
				&& available.CompareTo(installed) > 0;
		}

		public static bool IsTrustedReleaseUrl(string i_url)
		{
			if (!Uri.TryCreate(i_url, UriKind.Absolute, out Uri uri)
				|| !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
				|| !string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
				|| !uri.IsDefaultPort || !string.IsNullOrEmpty(uri.UserInfo)) return false;

			return uri.AbsolutePath.StartsWith("/RealmsStuff/Captivity-Reloaded/releases/", StringComparison.OrdinalIgnoreCase);
		}
	}
}
