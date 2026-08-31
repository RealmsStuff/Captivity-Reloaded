using System;

namespace CaptivityReloaded.Modding
{
	public readonly struct ContentId : IEquatable<ContentId>, IComparable<ContentId>
	{
		private const int MaxLength = 160;

		private readonly string m_value;

		public string Namespace { get; }

		public string Path { get; }

		private ContentId(string i_namespace, string i_path)
		{
			Namespace = i_namespace;
			Path = i_path;
			m_value = i_namespace + ":" + i_path;
		}

		public static ContentId Parse(string i_value)
		{
			if (!TryParse(i_value, out ContentId result))
			{
				throw new FormatException("Invalid content ID: " + (i_value ?? "<null>"));
			}
			return result;
		}

		public static bool TryParse(string i_value, out ContentId o_result)
		{
			o_result = default;
			if (string.IsNullOrEmpty(i_value) || i_value.Length > MaxLength)
			{
				return false;
			}

			int separator = i_value.IndexOf(':');
			if (separator <= 0 || separator != i_value.LastIndexOf(':') || separator == i_value.Length - 1)
			{
				return false;
			}

			string idNamespace = i_value.Substring(0, separator);
			string path = i_value.Substring(separator + 1);
			if (!IsValidNamespace(idNamespace) || !IsValidPath(path))
			{
				return false;
			}

			o_result = new ContentId(idNamespace, path);
			return true;
		}

		public static bool IsValidNamespace(string i_value)
		{
			if (string.IsNullOrEmpty(i_value) || !IsAsciiLowercaseOrDigit(i_value[0]))
			{
				return false;
			}
			for (int i = 1; i < i_value.Length; i++)
			{
				char character = i_value[i];
				if (!IsAsciiLowercaseOrDigit(character) && character != '.' && character != '_' && character != '-')
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsValidPath(string i_value)
		{
			if (string.IsNullOrEmpty(i_value) || i_value[0] == '/' || i_value[i_value.Length - 1] == '/')
			{
				return false;
			}
			string[] segments = i_value.Split('/');
			foreach (string segment in segments)
			{
				if (string.IsNullOrEmpty(segment) || !IsAsciiLowercaseOrDigit(segment[0]))
				{
					return false;
				}
				for (int i = 1; i < segment.Length; i++)
				{
					char character = segment[i];
					if (!IsAsciiLowercaseOrDigit(character) && character != '-')
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool IsAsciiLowercaseOrDigit(char i_character)
		{
			return (i_character >= 'a' && i_character <= 'z') || (i_character >= '0' && i_character <= '9');
		}

		public int CompareTo(ContentId i_other)
		{
			return string.Compare(m_value, i_other.m_value, StringComparison.Ordinal);
		}

		public bool Equals(ContentId i_other)
		{
			return string.Equals(m_value, i_other.m_value, StringComparison.Ordinal);
		}

		public override bool Equals(object i_object)
		{
			return i_object is ContentId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return m_value == null ? 0 : StringComparer.Ordinal.GetHashCode(m_value);
		}

		public override string ToString()
		{
			return m_value ?? string.Empty;
		}

		public static bool operator ==(ContentId i_left, ContentId i_right)
		{
			return i_left.Equals(i_right);
		}

		public static bool operator !=(ContentId i_left, ContentId i_right)
		{
			return !i_left.Equals(i_right);
		}
	}
}
