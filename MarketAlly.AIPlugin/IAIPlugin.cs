namespace MarketAlly.AIPlugin
{
	public interface IAIPlugin
	{
		Task<string> ExecuteAsync(Dictionary<string, string> parameters);
	}

}
