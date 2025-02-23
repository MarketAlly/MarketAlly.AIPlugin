namespace MarketAlly.AIPlugin
{
	public interface IAIPlugin
	{
		IReadOnlyDictionary<string, Type> SupportedParameters { get; }

		Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters);
	}

	public class AIPluginResult
	{
		public bool Success { get; }
		public string Message { get; }
		public object Data { get; }
		public Exception Error { get; }

		public AIPluginResult(object data, string message = null)
		{
			Success = true;
			Data = data;
			Message = message;
		}

		public AIPluginResult(Exception error, string message)
		{
			Success = false;
			Error = error;
			Message = message;
		}
	}

}
