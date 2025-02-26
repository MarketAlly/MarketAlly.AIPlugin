using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("RetrieveAnswer", "Retrieves a previously stored AI answer by its ID")]
	public class RetrieveAnswerPlugin : IAIPlugin
	{
		// Static concurrent dictionary to store answers across instances
		// In a production environment, this would be replaced with a persistent storage solution
		private static readonly ConcurrentDictionary<string, object> _answerStore = new ConcurrentDictionary<string, object>();

		[AIParameter("The unique ID of the answer to retrieve", required: true)]
		public string AnswerId { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["answerId"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				// Extract the answer ID
				string answerId = parameters["answerId"].ToString();

				// Try to retrieve the stored answer
				if (_answerStore.TryGetValue(answerId, out var storedAnswer))
				{
					return new AIPluginResult(storedAnswer, $"Successfully retrieved answer with ID: {answerId}");
				}
				else
				{
					return new AIPluginResult(
						new KeyNotFoundException($"No answer found with ID: {answerId}"),
						"Answer not found"
					);
				}
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to retrieve answer");
			}
		}

		// Static method to store an answer
		public static bool StoreAnswer(string id, object answer)
		{
			return _answerStore.TryAdd(id, answer);
		}

		// Static method to clear all stored answers (useful for testing/cleanup)
		public static void ClearAllAnswers()
		{
			_answerStore.Clear();
		}
	}
}