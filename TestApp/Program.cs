using MarketAlly.AIPlugin;
using MarketAlly.AIPlugin.Plugins;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TestApp
{
	public class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				// Setup logging
				using var loggerFactory = LoggerFactory.Create(builder =>
				{
					builder
						.AddConsole()
						.SetMinimumLevel(LogLevel.Debug);
				});
				var logger = loggerFactory.CreateLogger<AIPluginRegistry>();

				// Create and setup plugin registry
				var registry = new AIPluginRegistry(logger);

				registry.RegisterPlugin(new GetDateTimePlugin());
				registry.RegisterPlugin(new GenerateRandomNumberPlugin());
				registry.RegisterPlugin(new StringManipulatorPlugin());
				registry.RegisterPlugin(new SystemInfoPlugin());

				// Main interaction loop
				while (true)
				{
					Console.WriteLine("\nAvailable plugins:");
					var plugins = registry.GetAvailableFunctions();
					foreach (var plugin in plugins)
					{
						Console.WriteLine($"\n{plugin.Value.Name} ({plugin.Key})");
						Console.WriteLine($"Description: {plugin.Value.Description}");
						if (plugin.Value.Parameters.Any())
						{
							Console.WriteLine("Parameters:");
							foreach (var param in plugin.Value.Parameters)
							{
								Console.WriteLine($"  - {param.Key}: {param.Value.Description} ({param.Value.Type.Name}){(param.Value.Required ? " [Required]" : "")}");
							}
						}
						else
						{
							Console.WriteLine("No parameters required");
						}
					}

					Console.Write("\nEnter plugin name (or 'exit' to quit): ");
					string input = Console.ReadLine()?.Trim();

					if (string.IsNullOrWhiteSpace(input))
						continue;

					if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
						break;

					// Find the matching plugin key
					var pluginKey = plugins.FirstOrDefault(p =>
						p.Key.Equals(input.ToLower()) ||
						p.Value.Name.Equals(input, StringComparison.OrdinalIgnoreCase)
					).Key;

					if (pluginKey == null)
					{
						Console.WriteLine("Unknown plugin");
						continue;
					}

					// Get parameters based on selected plugin
					var parameters = new Dictionary<string, object>();
					switch (pluginKey)
					{
						case "getdatetime":
							Console.Write("Use UTC? (true/false): ");
							parameters["useUtc"] = bool.Parse(Console.ReadLine());
							Console.Write("Format (optional, press Enter to skip): ");
							string format = Console.ReadLine();
							if (!string.IsNullOrWhiteSpace(format))
								parameters["format"] = format;
							break;

						case "generaterandomnumber":
							Console.Write("Min value: ");
							parameters["min"] = int.Parse(Console.ReadLine());
							Console.Write("Max value: ");
							parameters["max"] = int.Parse(Console.ReadLine());
							break;

						case "stringmanipulator":
							Console.Write("Input text: ");
							parameters["input"] = Console.ReadLine();
							Console.Write("Operation (reverse/uppercase/lowercase/trim): ");
							parameters["operation"] = Console.ReadLine();
							break;

						case "systeminfo":
							// No parameters needed
							break;
					}

					try
					{
						// Execute plugin
						var result = await registry.CallFunctionAsync(input, parameters);

						if (result.Success)
						{
							Console.WriteLine("\nSuccess!");
							Console.WriteLine($"Message: {result.Message}");
							Console.WriteLine($"Data: {JsonSerializer.Serialize(result.Data, new JsonSerializerOptions { WriteIndented = true })}");
						}
						else
						{
							Console.WriteLine("\nError!");
							Console.WriteLine($"Message: {result.Message}");
							Console.WriteLine($"Error: {result.Error?.Message}");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"\nError executing plugin: {ex.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Fatal error: {ex}");
			}
		}
	}
}
