using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("UrlValidator", "Validates and parses URLs")]
	public class UrlValidatorPlugin : IAIPlugin
	{
		[AIParameter("URL to validate", required: true)]
		public string Url { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["url"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				var uri = new Uri(parameters["url"].ToString());

				var result = new
				{
					IsValid = true,
					Scheme = uri.Scheme,
					Host = uri.Host,
					Port = uri.Port,
					Path = uri.AbsolutePath,
					Query = uri.Query
				};

				return new AIPluginResult(result);
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Invalid URL");
			}
		}
	}
}
