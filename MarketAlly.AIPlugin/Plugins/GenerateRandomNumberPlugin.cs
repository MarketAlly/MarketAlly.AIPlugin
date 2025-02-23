using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	public class GenerateRandomNumberPlugin : IAIPlugin
	{
		private readonly Random _random = new Random();

		public async Task<string> ExecuteAsync(Dictionary<string, string> parameters)
		{
			await Task.Delay(200);
			int randomNumber = _random.Next(1, 100);
			return $"Random number: {randomNumber}";
		}
	}
}
