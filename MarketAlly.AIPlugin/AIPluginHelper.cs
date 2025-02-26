using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	public static class AIPluginHelper
	{
		public enum AIModel
		{
			OpenAI,
			Claude,
			Qwen,
			Mistral,
			Gemini
		}

		public static string GenerateSchema(AIModel modelType)
		{
			var pluginTypes = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.GetCustomAttribute<AIPluginAttribute>() != null);

			var schemaList = new List<object>();

			foreach (var pluginType in pluginTypes)
			{
				var pluginAttr = pluginType.GetCustomAttribute<AIPluginAttribute>();
				var properties = pluginType.GetProperties()
					.Where(p => p.GetCustomAttribute<AIParameterAttribute>() != null)
					.Select(p =>
					{
						var paramAttr = p.GetCustomAttribute<AIParameterAttribute>();
						return new
						{
							name = p.Name.ToLower(),
							type = MapTypeToJson(p.PropertyType),
							description = paramAttr.Description,
							required = paramAttr.Required
						};
					}).ToList();

				var schema = new
				{
					name = pluginAttr.Name,
					description = pluginAttr.Description,
					parameters = new
					{
						type = "object",
						properties = properties.ToDictionary(p => p.name, p => new
						{
							type = p.type,
							description = p.description
						}),
						required = properties.Where(p => p.required).Select(p => p.name).ToArray()
					}
				};

				schemaList.Add(schema);
			}

			return JsonSerializer.Serialize(BuildModelSchema(modelType, schemaList), new JsonSerializerOptions { WriteIndented = true });
		}

		private static object BuildModelSchema(AIModel modelType, List<object> schemaList)
		{
			return modelType switch
			{
				AIModel.OpenAI => new
				{
					tools = schemaList.Select(schema => new
					{
						type = "function",
						function = schema
					}).ToList()
				},
				AIModel.Claude => new { tools = schemaList },
				AIModel.Qwen => new { apis = schemaList },
				AIModel.Mistral => new { tools = schemaList }, // Mimics Claude's format
				AIModel.Gemini => new { functions = schemaList }, // Mimics OpenAI's format
				_ => throw new NotImplementedException($"Schema format for {modelType} is not implemented."),
			};
		}

		private static string MapTypeToJson(Type type)
		{
			if (type == typeof(string)) return "string";
			if (type == typeof(int)) return "integer";
			if (type == typeof(float) || type == typeof(double) || type == typeof(decimal)) return "number";
			if (type == typeof(bool)) return "boolean";
			if (typeof(IEnumerable<string>).IsAssignableFrom(type)) return "array";
			if (typeof(Dictionary<string, string>).IsAssignableFrom(type)) return "object";
			if (type == typeof(DateTime)) return "string";
			return "string"; // Default to string for unknown types
		}

		/// <summary>
		/// Maps the standard message role to the provider-specific role
		/// </summary>
		private static string MapRole(AIModel modelType, string role)
		{
			// Claude has specific role naming requirements
			if (modelType == AIModel.Claude && role == "assistant")
				return "assistant";

			// Default mapping (works for most providers)
			return role.ToLower();
		}

		/// <summary>
		/// Prepares a request dictionary with tools/functions based on AI model type
		/// </summary>
		/// <param name="modelType">The AI model type to prepare for</param>
		/// <param name="messages">The messages to be included in the request</param>
		/// <param name="functionDefinitions">The function definitions to include</param>
		/// <param name="defaultModel">The model name to use</param>
		/// <param name="temperature">Temperature setting for generation</param>
		/// <param name="maxTokens">Maximum tokens for the response</param>
		/// <param name="toolChoice">The tool choice setting (auto, none, or specific function name)</param>
		/// <returns>The prepared request dictionary</returns>
		public static Dictionary<string, object> PrepareRequestWithTools(
			AIModel modelType,
			IEnumerable<dynamic> messages,
			List<FunctionDefinition> functionDefinitions,
			string defaultModel,
			double temperature = 0.7,
			int? maxTokens = null,
			string toolChoice = "auto")
		{
			var result = new Dictionary<string, object>();

			// Set the model name
			result["model"] = defaultModel;

			// Set temperature
			result["temperature"] = temperature;

			// Set max tokens if provided
			if (maxTokens.HasValue)
			{
				result["max_tokens"] = maxTokens.Value;
			}

			// Handle messages based on model type
			switch (modelType)
			{
				case AIModel.Claude:
					// Claude handles system messages separately
					var systemMessage = messages.FirstOrDefault(m => m.Role?.ToString().ToLower() == "system")?.Content;
					var filteredMessages = messages
						.Where(m => m.Role?.ToString().ToLower() != "system")
						.Select(m => new {
							role = MapRole(modelType, m.Role?.ToString()),
							content = m.Content
						})
						.ToList();

					result["messages"] = filteredMessages;

					if (systemMessage != null)
					{
						result["system"] = systemMessage;
					}
					break;

				default:
					// Standard message handling for other models
					result["messages"] = messages.Select(m => new {
						role = MapRole(modelType, m.Role?.ToString()),
						content = m.Content
					}).ToList();
					break;
			}

			// Add tools/functions if provided
			if (functionDefinitions != null && functionDefinitions.Count > 0)
			{
				switch (modelType)
				{
					case AIModel.OpenAI:
					case AIModel.Gemini:
						result["tools"] = functionDefinitions.Select(fd => new
						{
							type = "function",
							function = new
							{
								name = fd.Name,
								description = fd.Description,
								parameters = fd.Parameters
							}
						}).ToList();
						result["tool_choice"] = toolChoice;
						break;

					case AIModel.Claude:
						result["tools"] = functionDefinitions.Select(fd => new
						{
							name = fd.Name,
							description = fd.Description,
							input_schema = fd.Parameters  // Claude uses input_schema instead of parameters
						}).ToList();
						result["tool_choice"] = toolChoice;
						break;

					case AIModel.Mistral:
						result["tools"] = functionDefinitions.Select(fd => new
						{
							name = fd.Name,
							description = fd.Description,
							parameters = fd.Parameters
						}).ToList();
						result["tool_choice"] = toolChoice;
						break;

					case AIModel.Qwen:
						result["apis"] = functionDefinitions.Select(fd => new
						{
							name = fd.Name,
							description = fd.Description,
							parameters = fd.Parameters
						}).ToList();
						result["api_choice"] = toolChoice;
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Serializes a request with tools/functions to JSON for AI model consumption
		/// </summary>
		/// <param name="modelType">The AI model type to prepare for</param>
		/// <param name="messages">The messages to be included in the request</param>
		/// <param name="functionDefinitions">The function definitions to include</param>
		/// <param name="defaultModel">The model name to use</param>
		/// <param name="temperature">Temperature setting for generation</param>
		/// <param name="maxTokens">Maximum tokens for the response</param>
		/// <param name="toolChoice">The tool choice setting (auto, none, or specific function name)</param>
		/// <param name="options">Optional JsonSerializerOptions for serialization</param>
		/// <returns>A JSON string representation of the request</returns>
		public static string SerializeRequestWithTools(
			AIModel modelType,
			IEnumerable<dynamic> messages,
			List<FunctionDefinition> functionDefinitions,
			string defaultModel,
			double temperature = 0.7,
			int? maxTokens = null,
			string toolChoice = "auto",
			JsonSerializerOptions options = null)
		{
			var requestObj = PrepareRequestWithTools(
				modelType,
				messages,
				functionDefinitions,
				defaultModel,
				temperature,
				maxTokens,
				toolChoice);

			return JsonSerializer.Serialize(requestObj, options ?? new JsonSerializerOptions { WriteIndented = true });
		}
	}
}
