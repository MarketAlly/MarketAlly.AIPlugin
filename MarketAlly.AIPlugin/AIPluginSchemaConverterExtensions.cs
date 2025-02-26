using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Nodes;
using MarketAlly.AIPlugin.Models;

namespace MarketAlly.AIPlugin
{
	/// <summary>
	/// Extensions for the AIPluginSchemaConverter to enhance schema generation
	/// </summary>
	public static class AIPluginSchemaConverterExtensions
	{
		/// <summary>
		/// Extension method to enhance schema generation for specific dictionary types
		/// </summary>
		public static JsonObject EnhanceJsonSchema(this JsonObject schema, Type propertyType, PropertyInfo property = null)
		{
			// Handle Dictionary<int, LineChange> for PartialContent
			if (IsTypeDictionaryOfLineChanges(propertyType))
			{
				var enhancedSchema = new JsonObject
				{
					["type"] = "object",
					["description"] = property?.GetCustomAttribute<AIParameterAttribute>()?.Description
									 ?? "Partial content with line changes"
				};

				// Add schema for the LineChange items
				var additionalProperties = new JsonObject
				{
					["type"] = "object",
					["properties"] = new JsonObject
					{
						["changeType"] = new JsonObject
						{
							["type"] = "string",
							["enum"] = new JsonArray
							{
								"Added",
								"Modified",
								"Deleted",
								"Context"
							},
							["description"] = "The type of change made to this line"
						},
						["content"] = new JsonObject
						{
							["type"] = "string",
							["description"] = "The content of the line"
						},
						["originalContent"] = new JsonObject
						{
							["type"] = "string",
							["description"] = "For modified lines, the original content before changes"
						}
					},
					["required"] = new JsonArray
					{
						"changeType",
						"content"
					}
				};

				enhancedSchema["additionalProperties"] = additionalProperties;
				return enhancedSchema;
			}

			// For other types, return the original schema
			return schema;
		}

		/// <summary>
		/// Check if a type is Dictionary<int, LineChange>
		/// </summary>
		private static bool IsTypeDictionaryOfLineChanges(Type type)
		{
			if (!type.IsGenericType)
				return false;

			if (type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
				return false;

			var typeArgs = type.GetGenericArguments();
			if (typeArgs.Length != 2)
				return false;

			return typeArgs[0] == typeof(int) &&
				   typeArgs[1] == typeof(LineChange);
		}
	}
}