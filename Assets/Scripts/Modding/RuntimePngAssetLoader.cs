using System;
using System.IO;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class RuntimePngAssetLoader
	{
		public const int MaximumPngBytes = 32 * 1024 * 1024;

		public static bool TryLoad(string i_packRoot, string i_relativePath, string i_textureName,
			FilterMode i_filterMode, ValidationReport io_report, string i_fileIssueCode,
			string i_decodeIssueCode, string i_source, out Texture2D o_texture)
		{
			o_texture = null;
			Texture2D texture = null;
			string path = i_relativePath ?? string.Empty;
			try
			{
				path = Path.GetFullPath(Path.Combine(i_packRoot ?? string.Empty, path));
				if (!ModPath.IsSafeRelativePath(i_relativePath) || !AssetPatchDiscovery.IsInside(path, i_packRoot) || !File.Exists(path))
				{
					io_report?.Add(ValidationSeverity.Error, i_fileIssueCode, "PNG is missing or outside its pack: " + i_relativePath, i_source);
					return false;
				}

				FileInfo file = new FileInfo(path);
				if (file.Length <= 0 || file.Length > MaximumPngBytes) throw new InvalidDataException("PNG size must be between 1 byte and 32 MiB.");
				texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				if (!texture.LoadImage(File.ReadAllBytes(path), false))
				{
					UnityEngine.Object.Destroy(texture);
					texture = null;
					throw new InvalidDataException("Unity could not decode the PNG.");
				}
				texture.name = i_textureName ?? string.Empty;
				texture.filterMode = i_filterMode;
				o_texture = texture;
				return true;
			}
			catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException ||
				exception is InvalidDataException || exception is ArgumentException || exception is NotSupportedException)
			{
				if (texture != null) UnityEngine.Object.Destroy(texture);
				io_report?.Add(ValidationSeverity.Error, i_decodeIssueCode, exception.Message, path);
				return false;
			}
		}
	}
}
