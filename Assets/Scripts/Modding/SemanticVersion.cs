using System;

namespace CaptivityReloaded.Modding
{
	public readonly struct SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
	{
		public int Major { get; }
		public int Minor { get; }
		public int Patch { get; }
		public string PreRelease { get; }

		private SemanticVersion(int i_major, int i_minor, int i_patch, string i_preRelease)
		{
			Major = i_major;
			Minor = i_minor;
			Patch = i_patch;
			PreRelease = i_preRelease ?? string.Empty;
		}

		public static bool TryParse(string i_value, out SemanticVersion o_version)
		{
			o_version = default;
			if (string.IsNullOrWhiteSpace(i_value))
			{
				return false;
			}

			string value = i_value.Trim();
			int buildIndex = value.IndexOf('+');
			if (buildIndex >= 0)
			{
				if (!IsValidIdentifierList(value.Substring(buildIndex + 1)))
				{
					return false;
				}
				value = value.Substring(0, buildIndex);
			}

			string preRelease = string.Empty;
			int preReleaseIndex = value.IndexOf('-');
			if (preReleaseIndex >= 0)
			{
				preRelease = value.Substring(preReleaseIndex + 1);
				if (!IsValidIdentifierList(preRelease))
				{
					return false;
				}
				value = value.Substring(0, preReleaseIndex);
			}

			string[] parts = value.Split('.');
			if (parts.Length != 3 || !TryParseNumber(parts[0], out int major) || !TryParseNumber(parts[1], out int minor) || !TryParseNumber(parts[2], out int patch))
			{
				return false;
			}

			o_version = new SemanticVersion(major, minor, patch, preRelease);
			return true;
		}

		private static bool TryParseNumber(string i_value, out int o_number)
		{
			if (string.IsNullOrEmpty(i_value) || (i_value.Length > 1 && i_value[0] == '0'))
			{
				o_number = 0;
				return false;
			}
			return int.TryParse(i_value, out o_number) && o_number >= 0;
		}

		private static bool IsValidIdentifierList(string i_value)
		{
			if (string.IsNullOrEmpty(i_value))
			{
				return false;
			}
			foreach (char character in i_value)
			{
				bool valid = (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') || (character >= '0' && character <= '9') || character == '-' || character == '.';
				if (!valid)
				{
					return false;
				}
			}
			return true;
		}

		public int CompareTo(SemanticVersion i_other)
		{
			int result = Major.CompareTo(i_other.Major);
			if (result == 0) result = Minor.CompareTo(i_other.Minor);
			if (result == 0) result = Patch.CompareTo(i_other.Patch);
			if (result != 0) return result;
			if (PreRelease.Length == 0 && i_other.PreRelease.Length > 0) return 1;
			if (PreRelease.Length > 0 && i_other.PreRelease.Length == 0) return -1;
			return ComparePreRelease(PreRelease, i_other.PreRelease);
		}

		private static int ComparePreRelease(string i_left, string i_right)
		{
			string[] leftParts = i_left.Split('.');
			string[] rightParts = i_right.Split('.');
			int length = Math.Min(leftParts.Length, rightParts.Length);
			for (int i = 0; i < length; i++)
			{
				bool leftNumeric = int.TryParse(leftParts[i], out int leftNumber);
				bool rightNumeric = int.TryParse(rightParts[i], out int rightNumber);
				int result;
				if (leftNumeric && rightNumeric) result = leftNumber.CompareTo(rightNumber);
				else if (leftNumeric) result = -1;
				else if (rightNumeric) result = 1;
				else result = string.Compare(leftParts[i], rightParts[i], StringComparison.Ordinal);
				if (result != 0) return result;
			}
			return leftParts.Length.CompareTo(rightParts.Length);
		}

		public bool Equals(SemanticVersion i_other)
		{
			return CompareTo(i_other) == 0;
		}

		public override bool Equals(object i_object)
		{
			return i_object is SemanticVersion other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Major;
				hash = hash * 397 ^ Minor;
				hash = hash * 397 ^ Patch;
				hash = hash * 397 ^ StringComparer.Ordinal.GetHashCode(PreRelease);
				return hash;
			}
		}

		public override string ToString()
		{
			return Major + "." + Minor + "." + Patch + (PreRelease.Length == 0 ? string.Empty : "-" + PreRelease);
		}
	}

	public readonly struct VersionRange
	{
		private readonly SemanticVersion m_version;
		private readonly bool m_isMinimum;

		private VersionRange(SemanticVersion i_version, bool i_isMinimum)
		{
			m_version = i_version;
			m_isMinimum = i_isMinimum;
		}

		public static bool TryParse(string i_value, out VersionRange o_range)
		{
			o_range = default;
			if (string.IsNullOrWhiteSpace(i_value)) return false;
			string value = i_value.Trim();
			bool isMinimum = value.StartsWith(">=", StringComparison.Ordinal);
			if (isMinimum) value = value.Substring(2).Trim();
			if (!SemanticVersion.TryParse(value, out SemanticVersion version)) return false;
			o_range = new VersionRange(version, isMinimum);
			return true;
		}

		public bool Contains(SemanticVersion i_version)
		{
			int comparison = i_version.CompareTo(m_version);
			return m_isMinimum ? comparison >= 0 : comparison == 0;
		}
	}
}
