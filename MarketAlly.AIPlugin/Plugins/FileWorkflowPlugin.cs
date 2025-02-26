using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarketAlly.AIPlugin.Models;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("FileWorkflow", "Apply changes to a file using line change tracking")]
	public class FileWorkflowPlugin : IAIPlugin
	{
		[AIParameter("Full path to the file to modify", required: true)]
		public string FilePath { get; set; }

		[AIParameter("Dictionary of line changes with line numbers as keys", required: true)]
		public Dictionary<int, LineChange> LineChanges { get; set; }

		[AIParameter("Whether to create a backup of the original file", required: false)]
		public bool CreateBackup { get; set; } = true;

		[AIParameter("Whether to update the file in place (true) or just return the modified content (false)", required: false)]
		public bool UpdateFile { get; set; } = false;

		[AIParameter("Comment to add at the top of the file (optional)", required: false)]
		public string HeaderComment { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["filePath"] = typeof(string),
			["lineChanges"] = typeof(Dictionary<int, LineChange>),
			["createBackup"] = typeof(bool),
			["updateFile"] = typeof(bool),
			["headerComment"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				// Extract parameters
				string filePath = parameters["filePath"].ToString();

				bool createBackup = parameters.TryGetValue("createBackup", out var backupObj)
					? Convert.ToBoolean(backupObj)
					: true;

				bool updateFile = parameters.TryGetValue("updateFile", out var updateObj)
					? Convert.ToBoolean(updateObj)
					: false;

				string headerComment = parameters.TryGetValue("headerComment", out var headerObj)
					? headerObj?.ToString()
					: null;

				// Extract line changes
				Dictionary<int, LineChange> lineChanges = null;
				if (parameters.TryGetValue("lineChanges", out var changesObj) && changesObj != null)
				{
					// Handle different possible input formats for line changes
					if (changesObj is Dictionary<int, LineChange> typedChanges)
					{
						lineChanges = typedChanges;
					}
					else if (changesObj is Dictionary<string, object> stringKeyChanges)
					{
						lineChanges = ConvertToDictionaryOfLineChanges(stringKeyChanges);
					}
					else if (changesObj is Dictionary<object, object> objectKeyChanges)
					{
						lineChanges = ConvertToDictionaryOfLineChanges(objectKeyChanges);
					}
					else
					{
						return new AIPluginResult(
							new ArgumentException("Invalid format for lineChanges parameter"),
							"Invalid lineChanges format"
						);
					}
				}

				if (lineChanges == null || lineChanges.Count == 0)
				{
					return new AIPluginResult(
						new ArgumentException("No line changes specified"),
						"No changes to apply"
					);
				}

				// Check if file exists
				if (!File.Exists(filePath))
				{
					return new AIPluginResult(
						new FileNotFoundException($"File not found: {filePath}"),
						"File not found"
					);
				}

				// Make backup if needed
				if (createBackup)
				{
					string backupPath = $"{filePath}.{DateTime.Now:yyyyMMdd_HHmmss}.bak";
					File.Copy(filePath, backupPath);
				}

				// Read the original file
				string[] originalLines = await File.ReadAllLinesAsync(filePath);

				// Apply changes
				var (modifiedContent, changeStats) = ApplyChanges(originalLines, lineChanges, headerComment);

				// Write back to file if requested
				if (updateFile)
				{
					await File.WriteAllTextAsync(filePath, modifiedContent, Encoding.UTF8);
				}

				// Return result
				return new AIPluginResult(new
				{
					FilePath = filePath,
					ModifiedContent = modifiedContent,
					ChangeStats = changeStats,
					FileUpdated = updateFile,
					Success = true
				});
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, $"Failed to apply changes: {ex.Message}");
			}
		}

		private Dictionary<int, LineChange> ConvertToDictionaryOfLineChanges(Dictionary<string, object> stringKeyChanges)
		{
			var result = new Dictionary<int, LineChange>();

			foreach (var kvp in stringKeyChanges)
			{
				if (int.TryParse(kvp.Key, out int lineNumber))
				{
					var change = ConvertToLineChange(kvp.Value);
					if (change != null)
					{
						result[lineNumber] = change;
					}
				}
			}

			return result;
		}

		private Dictionary<int, LineChange> ConvertToDictionaryOfLineChanges(Dictionary<object, object> objectKeyChanges)
		{
			var result = new Dictionary<int, LineChange>();

			foreach (var kvp in objectKeyChanges)
			{
				if (kvp.Key != null && int.TryParse(kvp.Key.ToString(), out int lineNumber))
				{
					var change = ConvertToLineChange(kvp.Value);
					if (change != null)
					{
						result[lineNumber] = change;
					}
				}
			}

			return result;
		}

		private LineChange ConvertToLineChange(object changeObj)
		{
			if (changeObj == null)
				return null;

			if (changeObj is LineChange typedChange)
				return typedChange;

			if (changeObj is Dictionary<string, object> changeDict)
			{
				LineChangeType changeType = LineChangeType.Modified;
				string content = null;
				string originalContent = null;

				if (changeDict.TryGetValue("changeType", out var typeObj) && typeObj != null)
				{
					string typeStr = typeObj.ToString();
					if (Enum.TryParse<LineChangeType>(typeStr, true, out var parsedType))
					{
						changeType = parsedType;
					}
				}

				if (changeDict.TryGetValue("content", out var contentObj) && contentObj != null)
				{
					content = contentObj.ToString();
				}

				if (changeDict.TryGetValue("originalContent", out var origContentObj) && origContentObj != null)
				{
					originalContent = origContentObj.ToString();
				}

				if (content != null)
				{
					return new LineChange(changeType, content, originalContent);
				}
			}

			return null;
		}

		private (string ModifiedContent, object ChangeStats) ApplyChanges(string[] originalLines, Dictionary<int, LineChange> lineChanges, string headerComment)
		{
			// Keep track of stats
			int linesAdded = 0;
			int linesModified = 0;
			int linesDeleted = 0;

			// Create a list to hold the modified content
			var modifiedLines = new List<string>();

			// Add header comment if provided
			if (!string.IsNullOrEmpty(headerComment))
			{
				modifiedLines.Add(headerComment);
				linesAdded++;
			}

			// Track which lines have been deleted
			var deletedLines = lineChanges
				.Where(x => x.Value.ChangeType == LineChangeType.Deleted)
				.Select(x => x.Key)
				.ToHashSet();

			// Track which lines have been modified
			var modifiedLineDict = lineChanges
				.Where(x => x.Value.ChangeType == LineChangeType.Modified)
				.ToDictionary(x => x.Key, x => x.Value.Content);

			// Track which lines have been added (with position)
			var addedLines = lineChanges
				.Where(x => x.Value.ChangeType == LineChangeType.Added)
				.OrderBy(x => x.Key)
				.ToList();

			// Process original lines
			for (int i = 0; i < originalLines.Length; i++)
			{
				int lineNumber = i + 1; // Line numbers are 1-based

				// If this line should be deleted, skip it
				if (deletedLines.Contains(lineNumber))
				{
					linesDeleted++;
					continue;
				}

				// Check if we need to add lines before this line
				foreach (var added in addedLines.Where(x => x.Key == lineNumber))
				{
					modifiedLines.Add(added.Value.Content);
					linesAdded++;
				}

				// If this line should be modified, use the modified content
				if (modifiedLineDict.TryGetValue(lineNumber, out var lineContent))
				{
					modifiedLines.Add(lineContent);
					linesModified++;
				}
				else
				{
					// Otherwise, use the original content
					modifiedLines.Add(originalLines[i]);
				}
			}

			// Check if we need to add lines at the end
			foreach (var added in addedLines.Where(x => x.Key > originalLines.Length))
			{
				modifiedLines.Add(added.Value.Content);
				linesAdded++;
			}

			// Create the final content with line breaks
			string finalContent = string.Join(Environment.NewLine, modifiedLines);

			// Return the modified content and stats
			return (finalContent, new
			{
				OriginalLineCount = originalLines.Length,
				ModifiedLineCount = modifiedLines.Count,
				LinesAdded = linesAdded,
				LinesModified = linesModified,
				LinesDeleted = linesDeleted,
				TotalChanges = linesAdded + linesModified + linesDeleted
			});
		}
	}
}