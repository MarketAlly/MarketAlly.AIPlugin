using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("StringManipulator", "Performs various string operations")]
	public class StringManipulatorPlugin : IAIPlugin
	{
		[AIParameter("Input text to process", required: true)]
		public string Input { get; set; }

		[AIParameter("Operation to perform (reverse, uppercase, lowercase, trim)", required: true)]
		public string Operation { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["input"] = typeof(string),
			["operation"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			string input = parameters["input"].ToString();
			string operation = parameters["operation"].ToString().ToLower();

			string result = operation switch
			{
				"reverse" => new string(input.Reverse().ToArray()),
				"uppercase" => input.ToUpper(),
				"lowercase" => input.ToLower(),
				"trim" => input.Trim(),
				_ => throw new ArgumentException($"Unknown operation: {operation}")
			};

			return new AIPluginResult(result);
		}
	}
}
