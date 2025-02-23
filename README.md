# MarketAlly.AIPlugin

A flexible .NET plugin framework for AI model function calling and tool integration. This framework provides a unified way to create and manage plugins that can be used with various AI models including OpenAI, Claude, Qwen, Mistral, and Gemini.

## Features

- 🔌 Declarative plugin registration using attributes
- 📊 Automatic schema generation for different AI model formats
- ✅ Type-safe parameter validation
- 📝 Built-in plugin registry and execution system
- 🔄 Extensible plugin architecture with async support
- 🛡️ Comprehensive error handling and logging

## Installation

Install the package via NuGet:

```bash
dotnet add package MarketAlly.AIPlugin
```

## Quick Start

### 1. Create a Plugin

Create a new plugin by implementing the `IAIPlugin` interface and decorating it with the appropriate attributes:

```csharp
[AIPlugin("Calculator", "Performs basic arithmetic operations")]
public class CalculatorPlugin : IAIPlugin
{
    [AIParameter("First number", required: true)]
    public double Number1 { get; set; }

    [AIParameter("Second number", required: true)]
    public double Number2 { get; set; }

    [AIParameter("Operation to perform (add, subtract, multiply, divide)", required: true)]
    public string Operation { get; set; }

    public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
    {
        ["number1"] = typeof(double),
        ["number2"] = typeof(double),
        ["operation"] = typeof(string)
    };

    public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
    {
        double n1 = Convert.ToDouble(parameters["number1"]);
        double n2 = Convert.ToDouble(parameters["number2"]);
        string op = parameters["operation"].ToString().ToLower();

        double result = op switch
        {
            "add" => n1 + n2,
            "subtract" => n1 - n2,
            "multiply" => n1 * n2,
            "divide" => n2 != 0 ? n1 / n2 : throw new DivideByZeroException(),
            _ => throw new ArgumentException($"Unknown operation: {op}")
        };

        return new AIPluginResult(result);
    }
}
```

### 2. Register Plugins

Register your plugins with the plugin registry:

```csharp
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<AIPluginRegistry>();
var registry = new AIPluginRegistry(logger);

// Register plugins
registry.RegisterPlugin(new CalculatorPlugin());
registry.RegisterPlugin(new StringManipulatorPlugin());
```

### 3. Generate Schema

Generate the appropriate schema for your AI model:

```csharp
// Generate schema for OpenAI
string openAiSchema = AIPluginHelper.GenerateSchema(AIPluginHelper.AIModel.OpenAI);

// Generate schema for Claude
string claudeSchema = AIPluginHelper.GenerateSchema(AIPluginHelper.AIModel.Claude);
```

### 4. Execute Plugins

Execute plugin functions:

```csharp
var parameters = new Dictionary<string, object>
{
    ["number1"] = 10,
    ["number2"] = 5,
    ["operation"] = "multiply"
};

var result = await registry.CallFunctionAsync("Calculator", parameters);
if (result.Success)
{
    Console.WriteLine($"Result: {result.Data}");
}
else
{
    Console.WriteLine($"Error: {result.Message}");
}
```

## Built-in Plugins

The framework comes with several built-in plugins:

- **StringManipulator**: Performs string operations (reverse, uppercase, lowercase, trim)
- **SystemInfo**: Retrieves current system information
- **UrlValidator**: Validates and parses URLs
- **FileInfo**: Gets metadata about files
- **GenerateRandomNumber**: Generates random numbers within a specified range
- **GetDateTime**: Retrieves formatted date and time

## Plugin Development Guide

### Creating a New Plugin

1. Create a class that implements `IAIPlugin`
2. Decorate the class with `[AIPlugin]` attribute
3. Add properties with `[AIParameter]` attributes
4. Implement the `SupportedParameters` property
5. Implement the `ExecuteAsync` method

### Plugin Attributes

- `AIPluginAttribute`: Defines plugin name and description
- `AIParameterAttribute`: Defines parameter properties and requirements

### Parameter Validation

The framework automatically validates:
- Required parameters
- Parameter types
- Parameter presence

### Error Handling

Use `AIPluginResult` to return results:

```csharp
// Success case
return new AIPluginResult(data, "Operation successful");

// Error case
return new AIPluginResult(exception, "Operation failed");
```

## AI Model Support

The framework supports schema generation for:
- OpenAI (functions format)
- Claude (tools format)
- Qwen (apis format)
- Mistral (tools format)
- Gemini (functions format)

## Dependency Injection

The framework supports dependency injection through the `AIPluginRegistry`:

```csharp
services.AddSingleton<AIPluginRegistry>();
services.AddTransient<CalculatorPlugin>();
```

## Logging

The framework uses `Microsoft.Extensions.Logging` for comprehensive logging:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For support, please open an issue in the GitHub repository.