using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("GenerateRandomNumber", "Generates a random number within a specified range")]
	public class GenerateRandomNumberPlugin : IAIPlugin
	{
		private readonly Random _random = new Random();

		[AIParameter("The minimum value (inclusive)", required: true)]
		public int Min { get; set; }

		[AIParameter("The maximum value (exclusive)", required: true)]
		public int Max { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["min"] = typeof(int),
			["max"] = typeof(int)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				int min = Convert.ToInt32(parameters["min"]);
				int max = Convert.ToInt32(parameters["max"]);

				if (min >= max)
				{
					return new AIPluginResult(
						new ArgumentException($"Min value ({min}) must be less than max value ({max})"),
						"Invalid parameters");
				}

				int randomNumber = _random.Next(min, max);
				return new AIPluginResult(randomNumber, $"Random number between {min} and {max}: {randomNumber}");
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to generate random number");
			}
		}
	}
}
