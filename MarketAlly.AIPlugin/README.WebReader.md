# WebReader Plugin for MarketAlly.AIPlugin

A flexible plugin for reading and extracting content from web pages, designed to work with the MarketAlly.AIPlugin framework.

## Features

- 🌐 Read and extract content from any website
- 🧹 Clean and format HTML content automatically
- ✂️ Limit content length to avoid overwhelming responses
- 🔌 Extensible architecture via custom content providers
- 📋 Easy integration with MarketAlly.AIPlugin registry
- 🛡️ Comprehensive error handling and logging

## Installation

The WebReader plugin is distributed as part of the MarketAlly.AIPlugin package. If you've already installed the main package, you have access to this plugin.

```bash
dotnet add package MarketAlly.AIPlugin
```

## Quick Start

### Basic Usage

```csharp
// Create the default web content provider
var contentProvider = new DefaultWebContentProvider();

// Create the plugin
var webReaderPlugin = new WebReaderPlugin(contentProvider);

// Register with the AIPluginRegistry
var registry = new AIPluginRegistry(logger);
registry.RegisterPlugin(webReaderPlugin);
```

### Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddHttpClient<IWebContentProvider, DefaultWebContentProvider>();
services.AddTransient<WebReaderPlugin>();
services.AddSingleton<AIPluginRegistry>();
```

## Parameters

The WebReader plugin accepts the following parameters:

| Parameter  | Type    | Required | Default | Description                                      |
|------------|---------|----------|---------|--------------------------------------------------|
| url        | string  | Yes      | -       | URL of the webpage to read                       |
| cleanhtml  | boolean | No       | true    | Whether to clean and format the HTML content     |
| maxlength  | integer | No       | 0       | Maximum content length to return (0 for unlimited)|

## Custom Content Providers

The WebReaderPlugin uses an `IWebContentProvider` interface, allowing you to create custom implementations for specific needs.

### Interface Definition

```csharp
public interface IWebContentProvider
{
    Task<string> FetchContentAsync(string url, IDictionary<string, object> options = null);
}
```

### Creating a Custom Provider

```csharp
public class CustomWebContentProvider : IWebContentProvider
{
    public async Task<string> FetchContentAsync(string url, IDictionary<string, object> options = null)
    {
        // Your custom implementation
        // ...
        
        return content;
    }
}
```

### Example: Headless Browser Provider

```csharp
public class HeadlessBrowserProvider : IWebContentProvider
{
    private readonly IBrowser _browser;
    
    public HeadlessBrowserProvider(IBrowser browser)
    {
        _browser = browser;
    }
    
    public async Task<string> FetchContentAsync(string url, IDictionary<string, object> options = null)
    {
        var page = await _browser.NewPageAsync();
        await page.GoToAsync(url);
        
        // Wait for JavaScript to execute
        await page.WaitForNetworkIdleAsync();
        
        // Get page content
        var content = await page.GetContentAsync();
        
        // Clean if requested
        bool cleanHtml = options?.TryGetValue("cleanHtml", out var clean) == true && 
                          clean is bool cleanBool && cleanBool;
        
        if (cleanHtml)
        {
            content = CleanHtmlContent(content);
        }
        
        return content;
    }
    
    private string CleanHtmlContent(string content)
    {
        // Custom cleaning logic
        // ...
        return content;
    }
}
```

## Options Dictionary

The `options` dictionary in `FetchContentAsync` supports the following standard keys:

| Key         | Type    | Description                                     |
|-------------|---------|-------------------------------------------------|
| cleanHtml   | boolean | Whether to clean HTML tags and scripts          |
| maxLength   | integer | Maximum length of content to return             |
| timeout     | integer | Request timeout in milliseconds                 |

Custom implementations can support additional options specific to their needs.

## Error Handling

The WebReaderPlugin includes comprehensive error handling:

- Network errors are caught and returned as failure results
- Invalid URLs are properly validated
- Timeout handling is included in the default provider
- All errors are logged when a logger is provided

## Logging

The plugin supports the standard Microsoft.Extensions.Logging pattern:

```csharp
// With logging
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebReaderPlugin>();
var webReaderPlugin = new WebReaderPlugin(contentProvider, logger);
```

## Example Usage in an LLM Context

```csharp
var parameters = new Dictionary<string, object>
{
    ["url"] = "https://example.com/article",
    ["cleanhtml"] = true,
    ["maxlength"] = 5000
};

var result = await registry.CallFunctionAsync("WebReader", parameters);
if (result.Success)
{
    // Send the web content to the LLM
    var llmResponse = await llmClient.GetCompletionAsync(
        "Summarize this article: " + result.Data.ToString());
    
    Console.WriteLine(llmResponse);
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

## Security Considerations

- The default provider does not handle authentication
- Consider implementing rate limiting in production scenarios
- Be aware of web scraping ethics and restrictions
- Respect robots.txt and site terms of service

## Advanced Features

### Extending DefaultWebContentProvider

```csharp
public class EnhancedWebContentProvider : DefaultWebContentProvider
{
    public EnhancedWebContentProvider(HttpClient httpClient = null, ILogger<DefaultWebContentProvider> logger = null)
        : base(httpClient, logger)
    {
    }
    
    protected override string CleanHtmlContent(string htmlContent)
    {
        // First apply the base cleaning
        htmlContent = base.CleanHtmlContent(htmlContent);
        
        // Then apply enhanced cleaning
        htmlContent = RemoveAds(htmlContent);
        htmlContent = ExtractMainContent(htmlContent);
        
        return htmlContent;
    }
    
    private string RemoveAds(string content)
    {
        // Remove common ad patterns
        // ...
        return content;
    }
    
    private string ExtractMainContent(string content)
    {
        // Extract main article content
        // ...
        return content;
    }
}
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request with additional features or improvements.

## License

This project is licensed under the MIT License - see the LICENSE file for details.