using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("ReadFile", "Reads a file from the file system for analysis and modification")]
	public class ReadFilePlugin : IAIPlugin
	{
		[AIParameter("Full path to the file to read", required: true)]
		public string FilePath { get; set; }

		[AIParameter("Whether to return the content as lines with line numbers (true) or as a single string (false)", required: false)]
		public bool ReturnWithLineNumbers { get; set; } = true;

		[AIParameter("Maximum number of lines to return (0 for unlimited)", required: false)]
		public int MaxLines { get; set; } = 0;

		[AIParameter("Whether to include only specific line ranges, format: '1-10,15,20-25'", required: false)]
		public string LineRanges { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["filePath"] = typeof(string),
			["returnWithLineNumbers"] = typeof(bool),
			["maxLines"] = typeof(int),
			["lineRanges"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				// Extract parameters
				string filePath = parameters["filePath"].ToString();

				bool returnWithLineNumbers = parameters.TryGetValue("returnWithLineNumbers", out var lineNumObj)
					? Convert.ToBoolean(lineNumObj)
					: true;

				int maxLines = parameters.TryGetValue("maxLines", out var maxLinesObj)
					? Convert.ToInt32(maxLinesObj)
					: 0;

				string lineRanges = parameters.TryGetValue("lineRanges", out var rangesObj)
					? rangesObj?.ToString()
					: null;

				// Check if file exists
				if (!File.Exists(filePath))
				{
					return new AIPluginResult(
						new FileNotFoundException($"File not found: {filePath}"),
						"File not found"
					);
				}

				// Calculate file stats
				var fileInfo = new FileInfo(filePath);

				// Get file extension/type
				string fileExtension = Path.GetExtension(filePath).TrimStart('.').ToLower();

				// Parse line ranges if specified
				HashSet<int> linesToInclude = null;
				if (!string.IsNullOrEmpty(lineRanges))
				{
					linesToInclude = ParseLineRanges(lineRanges);
				}

				// Read the file content
				if (returnWithLineNumbers)
				{
					// Read line by line with numbers
					var lines = new Dictionary<int, string>();
					int lineCounter = 0;
					int linesIncluded = 0;

					foreach (string line in File.ReadLines(filePath, Encoding.UTF8))
					{
						lineCounter++;

						// Check if we should include this line
						if (linesToInclude != null && !linesToInclude.Contains(lineCounter))
						{
							continue;
						}

						lines[lineCounter] = line;
						linesIncluded++;

						// Check if we've reached the maximum
						if (maxLines > 0 && linesIncluded >= maxLines)
						{
							break;
						}
					}

					var result = new
					{
						FileName = Path.GetFileName(filePath),
						FileType = fileExtension,
						FileSizeBytes = fileInfo.Length,
						TotalLines = lineCounter,
						IncludedLines = linesIncluded,
						Content = lines
					};

					return new AIPluginResult(result);
				}
				else
				{
					// Read as a single string (potentially with filtering)
					string content;

					if (linesToInclude != null || maxLines > 0)
					{
						var sb = new StringBuilder();
						int lineCounter = 0;
						int linesIncluded = 0;

						foreach (string line in File.ReadLines(filePath, Encoding.UTF8))
						{
							lineCounter++;

							// Check if we should include this line
							if (linesToInclude != null && !linesToInclude.Contains(lineCounter))
							{
								continue;
							}

							sb.AppendLine(line);
							linesIncluded++;

							// Check if we've reached the maximum
							if (maxLines > 0 && linesIncluded >= maxLines)
							{
								break;
							}
						}

						content = sb.ToString();
					}
					else
					{
						// Read the entire file
						content = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
					}

					var result = new
					{
						FileName = Path.GetFileName(filePath),
						FileType = fileExtension,
						FileSizeBytes = fileInfo.Length,
						Content = content
					};

					return new AIPluginResult(result);
				}
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to read file");
			}
		}

		/// <summary>
		/// Parses a string like "1-10,15,20-25" into a set of line numbers
		/// </summary>
		private HashSet<int> ParseLineRanges(string rangeSpec)
		{
			var result = new HashSet<int>();

			if (string.IsNullOrEmpty(rangeSpec))
				return result;

			var parts = rangeSpec.Split(',', StringSplitOptions.RemoveEmptyEntries);

			foreach (var part in parts)
			{
				part.Trim();

				if (part.Contains('-'))
				{
					// Range like "1-10"
					var rangeParts = part.Split('-');
					if (rangeParts.Length == 2 &&
						int.TryParse(rangeParts[0], out int start) &&
						int.TryParse(rangeParts[1], out int end))
					{
						for (int i = start; i <= end; i++)
						{
							result.Add(i);
						}
					}
				}
				else
				{
					// Single line like "15"
					if (int.TryParse(part, out int lineNum))
					{
						result.Add(lineNum);
					}
				}
			}

			return result;
		}
	}
}