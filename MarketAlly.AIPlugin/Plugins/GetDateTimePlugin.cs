using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("GetDateTime", "Gets the current date and time")]
	public class GetDateTimePlugin : IAIPlugin
	{
		[AIParameter("The format string for the datetime", required: false)]
		public string Format { get; set; }

		[AIParameter("Whether to use UTC time", required: true)]
		public bool UseUtc { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["format"] = typeof(string),
			["useUtc"] = typeof(bool)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				string? format = parameters.TryGetValue("format", out var formatValue)
					? formatValue?.ToString()
					: "yyyy-MM-dd HH:mm:ss";

				bool useUtc = parameters.TryGetValue("useUtc", out var utcValue)
					? Convert.ToBoolean(utcValue)
					: true; // Default to UTC for safety

				DateTime currentTime = useUtc ? DateTime.UtcNow : DateTime.Now;
				string timeZone = useUtc ? "UTC" : TimeZoneInfo.Local.DisplayName;
				string result = currentTime.ToString(format);

				return new AIPluginResult(
					result,
					$"Current DateTime: {result} {timeZone}"
				);
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to get current date time");
			}
		}
	}
}
