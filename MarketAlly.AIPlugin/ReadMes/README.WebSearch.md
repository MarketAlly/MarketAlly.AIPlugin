# WebSearch Plugin for MarketAlly AI Plugin Toolkit

A flexible and extensible plugin for performing web searches through various search engines, designed to integrate seamlessly with the MarketAlly.AIPlugin framework.

## Features

- 🔍 Perform web searches across multiple search engines (Brave, Google, Bing)
- 🔄 Configurable number of results to retrieve
- 🏢 Site-specific filtering capabilities
- 🔌 Extensible architecture via custom search providers
- 📋 Clean, standardized result format for AI consumption
- 🛡️ Comprehensive error handling and logging

## Installation

The WebSearch plugin is included in the MarketAlly.AIPlugin package:

```bash
dotnet add package MarketAlly.AIPlugin
```

## Quick Start

### Basic Usage

```csharp
// Create a search provider with your API key
var searchProvider = new DefaultWebSearchProvider("your-api-key");

// Create the plugin
var webSearchPlugin = new WebSearchPlugin(searchProvider);

// Register with the AIPluginRegistry
var registry = new AIPluginRegistry(logger);
registry.RegisterPlugin(webSearchPlugin);
```

### Dependency Injection

```csharp
// In Startup.cs or Program.cs
services.AddHttpClient<DefaultWebSearchProvider>(client => {
    client.BaseAddress = new Uri("https://api.search.brave.com/");
});
services.AddSingleton<IWebSearchProvider>(sp => 
    new DefaultWebSearchProvider(
        Configuration["SearchApiKey"], 
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<ILogger<DefaultWebSearchProvider>>()
    )
);
services.AddTransient<WebSearchPlugin>();
services.AddSingleton<AIPluginRegistry>();
```

## Parameters

The WebSearch plugin accepts the following parameters:

| Parameter | Type    | Required | Default | Description                                    |
|-----------|---------|----------|---------|------------------------------------------------|
| query     | string  | Yes      | -       | Search query                                   |
| count     | integer | No       | 3       | Number of results to return (max 10)           |
| engine    | string  | No       | brave   | Search engine to use (brave, google, bing)     |
| site      | string  | No       | null    | Filter results to specific site or domain      |

## Search Result Format

Each search result contains the following information:

```csharp
public class SearchResult
{
    public string Title { get; set; }    // Title of the result
    public string Url { get; set; }      // URL of the result
    public string Snippet { get; set; }  // Description or snippet
    public DateTime? Date { get; set; }  // Optional publication date
    public string Source { get; set; }   // Source domain
}
```

## Custom Search Providers

The WebSearchPlugin uses an `IWebSearchProvider` interface, allowing you to create custom implementations:

### Interface Definition

```csharp
public interface IWebSearchProvider
{
    Task<IList<SearchResult>> SearchAsync(string query, IDictionary<string, object> options = null);
}
```

### Creating a Custom Provider

```csharp
public class CustomSearchProvider : IWebSearchProvider
{
    public async Task<IList<SearchResult>> SearchAsync(string query, IDictionary<string, object> options = null)
    {
        // Your custom implementation
        // ...
        
        return results;
    }
}
```

### Example: Specialized Search Provider

```csharp
public class AcademicSearchProvider : IWebSearchProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public AcademicSearchProvider(string apiKey, HttpClient httpClient = null)
    {
        _apiKey = apiKey;
        _httpClient = httpClient ?? new HttpClient();
    }
    
    public async Task<IList<SearchResult>> SearchAsync(string query, IDictionary<string, object> options = null)
    {
        // Add academic-specific parameters
        var requestUri = $"https://api.academic-search.com/search?q={Uri.EscapeDataString(query)}&key={_apiKey}";
        
        // Extract "year" parameter if provided
        if (options?.TryGetValue("year", out var yearObj) == true && yearObj is int year)
        {
            requestUri += $"&year={year}";
        }
        
        var response = await _httpClient.GetStringAsync(requestUri);
        
        // Parse response and map to SearchResult objects
        // ...
        
        return results;
    }
}
```

## Options Dictionary

The `options` dictionary in `SearchAsync` supports the following standard keys:

| Key       | Type    | Description                                     |
|-----------|---------|-------------------------------------------------|
| count     | integer | Number of results to return                     |
| engine    | string  | Search engine to use                            |
| site      | string  | Filter results to specific site                 |

Custom implementations can support additional options specific to their needs.

## Error Handling

The WebSearchPlugin includes comprehensive error handling:

- API errors are caught and returned as failure results
- Invalid parameter values are validated and corrected
- Results are limited to reasonable values
- All errors are logged when a logger is provided

## Usage Examples

### Basic Search

```csharp
var parameters = new Dictionary<string, object>
{
    ["query"] = "climate change solutions",
    ["count"] = 5
};

var result = await registry.CallFunctionAsync("WebSearch", parameters);
if (result.Success)
{
    var searchResults = (IList<SearchResult>)result.Data;
    foreach (var item in searchResults)
    {
        Console.WriteLine($"Title: {item.Title}");
        Console.WriteLine($"URL: {item.Url}");
        Console.WriteLine($"Snippet: {item.Snippet}");
        Console.WriteLine();
    }
}
```

### Site-Specific Search

```csharp
var parameters = new Dictionary<string, object>
{
    ["query"] = "renewable energy",
    ["site"] = "stanford.edu",
    ["engine"] = "google"
};

var result = await registry.CallFunctionAsync("WebSearch", parameters);
```

### Using Search Results with an LLM

```csharp
var searchParams = new Dictionary<string, object>
{
    ["query"] = "latest developments in quantum computing",
    ["count"] = 3
};

var searchResult = await registry.CallFunctionAsync("WebSearch", searchParams);
if (searchResult.Success)
{
    var searchResults = (IList<SearchResult>)searchResult.Data;
    
    // Format search results for the LLM
    var prompt = "Based on these search results about quantum computing, provide a summary of the latest developments:\n\n";
    
    foreach (var result in searchResults)
    {
        prompt += $"Title: {result.Title}\n";
        prompt += $"Source: {result.Source}\n";
        prompt += $"Description: {result.Snippet}\n\n";
    }
    
    // Send the prompt to the LLM
    var llmResponse = await llmClient.GetCompletionAsync(prompt);
    Console.WriteLine(llmResponse);
}
```

## API Key Management

For security, it's recommended to store API keys in a secure configuration system:

```csharp
// In appsettings.json
{
  "SearchApi": {
    "BraveApiKey": "your-api-key-here"
  }
}

// In Startup.cs or Program.cs
services.AddSingleton<IWebSearchProvider>(sp => 
    new DefaultWebSearchProvider(
        Configuration["SearchApi:BraveApiKey"],
        sp.GetRequiredService<HttpClient>()
    )
);
```

## Extending the Default Provider

You can extend the default provider to customize search behavior:

```csharp
public class EnhancedSearchProvider : DefaultWebSearchProvider
{
    public EnhancedSearchProvider(string apiKey, HttpClient httpClient = null, ILogger<DefaultWebSearchProvider> logger = null)
        : base(apiKey, httpClient, logger)
    {
    }
    
    protected override async Task<IList<SearchResult>> PerformSearchRequestAsync(
        string engine, string query, int count, IDictionary<string, object> options)
    {
        // Pre-process query (e.g., add specific keywords, remove unwanted terms)
        query = PreProcessQuery(query);
        
        // Call the base implementation
        var results = await base.PerformSearchRequestAsync(engine, query, count, options);
        
        // Post-process results (e.g., filter out unwanted domains, rerank)
        return PostProcessResults(results);
    }
    
    private string PreProcessQuery(string query)
    {
        // Add your custom query processing logic
        return query;
    }
    
    private IList<SearchResult> PostProcessResults(IList<SearchResult> results)
    {
        // Add your custom result processing logic
        return results;
    }
}
```

## Search Engine Support

The default implementation supports the following search engines:

- **Brave Search** - Default engine, offers fast and privacy-respecting results
- **Google Search** - Comprehensive results with broad coverage
- **Bing Search** - Microsoft's search engine with good integration for Microsoft-related content

Each engine requires its own API key and follows different API specifications.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.