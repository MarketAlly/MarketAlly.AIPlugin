using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	public class AIPluginRegistry
	{
		private readonly ILogger<AIPluginRegistry> _logger;
		private readonly Dictionary<string, IAIPlugin> _plugins = new();

		public class PluginInfo
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public Dictionary<string, (Type Type, string Description, bool Required)> Parameters { get; set; }
		}

		public AIPluginRegistry(ILogger<AIPluginRegistry> logger)
		{
			_logger = logger;
		}

		public IReadOnlyDictionary<string, PluginInfo> GetAvailableFunctions()
		{
			var pluginInfo = new Dictionary<string, PluginInfo>();

			foreach (var plugin in _plugins)
			{
				var type = plugin.Value.GetType();
				var pluginAttr = type.GetCustomAttribute<AIPluginAttribute>();

				if (pluginAttr == null) continue;

				var parameters = type.GetProperties()
					.Where(p => p.GetCustomAttribute<AIParameterAttribute>() != null)
					.ToDictionary(
						p => p.Name.ToLower(),
						p =>
						{
							var paramAttr = p.GetCustomAttribute<AIParameterAttribute>();
							return (p.PropertyType, paramAttr.Description, paramAttr.Required);
						}
					);

				pluginInfo[plugin.Key] = new PluginInfo
				{
					Name = pluginAttr.Name,
					Description = pluginAttr.Description,
					Parameters = parameters
				};
			}

			return pluginInfo;
		}

		public void RegisterPlugin(IAIPlugin plugin)
		{
			ArgumentNullException.ThrowIfNull(plugin);

			var pluginAttr = plugin.GetType().GetCustomAttribute<AIPluginAttribute>();
			if (pluginAttr == null)
			{
				throw new ArgumentException($"Plugin {plugin.GetType().Name} must have AIPluginAttribute");
			}

			var pluginName = pluginAttr.Name.ToLower();
			_logger.LogInformation("Registering plugin: {PluginName}", pluginName);
			_plugins[pluginName] = plugin;
		}

		public void RegisterPlugin(string functionName, IAIPlugin plugin)
		{
			ArgumentNullException.ThrowIfNull(functionName);
			ArgumentNullException.ThrowIfNull(plugin);

			_logger.LogInformation("Registering plugin: {FunctionName}", functionName);
			_plugins[functionName] = plugin;
		}

		public async Task<AIPluginResult> CallFunctionAsync(string functionName, Dictionary<string, object> parameters)
		{
			try
			{
				// Convert to lowercase for comparison
				var key = functionName.ToLower();
				if (!_plugins.TryGetValue(key, out var plugin))
				{
					return new AIPluginResult(
						new KeyNotFoundException($"Function '{functionName}' not found."),
						"Plugin not found");
				}

				ValidateParameters(plugin, parameters);

				_logger.LogDebug("Executing plugin: {FunctionName}", functionName);
				return await plugin.ExecuteAsync(parameters);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error executing plugin: {FunctionName}", functionName);
				return new AIPluginResult(ex, "Plugin execution failed");
			}
		}

		private void ValidateParameters(IAIPlugin plugin, IReadOnlyDictionary<string, object> parameters)
		{
			var pluginType = plugin.GetType();
			var requiredParams = pluginType.GetProperties()
				.Where(p => p.GetCustomAttribute<AIParameterAttribute>()?.Required == true)
				.Select(p => p.Name.ToLower());

			// Check if any required parameters are missing
			var missingParams = requiredParams.Except(parameters.Keys, StringComparer.OrdinalIgnoreCase);
			if (missingParams.Any())
			{
				throw new ArgumentException(
					$"Missing required parameters: {string.Join(", ", missingParams)}");
			}

			// Validate parameter types
			foreach (var param in parameters)
			{
				if (!plugin.SupportedParameters.TryGetValue(param.Key, out var expectedType))
				{
					throw new ArgumentException($"Unsupported parameter: {param.Key}");
				}

				if (param.Value != null && !expectedType.IsAssignableFrom(param.Value.GetType()))
				{
					throw new ArgumentException(
						$"Invalid type for parameter '{param.Key}'. Expected {expectedType.Name}, got {param.Value.GetType().Name}");
				}
			}
		}
	}
}
