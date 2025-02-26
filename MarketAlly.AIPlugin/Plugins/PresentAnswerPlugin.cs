using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MarketAlly.AIPlugin.Services;
using MarketAlly.AIPlugin.Models;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("PresentAnswer", "Returns a formatted answer with metadata and confidence score")]
	public class PresentAnswerPlugin : IAIPlugin
	{
		[AIParameter("The answer message to present to the user", required: true)]
		public string Message { get; set; }

		[AIParameter("The probability/confidence score (0.0-1.0) that the answer is correct", required: false)]
		public double Probability { get; set; } = 1.0;

		[AIParameter("The file type if working with a specific file (e.g., 'csharp', 'python', 'json')", required: false)]
		public string FileType { get; set; }

		[AIParameter("The entire content of the file if applicable", required: false)]
		public string EntireContent { get; set; }

		[AIParameter("Partial content with line changes. Keys are line numbers, values describe what changed for each line. Use changeType (Added/Modified/Deleted/Context), content, and optionally originalContent for modifications.", required: false)]
		public Dictionary<int, LineChange> PartialContent { get; set; }

		[AIParameter("List of citation URLs that support the answer", required: false)]
		public List<string> Citations { get; set; }

		[AIParameter("Condensed summary of the conversation for the AI's reference in subsequent calls", required: false)]
		public string Summary { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["message"] = typeof(string),
			["probability"] = typeof(double),
			["fileType"] = typeof(string),
			["entireContent"] = typeof(string),
			["partialContent"] = typeof(Dictionary<int, LineChange>),
			["citations"] = typeof(List<string>),
			["summary"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				// Extract required parameters
				string message = parameters["message"].ToString();

				// Extract optional parameters with defaults
				double probability = parameters.TryGetValue("probability", out var prob)
					? Convert.ToDouble(prob)
					: 1.0;

				// Validate probability is between 0 and 1
				if (probability < 0 || probability > 1)
				{
					return new AIPluginResult(
						new ArgumentOutOfRangeException("probability", "Probability must be between 0.0 and 1.0"),
						"Invalid probability value"
					);
				}

				// Build file information if present
				var fileInfo = new Dictionary<string, object>();
				if (parameters.TryGetValue("fileType", out var fileType) && fileType != null)
				{
					fileInfo["type"] = fileType.ToString();

					if (parameters.TryGetValue("entireContent", out var entireContent) && entireContent != null)
					{
						fileInfo["entireContent"] = entireContent.ToString();
					}

					if (parameters.TryGetValue("partialContent", out var partialContent) && partialContent != null)
					{
						fileInfo["partialContent"] = partialContent;
					}
				}

				// Extract citations if present
				List<string> citations = new List<string>();
				if (parameters.TryGetValue("citations", out var citationsObj) && citationsObj != null)
				{
					if (citationsObj is List<object> citationsList)
					{
						foreach (var citation in citationsList)
						{
							citations.Add(citation.ToString());
						}
					}
				}

				// Extract summary if present
				string summary = null;
				if (parameters.TryGetValue("summary", out var summaryObj) && summaryObj != null)
				{
					summary = summaryObj.ToString();
				}

				// Generate a unique identifier for this answer
				string answerId = Guid.NewGuid().ToString();

				// Build the result object
				var result = new
				{
					Id = answerId,
					Message = message,
					Probability = probability,
					File = fileInfo.Count > 0 ? fileInfo : null,
					Citations = citations.Count > 0 ? citations : null,
					Summary = summary,
					Timestamp = DateTime.UtcNow
				};

				// Store the answer in the answer store
				AnswerStoreManager.Instance.StoreAnswer(answerId, result);

				return new AIPluginResult(result);
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to present answer");
			}
		}
	}
}