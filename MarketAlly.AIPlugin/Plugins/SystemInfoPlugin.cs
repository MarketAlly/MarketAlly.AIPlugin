using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("SystemInfo", "Gets current system information")]
	public class SystemInfoPlugin : IAIPlugin
	{
		public IReadOnlyDictionary<string, Type> SupportedParameters =>
			new Dictionary<string, Type>();

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			var info = new
			{
				OSVersion = Environment.OSVersion.ToString(),
				ProcessorCount = Environment.ProcessorCount,
				MachineName = Environment.MachineName,
				SystemDirectory = Environment.SystemDirectory,
				WorkingMemory = Environment.WorkingSet,
				Is64BitOperatingSystem = Environment.Is64BitOperatingSystem
			};

			return new AIPluginResult(info);
		}
	}
}
