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
	}
}
