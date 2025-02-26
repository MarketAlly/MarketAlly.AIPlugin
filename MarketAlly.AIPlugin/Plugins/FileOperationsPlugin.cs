using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("FileOperations", "Create, update, or delete files in the file system")]
	public class FileOperationsPlugin : IAIPlugin
	{
		[AIParameter("The operation to perform (create, update, delete)", required: true)]
		public string Operation { get; set; }

		[AIParameter("Full path to the file", required: true)]
		public string FilePath { get; set; }

		[AIParameter("Content to write to the file (for create/update operations)", required: false)]
		public string Content { get; set; }

		[AIParameter("Whether to create the directory structure if it doesn't exist (for create operations)", required: false)]
		public bool CreateDirectories { get; set; } = true;

		[AIParameter("Whether to overwrite existing files (for create operations)", required: false)]
		public bool Overwrite { get; set; } = false;

		[AIParameter("Whether to make a backup before updating or deleting", required: false)]
		public bool MakeBackup { get; set; } = true;

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["operation"] = typeof(string),
			["filePath"] = typeof(string),
			["content"] = typeof(string),
			["createDirectories"] = typeof(bool),
			["overwrite"] = typeof(bool),
			["makeBackup"] = typeof(bool)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				// Extract parameters
				string operation = parameters["operation"].ToString().ToLowerInvariant();
				string filePath = parameters["filePath"].ToString();

				bool createDirectories = parameters.TryGetValue("createDirectories", out var createDirsObj)
					? Convert.ToBoolean(createDirsObj)
					: true;

				bool overwrite = parameters.TryGetValue("overwrite", out var overwriteObj)
					? Convert.ToBoolean(overwriteObj)
					: false;

				bool makeBackup = parameters.TryGetValue("makeBackup", out var backupObj)
					? Convert.ToBoolean(backupObj)
					: true;

				// Validate operation
				if (operation != "create" && operation != "update" && operation != "delete")
				{
					return new AIPluginResult(
						new ArgumentException($"Invalid operation: {operation}. Must be 'create', 'update', or 'delete'."),
						"Invalid operation"
					);
				}

				// Make backup if needed
				if (File.Exists(filePath) && makeBackup && (operation == "update" || operation == "delete"))
				{
					string backupPath = $"{filePath}.{DateTime.Now:yyyyMMdd_HHmmss}.bak";
					File.Copy(filePath, backupPath);
				}

				switch (operation)
				{
					case "create":
						return await CreateFile(filePath, parameters, createDirectories, overwrite);

					case "update":
						return await UpdateFile(filePath, parameters);

					case "delete":
						return DeleteFile(filePath);

					default:
						return new AIPluginResult(
							new ArgumentException($"Unhandled operation: {operation}"),
							"Unhandled operation"
						);
				}
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, $"Failed to perform file operation: {ex.Message}");
			}
		}

		private async Task<AIPluginResult> CreateFile(string filePath, IReadOnlyDictionary<string, object> parameters, bool createDirectories, bool overwrite)
		{
			// Check if content is provided
			if (!parameters.TryGetValue("content", out var contentObj) || contentObj == null)
			{
				return new AIPluginResult(
					new ArgumentException("Content is required for 'create' operation"),
					"Missing content parameter"
				);
			}

			string content = contentObj.ToString();

			// Check if file already exists
			if (File.Exists(filePath) && !overwrite)
			{
				return new AIPluginResult(
					new IOException($"File already exists: {filePath}. Set 'overwrite' to true to replace it."),
					"File already exists"
				);
			}

			// Create directory structure if needed
			string directory = Path.GetDirectoryName(filePath);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				if (createDirectories)
				{
					Directory.CreateDirectory(directory);
				}
				else
				{
					return new AIPluginResult(
						new DirectoryNotFoundException($"Directory does not exist: {directory}. Set 'createDirectories' to true to create it."),
						"Directory not found"
					);
				}
			}

			// Write the content to the file
			await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

			return new AIPluginResult(new
			{
				Operation = "create",
				FilePath = filePath,
				FileSize = new FileInfo(filePath).Length,
				Success = true,
				Message = $"File created successfully: {filePath}"
			});
		}

		private async Task<AIPluginResult> UpdateFile(string filePath, IReadOnlyDictionary<string, object> parameters)
		{
			// Check if file exists
			if (!File.Exists(filePath))
			{
				return new AIPluginResult(
					new FileNotFoundException($"File not found: {filePath}"),
					"File not found"
				);
			}

			// Check if content is provided
			if (!parameters.TryGetValue("content", out var contentObj) || contentObj == null)
			{
				return new AIPluginResult(
					new ArgumentException("Content is required for 'update' operation"),
					"Missing content parameter"
				);
			}

			string content = contentObj.ToString();

			// Update the file
			await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

			return new AIPluginResult(new
			{
				Operation = "update",
				FilePath = filePath,
				FileSize = new FileInfo(filePath).Length,
				Success = true,
				Message = $"File updated successfully: {filePath}"
			});
		}

		private AIPluginResult DeleteFile(string filePath)
		{
			// Check if file exists
			if (!File.Exists(filePath))
			{
				return new AIPluginResult(
					new FileNotFoundException($"File not found: {filePath}"),
					"File not found"
				);
			}

			// Delete the file
			File.Delete(filePath);

			return new AIPluginResult(new
			{
				Operation = "delete",
				FilePath = filePath,
				Success = true,
				Message = $"File deleted successfully: {filePath}"
			});
		}
	}
}