using System;
using System.IO;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class RuntimePngAssetLoader
	{
		public const int MaximumPngBytes = 32 * 1024 * 1024;
		public const int MaximumPngDimension = 8192;
		public const long MaximumPngPixels = 32L * 1024L * 1024L;

		public static Vector2 GetReplacementPivot(Sprite i_baseline, int i_width, int i_height)
		{
			if (i_baseline.texture != null && i_width == i_baseline.texture.width && i_height == i_baseline.texture.height)
			{
				return new Vector2(
					(i_baseline.rect.x + i_baseline.pivot.x) / i_width,
					(i_baseline.rect.y + i_baseline.pivot.y) / i_height);
			}
			return new Vector2(i_baseline.pivot.x / i_baseline.rect.width, i_baseline.pivot.y / i_baseline.rect.height);
		}

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
				if (texture.width > MaximumPngDimension || texture.height > MaximumPngDimension
					|| (long)texture.width * texture.height > MaximumPngPixels)
				{
					UnityEngine.Object.Destroy(texture);
					texture = null;
					throw new InvalidDataException("PNG dimensions exceed 8192 pixels on one side or 32 megapixels total.");
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
