using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	public static class AIPluginSchemaConverter
	{
		/// <summary>
		/// Converts a list of IAIPlugin instances to FunctionDefinition objects for LLM providers
		/// </summary>
		public static List<FunctionDefinition> ConvertPluginsToFunctionDefinitions(IEnumerable<IAIPlugin> plugins)
		{
			var functionDefinitions = new List<FunctionDefinition>();

			foreach (var plugin in plugins)
			{
				// Get plugin attributes using reflection
				var pluginType = plugin.GetType();
				var pluginAttribute = pluginType.GetCustomAttribute<AIPluginAttribute>();

				if (pluginAttribute == null)
					continue;

				// Create parameter schema
				var parameters = new JsonObject
				{
					["type"] = "object",
					["properties"] = new JsonObject(),
					["required"] = new JsonArray()
				};

				// Get all properties with AIParameter attribute
				var parameterProperties = pluginType.GetProperties()
					.Where(p => p.GetCustomAttribute<AIParameterAttribute>() != null)
					.ToList();

				foreach (var prop in parameterProperties)
				{
					var paramAttr = prop.GetCustomAttribute<AIParameterAttribute>();

					// Add to properties object
					var paramSchema = new JsonObject
					{
						["type"] = GetJsonSchemaType(prop.PropertyType),
						["description"] = paramAttr.Description
					};

					// Add default value if property has one and it's not the default for its type
					var defaultValue = prop.GetValue(plugin);
					if (defaultValue != null && !IsDefaultValue(defaultValue, prop.PropertyType))
					{
						paramSchema["default"] = JsonSerializer.SerializeToNode(defaultValue);
					}

					// Use parameter name from supported parameters if available
					string paramName = prop.Name.ToLowerInvariant();

					((JsonObject)parameters["properties"])[paramName] = paramSchema;

					// Add to required array if required
					if (paramAttr.Required)
					{
						((JsonArray)parameters["required"]).Add(paramName);
					}
				}

				// Create function definition
				functionDefinitions.Add(new FunctionDefinition
				{
					Name = pluginAttribute.Name,
					Description = pluginAttribute.Description,
					Parameters = parameters
				});
			}

			return functionDefinitions;
		}

		/// <summary>
		/// Convert a single plugin to a function definition
		/// </summary>
		public static FunctionDefinition ConvertPluginToFunctionDefinition(IAIPlugin plugin)
		{
			return ConvertPluginsToFunctionDefinitions(new[] { plugin }).FirstOrDefault();
		}

		/// <summary>
		/// Get the JSON schema type for a .NET type
		/// </summary>
		private static string GetJsonSchemaType(Type type)
		{
			if (type == typeof(string))
				return "string";
			else if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte))
				return "integer";
			else if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
				return "number";
			else if (type == typeof(bool))
				return "boolean";
			else if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
				return "array";
			else
				return "object";
		}

		/// <summary>
		/// Check if a value is the default for its type
		/// </summary>
		private static bool IsDefaultValue(object value, Type type)
		{
			if (type.IsValueType)
				return value.Equals(Activator.CreateInstance(type));
			return value == null;
		}

		/// <summary>
		/// Converts parameters from an JsonObject to a Dictionary for plugin execution
		/// </summary>
		public static Dictionary<string, object> ConvertJsonElementToParameters(JsonElement jsonElement)
		{
			var parameters = new Dictionary<string, object>();

			if (jsonElement.ValueKind == JsonValueKind.Object)
			{
				foreach (var property in jsonElement.EnumerateObject())
				{
					parameters[property.Name] = ExtractJsonElementValue(property.Value);
				}
			}

			return parameters;
		}

		/// <summary>
		/// Extract value from JsonElement
		/// </summary>
		private static object ExtractJsonElementValue(JsonElement element)
		{
			switch (element.ValueKind)
			{
				case JsonValueKind.String:
					return element.GetString();
				case JsonValueKind.Number:
					if (element.TryGetInt32(out int intValue))
						return intValue;
					if (element.TryGetInt64(out long longValue))
						return longValue;
					return element.GetDouble();
				case JsonValueKind.True:
					return true;
				case JsonValueKind.False:
					return false;
				case JsonValueKind.Null:
					return null;
				case JsonValueKind.Array:
					var array = new List<object>();
					foreach (var item in element.EnumerateArray())
					{
						array.Add(ExtractJsonElementValue(item));
					}
					return array;
				case JsonValueKind.Object:
					var obj = new Dictionary<string, object>();
					foreach (var property in element.EnumerateObject())
					{
						obj[property.Name] = ExtractJsonElementValue(property.Value);
					}
					return obj;
				default:
					return null;
			}
		}
	}

	public class FunctionDefinition
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("parameters")]
		public JsonObject Parameters { get; set; }
	}

}
