using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MarketAlly.AIPlugin.Models
{
	/// <summary>
	/// Represents the type of change made to a line of code
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum LineChangeType
	{
		/// <summary>
		/// A new line added to the file
		/// </summary>
		Added,

		/// <summary>
		/// An existing line modified in the file
		/// </summary>
		Modified,

		/// <summary>
		/// A line removed from the file
		/// </summary>
		Deleted,

		/// <summary>
		/// A line that remains unchanged but is included for context
		/// </summary>
		Context
	}

	/// <summary>
	/// Represents a change to a specific line in a file
	/// </summary>
	public class LineChange
	{
		/// <summary>
		/// The type of change (Added, Modified, Deleted, Context)
		/// </summary>
		public LineChangeType ChangeType { get; set; }

		/// <summary>
		/// The content of the line
		/// </summary>
		public string Content { get; set; }

		/// <summary>
		/// Optional: The original content of the line (for Modified lines)
		/// This is only applicable when ChangeType is Modified
		/// </summary>
		public string OriginalContent { get; set; }

		public LineChange()
		{
			// Default constructor for deserialization
		}

		public LineChange(LineChangeType changeType, string content, string originalContent = null)
		{
			ChangeType = changeType;
			Content = content;
			OriginalContent = originalContent;
		}
	}
}