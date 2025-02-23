using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin
{
	public class AIPluginRegistry
	{
		private readonly Dictionary<string, IAIPlugin> _plugins = new Dictionary<string, IAIPlugin>();

		public void RegisterPlugin(string functionName, IAIPlugin plugin)
		{
			_plugins[functionName] = plugin;
		}

		public async Task<string> CallFunctionAsync(string functionName, Dictionary<string, string> parameters)
		{
			if (_plugins.TryGetValue(functionName, out var plugin))
			{
				return await plugin.ExecuteAsync(parameters);
			}
			return $"Function '{functionName}' not found.";
		}

		public List<string> GetAvailableFunctions()
		{
			return new List<string>(_plugins.Keys);
		}
	}
}
