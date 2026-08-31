using System.Collections.Generic;

namespace CaptivityReloaded.Modding
{
	public enum ValidationSeverity
	{
		Info,
		Warning,
		Error
	}

	public sealed class ValidationIssue
	{
		public ValidationSeverity Severity { get; }
		public string Code { get; }
		public string Message { get; }
		public string Source { get; }

		public ValidationIssue(ValidationSeverity i_severity, string i_code, string i_message, string i_source = null)
		{
			Severity = i_severity;
			Code = i_code ?? string.Empty;
			Message = i_message ?? string.Empty;
			Source = i_source ?? string.Empty;
		}

		public override string ToString()
		{
			string source = Source.Length == 0 ? string.Empty : " [" + Source + "]";
			return Severity + " " + Code + source + ": " + Message;
		}
	}

	public sealed class ValidationReport
	{
		private readonly List<ValidationIssue> m_issues = new List<ValidationIssue>();

		public IReadOnlyList<ValidationIssue> Issues => m_issues;

		public bool IsValid
		{
			get
			{
				foreach (ValidationIssue issue in m_issues)
				{
					if (issue.Severity == ValidationSeverity.Error) return false;
				}
				return true;
			}
		}

		public void Add(ValidationSeverity i_severity, string i_code, string i_message, string i_source = null)
		{
			m_issues.Add(new ValidationIssue(i_severity, i_code, i_message, i_source));
		}

		public void Merge(ValidationReport i_report)
		{
			if (i_report == null) return;
			m_issues.AddRange(i_report.m_issues);
		}
	}
}
