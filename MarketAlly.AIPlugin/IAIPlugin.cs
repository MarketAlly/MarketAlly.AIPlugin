namespace MarketAlly.AIPlugin
{
	public interface IAIPlugin
	{
		IReadOnlyDictionary<string, Type> SupportedParameters { get; }

		// More specific return type than just string
		Task<AIPluginResult> ExecuteAsync(IReadOnlyDictionary<string, object> parameters);
	}

	public class AIPluginResult
	{
		public bool Success { get; }
		public string Message { get; }
		public object Data { get; }
		public Exception Error { get; }

		// Constructor for success
		public AIPluginResult(object data, string message = null)
		{
			Success = true;
			Data = data;
			Message = message;
		}

		// Constructor for failure
		public AIPluginResult(Exception error, string message)
		{
			Success = false;
			Error = error;
			Message = message;
		}
	}

}
