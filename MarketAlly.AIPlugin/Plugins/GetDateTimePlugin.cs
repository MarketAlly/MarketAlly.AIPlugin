using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	public class GetDateTimePlugin : IAIPlugin
	{
		public async Task<string> ExecuteAsync(Dictionary<string, string> parameters)
		{
			await Task.Delay(20);
			return $"Current DateTime: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";
		}
	}
}
