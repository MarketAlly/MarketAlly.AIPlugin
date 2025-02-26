using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MarketAlly.AIPlugin.Plugins
{
	/// <summary>
	/// Interface for web content providers
	/// </summary>
	public interface IWebContentProvider
	{
		/// <summary>
		/// Fetches content from a web page
		/// </summary>
		/// <param name="url">URL to fetch content from</param>
		/// <param name="options">Additional options for content retrieval</param>
		/// <returns>The web page content as a string</returns>
		Task<string> FetchContentAsync(string url, IDictionary<string, object> options = null);
	}

	/// <summary>
	/// Plugin that reads and extracts content from web pages
	/// </summary>
	[AIPlugin("WebReader", "Reads and extracts content from web pages")]
	public class WebReaderPlugin : IAIPlugin
	{
		private readonly IWebContentProvider _contentProvider;
		private readonly ILogger<WebReaderPlugin> _logger;

		/// <summary>
		/// Creates a new instance of WebReaderPlugin
		/// </summary>
		/// <param name="contentProvider">The content provider to use for fetching web content</param>
		/// <param name="logger">Optional logger for recording operations</param>
		public WebReaderPlugin(IWebContentProvider contentProvider, ILogger<WebReaderPlugin> logger = null)
		{
			_contentProvider = contentProvider ?? throw new ArgumentNullException(nameof(contentProvider));
			_logger = logger;
		}

		[AIParameter("URL of the webpage to read", required: true)]
		public string Url { get; set; }

		[AIParameter("Whether to clean and format the HTML content", required: false)]
		public bool CleanHtml { get; set; } = true;

		[AIParameter("Maximum content length to return (0 for unlimited)", required: false)]
		public int MaxLength { get; set; } = 0;

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["url"] = typeof(string),
			["cleanhtml"] = typeof(bool),
			["maxlength"] = typeof(int)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				_logger?.LogInformation("WebReader plugin executing for URL {Url}", parameters["url"]);

				string url = parameters["url"].ToString();

				// Extract optional parameters
				bool cleanHtml = parameters.TryGetValue("cleanhtml", out var cleanHtmlValue)
					? Convert.ToBoolean(cleanHtmlValue)
					: true;  // Default to true if not specified

				int maxLength = parameters.TryGetValue("maxlength", out var maxLengthValue)
					? Convert.ToInt32(maxLengthValue)
					: 0;  // Default to unlimited

				// Create options dictionary for the content provider
				var options = new Dictionary<string, object>
				{
					["cleanHtml"] = cleanHtml,
					["maxLength"] = maxLength
				};

				// Fetch the content using the provided content provider
				var content = await _contentProvider.FetchContentAsync(url, options);

				// Trim content if maxLength is specified and greater than 0
				if (maxLength > 0 && content.Length > maxLength)
				{
					content = content.Substring(0, maxLength) + "... (content truncated)";
				}

				_logger?.LogInformation("Successfully fetched {ContentLength} characters from {Url}",
					content.Length, url);

				return new AIPluginResult(
					content,
					$"Successfully fetched content from {url}"
				);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Failed to read web content from {Url}", parameters["url"]);
				return new AIPluginResult(ex, "Failed to read web content");
			}
		}
	}

	/// <summary>
	/// Default implementation of IWebContentProvider using HttpClient
	/// </summary>
	public class DefaultWebContentProvider : IWebContentProvider
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<DefaultWebContentProvider> _logger;

		/// <summary>
		/// Creates a new instance of DefaultWebContentProvider
		/// </summary>
		/// <param name="httpClient">Optional HttpClient instance</param>
		/// <param name="logger">Optional logger for recording operations</param>
		public DefaultWebContentProvider(HttpClient httpClient = null, ILogger<DefaultWebContentProvider> logger = null)
		{
			_httpClient = httpClient ?? new HttpClient();
			_logger = logger;
		}

		/// <inheritdoc/>
		public async Task<string> FetchContentAsync(string url, IDictionary<string, object> options = null)
		{
			try
			{
				_logger?.LogDebug("Fetching content from {Url}", url);

				var response = await _httpClient.GetStringAsync(url);

				bool cleanHtml = options != null &&
								 options.TryGetValue("cleanHtml", out var cleanHtmlObj) &&
								 cleanHtmlObj is bool cleanHtmlBool &&
								 cleanHtmlBool;

				if (cleanHtml)
				{
					_logger?.LogDebug("Cleaning HTML content from {Url}", url);
					response = CleanHtmlContent(response);
				}

				return response;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error fetching content from {Url}", url);
				throw new Exception($"Error fetching content from {url}: {ex.Message}", ex);
			}
		}

		/// <summary>
		/// Cleans HTML content by removing scripts, styles, and comments
		/// </summary>
		/// <param name="htmlContent">The raw HTML content</param>
		/// <returns>Cleaned HTML content</returns>
		protected virtual string CleanHtmlContent(string htmlContent)
		{
			// This is a basic implementation - users can override this method to provide custom cleaning logic

			// Remove script tags
			htmlContent = System.Text.RegularExpressions.Regex.Replace(
				htmlContent,
				@"<script[^>]*>[\s\S]*?</script>",
				string.Empty,
				System.Text.RegularExpressions.RegexOptions.IgnoreCase
			);

			// Remove style tags
			htmlContent = System.Text.RegularExpressions.Regex.Replace(
				htmlContent,
				@"<style[^>]*>[\s\S]*?</style>",
				string.Empty,
				System.Text.RegularExpressions.RegexOptions.IgnoreCase
			);

			// Remove HTML comments
			htmlContent = System.Text.RegularExpressions.Regex.Replace(
				htmlContent,
				@"<!--[\s\S]*?-->",
				string.Empty,
				System.Text.RegularExpressions.RegexOptions.IgnoreCase
			);

			return htmlContent;
		}
	}
}