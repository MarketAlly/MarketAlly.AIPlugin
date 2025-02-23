using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketAlly.AIPlugin.Plugins
{
	[AIPlugin("FileInfo", "Gets metadata about a file")]
	public class FileInfoPlugin : IAIPlugin
	{
		[AIParameter("Full path to the file", required: true)]
		public string FilePath { get; set; }

		public IReadOnlyDictionary<string, Type> SupportedParameters => new Dictionary<string, Type>
		{
			["filePath"] = typeof(string)
		};

		public async Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters)
		{
			try
			{
				string path = parameters["filePath"].ToString();
				var fileInfo = new FileInfo(path);

				var result = new
				{
					Exists = fileInfo.Exists,
					Size = fileInfo.Length,
					CreationTime = fileInfo.CreationTime,
					LastModified = fileInfo.LastWriteTime,
					Extension = fileInfo.Extension
				};

				return new AIPluginResult(result);
			}
			catch (Exception ex)
			{
				return new AIPluginResult(ex, "Failed to get file information");
			}
		}
	}
}
